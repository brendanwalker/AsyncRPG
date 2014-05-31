using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    private static GameManager m_instance = null;

    private GameState m_currentGameState= null;
    private string m_targetGameStateResource= "";

	// Use this for initialization
	void Start () 
    {
        m_instance = this;
	}

    void OnDestroy()
    {
        m_instance = null;
    }

    public static GameManager GetInstance()
    {
        return m_instance;
    }

	// Update is called once per frame
	void Update () 
    {
        string currentStateName= (m_currentGameState != null) ? m_currentGameState.StateName : "";

        if (m_targetGameStateResource != currentStateName)
        {
            Debug.Log("GameManager: New State Requested: " + m_targetGameStateResource);

            // Free the old game state
            if (m_currentGameState != null)
            {
                Debug.Log("GameManager: Deleting Old State: " + m_currentGameState.StateName);
                Object.Destroy(m_currentGameState.gameObject);
                m_currentGameState= null;
            }

            // Create the new game state
            if (m_targetGameStateResource.Length > 0)
            {
                Debug.Log("GameManager: Creating New State: " + m_targetGameStateResource);

                m_currentGameState =
                    (GameObject.Instantiate(Resources.Load(m_targetGameStateResource)) as GameObject).GetComponent<GameState>();
                m_currentGameState.StateName = m_targetGameStateResource;
            }
        }
	}

    public void SetTargetGameState(string gameStateResource)
    {
        m_targetGameStateResource = gameStateResource;
    }
}
