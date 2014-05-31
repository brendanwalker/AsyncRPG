using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;

public class AsyncRPG : MonoBehaviour 
{
    private GameManager m_gameManager;
    private RoomTemplateManager m_roomTemplateManager;
    private MobTypeManager m_mobTypeManager;

	// Use this for initialization
	void Start () 
    {
        Screen.SetResolution(GameConstants.GAME_SCREEN_PIXEL_WIDTH, GameConstants.GAME_SCREEN_PIXEL_HEIGHT, false);

        m_gameManager = this.gameObject.AddComponent("GameManager") as GameManager;
        m_gameManager.SetTargetGameState("LoginScreen");

        m_roomTemplateManager=
            (GameObject.Instantiate(Resources.Load("RoomTemplates")) as GameObject).GetComponent<RoomTemplateManager>();

        m_mobTypeManager =
            (GameObject.Instantiate(Resources.Load("MobTypes")) as GameObject).GetComponent<MobTypeManager>();
	}

    void OnDestroy()
    {
        Object.Destroy(m_roomTemplateManager);
        Object.Destroy(m_gameManager);
        Object.Destroy(m_mobTypeManager);
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
