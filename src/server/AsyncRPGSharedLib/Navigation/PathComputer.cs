using System;
using System.Collections.Generic;
using AsyncRPGSharedLib.Environment;
using System.Diagnostics;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Navigation
{
    public class PathComputer
    {
        private const uint NON_BLOCKING_MAX_NODES_SEARCHED_PER_UPDATE = 16;
        private const uint MAX_TOTAL_NODES_SEARCHED_ALLOWED = 1024; // Should always be larger that the max possible cells in a nav mesh

        public delegate void OnPathComputedCallback(PathComputer.eResult result);
        public delegate void OnPathComputerComplete(PathComputer computer);

        public enum eState
        {
            invalid,
            compute_end_points,
            setup_raw_path,
            compute_raw_path,
            finalize_raw_path,
            compute_smoothed_path,
            complete
        };

        public enum eResult
        {
            success,
            failed_start_off_nav_mesh,
            failed_end_off_nav_mesh,
            failed_endpoints_unconnected,
            failed_raw_path_search
        };

        // Request State		
        private NavMesh m_navMesh;
        private RoomKey m_roomKey;
        private Point3d m_startPosition;
        private Point3d m_endPosition;
        private NavRef m_startNavRef;
        private NavRef m_endNavRef;

        // Query State
        private OnPathComputerComplete m_completeCallback;
        private eState m_state;
        private uint m_nextPathNodeID;
        private uint m_maxNodesSearchedPerUpdate;
        private uint m_totalNodesSearched;
        private Dictionary<uint, PathNode> m_navCellToNodeMap;
        private PathNodeHeap m_nodeHeap;

        // Result State
        private eResult m_resultCode;
        private List<NavRef> m_rawPath;
        private List<PathStep> m_finalPath;

        public PathComputer()
        {
            Reset();
        }

        public void Reset()
        {
            m_state = eState.invalid;

            ResetRequest();
            ResetResult();
        }

        // Blocking Query Functions
        public bool BlockingPathRequest(
            NavMesh navMesh, 
            RoomKey roomKey, 
            Point3d startPosition, 
            Point3d endPosition)
        {
            ResetRequest();
            ResetResult();

            m_navMesh = navMesh;
            m_roomKey = roomKey;
            m_startPosition = startPosition;
            m_endPosition = endPosition;

            m_state = eState.compute_end_points;
            m_maxNodesSearchedPerUpdate = 0; // Allowed to search as many nodes as we want

            while (m_state != eState.complete)
            {
                ComputeState();
            }

            return m_resultCode == eResult.success;
        }

        // Non-Blocking Query Functions
        public bool NonBlockingPathRequest(
            NavMesh navMesh,
            RoomKey roomKey,
            Point3d startPosition,
            Point3d endPosition,
            OnPathComputerComplete onComplete)
        {
            bool success = false;

            if (m_state == eState.invalid || m_state == eState.complete)
            {
                ResetRequest();
                ResetResult();

                m_navMesh = navMesh;
                m_roomKey = roomKey;
                m_startPosition = startPosition;
                m_endPosition = endPosition;

                m_state = eState.compute_end_points;
                m_maxNodesSearchedPerUpdate = NON_BLOCKING_MAX_NODES_SEARCHED_PER_UPDATE;
                m_completeCallback = onComplete;

                success = true;
            }

            return success;
        }

        public void UpdateNonBlockingPathRequest(
            uint maxWorkMiliseconds)
        {
            if (m_state != eState.invalid)
            {
                Stopwatch stopWatch = new Stopwatch();

                stopWatch.Start();
                while (m_state != eState.complete && stopWatch.ElapsedMilliseconds < maxWorkMiliseconds)
                {
                    ComputeState();

                    if (m_state == eState.complete)
                    {
                        m_completeCallback(this);
                    }
                }
            }
        }

        // Result Accessors
        public eState State
        {
            get { return m_state; }
        }

        public eResult ResultCode
        {
            get { return m_resultCode; }
        }

        public List<PathStep> FinalPath
        {
            get { return m_finalPath; }
        }

        public float DestinationFacingAngle
        {
            get
            {
                float angle = MathConstants.GetAngleForDirection(MathConstants.eDirection.down);
                Vector2d lastStepDirection = new Vector2d();

                if (m_finalPath.Count > 1)
                {
                    Point2d lastStepPoint =
                        GameConstants.ConvertRoomPositionToPixelPosition(m_finalPath[m_finalPath.Count - 1].StepPoint);
                    Point2d previousStepPoint =
                        GameConstants.ConvertRoomPositionToPixelPosition(m_finalPath[m_finalPath.Count - 2].StepPoint);

                    lastStepDirection = lastStepPoint - previousStepPoint;
                }
                else
                {
                    Point2d endPixelPoint = GameConstants.ConvertRoomPositionToPixelPosition(m_endPosition);
                    Point2d startPixelPoint = GameConstants.ConvertRoomPositionToPixelPosition(m_startPosition);

                    lastStepDirection = endPixelPoint - startPixelPoint;
                }

                angle = MathConstants.GetAngleForVector(lastStepDirection);

                return angle;
            }
        }

        // Internal Path Computation Stages	
        private void ComputeState()
        {
            switch (m_state)
            {
                case eState.compute_end_points:
                    ComputeEndpoints();
                    break;
                case eState.setup_raw_path:
                    SetupRawPath();
                    break;
                case eState.compute_raw_path:
                    ComputeRawPath();
                    break;
                case eState.finalize_raw_path:
                    FinalizeRawPath();
                    break;
                case eState.compute_smoothed_path:
                    ComputeSmoothedPath();
                    break;
            }
        }

        private void ComputeEndpoints()
        {
            m_startNavRef = m_navMesh.ComputeNavRefAtPoint(m_startPosition);
            m_endNavRef = m_navMesh.ComputeNavRefAtPoint(m_endPosition);

            if (m_startNavRef.IsValid)
            {
                if (m_endNavRef.IsValid)
                {
                    if (m_navMesh.AreNavRefsConnected(m_startNavRef, m_endNavRef))
                    {
                        if (m_startNavRef.Equals(m_endNavRef))
                        {
                            // We can head directly to the destination
                            m_finalPath.Add(new PathStep(m_startNavRef, m_startPosition));
                            m_finalPath.Add(new PathStep(m_endNavRef, m_endPosition));
                            m_state = eState.complete;
                        }
                        else
                        {
                            // We have to do the expensive query
                            m_state = eState.setup_raw_path;
                        }
                    }
                    else
                    {
                        m_state = eState.complete;
                        m_resultCode = eResult.failed_start_off_nav_mesh;
                    }
                }
                else
                {
                    m_state = eState.complete;
                    m_resultCode = eResult.failed_end_off_nav_mesh;
                }
            }
            else
            {
                m_state = eState.complete;
                m_resultCode = eResult.failed_start_off_nav_mesh;
            }
        }

        private void SetupRawPath()
        {
            m_nextPathNodeID = 0;
            m_navCellToNodeMap = new Dictionary<uint, PathNode>();
            m_nodeHeap = new PathNodeHeap();

            PathNode pathNode =
                new PathNode(
                    (int)m_nextPathNodeID,
                    null,
                    (uint)m_startNavRef.NavCellIndex,
                    0,
                    ComputeHeuristicCostEstimate(m_startPosition, m_endPosition));

            m_nextPathNodeID++;
            m_totalNodesSearched = 0;

            m_nodeHeap.Push(pathNode);
            m_navCellToNodeMap[pathNode.NavCellIndex] = pathNode;

            m_state = eState.compute_raw_path;
        }

        private void ComputeRawPath()
        {
            int nodesSearchedThisUpdate = 0;

            // Keep searching until we either find the end
            // or we're not allowed to search anymore
            while (!m_nodeHeap.Empty() &&
                    m_totalNodesSearched < MAX_TOTAL_NODES_SEARCHED_ALLOWED &&
                    (m_maxNodesSearchedPerUpdate == 0 || (m_maxNodesSearchedPerUpdate > 0 && nodesSearchedThisUpdate < m_maxNodesSearchedPerUpdate)))
            {
                PathNode currentNode = m_nodeHeap.Top();

                // Keep track of how many nodes total we have visited so far
                m_totalNodesSearched++;

                // Keep track of how many nodes were searched this update
                nodesSearchedThisUpdate++;

                // Make sure we haven't hit the destination
                if (currentNode.NavCellIndex != m_endNavRef.NavCellIndex)
                {
                    // Remove the current node from the heap now that we're investigating it
                    m_nodeHeap.Pop();

                    // Add the current nav-cell to the closed set 
                    // (since we just visited it and don't want to visit it again)
                    currentNode.MarkAsInClosedSet();

                    // See which neighbor, if any, looks best at this moment to search from
                    for (NavCellNeighborIterator iterator = new NavCellNeighborIterator(m_navMesh, currentNode.NavCellIndex);
                        iterator.Valid(); iterator.Next())
                    {
                        // See if we already have a search node for the neighbor nav-cell
                        uint neighborNavCellIndex = iterator.NeighborNavCellIndex;
                        PathNode neighborNode = null;
                        bool hasNeighbor = m_navCellToNodeMap.TryGetValue(neighborNavCellIndex, out neighborNode);

                        // Skip this neighbor if we already put it in the closed set
                        if (!hasNeighbor || !neighborNode.InClosedSet)
                        {
                            // Compute G(x): The path cost from the start to this neighbor
                            float netTraversalCostToNeighbor = currentNode.Cost +
                                ComputeTraversalCost(currentNode.NavCellIndex, neighborNavCellIndex);

                            // Add this neighbor into consideration if:
                            // A) We haven't seen it before (i.e. not in the open set)
                            // B) We have seen it before, but the net traversal cost to the neighbor
                            //    from the start through the current node is cheaper than some previous 
                            //    traversal to the neighbor.
                            if (neighborNode == null || netTraversalCostToNeighbor < neighborNode.Cost)
                            {
                                // Compute F(x)= G(x) + H(x): The past cost to the neighbor plus the estimated distance to the end
                                Point3d neighborCenter = m_navMesh.ComputeNavCellCenter(neighborNavCellIndex);
                                float netTraversalPlusHeuristicCost =
                                    netTraversalCostToNeighbor +
                                    ComputeHeuristicCostEstimate(neighborCenter, m_endPosition);

                                // Make sure to add the neighbor to the open set if not already added
                                if (neighborNode == null)
                                {
                                    neighborNode =
                                        new PathNode(
                                            (int)m_nextPathNodeID,
                                            currentNode,
                                            neighborNavCellIndex,
                                            netTraversalCostToNeighbor,
                                            netTraversalPlusHeuristicCost);
                                    m_nextPathNodeID++;

                                    m_navCellToNodeMap[neighborNavCellIndex] = neighborNode;
                                }
                                else
                                {
                                    neighborNode.ParentNode = currentNode;
                                    neighborNode.Cost = netTraversalCostToNeighbor;
                                    neighborNode.Total = netTraversalPlusHeuristicCost;
                                }

                                // Add the neighbor into the heap for future consideration
                                m_nodeHeap.Push(neighborNode);
                            }
                        }
                    }
                }
                else
                {
                    // Stop searching and compute the raw path from the search nodes
                    m_state = eState.finalize_raw_path;
                    break;
                }
            }

            // Sanity check to make sure we never get in an infinite loop
            if (m_nodeHeap.Empty())
            {
                m_resultCode = eResult.failed_raw_path_search;
                m_state = eState.complete;
            }
            else if (m_totalNodesSearched >= MAX_TOTAL_NODES_SEARCHED_ALLOWED)
            {
                //Debug.log("PathComputer: Ran out of search node!", Debug.RED);
                m_resultCode = eResult.failed_raw_path_search;
                m_state = eState.complete;
            }
        }

        private void FinalizeRawPath()
        {
            PathNode currentNode = m_nodeHeap.Top();

            // If the current best node in the heap isn't the end, 
            // then we couldn't find a path to end
            if (currentNode.NavCellIndex == m_endNavRef.NavCellIndex)
            {
                uint iterations_left = MAX_TOTAL_NODES_SEARCHED_ALLOWED;

                // Walk from the end to the start, then reverse the path
                while (currentNode != null && iterations_left > 0)
                {
                    m_rawPath.Add(new NavRef((int)currentNode.NavCellIndex, m_roomKey));
                    currentNode = currentNode.ParentNode;
                    iterations_left--;
                }

                if (iterations_left > 0)
                {
                    m_rawPath.Reverse();
                    m_state = eState.compute_smoothed_path;
                }
                else
                {
                    // Sanity check to make sure we don't hit any infinite loops
                    // TODO: Probably should log this case
                    m_resultCode = eResult.failed_raw_path_search;
                    m_state = eState.complete;
                }
            }
            else
            {
                // This case should technically be cause by the connectivity ID case
                // TODO: Probably should log this case
                m_resultCode = eResult.failed_raw_path_search;
                m_state = eState.complete;
            }
        }

        private void ComputeSmoothedPath()
        {
            // Convert the raw path steps into a proper path step sequence

            // Always add the first step
            m_finalPath.Add(new PathStep(m_startNavRef, m_startPosition));

            // Add a step at the midpoint between each neighboring nav-cell in the path
            for (uint rawStepIndex = 1; rawStepIndex < m_rawPath.Count; rawStepIndex++)
            {
                NavRef previousNavRef = m_rawPath[(int)rawStepIndex - 1];
                NavRef currentNavRef = m_rawPath[(int)rawStepIndex];
                Point3d portalLeft, portalRight;

                if (m_navMesh.ComputePortalPoints(
                        previousNavRef,
                        currentNavRef,
                        out portalLeft,
                        out portalRight))
                {
                    Point3d portalMidpoint = Point3d.Interpolate(portalLeft, portalRight, 0.5F);

                    m_finalPath.Add(new PathStep(currentNavRef, portalMidpoint));
                }
            }

            // Always add the lest step
            m_finalPath.Add(new PathStep(m_endNavRef, m_endPosition));

            // TODO: Remove the extraneous path steps using funnel algorithm + ray casting			
            m_state = eState.complete;
        }

        // Helper Functions
        private void ResetRequest()
        {
            m_navMesh = null;
            m_roomKey = null;
            m_startPosition = new Point3d();
            m_endPosition = new Point3d();
            m_startNavRef = null;
            m_endNavRef = null;
        }

        private void ResetResult()
        {
            m_resultCode = eResult.success;
            m_rawPath = new List<NavRef>();
            m_finalPath = new List<PathStep>();
        }

        private float ComputeHeuristicCostEstimate(Point3d start, Point3d end)
        {
            return Point3d.Distance(start, end);
        }

        private float ComputeTraversalCost(uint navCellIndex, uint neighborNavCellIndex)
        {
            Point3d center = m_navMesh.ComputeNavCellCenter(navCellIndex);
            Point3d neighborCenter = m_navMesh.ComputeNavCellCenter(neighborNavCellIndex);

            return Point3d.Distance(center, neighborCenter);
        }
    }
}
