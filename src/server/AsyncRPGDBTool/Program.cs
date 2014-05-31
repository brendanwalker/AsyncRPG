using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncRPGDBTool.UnitTests;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGDBTool
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineParser commandLineParser = new CommandLineParser(Console.Out);
            DungeonValidator dungeonValidator = new DungeonValidator(Console.Out);
            UnitTestDriver unitTestDriver = new UnitTestDriver(Console.Out);
            WebServiceValidator webServiceValidator = new WebServiceValidator(Console.Out);
            Command parsedCommand= null;
            bool success = true;

            // Add the list of commands we can add
            commandLineParser.AddCommand(
                new Command(
                    "help",
                    commandLineParser.PrintCommands, 
                    null, 
                    "Prints a list of all the supported commands"));
            commandLineParser.AddCommand(
                new Command(
                    "rebuild_database",
                    RebuildDatabase,
                    new CommandArgument[] {
                        new CommandArgument_String("C", "Connection String", false),
                        new CommandArgument_String("M", "Mob Data Folder", false),
                        new CommandArgument_String("T", "Room Template Folder", false),
                        new CommandArgument_String("EA", "Server Email Address", true),
                        new CommandArgument_String("EH", "Server Email Host", true),
                        new CommandArgument_Int32("EP", "Server Email Password", true),
                        new CommandArgument_String("EU", "Connection String", true),
                        new CommandArgument_String("ED", "Connection String", true)
                    }, 
                    "(Re)parses the room template XML files and stores them in the room_templates DB table"));
            commandLineParser.AddCommand(
                new Command(
                    "run_web_service_test_script",
                    webServiceValidator.ValidateWebService,
                    new CommandArgument[] {
                        new CommandArgument_String("S", "JSON Test Script Filename"),
                        new CommandArgument_String("W", "Web Service Base URL")
                    },
                    "Runs a set of web service calls and validates responses"));            
            commandLineParser.AddCommand(
                new Command(
                    "validate_dungeons",
                    dungeonValidator.ValidateDungeons,
                    new CommandArgument[] {
                        new CommandArgument_String("C", "Connection String"),
                        new CommandArgument_Int32("G", "GameID", true),
                        new CommandArgument_String("D", "3D Geometry Dump Path", true)
                    },
                    "Validates that all possible dungeons are traversable"));
            commandLineParser.AddCommand(
                new Command(
                    "run_unit_tests",
                    unitTestDriver.RunUnitTests,
                    null,
                    "Runs unit tests for the AsyncRPGSharedLib"));

            // Parse the command line fragments
            if (!commandLineParser.ParseCommandLine(args, out parsedCommand))
            {
                Console.Out.WriteLine("usage: dbtool.exe <command> [arguments]");
                Console.Out.WriteLine("'dbtool.exe help' prints a list of commands");

                success = false;
            }

            // Execute the parsed command
            if (success)
            {
                if (parsedCommand.Execute())
                {
                    Console.Out.WriteLine("Successfully executed command {0}", parsedCommand.CommandName);
                }
                else
                {
                    Console.Out.WriteLine("Error executing command {0}", parsedCommand.CommandName);
                }
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Out.WriteLine("Press any key to continue...");
                Console.In.ReadLine();
            }
        }

        private static bool RebuildDatabase(Command command)
        {
            bool success = true;
            string connection_string = "";
            string maps_directory = "";
            string mobs_directory = "";
            Logger logger = new Logger(Console.Out.WriteLine);

            if (command.HasArgumentWithName("C"))
            {
                connection_string = command.GetTypedArgumentByName<CommandArgument_String>("C").ArgumentValue;
            }
            else
            {
                logger.LogError("RebuildDatabase: Missing expected connection string parameter");
                success = false;
            }

            if (command.HasArgumentWithName("M"))
            {
                mobs_directory= command.GetTypedArgumentByName<CommandArgument_String>("M").ArgumentValue;
            }
            else
            {
                logger.LogError("RebuildDatabase: No mob data directory given");
            }

            if (command.HasArgumentWithName("T"))
            {
                maps_directory= command.GetTypedArgumentByName<CommandArgument_String>("T").ArgumentValue;
            }
            else
            {
                logger.LogError("RoomTemplateParser: No template directory given");
                success = false;
            }

            // Set the optional e-mail account constants
            if (success)
            {
                if (command.HasArgumentWithName("EA"))
                {
                    MailConstants.WEB_SERVICE_EMAIL_ADDRESS = 
                        command.GetTypedArgumentByName<CommandArgument_String>("EA").ArgumentValue;
                }

                if (command.HasArgumentWithName("EH"))
                {
                    MailConstants.WEB_SERVICE_EMAIL_HOST =
                        command.GetTypedArgumentByName<CommandArgument_String>("EH").ArgumentValue;
                }

                if (command.HasArgumentWithName("EP"))
                {
                    MailConstants.WEB_SERVICE_EMAIL_PORT =
                        command.GetTypedArgumentByName<CommandArgument_Int32>("EP").ArgumentValue;
                }


                if (command.HasArgumentWithName("EU"))
                {
                    MailConstants.WEB_SERVICE_EMAIL_USERNAME =
                        command.GetTypedArgumentByName<CommandArgument_String>("EU").ArgumentValue;
                }

                if (command.HasArgumentWithName("ED"))
                {
                    MailConstants.WEB_SERVICE_EMAIL_USERNAME =
                        command.GetTypedArgumentByName<CommandArgument_String>("ED").ArgumentValue;
                }
            }

            if (success)
            {
                try
                {
                    string constructionResult = "";
                    DatabaseManagerConfig dbConfig = 
                        new DatabaseManagerConfig(connection_string, mobs_directory, maps_directory);
                    DatabaseManager dbManager = new DatabaseManager(dbConfig);

                    if (!dbManager.ReCreateDatabase(logger, out constructionResult))
                    {
                        logger.LogError(constructionResult);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.LogError(string.Format("Failed to recreate database: {0}", ex.Message));
                }
            }

            return success;
        }
    }
}
