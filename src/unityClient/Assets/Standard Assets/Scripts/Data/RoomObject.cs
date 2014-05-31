using LitJson;
using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Environment;

public class RoomObject
{
    public int object_id;
    public Point3d position;

    public RoomObject()
    {
        object_id = -1;
        position = new Point3d();
    }

    public static RoomObject FromObject(JsonData jsonData)
    {
        RoomObject result = new RoomObject();

        result.object_id = (int)jsonData["id"];
        result.position.Set(
            jsonData["x"].IsInt ? (float)((int)jsonData["x"]) : (float)((double)jsonData["x"]),
            jsonData["y"].IsInt ? (float)((int)jsonData["y"]) : (float)((double)jsonData["y"]),
            jsonData["z"].IsInt ? (float)((int)jsonData["z"]) : (float)((double)jsonData["z"]));

        return result;
    }
}