namespace NDB.Library.NScript
{
    public class NScript
    {

        public bool readScript(String[] scriptText)
        {
            List<String> requiredFields = new List<string>() { "command", "summary", "remarks" };
            List<NScriptCommand> commands = new List<NScriptCommand>();
            foreach (String scriptLine in scriptText)
            {
                if (scriptLine.Replace(" ", "") == "") { continue; }
                String[] scriptLineSpaced = scriptLine.Split(' ');
                NScriptCommand command = new NScriptCommand();
                command.key = scriptLineSpaced[0];
                String fullValue = "";
                if (scriptLineSpaced[1].Contains("="))
                {
                    command.action = "set";
                    if (scriptLineSpaced.Length == 3) { fullValue = scriptLineSpaced[2]; }
                    else
                    {
                        for (int i = 1; i < scriptLineSpaced.Length; i++)
                        {
                            fullValue += " " + scriptLineSpaced[i];
                        }
                    }
                }
                else
                {
                    command.action = "do";
                    if(scriptLineSpaced.Length == 2) { fullValue = scriptLineSpaced[1]; } else
                    {
                        for (int i = 1; i < scriptLineSpaced.Length; i++)
                        {
                            fullValue += " " + scriptLineSpaced[i];
                        }
                    }
                }
                fullValue = fullValue.Trim();
                command.value = fullValue;
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
                return true; 
            } else {
                return false; 
            }
        }

        public bool registerScript(List<NScriptCommand> nScriptCommands)
        {
            String commandName = nScriptCommands[0].value;
            String summaryName = nScriptCommands[1].value;
            String remarksName = nScriptCommands[2].value;
            nScriptCommands.RemoveRange(0, 3);
            if (commandName == "" || summaryName == "" || remarksName == "")
            {
                return false;
            } else
            {
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

    public struct NScriptCommand
    {
        public String value;
        public String key;
        public String action;
    }
}