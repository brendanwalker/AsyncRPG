using UnityEngine;
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