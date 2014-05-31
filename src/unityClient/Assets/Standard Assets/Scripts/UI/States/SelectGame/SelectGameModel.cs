using LitJson;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectGameModel
{
    private SelectGameController m_selectGameController;
    private int m_selectedListIndex;

    public bool IsGameListRequestPending { get; private set; }
    public bool IsGameDeleteRequestPending { get; private set; }

    public SelectGameModel(SelectGameController selectGameController)
    {
        m_selectGameController = selectGameController;
        m_selectedListIndex = 0;
        IsGameListRequestPending = false;
        IsGameDeleteRequestPending = false;
    }

    public int SelectedListIndex
    {
        get
        {
            return m_selectedListIndex;
        }

        set
        {
            SessionData sessionData = SessionData.GetInstance();

            m_selectedListIndex = value;
            sessionData.GameID = sessionData.GameList[value].game_id;
            sessionData.GameName = sessionData.GameList[value].game_name;
        }
    }

	public List<object> GetGameList()
	{
		SessionData sessionData = SessionData.GetInstance();
        List<object> gameList = new List<object>();

        if (sessionData.Authenticated)
        {
            foreach (object gameEntry in sessionData.GameList)
            {
                gameList.Add(gameEntry);
            }
        }

        return gameList;
	}
		
	public GameResponseEntry GetGameEntry(int index) 
	{
		SessionData sessionData = SessionData.GetInstance();
			
		return sessionData.Authenticated ? sessionData.GameList[index] : null;
	}	
		
	public int GetGameCount() 
	{
		SessionData sessionData = SessionData.GetInstance();
			
		return sessionData.Authenticated ? sessionData.GameList.Count : 0;
	}

    public void RequestGameList()
    {
        if (!IsGameListRequestPending)
        {
            AsyncJSONRequest gameListRequest = AsyncJSONRequest.Create(m_selectGameController.gameObject);

            IsGameListRequestPending = true;

            gameListRequest.GET(
                ServerConstants.gameListRequestURL,
                (AsyncJSONRequest asyncRequest) =>
            {
                if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                {
                    JsonData response = asyncRequest.GetResult();
                    string responseResult = (string)response["result"];

                    if (responseResult== "Success")
                    {
                        SessionData sessionData = SessionData.GetInstance();                        
                        JsonData gamesList = response["game_list"];

                        sessionData.GameList= new List<GameResponseEntry>();
                        for (int listIndex = 0; listIndex < gamesList.Count; listIndex++)
                        {
                            sessionData.GameList.Add(GameResponseEntry.FromObject(gamesList[listIndex]));
                        }

                        if (sessionData.GameList.Count > 0)
                        {
                            SelectedListIndex = 0;
                        }

                        m_selectGameController.OnGameListUpdated();
                    }
                    else
                    {
                        m_selectGameController.OnRequestFailed(responseResult);
                        Debug.LogError("Get Game List Failed: " + asyncRequest.GetFailureReason());
                    }
                }
                else
                {
                    m_selectGameController.OnRequestFailed("Connection Failure!");
                    Debug.LogError("Get Game List Failed: " + asyncRequest.GetFailureReason());
                }

                IsGameListRequestPending = false;
            });
        }
    }

    public void RequestSelectedGameDeletion()
    {
        SessionData sessionData = SessionData.GetInstance();

        if (!IsGameDeleteRequestPending && sessionData.GameList.Count > 0)
        {
            AsyncJSONRequest gameDeleteRequest = AsyncJSONRequest.Create(m_selectGameController.gameObject);
            Dictionary<string, object> requestParameters = new Dictionary<string, object>();

            requestParameters["game_id"] = sessionData.GameID;

            IsGameDeleteRequestPending = true;

            gameDeleteRequest.POST(
                ServerConstants.gameDeleteRequestURL,
                requestParameters,
                (AsyncJSONRequest asyncRequest) =>
                {
                    if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                    {
                        JsonData response = asyncRequest.GetResult();
                        string responseResult = (string)response["result"];

                        if (responseResult == "Success")
                        {
                            // Request an updated game list now that we deleted a game
                            m_selectGameController.OnGameDeleted();
                        }
                        else
                        {
                            m_selectGameController.OnRequestFailed(responseResult);
                            Debug.LogError("Get Game List Failed: " + asyncRequest.GetFailureReason());
                        }
                    }
                    else
                    {
                        m_selectGameController.OnRequestFailed("Connection Failure!");
                        Debug.LogError("Get Game List Failed: " + asyncRequest.GetFailureReason());
                    }

                    IsGameDeleteRequestPending = false;
                });
        }
    }
}
