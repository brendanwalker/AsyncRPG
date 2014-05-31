using UnityEngine;
using System.Collections;
using System;

public class Fader : MonoBehaviour 
{
    public enum eFadeType
    {
        fadeIn,
        fadeOut,
    }

    public delegate void OnFadeComplete();

    // Fade Runtime
    private Texture2D m_texture= null;
    private float m_startTime = 0.0f; // seconds
    private float m_alpha = 1.0f;
    
    // Fade Parameters
    private float m_duration = 0.0f; // seconds    
    private Color m_fadeColor = Color.black;
    private eFadeType m_fadeType = eFadeType.fadeOut;
    private OnFadeComplete m_completeHandler;

	void Start () 
    {
        m_startTime = Time.time;

        m_texture = new Texture2D(1, 1);
        m_texture.SetPixel(0, 0, Color.white);
	}

    void OnGUI()
    {
        GUI.depth = 0;
        GUI.color = new Color(m_fadeColor.r, m_fadeColor.g, m_fadeColor.b, m_alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_texture, ScaleMode.StretchToFill, true);
        GUI.color = Color.white;
    }

	void Update () 
    {
        float t= Math.Max(Math.Min((Time.time - m_startTime) / m_duration, 1.0f), 0.0f);

        switch (m_fadeType)
        {
            case eFadeType.fadeIn:
                m_alpha = 1.0f - t;
                break;
            case eFadeType.fadeOut:
                m_alpha = t;
                break;
        }        

        if (t >= 1.0f)
        {
            if (m_completeHandler != null)
            {
                m_completeHandler();
            }

            GameObject.Destroy(this);
        }
	}

    public static void AttachFaderTo(
        GameObject gameObject,
        Color fadeColor,
        eFadeType fadeType,
        float duration, 
        OnFadeComplete callback)
    {
        Fader fader= gameObject.AddComponent<Fader>();

        fader.m_duration = duration;
        fader.m_fadeColor = fadeColor;
        fader.m_fadeType = fadeType;
        fader.m_completeHandler = callback;
    }
}
