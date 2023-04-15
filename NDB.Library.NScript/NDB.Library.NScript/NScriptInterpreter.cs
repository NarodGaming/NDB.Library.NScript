using Discord;
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
            if (message.Content.StartsWith(NDB_Main._config["stringprefix"]) == false) { return; }
            String commandToCheck = (message.Content.Split(" ")[0].Substring(1));
            if (fastCommands.Contains(commandToCheck))
            {
                commandInterpreter = new();
                SocketUserMessage? userMessage = message as SocketUserMessage;
                SocketCommandContext context = new(NDB_Main._client, userMessage);
                await commandInterpreter.commandInterpreter(context, commandToCheck, message.Content.Split(" "));
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

    public class NScriptCommandInterpreter
    {
        internal Dictionary<String, String> variables = new Dictionary<String, String>();
        public async Task commandInterpreter(SocketCommandContext context, String commandPassed, String[] passedArgs)
        {
            variables = new Dictionary<String, String>();
            int argnum = 0;
            foreach (String argument in passedArgs)
            {
                if (argnum == 0) { argnum = 1; continue; }
                Console.WriteLine($"Adding argument {argnum} ({argument})");
                variables.Add($"arg{argnum}", argument);
                argnum++;
            }
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
                            }
                            else if (variables.ContainsKey(commandLine.key) == false)
                            {
                                Console.WriteLine("Script attempted to set variable which doesn't exist!");
                            } else
                            {
                                variables[commandLine.key] = (string)commandLine.value;
                            }
                        } else if (commandLine.action == "do") // command to complete (e.g. say text)
                        {
                            switch (commandLine.key)
                            {
                                case "var":
                                    if (commandLine.value.ToString().StartsWith("arg"))
                                    {
                                        Console.WriteLine("Script attempted to set argument variable.");
                                    }
                                    else
                                    {
                                        variables.Add((string)commandLine.value, "");
                                    }
                                    break;
                                case "say":
                                    String messageToSay = (string)commandLine.value;
                                    if (variables.ContainsKey((string)commandLine.value))
                                    {
                                        messageToSay = variables[(string)commandLine.value];
                                    }
                                    await context.Channel.SendMessageAsync(messageToSay);
                                    break;
                                case "dm":
                                    SocketUser user = context.User;
                                    if (context.Message.MentionedUsers.Count >= 1) { user = context.Message.MentionedUsers.First(); }
                                    String messageToDM = (string)commandLine.value;
                                    if (variables.ContainsKey((string)commandLine.value))
                                    {
                                        messageToSay = variables[(string)commandLine.value];
                                    }
                                    await user.SendMessageAsync(messageToDM);
                                    break;
                                case "random":
                                    List<Object> args = (List<object>)commandLine.value;
                                    if (variables.ContainsKey(args[0].ToString()) == false)
                                    {
                                        Console.WriteLine("Script attempted to write to argument before setting it.");
                                    } else
                                    {
                                        int safeLow;
                                        int safeHigh;
                                        if (Int32.TryParse(args[1].ToString(), out safeLow) == false)
                                        {
                                            String safeLowStr;
                                            if (variables.TryGetValue(args[1].ToString(), out safeLowStr) == false)
                                            {
                                                Console.WriteLine("Script provided invalid 'Low' number.");
                                                break;
                                            }
                                            if (Int32.TryParse(safeLowStr, out safeLow) == false)
                                            {
                                                Console.WriteLine("Script provided 'Low' number from a variable which wasn't an integer.");
                                                break;
                                            }
                                        }
                                        if (Int32.TryParse(args[2].ToString(), out safeHigh) == false)
                                        {
                                            String safeHighStr;
                                            if (variables.TryGetValue(args[2].ToString(), out safeHighStr) == false)
                                            {
                                                Console.WriteLine("Script provided invalid 'High' number.");
                                                break;
                                            }
                                            if (Int32.TryParse(safeHighStr, out safeHigh) == false)
                                            {
                                                Console.WriteLine(safeHighStr);
                                                Console.WriteLine("Script provided 'High' number from a variable which wasn't an integer.");
                                                break;
                                            }
                                        }
                                        variables[args[0].ToString()] = Random.Shared.Next(safeLow, safeHigh).ToString();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}
