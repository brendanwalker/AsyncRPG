using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace AsyncRPGSharedLib.Navigation
{
    public class NavMesh
    {
        private const int COMPRESSED_DATA_HEADER_BYTE_COUNT = 2;

        public const int EMPTY_NAV_CELL = -1;
        public const int EMPTY_PVS_CELL = -1;

        private class NavCell
        {
            public short connectivityId;
            public short pvsCellIndex;

            public NavCell()
            {
                this.connectivityId = (short)EMPTY_NAV_CELL;
                this.pvsCellIndex = (short)EMPTY_PVS_CELL;
            }

            public NavCell(uint connectivityId, uint pvsCellIndex)
            {
                this.connectivityId = (short)connectivityId;
                this.pvsCellIndex = (short)pvsCellIndex;
            }

            public bool Equals(NavCell other)
            {
                return connectivityId == other.connectivityId && pvsCellIndex == other.pvsCellIndex;
            }
        }

        private RoomKey m_roomKey;
        private AABB3d m_boundingBox;
        private uint m_rowCount;
        private uint m_colomnCount;
        private uint m_nonEmptyNavCellCount;
        private NavCell[] m_navCells; // Regular grid of connectivity ids
        private PotentiallyVisibleSet m_pvs; // visibility from one cell to another

        public bool IsNavCellDataEmpty
        {
            get { return m_navCells == null; }
        }

        public PotentiallyVisibleSet PVS
        {
            get { return m_pvs; }
        }

        public NavMesh()
        {
            m_roomKey = null;
            m_boundingBox = new AABB3d();
            m_rowCount = 0;
            m_colomnCount = 0;
            m_nonEmptyNavCellCount = 0;
            m_navCells = null;
            m_pvs = null;
        }

        public NavMesh(RoomKey roomKey, NavMesh navMeshTemplate)
        {
            m_roomKey = new RoomKey(roomKey);
            m_boundingBox = new AABB3d(navMeshTemplate.m_boundingBox);

            m_rowCount = navMeshTemplate.m_rowCount;
            m_colomnCount = navMeshTemplate.m_colomnCount;
            m_nonEmptyNavCellCount = navMeshTemplate.m_nonEmptyNavCellCount;

            m_navCells = new NavCell[navMeshTemplate.m_navCells.Length];
            Array.Copy(navMeshTemplate.m_navCells, m_navCells, m_navCells.Length);

            m_pvs = new PotentiallyVisibleSet(navMeshTemplate.m_pvs);
        }

        public bool Equals(NavMesh other)
        {
            if ((m_roomKey == null && other.m_roomKey != null) ||
                (m_roomKey != null && other.m_roomKey == null) ||
                (m_roomKey != null && other.m_roomKey != null && m_roomKey.Equals(other.m_roomKey)))
            {
                return false;
            }

            if (!m_boundingBox.Equals(other.m_boundingBox))
            {
                return false;
            }

            if (m_rowCount != other.m_rowCount)
            {
                return false;
            }

            if (m_colomnCount != other.m_colomnCount)
            {
                return false;
            }

            if (m_nonEmptyNavCellCount != other.m_nonEmptyNavCellCount)
            {
                return false;
            }

            if ((m_navCells == null && other.m_navCells != null) ||
                (m_navCells != null && other.m_navCells == null))
            {
                return false;
            }
            else if (m_navCells != null && other.m_navCells != null)
            {
                if (m_navCells.Length != other.m_navCells.Length)
                {
                    return false;
                }
                else
                {
                    for (int navCellIndex = 0; navCellIndex < m_navCells.Length; navCellIndex++)
                    {
                        if (!m_navCells[navCellIndex].Equals(other.m_navCells[navCellIndex]))
                        {
                            return false;
                        }
                    }
                }
            }

            if ((m_pvs == null && other.m_pvs != null) ||
                (m_pvs != null && other.m_pvs == null))
            {
                return false;
            }
            else if (m_pvs != null && other.m_pvs != null && !m_pvs.Equals(other.m_pvs))
            {
                return false;
            }

            return true;
        }

        public static NavMesh FromNavMeshXML(XmlNode navMeshXml)
        {
            NavMesh navMesh = new NavMesh();

            navMesh.NavCellsFromXML(navMeshXml);

            if (navMesh.m_navCells != null && navMesh.m_navCells.Length > 0)
            {
                navMesh.BuildNavCellPotentiallyVisibleSet();
            }

            return navMesh;
        }

        public static NavMesh FromCompressedNavMeshData(byte[] compressedNavCells, byte[] compressedPVS)
        {
            NavMesh navMesh = new NavMesh();

            if (compressedNavCells != null && compressedNavCells.Length > 0)
            {
                navMesh.NavCellsFromCompressedRawByteArray(compressedNavCells);
            }

            if (compressedPVS != null && compressedPVS.Length > 0)
            {
                navMesh.m_pvs = PotentiallyVisibleSet.FromCompressedRawByteArray(navMesh.m_nonEmptyNavCellCount, compressedPVS);
            }

            return navMesh;
        }

        public void ToCompressedData(out byte[] compressedNavCells, out byte[] compressedPVS)
        {
            compressedNavCells = NavCellsToCompressedRawByteArray();
            compressedPVS = (m_pvs != null) ? m_pvs.ToCompressedRawByteArray() : null;
        }

        public void ToStringData(StringBuilder report)
        {
            NavCellsToString(report);

            if (m_pvs != null)
            {
                m_pvs.ToString(report);
            }
        }

        private void NavCellsFromXML(XmlNode navMeshXml)
        {
            string[] navMeshStringRows = navMeshXml.InnerText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            uint rowCount = (uint)navMeshStringRows.Length;
            uint colomnCount = (uint)navMeshStringRows[0].Length;

            // Compute derived room properties
            float roomWidth = (float)colomnCount * GameConstants.NAV_MESH_WORLD_UNITS_SIZE;
            float roomHeight = (float)rowCount * GameConstants.NAV_MESH_WORLD_UNITS_SIZE;
            BitArray navCellData = new BitArray((int)(rowCount * colomnCount));
            int navMeshDataIndex = 0;

            foreach (string rowString in navMeshStringRows)
            {
                foreach (char c in rowString)
                {
                    navCellData[navMeshDataIndex] = (c == '1');
                    ++navMeshDataIndex;
                }
            }

            m_colomnCount = colomnCount;
            m_rowCount = rowCount;
            m_nonEmptyNavCellCount = 0;
            m_roomKey = null;
            m_boundingBox = new AABB3d();
            m_boundingBox.SetBounds2d(-roomWidth / 2.0f, -roomHeight / 2.0f, roomWidth / 2.0f, roomHeight / 2.0f);

            NavMesh.BuildNavCellConnectivityGrid(
                colomnCount,
                rowCount,
                navCellData,
                out m_navCells,
                out m_nonEmptyNavCellCount);
        }

        private void NavCellsFromCompressedRawByteArray(
            byte[] compressedNavCells)
        {
            byte[] header = new byte[COMPRESSED_DATA_HEADER_BYTE_COUNT];
            byte[] uncompressed =
                CompressionUtilities.RunLengthDecodeByteArray(
                    compressedNavCells,
                    CompressionUtilities.eEncodeType.Zeros,
                    header);
            BitArray navCellData = new BitArray(uncompressed);

            // Read out the header
            Debug.Assert(COMPRESSED_DATA_HEADER_BYTE_COUNT == 2);
            uint colomnCount = (uint)header[0];
            uint rowCount = (uint)header[1];

            // Compute derived room properties
            int navCellCount = (int)(rowCount * colomnCount);
            float roomWidth = (float)colomnCount * GameConstants.NAV_MESH_WORLD_UNITS_SIZE;
            float roomHeight = (float)rowCount * GameConstants.NAV_MESH_WORLD_UNITS_SIZE;

            // Initialize the nav cell derived data
            m_colomnCount = colomnCount;
            m_rowCount = rowCount;
            m_nonEmptyNavCellCount = 0;
            m_roomKey = null;
            m_boundingBox = new AABB3d();
            m_boundingBox.SetBounds2d(-roomWidth / 2.0f, -roomHeight / 2.0f, roomWidth / 2.0f, roomHeight / 2.0f);

            // Truncate extra bits added due to the bit array getting rounded up to the nearest byte
            navCellData.Length = navCellCount;

            // Compute the connectivity data from the nav cell bit array            
            NavMesh.BuildNavCellConnectivityGrid(
                colomnCount,
                rowCount,
                navCellData,
                out m_navCells,
                out m_nonEmptyNavCellCount);
        }

        private byte[] NavCellsToCompressedRawByteArray()
        {
            byte[] compressed = null;

            if (m_navCells != null)
            {
                BitArray validNavCellFlags = new BitArray(m_navCells.Length);
                int navCellBytesNeeded = (m_navCells.Length + 7) / 8;
                byte[] uncompressedNavCells = new byte[navCellBytesNeeded];

                // Store the header first in the buffer
                Debug.Assert(m_colomnCount < 256);
                Debug.Assert(m_rowCount < 256);
                Debug.Assert(COMPRESSED_DATA_HEADER_BYTE_COUNT == 2);
                byte[] header = new byte[COMPRESSED_DATA_HEADER_BYTE_COUNT];
                header[0] = (byte)m_colomnCount;
                header[1] = (byte)m_rowCount;

                // Store the nav cells as a simple valid/invalid bit array.
                // The connectivity info can be recomputed from this.
                for (int navCellIndex = 0; navCellIndex < m_navCells.Length; navCellIndex++)
                {
                    validNavCellFlags.Set(navCellIndex, m_navCells[navCellIndex].connectivityId != EMPTY_NAV_CELL);
                }

                validNavCellFlags.CopyTo(uncompressedNavCells, 0);

                compressed =
                    CompressionUtilities.RunLengthEncodeByteArray(
                        uncompressedNavCells,
                        CompressionUtilities.eEncodeType.Zeros,
                        header);
            }

            return compressed;
        }

        private void NavCellsToString(StringBuilder report)
        {
            if (m_navCells != null)
            {
                int cellIndex = 0;

                report.AppendLine("NavCells:");

                for (int row = 0; row < m_rowCount; row++)
                {
                    for (int col = 0; col < m_colomnCount; col++)
                    {
                        NavCell navCell = m_navCells[cellIndex++];

                        report.Append(navCell.connectivityId != EMPTY_NAV_CELL ? '1' : '0');
                    }

                    report.Append('\n');
                }
            }
        }

        private static void BuildNavCellConnectivityGrid(
            uint colomnCount,
            uint rowCount,
            BitArray navMeshData,
            out NavCell[] navCells,
            out uint nonEmptyNavCellCount)
        {
            UnionFind<uint> navCellUnion = new UnionFind<uint>();

            // Create an initial set of nav cells
            // Any valid cell gets a connectivity id of 0
            // Any invalid cell gets a connectivity id of EMPTY_NAV_CELL
            // Valid nav cells get added to the nav cell union set
            navCells = new NavCell[rowCount * colomnCount];
            nonEmptyNavCellCount = 0;

            for (int navCellIndex = 0; navCellIndex < navMeshData.Length; ++navCellIndex)
            {
                if (navMeshData[navCellIndex])
                {
                    uint pvsCellIndex = nonEmptyNavCellCount++;

                    navCells[navCellIndex] = new NavCell(0, pvsCellIndex);

                    navCellUnion.AddElement((uint)navCellIndex);
                }
                else
                {
                    navCells[navCellIndex] = new NavCell();
                }
            }

            // Union together all neighboring nav cells
            for (int unionElementIndex = 0; unionElementIndex < navCellUnion.SetSize; ++unionElementIndex)
            {
                uint navCellIndex = navCellUnion.GetElement(unionElementIndex);

                for (MathConstants.eDirection direction = MathConstants.eDirection.first;
                     direction < MathConstants.eDirection.count;
                     ++direction)
                {
                    uint neighborNavCellIndex;

                    if (TryGetValidNeighborNavCellIndex(
                            navCells,
                            colomnCount,
                            rowCount,
                            navCellIndex,
                            direction,
                            out neighborNavCellIndex))
                    {
                        navCellUnion.Union(navCellIndex, neighborNavCellIndex);
                    }
                }
            }

            // Write the final connectivity IDs back into the nav cells
            for (int unionElementIndex = 0; unionElementIndex < navCellUnion.SetSize; ++unionElementIndex)
            {
                uint navCellIndex = navCellUnion.GetElement(unionElementIndex);
                int connectivityID = navCellUnion.FindRootIndex(unionElementIndex);

                navCells[navCellIndex].connectivityId = (short)connectivityID;
            }
        }

        private void BuildNavCellPotentiallyVisibleSet()
        {
            if (m_nonEmptyNavCellCount > 0)
            {
                // Only need PVS information on non-empty nav cells
                m_pvs = new PotentiallyVisibleSet(m_nonEmptyNavCellCount);

                // Raycast from each non-empty nav cell...
                for (int startNavCellIndex = 0; startNavCellIndex < m_navCells.Length; ++startNavCellIndex)
                {
                    NavRef startNavRef = new NavRef(startNavCellIndex, m_roomKey);
                    NavCell startNavCell = m_navCells[startNavCellIndex];

                    if (startNavCell.connectivityId != EMPTY_NAV_CELL)
                    {
                        // ... to every other non-empty nav cell
                        for (int endNavCellIndex = 0; endNavCellIndex < m_navCells.Length; ++endNavCellIndex)
                        {
                            NavCell endNavCell = m_navCells[endNavCellIndex];

                            if (startNavCellIndex != endNavCellIndex &&
                                endNavCell.connectivityId != EMPTY_NAV_CELL)
                            {
                                NavRef endNavRef = new NavRef(endNavCellIndex, m_roomKey);

                                Point3d start = ComputeNavCellCenter((uint)startNavCellIndex);
                                Point3d end = ComputeNavCellCenter((uint)endNavCellIndex);

                                float t;
                                bool hit = Raycast(start, startNavRef, end, endNavRef, out t);

                                // Mark visibility in the PVS where the raycast succeeds
                                if (!hit)
                                {
                                    m_pvs.SetCellCanSeeOtherCell((uint)startNavCell.pvsCellIndex, (uint)endNavCell.pvsCellIndex);
                                }
                            }
                        }
                    }
                }
            }
        }

        public NavRef ComputeNavRefAtPoint(Point3d point)
        {
            return ComputeNavRefAtPoint(point.ToPoint2d());
        }

        public NavRef ComputeNavRefAtPoint(Point2d point)
        {
            NavRef navRef = null;

            if (m_boundingBox.ContainsPoint2d(point))
            {
                if (m_navCells.Length > 0)
                {
                    float cellSize = GameConstants.NAV_MESH_WORLD_UNITS_SIZE;
                    uint colomnIndex = (uint)((point.x - m_boundingBox.Min.x) / cellSize);
                    uint rowIndex = (uint)((m_boundingBox.Max.y - point.y) / cellSize);
                    uint cellIndex = GetNavCellIndex(rowIndex, colomnIndex);

                    if (m_navCells[cellIndex].connectivityId != EMPTY_NAV_CELL)
                    {
                        navRef = new NavRef((int)cellIndex, m_roomKey);
                    }
                    else
                    {
                        navRef = new NavRef();
                    }
                }
                else
                {
                    // The whole nav mesh is one big nav cell
                    navRef = new NavRef(0, m_roomKey);
                }
            }
            else
            {
                navRef = new NavRef();
            }

            return navRef;
        }

        public Point3d ComputeNavCellCenter(uint navCellIndex)
        {
            uint colomn = GetNavCellColomn(navCellIndex);
            uint row = GetNavCellRow(navCellIndex);
            float halfSize = GameConstants.NAV_MESH_WORLD_UNITS_SIZE * 0.5f;

            return new Point3d(
                m_boundingBox.Min.x + ((float)colomn * GameConstants.NAV_MESH_WORLD_UNITS_SIZE) + halfSize,
                m_boundingBox.Max.y - ((float)row * GameConstants.NAV_MESH_WORLD_UNITS_SIZE) - halfSize,
				0.0f);
        }

        public bool ComputePortalPoints(NavRef fromNavRef, NavRef toNavRef, out Point3d portalLeft, out Point3d portalRight)
        {
            bool validPortal = false;

            if (fromNavRef.IsValid && toNavRef.IsValid &&
                fromNavRef.NavRoomKey.Equals(toNavRef.NavRoomKey))
            {
                uint fromColomn = GetNavCellColomn((uint)fromNavRef.NavCellIndex);
                uint fromRow = GetNavCellRow((uint)fromNavRef.NavCellIndex);
                uint toColomn = GetNavCellColomn((uint)toNavRef.NavCellIndex);
                uint toRow = GetNavCellRow((uint)toNavRef.NavCellIndex);

                // toNavRef is to the right of the fromNavRef (+X)
                if (toColomn == fromColomn + 1 && toRow == fromRow)
                {
                    validPortal =
                        ComputePointsOnNavCellSide(
                            (uint)fromNavRef.NavCellIndex,
                            MathConstants.eDirection.right,
                            out portalLeft,
                            out portalRight);
                }
                // toNavRef is to the left of the fromNavRef (-X)
                else if (toColomn == fromColomn - 1 && toRow == fromRow)
                {
                    validPortal =
                        ComputePointsOnNavCellSide(
                            (uint)fromNavRef.NavCellIndex,
                            MathConstants.eDirection.left,
                            out portalLeft,
                            out portalRight);
                }
                // toNavRef is below fromNavRef (-Y)
                else if (toRow == fromRow + 1 && toColomn == fromColomn)
                {
                    validPortal =
                        ComputePointsOnNavCellSide(
                            (uint)fromNavRef.NavCellIndex,
                            MathConstants.eDirection.down,
                            out portalLeft,
                            out portalRight);
                }
                // toNavRef is above fromNavRef (+Y)
                else if (toRow == fromRow - 1 && toColomn == fromColomn)
                {
                    validPortal =
                        ComputePointsOnNavCellSide(
                            (uint)fromNavRef.NavCellIndex,
                            MathConstants.eDirection.up,
                            out portalLeft,
                            out portalRight);
                }
                else
                {
                    portalLeft = new Point3d();
                    portalRight = new Point3d();
                }
            }
            else
            {
                portalLeft = new Point3d();
                portalRight = new Point3d();
            }

            return validPortal;
        }

        public bool ComputePointsOnNavCellSide(
            uint navCellIndex,
            MathConstants.eDirection direction,
            out Point3d portalLeft,
            out Point3d portalRight)
        {
            bool validPortal = false;

            if (m_navCells.Length > 1)
            {
                float halfCellSize = GameConstants.NAV_MESH_WORLD_UNITS_SIZE * 0.5F;

                portalLeft = ComputeNavCellCenter(navCellIndex);
                portalRight = new Point3d(portalLeft);

                // toNavRef is to the right of the fromNavRef (+X)
                switch (direction)
                {
                    // (+X)
                    case MathConstants.eDirection.right:
                        {
                            portalLeft.x += halfCellSize;
                            portalLeft.y -= halfCellSize;
                            portalRight.x += halfCellSize;
                            portalRight.y += halfCellSize;

                            validPortal = true;
                        } break;
                    // (-X)
                    case MathConstants.eDirection.left:
                        {
                            portalLeft.x -= halfCellSize;
                            portalLeft.y += halfCellSize;
                            portalRight.x -= halfCellSize;
                            portalRight.y -= halfCellSize;

                            validPortal = true;
                        } break;
                    // (-Y)
                    case MathConstants.eDirection.down:
                        {
                            portalLeft.x += halfCellSize;
                            portalLeft.y -= halfCellSize;
                            portalRight.x -= halfCellSize;
                            portalRight.y -= halfCellSize;

                            validPortal = true;
                        } break;
                    // (+Y)
                    case MathConstants.eDirection.up:
                        {
                            portalLeft.x -= halfCellSize;
                            portalLeft.y += halfCellSize;
                            portalRight.x += halfCellSize;
                            portalRight.y += halfCellSize;

                            validPortal = true;
                        } break;
                }
            }
            else
            {
                portalLeft = new Point3d();
                portalRight = new Point3d();
            }

            return validPortal;
        }

        public AABB2d ComputeNavCellBounds2d(uint navCellIndex)
        {
            uint colomn = GetNavCellColomn(navCellIndex);
            uint row = GetNavCellRow(navCellIndex);

            return new AABB2d(
                new Point2d( // Min corner
                    m_boundingBox.Min.x + ((float)colomn * GameConstants.NAV_MESH_WORLD_UNITS_SIZE),
                    m_boundingBox.Max.y - ((float)row * GameConstants.NAV_MESH_WORLD_UNITS_SIZE) - GameConstants.NAV_MESH_WORLD_UNITS_SIZE),
                new Point2d( // Max corner
                    m_boundingBox.Min.x + ((float)colomn * GameConstants.NAV_MESH_WORLD_UNITS_SIZE) + GameConstants.NAV_MESH_WORLD_UNITS_SIZE,
                    m_boundingBox.Max.y - ((float)row * GameConstants.NAV_MESH_WORLD_UNITS_SIZE)));
        }

        public int GetNavRefConnectivityID(NavRef navRef)
        {
            int connectivityID = EMPTY_NAV_CELL;

            if (navRef.IsValid)
            {
                connectivityID = GetNavCellConnectivityID((uint)navRef.NavCellIndex);
            }

            return connectivityID;
        }

        public int GetNavCellConnectivityID(uint navCellIndex)
        {
            return m_navCells[navCellIndex].connectivityId;
        }

        public bool AreNavRefsConnected(NavRef navRefA, NavRef navRefB)
        {
            bool areConnected = false;

            if (navRefA.IsValid && navRefB.IsValid)
            {
                areConnected = navRefA.NavRoomKey.Equals(navRefB.NavRoomKey) &&
                    ((navRefA.NavCellIndex == navRefB.NavCellIndex) ||
                     (m_navCells[navRefA.NavCellIndex].connectivityId == m_navCells[navRefB.NavCellIndex].connectivityId));
            }

            return areConnected;
        }

        public bool PointCanSeeOtherPoint(Point3d a, Point3d b)
        {
            NavRef navRefA = ComputeNavRefAtPoint(a);
            NavRef navRefB = ComputeNavRefAtPoint(b);

            return NavRefCanSeeOtherNavRef(navRefA, navRefB);
        }

        public bool NavRefCanSeeOtherNavRef(NavRef navRefA, NavRef navRefB)
        {
            bool canSee = false;

            if (navRefA.IsValid && navRefB.IsValid)
            {
                canSee = true;

                if (m_pvs != null)
                {
                    NavCell navCellA = m_navCells[navRefA.NavCellIndex];
                    NavCell navCellB = m_navCells[navRefB.NavCellIndex];

                    canSee =
                        m_pvs.CanCellSeeOtherCell(
                            (uint)navCellA.pvsCellIndex,
                            (uint)navCellB.pvsCellIndex);
                }
            }

            return canSee;
        }

        public uint[] ComputeNavCellsInRadius(Point3d center, float radius, bool include_center)
        {
            List<uint> navCells = new List<uint>();
            NavRef centerRef = ComputeNavRefAtPoint(center);
            float radius_squared = radius * radius;

            if (centerRef.NavCellIndex != EMPTY_NAV_CELL)
            {
                uint centerCellIndex = (uint)centerRef.NavCellIndex;
                uint centerColomn = GetNavCellColomn(centerCellIndex);
                uint centerRow = GetNavCellRow(centerCellIndex);

                uint radiusInCells = (uint)Math.Ceiling(radius / GameConstants.NAV_MESH_WORLD_UNITS_SIZE);
                uint minColomnIndex = (centerColomn >= radiusInCells) ? centerColomn - radiusInCells : 0;
                uint maxColomnIndex = Math.Min(centerColomn + radiusInCells, m_colomnCount - 1);
                uint minRowIndex = (centerRow >= radiusInCells) ? centerRow - radiusInCells : 0;
                uint maxRowIndex = Math.Min(centerRow + radiusInCells, m_rowCount - 1);

                for (uint row = minRowIndex; row <= maxRowIndex; ++row)
                {
                    for (uint colomn = minColomnIndex; colomn <= maxColomnIndex; ++colomn)
                    {
                        uint cellIndex = GetNavCellIndex(row, colomn);

                        if ((cellIndex != centerCellIndex || include_center) &&
                            m_navCells[cellIndex].connectivityId == m_navCells[centerCellIndex].connectivityId)
                        {
                            Point3d cellCenter = ComputeNavCellCenter(cellIndex);

                            if (Point3d.DistanceSquared(cellCenter, center) <= radius_squared)
                            {
                                navCells.Add(cellIndex);
                            }
                        }
                    }
                }
            }

            return navCells.ToArray();
        }

        public bool Raycast(Point3d start, Point3d end, out float t)
        {
            NavRef startNavRef = ComputeNavRefAtPoint(start);
            NavRef endNavRef = ComputeNavRefAtPoint(end);

            return Raycast(start, startNavRef, end, endNavRef, out t);
        }

        public bool Raycast(
            Point3d start,
            NavRef startNavRef,
            Point3d end,
            NavRef endNavRef,
            out float t)
        {
            bool hit = false;

            if (!startNavRef.IsValid || !endNavRef.IsValid)
            {
                hit = true;
                t = 0.0f;
            }
            else if (startNavRef.Equals(endNavRef))
            {
                hit = false;
                t = 1.0f;
            }
            else
            {
                uint maxRaycastIteration = m_rowCount + m_colomnCount;
                uint iterationCount = 0;

                NavRef currentNavRef = startNavRef;
                AABB2d currentNavCellBounds = ComputeNavCellBounds2d((uint)currentNavRef.NavCellIndex);
                Point2d start2d = start.ToPoint2d();
                Vector2d rayDirection = (end - start).ToVector2d();
                float tEpsilon = MathConstants.POSITIONAL_EPSILON / rayDirection.Magnitude();

                t = 0.0f;

                while (t < 1.0f && !hit)
                {
                    float clipMinT = 0.0f;
                    float clipMaxT = 0.0f;

                    // Compute where the ray exists the bounding box of the cell
                    if (!currentNavCellBounds.ClipRay(
                            start2d,
                            rayDirection,
                            out clipMinT,
                            out clipMaxT))
                    {
                        // If we failed to clip against the bounds that we were suppose to be inside of, 
                        // just use the current t and rely on the positional epsilon advancement to get
                        // us back on track
                        Debug.Assert(false, "Raycast didn't clip against box it was suppose to intersect");
                        clipMinT = t;
                        clipMaxT = t;
                    }

                    if (clipMaxT < 1.0f)
                    {
                        // If we haven't gotten to the end of the ray yet, try to advance to the next nav cell
                        Point2d newTestPoint = start2d + rayDirection * (clipMaxT + tEpsilon);

                        // Find the adjacent neighboring cell, if any
                        currentNavRef = ComputeNavRefAtPoint(newTestPoint);

                        if (currentNavRef.IsValid)
                        {
                            currentNavCellBounds = ComputeNavCellBounds2d((uint)currentNavRef.NavCellIndex);

                            if (currentNavRef.Equals(endNavRef))
                            {
                                t = 1.0f;
                                hit = false;
                            }
                            else
                            {
                                t = Math.Min(clipMaxT + tEpsilon, 1.0f);
                                hit = false;
                            }
                        }
                        else
                        {
                            t = clipMaxT;
                            hit = true;
                        }
                    }
                    else
                    {
                        // Made it all the way to the end without hitting anything
                        t = 1.0f;
                        hit = false;
                    }

                    // Safety iteration max to prevent infinite loops
                    iterationCount++;
                    if (iterationCount > maxRaycastIteration)
                    {
                        // Something funky happened. Just assume we can see to the end
                        Debug.Assert(false, "Raycast hit iteration limit");
                        t = 1.0f;
                        hit = false;
                        break;
                    }
                }
            }

            return hit;
        }

        public bool TryGetValidNeighborNavCellIndex(
            uint navCellIndex,
            MathConstants.eDirection direction,
            out uint neighborCellIndex)
        {
            return NavMesh.TryGetValidNeighborNavCellIndex(
                m_navCells,
                m_colomnCount,
                m_rowCount,
                navCellIndex,
                direction,
                out neighborCellIndex);
        }

        private static bool TryGetValidNeighborNavCellIndex(
            NavCell[] navCells,
            uint colomnCount,
            uint rowCount,
            uint navCellIndex,
            MathConstants.eDirection direction,
            out uint neighborCellIndex)
        {
            bool hasNeighbor = false;

            neighborCellIndex = 0;

            if (navCells.Length > 1)
            {
                uint colomn = GetNavCellColomn(colomnCount, navCellIndex);
                uint row = GetNavCellRow(colomnCount, navCellIndex);

                switch (direction)
                {
                    case MathConstants.eDirection.down:
                        if ((row + 1) < rowCount)
                        {
                            neighborCellIndex = GetNavCellIndex(colomnCount, row + 1, colomn);
                            hasNeighbor = true;
                        }
                        break;
                    case MathConstants.eDirection.up:
                        if ((row - 1) > 0)
                        {
                            neighborCellIndex = GetNavCellIndex(colomnCount, row - 1, colomn);
                            hasNeighbor = true;
                        }
                        break;
                    case MathConstants.eDirection.left:
                        if ((colomn - 1) >= 0)
                        {
                            neighborCellIndex = GetNavCellIndex(colomnCount, row, colomn - 1);
                            hasNeighbor = true;
                        }
                        break;
                    case MathConstants.eDirection.right:
                        if ((colomn + 1) >= 0)
                        {
                            neighborCellIndex = GetNavCellIndex(colomnCount, row, colomn + 1);
                            hasNeighbor = true;
                        }
                        break;
                }

                if (hasNeighbor && navCells[neighborCellIndex].connectivityId == EMPTY_NAV_CELL)
                {
                    neighborCellIndex = 0;
                    hasNeighbor = false;
                }
            }

            return hasNeighbor;
        }

        public bool NavCellHasNeighbor(uint navCellIndex, MathConstants.eDirection direction)
        {
            uint neighborNavCellIndex;

            return TryGetValidNeighborNavCellIndex(navCellIndex, direction, out neighborNavCellIndex);
        }

        public void SetRoomKey(RoomKey roomKey)
        {
            m_roomKey = new RoomKey(roomKey);
        }

        public int GetNavCellCount()
        {
            return m_navCells.Length;
        }

        private uint GetNavCellColomn(uint navCellIndex)
        {
            return NavMesh.GetNavCellColomn(m_colomnCount, navCellIndex);
        }

        private uint GetNavCellRow(uint navCellIndex)
        {
            return NavMesh.GetNavCellRow(m_colomnCount, navCellIndex);
        }

        private uint GetNavCellIndex(uint row, uint colomn)
        {
            return NavMesh.GetNavCellIndex(m_colomnCount, row, colomn);
        }

        private static uint GetNavCellColomn(uint colomnCount, uint navCellIndex)
        {
            return (colomnCount > 0) ? navCellIndex % colomnCount : 0;
        }

        private static uint GetNavCellRow(uint colomnCount, uint navCellIndex)
        {
            return (colomnCount > 0) ? navCellIndex / colomnCount : 0;
        }

        private static uint GetNavCellIndex(uint colomnCount, uint row, uint colomn)
        {
            return (row * colomnCount) + colomn;
        }
    }

    public class NavCellNeighborIterator
    {
        private NavMesh m_navMesh;
        private uint m_navCellIndex;
        private MathConstants.eDirection m_neighborDirection;
        private uint m_neighborNavCellIndex;

        public NavCellNeighborIterator(NavMesh navMesh, uint navCellIndex)
        {
            m_navMesh = navMesh;
            m_navCellIndex = navCellIndex;
            m_neighborDirection = MathConstants.eDirection.none;
            m_neighborNavCellIndex = navCellIndex;

            Next();
        }

        public uint NeighborNavCellIndex
        {
            get { return m_neighborNavCellIndex; }
        }

        public bool Valid()
        {
            return m_neighborDirection < MathConstants.eDirection.count;
        }

        public void Next()
        {
            m_neighborDirection++;

            while (m_neighborDirection <= MathConstants.eDirection.count &&
                   !m_navMesh.TryGetValidNeighborNavCellIndex(m_navCellIndex, m_neighborDirection, out m_neighborNavCellIndex))
            {
                m_neighborDirection++;
            }
        }
    }
}
