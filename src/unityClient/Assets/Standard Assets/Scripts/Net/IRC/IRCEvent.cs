using UnityEngine;
using System;
using System.Collections;


/* Ported from as3irclib
 * Copyright the original author or authors.
 * 
 * Licensed under the MOZILLA PUBLIC LICENSE, Version 1.1 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.mozilla.org/MPL/MPL-1.1.html
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
public class IRCEvent 
{
	//-- socket events
	public const string EVENT_SOCKCLOSE = "onSockClose";
	public const string EVENT_SOCKOPEN = "onSockOpen";
	public const string EVENT_SOCKERROR = "onSockError";
		
	//-- msg events
	public const string EVENT_STATUSMESSAGE = "onStatusMessage";
	public const string EVENT_ACTIVEMESSAGE = "onActiveMessage";

	//-- irc events
	public const string EVENT_WELCOME = "onWelcome";
	public const string EVENT_PING = "onPing";		
	public const string EVENT_ERROR = "onIRCError";
	public const string EVENT_MOTD = "onMotd";
	public const string EVENT_NOTICE = "onNotice";
	public const string EVENT_JOIN = "onJoin";
	public const string EVENT_PART = "onPart";
	public const string EVENT_PRIVMSG = "onText";
	public const string EVENT_TOPIC = "onTopic";
	public const string EVENT_MODE = "onRawMode";
	public const string EVENT_NICK = "onNick";
	public const string EVENT_KICK = "onKick";
	public const string EVENT_QUIT = "onQuit";

    public string EventType { get; private set; }

    public IRCEvent(string type)
    {
        this.EventType = type;
    }
}

public class IRCErrorEvent : IRCEvent
{
	public string Message { get; set; }
		
	public IRCErrorEvent()
        : base(IRCEvent.EVENT_ERROR)
	{
		Message = "";
	}
}

public class IRCJoinEvent : IRCEvent
{
	public string Channel { get; set; }
	public IRCUser User { get; set; }
	public bool OnJoin { get; set; }
		
	public IRCJoinEvent() 
        : base(IRCEvent.EVENT_JOIN)
	{
        Channel = "";
        User = null;
		OnJoin = true;
	}
}

public class IRCKickEvent : IRCEvent
{
    public IRCUser Agressor { get; set; }
    public IRCUser Victim { get; set; }
    public string Channel { get; set; }
    public string Reason { get; set; }
		
	public IRCKickEvent() 
        : base(IRCEvent.EVENT_KICK)
	{
        Agressor = null;
        Victim = null;
        Channel = "";
		Reason = "";
	}
}

public class IRCModeEvent : IRCEvent
{
	public string Channel;
	public string Mode;
	public IRCUser User;

    public IRCModeEvent() 
        : base(IRCEvent.EVENT_MODE)
	{
        Channel = "";
        Mode = "";
        User = null;
	}
}

public class IRCMotdEvent : IRCEvent
{
	public string Message { get; set; }

    public IRCMotdEvent() 
        : base(IRCEvent.EVENT_MOTD)
	{
        Message = "";
	}
}

public class IRCNickEvent : IRCEvent
{
    public IRCUser OldUser { get; set; }
    public IRCUser NewUser { get; set; }
		
	public IRCNickEvent() 
        : base(IRCEvent.EVENT_NICK)
	{
        OldUser = null;
        NewUser = null;
	}
}

public class IRCNoticeEvent : IRCEvent
{
	public string Text { get; set; }
	public IRCUser User { get; set; }
		
	public IRCNoticeEvent() 
        : base(IRCEvent.EVENT_NOTICE)
	{
        Text = "";
        User = null;
	}
}

public class IRCPartEvent : IRCEvent
{
	public string Channel { get; set; }
	public IRCUser User { get; set; }
	public string Message { get; set; }
	public bool OnPart { get; set; }
		
	public IRCPartEvent() 
        : base(IRCEvent.EVENT_PART)
	{
        Channel= "";
        User = null;
        Message = "";
		OnPart = true;
	}
}

public class IRCPrivmsgEvent : IRCEvent
{
	public string Channel { get; set; }
	public IRCUser User { get; set; }
	public string Message { get; set; }

    public IRCPrivmsgEvent()
        : base(IRCEvent.EVENT_PRIVMSG)
	{
        Channel = "";
        User = null;
        Message = "";
	}
}

public class IRCQuitEvent : IRCEvent
{
	public IRCUser User { get; set; }
	public String Message { get; set; }
		
	public IRCQuitEvent()
        : base(IRCEvent.EVENT_QUIT)
	{
        User= null;
		Message= "";
	}
}

public class IRCTopicEvent : IRCEvent
{
    public enum eModeType
    {
        invalid,
        set_by,
        no_topic,
        topic
    }

    public string Channel;
    public string Message;
    public eModeType Mode;

    public IRCTopicEvent()
        : base(IRCEvent.EVENT_TOPIC)
    {
        Channel = "";
        Message = "";
        Mode = eModeType.invalid;
    }
}