using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDB.Library.NScript
{
    public class NScriptCommands : ModuleBase<SocketCommandContext>
    {
        private NScript nscriptHandler = new();


        [Command("scriptload")]
        [Summary("OWNER: Loads in NScript files.")]
        [Remarks("scriptload <nscript file>")]
        public Task loadScript(String fileName)
        {
            ReplyAsync($"Loading {fileName}");
            String[] scriptFile = File.ReadAllLines(fileName);
            if (nscriptHandler.readScript(scriptFile))
            {
                return ReplyAsync($"Finished loading {fileName}");
            } else
            {
                return ReplyAsync($"Failed to load {fileName}");
            }
        }
    }
}
