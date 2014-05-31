using AsyncRPGSharedLib.Common;
using System;
using System.Collections.Generic;

namespace AsyncRPGSharedLib.Navigation
{
    public class RoomKey : IEquatable<RoomKey>
    {
        public int game_id;
        public int x, y, z;             // World room indices

        public RoomKey()
        {
            Set(-1, 0, 0, 0);
        }

        public RoomKey(int game_id, int x, int y, int z)
        {
            Set(game_id, x, y, z);
        }

        public RoomKey(RoomKey other)
        {
            Set(other);
        }

        public void Set(int game_id, int x, int y, int z)
        {
            this.game_id = game_id;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Set(RoomKey other)
        {
            this.game_id = other.game_id;
            this.x = other.x;
            this.y = other.y;
            this.z = other.z;
        }

        public RoomKey[] GetAdjacentRoomKeys()
        {
            return new RoomKey[] { 
                new RoomKey(game_id, x-1, y, z),
                new RoomKey(game_id, x+1, y, z),
                new RoomKey(game_id, x, y-1, z),
                new RoomKey(game_id, x, y+1, z),
                new RoomKey(game_id, x, y, z-1),
                new RoomKey(game_id, x, y, z+1),
            };
        }

        public MathConstants.eSignedDirection[] GetAdjacentRoomSides()
        {
            return new MathConstants.eSignedDirection[] {
                MathConstants.eSignedDirection.negative_x,
                MathConstants.eSignedDirection.positive_x,
                MathConstants.eSignedDirection.negative_y,
                MathConstants.eSignedDirection.positive_y,
                MathConstants.eSignedDirection.negative_z,
                MathConstants.eSignedDirection.positive_z,
            };
        }

        public static MathConstants.eSignedDirection GetOpposingRoomSide(
            MathConstants.eSignedDirection room_side)
        {
            MathConstants.eSignedDirection opposing_room_side = MathConstants.eSignedDirection.negative_x;

            switch (room_side)
            {
                case MathConstants.eSignedDirection.positive_x:
                    {
                        opposing_room_side = MathConstants.eSignedDirection.negative_x;
                    }
                    break;
                case MathConstants.eSignedDirection.negative_x:
                    {
                        opposing_room_side = MathConstants.eSignedDirection.positive_x;
                    }
                    break;
                case MathConstants.eSignedDirection.positive_y:
                    {
                        opposing_room_side = MathConstants.eSignedDirection.negative_y;
                    }
                    break;
                case MathConstants.eSignedDirection.negative_y:
                    {
                        opposing_room_side = MathConstants.eSignedDirection.positive_y;
                    }
                    break;
                case MathConstants.eSignedDirection.positive_z:
                    {
                        opposing_room_side = MathConstants.eSignedDirection.negative_z;
                    }
                    break;
                case MathConstants.eSignedDirection.negative_z:
                    {
                        opposing_room_side = MathConstants.eSignedDirection.positive_z;
                    }
                    break;
            }

            return opposing_room_side;
        }

        public bool Equals(RoomKey other)
        {
            return game_id == other.game_id && x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode()
        {
            return game_id + x * 1021 + y * 2053 + z * 4093; // Some random primes
        }

        public string GetHashKey()
        {
            return string.Format("{0},{1},{2},{3}", game_id, x, y, z);
        }
    }

    public class RoomKeyEqualityComparer : IEqualityComparer<RoomKey>
    {

        public bool Equals(RoomKey k1, RoomKey k2)
        {
            return k1.Equals(k2);
        }


        public int GetHashCode(RoomKey key)
        {
            return key.GetHashCode();
        }
    }
}
