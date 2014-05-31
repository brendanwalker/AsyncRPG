using LitJson;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameResponseEntry 
{
    public int game_id;
    public string game_name;
    public int owner_account_id;
    public string owner_account_name;
    public List<string> character_names;

	public GameResponseEntry() 
	{
		this.game_id= -1;
		this.game_name= "";
		this.owner_account_id= -1;
		this.owner_account_name = "";
		this.character_names = new List<string>();
	}
	
	public static GameResponseEntry FromObject(JsonData jsonData)
	{	
		GameResponseEntry result = new GameResponseEntry();

        result.game_id = (int)jsonData["game_id"];
        result.game_name = (string)jsonData["game_name"];
        result.owner_account_id = (int)jsonData["owner_account_id"];
        result.owner_account_name = (string)jsonData["owner_account_name"];

        JsonData character_names = jsonData["character_names"];
        for (int namesListIndex= 0; namesListIndex < character_names.Count; namesListIndex++)
        {
            result.character_names.Add((string)character_names[namesListIndex]);
        }
			
		return result;
	}
}
