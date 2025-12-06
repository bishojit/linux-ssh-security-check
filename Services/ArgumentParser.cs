using SshSecurityCheck.Models;

namespace SshSecurityCheck.Services;

/// <summary>
/// Service for parsing command line arguments.
/// </summary>
internal class ArgumentParser
{
    /// <summary>
    /// Parses command line arguments into an options object.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Parsed options</returns>
    public CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;
                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;
                case "-c":
                case "--config":
                    if (i + 1 < args.Length)
                    {
                        options.ConfigPath = args[++i];
                    }
                    break;
                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        options.OutputFile = args[++i];
                    }
                    break;
                case "-f":
                case "--fix":
                    options.FixMode = true;
                    break;
            }
        }

        return options;
    }
}
