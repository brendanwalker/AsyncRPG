using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

[System.Serializable]
public class CreateAccountView
{
    public Texture panelTexture;
    public float panelWidth = 200;
    public float panelHeight = 200;
    public float borderWidth = 9;
    public float colomnWidth = 108;

    private string m_username = "";
    private string m_password = "";
    private string m_passwordRepeat = "";
    private string m_emailAddress = "";

    public CreateAccountController createAccountController { get; set; }

    public string GetUserName()
    {
        return m_username;
    }

    public string GetPassword()
    {
        return m_password;
    }

    public bool DoPasswordsMatch()
    {
        return m_password == m_passwordRepeat;
    }

    public string GetEmailAddress()
    {
        return m_emailAddress;
    }

    public void Start()
    {
    }

    public void OnGUI()
    {
        GUI.BeginGroup(
            new Rect(
                Screen.width / 2 - panelWidth / 2,
                Screen.height / 2 - panelHeight / 2,
                panelWidth, panelHeight),
                panelTexture);
        GUILayout.BeginArea(
            new Rect(
                borderWidth,
                borderWidth,
                panelWidth - (2.0f * borderWidth),
                panelHeight - (2.0f * borderWidth)));
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("Username:", GUILayout.Width(colomnWidth));
                        GUILayout.Label("Password:", GUILayout.Width(colomnWidth));
                        GUILayout.Label("Verify Password:", GUILayout.Width(colomnWidth));
                        GUILayout.Label("E-mail:", GUILayout.Width(colomnWidth));
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        m_username = GUILayout.TextField(m_username, 8, GUILayout.Width(colomnWidth));
                        m_password = GUILayout.PasswordField(m_password, '*', 8, GUILayout.Width(colomnWidth));
                        m_passwordRepeat = GUILayout.PasswordField(m_passwordRepeat, '*', 8, GUILayout.Width(colomnWidth));
                        m_emailAddress = GUILayout.TextField(m_emailAddress, 128, GUILayout.Width(colomnWidth));
                        m_emailAddress = Regex.Replace(m_emailAddress, @"[^a-zA-Z0-9.@\!\#\$%&\*\+\\\-]", "");
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                {
                    if (createAccountController.IsReadyForInput)
                    {
                        if (!createAccountController.Model.IsCreateAccountRequestPending)
                        {
                            if (GUILayout.Button("Create", GUILayout.Width(colomnWidth)))
                            {
                                createAccountController.OnCreateAccountClicked();
                            }
                        }

                        if (GUILayout.Button("Cancel", GUILayout.Width(colomnWidth)))
                        {
                            createAccountController.OnCancelClicked();
                        }
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(createAccountController.Status);
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndArea();
        GUI.EndGroup();
    }

}
