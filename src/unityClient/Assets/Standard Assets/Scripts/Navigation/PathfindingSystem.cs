using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;

public class PathfindingSystem : MonoBehaviour 
{
    private const uint MAX_WORK_MILLISECONDS = 5;

    private static PathfindingSystem m_instance;
    private List<PathComputer> m_requestQueue;

	void Start () 
    {
        m_requestQueue = new List<PathComputer>();

        // TODO: Add a path cache in the event that a same start and end are requested multiple times in a row
        m_instance= this;
	}

    void OnDestroy()
    {
        m_instance= null;
    }
	
	void Update () 
    {
        Stopwatch stopWatch = new Stopwatch();
			
		// Keep processing requests while there is work to do and there is time left to do it
		while (m_requestQueue.Count > 0 && stopWatch.ElapsedMilliseconds < MAX_WORK_MILLISECONDS)
		{
			PathComputer pathRequest = m_requestQueue[0];
				
			// If the next job isn't complete do work on it in the time we have remaining
			if (pathRequest.State != PathComputer.eState.complete)
			{
                pathRequest.UpdateNonBlockingPathRequest(MAX_WORK_MILLISECONDS - (uint)stopWatch.ElapsedMilliseconds);					
			}				
				
			// If it is complete, remove it from the queue
			// The request will notify the requester when it's complete
			if (pathRequest.State == PathComputer.eState.complete)
			{
                m_requestQueue.RemoveAt(0);
			}			
		}	
	}

	public static bool AddPathRequest(
        RoomKey roomKey, 
        Point3d startPoint, 
        Point3d endPoint, 
        PathComputer.OnPathComputerComplete onComplete)
	{
		bool success = false;			
			
		if (m_instance != null)
		{
			PathComputer pathComputer = new PathComputer();
            AsyncRPGSharedLib.Navigation.NavMesh navMesh = GetNavMesh(roomKey);
				
			if (pathComputer.NonBlockingPathRequest(navMesh, roomKey, startPoint, endPoint, onComplete))
			{
                m_instance.m_requestQueue.Add(pathComputer);
				success = true;
			}				
		}
			
		return success;
	}

    public static AsyncRPGSharedLib.Navigation.NavMesh GetNavMesh(RoomKey roomKey)
    {
        SessionData sessionData = SessionData.GetInstance();
        GameData gameData = sessionData.CurrentGameData;
        RoomData roomData = gameData.GetCachedRoomData(roomKey);

        return roomData.StaticRoomData.NavMesh;
    }
}
