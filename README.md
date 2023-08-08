# An NScript Parser & Interpreter for Discord.NET Bots

NDB.Library.NScript (Beta) is an experimental but functional way of writing Discord bot commands in the custom language of NScript.

This specific library is written for the NDB bot, however it should be compatible with all Discord.NET written bots on .NET 6.0 and higher.

## What is NScript?

NScript is the custom language which is used to create compatible scripts, which can be read in by this parser & interpreter.

It takes inspiration from several different programming languages, such as Visual Basic, C# and Python.

Want to know all compatible NScript commands supported by this library & learn more about NScript? Check the wiki.

## How do I use this library?

If you're using NDB & NDB.Loader:

1. Download (or compile) this library.
2. Place it in the same folder as your NDB.Main executable.
3. Either add this library to your config.loader.json file, or run the load command when the bot is turned on!

If you're using a different bot (or a custom bot):

You'll need to follow your specific bots instructions. If it has none, or you're using a custom bot:

1. Download the source code for this library.
2. Add a reference to this library in your Discord Bot (usually right click project on right hand side -> Add Reference)
3. Load in this library like you would any new library - this may be vague but this can be so very varied!

We are unable to provide any assistance for custom or different bots.

## Limitation

There's a lot. These change on an on-going basis, and as such it's also difficult to log them. To name a few as of the current version:

- Nested IF loops are unsupported.
- Additional operators other than EQUALS (=) are unsupported for IF statements.
- Spaces in random places can cause the command to not function as expected - do not put spaces around the commas in the arguments for your command. e.g. `index = random(0,length)` is good, `index = random(0, length)` is bad.
- Command, summary and remarks must be the first 3 lines in your NScript file, and also be in that order.
- Command supported is very limited.
- No looping or recursion support.
- General instability

## Why use NScript?

NScript may have limitations and be slower than writing a command in plain VB, C# or other programming language, but it's also simpler, and requires no IDE, compiler or previous programming knowledge.

It also abstracts away complications, at the expense of lowering how complex your script can be. For example, your data type (String, Integer, Boolean, etc.) is handled automatically.

Error handling is also baked in (early stages), so you'll get a better idea of what went wrong any why.

Being a scripting language, NScripts can also be loaded in whilst the bot is live with zero fuss, without messy library issues, errors and memory leaks.