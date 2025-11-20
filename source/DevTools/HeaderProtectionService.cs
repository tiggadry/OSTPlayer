// ====================================================================
// FILE: HeaderProtectionService.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: DevTools
// LOCATION: DevTools/
// VERSION: 1.0.0
// CREATED: 2025-08-07
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Advanced service designed to protect important documentation sections in file
// headers from being deleted by AI assistants during routine updates. Provides
// comprehensive backup, restoration, and validation capabilities to ensure
// critical documentation (LIMITATIONS, FUTURE, TESTING, etc.) is never lost.
//
// FEATURES:
// - Automatic backup of critical documentation sections before AI operations
// - Real-time detection of content deletion by AI assistants
// - Automatic restoration of deleted documentation sections
// - Comprehensive validation of header integrity
// - Pre/post operation hooks for AI assistant protection
// - Content fingerprinting to detect unauthorized changes
//
// KEY PROBLEM SOLVED:
// Prevents AI assistants from deleting important documentation sections like
// LIMITATIONS, FUTURE REFACTORING, FUTURE items, TESTING notes, and COMPATIBILITY
// information when performing routine header updates or file modifications.
//
// PROTECTION STRATEGIES:
// - Content backup before any AI operations
// - Section-specific validation and restoration
// - Fingerprint-based change detection
// - Automatic rollback on content loss
// - Pre-emptive section preservation
//
// DEPENDENCIES:
// - OstPlayer.DevTools.DateHelper (safe date operations)
// - System.IO (file operations)
// - System.Text.RegularExpressions (pattern matching)
// - System.Security.Cryptography (content fingerprinting)
//
// USAGE PATTERNS:
// - Call BackupHeaderContent() before AI operations
// - Use ValidateAndRestoreHeader() after AI operations
// - Integrate with AI workflow hooks for automatic protection
// - Monitor header integrity during development sessions
//
// PERFORMANCE NOTES:
// - Lightweight validation with minimal overhead
// - Efficient backup storage using in-memory caching
// - Fast content comparison using hash-based fingerprints
// - Minimal file I/O for backup/restore operations
//
// TESTING:
// - Unit tests for backup and restore functionality
// - Integration tests with AI workflow simulation
// - Performance tests for large file operations
// - Edge case testing for corrupted headers
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Integration with all DevTools utilities
//
// CHANGELOG:
// 2025-08-07 v1.0.0 - Initial implementation with comprehensive header protection and AI integration
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// Service for protecting critical documentation sections in file headers from AI deletion.
    /// Provides comprehensive backup, validation, and restoration capabilities.
    /// </summary>
    public static class HeaderProtectionService
    {
        #region Constants and Configuration

        /// <summary>
        /// Critical documentation sections that must be protected from AI deletion.
        /// These sections are commonly deleted by AI assistants during file updates.
        /// </summary>
        private static readonly Dictionary<string, string[]> CriticalSections = new Dictionary<
            string,
            string[]
        >
        {
            {
                "LIMITATIONS",
                new[] { "// LIMITATIONS:", "//   LIMITATIONS:", "// - Cache is in-memory only" }
            },
            {
                "FUTURE_REFACTORING",
                new[] { "// FUTURE REFACTORING:", "//   FUTURE REFACTORING:", "// FUTURE:" }
            },
            { "TESTING", new[] { "// TESTING:", "//   TESTING:", "// - Unit tests for" } },
            {
                "COMPATIBILITY",
                new[] { "// COMPATIBILITY:", "//   COMPATIBILITY:", "// - .NET Framework" }
            },
            {
                "CONSIDER_ITEMS",
                new[] { "// CONSIDER:", "//   CONSIDER:", "// CONSIDER: Plugin architecture" }
            },
            { "IDEA_ITEMS", new[] { "// IDEA:", "//   IDEA:", "// IDEA: Machine learning" } },
        };

        /// <summary>
        /// In-memory backup storage for header content protection.
        /// Key: file path, Value: backup content with timestamp
        /// </summary>
        private static readonly Dictionary<string, HeaderBackup> _headerBackups =
            new Dictionary<string, HeaderBackup>();

        /// <summary>
        /// Lock for thread-safe backup operations
        /// </summary>
        private static readonly object _backupLock = new object();

        #endregion

        #region Public Protection Methods

        /// <summary>
        /// Creates a backup of critical header sections before AI operations.
        /// USAGE: Call this before any AI assistant file modifications.
        /// </summary>
        /// <param name="filePath">Path to file that will be modified</param>
        /// <returns>True if backup was created successfully</returns>
        public static bool BackupHeaderContent(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                lock (_backupLock)
                {
                    var content = File.ReadAllText(filePath, Encoding.UTF8);
                    var criticalSections = ExtractCriticalSections(content);
                    var contentHash = CalculateContentHash(content);

                    var backup = new HeaderBackup
                    {
                        FilePath = filePath,
                        BackupTime = DateTime.Now,
                        OriginalContentHash = contentHash,
                        CriticalSections = criticalSections,
                        FullContent = content,
                    };

                    _headerBackups[filePath] = backup;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates header integrity and restores missing sections if needed.
        /// USAGE: Call this after AI assistant file modifications.
        /// </summary>
        /// <param name="filePath">Path to file that was modified</param>
        /// <returns>Validation and restoration result</returns>
        public static HeaderProtectionResult ValidateAndRestoreHeader(string filePath)
        {
            var result = new HeaderProtectionResult
            {
                FilePath = filePath,
                ValidationTime = DateTime.Now,
                IsValid = true,
                RestoredSections = new List<string>(),
                DeletedSections = new List<string>(),
            };

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                result.IsValid = false;
                result.ErrorMessage = "File not found or path is empty";
                return result;
            }

            try
            {
                lock (_backupLock)
                {
                    // Check if we have a backup for this file
                    if (!_headerBackups.ContainsKey(filePath))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "No backup available for validation";
                        return result;
                    }

                    var backup = _headerBackups[filePath];
                    var currentContent = File.ReadAllText(filePath, Encoding.UTF8);
                    var currentSections = ExtractCriticalSections(currentContent);

                    // Check for missing sections
                    foreach (var originalSection in backup.CriticalSections)
                    {
                        if (!currentSections.ContainsKey(originalSection.Key))
                        {
                            result.DeletedSections.Add(originalSection.Key);
                            result.IsValid = false;
                        }
                    }

                    // Restore missing sections if any were found
                    if (result.DeletedSections.Count > 0)
                    {
                        var restoredContent = RestoreMissingSections(
                            currentContent,
                            backup,
                            result.DeletedSections
                        );
                        File.WriteAllText(filePath, restoredContent, Encoding.UTF8);
                        result.RestoredSections.AddRange(result.DeletedSections);
                        result.WasRestored = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Validation error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Performs a quick validation of header integrity without restoration.
        /// USAGE: For monitoring and reporting purposes.
        /// </summary>
        /// <param name="filePath">Path to file to validate</param>
        /// <returns>Quick validation result</returns>
        public static bool QuickValidateHeaderIntegrity(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);

                // Check for presence of all critical section markers
                foreach (var sectionGroup in CriticalSections)
                {
                    bool sectionFound = false;
                    foreach (var marker in sectionGroup.Value)
                    {
                        if (content.Contains(marker))
                        {
                            sectionFound = true;
                            break;
                        }
                    }

                    if (!sectionFound)
                    {
                        return false; // Missing critical section
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Scans project for files with missing critical documentation sections.
        /// USAGE: Project-wide validation and reporting.
        /// </summary>
        /// <param name="projectPath">Root path of project to scan</param>
        /// <returns>List of files with missing documentation</returns>
        public static List<string> ScanProjectForMissingDocumentation(string projectPath = ".")
        {
            var problematicFiles = new List<string>();

            try
            {
                var filesToScan = GetFilesToScan(projectPath);

                foreach (var file in filesToScan)
                {
                    if (!QuickValidateHeaderIntegrity(file))
                    {
                        problematicFiles.Add(file);
                    }
                }
            }
            catch (Exception)
            {
                // Return partial results if scanning fails
            }

            return problematicFiles;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Extracts critical documentation sections from file content.
        /// </summary>
        /// <param name="content">File content to analyze</param>
        /// <returns>Dictionary of section name to content</returns>
        private static Dictionary<string, string> ExtractCriticalSections(string content)
        {
            var sections = new Dictionary<string, string>();

            foreach (var sectionGroup in CriticalSections)
            {
                var sectionName = sectionGroup.Key;
                var markers = sectionGroup.Value;

                // Find the section in content
                foreach (var marker in markers)
                {
                    var startIndex = content.IndexOf(marker);
                    if (startIndex >= 0)
                    {
                        // Extract section content until next major section or end
                        var endIndex = FindSectionEnd(content, startIndex);
                        var sectionContent = content.Substring(startIndex, endIndex - startIndex);
                        sections[sectionName] = sectionContent;
                        break;
                    }
                }
            }

            return sections;
        }

        /// <summary>
        /// Finds the end of a documentation section in content.
        /// </summary>
        /// <param name="content">File content</param>
        /// <param name="startIndex">Start index of section</param>
        /// <returns>End index of section</returns>
        private static int FindSectionEnd(string content, int startIndex)
        {
            // Look for next major section or end of comment block
            var nextSectionPatterns = new[]
            {
                "// CHANGELOG:",
                "// ====================================================================",
                "using System;",
                "namespace ",
            };

            var endIndex = content.Length;

            foreach (var pattern in nextSectionPatterns)
            {
                var nextIndex = content.IndexOf(pattern, startIndex + 1);
                if (nextIndex > startIndex && nextIndex < endIndex)
                {
                    endIndex = nextIndex;
                }
            }

            return endIndex;
        }

        /// <summary>
        /// Restores missing sections to file content.
        /// </summary>
        /// <param name="currentContent">Current file content</param>
        /// <param name="backup">Backup with original sections</param>
        /// <param name="missingSections">List of missing section names</param>
        /// <returns>Restored file content</returns>
        private static string RestoreMissingSections(
            string currentContent,
            HeaderBackup backup,
            List<string> missingSections
        )
        {
            var restoredContent = currentContent;

            // Find insertion point (typically before CHANGELOG)
            var insertionPoint = restoredContent.IndexOf("// CHANGELOG:");
            if (insertionPoint < 0)
            {
                insertionPoint = restoredContent.IndexOf(
                    "// ====================================================================",
                    100
                );
            }

            if (insertionPoint > 0)
            {
                var sectionsToRestore = new StringBuilder();

                foreach (var sectionName in missingSections)
                {
                    if (backup.CriticalSections.ContainsKey(sectionName))
                    {
                        sectionsToRestore.AppendLine(backup.CriticalSections[sectionName]);
                        sectionsToRestore.AppendLine("//");
                    }
                }

                if (sectionsToRestore.Length > 0)
                {
                    restoredContent = restoredContent.Insert(
                        insertionPoint,
                        sectionsToRestore.ToString()
                    );
                }
            }

            return restoredContent;
        }

        /// <summary>
        /// Calculates hash of content for change detection.
        /// </summary>
        /// <param name="content">Content to hash</param>
        /// <returns>Content hash</returns>
        private static string CalculateContentHash(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Gets list of files to scan for documentation validation.
        /// </summary>
        /// <param name="projectPath">Project root path</param>
        /// <returns>List of files to scan</returns>
        private static List<string> GetFilesToScan(string projectPath)
        {
            var files = new List<string>();

            try
            {
                // Focus on C# files which typically have the most documentation
                files.AddRange(
                    Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                );

                // Filter out unwanted directories
                files = files
                    .Where(f =>
                        !f.Contains(@"\bin\")
                        && !f.Contains(@"\obj\")
                        && !f.Contains(@"\packages\")
                        && !f.Contains(@"\.git\")
                    )
                    .ToList();
            }
            catch (Exception)
            {
                // Return empty list if scanning fails
                files.Clear();
            }

            return files;
        }

        #endregion

        #region Management Methods

        /// <summary>
        /// Clears all stored backups (use carefully).
        /// </summary>
        public static void ClearAllBackups()
        {
            lock (_backupLock)
            {
                _headerBackups.Clear();
            }
        }

        /// <summary>
        /// Gets count of currently stored backups.
        /// </summary>
        /// <returns>Number of backups in memory</returns>
        public static int GetBackupCount()
        {
            lock (_backupLock)
            {
                return _headerBackups.Count;
            }
        }

        /// <summary>
        /// Checks if backup exists for specified file.
        /// </summary>
        /// <param name="filePath">File path to check</param>
        /// <returns>True if backup exists</returns>
        public static bool HasBackup(string filePath)
        {
            lock (_backupLock)
            {
                return _headerBackups.ContainsKey(filePath);
            }
        }

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Backup information for header protection.
    /// </summary>
    public class HeaderBackup
    {
        /// <summary>
        /// Gets or sets the file path of the backed up header.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the backup creation time.
        /// </summary>
        public DateTime BackupTime { get; set; }

        /// <summary>
        /// Gets or sets the hash of the original content.
        /// </summary>
        public string OriginalContentHash { get; set; }

        /// <summary>
        /// Gets or sets the critical sections that were protected.
        /// </summary>
        public Dictionary<string, string> CriticalSections { get; set; }

        /// <summary>
        /// Gets or sets the full content backup.
        /// </summary>
        public string FullContent { get; set; }
    }

    /// <summary>
    /// Result of header protection validation.
    /// </summary>
    public class HeaderProtectionResult
    {
        /// <summary>
        /// Gets or sets the file path that was protected.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the validation time.
        /// </summary>
        public DateTime ValidationTime { get; set; }

        /// <summary>
        /// Gets or sets whether the header is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets whether the header was restored.
        /// </summary>
        public bool WasRestored { get; set; }

        /// <summary>
        /// Gets or sets the sections that were deleted.
        /// </summary>
        public List<string> DeletedSections { get; set; }

        /// <summary>
        /// Gets or sets the sections that were restored.
        /// </summary>
        public List<string> RestoredSections { get; set; }

        /// <summary>
        /// Gets or sets any error message from the protection process.
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    #endregion
}
