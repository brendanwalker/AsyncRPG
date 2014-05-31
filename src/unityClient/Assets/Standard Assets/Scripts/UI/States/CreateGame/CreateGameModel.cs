using LitJson;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;

public class CreateGameModel
{
    private CreateGameController m_createGameController;

    public bool IsGameCreateRequestPending { get; private set; }

    public CreateGameModel(CreateGameController createGameController)
    {
        m_createGameController = createGameController;
        IsGameCreateRequestPending = false;
    }

    public void CreateGame(
        string gameName,
        GameConstants.eDungeonSize dungeonSize,
        GameConstants.eDungeonDifficulty dungeonDifficulty,
        string ircServer,
        uint ircPort,
        bool ircEnabled,
        bool ircEncryptionEnabled)
    {
        if (!IsGameCreateRequestPending)
        {
            AsyncJSONRequest gameCreateRequest = AsyncJSONRequest.Create(m_createGameController.gameObject);
            Dictionary<string, object> requestParameters = new Dictionary<string, object>();

            requestParameters["game_name"] = gameName;
            requestParameters["irc_enabled"] = ircEnabled;
            requestParameters["irc_server"] = ircServer;
            requestParameters["irc_port"] = ircPort;
            requestParameters["irc_encryption_enabled"] = ircEncryptionEnabled;
            requestParameters["dungeon_size"] = (int)dungeonSize;
            requestParameters["dungeon_difficulty"] = (int)dungeonDifficulty;

            IsGameCreateRequestPending = true;

            gameCreateRequest.POST(
                ServerConstants.gameCreateRequestURL,
                requestParameters,
                (AsyncJSONRequest asyncRequest) =>
            {
                if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
                {
                    JsonData response = asyncRequest.GetResult();
                    string responseResult = (string)response["result"];

                    if (responseResult== "Success")
                    {
                        m_createGameController.OnGameCreated();
                    }
                    else
                    {
                        m_createGameController.OnRequestFailed(responseResult);
                        Debug.LogError("Create Game Failed: " + asyncRequest.GetFailureReason());
                    }
                }
                else
                {
                    m_createGameController.OnRequestFailed("Connection Failure!");
                    Debug.LogError("Create Game Failed: " + asyncRequest.GetFailureReason());
                }

                IsGameCreateRequestPending = false;
            });
        }
    }
}
