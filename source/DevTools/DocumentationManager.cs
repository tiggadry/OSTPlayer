// ====================================================================
// FILE: DocumentationManager.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: DevTools
// LOCATION: DevTools
// VERSION: 1.3.0
// CREATED: 2025-08-07
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Centralized development documentation management utility for the OstPlayer project.
// This tool is designed for AI assistants and development automation, providing
// automated documentation updates, consistency validation, and intelligent module
// summary management. Now includes specialized README management for enhanced
// navigation and organization.
//
// FEATURES:
// - Automated module summary updates
// - Main changelog management
// - Documentation consistency validation
// - Cross-reference path management
// - Template-based documentation generation
// - **NEW v1.3.0**: Specialized README management with intelligent hierarchy detection
// - **NEW v1.3.0**: Navigation README auto-updates with cross-reference synchronization
// - **NEW v1.3.0**: Technical README management for module documentation
//
// DEPENDENCIES:
// - System.IO for file operations
// - System.Text for text manipulation
// - System.Collections.Generic for collections
// - System.Linq for LINQ operations
// - OstPlayer.DevTools.DateHelper for date management
//
// DESIGN PATTERNS:
// - Static utility class pattern
// - Template method pattern for documentation updates
// - Strategy pattern for different documentation types
// - **NEW**: Factory pattern for README type detection
// - **NEW**: Observer pattern for cascade README updates
//
// PERFORMANCE NOTES:
// - Efficient file I/O with minimal memory allocation
// - Cached template loading for repeated operations
// - Batch operations for multiple file updates
// - **NEW**: Intelligent README hierarchy traversal
//
// LIMITATIONS:
// - Requires proper file structure and naming conventions
// - Limited to predefined documentation templates
// - No real-time change detection
//
// FUTURE REFACTORING:
// FUTURE: Add async operations for large documentation sets
// FUTURE: Implement change detection and incremental updates
// FUTURE: Add plugin architecture for custom documentation types
// FUTURE: Implement automatic backup functionality
// FUTURE: Add real-time README synchronization
// CONSIDER: Add documentation metrics and reporting
// CONSIDER: Integration with version control systems
//
// TESTING:
// - Unit tests for each documentation operation
// - Integration tests with actual project files
// - Performance tests for large documentation sets
// - Validation tests for consistency checking
// - **NEW**: README hierarchy validation tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
//
// CHANGELOG:
// 2025-08-07 v1.3.0 - Enhanced README management: Added specialized README-specific methods, intelligent hierarchy detection, navigation README auto-updates
// 2025-08-07 v1.2.1 - Moved to DevTools module and updated DateHelper reference
// 2025-08-07 v1.2.0 - Initial implementation for enhanced automation utilities
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// README type enumeration for specialized handling
    /// </summary>
    public enum ReadmeType
    {
        /// <summary>
        /// Documentation/*/README.md - Navigation and index files
        /// </summary>
        Navigation,

        /// <summary>
        /// Module/README.md - Technical module documentation
        /// </summary>
        Technical,

        /// <summary>
        /// Documentation/SubCategory/README.md - Category overviews
        /// </summary>
        Category,

        /// <summary>
        /// Documentation/README.md - Main project navigation
        /// </summary>
        Root,
    }

    /// <summary>
    /// Centralized development documentation management utility with enhanced README management
    /// </summary>
    public static class DocumentationManager
    {
        // Documentation file paths
        private const string DocumentationFolder = "Documentation";
        private const string ChangelogFile = "Documentation/Core/CHANGELOG.md";
        private const string TemplatesFile = "Documentation/FileHeaderTemplates.md";

        // README patterns for different types
        private static readonly Dictionary<ReadmeType, string[]> ReadmePatterns = new Dictionary<
            ReadmeType,
            string[]
        >
        {
            [ReadmeType.Root] = new[] { "Documentation/README.md" },
            [ReadmeType.Navigation] = new[]
            {
                "Documentation/*/README.md",
                "Documentation/Modules/README.md",
                "Documentation/Development/README.md",
                "Documentation/AI-Assistant/README.md",
                "Documentation/Archive/README.md",
            },
            [ReadmeType.Technical] = new[]
            {
                "ViewModels/README.md",
                "DevTools/README.md",
                "Utils/README.md",
                "Services/README.md",
                "Models/README.md",
            },
            [ReadmeType.Category] = new[]
            {
                "Documentation/Development/*/README.md",
                "Documentation/Modules/*/README.md",
            },
        };

        // Regex patterns for documentation parsing
        private static readonly Regex VersionHeaderRegex = new Regex(
            @"## \[([^\]]+)\] - (\d{4}-\d{2}-\d{2})",
            RegexOptions.Compiled
        );
        private static readonly Regex ModuleSummaryRegex = new Regex(
            @"# (.+) - (.+)",
            RegexOptions.Compiled
        );
        private static readonly Regex ReadmeQuickNavRegex = new Regex(
            @"\| \*\*([^*]+)\*\* \| ([^|]+) \| ([^|]+) \|",
            RegexOptions.Compiled
        );

        #region Module Summary Management

        /// <summary>
        /// Updates a module summary file with new changes
        /// </summary>
        /// <param name="moduleName">Name of the module (e.g., "Utils", "ViewModels")</param>
        /// <param name="changes">Description of changes made</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateModuleSummary(string moduleName, string changes)
        {
            try
            {
                var summaryFile = FindModuleSummaryFile(moduleName);
                if (summaryFile == null)
                {
                    var success = CreateModuleSummary(moduleName, changes);
                    if (success)
                    {
                        // Auto-update navigation README when new module summary is created
                        UpdateNavigationReadme(
                            "Modules",
                            $"Added {moduleName}ModuleUpdateSummary.md"
                        );
                    }
                    return success;
                }

                return AppendToModuleSummary(summaryFile, changes);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the module summary file for a given module
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>Path to the summary file or null if not found</returns>
        private static string FindModuleSummaryFile(string moduleName)
        {
            var patterns = new[]
            {
                $"Documentation/Modules/{moduleName}ModuleUpdateSummary.md",
                $"Documentation/{moduleName}ModuleUpdateSummary.md",
                $"Documentation/{moduleName}ModuleSummary.md",
                $"{moduleName}/README.md",
                $"Documentation/{moduleName}Summary.md",
            };

            return patterns.FirstOrDefault(File.Exists);
        }

        /// <summary>
        /// Creates a new module summary file
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="changes">Initial changes description</param>
        /// <returns>True if successfully created, false otherwise</returns>
        private static bool CreateModuleSummary(string moduleName, string changes)
        {
            try
            {
                var summaryPath = $"Documentation/Modules/{moduleName}ModuleUpdateSummary.md";
                var currentDate = DateHelper.GetCurrentDateString();

                var content =
                    $@"# OstPlayer {moduleName} Module - Update Summary

## ?? Module Overview

This document tracks updates and changes to the {moduleName} module of the OstPlayer project.

## ?? Recent Updates

### {currentDate}
- {changes}

## ?? Module Status

- **Last Updated**: {currentDate}
- **Status**: Active Development
- **Stability**: Stable

---

**This file is automatically maintained by DocumentationManager utility.**
";

                Directory.CreateDirectory(Path.GetDirectoryName(summaryPath));
                File.WriteAllText(summaryPath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Appends changes to an existing module summary
        /// </summary>
        /// <param name="summaryFile">Path to the summary file</param>
        /// <param name="changes">Changes to append</param>
        /// <returns>True if successfully appended, false otherwise</returns>
        private static bool AppendToModuleSummary(string summaryFile, string changes)
        {
            try
            {
                var content = File.ReadAllText(summaryFile, Encoding.UTF8);
                var currentDate = DateHelper.GetCurrentDateString();
                var newEntry = $"\n### {currentDate}\n- {changes}\n";

                // Find the "Recent Updates" section and insert after it
                var recentUpdatesIndex = content.IndexOf("## ?? Recent Updates");
                if (recentUpdatesIndex == -1)
                {
                    recentUpdatesIndex = content.IndexOf("## ?? Recent Updates"); // Fallback for old format
                }

                if (recentUpdatesIndex != -1)
                {
                    var insertIndex = content.IndexOf('\n', recentUpdatesIndex) + 1;
                    content = content.Insert(insertIndex, newEntry);

                    File.WriteAllText(summaryFile, content, Encoding.UTF8);
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

        #region README Management - NEW v1.3.0

        /// <summary>
        /// Updates a navigation README file with new content or changes
        /// </summary>
        /// <param name="category">Category name (e.g., "Modules", "Development", "AI-Assistant")</param>
        /// <param name="changes">Description of changes made</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateNavigationReadme(string category, string changes)
        {
            try
            {
                var readmePath = $"Documentation/{category}/README.md";

                if (!File.Exists(readmePath))
                {
                    return CreateNavigationReadme(category, changes);
                }

                return UpdateReadmeWithChanges(readmePath, changes, ReadmeType.Navigation);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates a technical README file for a module
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="newFiles">Array of new files added to the module</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateTechnicalReadme(string moduleName, string[] newFiles)
        {
            try
            {
                var readmePath = $"{moduleName}/README.md";

                if (!File.Exists(readmePath))
                {
                    return CreateTechnicalReadme(moduleName, newFiles);
                }

                return AddFilesToTechnicalReadme(readmePath, newFiles);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates a category README file
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="newDocuments">Array of new documents added to the category</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateCategoryReadme(string category, string[] newDocuments)
        {
            try
            {
                var readmePath = $"Documentation/{category}/README.md";

                if (!File.Exists(readmePath))
                {
                    return CreateCategoryReadme(category, newDocuments);
                }

                return AddDocumentsToCategoryReadme(readmePath, newDocuments);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all README files in the project
        /// </summary>
        /// <returns>List of README file paths</returns>
        public static List<string> GetAllReadmeFiles()
        {
            var readmeFiles = new List<string>();

            try
            {
                // Find all README.md files in the project
                var allReadmes = Directory
                    .GetFiles(".", "README.md", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\.git\\"))
                    .Where(f => !f.Contains("\\bin\\"))
                    .Where(f => !f.Contains("\\obj\\"))
                    .ToList();

                readmeFiles.AddRange(allReadmes);
            }
            catch (Exception)
            {
                // Ignore errors
            }

            return readmeFiles;
        }

        /// <summary>
        /// Validates README hierarchy and cross-references
        /// </summary>
        /// <returns>True if hierarchy is valid, false otherwise</returns>
        public static bool ValidateReadmeHierarchy()
        {
            try
            {
                var issues = new List<string>();
                var readmeFiles = GetAllReadmeFiles();

                foreach (var readmeFile in readmeFiles)
                {
                    var readmeType = GetReadmeType(readmeFile);
                    ValidateReadmeStructure(readmeFile, readmeType, issues);
                    ValidateReadmeCrossReferences(readmeFile, issues);
                }

                return issues.Count == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Synchronizes cross-references between README files
        /// </summary>
        /// <returns>True if successfully synchronized, false otherwise</returns>
        public static bool SynchronizeReadmeCrossReferences()
        {
            try
            {
                var readmeFiles = GetAllReadmeFiles();
                var success = true;

                foreach (var readmeFile in readmeFiles)
                {
                    success &= UpdateReadmeCrossReferences(readmeFile);
                }

                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines the type of README file
        /// </summary>
        /// <param name="readmePath">Path to the README file</param>
        /// <returns>README type</returns>
        public static ReadmeType GetReadmeType(string readmePath)
        {
            var normalizedPath = readmePath.Replace('\\', '/');

            if (normalizedPath == "Documentation/README.md")
                return ReadmeType.Root;

            if (
                normalizedPath.StartsWith("Documentation/") && normalizedPath.EndsWith("/README.md")
            )
                return ReadmeType.Navigation;

            if (
                Regex.IsMatch(
                    normalizedPath,
                    @"^(ViewModels|DevTools|Utils|Services|Models|Clients|Views|Converters)/README\.md$"
                )
            )
                return ReadmeType.Technical;

            return ReadmeType.Category;
        }

        /// <summary>
        /// Creates a new navigation README file
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="changes">Initial changes</param>
        /// <returns>True if successfully created, false otherwise</returns>
        private static bool CreateNavigationReadme(string category, string changes)
        {
            try
            {
                var readmePath = $"Documentation/{category}/README.md";
                var currentDate = DateHelper.GetCurrentDateString();

                var content =
                    $@"# ?? {category} Documentation

## ?? **Purpose**

This folder contains {category.ToLower()} documentation for the OstPlayer project.

## ?? **Contents**

### **Recent Additions**
- {changes} (Added {currentDate})

## ?? **Target Audience**

- **Developers**: Technical implementation details
- **AI Assistants**: Context for intelligent modifications
- **Maintainers**: Understanding current state and changes

## ?? **Maintenance Process**

- ? **Automated**: Basic updates via DevTools
- ?? **Manual**: Significant changes and new features
- ?? **Triggered**: When files in category are modified

---

**Category**: {category} Documentation  
**Maintenance**: Automated via DevTools + Manual curation  
**Last Updated**: {currentDate}
";

                Directory.CreateDirectory(Path.GetDirectoryName(readmePath));
                File.WriteAllText(readmePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new technical README file
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="newFiles">New files to include</param>
        /// <returns>True if successfully created, false otherwise</returns>
        private static bool CreateTechnicalReadme(string moduleName, string[] newFiles)
        {
            try
            {
                var readmePath = $"{moduleName}/README.md";
                var currentDate = DateHelper.GetCurrentDateString();

                var content =
                    $@"# {moduleName} Module - Technical Documentation

## ?? Module Overview

The {moduleName} module provides core functionality for the OstPlayer project.

## ?? Files in {moduleName} Module

";
                foreach (var file in newFiles)
                {
                    var fileName = Path.GetFileName(file);
                    content += $"### **{fileName}**\n";
                    content += $"- **Purpose**: Core {moduleName.ToLower()} functionality\n";
                    content += $"- **Status**: ? Active\n\n";
                }

                content +=
                    $@"
## ??? Architecture Overview

### **Core Design Patterns Applied:**
1. **Pattern Name** - Description of usage
2. **Pattern Name** - Description of usage

---

**Module Status**: ? **ACTIVE**  
**Last Updated**: {currentDate}  
**Architecture**: Modern .NET Framework 4.6.2 implementation
";

                File.WriteAllText(readmePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new category README file
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="newDocuments">New documents to include</param>
        /// <returns>True if successfully created, false otherwise</returns>
        private static bool CreateCategoryReadme(string category, string[] newDocuments)
        {
            try
            {
                var readmePath = $"Documentation/{category}/README.md";
                var currentDate = DateHelper.GetCurrentDateString();

                var content =
                    $@"# ?? {category} Documentation

## ?? **Purpose**

This folder contains {category.ToLower()}-specific documentation for the OstPlayer project.

## ?? **Documents in this Category**

";
                foreach (var doc in newDocuments)
                {
                    var docName = Path.GetFileNameWithoutExtension(doc);
                    content += $"- **{docName}** - Description of document\n";
                }

                content +=
                    $@"

## ?? **Target Audience**

- **Category-specific users**
- **Related stakeholders**

---

**Category**: {category} Documentation  
**Last Updated**: {currentDate}
";

                Directory.CreateDirectory(Path.GetDirectoryName(readmePath));
                File.WriteAllText(readmePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates a README file with changes
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <param name="changes">Changes to add</param>
        /// <param name="readmeType">Type of README</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        private static bool UpdateReadmeWithChanges(
            string readmePath,
            string changes,
            ReadmeType readmeType
        )
        {
            try
            {
                var content = File.ReadAllText(readmePath, Encoding.UTF8);
                var currentDate = DateHelper.GetCurrentDateString();

                // Update Last Updated date
                content = Regex.Replace(
                    content,
                    @"\*\*Last Updated\*\*: \d{4}-\d{2}-\d{2}",
                    $"**Last Updated**: {currentDate}"
                );

                // Add changes to appropriate section based on README type
                if (readmeType == ReadmeType.Navigation)
                {
                    var recentAdditionsIndex = content.IndexOf("### **Recent Additions**");
                    if (recentAdditionsIndex != -1)
                    {
                        var insertIndex = content.IndexOf('\n', recentAdditionsIndex) + 1;
                        content = content.Insert(
                            insertIndex,
                            $"- {changes} (Added {currentDate})\n"
                        );
                    }
                }

                File.WriteAllText(readmePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds files to technical README
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <param name="newFiles">New files to add</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        private static bool AddFilesToTechnicalReadme(string readmePath, string[] newFiles)
        {
            try
            {
                var content = File.ReadAllText(readmePath, Encoding.UTF8);
                var currentDate = DateHelper.GetCurrentDateString();

                // Find module files section and add new files
                var moduleFilesIndex = content.IndexOf("## ?? Files in");
                if (moduleFilesIndex != -1)
                {
                    var insertIndex = content.IndexOf(
                        "## ??? Architecture Overview",
                        moduleFilesIndex
                    );
                    if (insertIndex == -1)
                        insertIndex = content.Length;

                    var newContent = "";
                    foreach (var file in newFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        newContent += $"\n### **{fileName}** ??\n";
                        newContent += $"- **Purpose**: New functionality added {currentDate}\n";
                        newContent += $"- **Status**: ? Active\n";
                    }

                    content = content.Insert(insertIndex, newContent + "\n");
                }

                // Update Last Updated date
                content = Regex.Replace(
                    content,
                    @"\*\*Last Updated\*\*: \d{4}-\d{2}-\d{2}",
                    $"**Last Updated**: {currentDate}"
                );

                File.WriteAllText(readmePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds documents to category README
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <param name="newDocuments">New documents to add</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        private static bool AddDocumentsToCategoryReadme(string readmePath, string[] newDocuments)
        {
            try
            {
                var content = File.ReadAllText(readmePath, Encoding.UTF8);
                var currentDate = DateHelper.GetCurrentDateString();

                // Find documents section and add new documents
                var documentsIndex = content.IndexOf("## ?? **Documents in this Category**");
                if (documentsIndex != -1)
                {
                    var insertIndex = content.IndexOf("\n\n## ??", documentsIndex);
                    if (insertIndex == -1)
                        insertIndex = content.Length;

                    var newContent = "";
                    foreach (var doc in newDocuments)
                    {
                        var docName = Path.GetFileNameWithoutExtension(doc);
                        newContent += $"- **{docName}** - New document added {currentDate}\n";
                    }

                    content = content.Insert(insertIndex, newContent);
                }

                // Update Last Updated date
                content = Regex.Replace(
                    content,
                    @"\*\*Last Updated\*\*: \d{4}-\d{2}-\d{2}",
                    $"**Last Updated**: {currentDate}"
                );

                File.WriteAllText(readmePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates README structure
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <param name="readmeType">Type of README</param>
        /// <param name="issues">List to collect issues</param>
        private static void ValidateReadmeStructure(
            string readmePath,
            ReadmeType readmeType,
            List<string> issues
        )
        {
            try
            {
                var content = File.ReadAllText(readmePath, Encoding.UTF8);

                // Check for required sections based on README type
                switch (readmeType)
                {
                    case ReadmeType.Root:
                        if (!content.Contains("## ?? **Documentation Structure**"))
                            issues.Add($"{readmePath}: Missing Documentation Structure section");
                        break;
                    case ReadmeType.Navigation:
                        if (!content.Contains("## ?? **Purpose**"))
                            issues.Add($"{readmePath}: Missing Purpose section");
                        break;
                    case ReadmeType.Technical:
                        if (!content.Contains("## ?? Module Overview"))
                            issues.Add($"{readmePath}: Missing Module Overview section");
                        break;
                }

                // Check for Last Updated field
                if (!content.Contains("**Last Updated**:"))
                    issues.Add($"{readmePath}: Missing Last Updated field");
            }
            catch (Exception ex)
            {
                issues.Add($"{readmePath}: Error validating structure - {ex.Message}");
            }
        }

        /// <summary>
        /// Validates cross-references in README
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <param name="issues">List to collect issues</param>
        private static void ValidateReadmeCrossReferences(string readmePath, List<string> issues)
        {
            try
            {
                var content = File.ReadAllText(readmePath, Encoding.UTF8);
                var linkRegex = new Regex(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);
                var linkMatches = linkRegex.Matches(content);

                foreach (Match linkMatch in linkMatches)
                {
                    var linkPath = linkMatch.Groups[2].Value;
                    if (linkPath.StartsWith("http"))
                        continue; // Skip external links

                    var fullPath = Path.Combine(Path.GetDirectoryName(readmePath), linkPath);
                    var normalizedPath = Path.GetFullPath(fullPath);

                    if (!File.Exists(normalizedPath))
                    {
                        issues.Add($"{readmePath}: Broken link - {linkPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"{readmePath}: Error validating cross-references - {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a README file's cross-references after path changes
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        private static bool UpdateReadmeCrossReferences(string readmePath)
        {
            try
            {
                var content = File.ReadAllText(readmePath, Encoding.UTF8);
                var originalContent = content;

                // Update known path changes
                var pathMappings = new Dictionary<string, string>
                {
                    ["Utils/DateHelper.cs"] = "DevTools/DateHelper.cs",
                    ["Utils/DocumentationManager.cs"] = "DevTools/DocumentationManager.cs",
                    ["Utils/ProjectAnalyzer.cs"] = "DevTools/ProjectAnalyzer.cs",
                    ["DevTools/FileHeaderPolicy.md"] =
                        "Documentation/Development/FileHeaderPolicy.md",
                    ["DevTools/SmartDateAutomation.md"] =
                        "Documentation/AI-Assistant/SmartDateAutomation.md",
                };

                foreach (var mapping in pathMappings)
                {
                    content = content.Replace(mapping.Key, mapping.Value);
                }

                if (content != originalContent)
                {
                    File.WriteAllText(readmePath, content, Encoding.UTF8);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Main Changelog Management

        /// <summary>
        /// Updates the main project changelog with a new version entry
        /// </summary>
        /// <param name="version">Version number</param>
        /// <param name="changes">Description of changes</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateMainChangelog(string version, string changes)
        {
            try
            {
                if (!File.Exists(ChangelogFile))
                {
                    return CreateMainChangelog(version, changes);
                }

                return InsertChangelogEntry(version, changes);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new main changelog file
        /// </summary>
        /// <param name="version">Initial version</param>
        /// <param name="changes">Initial changes</param>
        /// <returns>True if successfully created, false otherwise</returns>
        private static bool CreateMainChangelog(string version, string changes)
        {
            try
            {
                var currentDate = DateHelper.GetCurrentDateString();
                var content =
                    $@"# OstPlayer - Changelog

Všechny významné změny v tomto projektu budou dokumentovány v tomto souboru.

Formát je založen na [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
a tento projekt dodržuje [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Future enhancements and improvements

## [{version}] - {currentDate}

### Added
- {changes}

---

**Legend:**
- ?? New features
- ? Performance improvements  
- ?? Technical changes
- ?? Documentation
- ?? Bug fixes
- ?? Breaking changes
- ?? Compatibility
";

                Directory.CreateDirectory(Path.GetDirectoryName(ChangelogFile));
                File.WriteAllText(ChangelogFile, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts a new changelog entry into the existing changelog
        /// </summary>
        /// <param name="version">Version number</param>
        /// <param name="changes">Description of changes</param>
        /// <returns>True if successfully inserted, false otherwise</returns>
        private static bool InsertChangelogEntry(string version, string changes)
        {
            try
            {
                var content = File.ReadAllText(ChangelogFile, Encoding.UTF8);
                var currentDate = DateHelper.GetCurrentDateString();

                var newEntry =
                    $@"
## [{version}] - {currentDate}

### Added
- {changes}
";

                // Find the first existing version entry and insert before it
                var firstVersionMatch = VersionHeaderRegex.Match(content);
                if (firstVersionMatch.Success)
                {
                    var insertIndex = firstVersionMatch.Index;
                    content = content.Insert(insertIndex, newEntry + "\n");

                    File.WriteAllText(ChangelogFile, content, Encoding.UTF8);
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

        #region Documentation Consistency

        /// <summary>
        /// Validates documentation consistency across the project
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <returns>True if documentation is consistent, false otherwise</returns>
        public static bool ValidateDocumentationConsistency(string projectPath = ".")
        {
            try
            {
                var issues = new List<string>();

                ValidateChangelogConsistency(issues);
                ValidateModuleSummaries(projectPath, issues);
                ValidateCrossReferences(projectPath, issues);
                ValidateReadmeHierarchy(issues); // NEW: README validation

                return issues.Count == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates README hierarchy as part of documentation consistency
        /// </summary>
        /// <param name="issues">List to collect issues</param>
        private static void ValidateReadmeHierarchy(List<string> issues)
        {
            try
            {
                if (!ValidateReadmeHierarchy())
                {
                    issues.Add("README hierarchy validation failed");
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error validating README hierarchy: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates changelog consistency
        /// </summary>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateChangelogConsistency(List<string> issues)
        {
            try
            {
                if (!File.Exists(ChangelogFile))
                {
                    issues.Add("Main changelog file not found");
                    return;
                }

                var content = File.ReadAllText(ChangelogFile, Encoding.UTF8);
                var versionMatches = VersionHeaderRegex.Matches(content);

                var versions = new List<Version>();
                foreach (Match match in versionMatches)
                {
                    if (Version.TryParse(match.Groups[1].Value, out var version))
                    {
                        versions.Add(version);
                    }
                }

                // Check if versions are in descending order
                for (int i = 0; i < versions.Count - 1; i++)
                {
                    if (versions[i] < versions[i + 1])
                    {
                        issues.Add(
                            $"Changelog versions not in descending order: {versions[i]} < {versions[i + 1]}"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error validating changelog: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates module summaries
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateModuleSummaries(string projectPath, List<string> issues)
        {
            try
            {
                var moduleDirectories = Directory
                    .GetDirectories(projectPath)
                    .Where(d => !Path.GetFileName(d).StartsWith("."))
                    .Where(d => Path.GetFileName(d) != "Documentation")
                    .Where(d => Path.GetFileName(d) != "DevTools")
                    .ToList();

                foreach (var moduleDir in moduleDirectories)
                {
                    var moduleName = Path.GetFileName(moduleDir);
                    var summaryFile = FindModuleSummaryFile(moduleName);

                    if (summaryFile == null)
                    {
                        issues.Add($"No summary file found for module: {moduleName}");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error validating module summaries: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates cross-references between documentation files
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateCrossReferences(string projectPath, List<string> issues)
        {
            try
            {
                var documentationFiles = Directory.GetFiles(
                    DocumentationFolder,
                    "*.md",
                    SearchOption.AllDirectories
                );
                var linkRegex = new Regex(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);

                foreach (var docFile in documentationFiles)
                {
                    var content = File.ReadAllText(docFile, Encoding.UTF8);
                    var linkMatches = linkRegex.Matches(content);

                    foreach (Match linkMatch in linkMatches)
                    {
                        var linkPath = linkMatch.Groups[2].Value;
                        if (linkPath.StartsWith("http"))
                            continue; // Skip external links

                        var fullPath = Path.Combine(Path.GetDirectoryName(docFile), linkPath);
                        if (!File.Exists(fullPath))
                        {
                            issues.Add($"Broken link in {docFile}: {linkPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error validating cross-references: {ex.Message}");
            }
        }

        #endregion

        #region Path Management

        /// <summary>
        /// Updates documentation paths after file reorganization
        /// </summary>
        /// <param name="pathMappings">Dictionary of old path to new path mappings</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateDocumentationPaths(Dictionary<string, string> pathMappings)
        {
            try
            {
                var documentationFiles = Directory.GetFiles(
                    DocumentationFolder,
                    "*.md",
                    SearchOption.AllDirectories
                );
                var readmeFiles = GetAllReadmeFiles(); // Include README files
                var allFiles = documentationFiles.Concat(readmeFiles).Distinct().ToArray();

                var success = true;

                foreach (var docFile in allFiles)
                {
                    if (!UpdatePathsInFile(docFile, pathMappings))
                    {
                        success = false;
                    }
                }

                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates paths in a specific documentation file
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <param name="pathMappings">Dictionary of path mappings</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        private static bool UpdatePathsInFile(
            string filePath,
            Dictionary<string, string> pathMappings
        )
        {
            try
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var originalContent = content;

                foreach (var mapping in pathMappings)
                {
                    content = content.Replace(mapping.Key, mapping.Value);
                }

                if (content != originalContent)
                {
                    File.WriteAllText(filePath, content, Encoding.UTF8);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets all documentation files in the project
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <returns>List of documentation file paths</returns>
        public static List<string> GetDocumentationFiles(string projectPath = ".")
        {
            var files = new List<string>();

            if (Directory.Exists(DocumentationFolder))
            {
                files.AddRange(
                    Directory.GetFiles(DocumentationFolder, "*.md", SearchOption.AllDirectories)
                );
            }

            // Add module-specific documentation
            var moduleDirectories = Directory
                .GetDirectories(projectPath)
                .Where(d => !Path.GetFileName(d).StartsWith("."))
                .Where(d => Path.GetFileName(d) != "Documentation")
                .Where(d => Path.GetFileName(d) != "DevTools");

            foreach (var moduleDir in moduleDirectories)
            {
                files.AddRange(
                    Directory.GetFiles(moduleDir, "*.md", SearchOption.TopDirectoryOnly)
                );
            }

            return files;
        }

        /// <summary>
        /// Checks if documentation structure is valid
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <returns>True if structure is valid, false otherwise</returns>
        public static bool IsDocumentationStructureValid(string projectPath = ".")
        {
            return Directory.Exists(DocumentationFolder)
                && File.Exists(ChangelogFile)
                && ValidateReadmeHierarchy(); // Include README validation
        }

        #endregion

        #region Module Summary Management - Enhanced v1.3.2

        /// <summary>
        /// Automatically updates module summaries based on detected file changes.
        /// AUTOMATION: Integrates with ProjectAnalyzer to detect and document module changes.
        /// </summary>
        /// <param name="changedFiles">List of files that were modified</param>
        /// <returns>Number of module summaries updated</returns>
        public static int UpdateModuleSummariesFromChanges(IEnumerable<string> changedFiles)
        {
            try
            {
                var moduleChanges = ProjectAnalyzer.DetectModuleChanges(changedFiles);
                var updatedCount = 0;

                foreach (var moduleChange in moduleChanges)
                {
                    var moduleName = moduleChange.Key;
                    var changes = moduleChange.Value;

                    var changeDescription = string.Join(", ", changes);
                    var success = UpdateModuleSummary(moduleName, changeDescription);

                    if (success)
                    {
                        updatedCount++;
                    }
                }

                return updatedCount;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Updates a specific module summary with comprehensive change tracking.
        /// ENHANCEMENT: Enhanced version with better change categorization and formatting.
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="changes">Description of changes</param>
        /// <param name="changeType">Type of change (Added, Modified, Refactored, etc.)</param>
        /// <returns>True if successfully updated</returns>
        public static bool UpdateModuleSummaryEnhanced(
            string moduleName,
            string changes,
            string changeType = "Modified"
        )
        {
            try
            {
                var summaryFile = FindModuleSummaryFile(moduleName);

                if (summaryFile == null)
                {
                    // Create new module summary if it doesn't exist
                    return CreateModuleSummary(moduleName, $"{changeType}: {changes}");
                }

                return AppendToModuleSummaryEnhanced(summaryFile, changes, changeType);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a comprehensive new module summary file with standard structure.
        /// ENHANCEMENT: Improved template with better organization and metadata.
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="initialChanges">Initial changes description</param>
        /// <returns>True if successfully created</returns>
        private static bool CreateModuleSummaryEnhanced(string moduleName, string initialChanges)
        {
            try
            {
                var summaryPath = $"Documentation/Modules/{moduleName}ModuleUpdateSummary.md";
                var currentDate = DateHelper.GetCurrentDateString();

                var content =
                    $@"# OstPlayer {moduleName} Module - Update Summary

## ?? **Module Overview**

The {moduleName} module provides essential functionality for the OstPlayer plugin, handling [module-specific responsibilities].

### **Module Responsibilities**
- [Key responsibility 1]
- [Key responsibility 2]
- [Key responsibility 3]

### **Integration Points**
- Dependencies: [Other modules this depends on]
- Dependents: [Modules that depend on this one]
- External APIs: [External services or libraries used]

## ?? **Recent Updates**

### {currentDate}
- {initialChanges}

## ?? **Module Statistics**

- **Files in Module**: [Number of files]
- **Last Major Update**: {currentDate}
- **Stability Level**: Stable | Active Development | Experimental
- **Test Coverage**: [Coverage percentage if available]

## ??? **Architecture Notes**

### **Design Patterns Used**
- [Pattern 1]: [Brief description]
- [Pattern 2]: [Brief description]

### **Performance Characteristics**
- [Performance aspect 1]
- [Performance aspect 2]

## ?? **Dependencies**

### **Internal Dependencies**
- [OstPlayer module dependencies]

### **External Dependencies**
- [External libraries and frameworks]

## ?? **Future Plans**

### **Planned Improvements**
- [Improvement 1]
- [Improvement 2]

### **Technical Debt**
- [Technical debt item 1]
- [Technical debt item 2]

---

**Last Updated**: {currentDate}  
**Module Status**: Active Development  
**Maintainer**: TiggAdry  
**Documentation Version**: 1.0.0

*This file is automatically maintained by DocumentationManager utility.*
";

                Directory.CreateDirectory(Path.GetDirectoryName(summaryPath));
                File.WriteAllText(summaryPath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Appends changes to module summary with enhanced formatting and categorization.
        /// ENHANCEMENT: Better organization and change type categorization.
        /// </summary>
        /// <param name="summaryFile">Path to the summary file</param>
        /// <param name="changes">Changes to append</param>
        /// <param name="changeType">Type of change</param>
        /// <returns>True if successfully appended</returns>
        private static bool AppendToModuleSummaryEnhanced(
            string summaryFile,
            string changes,
            string changeType
        )
        {
            try
            {
                var content = File.ReadAllText(summaryFile, Encoding.UTF8);
                var currentDate = DateHelper.GetCurrentDateString();

                // Determine emoji based on change type
                var emoji = GetChangeTypeEmoji(changeType);
                var newEntry = $"\n### {currentDate}\n- {emoji} **{changeType}**: {changes}\n";

                // Find the "Recent Updates" section and insert after it
                var recentUpdatesIndex = content.IndexOf("## ?? **Recent Updates**");
                if (recentUpdatesIndex == -1)
                {
                    recentUpdatesIndex = content.IndexOf("## Recent Updates"); // Fallback
                }

                if (recentUpdatesIndex != -1)
                {
                    var insertIndex = content.IndexOf('\n', recentUpdatesIndex) + 1;
                    content = content.Insert(insertIndex, newEntry);

                    // Update "Last Updated" timestamp
                    content = UpdateLastUpdatedTimestamp(content, currentDate);

                    File.WriteAllText(summaryFile, content, Encoding.UTF8);
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
        /// Gets appropriate emoji for change type.
        /// VISUAL: Provides visual indicators for different types of changes.
        /// </summary>
        /// <param name="changeType">Type of change</param>
        /// <returns>Emoji representing the change type</returns>
        private static string GetChangeTypeEmoji(string changeType)
        {
            switch (changeType.ToLower())
            {
                case "added":
                    return "?";
                case "modified":
                    return "??";
                case "refactored":
                    return "??";
                case "fixed":
                    return "??";
                case "enhanced":
                    return "?";
                case "deprecated":
                    return "??";
                case "removed":
                    return "???";
                default:
                    return "??";
            }
        }

        /// <summary>
        /// Updates the "Last Updated" timestamp in module summary content.
        /// MAINTENANCE: Keeps timestamp metadata current.
        /// </summary>
        /// <param name="content">Module summary content</param>
        /// <param name="newDate">New date to set</param>
        /// <returns>Updated content</returns>
        private static string UpdateLastUpdatedTimestamp(string content, string newDate)
        {
            var timestampPattern = @"\*\*Last Updated\*\*:\s*\d{4}-\d{2}-\d{2}";
            var replacement = $"**Last Updated**: {newDate}";

            return Regex.Replace(content, timestampPattern, replacement);
        }

        /// <summary>
        /// Analyzes module activity and provides update recommendations.
        /// INTELLIGENCE: Uses ProjectAnalyzer to suggest documentation updates.
        /// </summary>
        /// <param name="changedFiles">List of files that were changed</param>
        /// <returns>List of recommendations for module documentation updates</returns>
        public static List<ModuleUpdateRecommendation> GetModuleUpdateRecommendations(
            IEnumerable<string> changedFiles
        )
        {
            try
            {
                var moduleChanges = ProjectAnalyzer.DetectModuleChanges(changedFiles);
                return ProjectAnalyzer.AnalyzeModuleActivity(moduleChanges);
            }
            catch (Exception)
            {
                return new List<ModuleUpdateRecommendation>();
            }
        }

        /// <summary>
        /// Processes a list of module update recommendations and applies them.
        /// AUTOMATION: Batch processing of module documentation updates.
        /// </summary>
        /// <param name="recommendations">List of update recommendations</param>
        /// <returns>Number of recommendations successfully processed</returns>
        public static int ProcessModuleUpdateRecommendations(
            List<ModuleUpdateRecommendation> recommendations
        )
        {
            var processedCount = 0;

            foreach (var recommendation in recommendations)
            {
                try
                {
                    if (!recommendation.SummaryExists)
                    {
                        // Create new module summary
                        var initialChanges = string.Join(", ", recommendation.Changes);
                        var success = CreateModuleSummaryEnhanced(
                            recommendation.ModuleName,
                            initialChanges
                        );
                        if (success)
                            processedCount++;
                    }
                    else
                    {
                        // Update existing module summary
                        var changes = string.Join(", ", recommendation.Changes);
                        var changeType = DetermineChangeType(recommendation.Changes);
                        var success = UpdateModuleSummaryEnhanced(
                            recommendation.ModuleName,
                            changes,
                            changeType
                        );
                        if (success)
                            processedCount++;
                    }
                }
                catch (Exception)
                {
                    // Continue processing other recommendations even if one fails
                    continue;
                }
            }

            return processedCount;
        }

        /// <summary>
        /// Determines the primary change type from a list of changes.
        /// CLASSIFICATION: Categorizes changes for better documentation organization.
        /// </summary>
        /// <param name="changes">List of change descriptions</param>
        /// <returns>Primary change type</returns>
        private static string DetermineChangeType(List<string> changes)
        {
            if (changes.Any(c => c.Contains("Added")))
                return "Added";
            if (changes.Any(c => c.Contains("Enhanced") || c.Contains("Improved")))
                return "Enhanced";
            if (changes.Any(c => c.Contains("Fixed") || c.Contains("Corrected")))
                return "Fixed";
            if (changes.Any(c => c.Contains("Refactor")))
                return "Refactored";

            return "Modified";
        }

        #endregion
    }
}
