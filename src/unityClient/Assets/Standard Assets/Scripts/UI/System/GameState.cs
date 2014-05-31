using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour 
{
    public string StateName { get; set; }

	public virtual void Start () 
    {	
	}

    public virtual void OnDestroy()
    {
    }

    public virtual void Update() 
    {
	
	}

    public virtual void OnGUI()
    {
        // All game UI is on depth >= 1
        GUI.depth = 1;
    }
}
