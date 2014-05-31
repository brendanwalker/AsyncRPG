using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SessionData 
{
    public bool Authenticated { get; set; }
    public string UserName { get; set; }
    public string Cookie { get; set; }
    public List<CharacterData> CharacterList { get; set; }
    public List<GameResponseEntry> GameList { get; set; }

    private GameData m_currentGameData;

    private static SessionData m_instance= null;
		
	public SessionData() 
	{
		Reset();
	}
		
	public static SessionData GetInstance() 
	{
        if (m_instance == null)
        {
            m_instance = new SessionData();
        }

		return m_instance;
	}
		
	public void Reset()
	{
		UserName = "";
		Authenticated = false;
        Cookie = "";
        CharacterList = new List<CharacterData>();
        GameList = new List<GameResponseEntry>();
        m_currentGameData = new GameData();
	}


    public GameData CurrentGameData
    {
        get { return m_currentGameData; }
        set { m_currentGameData= value; }
    }

    public int CharacterID
    {
        get { return m_currentGameData.CharacterID; }
        set { m_currentGameData.CharacterID = value; }
    }
	
    public int GameID
    {
        get { return m_currentGameData.GameID; }
        set { m_currentGameData.GameID= value; }
    }
		
    		
    public string GameName
    {    
        get { return m_currentGameData.GameName; }
        set { m_currentGameData.GameName= value; }
    }
}
