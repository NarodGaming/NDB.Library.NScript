using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.Interactions.Builders;
using Discord.WebSocket;
using System;
using System.Collections;
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
                    bool conditionalPassed = false;
                    bool isInIf = false;
                    foreach (NScriptCommand commandLine in fullCommand.scriptCommands)
                    {
                        if (isInIf && !conditionalPassed && commandLine.action != "endif") { Console.WriteLine("Skipping failed conditional command.");  continue; } // means that if we're in an if code block, but the condition failed, do not run and skip
                        if (isInIf && conditionalPassed) { Console.WriteLine(commandLine.key); }
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
                                if(commandLine.type == NScriptValueType.Command)
                                {
                                    ActionResponse actionresp = actionInterpreter(commandLine, variables, context);
                                    variables = actionresp.variablesReturned;
                                    variables[commandLine.key] = actionresp.valueReturned.ToString();
                                } else
                                {
                                    variables[commandLine.key] = (string)commandLine.value;
                                }
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
                                    Object messageToSay = commandLine.value;
                                    try
                                    {
                                        if (variables.ContainsKey((string)commandLine.value))
                                        {
                                            messageToSay = variables[(string)commandLine.value];
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("Warning! Failed to cast conversion at say:try1 - probably not a string.");
                                    }
                                    if (messageToSay is not IList)
                                    {
                                        await context.Channel.SendMessageAsync((String)messageToSay);
                                    } else
                                    {
                                        // await context.Channel.SendMessageAsync("Oops! You tried to print a non-String message, but that isn't currently supported.");
                                        List<String> messageToSayList = ((List<Object>)messageToSay).Select(s => (string)s).ToList();
                                        String finalMessageOutput = "";
                                        foreach (String message in messageToSayList)
                                        {
                                            if(message.StartsWith('"'))
                                            {
                                                finalMessageOutput += message.TrimStart('"').TrimEnd('"');
                                                continue;
                                            } else
                                            {
                                                finalMessageOutput += variables[message];
                                            }
                                        }
                                        await context.Channel.SendMessageAsync(finalMessageOutput);
                                    }
                                    break;
                                case "embedsay":
                                    Object embedToSay = commandLine.value;
                                    try
                                    {
                                        if (variables.ContainsKey((string)commandLine.value))
                                        {
                                            embedToSay = variables[(string)commandLine.value];
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("Warning! Failed to cast conversion at embedsay:try1 - probably not a string.");
                                    }
                                    if (embedToSay is not IList)
                                    {
                                        EmbedBuilder embedBuilder = new EmbedBuilder();
                                        embedBuilder.Description = (String)embedToSay;
                                        embedBuilder.Title = $"{NDB_Main._config["botname"]} | Script Response";
                                        await context.Channel.SendMessageAsync("", embed: embedBuilder.Build());
                                    }
                                    else
                                    {
                                        // await context.Channel.SendMessageAsync("Oops! You tried to print a non-String message, but that isn't currently supported.");
                                        List<String> messageToSayList = ((List<Object>)embedToSay).Select(s => (string)s).ToList();
                                        String finalMessageOutput = "";
                                        foreach (String message in messageToSayList)
                                        {
                                            if (message.StartsWith('"'))
                                            {
                                                finalMessageOutput += message.TrimStart('"').TrimEnd('"');
                                                continue;
                                            }
                                            else
                                            {
                                                finalMessageOutput += variables[message];
                                            }
                                        }
                                        EmbedBuilder embedBuilder = new EmbedBuilder();
                                        embedBuilder.Description = finalMessageOutput;
                                        embedBuilder.Title = $"{NDB_Main._config["botname"]} | Script Response";
                                        await context.Channel.SendMessageAsync("", embed: embedBuilder.Build());
                                    }
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
                            }
                        } else if (commandLine.action == "if")
                        {
                            isInIf = true;
                            string[] conditionToCheck = ((string)commandLine.value).Split(' ');
                            string firstArg;
                            string comparisonType = conditionToCheck[1];
                            string secondArg;
                            if (conditionToCheck[0].StartsWith('"')) // if the first condition is a string
                            {
                                firstArg = conditionToCheck[0].Trim('"');
                            } else
                            {
                                firstArg = variables[conditionToCheck[0]];
                            }
                            if (conditionToCheck[2].StartsWith('"'))
                            {
                                secondArg = conditionToCheck[2].Trim('"');
                            } else
                            {
                                secondArg = variables[conditionToCheck[2]];
                            }

                            if(comparisonType == "=")
                            {
                                if (firstArg == secondArg)
                                {
                                    conditionalPassed = true;
                                } else
                                {
                                    conditionalPassed = false;
                                }
                                Console.WriteLine($"Conditional ran: {conditionalPassed}");
                            }
                        } else if (commandLine.action == "endif")
                        {
                            isInIf = false;
                            conditionalPassed = false;
                        }
                    }
                    break;
                }
            }
        }

        public ActionResponse actionInterpreter(NScriptCommand command, Dictionary<String, String> variables, SocketCommandContext context)
        {
            string commandName = ((String)command.value).Split('(')[0]; // gets the command name (everything before the first bracket open)
            string argsPassed = ((String)command.value).Substring(((String)command.value).IndexOf("(")+1);
            argsPassed = argsPassed.TrimEnd(')');
            List<String> argsArray = argsPassed.Split(",").ToList();
            List<String> args = new();
            foreach (String arg in argsArray)
            {
                if (arg.StartsWith("arg"))
                {
                    Console.WriteLine($"Added arg to args list {variables[arg]}");
                    args.Add(variables[arg]);
                } else
                {
                    Console.WriteLine($"Added literal to args list {arg}");
                    args.Add(arg);
                }
            }
            ActionResponse response = new ActionResponse();
            switch (commandName)
            {
                case "random":
                    int safeLow;
                    int safeHigh;
                    if (Int32.TryParse(args[0].ToString(), out safeLow) == false)
                    {
                        String safeLowStr;
                        if (variables.TryGetValue(args[0].ToString(), out safeLowStr) == false)
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
                    if (Int32.TryParse(args[1].ToString(), out safeHigh) == false)
                    {
                        String safeHighStr;
                        if (variables.TryGetValue(args[1].ToString(), out safeHighStr) == false)
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
                    response.valueReturned = Random.Shared.Next(safeLow, safeHigh).ToString();
                    response.variablesReturned = variables;
                    return response;
                case "context.guildname":
                    response.valueReturned = context.Guild.Name;
                    response.variablesReturned = variables;
                    return response;
                case "arg.username":
                    response.valueReturned = context.Message.MentionedUsers.First().Username;
                    response.variablesReturned = variables;
                    return response;
                case "arg.userjoinedat":
                    SocketGuildUser guildUser = (SocketGuildUser)context.Message.MentionedUsers.First() as SocketGuildUser;
                    response.valueReturned = guildUser.JoinedAt.Value.ToString("f");
                    response.variablesReturned = variables;
                    return response;
                case "arg.usercreatedat":
                    response.valueReturned = context.Message.MentionedUsers.First().CreatedAt.ToString("f");
                    response.variablesReturned = variables;
                    return response;
                default:
                    break;
            }
            Console.WriteLine("Action Interpreter was run, but none of the commands matched!");
            return new ActionResponse();
        }
    }
}
