namespace SshSecurityCheck.Services;

/// <summary>
/// Service for creating backups of SSH configuration files.
/// </summary>
internal class BackupService
{
    /// <summary>
    /// Creates a backup of the specified configuration file.
    /// </summary>
    /// <param name="configPath">Path to the configuration file to backup</param>
    /// <returns>Path to the backup file</returns>
    public string CreateBackup(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath}");
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = $"{configPath}.backup_{timestamp}";

        File.Copy(configPath, backupPath);

        return backupPath;
    }

    /// <summary>
    /// Verifies that a backup file was created successfully.
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    /// <returns>True if backup exists and is valid</returns>
    public bool VerifyBackup(string backupPath)
    {
        if (!File.Exists(backupPath))
            return false;

        var fileInfo = new FileInfo(backupPath);
        return fileInfo.Length > 0;
    }

    /// <summary>
    /// Restores a configuration file from backup.
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    /// <param name="originalPath">Path to restore the file to</param>
    public void RestoreFromBackup(string backupPath, string originalPath)
    {
        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException($"Backup file not found: {backupPath}");
        }

        File.Copy(backupPath, originalPath, overwrite: true);
    }

    /// <summary>
    /// Lists all backup files for a given configuration file.
    /// </summary>
    /// <param name="configPath">Path to the configuration file</param>
    /// <returns>List of backup file paths</returns>
    public List<string> ListBackups(string configPath)
    {
        var directory = Path.GetDirectoryName(configPath) ?? "/etc/ssh";
        var fileName = Path.GetFileName(configPath);
        var pattern = $"{fileName}.backup_*";

        if (!Directory.Exists(directory))
            return new List<string>();

        return Directory.GetFiles(directory, pattern)
            .OrderByDescending(f => f)
            .ToList();
    }
}
