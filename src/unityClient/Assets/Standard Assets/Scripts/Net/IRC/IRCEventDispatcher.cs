using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class IRCEventDispatcher 
{
    public delegate void IRCEventDelegate(IRCEvent ircEvent);
    public delegate string PostProcessChatDelegate(string chatText);

    private Dictionary<string, IRCEventDelegate> m_ircEventHandlers;

	private IRCSocket m_socket;
	private string m_userName;
	private string m_nickName;		
	private string m_fullIdentity;
	private bool m_invisibleUser;
		
	private string m_lastErrorMessage;
	private string m_lastStatusMessage;
	private string m_lastActiveMessage;

    public IRCEventDispatcher()
    {
        m_ircEventHandlers = new Dictionary<string, IRCEventDelegate>();

        m_socket = new IRCSocket();

        m_socket.OnSocketClose = OnSocketClose;
        m_socket.OnSocketError = OnSocketError;
        m_socket.OnSocketOpen = OnSocketOpen;
        m_socket.OnSocketRead = OnSocketRead;

        m_userName = "";
        m_nickName = "";
        m_fullIdentity = "";
        m_invisibleUser = false;

        m_lastErrorMessage = "";
        m_lastStatusMessage = "";
        m_lastActiveMessage = "";
    }

    public void AddEventListener(string ircEventType, IRCEventDelegate eventDelegate)
    {
        m_ircEventHandlers.Add(ircEventType, eventDelegate);
    }

	public bool IsConnected
	{
        get { return m_socket.SocketReady; }
	}

	public string GetLastError()
    {			
		return m_lastErrorMessage;
	}
		
	// @TODO Store the last message in the first ONSTATUS
	public string GetLastStatusMessage()
    {
		return m_lastStatusMessage;
	}
		
	// @TODO Store the last message in the first ONACTIVE
	public string GetLastActiveMessage()
    {
		return m_lastActiveMessage;
	}

	public void Connect( 
	    string server, 
	    uint port, 
	    string nick, 
	    string user)
	{
		Connect (server, port, nick, user, "Default", false);
	}

	public void Connect( 
	    string server, 
	    uint port, 
	    string nick, 
	    string user, 
	    string ident)
	{
		Connect (server, port, nick, user, ident, false);
	}

	public void Connect( 
        string server, 
		uint port, 
		string nick, 
		string user, 
		string ident, 
		bool invisible)
	{
        m_userName = user;
        m_nickName = nick;
        m_fullIdentity = ident;
        m_invisibleUser = invisible;

        // Calls back into OnSocketOpen immediately
		m_socket.Connect( server, (int)port );
	}
		
	public void Disconnect(string mesg)
	{
		m_socket.SendString("QUIT :"+mesg);
		m_socket.Disconnect();
	}

    public void Update()
    {
        m_socket.Update();
    }

    // Outbound Chat or a Console Command
	public void OnInput( IRCPrivmsgEvent privmsgEvent, PostProcessChatDelegate postProcessChat)
	{
		int n = privmsgEvent.Message.IndexOf( "/" );
		int n2 = privmsgEvent.Message.IndexOf( "me", 0 ); //no action?
			
		if ( n == 0 && n2 != 1 ) // this is a little hack because /me is not handled like a command
		{
			try
			{
				ProcessInput( privmsgEvent.Message );
			}
			catch ( Exception e )
			{
				m_lastActiveMessage = e.Message;
				DispatchEvent( new IRCEvent( IRCEvent.EVENT_ACTIVEMESSAGE ) );
			}
		}
		else
		{
			IRCPrivmsgEvent newPrivmsgEvent = new IRCPrivmsgEvent();
			string str= ""; 
			string finalMesg= "";
				
			if ( n2 != 1 )
			{
                // message
				finalMesg = (postProcessChat != null) ? postProcessChat(privmsgEvent.Message) : privmsgEvent.Message;
				str = "PRIVMSG " + privmsgEvent.Channel + " :" + finalMesg;
				newPrivmsgEvent.Message = finalMesg;				
			}
			else
			{ 
                // action
				string sourceMesg = privmsgEvent.Message.Substring( 4 );
					
				str = "PRIVMSG " + privmsgEvent.Channel + " :\u0001ACTION " + sourceMesg + "\u0001";
				newPrivmsgEvent.Message = "\u0001ACTION " + sourceMesg + "\u0001";
			}
				
			m_socket.SendString( str );
				
			newPrivmsgEvent.Channel = privmsgEvent.Channel;
				
			newPrivmsgEvent.User = new IRCUser();				
			newPrivmsgEvent.User.Nick = m_nickName;
			newPrivmsgEvent.User.Ident = "";
            newPrivmsgEvent.User.Host = "";
				
			DispatchEvent( newPrivmsgEvent );
		}
	}

	public void ProcessInput(string str)
    {
		// remove the slash and the split-er control tokens
		string[] tokens = str.Substring(1).Split(new char[] {' '});
		string cmd = tokens[ 0 ].ToLower();
			
		switch ( cmd )
        {
			case "quit":
			case "notice":
			case "join":
				m_socket.SendString( str.Substring( 1 ) );
			break;
				
			case "list":
				throw new Exception( "\"/" + cmd + "\" has been disabled." );
				
			case "nick":
				m_socket.SendString( "NICK :" + tokens[ 1 ] );
			break;
				
			default:
				m_socket.SendString( str.Substring( 1 ) );
			break;
		}		
	}

    // -- Socket Event Handling --

    private void OnSocketOpen()
    {
		string userMode = m_invisibleUser ? "8" : "0";
			
		// Initiate IRC Log-on handshake
		m_socket.SendString( "NICK " + m_nickName );
		m_socket.SendString( "USER " + m_userName + " "+userMode+" * :" + m_fullIdentity );

        DispatchEvent(new IRCEvent(IRCEvent.EVENT_SOCKOPEN));
    }

    private void OnSocketClose()
    {
        DispatchEvent(new IRCEvent(IRCEvent.EVENT_SOCKCLOSE));
    }

    private void OnSocketError(Exception error)
    {
        Debug.LogException(error);

        m_lastErrorMessage = error.Message;
        DispatchEvent(new IRCEvent(IRCEvent.EVENT_SOCKERROR));
    }

    private void OnSocketRead(string input)
    {
		if ( input.Length == 0 ) 
            return;        

		// Separate the command tokens
		string[] tokens = input.Split( new char[] {' '} );			
		string command = tokens[0].ToLower();

		if ( command == IRCCommands.PING )
		{
			m_socket.SendString( "PONG " + tokens[ 1 ] );
			DispatchEvent( new IRCEvent( IRCEvent.EVENT_PING ) );
			return;
		}
        else if (command == IRCCommands.ERROR)
        {
            m_socket.Disconnect();
            m_lastErrorMessage = string.Join(" ", tokens, 1, tokens.Length - 1).Substring(1);
            DispatchEvent(new IRCEvent(IRCEvent.EVENT_SOCKERROR));
            return;
        }
        else
        {
            // Extract the message from ":nick!ident@host event params :message"
            Regex regex = new Regex(@"\:(.+?)\:(.+)");
            MatchCollection m = regex.Matches(input);
            string message = (m.Count >= 2) ? m[2].Value : "";

            // Translation Event
            string eventType = tokens[1].ToLower();

            switch (eventType)
            {
                case IRCCommands.RPL_WELCOME:
                    {
                        OnIRCReplyWelcomeEvent(input);
                    }
                    break;

                case IRCCommands.RPL_YOURHOST:
                case IRCCommands.RPL_CREATED:
                case IRCCommands.RPL_MYINFO:
                case IRCCommands.RPL_BOUNCE:
                case IRCCommands.RPL_LUSERCLIENT:
                case IRCCommands.RPL_LUSEROP:
                case IRCCommands.RPL_LUSERUNKNOWN:
                case IRCCommands.RPL_LUSERCHANNELS:
                case IRCCommands.RPL_LUSERME:
                    {
                        OnIrcReplyStatusEvent(input);
                    }
                    break;

                //-- The following errors are displayed in the main window
                case IRCCommands.ERR_NOSUCHNICK:
                case IRCCommands.ERR_NOSUCHSERVER:
                case IRCCommands.ERR_NOSUCHCHANNEL:
                case IRCCommands.ERR_TOOMANYCHANNELS:
                case IRCCommands.ERR_TOOMANYTARGETS:
                case IRCCommands.ERR_NOSUCHSERVICE:
                case IRCCommands.ERR_NOORIGIN:
                case IRCCommands.ERR_NORECIPIENT:
                case IRCCommands.ERR_NOTEXTTOSEND:
                case IRCCommands.ERR_NOTOPLEVEL:
                case IRCCommands.ERR_WILDTOPLEVEL:
                case IRCCommands.ERR_BADMASK:
                case IRCCommands.ERR_UNAVAILRESOURCE:
                case IRCCommands.ERR_NOTONCHANNEL:
                case IRCCommands.ERR_USERONCHANNEL:
                case IRCCommands.ERR_NOLOGIN:
                case IRCCommands.ERR_NOTREGISTERED:
                case IRCCommands.ERR_NEEDMOREPARAMS:
                case IRCCommands.ERR_ALREADYREGISTRED:
                case IRCCommands.ERR_PASSWDMISMATCH:
                case IRCCommands.ERR_YOUREBANNEDCREEP:
                case IRCCommands.ERR_YOUWILLBEBANNED:
                case IRCCommands.ERR_KEYSET:
                case IRCCommands.ERR_NOPRIVILEGES:
                case IRCCommands.ERR_CHANOPRIVSNEEDED:
                case IRCCommands.ERR_UMODEUNKNOWNFLAG:
                case IRCCommands.ERR_USERSDONTMATCH:
                case IRCCommands.ERR_ERRONEUSNICKNAME:
                case IRCCommands.ERR_NICKNAMEINUSE:
                case IRCCommands.ERR_NOADMININFO:
                case IRCCommands.ERR_NOMOTD:
                case IRCCommands.ERR_NONICKNAMEGIVEN:
                case IRCCommands.ERR_UNKNOWNCOMMAND:
                case IRCCommands.ERR_WASNOSUCHNICK:
                case IRCCommands.ERR_USERNOTINCHANNEL:
                    {
                        OnIRCErrorEvent(tokens, message);
                    }
                    break;

                //-- Error Messages
                case IRCCommands.ERR_CANNOTSENDTOCHAN:
                case IRCCommands.ERR_CHANNELISFULL:
                case IRCCommands.ERR_UNKNOWNMODE:
                case IRCCommands.ERR_INVITEONLYCHAN:
                case IRCCommands.ERR_BANNEDFROMCHAN:
                case IRCCommands.ERR_BADCHANNELKEY:
                case IRCCommands.ERR_BADCHANMASK:
                case IRCCommands.ERR_NOCHANMODES:
                case IRCCommands.ERR_BANLISTFULL:
                    {
                        OnIRCActiveMessageEvent(message);
                    }
                    break;

                case IRCCommands.RPL_MOTDSTART:
                case IRCCommands.RPL_MOTD:
                case IRCCommands.RPL_ENDOFMOTD:
                    {
                        OnIRCReplyMOTD(message);
                    }
                    break;

                case IRCCommands.RPL_NAMREPLY:
                    {
                        OnIRCReplyUserList(tokens, message);
                    }
                    break;

                case IRCCommands.RPL_ENDOFNAMES:
                    {
                        m_lastStatusMessage = message;
                        DispatchEvent(new IRCEvent(IRCEvent.EVENT_STATUSMESSAGE));
                    }
                    break;


                case IRCCommands.RPL_TOPIC:
                    {
                        OnIRCReplyTopic(tokens, message);
                    }
                    break;

                case IRCCommands.RPL_TOPICSETBY:                    
                    {
                        OnIRCReplyTopicSetBy(tokens);
                    }
                    break;

                case IRCCommands.RPL_NOTOPIC:
                    {
                        OnIRCReplyNoTopic(tokens, message);
                    }
                    break;

                case IRCCommands.NOTICE:
                    {
                        OnIRCNotice(tokens, message);
                    }
                    break;

                case IRCCommands.RPL_CHANNELCREATED:
                    {
                        // TODO: Add channel created timestamp parsing code
                        // irc.server.com 329 nick #chan 113975236
                    }
                    break;

                case IRCCommands.RPL_CHANNELMODES:
                    {
                        OnIRCReplyChannelModes(tokens);
                    }
                    break;

                case IRCCommands.JOIN:
                    {
                        OnIRCUserJoin(tokens);
                    }
                    break;

                case IRCCommands.PART:
                    {
                        OnIRCUserPart(tokens, message);
                    }
                    break;


                case IRCCommands.INVITE:
                    {
                        OnIRCInvite(tokens);
                    }
                    break;

                case IRCCommands.TOPIC:
                    {
                        OnIRCTopic(tokens, message);
                    }
                    break;

                // Private message
                case IRCCommands.PRIVMSG:
                    {
                        OnIRCPrivateMessage(tokens, message);
                    }
                    break;

                // Changing user nick-name
                case IRCCommands.NICK:
                    {
                        OnIRCUserNickChanged(tokens, message);
                    }
                    break;

                case IRCCommands.MODE:
                    {
                        OnIRCChannelMode(tokens);
                    }
                    break;

                case IRCCommands.KICK:
                    {
                        OnIRCUserKicked(tokens, message);
                    }
                    break;

                case IRCCommands.QUIT:
                    {
                        OnIRCQuit(tokens, message);
                    }
                    break;

                default:
                    {
                        m_lastStatusMessage = input;
                        DispatchEvent(new IRCEvent(IRCEvent.EVENT_STATUSMESSAGE));
                    }
                    break;
            }
        }			
    }

    private void OnIRCReplyWelcomeEvent(string input)
    {
        string[] customTokens = input.Split(new char[] { ':' });
        m_lastStatusMessage = customTokens[2];

        DispatchEvent(new IRCEvent(IRCEvent.EVENT_WELCOME));
    }

    private void OnIrcReplyStatusEvent(string input)
    {
        string[] customTokens = input.Split( new char[] {':'} );

        if (customTokens.Length >= 3)
        {
            m_lastStatusMessage = customTokens[2];
        }
        else if (customTokens.Length == 2)
        {
            m_lastStatusMessage = customTokens[1];
        }
        else
        {
            m_lastStatusMessage = input;
        }
					
        DispatchEvent( new IRCEvent( IRCEvent.EVENT_STATUSMESSAGE ) );
    }

    private void OnIRCErrorEvent(string[] tokens, string message)
    {
        IRCErrorEvent err = new IRCErrorEvent();

        err.Message = message;
					
        //-- Display attribute error if the format is ":ADDRESS DATA NICK (ATTRIBUTE) :MESSAGE"
        string attr = tokens[ 3 ];
        if ( attr.Substring( 0, 1 ) != ":" )
        {
            err.Message += " \"" + attr + "\"";
        }
						
        DispatchEvent( err );
    }

    private void OnIRCActiveMessageEvent(string message)
    {
        m_lastActiveMessage = message;
        DispatchEvent( new IRCEvent( IRCEvent.EVENT_ACTIVEMESSAGE ) );
    }

    private void OnIRCReplyMOTD(string message)
    {
        IRCMotdEvent motd = new IRCMotdEvent();
        motd.Message = message;

        // Shouldn't this be up to the client code?
        //if ( event == IRCCommands.RPL_ENDOFMOTD )
        //	processInput( '/join #irc' );

        DispatchEvent(motd);
    }

    private void OnIRCReplyUserList(string[] tokens, string message)
    {
        m_lastStatusMessage = message;

        string[] list = message.Split(new char[] { ' ' });
        string target = tokens[4];
        int i = list.Length;

        while (--i > -1)
        {
            if (list[i] == "" || list[i] == " " || list[i] == null)
                continue;

            IRCJoinEvent name = new IRCJoinEvent();
            IRCUser user = new IRCUser();

            user.Nick = list[i];
            name.User = user;
            name.Channel = target;
            name.OnJoin = false;

            DispatchEvent(name);
        }

        DispatchEvent(new IRCEvent(IRCEvent.EVENT_STATUSMESSAGE));
    }

    private void OnIRCReplyTopic(string[] tokens, string message)
    {
        IRCTopicEvent topic = new IRCTopicEvent();

        topic.Mode = IRCTopicEvent.eModeType.topic;
        topic.Channel = tokens[3];
        topic.Message = message;

        DispatchEvent(topic);
    }

    private void OnIRCReplyTopicSetBy(string[] tokens)
    {
        IRCTopicEvent topic = new IRCTopicEvent();

        topic.Mode = IRCTopicEvent.eModeType.set_by;
        topic.Channel = tokens[3];
        topic.Message = tokens[4] + ' ' + tokens[5];

        DispatchEvent(topic);
    }

    private void OnIRCReplyNoTopic(string[] tokens, string message)
    {
        IRCTopicEvent topic = new IRCTopicEvent();

        topic.Mode = IRCTopicEvent.eModeType.no_topic;
        topic.Channel = tokens[3];
        topic.Message = message;

        DispatchEvent(topic);
    }

    private void OnIRCNotice(string[] tokens, string message)
    {
        IRCNoticeEvent notice = new IRCNoticeEvent();

        notice.Text = message;
        notice.User = IRCUser.GetUserFromAddress(tokens[0].Substring(1));

        DispatchEvent(notice);
    }

    private void OnIRCReplyChannelModes(string[] tokens)
    {
        IRCModeEvent mode = new IRCModeEvent();
        mode.Channel = tokens[3];
        mode.Mode = tokens[4];

        for (int tokenIndex = 5; tokenIndex < tokens.Length; tokenIndex++)
        {
            mode.Mode += ' ' + tokens[tokenIndex];
        }

        DispatchEvent(mode);
    }

    private void OnIRCUserJoin(string[] tokens)
    {
        IRCJoinEvent join = new IRCJoinEvent();

        join.User = IRCUser.GetUserFromAddress(tokens[0].Substring(1));
        join.Channel = tokens[2];

        if (join.User.Nick == m_nickName)
        {
            m_socket.SendString("MODE " + join.Channel);
        }

        DispatchEvent(join);
    }

    private void OnIRCUserPart(string[] tokens, string message)
    {
        IRCPartEvent part = new IRCPartEvent();

        part.User = IRCUser.GetUserFromAddress(tokens[0].Substring(1));
        part.Channel = tokens[2];
        part.Message = message;

        DispatchEvent(part);
    }

    private void OnIRCInvite(string[] tokens)
    {
        // We can manage this command with an event as the user may join the channel automatically
        IRCUser user = IRCUser.GetUserFromAddress(tokens[0].Substring(1));

        m_lastStatusMessage = user.Nick + " invited you to join " + tokens[3].Substring(1);
        DispatchEvent(new IRCEvent(IRCEvent.EVENT_STATUSMESSAGE));
    }

    private void OnIRCTopic(string[] tokens, string message)
    {
        IRCUser user = IRCUser.GetUserFromAddress(tokens[0].Substring(1));
        IRCTopicEvent topic = new IRCTopicEvent();
        IRCTopicEvent topicBy = new IRCTopicEvent();
        DateTime time = DateTime.Now;

        topic.Mode = IRCTopicEvent.eModeType.topic;
        topicBy.Mode = IRCTopicEvent.eModeType.set_by;

        topic.Channel = topicBy.Channel = tokens[2];

        topic.Message = message;
        topicBy.Message = user.Nick + " " + (time.Ticks / 1000).ToString();

        DispatchEvent(topic);
        DispatchEvent(topicBy);
    }

    private void OnIRCPrivateMessage(string[] tokens, string message)
    {
        IRCPrivmsgEvent pmsg = new IRCPrivmsgEvent();

        pmsg.Channel = tokens[2];
        pmsg.User = IRCUser.GetUserFromAddress(tokens[0].Substring(1));
        pmsg.Message = message;

        DispatchEvent(pmsg);
    }

    private void OnIRCUserNickChanged(string[] tokens, string message)
    {
        IRCUser oldUser = IRCUser.GetUserFromAddress(tokens[0].Substring(1));

        if (oldUser.Nick == m_nickName)
        {
            m_nickName = message;
        }

        {
            IRCNickEvent nick = new IRCNickEvent();

            nick.OldUser = oldUser;
            nick.NewUser = new IRCUser();

            nick.NewUser.Nick = message;
            nick.NewUser.Host = oldUser.Host;
            nick.NewUser.Ident = oldUser.Ident;

            DispatchEvent(nick);
        }
    }

    private void OnIRCChannelMode(string[] tokens)
    {
        IRCModeEvent modeEvent = new IRCModeEvent();

        modeEvent.Channel = tokens[2];
        modeEvent.Mode = tokens[3];
        modeEvent.User = IRCUser.GetUserFromAddress(tokens[0].Substring(1));

        for (int tokenIndex = 4; tokenIndex < tokens.Length; tokenIndex++)
        {
            modeEvent.Mode += ' ' + tokens[tokenIndex];
        }

        DispatchEvent(modeEvent);
    }

    private void OnIRCUserKicked(string[] tokens, string message)
    {
        IRCKickEvent kick = new IRCKickEvent();

        kick.Agressor = IRCUser.GetUserFromAddress(tokens[0].Substring(1));
        kick.Channel = tokens[2];
        kick.Victim = new IRCUser();
        kick.Victim.Nick = tokens[3];
        kick.Reason = (message == tokens[3]) ? "" : message;

        DispatchEvent(kick);
    }

    private void OnIRCQuit(string[] tokens, string message)
    {
        IRCQuitEvent quit = new IRCQuitEvent();

        quit.User = IRCUser.GetUserFromAddress(tokens[0].Substring(1));
        quit.Message = message;

        DispatchEvent(quit);
    }

    private void DispatchEvent(IRCEvent ircEvent)
    {
        IRCEventDelegate eventDelegate= null;

        if (m_ircEventHandlers.TryGetValue(ircEvent.EventType, out eventDelegate))
        {
            eventDelegate(ircEvent);
        }
    }
}
