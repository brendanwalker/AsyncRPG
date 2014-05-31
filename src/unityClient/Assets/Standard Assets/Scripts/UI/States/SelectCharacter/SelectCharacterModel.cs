using LitJson;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectCharacterModel
{
    private SelectCharacterController m_selectCharacterController;
    private int m_selectedListIndex;

    public bool IsCharacterListRequestPending { get; private set; }
    public bool IsCharacterDeleteRequestPending { get; private set; }
    public bool IsCharacterBindRequestPending { get; private set; }

    public SelectCharacterModel(SelectCharacterController SelectCharacterController)
    {
        m_selectCharacterController = SelectCharacterController;
        m_selectedListIndex = 0;
        IsCharacterListRequestPending = false;
        IsCharacterDeleteRequestPending = false;
        IsCharacterBindRequestPending = false;
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
            sessionData.CharacterID = sessionData.CharacterList[value].character_id;
        }
    }

	public List<object> GetCharacterList()
	{
		SessionData sessionData = SessionData.GetInstance();
        List<object> characterList = new List<object>();

        if (sessionData.Authenticated)
        {
            foreach (object characterEntry in sessionData.CharacterList)
            {
                characterList.Add(characterEntry);
            }
        }

        return characterList;
	}
		
	public CharacterData GetCharacterEntry(int index) 
	{
		SessionData sessionData = SessionData.GetInstance();
			
		return sessionData.Authenticated ? sessionData.CharacterList[index] : null;
	}	
		
	public int GetCharacterCount() 
	{
		SessionData sessionData = SessionData.GetInstance();
			
		return sessionData.Authenticated ? sessionData.CharacterList.Count : 0;
	}

    public void RequestCharacterList()
    {
        if (!IsCharacterListRequestPending)
        {
            AsyncJSONRequest gameListRequest = AsyncJSONRequest.Create(m_selectCharacterController.gameObject);
            Dictionary<string, object> request = new Dictionary<string, object>();

            request["username"] = SessionData.GetInstance().UserName;

            IsCharacterListRequestPending = true;

            gameListRequest.POST(
                ServerConstants.characterListRequestURL,
                request,
                (AsyncJSONRequest asyncRequest) =>
            {
                if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                {
                    JsonData response = asyncRequest.GetResult();
                    string responseResult = (string)response["result"];

                    if (responseResult== "Success")
                    {
                        SessionData sessionData = SessionData.GetInstance();
                        JsonData characterList = response["character_list"];

                        sessionData.CharacterList= new List<CharacterData>();
                        for (int listIndex = 0; listIndex < characterList.Count; listIndex++)
                        {
                            sessionData.CharacterList.Add(CharacterData.FromObject(characterList[listIndex]));
                        }

                        if (sessionData.CharacterList.Count > 0)
                        {
                            SelectedListIndex = 0;
                        }

                        m_selectCharacterController.OnCharacterListUpdated();
                    }
                    else
                    {
                        m_selectCharacterController.OnRequestFailed(responseResult);
                        Debug.LogError("Get Character List Failed: " + asyncRequest.GetFailureReason());
                    }
                }
                else
                {
                    m_selectCharacterController.OnRequestFailed("Connection Failure!");
                    Debug.LogError("Get Game List Failed: " + asyncRequest.GetFailureReason());
                }

                IsCharacterListRequestPending = false;
            });
        }
    }

    public void RequestSelectedCharacterDeletion()
    {
        SessionData sessionData = SessionData.GetInstance();

        if (!IsCharacterDeleteRequestPending && sessionData.GameList.Count > 0)
        {
            AsyncJSONRequest gameDeleteRequest = AsyncJSONRequest.Create(m_selectCharacterController.gameObject);
            Dictionary<string, object> requestParameters = new Dictionary<string, object>();

            requestParameters["character_id"] = sessionData.CharacterList[m_selectedListIndex].character_id;

            IsCharacterDeleteRequestPending = true;

            gameDeleteRequest.POST(
                ServerConstants.characterDeleteRequestURL,
                requestParameters,
                (AsyncJSONRequest asyncRequest) =>
                {
                    if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                    {
                        JsonData response = asyncRequest.GetResult();
                        string responseResult = (string)response["result"];

                        if (responseResult == "Success")
                        {
                            // Request an updated character list now that we deleted a game
                            m_selectCharacterController.OnCharacterDeleted();
                        }
                        else
                        {
                            m_selectCharacterController.OnRequestFailed(responseResult);
                            Debug.LogError("Delete Character Failed: " + asyncRequest.GetFailureReason());
                        }
                    }
                    else
                    {
                        m_selectCharacterController.OnRequestFailed("Connection Failure!");
                        Debug.LogError("Delete Character Failed: " + asyncRequest.GetFailureReason());
                    }

                    IsCharacterDeleteRequestPending = false;
                });
        }
    }

    public void RequestBindCharacterToGame()
    {
        SessionData sessionData = SessionData.GetInstance();

        if (!IsCharacterBindRequestPending && sessionData.GameList.Count > 0)
        {
            AsyncJSONRequest gameDeleteRequest = AsyncJSONRequest.Create(m_selectCharacterController.gameObject);
            Dictionary<string, object> requestParameters = new Dictionary<string, object>();

            requestParameters["character_id"] = sessionData.CharacterID;
            requestParameters["game_id"] = sessionData.GameID;

            IsCharacterBindRequestPending = true;

            gameDeleteRequest.POST(
                ServerConstants.bindCharacterToGameRequestURL,
                requestParameters,
                (AsyncJSONRequest asyncRequest) =>
                {
                    if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                    {
                        JsonData response = asyncRequest.GetResult();
                        string responseResult = (string)response["result"];

                        if (responseResult == "Success")
                        {
                            // Request an updated character list now that we deleted a game
                            m_selectCharacterController.OnCharacterBoundToGame();
                        }
                        else
                        {
                            m_selectCharacterController.OnRequestFailed(responseResult);
                            Debug.LogError("Bind CharacterTo Game Failed: " + asyncRequest.GetFailureReason());
                        }
                    }
                    else
                    {
                        m_selectCharacterController.OnRequestFailed("Connection Failure!");
                        Debug.LogError("Bind CharacterTo Game Failed: " + asyncRequest.GetFailureReason());
                    }

                    IsCharacterBindRequestPending = false;
                });
        }
    }
}
