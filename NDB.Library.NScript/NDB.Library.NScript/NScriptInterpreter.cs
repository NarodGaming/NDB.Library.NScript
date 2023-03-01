using Discord.Commands;
using Discord.Commands.Builders;
using Discord.Interactions.Builders;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDB.Library.NScript
{
    public static class NScriptInterpreter
    {
        public static List<fullCommand> commands = new();
        public static List<String> fastCommands = new();

        public static NScriptCommandInterpreter commandInterpreter = new();

        static NScriptInterpreter()
        {
            NDB_Main._client.MessageReceived += ScriptHandler;
        }

        public static async Task ScriptHandler(SocketMessage message)
        {
            String commandToCheck = (message.Content.Split(" ")[0].Substring(1));
            if (message.Content.StartsWith(NDB_Main._config["stringprefix"]) && fastCommands.Contains(commandToCheck))
            {
                SocketUserMessage? userMessage = message as SocketUserMessage;
                SocketCommandContext context = new(NDB_Main._client, userMessage);
                await commandInterpreter.commandInterpreter(context, commandToCheck);
            }
        }
        public struct fullCommand
        {
            public String commandName;
            public String summaryName;
            public String remarksName;
            public List<NScriptCommand> scriptCommands;
        }
    }

    public class NScriptCommandInterpreter : ModuleBase<SocketCommandContext>
    {
        internal Dictionary<String, String> variables = new Dictionary<String, String>();
        public async Task commandInterpreter(SocketCommandContext context, String commandPassed)
        {
            foreach (NScriptInterpreter.fullCommand fullCommand in NScriptInterpreter.commands)
            {
                if(fullCommand.commandName == commandPassed)
                {
                    foreach (NScriptCommand commandLine in fullCommand.scriptCommands)
                    {
                        if(commandLine.action == "set") // command with equals (e.g. text = test123)
                        {
                            if (commandLine.key == null)
                            {
                                Console.WriteLine("Script attempted to set variable to null, not possible!");
                            } else if (variables.ContainsKey(commandLine.key) == false)
                            {
                                Console.WriteLine("Script attempted to set variable which doesn't exist!");
                            } else
                            {
                                variables[commandLine.key] = commandLine.value;
                            }
                        } else if (commandLine.action == "do") // command to complete (e.g. say text)
                        {
                            if (commandLine.key == "var")
                            {
                                variables.Add(commandLine.value, "");
                            }
                            if (commandLine.key == "say")
                            {
                                String messageToSay = commandLine.value;
                                if (variables.ContainsKey(commandLine.value))
                                {
                                    messageToSay = variables[commandLine.value];
                                }
                                await context.Channel.SendMessageAsync(messageToSay);
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}
