using UnityEngine;
using System.Collections;

public class IRCChannelItem 
{
	public string Name { get; private set; }
	public uint UsersCount { get; private set; }
	public string Topic { get; private set; }

	public IRCChannelItem(string name, uint size, string topic)
	{
		this.Name = name;
		this.UsersCount = size;
		this.Topic = topic;
	}
}