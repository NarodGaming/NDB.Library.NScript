using Discord.Commands;

namespace NDB.Library.NScript
{
    public class NScriptCommands : ModuleBase<SocketCommandContext>
    {
        private NScript nscriptHandler = new(); // nscript handler which carries most of the functions out


        [Command("scriptload")]
        [Summary("OWNER: Loads in NScript files.")]
        [Remarks("scriptload <nscript file>")]
        public Task loadScript(String fileName)
        {
            ReplyAsync($"Loading {fileName}");
            String[] scriptFile = File.ReadAllLines(fileName);
            if (nscriptHandler.readScript(scriptFile)) // if it was successful in reading the script in
            {
                return ReplyAsync($"Finished loading {fileName}");
            } else
            {
                return ReplyAsync($"Failed to load {fileName}");
            }
        }

        [Command("scriptsloaded")]
        [Alias("loadedscripts", "scripts")]
        [Summary("Shows all loaded scripts.")]
        [Remarks("scriptsloaded")]
        public Task showScripts() // will print out all scripts that are loaded in
        {
            String combineMessage = "These are the loaded scripts:" + Environment.NewLine; 
            foreach (String command in NScriptInterpreter.fastCommands)
            {
                combineMessage += command + Environment.NewLine;
            }
            return ReplyAsync(combineMessage);
        }
    }
}
