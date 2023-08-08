using System.Text.RegularExpressions;

namespace NDB.Library.NScript
{
    public class NScript
    {

        public bool readScript(String[] scriptText)
        {
            List<String> requiredFields = new List<string>() { "command", "summary", "remarks" };
            List<NScriptCommand> commands = new List<NScriptCommand>();
            bool inCodeBlock = false;
            foreach (String scriptLine in scriptText)
            {
                Console.WriteLine(scriptLine);
                if (scriptLine.Trim() == "" || scriptLine == "") { continue; } // skip empty lines
                String cleanScriptLine = scriptLine.Trim(); // remove starting and ending whitespace, otherwise the regex below gets f*cked up!
                String[] scriptLineSpaced = Regex.Split(cleanScriptLine, @"\s+(?=(?:[^""]*""[^""]*"")*[^""]*$)(?=(?:[^\[\]]*\[[^\[\]]*\])*[^\[\]]*$)"); // split by spaces but preserve quoted strings
                NScriptCommand command = new NScriptCommand();
                command.key = scriptLineSpaced[0];
                object fullValue = null;
                NScriptValueType fullValueType = NScriptValueType.String;
                if (scriptLine == "endif")
                {
                    command.action = "endif";
                    command.value = "";
                    command.inCodeBlock = false;
                    inCodeBlock = false;
                    command.type = NScriptValueType.Conditional;
                    commands.Add(command);
                    continue;
                }
                if (scriptLineSpaced[1].Contains("="))
                {
                    command.action = "set";
                    fullValue = scriptLineSpaced[2]; // get the second part as the value
                } else if (command.key == "if")
                {
                    command.action = "if";
                    fullValue = cleanScriptLine.Substring(cleanScriptLine.IndexOf("{") + 1).TrimEnd('}'); // so this should look something like 'username = "Narod"'
                    fullValueType = NScriptValueType.Conditional;
                    command.value = fullValue;
                    command.type = fullValueType;
                    inCodeBlock = true;
                    command.inCodeBlock = false;
                    commands.Add(command);
                    continue; // if statements work differently, we don't need to run any further code
                }
                else
                {
                    command.action = "do";
                    fullValue = scriptLineSpaced[1]; // get the first part as the value
                }
                fullValue = fullValue.ToString().Trim('"'); // remove any leading or trailing quotes

                // try to parse the value according to its type
                int intValue;
                float floatValue;
                bool boolValue;
                if (Int32.TryParse(fullValue.ToString(), out intValue))
                {
                    fullValue = intValue; // assign the parsed int value
                    fullValueType = NScriptValueType.Int; // set the type to int
                }
                else if (Single.TryParse(fullValue.ToString(), out floatValue))
                {
                    fullValue = floatValue; // assign the parsed float value
                    fullValueType = NScriptValueType.Float; // set the type to float
                }
                else if (Boolean.TryParse(fullValue.ToString(), out boolValue))
                {
                    fullValue = boolValue; // assign the parsed bool value
                    fullValueType = NScriptValueType.Bool; // set the type to bool
                }
                else if (fullValue.ToString().StartsWith("[") && fullValue.ToString().EndsWith("]"))
                {
                    List<string> listValue = new List<string>(); // create a new list object
                    String[] listElements = fullValue.ToString().Trim('[', ']').Split(','); // split by commas and remove brackets
                    foreach (String element in listElements)
                    {
                        listValue.Add(element.Trim()); // add each element to the list after trimming spaces
                    }
                    fullValue = listValue; // assign the list value
                    fullValueType = NScriptValueType.List; // set the type to list
                } else
                {
                    bool isCommand = Regex.IsMatch(fullValue.ToString(), @"^[a-zA-Z.]+\([^\(\)]*\)$");
                    if (isCommand)
                    {
                        fullValueType = NScriptValueType.Command;
                    } else
                    {
                        fullValueType = NScriptValueType.String; // we will assume that it is a string
                    }
                }

                Console.WriteLine(fullValue);
                command.value = fullValue;
                command.type = fullValueType;
                command.inCodeBlock = inCodeBlock;
                commands.Add(command);
            }
            foreach (NScriptCommand nScriptCommand in commands)
            {
                if (requiredFields.Contains(nScriptCommand.key))
                {
                    requiredFields.Remove(nScriptCommand.key);
                }
            }
            if (requiredFields.Count == 0) {
                registerScript(commands);
                Console.WriteLine("Registering script...");
                return true; 
            } else {
                Console.WriteLine("Will not register script.");
                return false; 
            }
        }

        public bool registerScript(List<NScriptCommand> nScriptCommands)
        {
            String commandName = (string)nScriptCommands[0].value;
            String summaryName = (string)nScriptCommands[1].value;
            String remarksName = (string)nScriptCommands[2].value;
            nScriptCommands.RemoveRange(0, 3);
            if (commandName == "" || summaryName == "" || remarksName == "")
            {
                Console.WriteLine("Failed to register script.");
                return false;
            } else
            {
                Console.WriteLine("Script registering almost complete...");
                NScriptInterpreter.fastCommands.Add(commandName);
                NScriptInterpreter.fullCommand newCommand = new();
                newCommand.commandName = commandName;
                newCommand.summaryName = summaryName;
                newCommand.remarksName = remarksName;
                newCommand.scriptCommands = nScriptCommands;
                NScriptInterpreter.commands.Add(newCommand);
                return true;
            }
        }
    }

    public enum NScriptValueType // list of supported nscript value types
    {
        Command, // regex will check if it's a command or not
        Conditional, // used for IF statements
        String,
        Int,
        Float,
        Bool,
        List // lists are currently only supported as String lists
    }

    public struct NScriptCommand
    {
        public object value;
        public NScriptValueType type;
        public String key;
        public String action;
        public bool inCodeBlock; // used if it's within a conditional or something else. the interpreter will know to only run this if the previous conditional check succeeded until it hits an end-if
    }

    public struct ActionResponse
    {
        public object valueReturned;
        public Dictionary<String, Object> variablesReturned;
    }
}