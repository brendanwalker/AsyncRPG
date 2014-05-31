using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;

public class IRCUser
{
	public string Nick { get; set; }
	public string Ident { get; set; }
	public string Host { get; set; }
	public string Mail { get; set; }
	public string AltNick { get; set; }
	public string Description { get; set; }
	public string Password { get; set; }
	
	public IRCUser()
	{
		Nick = "";
        Ident = "";
        Host = "";
        Mail = "";
        AltNick = "";
        Description= "";
        Password= "";
	}
		
	public string GetMask()
	{
		return (Nick + '!' + Ident + '@' + Host);
	}
		
	// Convert an address string to an IRCUser
	static public IRCUser GetUserFromAddress(string address)
    {
        IRCUser user = new IRCUser();

        if (address.Length > 0)
        {
            Regex regex = new Regex(@"(.+?)\!(.+?)\@(.+)", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(address);
            bool foundMatch = false;

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 4)
                {
                    user.Nick = match.Groups[1].Value;
                    user.Ident = match.Groups[2].Value;
                    user.Host = match.Groups[3].Value;

                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                user.Nick = address;
                user.Ident = "";
                user.Host = "";
            }
        }
        else
        {
            throw new Exception("Empty User Address");
        }
			
		return user;
	}		
}