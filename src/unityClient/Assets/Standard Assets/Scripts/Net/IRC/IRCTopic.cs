using UnityEngine;
using System.Collections;

public class IRCTopic
{
	public string Title { get; set; }
	public string SetBy { get; set; }
	public string SetOn { get; set; }
		
	public IRCTopic()
	{
		Title = SetBy = SetOn = "";
	}
}