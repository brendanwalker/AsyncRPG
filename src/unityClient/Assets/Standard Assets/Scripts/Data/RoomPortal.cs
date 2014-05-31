using LitJson;
using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

public class RoomPortal 
{
	public int portal_id;
	public int target_portal_id;
	public AABB3d boundingBox;
		
	public RoomPortal() 
	{
		portal_id = -1;
		target_portal_id = -1;
		boundingBox = new AABB3d();
	}
	
	public static RoomPortal FromObject(JsonData jsonData)
	{
		RoomPortal portal = new RoomPortal();
			
		portal.portal_id = (int)jsonData["id"];
		portal.target_portal_id = (int)jsonData["target_portal_id"];

        float x0 = jsonData["x0"].IsInt ? (float)((int)jsonData["x0"]) : (float)((double)jsonData["x0"]);
        float y0 = jsonData["y0"].IsInt ? (float)((int)jsonData["y0"]) : (float)((double)jsonData["y0"]);
        float x1 = jsonData["x1"].IsInt ? (float)((int)jsonData["x1"]) : (float)((double)jsonData["x1"]);
        float y1 = jsonData["y1"].IsInt ? (float)((int)jsonData["y1"]) : (float)((double)jsonData["y1"]);
	
		portal.boundingBox.SetBounds2d(x0, y0, x1, y1);
			
		return portal;
	}
}