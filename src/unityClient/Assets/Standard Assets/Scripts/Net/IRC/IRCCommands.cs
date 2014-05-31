using UnityEngine;
using System.Collections;

public class IRCCommands 
{
	//-- List of errots returned from the server 
	public const string ERR_UNKNOWNCOMMAND = "421";
	public const string ERR_NOMOTD = "422";
	public const string ERR_NOADMININFO = "423";
	public const string ERR_NONICKNAMEGIVEN = "431";
    public const string ERR_ERRONEUSNICKNAME = "432";
	public const string ERR_NICKNAMEINUSE = "433";
	public const string ERR_NOSUCHNICK = "401";
	public const string ERR_NOSUCHSERVER = "402";
	public const string ERR_NOSUCHCHANNEL = "403";
	public const string ERR_TOOMANYCHANNELS = "405";
	public const string ERR_TOOMANYTARGETS = "407";
	public const string ERR_NOSUCHSERVICE = "408";
	public const string ERR_NOORIGIN = "409";
	public const string ERR_NORECIPIENT = "411";
	public const string ERR_NOTEXTTOSEND = "412";
	public const string ERR_NOTOPLEVEL = "413";
	public const string ERR_WILDTOPLEVEL = "414";
	public const string ERR_BADMASK = "415";
	public const string ERR_UNAVAILRESOURCE = "437";
	public const string ERR_NOTONCHANNEL = "442";
	public const string ERR_USERONCHANNEL = "443";
	public const string ERR_NOLOGIN = "444";
	public const string ERR_NOTREGISTERED = "451";
	public const string ERR_NEEDMOREPARAMS = "461";
	public const string ERR_ALREADYREGISTRED = "462";
	public const string ERR_PASSWDMISMATCH = "464";
	public const string ERR_YOUREBANNEDCREEP = "465";
	public const string ERR_YOUWILLBEBANNED = "466";
	public const string ERR_KEYSET = "467";
	public const string ERR_NOPRIVILEGES = "481";
	public const string ERR_CHANOPRIVSNEEDED = "482";
	public const string ERR_UMODEUNKNOWNFLAG = "501";
	public const string ERR_USERSDONTMATCH = "502";
	public const string ERR_WASNOSUCHNICK = "406";
	public const string ERR_USERNOTINCHANNEL = "441";
	public const string ERR_CANNOTSENDTOCHAN = "404";
	public const string ERR_CHANNELISFULL = "471";
	public const string ERR_UNKNOWNMODE = "472";
	public const string ERR_INVITEONLYCHAN = "473";
	public const string ERR_BANNEDFROMCHAN = "474";
	public const string ERR_BADCHANNELKEY = "475";
	public const string ERR_BADCHANMASK = "476";
	public const string ERR_NOCHANMODES = "477";
	public const string ERR_BANLISTFULL = "478";
		
	//-- Client-Server Exchanges
	public const string RPL_WELCOME = "001";
	public const string RPL_YOURHOST = "002";
	public const string RPL_CREATED = "003";
	public const string RPL_MYINFO = "004";
	public const string RPL_BOUNCE = "005";
	public const string RPL_LUSERCLIENT = "251";
	public const string RPL_LUSEROP = "252";
	public const string RPL_LUSERUNKNOWN = "253";
	public const string RPL_LUSERCHANNELS = "254";
	public const string RPL_LUSERME = "255";
		
	//-- Message of the day
    /*
    When the server responds to the MOTD message and the MOTD file
    is found, the file is displayed line by line,
    each line must not exceed 80 characters,
    using responses RPL_MOTD format.
    They must be bounded by a RPL_MOTDSTART (before RPL_MOTDs) RPL_ENDOFMOTD and (after).
    */
    public const string RPL_MOTDSTART = "375";
	public const string RPL_MOTD = "372";
	public const string RPL_ENDOFMOTD = "376";
		
	//-- NAME
    /*
    In reply to a NAMES message, a pair consisting of RPL_NAMREPLY and RPL_ENDOFNAMES
    is returned by the server to the client.
    If there is no channel from the query, only RPL_ENDOFNAMES returned.
    The exception to this is when a NAMES message is sent without
    parameters and all visible channels and contents are sent back
    in a sequence of messages with a RPL_NAMEREPLY RPL_ENDOFNAMES indicating the end.
    */
    public const string RPL_NAMREPLY = "353";
	public const string RPL_ENDOFNAMES = "366";
                            
	//-- TOPIC
    /*
    When sending a message to determine the TOPIC about a channel,
    one of these two responses is sent. If the subject is defined,
    RPL_TOPIC is returned, otherwise RPL_NOTOPIC.
    */
    public const string RPL_NOTOPIC = "331";
	public const string RPL_TOPIC = "332";
	public const string RPL_TOPICSETBY = "333";

	//-- MODE
	public const string RPL_CHANNELCREATED = "329";
	public const string RPL_CHANNELMODES = "324";

	//-- data event
	public const string NOTICE = "notice";
	public const string MODE = "mode";
	public const string JOIN = "join";
	public const string PART = "part";
	public const string PRIVMSG = "privmsg";
	public const string PING = "ping";
	public const string ERROR = "error";
	public const string INVITE = "invite";
	public const string TOPIC = "topic";
	public const string NICK = "nick";
	public const string KICK = "kick";
	public const string QUIT = "quit";
}
