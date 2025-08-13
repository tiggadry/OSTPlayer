// ====================================================================
// FILE: DateHelper.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: DevTools
// LOCATION: DevTools/
// VERSION: 1.3.0
// CREATED: 2025-08-07
// UPDATED: 2025-08-09
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Enhanced development utility class for standardized date formatting and file manipulation
// across the OstPlayer project. This tool is designed for AI assistants and development
// automation, providing consistent ISO 8601 date formatting for documentation, file headers,
// changelog entries, and automated file updates.
//
// FEATURES:
// - ISO 8601 date formatting (YYYY-MM-DD)
// - Standardized header date formatting
// - Changelog entry formatting utilities
// - Automated file header updates
// - Project-wide date validation
// - File manipulation with date management
// - **PHASE 5**: Enhanced with header protection against AI content deletion
// - **PHASE 5**: Safe update methods that preserve documentation sections
//
// DEPENDENCIES:
// - System namespace for DateTime operations
// - System.IO for file operations
// - System.Text for text manipulation
// - System.Collections.Generic for collections
//
// UTILITY FUNCTIONS:
// - GetCurrentDateString() - Basic ISO 8601 date string
// - GetCurrentDateForHeader() - Formatted for file headers
// - GetChangelogHeader() - Formatted for changelog versions
// - GetChangelogEntry() - Formatted for changelog entries
// - UpdateFileHeader() - Automated file header updates
// - AddChangelogEntry() - Automated changelog entry addition
// - ValidateAllDatesInProject() - Project-wide date validation
//
// PERFORMANCE NOTES:
// - Uses DateTime.Now for current date retrieval
// - String formatting operations are lightweight
// - Static methods for performance and simplicity
// - File I/O operations are optimized for small files
//
// FUTURE REFACTORING:
// TODO: Add timezone support for international development
// TODO: Add custom date format options
// TODO: Add validation for date ranges
// TODO: Implement backup functionality for file updates
// CONSIDER: Add localization support for different date formats
// CONSIDER: Add async file operations for large projects
//
// TESTING:
// - Unit tests for date format validation
// - Tests for string formatting accuracy
// - Validation of ISO 8601 compliance
// - File manipulation testing
// - Project validation testing
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
//
// CHANGELOG:
// 2025-08-09 v1.3.0 - Phase 5 DI Implementation: Enhanced with header protection and safe update methods
// 2025-08-07 v1.2.1 - Moved to DevTools module for better organization
// 2025-08-07 v1.2.0 - Enhanced with file manipulation and validation capabilities
// 2025-08-07 v1.1.2 - Initial implementation for date management system
// ====================================================================

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// Enhanced development utility for generating current date in standardized format and managing file dates
    /// </summary>
    public static class DateHelper
    {
        // Constants for file header patterns
        private static readonly Regex UpdatedDateRegex = new Regex(@"// UPDATED: (\d{4}-\d{2}-\d{2})", RegexOptions.Compiled);
        private static readonly Regex ChangelogRegex = new Regex(@"// CHANGELOG:", RegexOptions.Compiled);
        private static readonly Regex VersionRegex = new Regex(@"// VERSION: (.+)", RegexOptions.Compiled);

        #region Basic Date Formatting

        /// <summary>
        /// Gets current date in ISO 8601 format (YYYY-MM-DD)
        /// </summary>
        /// <returns>Current date as string in YYYY-MM-DD format</returns>
        public static string GetCurrentDateString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Gets current date formatted for file headers
        /// </summary>
        /// <returns>Current date string for use in file headers</returns>
        public static string GetCurrentDateForHeader()
        {
            return $"// UPDATED: {GetCurrentDateString()}";
        }

        /// <summary>
        /// Gets current date formatted for changelog entries
        /// </summary>
        /// <param name="version">Version number</param>
        /// <returns>Formatted changelog header</returns>
        public static string GetChangelogHeader(string version)
        {
            return $"## [{version}] - {GetCurrentDateString()}";
        }

        /// <summary>
        /// Gets current date formatted for changelog entry in file headers
        /// </summary>
        /// <param name="version">Version number</param>
        /// <param name="description">Description of changes</param>
        /// <returns>Formatted changelog entry</returns>
        public static string GetChangelogEntry(string version, string description)
        {
            return $"{GetCurrentDateString()} v{version} - {description}";
        }

        #endregion

        #region File Manipulation

        /// <summary>
        /// Updates the UPDATED date in a file header
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateFileHeader(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var updatedContent = UpdatedDateRegex.Replace(content, GetCurrentDateForHeader());

                if (content != updatedContent)
                {
                    File.WriteAllText(filePath, updatedContent, Encoding.UTF8);
                    return true;
                }

                return false; // No changes needed
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a changelog entry to a file header
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <param name="version">Version number</param>
        /// <param name="description">Description of changes</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public static bool AddChangelogEntry(string filePath, string version, string description)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var changelogEntry = GetChangelogEntry(version, description);
                
                var changelogMatch = ChangelogRegex.Match(content);
                if (changelogMatch.Success)
                {
                    var insertIndex = changelogMatch.Index + changelogMatch.Length;
                    var updatedContent = content.Insert(insertIndex, $"\n{changelogEntry}");
                    
                    File.WriteAllText(filePath, updatedContent, Encoding.UTF8);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Project Validation

        /// <summary>
        /// Validates all dates in the project for consistency
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <returns>True if all dates are valid and consistent, false otherwise</returns>
        public static bool ValidateAllDatesInProject(string projectPath = ".")
        {
            try
            {
                var issues = new List<string>();
                var files = GetProjectFiles(projectPath);

                foreach (var file in files)
                {
                    ValidateFileDate(file, issues);
                }

                return issues.Count == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a list of project files to validate
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <returns>List of file paths to validate</returns>
        private static List<string> GetProjectFiles(string projectPath)
        {
            var files = new List<string>();
            
            // Add C# files
            files.AddRange(Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories));
            
            // Add documentation files
            files.AddRange(Directory.GetFiles(projectPath, "*.md", SearchOption.AllDirectories));
            
            // Add PowerShell scripts
            files.AddRange(Directory.GetFiles(projectPath, "*.ps1", SearchOption.AllDirectories));

            return files;
        }

        /// <summary>
        /// Validates dates in a specific file
        /// </summary>
        /// <param name="filePath">Path to the file to validate</param>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateFileDate(string filePath, List<string> issues)
        {
            try
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                
                // Check for valid date format in UPDATED field
                var updatedMatch = UpdatedDateRegex.Match(content);
                if (updatedMatch.Success)
                {
                    var dateString = updatedMatch.Groups[1].Value;
                    if (!IsValidDateFormat(dateString))
                    {
                        issues.Add($"{filePath}: Invalid date format in UPDATED field: {dateString}");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"{filePath}: Error validating file - {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if a string is in correct ISO 8601 date format
        /// </summary>
        /// <param name="dateString">Date string to validate</param>
        /// <returns>True if valid format, false otherwise</returns>
        private static bool IsValidDateFormat(string dateString)
        {
            return DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, 
                System.Globalization.DateTimeStyles.None, out _);
        }

        #endregion

        #region Enhanced Date Validation

        /// <summary>
        /// Gets current date with validation to ensure it's safe for AI usage.
        /// ENHANCEMENT: Integrates with DateValidationService to prevent AI date errors.
        /// </summary>
        /// <returns>Validated current date string</returns>
        public static string GetValidatedCurrentDateString()
        {
            return DateValidationService.GetValidatedCurrentDate();
        }

        /// <summary>
        /// Updates file header with validated current date.
        /// ENHANCEMENT: Uses validation service to prevent incorrect date usage.
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <param name="operationType">Type of operation for validation context</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateFileHeaderWithValidation(string filePath, AIOperationType operationType = AIOperationType.RoutineUpdate)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                // Get validated current date
                var validatedDate = DateValidationService.GetValidatedCurrentDate();
                var validationResult = DateValidationService.ValidateForAI(validatedDate, operationType);
                
                if (!validationResult.IsValid)
                {
                    // Log validation error but don't fail the operation
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var updatedContent = UpdatedDateRegex.Replace(content, $"// UPDATED: {validatedDate}");

                if (content != updatedContent)
                {
                    File.WriteAllText(filePath, updatedContent, Encoding.UTF8);
                    return true;
                }

                return false; // No changes needed
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds changelog entry with validated date.
        /// ENHANCEMENT: Prevents AI from using incorrect dates in changelog entries.
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <param name="version">Version number</param>
        /// <param name="description">Description of changes</param>
        /// <param name="operationType">Type of operation for validation</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public static bool AddValidatedChangelogEntry(string filePath, string version, string description, AIOperationType operationType = AIOperationType.VersionRelease)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                // Get validated date
                var validatedDate = DateValidationService.GetValidatedCurrentDate();
                var validationResult = DateValidationService.ValidateForAI(validatedDate, operationType);
                
                if (!validationResult.IsValid)
                {
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var changelogEntry = $"{validatedDate} v{version} - {description}";
                
                var changelogMatch = ChangelogRegex.Match(content);
                if (changelogMatch.Success)
                {
                    var insertIndex = changelogMatch.Index + changelogMatch.Length;
                    var updatedContent = content.Insert(insertIndex, $"\n{changelogEntry}");
                    
                    File.WriteAllText(filePath, updatedContent, Encoding.UTF8);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Protected Header Updates

        /// <summary>
        /// Updates only the UPDATED date in file header while preserving all other content.
        /// PROTECTION: Specifically designed to prevent AI from deleting important documentation.
        /// SAFETY: Only modifies the specific UPDATED line, leaves everything else intact.
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <param name="targetDate">Specific date to use (if null, uses current date)</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateOnlyDateInHeader(string filePath, string targetDate = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var originalContent = content;
                
                // Use provided date or get validated current date
                var dateToUse = targetDate ?? DateValidationService.GetValidatedCurrentDate();
                
                // CRITICAL: Only replace the UPDATED line, preserve everything else
                var updatedLine = $"// UPDATED: {dateToUse}";
                var newContent = UpdatedDateRegex.Replace(content, updatedLine);

                // Only write if something actually changed
                if (newContent != originalContent)
                {
                    File.WriteAllText(filePath, newContent, Encoding.UTF8);
                    return true;
                }

                return false; // No changes needed
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Safely adds changelog entry without affecting any other content.
        /// PROTECTION: Preserves ALL existing content, only inserts new changelog line.
        /// SAFETY: Uses precise insertion point, doesn't rewrite the file.
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <param name="version">Version number</param>
        /// <param name="description">Description of changes</param>
        /// <param name="targetDate">Specific date to use (if null, uses current date)</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public static bool SafelyAddChangelogEntry(string filePath, string version, string description, string targetDate = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                
                // Use provided date or get validated current date
                var dateToUse = targetDate ?? DateValidationService.GetValidatedCurrentDate();
                var changelogEntry = $"{dateToUse} v{version} - {description}";
                
                var changelogMatch = ChangelogRegex.Match(content);
                if (changelogMatch.Success)
                {
                    var insertIndex = changelogMatch.Index + changelogMatch.Length;
                    var updatedContent = content.Insert(insertIndex, $"\n{changelogEntry}");
                    
                    File.WriteAllText(filePath, updatedContent, Encoding.UTF8);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that file header contains all required documentation sections.
        /// DETECTION: Identifies if AI has deleted important documentation sections.
        /// PROTECTION: Can be used to verify header integrity before/after updates.
        /// </summary>
        /// <param name="filePath">Path to the file to check</param>
        /// <returns>Validation result with missing sections identified</returns>
        public static HeaderValidationResult ValidateHeaderIntegrity(string filePath)
        {
            var result = new HeaderValidationResult
            {
                FilePath = filePath,
                IsValid = true,
                MissingSections = new List<string>()
            };

            try
            {
                if (!File.Exists(filePath))
                {
                    result.IsValid = false;
                    result.MissingSections.Add("FILE_NOT_FOUND");
                    return result;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                
                // Check for critical documentation sections
                var requiredSections = new Dictionary<string, string>
                {
                    {"LIMITATIONS", "// LIMITATIONS:"},
                    {"FUTURE_REFACTORING", "// FUTURE REFACTORING:"},
                    {"TODO_ITEMS", "// TODO:"},
                    {"TESTING", "// TESTING:"},
                    {"COMPATIBILITY", "// COMPATIBILITY:"},
                    {"CONSIDER_ITEMS", "// CONSIDER:"},
                    {"IDEA_ITEMS", "// IDEA:"}
                };

                foreach (var section in requiredSections)
                {
                    if (!content.Contains(section.Value))
                    {
                        result.MissingSections.Add(section.Key);
                        result.IsValid = false;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.MissingSections.Add($"VALIDATION_ERROR: {ex.Message}");
                return result;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets version from file header
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Version string if found, null otherwise</returns>
        public static string GetFileVersion(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var versionMatch = VersionRegex.Match(content);
                
                return versionMatch.Success ? versionMatch.Groups[1].Value.Trim() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a file has a proper header structure
        /// </summary>
        /// <param name="filePath">Path to the file to check</param>
        /// <returns>True if file has proper header, false otherwise</returns>
        public static bool HasValidHeader(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                
                // Check for basic header elements
                return content.Contains("// FILE:") && 
                       content.Contains("// PROJECT:") && 
                       content.Contains("// VERSION:") &&
                       content.Contains("// UPDATED:");
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}

#region Supporting Types for Header Protection

/// <summary>
/// Result of header integrity validation to detect AI content deletion.
/// </summary>
public class HeaderValidationResult
{
    public string FilePath { get; set; }
    public bool IsValid { get; set; }
    public List<string> MissingSections { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets human-readable description of validation issues.
    /// </summary>
    public string GetValidationSummary()
    {
        if (IsValid)
        {
            return "Header integrity validated - all sections present";
        }
        
        return $"Header validation failed - missing sections: {string.Join(", ", MissingSections)}";
    }
}

#endregion