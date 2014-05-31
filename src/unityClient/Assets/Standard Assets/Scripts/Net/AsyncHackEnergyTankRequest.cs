using UnityEngine;
using System.Collections;
using LitJson;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using System.Collections.Generic;

public class AsyncHackEnergyTankRequest 
{
    public delegate void OnRequestCompleteDelegate(AsyncHackEnergyTankRequest asyncRequest);

    private const float WITHIN_ENERGY_TANK_TOLERANCE_SQUARED = 0.1f * 0.1f;

    public enum eRequestStatus
    {
	    preflight,
	    pending_path_find,
        pending_server_response,
	    completed
    }

    public enum eResult
    {
        none,
        success,
        failed_path,
        failed_server_connection,
        failed_server_denied
    }
				
	public eRequestStatus Status { get; private set; }

    public Point3d OriginalPosition { get; private set; }

	public EnergyTankData TargetEnergyTank { get; private set; }
    public Point3d EnergyTankApproachPoint { get; private set; }

    public eResult ResultCode { get; private set; }
    public string ResultDetails { get; private set; }
    public JsonData ServerResponse { get; private set; }

    private GameObject m_requestOwnerObject;
	private CharacterEntity m_entity;
	private OnRequestCompleteDelegate m_onCompleteCallback;

    public AsyncHackEnergyTankRequest(
        GameObject requestOwnerObject, 
        CharacterEntity entity,
        Point3d entryPoint, 
        EnergyTankData energyTank) 
	{		
		// Remember where we came from in case the server rejects our predictive movement request
		CharacterData characterData = entity.MyCharacterData;

		OriginalPosition = new Point3d(characterData.x, characterData.y, characterData.z);
        TargetEnergyTank = energyTank;
        EnergyTankApproachPoint = entryPoint;
			
		m_entity = entity;
        m_requestOwnerObject = requestOwnerObject;
									
		Status = eRequestStatus.preflight;
		ResultCode = eResult.none;
		ResultDetails = "";
		ServerResponse = null;
	}

					
	public void Execute(OnRequestCompleteDelegate onComplete)
	{
		if (Status != eRequestStatus.pending_path_find || Status != eRequestStatus.pending_server_response)
		{
			m_onCompleteCallback = onComplete;

            if (Point3d.DistanceSquared(OriginalPosition, EnergyTankApproachPoint) > WITHIN_ENERGY_TANK_TOLERANCE_SQUARED)
            {
                Status = eRequestStatus.pending_path_find;

                PathfindingSystem.AddPathRequest(
                    m_entity.CurrentRoomKey, 
                    m_entity.Position,
                    EnergyTankApproachPoint, 
                    this.OnPathComputed);
            }
            else
            {
                Status = eRequestStatus.pending_server_response;
            }	
		}
	}
		
	private void OnPathComputed(PathComputer pathResult) 
	{		
		// If the path find succeeded, the entity will set the new destination and start walking toward it
		if (pathResult.ResultCode == PathComputer.eResult.success)
		{
			// Tell the server where we would like to go
            AsyncJSONRequest serverRequest = AsyncJSONRequest.Create(m_requestOwnerObject);
            Dictionary<string, object> requestPayload = new Dictionary<string, object>();
				
			requestPayload["character_id"] = m_entity.MyCharacterData.character_id;
            requestPayload["energy_tank_id"] = TargetEnergyTank.energy_tank_id;
				
			Status = eRequestStatus.pending_server_response;

            serverRequest.POST(
                ServerConstants.energyTankHackRequestURL, 
                requestPayload,
                this.OnServerRequestComplete);
		}
		else 
		{
			Status = eRequestStatus.completed;				
			ResultCode = eResult.failed_path;
			ResultDetails = "Path Failure Code: " + pathResult.ResultCode.ToString();
				
			// Notify the caller that the request is completed
			m_onCompleteCallback(this);					
		}
	}				
		
	private void OnServerRequestComplete(AsyncJSONRequest serverRequest)
	{
		Status = eRequestStatus.completed;

		if (serverRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
		{
			ServerResponse = serverRequest.GetResult();

            string responseResult = JsonUtilities.ParseString(ServerResponse, "result");

            if (responseResult == "Success")
			{
				ResultCode = eResult.success;
				ResultDetails = "";
			}
			else 
			{
				// Successfully got response back, but server said no.
				ResultCode = eResult.failed_server_denied;
                ResultDetails = "Server Response: " + responseResult;
			}
		}
		else 
		{
			// Failed to get a response from the server
			ResultCode = eResult.failed_server_connection;
			ResultDetails = "Connection Failure: "+serverRequest.GetFailureReason();
		}
			
		// Notify the caller that the request is completed
		m_onCompleteCallback(this);
		m_onCompleteCallback = null;
	}
}
