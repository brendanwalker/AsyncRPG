using LitJson;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;

public class CreateCharacterModel
{
    private uint m_pictureId;
    private CreateCharacterController m_createCharacterController;

    public bool IsCharacterCreateRequestPending { get; private set; }

    public CreateCharacterModel(CreateCharacterController createCharacterController)
    {
        m_pictureId= 0;
        m_createCharacterController = createCharacterController;
        IsCharacterCreateRequestPending = false;
    }

	public GameConstants.eGender GetGender()
	{			
		return ClientGameConstants.GetGenderForPicture(m_pictureId);
	}
		
	public GameConstants.eArchetype GetArchetype()
	{
		return ClientGameConstants.GetArchetypeForPicture(m_pictureId);
	}

	public uint GetPictureId()
	{
		return m_pictureId;
	}
		
	public void SelectNextPicture()
	{
		m_pictureId = (m_pictureId + 1) % (uint)ClientGameConstants.GetPortraitCount();
	}
		
	public void SelectPreviousPicture()
	{
        uint portraitCount = (uint)ClientGameConstants.GetPortraitCount();

        m_pictureId = (m_pictureId + portraitCount - 1) % portraitCount;
	}

    public void CreateCharacter(
        string characterName)
    {
        if (!IsCharacterCreateRequestPending)
        {
            AsyncJSONRequest gameCreateRequest = AsyncJSONRequest.Create(m_createCharacterController.gameObject);
            Dictionary<string, object> requestParameters = new Dictionary<string, object>();

            requestParameters["name"] = characterName;
            requestParameters["archetype"] = (int)GetArchetype();
            requestParameters["gender"] = (int)GetGender();
            requestParameters["picture_id"] = GetPictureId();

            IsCharacterCreateRequestPending = true;

            gameCreateRequest.POST(
                ServerConstants.characterCreateRequestURL,
                requestParameters,
                (AsyncJSONRequest asyncRequest) =>
            {
                if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                {
                    JsonData response = asyncRequest.GetResult();
                    string responseResult = (string)response["result"];

                    if (responseResult== "Success")
                    {
                        m_createCharacterController.OnCharacterCreated();
                    }
                    else
                    {
                        m_createCharacterController.OnRequestFailed(responseResult);
                        Debug.LogError("Create Character Failed: " + asyncRequest.GetFailureReason());
                    }
                }
                else
                {
                    m_createCharacterController.OnRequestFailed("Connection Failure!");
                    Debug.LogError("Create Character Failed: " + asyncRequest.GetFailureReason());
                }

                IsCharacterCreateRequestPending = false;
            });
        }
    }
}
