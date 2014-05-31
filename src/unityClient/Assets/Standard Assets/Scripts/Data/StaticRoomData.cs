using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using AsyncRPGSharedLib.Navigation;

public class StaticRoomData
{
    public List<RoomObject> RoomObjects { get; set; }
    public RoomTemplate RoomTemplate { get; set; }
    public AsyncRPGSharedLib.Navigation.NavMesh NavMesh { get; set; } // Regular grid of connectivity ids
		
	public StaticRoomData() 
	{
		RoomObjects = new List<RoomObject>();		
		RoomTemplate = null;
        NavMesh = new AsyncRPGSharedLib.Navigation.NavMesh();
	}
		
	public static StaticRoomData FromObject(RoomKey roomKey, JsonData jsonData)
	{
		StaticRoomData staticRoomData = new StaticRoomData();

        // Fill in the room objects
        JsonData roomObjectArray = jsonData["objects"];
        if (roomObjectArray != null)
        {
            for (int roomObjectIndex = 0; roomObjectIndex < roomObjectArray.Count; roomObjectIndex++)
            {
                JsonData roomObject = roomObjectArray[roomObjectIndex];
                RoomObject room = RoomObject.FromObject(roomObject);

                staticRoomData.RoomObjects.Add(room);
            }
        }
			
		// Retrieve the room template by name
        string templateName = (string)jsonData["room_template_name"];
		staticRoomData.RoomTemplate = RoomTemplateManager.GetRoomTemplate(templateName);
			
		// Copy the nav mesh from the template
        staticRoomData.NavMesh = 
            new AsyncRPGSharedLib.Navigation.NavMesh(
                roomKey, 
                staticRoomData.RoomTemplate.NavMeshTemplate);
																
		return staticRoomData;
	}
}
