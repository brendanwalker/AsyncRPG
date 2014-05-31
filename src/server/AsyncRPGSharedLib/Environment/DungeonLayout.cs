using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGSharedLib.Environment
{
    public class RoomLayout : Room
    {
        public List<EnergyTank> energyTanks;

        public RoomLayout(RoomKey rk)
            : base(rk)
        {
            energyTanks = new List<EnergyTank>();
        }
    }

    public class DungeonLayout
    {
        private int m_gameId;
        private WorldTemplate m_worldTemplate;
        private RoomTemplateSet m_roomTemplateSet;
        private MobSpawnTableSet m_mobSpawnTableSet;
        private RoomLayout[, ,] m_roomGrid;
        private RoomKey m_minRoomKey;
        private int m_nextPortalId;

        public DungeonLayout(
            int gameId, 
            WorldTemplate worldTemplate,
            RoomTemplateSet roomTemplateSet,
            MobSpawnTableSet mobSpawnTableSet)
        {
            m_gameId = gameId;
            m_worldTemplate = worldTemplate;
            m_roomTemplateSet = roomTemplateSet;
            m_mobSpawnTableSet = mobSpawnTableSet;
            m_roomGrid = null;
            m_nextPortalId = 0;
        }

        public bool BuildRoomLayout(out string result)
        {
            Random rng = new Random(m_gameId);
            UnionFind<RoomIndex> roomUnion = new UnionFind<RoomIndex>();
            Dictionary<int, List<RoomIndex>> connectivityIdToRoomIndexMap = new Dictionary<int, List<RoomIndex>>();

            bool success = true;

            result = SuccessMessages.GENERAL_SUCCESS;

            // Generate an initial set of floors
            CreateFullyConnectedRoomGrid(rng);

            // Randomly remove a subset of rooms from the grid
            RandomlyRemoveRooms(rng);

            // Compute the connectivity_id for each room.
            // Two rooms with the same connectivity_id are in some way connected via doors or stairs.
            ComputeRoomConnectivity(roomUnion);

            // Group together rooms with the same connectivity id
            ComputeConnectivityIdToRoomIndexMap(roomUnion, connectivityIdToRoomIndexMap);

            // Try to connect disjoint sets of rooms together that are neighboring
            JoinAdjacentDisjointRoomSets(connectivityIdToRoomIndexMap);

            // Delete single isolated rooms 
            FilterIsolatedRooms(connectivityIdToRoomIndexMap);

            // Pick a random template for each room that matches the portal constraints
            if (success && !SelectRoomTemplates(rng, out result))
            {
                success = false;
            }

            // Create the actual portals between the rooms
            if (success && !CreateNormalPortals(out result))
            {
                success = false;
            }

            // Create teleporters between isolated regions
            if (success && !CreateTeporterPortals(rng, connectivityIdToRoomIndexMap, out result))
            {
                success= false;
            }

            // Create the mob spawners in each room
            if (success && !CreateMobSpawners(rng, out result))
            {
                success = false;
            }

            // Create the energy tanks in each room
            if (success && !CreateEnergyTanks(rng, out result))
            {
                success = false;
            }

            return success;
        }

        private int CreateFullyConnectedRoomGrid(
            Random rng)
        {
            int lateralRoomCount = m_worldTemplate.DungeonLateralRoomCount;
            int floorCount = m_worldTemplate.DungeonFloorCount;
            int totalRoomCount = lateralRoomCount * lateralRoomCount * floorCount;

            m_roomGrid = new RoomLayout[lateralRoomCount, lateralRoomCount, floorCount];
            m_minRoomKey = new RoomKey(m_gameId, -lateralRoomCount / 2, -lateralRoomCount / 2, 0);

            // Fully connect the rooms on each floor, but leave each floor unconnected initially
            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid, RoomIndexIterator.eIterationType.allRooms); 
                iterator.Valid;
                iterator.Next())
            {
                RoomIndex roomIndex= iterator.Current;

                RoomKey roomKey =
                    new RoomKey(
                        m_gameId,
                        roomIndex.X - lateralRoomCount / 2,
                        roomIndex.Y - lateralRoomCount / 2,
                        roomIndex.Z);
                RoomLayout room = new RoomLayout(roomKey);

                if (roomIndex.X > 0)
                {
                    room.RoomFlagPortalOnSide(MathConstants.eSignedDirection.negative_x, true);
                }

                if (roomIndex.X < lateralRoomCount - 1)
                {
                    room.RoomFlagPortalOnSide(MathConstants.eSignedDirection.positive_x, true);
                }

                if (roomIndex.Y > 0)
                {
                    room.RoomFlagPortalOnSide(MathConstants.eSignedDirection.negative_y, true);
                }

                if (roomIndex.Y < lateralRoomCount - 1)
                {
                    room.RoomFlagPortalOnSide(MathConstants.eSignedDirection.positive_y, true);
                }

                m_roomGrid[roomIndex.X, roomIndex.Y, roomIndex.Z] = room;
            }

            // Randomly add stairs connecting each floor
            for (int z_index = 0; z_index < floorCount - 1; z_index++)
            {
                Range<int> stairsRange = m_worldTemplate.StairsPerFloor;
                IList<RoomIndex> randomRoomIndices = GetRoomIndexListForFloor(z_index);
                int desiredStairsCount = RNGUtilities.RandomInt(rng, stairsRange.Min, stairsRange.Max);
                int currentStairsCount = 0;

                RNGUtilities.DeterministicKnuthShuffle(rng, randomRoomIndices);

                foreach (RoomIndex roomIndex in randomRoomIndices)
                {
                    Room room = m_roomGrid[roomIndex.X, roomIndex.Y, roomIndex.Z];
                    Room roomAbove = m_roomGrid[roomIndex.X, roomIndex.Y, roomIndex.Z + 1];

                    // Only consider rooms of the configuration X+X-Y+Y- to add stairs to
                    // because we only have rooms with stairs for the templates
                    // X+X-Y+Y-Z+ and X+X-Y+Y-Z-
                    // We do this so that we can get away with 18 room templates rather than 64
                    if (room.RoomHasAllPossibleDoors && !room.RoomHasStairs)
                    {
                        room.RoomFlagPortalOnSide(MathConstants.eSignedDirection.positive_z, true);
                        roomAbove.RoomFlagPortalOnSide(MathConstants.eSignedDirection.negative_z, true);
                        ++currentStairsCount;
                    }

                    if (currentStairsCount >= desiredStairsCount)
                    {
                        break;
                    }
                }
            }

            return totalRoomCount;
        }

        private void RandomlyRemoveRooms(
            Random rng)
        {
            int originalRoomCount = m_roomGrid.GetLength(0) * m_roomGrid.GetLength(1) * m_roomGrid.GetLength(2);
            int roomCount = originalRoomCount;

            // Determine the minimum number of rooms we allow for this level
            Range<float> roomDensityRange = m_worldTemplate.DungeonRoomDensity;
            float roomDensity = RNGUtilities.RandomFloat(rng, roomDensityRange.Min, roomDensityRange.Max);
            int minRoomCount = (int)(Math.Ceiling((float)originalRoomCount * roomDensity));

            // Generate a randomized list of rooms
            IList<RoomIndex> randomRoomIndices = GetRoomIndexList();
            RNGUtilities.DeterministicKnuthShuffle(rng, randomRoomIndices);

            // Randomly remove rooms one at time until we can't remove any more
            foreach (RoomIndex roomIndex in randomRoomIndices)
            {
                if (CanRemoveRoomFromGrid(roomIndex))
                {
                    RemoveRoomFromGrid(roomIndex);

                    roomCount--;

                    if (roomCount <= minRoomCount)
                    {
                        break;
                    }
                }
            }
        }

        private bool CanRemoveRoomFromGrid(RoomIndex roomIndex)
        {
            RoomLayout room = GetRoomByIndex(roomIndex);
            bool canRemoveRoom= true;

            // Can't remove this room if it contains stairs
            // Or we are neighboring a room that contains stairs
            if (!room.RoomHasStairs)
            {
                RoomKey roomKey = GetRoomKeyForRoomIndex(roomIndex);

                // Can't remove the origin room because that's where the player is spawned in a new game
                canRemoveRoom= !(roomKey.x == 0 && roomKey.y == 0 && roomKey.z == 0);

                if (canRemoveRoom && room.RoomHasPortalOnSide(MathConstants.eSignedDirection.positive_x))
                {
                    canRemoveRoom&= !GetRoomByIndex(roomIndex.Offset(1, 0, 0)).RoomHasStairs;
                }

                if (canRemoveRoom && room.RoomHasPortalOnSide(MathConstants.eSignedDirection.negative_x))
                {
                    canRemoveRoom &= !GetRoomByIndex(roomIndex.Offset(-1, 0, 0)).RoomHasStairs;
                }

                if (canRemoveRoom && room.RoomHasPortalOnSide(MathConstants.eSignedDirection.positive_y))
                {
                    canRemoveRoom &= !GetRoomByIndex(roomIndex.Offset(0, 1, 0)).RoomHasStairs;

                }

                if (canRemoveRoom && room.RoomHasPortalOnSide(MathConstants.eSignedDirection.negative_y))
                {
                    canRemoveRoom &= !GetRoomByIndex(roomIndex.Offset(0, -1, 0)).RoomHasStairs;
                }
            }
            else
            {
                canRemoveRoom = false;
            }

            return canRemoveRoom;
        }

        private void RemoveRoomFromGrid(RoomIndex roomIndex)
        {
            RoomLayout room = GetRoomByIndex(roomIndex);

            // Neighbor on the +x side
            if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.positive_x))
            {
                RoomLayout posXRoom = GetRoomByIndex(roomIndex.Offset(1, 0, 0));

                // No more room on the -x side
                posXRoom.RoomFlagPortalOnSide(MathConstants.eSignedDirection.negative_x, false);
            }

            // Neighbor on the -x side
            if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.negative_x))
            {
                RoomLayout negXRoom = GetRoomByIndex(roomIndex.Offset(-1, 0, 0));

                // No more room on the +x side
                negXRoom.RoomFlagPortalOnSide(MathConstants.eSignedDirection.positive_x, false);
            }

            // Neighbor on the +y side
            if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.positive_y))
            {
                RoomLayout posYRoom = GetRoomByIndex(roomIndex.Offset(0, 1, 0));

                // No more room on the -y side
                posYRoom.RoomFlagPortalOnSide(MathConstants.eSignedDirection.negative_y, false);
            }

            // Neighbor on the -y side
            if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.negative_y))
            {
                RoomLayout negYRoom = GetRoomByIndex(roomIndex.Offset(0, -1, 0));

                // No more room on the +y side
                negYRoom.RoomFlagPortalOnSide(MathConstants.eSignedDirection.positive_y, false);
            }

            // Stomp this room
            m_roomGrid[roomIndex.X, roomIndex.Y, roomIndex.Z] = null;
        }

        private void ComputeRoomConnectivity(UnionFind<RoomIndex> roomUnion)
        {
            IList<RoomIndex> roomIndices = GetRoomIndexList();

            roomUnion = new UnionFind<RoomIndex>();

            // Add all of the rooms into the union
            foreach (RoomIndex roomIndex in roomIndices)
            {
                roomUnion.AddElement(roomIndex);
            }

            // Start connecting to our neighbors
            foreach (RoomIndex roomIndex in roomIndices)
            {
                RoomLayout room = GetRoomByIndex(roomIndex);

                // Union with the neighbor on the +x side
                if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.positive_x))
                {
                    roomUnion.Union(roomIndex, roomIndex.Offset(1, 0, 0));
                }

                // Union with the neighbor on the -x side
                if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.negative_x))
                {
                    roomUnion.Union(roomIndex, roomIndex.Offset(-1, 0, 0));
                }

                // Union with the neighbor on the +y side
                if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.positive_y))
                {
                    roomUnion.Union(roomIndex, roomIndex.Offset(0, 1, 0));
                }

                // Union with the neighbor on the -y side
                if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.negative_y))
                {
                    roomUnion.Union(roomIndex, roomIndex.Offset(0, -1, 0));
                }

                // Union with the neighbor on the +z side
                if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.positive_z))
                {
                    roomUnion.Union(roomIndex, roomIndex.Offset(0, 0, 1));
                }

                // Union with the neighbor on the -z side
                if (room.RoomHasPortalOnSide(MathConstants.eSignedDirection.negative_z))
                {
                    roomUnion.Union(roomIndex, roomIndex.Offset(0, 0, -1));
                }
            }

            // Store the final connectivity id on the room
            foreach (RoomIndex roomIndex in roomIndices)
            {
                RoomLayout room = GetRoomByIndex(roomIndex);

                room.connectivity_id = roomUnion.FindRootIndex(roomIndex);
            }
        }

        private void ComputeConnectivityIdToRoomIndexMap(
            UnionFind<RoomIndex> roomUnion,
            Dictionary<int, List<RoomIndex>> connectivityIdToRoomIndexMap)
        {
            IList<RoomIndex> roomIndices = GetRoomIndexList();

            foreach (RoomIndex roomIndex in roomIndices)
            {
                RoomLayout room = GetRoomByIndex(roomIndex);
                int connectivityId= room.connectivity_id;
                List<RoomIndex> connectedRooms= null;

                if (connectivityIdToRoomIndexMap.TryGetValue(connectivityId, out connectedRooms))
                {
                    connectedRooms.Add(roomIndex);
                }
                else
                {
                    connectedRooms = new List<RoomIndex>();
                    connectedRooms.Add(roomIndex);

                    connectivityIdToRoomIndexMap.Add(connectivityId, connectedRooms);
                }
            }
        }

        private void JoinAdjacentDisjointRoomSets(
            Dictionary<int, List<RoomIndex>> connectivityIdToRoomIndexMap)
        {
            // Look for any null room that neighboring at least two disjoint sets of rooms
            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid, RoomIndexIterator.eIterationType.nullRooms);
                iterator.Valid;
                iterator.Next())
            {
                RoomIndex nullRoomIndex = iterator.Current;
                RoomIndex[] neighboringIndices = new RoomIndex[4] {
                    nullRoomIndex.Offset(1, 0, 0),
                    nullRoomIndex.Offset(-1, 0, 0),
                    nullRoomIndex.Offset(0, 1, 0),
                    nullRoomIndex.Offset(0, -1, 0)
                };
                MathConstants.eSignedDirection[] neighborSideFlags = new MathConstants.eSignedDirection[4] {
                    MathConstants.eSignedDirection.positive_x,
                    MathConstants.eSignedDirection.negative_x,
                    MathConstants.eSignedDirection.positive_y,
                    MathConstants.eSignedDirection.negative_y
                };

                bool createNewRoom = false;
                TypedFlags<MathConstants.eSignedDirection> nullRoomNeighborFlags = new TypedFlags<MathConstants.eSignedDirection>();
                int lastConnectivityId = -1;

                for (int side_index = 0; side_index < 4; ++side_index)
                {
                    MathConstants.eSignedDirection neighborSide = neighborSideFlags[side_index];
                    RoomIndex neighborRoomIndex= neighboringIndices[side_index];

                    // See if an adjacent room exists on this side
                    RoomLayout neighborRoom = TryGetRoomByIndex(neighborRoomIndex);

                    if (neighborRoom != null)
                    {
                        // Record that a neighboring room was found in this side
                        nullRoomNeighborFlags.Set(neighborSide, true);

                        // See if the neighboring room is actually disjoint from a previous neighbor 
                        // we have found on another side (different connectivity_id)
                        int roomConnectivityId = neighborRoom.connectivity_id;

                        if (lastConnectivityId != -1 &&
                            roomConnectivityId != lastConnectivityId)
                        {
                            List<RoomIndex> roomSet = connectivityIdToRoomIndexMap[roomConnectivityId];
                            List<RoomIndex> lastRoomSet = connectivityIdToRoomIndexMap[lastConnectivityId];

                            // Make the connectivity ids match for rooms in both sets
                            roomSet.ForEach(rIndex => GetRoomByIndex(rIndex).connectivity_id = lastConnectivityId);

                            // Merge the rooms in the set into the previous set
                            lastRoomSet.AddRange(roomSet);

                            // Remove the set
                            connectivityIdToRoomIndexMap.Remove(roomConnectivityId);

                            // Since we have merged two adjacent sets we now need a new room
                            // to bridge the disjoin sets
                            createNewRoom = true;
                        }

                        // Keep track of the last valid connectivity we found for the next iteration
                        lastConnectivityId = neighborRoom.connectivity_id;
                    }
                }

                if (createNewRoom)
                {
                    // Create a new room at the null room index location
                    RoomLayout newRoom = new RoomLayout(GetRoomKeyForRoomIndex(nullRoomIndex));

                    // Record which neighbors the null room has
                    newRoom.portalRoomSideBitmask = nullRoomNeighborFlags;

                    // All neighbors should have the same connectivity id now
                    // so just get the connectivity id from the last valid neighbor
                    newRoom.connectivity_id = lastConnectivityId;

                    // Finally store the new room in the room grid
                    m_roomGrid[nullRoomIndex.X, nullRoomIndex.Y, nullRoomIndex.Z] = newRoom;

                    // Add the new room to the connectivity set it's helping to bridge
                    {
                        List<RoomIndex> lastRoomSet = connectivityIdToRoomIndexMap[lastConnectivityId];

                        lastRoomSet.Add(new RoomIndex(nullRoomIndex));
                    }

                    // Tell all neighboring rooms about the new room adjacent to it
                    for (int side_index = 0; side_index < 4; ++side_index)
                    {
                        MathConstants.eSignedDirection neighborSide = neighborSideFlags[side_index];
                        RoomIndex neighborRoomIndex = neighboringIndices[side_index];

                        // See if an adjacent room exists on this side
                        if (newRoom.portalRoomSideBitmask.Test(neighborSide))
                        {
                            RoomLayout neighborRoom = GetRoomByIndex(neighborRoomIndex);
                            MathConstants.eSignedDirection opposingNeighborSide = RoomKey.GetOpposingRoomSide(neighborSide);

                            neighborRoom.RoomFlagPortalOnSide(opposingNeighborSide, true);
                        }
                    }
                }
            }
        }

        private void FilterIsolatedRooms(
            Dictionary<int, List<RoomIndex>> connectivityIdToRoomIndexMap)
        {
            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid);
                iterator.Valid;
                iterator.Next())
            {
                RoomIndex roomIndex = iterator.Current;
                RoomLayout room = GetRoomByIndex(roomIndex);

                if (room.RoomIsIsolated)
                {
                    List<RoomIndex> connectedSet= connectivityIdToRoomIndexMap[room.connectivity_id];

                    // Remove the room from the connectivity set
                    connectivityIdToRoomIndexMap.Remove(room.connectivity_id);

                    // Remove the room from the room grid
                    m_roomGrid[roomIndex.X, roomIndex.Y, roomIndex.Z] = null;
                }
            }
        }

        private bool SelectRoomTemplates(Random rng, out string result)
        {            
            List<RoomTemplate> filteredRoomTemplates = new List<RoomTemplate>();
            bool success = true;

            result= SuccessMessages.GENERAL_SUCCESS;

            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid);
                success && iterator.Valid;
                iterator.Next())
            {
                RoomIndex roomIndex = iterator.Current;
                RoomLayout room = GetRoomByIndex(roomIndex);

                filteredRoomTemplates.Clear();
                m_roomTemplateSet.GetTemplatesWithPortalBitmask(room.portalRoomSideBitmask, filteredRoomTemplates);

                if (filteredRoomTemplates.Count > 0)
                {
                    RoomTemplate roomTemplate = filteredRoomTemplates[rng.Next(filteredRoomTemplates.Count)];

                    room.static_room_data.room_template_name = roomTemplate.TemplateName;
                }
                else
                {
                    result = ErrorMessages.MISSING_ROOM_TEMPLATE;
                    success = false;
                }
            }

            return success;
        }

        private bool CreateNormalPortals(out string result)
        {
            bool success = true;

            result = SuccessMessages.GENERAL_SUCCESS;

            // Create portals in each room
            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid);
                iterator.Valid;
                iterator.Next())
            {
                RoomIndex roomIndex = iterator.Current;
                RoomKey roomKey= GetRoomKeyForRoomIndex(roomIndex);
                RoomLayout room = GetRoomByIndex(roomIndex);
                RoomTemplate roomTemplate = m_roomTemplateSet.GetTemplateByName(room.static_room_data.room_template_name);

                foreach (PortalTemplate portalTemplate in roomTemplate.PortalTemplates)
                {
                    if (portalTemplate.PortalType != ePortalType.teleporter)
                    {
                        Portal portal = new Portal();

                        portal.bounding_box = portalTemplate.BoundingBox;
                        portal.room_side = portalTemplate.PortalRoomSide;
                        portal.room_x = roomKey.x;
                        portal.room_y = roomKey.y;
                        portal.room_z = roomKey.z;
                        portal.portal_type = portalTemplate.PortalType;

                        // This is a temp ID for the purpose of portal connection.
                        // A new ID will get assigned when inserting this portal into the DB.
                        portal.portal_id = m_nextPortalId;
                        ++m_nextPortalId;

                        // This get sets in the next pass
                        portal.target_portal_id = -1;

                        room.portals.Add(portal);
                    }
                }
            }

            // Connect neighboring portals
            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid);
                success && iterator.Valid;
                iterator.Next())
            {
                RoomIndex roomIndex = iterator.Current;
                RoomLayout room = GetRoomByIndex(roomIndex);

                foreach(Portal portal in room.portals)
                {
                    MathConstants.eSignedDirection opposingRoomSide = RoomKey.GetOpposingRoomSide(portal.room_side);
                    RoomIndex opposingRoomIndex = roomIndex.GetOpposingRoomIndex(portal.room_side);
                    RoomLayout opposingRoom = GetRoomByIndex(opposingRoomIndex);
                    Portal opposingPortal = opposingRoom.portals.Find(p => p.room_side == opposingRoomSide);

                    if (opposingPortal != null)
                    {
                        portal.target_portal_id = opposingPortal.portal_id;
                        opposingPortal.target_portal_id = portal.portal_id;
                    }
                    else
                    {
                        result = ErrorMessages.DUNGEON_LAYOUT_ERROR;
                        success = false;
                        break;
                    }
                }
            }

            return success;
        }

        private bool CreateTeporterPortals(
            Random rng,
            Dictionary<int, List<RoomIndex>> connectivityIdToRoomIndexMap,
            out string result)
        {
            List<int> connectivityIDs = connectivityIdToRoomIndexMap.Keys.ToList<int>();
            bool success = true;

            result = SuccessMessages.GENERAL_SUCCESS;

            if (connectivityIDs.Count > 1)
            {
                for (int list_index = 1; list_index < connectivityIDs.Count; ++list_index)
                {
                    int connectivityId = connectivityIDs[list_index-1];
                    int otherConnectivityId = connectivityIDs[list_index];

                    // Only consider rooms in a set that don't already have teleporters in them.
                    // We do this because we only have the guarantee of one portal template per room.
                    // We could always have enough rooms to pick from if:
                    // 1) Each connectivity set has at least 2 rooms (enforced by FilterIsolatedRooms)
                    // 2) For N room sets we make N-1 portal pairs
                    List<RoomIndex> roomIndexSetA =
                        (from r in connectivityIdToRoomIndexMap[connectivityId]
                         where GetRoomByIndex(r).portals.Count(p => p.portal_type == ePortalType.teleporter) == 0
                         select r).ToList();                        
                    List<RoomIndex> roomIndexSetB =
                        (from r in connectivityIdToRoomIndexMap[otherConnectivityId]
                         where GetRoomByIndex(r).portals.Count(p => p.portal_type == ePortalType.teleporter) == 0
                         select r).ToList();

                    if (roomIndexSetA.Count > 0 && roomIndexSetB.Count > 0)
                    {
                        RoomIndex roomIndexA = roomIndexSetA[rng.Next(roomIndexSetA.Count)];
                        RoomIndex roomIndexB = roomIndexSetB[rng.Next(roomIndexSetB.Count)];

                        Portal teleporterA= CreateTeleporterPortal(rng, roomIndexA);
                        Portal teleporterB= CreateTeleporterPortal(rng, roomIndexB);

                        if (teleporterA != null && teleporterB != null)
                        {
                            teleporterA.target_portal_id = teleporterB.portal_id;
                            teleporterB.target_portal_id = teleporterA.portal_id;
                        }
                        else
                        {
                            result = ErrorMessages.DUNGEON_LAYOUT_ERROR;
                            success = false;
                            break;
                        }
                    }
                    else
                    {
                        result = ErrorMessages.DUNGEON_LAYOUT_ERROR;
                        success = false;
                        break;
                    }
                }
            }

            return success;
        }        

        private Portal CreateTeleporterPortal(
            Random rng,
            RoomIndex roomIndex)
        {
            Portal portal = null;
            RoomLayout room = GetRoomByIndex(roomIndex);
            RoomTemplate roomTemplate = m_roomTemplateSet.GetTemplateByName(room.static_room_data.room_template_name);
            List<PortalTemplate> teleporterTemplates = 
                roomTemplate.PortalTemplates.Where(t => t.PortalType == ePortalType.teleporter).ToList();

            if (teleporterTemplates.Count > 0)
            {
                RoomKey roomKey = GetRoomKeyForRoomIndex(roomIndex);
                PortalTemplate portalTemplate = teleporterTemplates[rng.Next(teleporterTemplates.Count)]; 

                portal = new Portal();
                portal.bounding_box = portalTemplate.BoundingBox;
                portal.room_side = portalTemplate.PortalRoomSide;
                portal.portal_type = portalTemplate.PortalType;
                portal.room_x = roomKey.x;
                portal.room_y = roomKey.y;
                portal.room_z = roomKey.z;

                // This is a temp ID for the purpose of portal connection.
                // A new ID will get assigned when inserting this portal into the DB.
                portal.portal_id = m_nextPortalId;
                ++m_nextPortalId;

                // This get sets in the next pass
                portal.target_portal_id = -1;

                // Add the new portal to the room
                room.portals.Add(portal);
            }

            return portal;
        }


        private bool CreateMobSpawners(
            Random rng,
            out String result)
        {
            bool success = true;

            result= SuccessMessages.GENERAL_SUCCESS;

            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid);
                success && iterator.Valid;
                iterator.Next())
            {
                RoomIndex roomIndex = iterator.Current;
                RoomLayout room = GetRoomByIndex(roomIndex);
                RoomTemplate roomTemplate = m_roomTemplateSet.GetTemplateByName(room.static_room_data.room_template_name);

                foreach (MobSpawnerTemplate mobSpawnerTemplate in roomTemplate.MobSpawnerTemplates)
                {
                    MobSpawner spawner = 
                        MobSpawner.CreateMobSpawner(
                            room.room_key,
                            mobSpawnerTemplate, 
                            m_mobSpawnTableSet, 
                            rng);

                    // Don't bother adding mob spawners that don't have any spawns left
                    if (spawner.RemainingSpawnCount > 0)
                    {
                        room.mobSpawners.Add(spawner);
                    }
                }
            }

            return success;
        }

        private bool CreateEnergyTanks(
            Random rng,
            out String result)
        {
            bool success = true;

            result = SuccessMessages.GENERAL_SUCCESS;

            for (RoomIndexIterator iterator = new RoomIndexIterator(m_roomGrid);
                success && iterator.Valid;
                iterator.Next())
            {
                RoomIndex roomIndex = iterator.Current;
                RoomLayout room = GetRoomByIndex(roomIndex);
                RoomTemplate roomTemplate = m_roomTemplateSet.GetTemplateByName(room.static_room_data.room_template_name);

                foreach (EnergyTankTemplate energyTankTemplate in roomTemplate.EnergyTankTemplates)
                {
                    //TODO: CreateEnergyTanks - Scale back energy tank distribution based on difficulty
                    EnergyTank energyTank = EnergyTank.CreateEnergyTank(room.room_key, energyTankTemplate);

                    room.energyTanks.Add(energyTank);
                }
            }

            return success;
        }

        //-- Accessor Functions -----
        public int GameID
        {
            get { return m_gameId; }
        }

        public RoomLayout[, ,] RoomGrid
        {
            get { return m_roomGrid; }
        }

        public WorldTemplate LayoutWorldTemplate
        {
            get { return m_worldTemplate; }
        }

        public RoomTemplateSet LayoutRoomTemplateSet
        {
            get { return m_roomTemplateSet; }
        }

        public RoomLayout GetRoomByIndex(RoomIndex roomIndex)
        {
            return m_roomGrid[roomIndex.X, roomIndex.Y, roomIndex.Z];
        }

        public RoomLayout TryGetRoomByIndex(RoomIndex roomIndex)
        {
            RoomLayout room = null;
            int roomXIndex = roomIndex.X;
            int roomYIndex = roomIndex.Y;
            int roomZIndex = roomIndex.Z;

            if (roomXIndex >= 0 && roomXIndex < m_roomGrid.GetLength(0) &&
                roomYIndex >= 0 && roomYIndex < m_roomGrid.GetLength(1) &&
                roomZIndex >= 0 && roomZIndex < m_roomGrid.GetLength(2))
            {
                room = m_roomGrid[roomXIndex, roomYIndex, roomZIndex];
            }

            return room;
        }

        public RoomKey GetRoomKeyForRoomIndex(RoomIndex roomIndex)
        {
            return new RoomKey(
                m_minRoomKey.game_id,
                m_minRoomKey.x + roomIndex.X,
                m_minRoomKey.y + roomIndex.Y,
                m_minRoomKey.z + roomIndex.Z);
        }

        public IList<RoomIndex> GetRoomIndexList()
        {
            List<RoomIndex> indices = new List<RoomIndex>();

            for (int z_index = 0; z_index < m_roomGrid.GetLength(2); z_index++)
            {
                indices.AddRange(GetRoomIndexListForFloor(z_index));
            }

            return indices;
        }

        public IList<RoomIndex> GetRoomIndexListForFloor(int z_index)
        {
            List<RoomIndex> indices = new List<RoomIndex>();

            for (int y_index = 0; y_index < m_roomGrid.GetLength(1); y_index++)
            {
                for (int x_index = 0; x_index < m_roomGrid.GetLength(0); x_index++)
                {
                    RoomLayout room = m_roomGrid[x_index, y_index, z_index];

                    if (room != null)
                    {
                        indices.Add(new RoomIndex(x_index, y_index, z_index));
                    }
                }
            }

            return indices;
        }

        //-- Room Iteration --------

        public class RoomIndex : IEquatable<RoomIndex>
        {
            private int x;
            private int y;
            private int z;

            public RoomIndex(RoomIndex roomIndex)
            {
                this.x = roomIndex.x;
                this.y = roomIndex.y;
                this.z = roomIndex.z;
            }

            public RoomIndex(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public int X
            {
                get { return x; }
                set { x = value; }
            }
            public int Y
            {
                get { return y; }
                set { y = value; }
            }
            public int Z
            {
                get { return z; }
                set { z = value; }
            }

            public RoomIndex Offset(int dx, int dy, int dz)
            {
                return new RoomIndex(x + dx, y + dy, z + dz);
            }

            public RoomIndex GetOpposingRoomIndex(MathConstants.eSignedDirection room_side)
            {
                RoomIndex opposingRoomIndex = null;

                switch (room_side)
                {
                    case MathConstants.eSignedDirection.positive_x:
                        {
                            opposingRoomIndex = this.Offset(1, 0, 0);
                        }
                        break;
                    case MathConstants.eSignedDirection.negative_x:
                        {
                            opposingRoomIndex = this.Offset(-1, 0, 0);
                        }
                        break;
                    case MathConstants.eSignedDirection.positive_y:
                        {
                            opposingRoomIndex = this.Offset(0, 1, 0);
                        }
                        break;
                    case MathConstants.eSignedDirection.negative_y:
                        {
                            opposingRoomIndex = this.Offset(0, -1, 0);
                        }
                        break;
                    case MathConstants.eSignedDirection.positive_z:
                        {
                            opposingRoomIndex = this.Offset(0, 0, 1);
                        }
                        break;
                    case MathConstants.eSignedDirection.negative_z:
                        {
                            opposingRoomIndex = this.Offset(0, 0, -1);
                        }
                        break;
                }

                return opposingRoomIndex;
            }

            public bool Equals(RoomIndex other)
            {
                return
                    this.x == other.x &&
                    this.y == other.y &&
                    this.z == other.z;
            }

            public override int GetHashCode()
            {
                return (x * WorldTemplate.k_max_dungeon_lateral_room_count + y) * WorldTemplate.k_max_dungeon_lateral_room_count + z;
            }
        }

        public class RoomIndexIterator
        {
            public enum eIterationType
            {
                nonNullRooms,
                nullRooms,
                allRooms
            };

            private RoomLayout[, ,] m_roomGrid;
            private RoomIndex m_roomIndex;
            private eIterationType m_iterationType;

			public RoomIndexIterator(RoomLayout[, ,] roomGrid)
			{
				m_roomIndex = new RoomIndex(-1, 0, 0);
				m_roomGrid = roomGrid;
				m_iterationType = eIterationType.nonNullRooms;
				
				Next();
			}

            public RoomIndexIterator(RoomLayout[, ,] roomGrid, eIterationType type)
            {
                m_roomIndex = new RoomIndex(-1, 0, 0);
                m_roomGrid = roomGrid;
                m_iterationType = type;

                Next();
            }

            public RoomIndex Current
            {
                get { return m_roomIndex; }
            }

            public bool Valid
            {
                get { return m_roomIndex.Z < m_roomGrid.GetLength(2); }
            }

            public bool Next()
            {
                bool foundValid = false;

                while (!foundValid && m_roomIndex.Z < m_roomGrid.GetLength(2))
                {
                    m_roomIndex.X++;

                    if (m_roomIndex.X >= m_roomGrid.GetLength(0))
                    {
                        m_roomIndex.X = 0;
                        m_roomIndex.Y++;
                    }

                    if (m_roomIndex.Y >= m_roomGrid.GetLength(1))
                    {
                        m_roomIndex.X = 0;
                        m_roomIndex.Y = 0;
                        m_roomIndex.Z++;
                    }

                    if (m_roomIndex.Z < m_roomGrid.GetLength(2))
                    {
                        bool nonNullRoom = (m_roomGrid[m_roomIndex.X, m_roomIndex.Y, m_roomIndex.Z] != null);

                        if (m_iterationType == eIterationType.allRooms ||
                            nonNullRoom && m_iterationType == eIterationType.nonNullRooms ||
                            !nonNullRoom && m_iterationType == eIterationType.nullRooms)
                        {
                            foundValid = true;
                        }
                    }
                }

                return foundValid;
            }
        }
    }

}
