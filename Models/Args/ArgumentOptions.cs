namespace linux_ssh_security_check.Models.Args;

/// <summary>
/// Command line options for the SSH security checker.
/// </summary>
public class ArgumentOptions
{
    /// <summary>
    /// Gets or sets whether to display help information.
    /// </summary>
    public bool ShowHelp { get; set; }

    /// <summary>
    /// Gets or sets whether to enable verbose output.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// Gets or sets the path to the SSH configuration file.
    /// </summary>
    public string? ConfigPath { get; set; }

    /// <summary>
    /// Gets or sets the output file path for the report.
    /// </summary>
    public string? OutputFile { get; set; }

    /// <summary>
    /// Gets or sets whether to enable interactive remediation mode.
    /// </summary>
    public bool FixMode { get; set; }
}
