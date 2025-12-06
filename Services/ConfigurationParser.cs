using System.Text.RegularExpressions;

namespace SshSecurityCheck.Services;

/// <summary>
/// Service for parsing SSH configuration files.
/// </summary>
internal class ConfigurationParser
{
    private string? _configContent;

    /// <summary>
    /// Loads the SSH configuration file.
    /// </summary>
    /// <param name="configPath">Path to the configuration file</param>
    public async Task LoadConfigurationAsync(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"SSH config file not found at {configPath}");
        }

        _configContent = await File.ReadAllTextAsync(configPath);

        // todo:debug
        Console.WriteLine($"+++++++++++++++++++++++++++++++++++++++++++++++");
        Console.WriteLine(_configContent);
        Console.WriteLine($"+++++++++++++++++++++++++++++++++++++++++++++++");
    }

    /// <summary>
    /// Checks if a configuration parameter has a specific value.
    /// </summary>
    /// <param name="parameter">Configuration parameter name</param>
    /// <param name="expectedValue">Expected value</param>
    /// <returns>True if the parameter has the expected value</returns>
    public bool CheckConfigValue(string parameter, string expectedValue)
    {
        if (string.IsNullOrEmpty(_configContent)) return false;

        var pattern = $@"^\s*{Regex.Escape(parameter)}\s+{Regex.Escape(expectedValue)}\s*(?:#.*)?$";
        return Regex.IsMatch(_configContent, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Checks if a configuration parameter exists.
    /// </summary>
    /// <param name="parameter">Configuration parameter name</param>
    /// <returns>True if the parameter is configured</returns>
    public bool CheckConfigExists(string parameter)
    {
        if (string.IsNullOrEmpty(_configContent)) return false;

        var pattern = $@"^\s*{Regex.Escape(parameter)}\s+\S+";
        return Regex.IsMatch(_configContent, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Gets the value of a configuration parameter.
    /// </summary>
    /// <param name="parameter">Configuration parameter name</param>
    /// <returns>The parameter value, or null if not found</returns>
    public string? GetConfigValue(string parameter)
    {
        if (string.IsNullOrEmpty(_configContent)) return null;

        var pattern = $@"^\s*{Regex.Escape(parameter)}\s+(\S+)";
        var match = Regex.Match(_configContent, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Checks if the configuration contains a specific pattern.
    /// </summary>
    /// <param name="pattern">Regular expression pattern</param>
    /// <returns>True if the pattern matches</returns>
    public bool ContainsPattern(string pattern)
    {
        if (string.IsNullOrEmpty(_configContent)) return false;
        return Regex.IsMatch(_configContent, pattern, RegexOptions.Multiline);
    }
}
