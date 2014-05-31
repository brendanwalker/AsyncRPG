using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AsyncRPGDBTool
{
    public abstract class CommandArgument
    {
        protected string _argumentName;
        protected string _argumentDesription;
        protected bool _argumentOptional;

        public CommandArgument(string name, string description, bool optional)
        {
            _argumentName= name;
            _argumentDesription = description;
            _argumentOptional= optional;
        }

        public string ArgumentName
        {
            get { return _argumentName; }
        }

        public string ArgumentDescription
        {
            get { return _argumentDesription; }
        }

        public bool ArgumentOptional
        {
            get { return _argumentOptional; }
        }

        public abstract CommandArgument Clone();
        public abstract bool Parse(string stringValue);
    }

    public class CommandArgument_String : CommandArgument
    {
        private string _argumentValue;

		public CommandArgument_String(string name) :
			base(name, "", false)
		{
			_argumentValue = "";
		}

		public CommandArgument_String(string name, string description) :
			base(name, description, false)
		{
			_argumentValue = "";
		}

		public CommandArgument_String(string name, string description, bool optional) :
			base(name, description, optional)
		{
			_argumentValue = "";
		}

        public CommandArgument_String(string name, string description, bool optional, string value) :
            base(name, description, optional)
        {
            _argumentValue = value;
        }

        public string ArgumentValue
        {
            get { return _argumentValue; }
        }

        public override CommandArgument Clone()
        {
            return new CommandArgument_String(_argumentName, _argumentDesription, _argumentOptional, _argumentValue);
        }

        public override bool Parse(string stringValue)
        {
            _argumentValue = stringValue;

            return true;
        }
    }

    public class CommandArgument_Int32 : CommandArgument
    {
        private int _argumentValue;

		public CommandArgument_Int32(string name) : 
			base(name, "", false)
		{
			_argumentValue = 0;
		}

		public CommandArgument_Int32(string name, string description) : 
			base(name, description, false)
		{
			_argumentValue = 0;
		}

		public CommandArgument_Int32(string name, string description, bool optional) : 
			base(name, description, optional)
		{
			_argumentValue = 0;
		}

        public CommandArgument_Int32(string name, string description, bool optional, Int32 value) : 
            base(name, description, optional)
        {
            _argumentValue = value;
        }

        public int ArgumentValue
        {
            get { return _argumentValue; }
        }

        public override CommandArgument Clone()
        {
            return new CommandArgument_Int32(_argumentName, _argumentDesription, _argumentOptional, _argumentValue);
        }

        public override bool Parse(string stringValue)
        {
            return int.TryParse(stringValue, out _argumentValue);
        }
    }

    public class CommandArgument_Bool : CommandArgument
    {
        private bool _argumentValue;

		public CommandArgument_Bool(string name) :
			base(name, "", false)
		{
			_argumentValue = false;
		}

		public CommandArgument_Bool(string name, string description) :
			base(name, description, false)
		{
			_argumentValue = false;
		}

		public CommandArgument_Bool(string name, string description, bool optional) :
			base(name, description, optional)
		{
			_argumentValue = false;
		}

        public CommandArgument_Bool(string name, string description, bool optional,  bool value) :
            base(name, description, optional)
        {
            _argumentValue = value;
        }

        public bool ArgumentValue
        {
            get { return _argumentValue; }
        }

        public override CommandArgument Clone()
        {
            return new CommandArgument_Bool(_argumentName, _argumentDesription, _argumentOptional, _argumentValue);
        }

        public override bool Parse(string stringValue)
        {
            return bool.TryParse(stringValue, out _argumentValue);
        }
    }

    public class CommandArgument_Float : CommandArgument
    {
        private float _argumentValue;

		public CommandArgument_Float(string name) :
			base(name, "", false)
		{
			_argumentValue = 0.0f;
		}

		public CommandArgument_Float(string name, string description) :
			base(name, description, false)
		{
			_argumentValue = 0.0f;
		}

		public CommandArgument_Float(string name, string description, bool optional) :
			base(name, description, optional)
		{
			_argumentValue = 0.0f;
		}

        public CommandArgument_Float(string name, string description, bool optional, float value) :
            base(name, description, optional)
        {
            _argumentValue = value;
        }

        public float ArgumentValue
        {
            get { return _argumentValue; }
        }

        public override CommandArgument Clone()
        {
            return new CommandArgument_Float(_argumentName, _argumentDesription, _argumentOptional, _argumentValue);
        }

        public override bool Parse(string stringValue)
        {
            return float.TryParse(stringValue, out _argumentValue);
        }
    }

    public class Command
    {
        public delegate bool ExecuteCommandDelegate(Command command);

        private string _commandName;
        private string _commandDescription;
        private Dictionary<String, CommandArgument> _argumentMap;
        private ExecuteCommandDelegate _commandDelegate;

		public Command(string name, ExecuteCommandDelegate commandDelegate)
		{
			_commandName = name;
			_commandDelegate = commandDelegate;
			_commandDescription = "";
			_argumentMap = new Dictionary<String, CommandArgument>();			
		}

		public Command(string name, ExecuteCommandDelegate commandDelegate, CommandArgument[] arguments)
		{
			_commandName = name;
			_commandDelegate = commandDelegate;
			_commandDescription = "";
			_argumentMap = new Dictionary<String, CommandArgument>();
			
			if (arguments != null)
			{
				foreach (CommandArgument argument in arguments)
				{
					_argumentMap.Add(argument.ArgumentName, argument);
				}
			}
		}

        public Command(string name, ExecuteCommandDelegate commandDelegate, CommandArgument[] arguments, string description)
        {
            _commandName = name;
            _commandDelegate = commandDelegate;
            _commandDescription = description;
            _argumentMap = new Dictionary<String, CommandArgument>();

            if (arguments != null)
            {
                foreach (CommandArgument argument in arguments)
                {
                    _argumentMap.Add(argument.ArgumentName, argument);
                }
            }
        }

        public string CommandName
        {
            get { return this._commandName;  }
        }

        public string CommandDescription
        {
            get { return this._commandDescription; }
        }

        public ExecuteCommandDelegate CommandDelegate
        {
            get { return this._commandDelegate; }
        }

        public Dictionary<String, CommandArgument> ArgumentMap
        {
            get { return this._argumentMap; }
        }

        public void AddArgument(CommandArgument argument)
        {
            _argumentMap.Add(argument.ArgumentName, argument);
        }

        public CommandArgument GetArgumentByName(string name)
        {
            CommandArgument argument = null;
            bool success = _argumentMap.TryGetValue(name, out argument);

            return success ? argument : null;
        }

        public T GetTypedArgumentByName<T>(string name) where T : CommandArgument
        {
            CommandArgument argument= null;
            bool success= _argumentMap.TryGetValue(name, out argument);

            return success ? (T)argument : null;
        }

        public bool HasArgumentWithName(string name)
        {
            return _argumentMap.ContainsKey(name);
        }

        public bool Execute()
        {
            return _commandDelegate(this);
        }
    }

    public class CommandLineParser
    {
        private Dictionary<String, Command> _commandMap;
        private TextWriter _logger;

        public CommandLineParser(TextWriter logger)
        {
            _commandMap = new Dictionary<String, Command>();
            _logger = logger;
        }

        public void AddCommand(Command command)
        {
            _commandMap.Add(command.CommandName, command);
        }

        public bool ParseCommandLine(string[] fragments, out Command parsedCommand)
        {
            bool success = true;
            Command templateCommand = null;

            parsedCommand = null;

            // Determine which command we are parsing
            if (fragments.Length > 0)
            {

                if (_commandMap.TryGetValue(fragments[0], out templateCommand))
                {
                    parsedCommand = new Command(fragments[0], templateCommand.CommandDelegate);
                }
                else
                {
                    _logger.WriteLine("Parser: Unknown command '{0}' ", fragments[0]);
                    success = false;
                }
            }
            else
            {
                _logger.WriteLine("Parser: No Command Given");
                success = false;
            }

            // Parse all of the given arguments
            if (success && (fragments.Length % 2) == 1)
            {
                for (int fragment_index = 1; success && fragment_index < fragments.Length; fragment_index+=2)
                {
                    string argumentName = fragments[fragment_index].Substring(1); // Skip the '-' in front of the argument name
                    string argumentStringValue = fragments[fragment_index + 1];

                    CommandArgument templateArgument= templateCommand.GetArgumentByName(argumentName);

                    if (templateArgument != null)
                    {
                        CommandArgument parsedArgument = templateArgument.Clone();

                        if (parsedArgument.Parse(argumentStringValue))
                        {
                            parsedCommand.AddArgument(parsedArgument);
                        }
                        else
                        {
                            _logger.WriteLine("Parser: Malformed argument value '{0}' for argument '{1}'", 
                                argumentStringValue, argumentName);
                            success = false;
                        }
                    }
                    else
                    {
                        _logger.WriteLine("Parser: Unknown argument '{0}'", argumentName);
                        success = false;
                    }
                }
            }

            // Make sure all required arguments were given
            if (success)
            {
                foreach (CommandArgument templateArgument in templateCommand.ArgumentMap.Values)
                {
                    if (!templateArgument.ArgumentOptional &&
                        !parsedCommand.HasArgumentWithName(templateArgument.ArgumentName))
                    {
                        _logger.WriteLine("Parser: Missing required argument '{0}'", templateArgument.ArgumentName);
                        success = false;
                        break;
                    }
                }
            }

            return success;
        }

        public bool PrintCommands(Command parameters)
        {
            foreach (Command command in _commandMap.Values)
            {
                _logger.Write("{0} ", command.CommandName);

                foreach (CommandArgument argument in command.ArgumentMap.Values)
                {
                    if (argument.ArgumentOptional)
                    {
                        _logger.Write("[-{0} ", argument.ArgumentName);
                    }
                    else
                    {
                        _logger.Write("-{0} ", argument.ArgumentName);
                    }

                    if (argument.ArgumentDescription.Length > 0)
                    {
                        _logger.Write("{0}", argument.ArgumentDescription);
                    }

                    if (argument.ArgumentOptional)
                    {
                        _logger.Write("] ");
                    }
                    else
                    {
                        _logger.Write(" ");
                    }
                }

                _logger.Write(_logger.NewLine);

                if (command.CommandDescription.Length > 0)
                {
                    _logger.WriteLine("  {0}", command.CommandDescription);
                }
            }

            return true;
        }
    }
}
