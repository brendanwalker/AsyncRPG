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

﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class IRCMode
{
	public bool OnlyOpsSetTopic { get; private set; }
	public bool NoExternalMessages { get; private set; }
	public bool InviteOnly { get; private set; }
	public bool Moderated { get; private set; }
	public bool HasKey { get; private set; }
	public string KeyValue { get; private set; }
	public bool HasLimit { get; private set; }
	public int LimitValue { get; private set; }
	public bool IsPrivate { get; private set; }
	public bool IsSecret { get; private set; }
		
	private bool ModeSet;
		
	public List<char> Op { get; private set; }
	public List<char> Voice { get; private set; }
		
	public List<char> DeOp { get; private set; }
	public List<char> DeVoice { get; private set; }
		
	public IRCMode()
	{
		OnlyOpsSetTopic = false;
		NoExternalMessages = false;
		InviteOnly = false;
		Moderated = false;
		HasKey = false;
		KeyValue = "";
		HasLimit = false;
		LimitValue = 0;
		IsPrivate = false;
		IsSecret = false;
		ModeSet = false;
	}
		
	public void ProcessString(string mode)
	{					
		string[] tokens = mode.Split( ' ' );
		int tokenId = 1;
		bool addMode= false;
		char chr= '\0';
			
		int scan_index = 0;
		int end_scan_index = tokens[ 0 ].Length;
	
		ModeSet = true;
			
		Op = new List<char>();
        DeOp = new List<char>();
        Voice = new List<char>();
        DeVoice = new List<char>();
		
		while ( scan_index < end_scan_index )
		{
			chr = tokens[ 0 ][scan_index];
				
			if ( chr == '+' )
            {
				addMode = true;
            }
			else if ( chr == '-' )
            {
				addMode = false;			
            }
			else if ( chr == 't' )
            {
				OnlyOpsSetTopic = addMode;
            }
			else if ( chr == 'n' )
            {
				NoExternalMessages = addMode;
            }
			else if ( chr == 'i' )
            {
				InviteOnly = addMode;
            }
			else if ( chr == 'm' )
            {
				Moderated = addMode;
            }
			else if ( chr == 'k' )
			{
				if ( addMode )
				{
					HasKey = true;
					KeyValue = tokens[ tokenId ];
						
					tokenId++;
				}
				else
				{
					HasKey = false;
					KeyValue = "";
				}
			}
			else if ( chr == 'l' )
			{
				if ( addMode )
				{
                    int intValue = 0;

                    if (int.TryParse(tokens[tokenId], out intValue))
                    {
                        HasLimit = true;
                        LimitValue = intValue; 
                    }
						
					tokenId++;
				}
				else
				{
					HasLimit = false;
					LimitValue = 0;
				}
			}
			else if ( chr == 'p' )
            {
				IsPrivate = addMode;
            }
			else if ( chr == 's' )
            {
				IsSecret = addMode;
            }
			else if ( chr == 'o' )
			{
				if ( addMode )
                {
					Op.Add(tokens[tokenId][0]);
                }
				else
                {
                    DeOp.Add(tokens[tokenId][0]);
                }
						
				tokenId++;
			}
			else if ( chr == 'v' )
			{
				if ( addMode )
                {
					Voice.Add(tokens[tokenId][0]);
                }
				else
                {
                    DeVoice.Add(tokens[tokenId][0]);
                }
			}
								
			scan_index++;
		}
	}
		
	public override string ToString()
	{
		StringBuilder modeString = new StringBuilder();

        modeString.Append('+');

		if ( !ModeSet )
        {
			modeString.Append('?');
        }
		else
        {		
		    if ( OnlyOpsSetTopic )
			    modeString.Append('t');
			
		    if ( NoExternalMessages )
			    modeString.Append('n');
			
		    if ( InviteOnly )
			    modeString.Append('i');
			
		    if ( Moderated )
			    modeString.Append('m');
			
		    if ( HasKey )
			    modeString.Append('k');
			
		    if ( HasLimit )
			    modeString.Append('l');
			
		    if ( IsPrivate )
			    modeString.Append('p');
			
		    if ( IsSecret ) 
			    modeString.Append('s');
				
		    //-- attributes are automatic in correct order, because we check HasKey first and then the HasLimit
		    if ( KeyValue.Length > 0)
			    modeString.AppendFormat(" {0}", KeyValue);
				
		    if ( LimitValue != 0 )
                modeString.AppendFormat(" {0}", LimitValue);
        }

        return modeString.ToString();
	}
		
	public bool IsEmpty
	{
        get { return ModeSet; }
	}
}