using System;
using AsyncRPGSharedLib.Environment;

namespace AsyncRPGSharedLib.Common
{
    public class SuccessMessages
    {
        public const string GENERAL_SUCCESS = "Success";
        public const string ROOM_DOES_NOT_EXIST = "Room Does Not Exist";
    }

    public class ErrorMessages
    {
        public const string GENERAL_ERROR = "Error!";
        public const string DB_ERROR = "DB Error!";
        public const string CACHE_ERROR = "Cache Error!";
        public const string SMTP_ERROR = "SMTP Error!";
        public const string NOT_AUTHENTICATED = "Not Authenticated!";
        public const string INSUFFICIENT_OPS_LEVEL = "Insufficient Ops Level!";
        public const string MALFORMED_REQUEST = "Malformed Request!";
        public const string MALFORMED_EMAIL = "Malformed E-mail Address!";
        public const string INVALID_REQUEST = "Invalid Request!";
        public const string NOT_LOGGED_IN = "User Not Logged In!";
        public const string ALREADY_LOGGED_IN = "Account Already Logged In!";
        public const string ALREADY_BOUND = "Character Already Bound!";
        public const string NOT_BOUND = "Character Not Bound!";
        public const string RESERVED_USERNAME = "Username Already Taken!";
        public const string INVALID_GAME = "Invalid Game!";
        public const string INVALID_WORLD = "Invalid World!";
        public const string INVALID_ROOM = "Invalid Room!";
        public const string INVALID_USERNAME = "Invalid Username!";
        public const string INVALID_PASSWORD = "Invalid Password!";
        public const string NOT_GAME_OWNER = "Not Game Owner!";
        public const string INVALID_AUTHENTICATION = "Invalid Username or Password!";
        public const string UNVERIFIED_EMAIL = "Unverified E-mail Address!";
        public const string NOT_IN_PORTAL_BOUNDS = "Not In Portal Bounds!";
        public const string TARGET_UNREACHABLE = "Target Unreachable!";
        public const string MISSING_ROOM_TEMPLATE = "Missing Needed Room Template(s)!";
        public const string DUNGEON_LAYOUT_ERROR = "Dungeon Layout Error!";
        public const string ENERGY_TANK_PLAYER_OWNED = "Energy Tank Player Owned";
        public const string ENERGY_TANK_NOT_PLAYER_OWNED = "Energy Tank Not Player Owned";
        public const string ENERGY_TANK_EMPTY = "Energy Tank Empty";
        public const string CANT_COMPUTE_PATH = "Can't Compute Path!";
    }

    public class DatabaseConstants
    {
        public const string APPLICATION_NAME = "AsyncRPG";
        public const int APPLICATION_VERSION = 2;

        public enum OpsLevel
        {
            invalid = -1,
            player = 0,
            game_master = 1,
            admin = 2
        }
    }

    public class GameConstants
    {
        public enum eGender
        {
            Female = 0,
            Male = 1
        }

        public enum eArchetype
        {
            mage = 0,
            archer = 1,
            thief = 2,
            warrior = 3
        }

        public enum eDungeonSize
        {
            small = 0,
            medium = 1,
            large = 2,

            count
        }

        public enum eDungeonDifficulty
        {
            easy = 0,
            normal = 1,
            hard = 2,

            count
        }

        [Serializable]
        public enum eFaction
        {
            neutral = 0,
            ai = 1,
            player = 2
        }

        public const int GAME_SCREEN_PIXEL_WIDTH = 800;
        public const int GAME_SCREEN_PIXEL_HEIGHT = 600;

        public const float TILE_WORLD_UNITS_SIZE = 0.5f; // meters
        public const uint TILE_PIXEL_SIZE = 16;

        public const float NAV_MESH_WORLD_UNITS_SIZE = TILE_WORLD_UNITS_SIZE * 2.0f; // meters
        public const uint NAV_MESH_PIXEL_SIZE = TILE_PIXEL_SIZE * 2;

        public const float WORLD_UNITS_TO_PIXELS = (float)(TILE_PIXEL_SIZE) / TILE_WORLD_UNITS_SIZE;
        public const float PIXELS_TO_WORLD_UNITS = WorldConstants.ROOM_TILE_SIZE / (float)(TILE_PIXEL_SIZE);

        // Based on how many 16x16px tiles would fit on an 800x600 pixel display
        public const uint ROOM_X_TILES = 50;
        public const uint ROOM_Y_TILES = 36;

        // Based on how many 32x32px tiles would fit on an 800x600 pixel display
        public const uint NAV_MESH_X_TILES = 25;
        public const uint NAV_MESH_Y_TILES = 18;

        // The room dimensions in world units
        public const float ROOM_WIDTH = TILE_WORLD_UNITS_SIZE * (float)ROOM_X_TILES;
        public const float ROOM_HEIGHT = TILE_WORLD_UNITS_SIZE * (float)ROOM_Y_TILES;

        // The room dimensions in pixels
        public const uint ROOM_PIXEL_WIDTH = TILE_PIXEL_SIZE * WorldConstants.ROOM_X_TILES;
        public const uint ROOM_PIXEL_HEIGHT = TILE_PIXEL_SIZE * WorldConstants.ROOM_Y_TILES;

        public static string GetGenderString(eGender gender)
        {
            string result = "";

            switch (gender)
            {
                case eGender.Female:
                    result = "Female";
                    break;
                case eGender.Male:
                    result = "Male";
                    break;
            }

            return result;
        }

        public static string GetArchetypeString(eArchetype archetype)
        {
            string result = "";

            switch (archetype)
            {
                case eArchetype.archer:
                    result = "Archer";
                    break;
                case eArchetype.mage:
                    result = "Mage";
                    break;
                case eArchetype.thief:
                    result = "Thief";
                    break;
                case eArchetype.warrior:
                    result = "Warrior";
                    break;
            }

            return result;
        }

        public static Point3d ConvertPixelPositionToRoomPosition(Point2d pixelPosition)
        {
            return ConvertPixelPositionToRoomPosition(pixelPosition.x, pixelPosition.y);
        }

        public static Point3d ConvertPixelPositionToRoomPosition(float pixelPositionX, float pixelPositionY)
        {
            float roomX = (pixelPositionX - ((float)ROOM_PIXEL_WIDTH / 2.0f)) * PIXELS_TO_WORLD_UNITS;
            float roomY = (((float)ROOM_PIXEL_HEIGHT / 2.0f) - pixelPositionY) * PIXELS_TO_WORLD_UNITS;

            return new Point3d(roomX, roomY, 0.0f);
        }

        public static Point2d ConvertRoomPositionToPixelPosition(Point3d roomPosition)
        {
            float pixelX = (roomPosition.x * WORLD_UNITS_TO_PIXELS) + ((float)ROOM_PIXEL_WIDTH / 2.0f);
            float pixelY = ((float)ROOM_PIXEL_HEIGHT / 2.0f) - (roomPosition.y * WORLD_UNITS_TO_PIXELS);

            return new Point2d(pixelX, pixelY);
        }
    }

    public class WorldConstants
    {
        public const string DEFAULT_TILE_SET = "outdoor_tileset";

        // Based on how many 16x16px tiles would fit on an 800x600 pixel display
        public const int ROOM_X_TILES = 50;
        public const int ROOM_Y_TILES = 36;

        // The room size in meters
        public const float ROOM_TILE_SIZE = 0.5F; // meters (~0.5 meters)
        public const float ROOM_X_SIZE = (float)ROOM_X_TILES * ROOM_TILE_SIZE; // meters 
        public const float ROOM_Y_SIZE = (float)ROOM_Y_TILES * ROOM_TILE_SIZE; // meters
        public const float ROOM_Z_SIZE = ROOM_X_SIZE; //3; // meters (~10 feet)

        // Room Relative Bounds in meters
        public static AABB3d ROOM_BOUNDS =
            new AABB3d(
                new Point3d(-ROOM_X_SIZE / 2F, -ROOM_Y_SIZE / 2F, -ROOM_Z_SIZE / 2F),
                new Point3d(ROOM_X_SIZE / 2F, ROOM_Y_SIZE / 2F, ROOM_Z_SIZE / 2F));

        // Portal constants
        public const float PORTAL_WIDTH = ROOM_TILE_SIZE;
        public const float GENERATE_DOOR_CHANCE = 0.9F;
        public const float GENERATE_DOWN_STAIRS_CHANCE = 0.1F;
        public const float GENERATE_UP_STAIRS_CHANCE = 0.01F;

        public static Point3d GetTilePosition(int column, int row)
        {
            float x_offset = (float)column * ROOM_TILE_SIZE;
            float y_offset = (float)row * ROOM_TILE_SIZE;

            return ROOM_BOUNDS.Min + Vector3d.I * x_offset + Vector3d.J * y_offset;
        }
    }

    public class MathConstants
    {
        public const float REAL_MIN = -float.MaxValue;
        public const float REAL_MAX = float.MaxValue;

        public const float EPSILON = 0.001f;
        public const float EPSILON_SQUARED = EPSILON * EPSILON;

        public const float POSITIONAL_EPSILON = 0.1f;
        public const float POSITIONAL_EPSILON_SQUARED = POSITIONAL_EPSILON * POSITIONAL_EPSILON;

        public const float PI = (float)Math.PI;
        public const float TWO_PI = 2.0f * (float)Math.PI;
        public const float DEGREES_TO_RADIANS = PI / 180.0f;
        public const float RADIANS_TO_DEGREES = 180.0f / PI;

        public enum eDirection
        {
            none = -1,
            right = 0,
            up = 1,
            left = 2,
            down = 3,

            first = right,
            count = 4
        }

        public enum eSignedDirection
        {
            none = -1,
            positive_x = 0,
            negative_x = 1,
            positive_y = 2,
            negative_y = 3,
            positive_z = 4,
            negative_z = 5,

            count,
            first = positive_x,
        }

        public static eDirection GetDirectionForVector(Vector3d vector)
        {
            eDirection direction = eDirection.none;

            if (vector.MagnitudeSquared() > EPSILON_SQUARED)
            {
                if (vector.i > 0)
                {
                    if (vector.j > vector.i)
                    {
                        direction = eDirection.down;
                    }
                    else if (vector.j < -vector.i)
                    {
                        direction = eDirection.up;
                    }
                    else
                    {
                        direction = eDirection.right;
                    }
                }
                else
                {
                    if (vector.j > -vector.i)
                    {
                        direction = eDirection.down;
                    }
                    else if (vector.j < vector.i)
                    {
                        direction = eDirection.up;
                    }
                    else
                    {
                        direction = eDirection.left;
                    }
                }
            }

            return direction;
        }

        public static eDirection GetDirectionForVector(Vector2d vector)
        {
            eDirection direction = eDirection.none;

            if (vector.MagnitudeSquared() > EPSILON_SQUARED)
            {
                if (vector.i > 0)
                {
                    if (vector.j > vector.i)
                    {
                        direction = eDirection.down;
                    }
                    else if (vector.j < -vector.i)
                    {
                        direction = eDirection.up;
                    }
                    else
                    {
                        direction = eDirection.right;
                    }
                }
                else
                {
                    if (vector.j > -vector.i)
                    {
                        direction = eDirection.down;
                    }
                    else if (vector.j < vector.i)
                    {
                        direction = eDirection.up;
                    }
                    else
                    {
                        direction = eDirection.left;
                    }
                }
            }

            return direction;
        }

        public static float GetAngleForVector(Vector3d v)
        {
            // (1, 0) -> 0 degrees
            // (0, 1) -> 90 degrees
            // (-1, 0) -> 180 degrees
            // (0, -1) -> 270 degrees
            return (((float)Math.Atan2(v.j, v.i) + MathConstants.TWO_PI) % MathConstants.TWO_PI) * RADIANS_TO_DEGREES;
        }

        public static float GetAngleForVector(Vector2d v)
        {
            // (1, 0) -> 0 degrees
            // (0, 1) -> 90 degrees
            // (-1, 0) -> 180 degrees
            // (0, -1) -> 270 degrees
            return (((float)Math.Atan2(v.j, v.i) + MathConstants.TWO_PI) % MathConstants.TWO_PI) * RADIANS_TO_DEGREES;
        }

        public static Vector2d GetUnitVectorForAngle(float angle)
        {
            float radians = angle * DEGREES_TO_RADIANS;

            return new Vector2d((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        public static eDirection GetDirectionForAngle(float angle)
        {
            return GetDirectionForVector(Vector3d.FromAngle(angle * DEGREES_TO_RADIANS));
        }

        public static float GetAngleForDirection(eDirection direction)
        {
            float angle = 0;

            switch (direction)
            {
                case eDirection.right:
                    angle = 0.0f;
                    break;
                case eDirection.up:
                    angle = 270.0f;
                    break;
                case eDirection.left:
                    angle = 180.0f;
                    break;
                case eDirection.down:
                case eDirection.none:
                    angle = 90.0f;
                    break;
            }

            return angle;
        }
    }

    public class ApplicationConstants
    {
        public static bool IsDebuggingEnabled { get; set; }
        public static string CONNECTION_STRING { get; set; }
        public static string MAPS_DIRECTORY { get; set; }
        public static string MOBS_DIRECTORY { get; set; }

        public const string APPLICATION_WEB_URL = "http://localhost:8080/";
        public const string APPLICATION_DEBUG_WEB_URL = "http://localhost:8080/";
        public const string ACCOUNT_WEB_SERVICE_URL = APPLICATION_WEB_URL + "Account.asmx/";
        public const string ACCOUNT_DEBUG_WEB_SERVICE_URL = APPLICATION_DEBUG_WEB_URL + "Account.asmx/";
        public const string ACCOUNT_CLIENT_URL = APPLICATION_WEB_URL + "AsyncRPG.html";
    }

    public class MailConstants
    {
        public static string WEB_SERVICE_EMAIL_ADDRESS = "";
        public static string WEB_SERVICE_EMAIL_HOST = "";
        public static int WEB_SERVICE_EMAIL_PORT = 0;
        public static string WEB_SERVICE_EMAIL_USERNAME = "";
        public static string WEB_SERVICE_EMAIL_PASSWORD = "";

        public static bool VerifyAccountEmail 
        {
            get { 
                return 
                    WEB_SERVICE_EMAIL_ADDRESS.Length > 0 &&
                    WEB_SERVICE_EMAIL_HOST.Length > 0 &&
                    WEB_SERVICE_EMAIL_PORT > 0 &&
                    WEB_SERVICE_EMAIL_USERNAME.Length > 0 && 
                    WEB_SERVICE_EMAIL_PASSWORD.Length > 0; 
            }
        }
    }

    public class IrcConstants
    {
        // TODO: Initialize these in the app.config
        public const string DEFAULT_IRC_SERVER = "irc.freenode.net";
        public const int DEFAULT_IRC_PORT = 6667;
        public const int LOWEST_VALID_IRC_PORT = 1024;
        public const int HIGHEST_VALID_IRC_PORT = 65535;
    }
}
