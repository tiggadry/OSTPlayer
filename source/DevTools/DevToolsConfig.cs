// ====================================================================
// FILE: DevToolsConfig.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: DevTools
// LOCATION: DevTools/
// VERSION: 1.3.0
// CREATED: 2025-08-07
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Configuration management system for DevTools automation and AI assistant
// integration. This component provides centralized configuration, runtime
// settings management, and intelligent defaults for development automation
// workflows.
//
// FEATURES:
// - Centralized configuration management
// - Runtime settings modification
// - Intelligent defaults for development workflows
// - Configuration validation and error handling
// - Profile-based configuration support
// - Integration with AI assistant preferences
//
// DEPENDENCIES:
// - System.IO for file operations
// - System.Text for JSON serialization
// - System.Collections.Generic for configuration collections
// - Newtonsoft.Json for JSON processing (if available)
//
// DESIGN PATTERNS:
// - Singleton pattern for configuration management
// - Builder pattern for configuration creation
// - Observer pattern for configuration change notifications
// - Strategy pattern for different configuration sources
//
// AI INTEGRATION:
// - AI assistant behavior configuration
// - Automation level settings
// - Template preferences and customization
// - Hook registration and management
//
// PERFORMANCE NOTES:
// - Lazy loading of configuration files
// - In-memory caching of frequently accessed settings
// - Minimal file I/O for configuration persistence
// - Efficient configuration validation
//
// LIMITATIONS:
// - Configuration changes require application restart for some settings
// - Limited to predefined configuration schema
// - No real-time configuration synchronization
//
// FUTURE REFACTORING:
// FUTURE: Add support for user-specific configuration profiles
// FUTURE: Implement real-time configuration change notifications
// FUTURE: Add configuration encryption for sensitive settings
// FUTURE: Implement configuration backup and restore
// CONSIDER: Integration with external configuration services
// CONSIDER: Support for environment-specific configurations
//
// TESTING:
// - Unit tests for configuration loading and validation
// - Integration tests with actual configuration files
// - Performance tests for configuration access
// - Validation tests for configuration schema
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - JSON configuration format
//
// CHANGELOG:
// 2025-08-07 v1.3.0 - Initial implementation for DevTools enhancement
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// Configuration management system for DevTools automation and AI assistant integration
    /// </summary>
    public static class DevToolsConfig
    {
        // Configuration file path
        private static readonly string ConfigFilePath = "DevTools/config.json";

        // Default configuration
        private static DevToolsConfiguration _configuration;
        private static readonly object _configLock = new object();
        private static DateTime _lastConfigLoad = DateTime.MinValue;

        // Configuration change event
        /// <summary>
        /// Occurs when the configuration is changed.
        /// </summary>
        public static event Action<DevToolsConfiguration> ConfigurationChanged;

        static DevToolsConfig()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Gets the current configuration
        /// </summary>
        /// <returns>Current DevTools configuration</returns>
        public static DevToolsConfiguration GetConfiguration()
        {
            lock (_configLock)
            {
                // Reload if file has changed
                if (File.Exists(ConfigFilePath))
                {
                    var lastWrite = File.GetLastWriteTime(ConfigFilePath);
                    if (lastWrite > _lastConfigLoad)
                    {
                        LoadConfiguration();
                    }
                }

                return _configuration ?? CreateDefaultConfiguration();
            }
        }

        /// <summary>
        /// Updates the configuration
        /// </summary>
        /// <param name="newConfiguration">New configuration to apply</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateConfiguration(DevToolsConfiguration newConfiguration)
        {
            try
            {
                lock (_configLock)
                {
                    if (ValidateConfiguration(newConfiguration))
                    {
                        _configuration = newConfiguration;
                        SaveConfiguration();
                        ConfigurationChanged?.Invoke(_configuration);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Log error but don't throw
            }

            return false;
        }

        /// <summary>
        /// Gets a specific configuration value
        /// </summary>
        /// <typeparam name="T">Type of the configuration value</typeparam>
        /// <param name="key">Configuration key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Configuration value or default</returns>
        public static T GetValue<T>(string key, T defaultValue = default(T))
        {
            var config = GetConfiguration();

            if (config.CustomSettings.ContainsKey(key))
            {
                try
                {
                    var value = config.CustomSettings[key];
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }

                    // Try to convert
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets a specific configuration value
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if successfully set, false otherwise</returns>
        public static bool SetValue(string key, object value)
        {
            try
            {
                var config = GetConfiguration();
                config.CustomSettings[key] = value;
                return UpdateConfiguration(config);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Resets configuration to defaults
        /// </summary>
        /// <returns>True if successfully reset, false otherwise</returns>
        public static bool ResetToDefaults()
        {
            var defaultConfig = CreateDefaultConfiguration();
            return UpdateConfiguration(defaultConfig);
        }

        /// <summary>
        /// Backs up current configuration
        /// </summary>
        /// <param name="backupPath">Path for backup file</param>
        /// <returns>True if successfully backed up, false otherwise</returns>
        public static bool BackupConfiguration(string backupPath = null)
        {
            try
            {
                backupPath =
                    backupPath ?? $"DevTools/config_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                var config = GetConfiguration();
                var json = SerializeConfiguration(config);

                var backupDir = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                File.WriteAllText(backupPath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Restores configuration from backup
        /// </summary>
        /// <param name="backupPath">Path to backup file</param>
        /// <returns>True if successfully restored, false otherwise</returns>
        public static bool RestoreConfiguration(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    return false;
                }

                var json = File.ReadAllText(backupPath, Encoding.UTF8);
                var config = DeserializeConfiguration(json);

                if (config != null && ValidateConfiguration(config))
                {
                    return UpdateConfiguration(config);
                }
            }
            catch (Exception)
            {
                // Ignore restore errors
            }

            return false;
        }

        /// <summary>
        /// Saves current configuration as a named profile
        /// </summary>
        /// <param name="profileName">Name of the profile</param>
        /// <returns>True if successfully saved, false otherwise</returns>
        public static bool SaveProfile(string profileName)
        {
            try
            {
                var config = GetConfiguration();
                var profilePath = $"DevTools/profiles/{profileName}.json";

                var profileDir = Path.GetDirectoryName(profilePath);
                if (!Directory.Exists(profileDir))
                {
                    Directory.CreateDirectory(profileDir);
                }

                var json = SerializeConfiguration(config);
                File.WriteAllText(profilePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads a named configuration profile
        /// </summary>
        /// <param name="profileName">Name of the profile to load</param>
        /// <returns>True if successfully loaded, false otherwise</returns>
        public static bool LoadProfile(string profileName)
        {
            try
            {
                var profilePath = $"DevTools/profiles/{profileName}.json";
                return RestoreConfiguration(profilePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a list of available configuration profiles
        /// </summary>
        /// <returns>List of profile names</returns>
        public static List<string> GetAvailableProfiles()
        {
            var profiles = new List<string>();

            try
            {
                var profileDir = "DevTools/profiles/";
                if (Directory.Exists(profileDir))
                {
                    var profileFiles = Directory.GetFiles(profileDir, "*.json");
                    foreach (var file in profileFiles)
                    {
                        profiles.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }

            return profiles;
        }

        /// <summary>
        /// Loads configuration from file
        /// </summary>
        private static void LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath, Encoding.UTF8);
                    _configuration = DeserializeConfiguration(json);
                    _lastConfigLoad = DateTime.Now;
                }

                if (_configuration == null)
                {
                    _configuration = CreateDefaultConfiguration();
                    SaveConfiguration();
                }
            }
            catch (Exception)
            {
                _configuration = CreateDefaultConfiguration();
            }
        }

        /// <summary>
        /// Saves configuration to file
        /// </summary>
        private static void SaveConfiguration()
        {
            try
            {
                var json = SerializeConfiguration(_configuration);

                var configDir = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                File.WriteAllText(ConfigFilePath, json, Encoding.UTF8);
                _lastConfigLoad = DateTime.Now;
            }
            catch (Exception)
            {
                // Ignore save errors
            }
        }

        /// <summary>
        /// Creates default configuration
        /// </summary>
        /// <returns>Default configuration</returns>
        private static DevToolsConfiguration CreateDefaultConfiguration()
        {
            return new DevToolsConfiguration
            {
                AutoUpdateDocumentation = true,
                AutoGenerateFileHeaders = true,
                AutoUpdateFileHeaders = true,
                EnableAIHooks = true,
                EnableTemplateEngine = true,
                DateFormat = "yyyy-MM-dd",
                DefaultAuthor = "TiggAdry",
                DefaultFramework = ".NET Framework 4.6.2",
                DefaultLanguage = "C# 7.3",
                MaxLogEntries = 1000,
                EnablePerformanceLogging = false,
                ExcludedDirectories = new List<string> { "bin", "obj", ".git", ".vs", "packages" },
                ExcludedFilePatterns = new List<string> { "*.tmp", "*.log", "*.cache" },
                CustomTemplates = new Dictionary<string, string>(),
                CustomSettings = new Dictionary<string, object>(),
                AIAssistantSettings = new AIAssistantSettings
                {
                    EnableAutomaticSuggestions = true,
                    EnableContextTracking = true,
                    AutoTriggerDocumentationUpdates = true,
                    MaxContextHistory = 50,
                    SuggestionThreshold = 0.8,
                },
                CodeGenerationSettings = new CodeGenerationSettings
                {
                    DefaultCommentStyle = CommentStyle.Explanatory,
                    GenerateXmlDocumentation = true,
                    GenerateInlineComments = false,
                    UseRegionsForOrganization = false,
                },
            };
        }

        /// <summary>
        /// Validates configuration
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool ValidateConfiguration(DevToolsConfiguration config)
        {
            if (config == null)
            {
                return false;
            }

            // Validate required fields
            if (
                string.IsNullOrEmpty(config.DateFormat)
                || string.IsNullOrEmpty(config.DefaultAuthor)
                || string.IsNullOrEmpty(config.DefaultFramework)
            )
            {
                return false;
            }

            // Validate date format
            try
            {
                DateTime.Now.ToString(config.DateFormat);
            }
            catch (Exception)
            {
                return false;
            }

            // Validate numeric ranges
            if (config.MaxLogEntries < 100 || config.MaxLogEntries > 10000)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Serializes configuration to JSON
        /// </summary>
        /// <param name="config">Configuration to serialize</param>
        /// <returns>JSON string</returns>
        private static string SerializeConfiguration(DevToolsConfiguration config)
        {
            // Simple JSON serialization without external dependencies
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine(
                $"  \"AutoUpdateDocumentation\": {config.AutoUpdateDocumentation.ToString().ToLower()},"
            );
            json.AppendLine(
                $"  \"AutoGenerateFileHeaders\": {config.AutoGenerateFileHeaders.ToString().ToLower()},"
            );
            json.AppendLine(
                $"  \"AutoUpdateFileHeaders\": {config.AutoUpdateFileHeaders.ToString().ToLower()},"
            );
            json.AppendLine($"  \"EnableAIHooks\": {config.EnableAIHooks.ToString().ToLower()},");
            json.AppendLine(
                $"  \"EnableTemplateEngine\": {config.EnableTemplateEngine.ToString().ToLower()},"
            );
            json.AppendLine($"  \"DateFormat\": \"{config.DateFormat}\",");
            json.AppendLine($"  \"DefaultAuthor\": \"{config.DefaultAuthor}\",");
            json.AppendLine($"  \"DefaultFramework\": \"{config.DefaultFramework}\",");
            json.AppendLine($"  \"DefaultLanguage\": \"{config.DefaultLanguage}\",");
            json.AppendLine($"  \"MaxLogEntries\": {config.MaxLogEntries},");
            json.AppendLine(
                $"  \"EnablePerformanceLogging\": {config.EnablePerformanceLogging.ToString().ToLower()}"
            );
            json.AppendLine("}");

            return json.ToString();
        }

        /// <summary>
        /// Deserializes configuration from JSON
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Configuration object</returns>
        private static DevToolsConfiguration DeserializeConfiguration(string json)
        {
            try
            {
                // Simple JSON deserialization without external dependencies
                var config = CreateDefaultConfiguration();

                // Parse key-value pairs (simplified JSON parsing)
                var lines = json.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Contains(":"))
                    {
                        var parts = trimmed.Split(':');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().Trim('"');
                            var value = parts[1].Trim().TrimEnd(',').Trim('"');

                            switch (key)
                            {
                                case "AutoUpdateDocumentation":
                                    config.AutoUpdateDocumentation = bool.Parse(value);
                                    break;
                                case "AutoGenerateFileHeaders":
                                    config.AutoGenerateFileHeaders = bool.Parse(value);
                                    break;
                                case "AutoUpdateFileHeaders":
                                    config.AutoUpdateFileHeaders = bool.Parse(value);
                                    break;
                                case "EnableAIHooks":
                                    config.EnableAIHooks = bool.Parse(value);
                                    break;
                                case "EnableTemplateEngine":
                                    config.EnableTemplateEngine = bool.Parse(value);
                                    break;
                                case "DateFormat":
                                    config.DateFormat = value;
                                    break;
                                case "DefaultAuthor":
                                    config.DefaultAuthor = value;
                                    break;
                                case "DefaultFramework":
                                    config.DefaultFramework = value;
                                    break;
                                case "DefaultLanguage":
                                    config.DefaultLanguage = value;
                                    break;
                                case "MaxLogEntries":
                                    config.MaxLogEntries = int.Parse(value);
                                    break;
                                case "EnablePerformanceLogging":
                                    config.EnablePerformanceLogging = bool.Parse(value);
                                    break;
                            }
                        }
                    }
                }

                return config;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// DevTools configuration settings
    /// </summary>
    public class DevToolsConfiguration
    {
        /// <summary>
        /// Whether to automatically update documentation
        /// </summary>
        public bool AutoUpdateDocumentation { get; set; } = true;

        /// <summary>
        /// Whether to automatically generate file headers
        /// </summary>
        public bool AutoGenerateFileHeaders { get; set; } = true;

        /// <summary>
        /// Whether to automatically update existing file headers
        /// </summary>
        public bool AutoUpdateFileHeaders { get; set; } = true;

        /// <summary>
        /// Whether to enable AI assistant hooks
        /// </summary>
        public bool EnableAIHooks { get; set; } = true;

        /// <summary>
        /// Whether to enable template engine
        /// </summary>
        public bool EnableTemplateEngine { get; set; } = true;

        /// <summary>
        /// Date format for file headers and documentation
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Default author name for file headers
        /// </summary>
        public string DefaultAuthor { get; set; } = "TiggAdry";

        /// <summary>
        /// Default framework version
        /// </summary>
        public string DefaultFramework { get; set; } = ".NET Framework 4.6.2";

        /// <summary>
        /// Default programming language
        /// </summary>
        public string DefaultLanguage { get; set; } = "C# 7.3";

        /// <summary>
        /// Maximum number of log entries to keep
        /// </summary>
        public int MaxLogEntries { get; set; } = 1000;

        /// <summary>
        /// Whether to enable performance logging
        /// </summary>
        public bool EnablePerformanceLogging { get; set; } = false;

        /// <summary>
        /// Directories to exclude from automation
        /// </summary>
        public List<string> ExcludedDirectories { get; set; } = new List<string>();

        /// <summary>
        /// File patterns to exclude from automation
        /// </summary>
        public List<string> ExcludedFilePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Custom templates
        /// </summary>
        public Dictionary<string, string> CustomTemplates { get; set; } =
            new Dictionary<string, string>();

        /// <summary>
        /// Custom settings
        /// </summary>
        public Dictionary<string, object> CustomSettings { get; set; } =
            new Dictionary<string, object>();

        /// <summary>
        /// AI assistant specific settings
        /// </summary>
        public AIAssistantSettings AIAssistantSettings { get; set; } = new AIAssistantSettings();

        /// <summary>
        /// Code generation specific settings
        /// </summary>
        public CodeGenerationSettings CodeGenerationSettings { get; set; } =
            new CodeGenerationSettings();
    }

    /// <summary>
    /// AI Assistant specific configuration settings
    /// </summary>
    public class AIAssistantSettings
    {
        /// <summary>
        /// Whether to enable automatic suggestions
        /// </summary>
        public bool EnableAutomaticSuggestions { get; set; } = true;

        /// <summary>
        /// Whether to enable context tracking
        /// </summary>
        public bool EnableContextTracking { get; set; } = true;

        /// <summary>
        /// Whether to automatically trigger documentation updates
        /// </summary>
        public bool AutoTriggerDocumentationUpdates { get; set; } = true;

        /// <summary>
        /// Maximum number of context history entries
        /// </summary>
        public int MaxContextHistory { get; set; } = 50;

        /// <summary>
        /// Suggestion confidence threshold
        /// </summary>
        public double SuggestionThreshold { get; set; } = 0.8;
    }

    /// <summary>
    /// Code generation specific configuration settings
    /// </summary>
    public class CodeGenerationSettings
    {
        /// <summary>
        /// Default comment style for code generation
        /// </summary>
        public CommentStyle DefaultCommentStyle { get; set; } = CommentStyle.Explanatory;

        /// <summary>
        /// Whether to generate XML documentation
        /// </summary>
        public bool GenerateXmlDocumentation { get; set; } = true;

        /// <summary>
        /// Whether to generate inline comments
        /// </summary>
        public bool GenerateInlineComments { get; set; } = false;

        /// <summary>
        /// Whether to use regions for code organization
        /// </summary>
        public bool UseRegionsForOrganization { get; set; } = false;
    }
}
