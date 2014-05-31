using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugRegistry 
{
    private static char[] COMMAND_SEPERATOR = new char[] { ' ' };
    private static char[] PATH_SEPERATOR = new char[] { '.' };

	private static DebugRegistry _instance;
	private Dictionary<string, object> _rootNode;		
		
	public DebugRegistry() 
	{
		_rootNode = new Dictionary<string, object>();
	}
		
	private static DebugRegistry GetInstance()
	{
		if (_instance == null)
		{
			_instance = new DebugRegistry();
		}
			
		return _instance;
	}
		
	public static string ExecuteDebugCommand(string commandString)
	{
		string result = "";
			
		if (commandString.IndexOf(":?") == 0 || commandString.IndexOf(":help") == 0)
		{
			result += "Command List: \n";
			result += "set - sets a debug toggle to true or false \n";
			result += "get - get the current value of a debug toggle \n";
		}
		else if (commandString.IndexOf(":set ") == 0)
		{
            string[] fragments = commandString.Substring(5).Split(COMMAND_SEPERATOR);
				
			if (fragments.Length == 2)
			{
				string setPath= fragments[0];
				string value= fragments[1].ToLower();
					
				if (value == "true" || value == "1")
				{
					SetToggle(setPath, true);
					result = "Set toggle to true";
				}
				else if (value == "false" || value == "0")
				{
					SetToggle(setPath, false);
					result = "Set toggle to false";
				}
				else 
				{
					result = ":set <path> <true/false>";
				}
			}				
			else 
			{
				result = ":set <path> <true/false>";
			}				
		}
		else if (commandString.IndexOf(":get ") == 0)
		{
			string getPath= commandString.Substring(5);
							
			if (getPath.Length > 0)
			{
				if (TestToggle(getPath))
				{
					result = getPath+" -> true";
				}
				else
				{
					result = getPath+" -> false";
				}
			}
			else 
			{
				result = ":get <path>";
			}
		}			
			
		return result;
	}
		
	public static List<string> CompleteDebugCommand(string commandString)
	{
		List<string> completions= new List<string>();
			
		if (commandString.IndexOf(":set ") == 0)
		{
			string partialSetPath= commandString.Substring(5);
			List<string> pathSetCompletions= MatchPathToken(partialSetPath);
				
			if (pathSetCompletions.Count > 1)
			{
				completions = pathSetCompletions;
			}
            else if (pathSetCompletions.Count == 1)
			{
				completions.Add(":set " + pathSetCompletions[0]);
			}
		}
		else if (commandString.IndexOf(":get ") == 0)
		{
			string partialGetPath= commandString.Substring(5);
			List<string> pathGetCompletions= MatchPathToken(partialGetPath);

            if (pathGetCompletions.Count > 1)
			{
				completions = pathGetCompletions;
			}
            else if (pathGetCompletions.Count == 1)
			{
                completions.Add(":get " + pathGetCompletions[0]);
			}
		}
			
		return completions;
	}
		
	public static void SetToggle(string pathString, bool value)
	{		
		if (pathString.Length > 0)
		{			
            Dictionary<string, object> parentNode= null;			
            string valueName= "";

			CreatePath(pathString, out parentNode, out valueName);

            if (parentNode.ContainsKey(valueName))
            {
                parentNode[valueName]= value;
            }
            else
            {
                parentNode.Add(valueName, value);
            }
		}
	}
				
	public static bool TestToggle(string pathString)
	{
		bool toggleValue= false;
			
		if (pathString.Length > 0)
		{			
            Dictionary<string, object> parentNode= null;			
            string valueName= "";

			TestPath(pathString, out parentNode, out valueName);				
				
			if (valueName.Length > 0 && parentNode != null)
			{
				if (valueName == "*")
				{
					toggleValue = TestAnyToggle(parentNode);
				}
				else 
				{					
					toggleValue = parentNode.ContainsKey(valueName) ? (bool)parentNode[valueName] : false;
				}
			}
		}
			
		return toggleValue;
	}
		
	private static bool TestAnyToggle(Dictionary<string, object> node)
	{
		bool result= false;
			
		foreach (string fieldName in node.Keys)
		{
			if (node[fieldName] is bool)
			{
				result = (bool)node[fieldName];
			}
			else
			{
				// Must be another dictionary
                result = TestAnyToggle((Dictionary<string, object>)node[fieldName]);
			}
				
			if (result == true)
			{
				break;
			}
		}
			
		return result;
	}
		
	private static List<string> MatchPathToken(string partialPath)
	{
		List<string> completedTokens= new List<string>();

        Dictionary<string, object> parentNode= null;			
        string valueName= "";

		TestPath(partialPath, out parentNode, out valueName);				
			
		if (valueName.Length > 0 && parentNode != null)
		{
			string[] tokens= partialPath.Split(PATH_SEPERATOR);
			string lastToken= (tokens.Length > 0) ? tokens[tokens.Length - 1] : partialPath;
			string path= "";
				
			for (int pathIndex= 0; pathIndex < (tokens.Length - 1); pathIndex++)
			{
				path += (tokens[pathIndex]+".");
			}
				
			foreach (string fieldName in parentNode.Keys)
			{
				if (lastToken.Length == 0 || fieldName.IndexOf(lastToken) == 0)
				{
					completedTokens.Add(path+fieldName);
				}
			}
		}
			
		return completedTokens;
	}
		
	private static void CreatePath(
        string pathString,
        out Dictionary<string, object> leafNodeParent,
        out string leafNodeToken)
	{
        string[] tokens = pathString.Split(PATH_SEPERATOR);
		string lastToken= (tokens.Length > 0) ? tokens[tokens.Length - 1] : pathString;

		DebugRegistry registry= GetInstance();
		Dictionary<string, object> parentNode= registry._rootNode;
			
		// Create any missing nodes along the path
		for (int tokenIndex= 0; tokenIndex < tokens.Length - 1; ++tokenIndex)
		{
			string token= tokens[tokenIndex];
				
			if (!parentNode.ContainsKey(token))
			{
				parentNode.Add(token, new Dictionary<string, object>());
			}
				
			parentNode = (Dictionary<string, object>)parentNode[token];
		}
		
        leafNodeParent= parentNode;
        leafNodeToken= lastToken;
	}
		
	private static void TestPath(
        string pathString,
        out Dictionary<string, object> leafNodeParent,
        out string leafNodeToken)
	{
        string[] tokens = pathString.Split(PATH_SEPERATOR);
		string lastToken= (tokens.Length > 0) ? tokens[tokens.Length - 1] : pathString;

		DebugRegistry registry= GetInstance();
		Dictionary<string, object> parentNode= registry._rootNode;
			
		// Make sure the path to the node exists
		for (int tokenIndex= 0; parentNode != null && tokenIndex < tokens.Length - 1; ++tokenIndex)
		{
			string token= tokens[tokenIndex];
				
			if (parentNode.ContainsKey(token))
			{
                parentNode = (Dictionary<string, object>)parentNode[token];
			}
			else
			{
				parentNode = null;
			}
		}			
			
        leafNodeParent= parentNode;
        leafNodeToken= lastToken;
	}		
}