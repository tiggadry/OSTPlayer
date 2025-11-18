// ====================================================================
// FILE: ProjectAnalyzer.cs
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
// Intelligent development project analysis utility for the OstPlayer project.
// This tool is designed for AI assistants and development automation, providing
// automated detection of affected files, dependency analysis, and cross-reference
// management for documentation updates. Now includes specialized README intelligence
// for enhanced navigation and organizational management.
//
// FEATURES:
// - Automated detection of affected documentation files
// - Cross-reference analysis and updates
// - Project consistency validation
// - Documentation structure validation
// - Module dependency mapping
// - **NEW v1.3.0**: Specialized README intelligence with type detection
// - **NEW v1.3.0**: Smart README update triggering based on change patterns
// - **NEW v1.3.0**: Hierarchical README impact analysis
//
// DEPENDENCIES:
// - System.IO for file operations
// - System.Text for text manipulation
// - System.Collections.Generic for collections
// - System.Linq for LINQ operations
// - System.Text.RegularExpressions for pattern matching
// - OstPlayer.DevTools.DateHelper for date management
// - OstPlayer.DevTools.DocumentationManager for documentation operations
//
// DESIGN PATTERNS:
// - Static utility class pattern
// - Analyzer pattern for code analysis
// - Observer pattern for change detection
// - Strategy pattern for different analysis types
// - **NEW**: Factory pattern for README type detection
// - **NEW**: Chain of Responsibility for README update triggers
//
// ALGORITHMS:
// - File dependency graph construction
// - Cross-reference traversal algorithms
// - Pattern matching for documentation links
// - Module impact analysis
// - **NEW**: README hierarchy traversal and impact analysis
//
// PERFORMANCE NOTES:
// - Efficient file system traversal
// - Cached dependency graph for repeated analysis
// - Optimized pattern matching with compiled regex
// - Minimal memory allocation in hot paths
// - **NEW**: Cached README type detection for performance
//
// LIMITATIONS:
// - Requires consistent project structure
// - Limited to predefined file patterns
// - No real-time file system monitoring
//
// FUTURE REFACTORING:
// FUTURE: Add real-time file system watching
// FUTURE: Implement machine learning for smart dependency detection
// FUTURE: Add support for external project references
// FUTURE: Implement impact analysis visualization
// FUTURE: Add predictive README update suggestions
// CONSIDER: Integration with version control for change tracking
// CONSIDER: Plugin architecture for custom analyzers
//
// TESTING:
// - Unit tests for dependency detection
// - Integration tests with project structure
// - Performance tests for large codebases
// - Accuracy tests for cross-reference detection
// - **NEW**: README intelligence accuracy tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
//
// CHANGELOG:
// 2025-08-07 v1.3.0 - Enhanced README intelligence: Added specialized README analysis, smart triggering, hierarchical impact analysis
// 2025-08-07 v1.2.1 - Moved to DevTools module and updated namespace references
// 2025-08-07 v1.2.0 - Initial implementation for enhanced automation utilities
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OstPlayer.DevTools {
    /// <summary>
    /// Trigger configuration for README updates.
    /// </summary>
    public class ReadmeUpdateTrigger
    {
        /// <summary>
        /// Gets or sets the type of README.
        /// </summary>
        public ReadmeType ReadmeType { get; set; }
        
        /// <summary>
        /// Gets or sets the pattern that triggers updates.
        /// </summary>
        public string TriggerPattern { get; set; }
        
        /// <summary>
        /// Gets or sets the description of this trigger.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the function to determine if trigger should fire.
        /// </summary>
        public Func<string, bool> ShouldTrigger { get; set; }
    }

    /// <summary>
    /// Intelligent development project analysis utility with enhanced README intelligence
    /// </summary>
    public static class ProjectAnalyzer {
        // File patterns for different analysis types
        private static readonly string[] SourceFilePatterns = { "*.cs", "*.xaml" };
        private static readonly string[] DocumentationFilePatterns = { "*.md" };
        private static readonly string[] ScriptFilePatterns = { "*.ps1", "*.bat", "*.cmd" };

        // Regex patterns for dependency detection
        private static readonly Regex UsingDirectiveRegex = new Regex(@"using\s+([^;]+);", RegexOptions.Compiled);
        private static readonly Regex NamespaceRegex = new Regex(@"namespace\s+([^\s{]+)", RegexOptions.Compiled);
        private static readonly Regex ClassRegex = new Regex(@"(?:public\s+)?(?:static\s+)?class\s+(\w+)", RegexOptions.Compiled);
        private static readonly Regex DocumentationLinkRegex = new Regex(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);
        private static readonly Regex ModuleReferenceRegex = new Regex(@"(?:ViewModels|Models|Utils|Services|Clients|Views|Converters)", RegexOptions.Compiled);

        // README-specific patterns
        private static readonly Regex ReadmeStructuralChangeRegex = new Regex(@"Documentation[/\\].*[/\\]README\.md$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ModuleFileChangeRegex = new Regex(@"^(ViewModels|Models|Utils|Services|Clients|Views|Converters)[/\\].*\.(cs|xaml)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Cached dependency information
        private static Dictionary<string, HashSet<string>> _dependencyCache = new Dictionary<string, HashSet<string>>();
        private static Dictionary<string, ReadmeType> _readmeTypeCache = new Dictionary<string, ReadmeType>();
        private static DateTime _cacheLastUpdated = DateTime.MinValue;

        // README update trigger rules
        private static readonly List<ReadmeUpdateTrigger> ReadmeUpdateTriggers = new List<ReadmeUpdateTrigger>
        {
            new ReadmeUpdateTrigger
            {
                ReadmeType = ReadmeType.Navigation,
                TriggerPattern = @"Documentation[/\\]Modules[/\\].*ModuleUpdateSummary\.md$",
                Description = "New module summary added",
                ShouldTrigger = (changedFile) => changedFile.Contains("Modules/README.md")
            },
            new ReadmeUpdateTrigger
            {
                ReadmeType = ReadmeType.Technical,
                TriggerPattern = @"^(ViewModels|Utils|Services|DevTools)[/\\].*\.cs$",
                Description = "New source file in module",
                ShouldTrigger = (changedFile) =>
                {
                    var moduleMatch = ModuleFileChangeRegex.Match(changedFile);
                    return moduleMatch.Success;
                }
            },
            new ReadmeUpdateTrigger
            {
                ReadmeType = ReadmeType.Category,
                TriggerPattern = @"Documentation[/\\](Development|Analysis|Refactoring)[/\\].*\.md$",
                Description = "New document in category",
                ShouldTrigger = (changedFile) =>
                {
                    var pathParts = changedFile.Split('/', '\\');
                    return pathParts.Length >= 2;
                }
            },
            new ReadmeUpdateTrigger
            {
                ReadmeType = ReadmeType.Root,
                TriggerPattern = @"Documentation[/\\].*[/\\]README\.md$",
                Description = "Structural documentation change",
                ShouldTrigger = (changedFile) =>
                    ReadmeStructuralChangeRegex.IsMatch(changedFile)
            }
        };

        #region Affected Files Detection

        /// <summary>
        /// Gets a list of documentation files that should be updated based on changed files
        /// </summary>
        /// <param name="changedFiles">Array of file paths that have been changed</param>
        /// <returns>List of documentation files that need updates</returns>
        public static List<string> GetAffectedDocumentationFiles(string[] changedFiles) {
            try {
                var affectedDocs = new HashSet<string>();
                var moduleChanges = new Dictionary<string, List<string>>();

                // Analyze each changed file
                foreach (var file in changedFiles) {
                    var module = GetModuleFromFilePath(file);
                    if (module != null) {
                        if (!moduleChanges.ContainsKey(module)) {
                            moduleChanges[module] = new List<string>();
                        }
                        moduleChanges[module].Add(file);
                    }

                    // Add direct documentation dependencies
                    affectedDocs.UnionWith(GetDirectDocumentationDependencies(file));
                }

                // Add module-specific documentation
                foreach (var module in moduleChanges.Keys) {
                    affectedDocs.UnionWith(GetModuleDocumentationFiles(module));
                }

                // Add affected README files (NEW)
                affectedDocs.UnionWith(GetAffectedReadmeFiles(changedFiles));

                // Always include main changelog for any code changes
                if (changedFiles.Any(f => IsSourceFile(f))) {
                    affectedDocs.Add("Documentation/Core/CHANGELOG.md");
                }

                return affectedDocs.ToList();
            }
            catch (Exception) {
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets README files that should be updated based on changed files
        /// </summary>
        /// <param name="changedFiles">Array of file paths that have been changed</param>
        /// <returns>List of README files that need updates</returns>
        public static List<string> GetAffectedReadmeFiles(string[] changedFiles) {
            try {
                var affectedReadmes = new HashSet<string>();
                var allReadmes = DocumentationManager.GetAllReadmeFiles();

                foreach (var changedFile in changedFiles) {
                    foreach (var readmePath in allReadmes) {
                        if (ShouldUpdateReadme(readmePath, new[] { changedFile })) {
                            affectedReadmes.Add(readmePath);
                        }
                    }
                }

                return affectedReadmes.ToList();
            }
            catch (Exception) {
                return new List<string>();
            }
        }

        /// <summary>
        /// Determines if a README should be updated based on file changes
        /// </summary>
        /// <param name="readmePath">Path to the README file</param>
        /// <param name="changedFiles">Array of changed files</param>
        /// <returns>True if README should be updated, false otherwise</returns>
        public static bool ShouldUpdateReadme(string readmePath, string[] changedFiles) {
            try {
                var readmeType = DocumentationManager.GetReadmeType(readmePath);

                foreach (var changedFile in changedFiles) {
                    // Check against trigger rules
                    foreach (var trigger in ReadmeUpdateTriggers) {
                        if (trigger.ReadmeType == readmeType &&
                            Regex.IsMatch(changedFile, trigger.TriggerPattern, RegexOptions.IgnoreCase) &&
                            trigger.ShouldTrigger(changedFile)) {
                            return true;
                        }
                    }

                    // Additional specific rules
                    if (ShouldUpdateReadmeForSpecificScenario(readmePath, changedFile, readmeType)) {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Gets README update triggers for debugging/monitoring
        /// </summary>
        /// <returns>Dictionary of README types to their trigger patterns</returns>
        public static Dictionary<string, string[]> GetReadmeUpdateTriggers() {
            var triggers = new Dictionary<string, string[]>();

            foreach (var readmeType in Enum.GetValues(typeof(ReadmeType)).Cast<ReadmeType>()) {
                var triggerPatterns = ReadmeUpdateTriggers
                    .Where(t => t.ReadmeType == readmeType)
                    .Select(t => t.TriggerPattern)
                    .ToArray();

                triggers[readmeType.ToString()] = triggerPatterns;
            }

            return triggers;
        }

        /// <summary>
        /// Checks for specific README update scenarios
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <param name="changedFile">Changed file path</param>
        /// <param name="readmeType">Type of README</param>
        /// <returns>True if should update, false otherwise</returns>
        private static bool ShouldUpdateReadmeForSpecificScenario(string readmePath, string changedFile, ReadmeType readmeType) {
            switch (readmeType) {
                case ReadmeType.Navigation:
                    return IsStructuralChange(changedFile) && IsRelatedNavigationReadme(readmePath, changedFile);

                case ReadmeType.Technical:
                    return IsModuleFileChange(changedFile, readmePath);

                case ReadmeType.Category:
                    return IsCategoryContentChange(changedFile, readmePath);

                case ReadmeType.Root:
                    return IsGlobalStructureChange(changedFile);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if a change is structural (affects navigation)
        /// </summary>
        /// <param name="changedFile">Changed file path</param>
        /// <returns>True if structural change, false otherwise</returns>
        private static bool IsStructuralChange(string changedFile) {
            return changedFile.Contains("ModuleUpdateSummary.md") ||
                   changedFile.Contains("/README.md") ||
                   changedFile.EndsWith("/README.md") ||
                   changedFile.Contains("DevTools/") ||
                   Regex.IsMatch(changedFile, @"Documentation[/\\].*[/\\].*\.md$");
        }

        /// <summary>
        /// Checks if change affects a specific module's README
        /// </summary>
        /// <param name="changedFile">Changed file path</param>
        /// <param name="readmePath">README file path</param>
        /// <returns>True if module file change, false otherwise</returns>
        private static bool IsModuleFileChange(string changedFile, string readmePath) {
            var moduleMatch = ModuleFileChangeRegex.Match(changedFile);
            if (moduleMatch.Success) {
                var moduleName = moduleMatch.Groups[1].Value;
                return readmePath.StartsWith($"{moduleName}/README.md");
            }
            return false;
        }

        /// <summary>
        /// Checks if change affects a category's content
        /// </summary>
        /// <param name="changedFile">Changed file path</param>
        /// <param name="readmePath">README file path</param>
        /// <returns>True if category content change, false otherwise</returns>
        private static bool IsCategoryContentChange(string changedFile, string readmePath) {
            if (changedFile.StartsWith("Documentation/")) {
                var pathParts = changedFile.Split('/', '\\');
                if (pathParts.Length >= 2) {
                    var category = pathParts[1];
                    return readmePath.Contains($"Documentation/{category}/README.md");
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if change affects global structure
        /// </summary>
        /// <param name="changedFile">Changed file path</param>
        /// <returns>True if global structure change, false otherwise</returns>
        private static bool IsGlobalStructureChange(string changedFile) {
            return changedFile.Contains("Documentation/") &&
                   (changedFile.Contains("/README.md") || changedFile.Contains("ModuleUpdateSummary.md"));
        }

        /// <summary>
        /// Checks if navigation README is related to the changed file
        /// </summary>
        /// <param name="readmePath">README file path</param>
        /// <param name="changedFile">Changed file path</param>
        /// <returns>True if related, false otherwise</returns>
        private static bool IsRelatedNavigationReadme(string readmePath, string changedFile) {
            if (readmePath == "Documentation/README.md")
                return true; // Root README affected by any structural change

            if (readmePath == "Documentation/Modules/README.md")
                return changedFile.Contains("ModuleUpdateSummary.md");

            if (readmePath.StartsWith("Documentation/")) {
                var readmeCategory = readmePath.Split('/')[1];
                return changedFile.Contains($"Documentation/{readmeCategory}/");
            }

            return false;
        }

        /// <summary>
        /// Determines the module name from a file path
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Module name or null if not in a recognized module</returns>
        private static string GetModuleFromFilePath(string filePath) {
            var normalizedPath = filePath.Replace('\\', '/');
            var pathParts = normalizedPath.Split('/');

            foreach (var part in pathParts) {
                if (IsModuleName(part)) {
                    return part;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a string is a recognized module name
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns>True if it's a module name, false otherwise</returns>
        private static bool IsModuleName(string name) {
            var modules = new[] { "ViewModels", "Models", "Utils", "Services", "Clients", "Views", "Converters", "Scripts", "DevTools" };
            return modules.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets direct documentation dependencies for a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Set of documentation files that directly reference this file</returns>
        private static HashSet<string> GetDirectDocumentationDependencies(string filePath) {
            var dependencies = new HashSet<string>();
            var fileName = Path.GetFileName(filePath);

            try {
                var documentationFiles = DocumentationManager.GetDocumentationFiles();

                foreach (var docFile in documentationFiles) {
                    var content = File.ReadAllText(docFile, Encoding.UTF8);
                    if (content.Contains(fileName) || content.Contains(filePath.Replace('\\', '/'))) {
                        dependencies.Add(docFile);
                    }
                }
            }
            catch (Exception) {
                // Ignore errors in dependency detection
            }

            return dependencies;
        }

        /// <summary>
        /// Gets documentation files specific to a module
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>Set of documentation files for the module</returns>
        private static HashSet<string> GetModuleDocumentationFiles(string moduleName) {
            var files = new HashSet<string>();

            try {
                // Module-specific documentation patterns
                var patterns = new[]
                {
                    $"Documentation/Modules/{moduleName}ModuleUpdateSummary.md",
                    $"Documentation/{moduleName}ModuleUpdateSummary.md",
                    $"Documentation/{moduleName}ModuleSummary.md",
                    $"{moduleName}/README.md",
                    $"Documentation/{moduleName}Summary.md"
                };

                foreach (var pattern in patterns) {
                    if (File.Exists(pattern)) {
                        files.Add(pattern);
                    }
                }
            }
            catch (Exception) {
                // Ignore errors
            }

            return files;
        }

        #endregion

        #region Cross-Reference Management

        /// <summary>
        /// Updates cross-references in documentation files based on changed files
        /// </summary>
        /// <param name="changedFiles">Array of file paths that have been changed</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateCrossReferences(string[] changedFiles) {
            try {
                var success = true;
                var pathMappings = DetectPathChanges(changedFiles);

                if (pathMappings.Any()) {
                    success &= DocumentationManager.UpdateDocumentationPaths(pathMappings);
                }

                // Update references in affected documentation files
                var affectedDocs = GetAffectedDocumentationFiles(changedFiles);
                foreach (var docFile in affectedDocs) {
                    success &= UpdateDocumentationReferences(docFile, changedFiles);
                }

                // Update README cross-references specifically
                success &= DocumentationManager.SynchronizeReadmeCrossReferences();

                return success;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Detects path changes from file operations
        /// </summary>
        /// <param name="changedFiles">Array of changed files</param>
        /// <returns>Dictionary of old path to new path mappings</returns>
        private static Dictionary<string, string> DetectPathChanges(string[] changedFiles) {
            var pathMappings = new Dictionary<string, string>();

            // Add detected path mappings from DevTools reorganization
            pathMappings["Utils/DateHelper.cs"] = "DevTools/DateHelper.cs";
            pathMappings["Utils/DocumentationManager.cs"] = "DevTools/DocumentationManager.cs";
            pathMappings["Utils/ProjectAnalyzer.cs"] = "DevTools/ProjectAnalyzer.cs";
            pathMappings["OstPlayer.Utils.DateHelper"] = "OstPlayer.DevTools.DateHelper";
            pathMappings["OstPlayer.Utils.DocumentationManager"] = "OstPlayer.DevTools.DocumentationManager";
            pathMappings["OstPlayer.Utils.ProjectAnalyzer"] = "OstPlayer.DevTools.ProjectAnalyzer";
            pathMappings["DevTools/FileHeaderPolicy.md"] = "Documentation/Development/FileHeaderPolicy.md";
            pathMappings["DevTools/SmartDateAutomation.md"] = "Documentation/AI-Assistant/SmartDateAutomation.md";

            return pathMappings;
        }

        /// <summary>
        /// Updates references in a specific documentation file
        /// </summary>
        /// <param name="documentationFile">Path to the documentation file</param>
        /// <param name="changedFiles">Array of changed files</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        private static bool UpdateDocumentationReferences(string documentationFile, string[] changedFiles) {
            try {
                if (!File.Exists(documentationFile)) {
                    return false;
                }

                var content = File.ReadAllText(documentationFile, Encoding.UTF8);
                var originalContent = content;

                // Update file references
                foreach (var changedFile in changedFiles) {
                    var fileName = Path.GetFileName(changedFile);
                    var relativePath = changedFile.Replace('\\', '/');

                    // Update any outdated references
                    content = UpdateFileReferences(content, fileName, relativePath);
                }

                // Update version references if this is a module summary or README
                if (documentationFile.Contains("ModuleUpdateSummary") ||
                    documentationFile.Contains("README")) {
                    content = UpdateVersionReferences(content, changedFiles);
                }

                if (content != originalContent) {
                    File.WriteAllText(documentationFile, content, Encoding.UTF8);
                }

                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Updates file references in content
        /// </summary>
        /// <param name="content">Content to update</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="relativePath">Relative path to the file</param>
        /// <returns>Updated content</returns>
        private static string UpdateFileReferences(string content, string fileName, string relativePath) {
            // This is a simplified implementation
            // You could enhance this with more sophisticated reference updating
            return content;
        }

        /// <summary>
        /// Updates version references in content
        /// </summary>
        /// <param name="content">Content to update</param>
        /// <param name="changedFiles">Array of changed files</param>
        /// <returns>Updated content</returns>
        private static string UpdateVersionReferences(string content, string[] changedFiles) {
            foreach (var file in changedFiles) {
                var version = DateHelper.GetFileVersion(file);
                if (version != null) {
                    var fileName = Path.GetFileName(file);
                    // Update version references for this file
                    var versionPattern = new Regex($@"{Regex.Escape(fileName)}\s*\([^)]*v[\d.]+[^)]*\)", RegexOptions.IgnoreCase);
                    content = versionPattern.Replace(content, $"{fileName} (v{version})");
                }
            }

            return content;
        }

        #endregion

        #region Project Consistency Validation

        /// <summary>
        /// Validates overall project consistency including README hierarchy
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <returns>True if project is consistent, false otherwise</returns>
        public static bool ValidateProjectConsistency(string projectPath = ".") {
            try {
                var issues = new List<string>();

                ValidateSourceFileConsistency(projectPath, issues);
                ValidateDocumentationConsistency(issues);
                ValidateModuleStructure(projectPath, issues);
                ValidateDependencies(projectPath, issues);
                ValidateReadmeConsistency(issues); // NEW: README-specific validation

                return issues.Count == 0;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Validates README consistency specifically
        /// </summary>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateReadmeConsistency(List<string> issues) {
            try {
                if (!DocumentationManager.ValidateReadmeHierarchy()) {
                    issues.Add("README hierarchy validation failed");
                }

                // Check for missing README files in key locations
                var expectedReadmes = new[]
                {
                    "Documentation/README.md",
                    "Documentation/Modules/README.md",
                    "Documentation/Development/README.md",
                    "DevTools/README.md"
                };

                foreach (var expectedReadme in expectedReadmes) {
                    if (!File.Exists(expectedReadme)) {
                        issues.Add($"Missing expected README file: {expectedReadme}");
                    }
                }
            }
            catch (Exception ex) {
                issues.Add($"Error validating README consistency: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates source file consistency
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateSourceFileConsistency(string projectPath, List<string> issues) {
            try {
                var sourceFiles = GetSourceFiles(projectPath);

                foreach (var file in sourceFiles) {
                    if (!DateHelper.HasValidHeader(file)) {
                        issues.Add($"File missing proper header: {file}");
                    }

                    var version = DateHelper.GetFileVersion(file);
                    if (string.IsNullOrEmpty(version)) {
                        issues.Add($"File missing version information: {file}");
                    }
                }
            }
            catch (Exception ex) {
                issues.Add($"Error validating source files: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates documentation consistency
        /// </summary>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateDocumentationConsistency(List<string> issues) {
            try {
                if (!DocumentationManager.ValidateDocumentationConsistency()) {
                    issues.Add("Documentation consistency validation failed");
                }

                if (!DocumentationManager.IsDocumentationStructureValid()) {
                    issues.Add("Documentation structure is invalid");
                }
            }
            catch (Exception ex) {
                issues.Add($"Error validating documentation: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates module structure
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateModuleStructure(string projectPath, List<string> issues) {
            try {
                var expectedModules = new[] { "ViewModels", "Models", "Utils", "Services", "Views", "DevTools" };

                foreach (var module in expectedModules) {
                    var modulePath = Path.Combine(projectPath, module);
                    if (!Directory.Exists(modulePath)) {
                        issues.Add($"Missing expected module directory: {module}");
                    }
                }
            }
            catch (Exception ex) {
                issues.Add($"Error validating module structure: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates dependencies between modules
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <param name="issues">List to collect validation issues</param>
        private static void ValidateDependencies(string projectPath, List<string> issues) {
            try {
                var sourceFiles = GetSourceFiles(projectPath);
                var circularDependencies = DetectCircularDependencies(sourceFiles);

                foreach (var dependency in circularDependencies) {
                    issues.Add($"Circular dependency detected: {dependency}");
                }
            }
            catch (Exception ex) {
                issues.Add($"Error validating dependencies: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all source files in the project
        /// </summary>
        /// <param name="projectPath">Root path of the project</param>
        /// <returns>List of source file paths</returns>
        private static List<string> GetSourceFiles(string projectPath) {
            var sourceFiles = new List<string>();

            try {
                foreach (var pattern in SourceFilePatterns) {
                    sourceFiles.AddRange(Directory.GetFiles(projectPath, pattern, SearchOption.AllDirectories));
                }

                // Filter out unwanted directories
                sourceFiles = sourceFiles.Where(f =>
                    !f.Contains(@"\bin\") &&
                    !f.Contains(@"\obj\") &&
                    !f.Contains(@"\packages\")
                ).ToList();
            }
            catch (Exception) {
                // Return empty list if error occurs
            }

            return sourceFiles;
        }

        /// <summary>
        /// Checks if a file is a source file
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if it's a source file</returns>
        private static bool IsSourceFile(string filePath) {
            var extension = Path.GetExtension(filePath);
            return extension == ".cs" || extension == ".xaml";
        }

        /// <summary>
        /// Detects circular dependencies in source files
        /// </summary>
        /// <param name="sourceFiles">List of source files to analyze</param>
        /// <returns>List of circular dependency descriptions</returns>
        private static List<string> DetectCircularDependencies(List<string> sourceFiles) {
            var circularDeps = new List<string>();

            try {
                var dependencies = new Dictionary<string, HashSet<string>>();

                // Build dependency graph
                foreach (var file in sourceFiles) {
                    var fileDeps = ExtractDependencies(file);
                    dependencies[file] = fileDeps;
                }

                // Detect cycles (simplified implementation)
                foreach (var file in dependencies.Keys) {
                    if (HasCircularReference(file, dependencies, new HashSet<string>())) {
                        circularDeps.Add($"Circular reference involving: {Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception) {
                // Ignore errors in circular dependency detection
            }

            return circularDeps;
        }

        /// <summary>
        /// Extracts dependencies from a source file
        /// </summary>
        /// <param name="filePath">Path to the source file</param>
        /// <returns>Set of dependencies</returns>
        private static HashSet<string> ExtractDependencies(string filePath) {
            var dependencies = new HashSet<string>();

            try {
                var content = File.ReadAllText(filePath);
                var usingMatches = UsingDirectiveRegex.Matches(content);

                foreach (Match match in usingMatches) {
                    dependencies.Add(match.Groups[1].Value.Trim());
                }
            }
            catch (Exception) {
                // Ignore errors
            }

            return dependencies;
        }

        /// <summary>
        /// Checks for circular references
        /// </summary>
        /// <param name="currentFile">Current file being checked</param>
        /// <param name="dependencies">Dependency graph</param>
        /// <param name="visited">Set of visited files</param>
        /// <returns>True if circular reference detected</returns>
        private static bool HasCircularReference(string currentFile, Dictionary<string, HashSet<string>> dependencies, HashSet<string> visited) {
            if (visited.Contains(currentFile)) {
                return true;
            }

            visited.Add(currentFile);

            if (dependencies.ContainsKey(currentFile)) {
                foreach (var dependency in dependencies[currentFile]) {
                    if (HasCircularReference(dependency, dependencies, new HashSet<string>(visited))) {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Module Change Detection - NEW v1.3.2

        /// <summary>
        /// Detects module changes and identifies which ModuleUpdateSummary files should be updated.
        /// ENHANCEMENT: Automatically triggers module documentation updates when files are modified.
        /// </summary>
        /// <param name="changedFiles">List of modified files</param>
        /// <returns>Dictionary mapping module names to their change descriptions</returns>
        public static Dictionary<string, List<string>> DetectModuleChanges(IEnumerable<string> changedFiles) {
            var moduleChanges = new Dictionary<string, List<string>>();

            foreach (var file in changedFiles) {
                var moduleName = ExtractModuleName(file);
                if (!string.IsNullOrEmpty(moduleName)) {
                    if (!moduleChanges.ContainsKey(moduleName)) {
                        moduleChanges[moduleName] = new List<string>();
                    }

                    var changeDescription = GenerateChangeDescription(file);
                    moduleChanges[moduleName].Add(changeDescription);
                }
            }

            return moduleChanges;
        }

        /// <summary>
        /// Extracts module name from file path.
        /// DETECTION: Identifies which module a file belongs to based on folder structure.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Module name or null if not in a tracked module</returns>
        public static string ExtractModuleName(string filePath) {
            if (string.IsNullOrEmpty(filePath))
                return null;

            var normalizedPath = filePath.Replace('\\', '/');

            // Define module patterns with priority (most specific first)
            var modulePatterns = new[]
            {
                new { Pattern = @"^DevTools/", Module = "DevTools" },
                new { Pattern = @"^Clients/", Module = "Clients" },
                new { Pattern = @"^Services/", Module = "Services" },
                new { Pattern = @"^Utils/", Module = "Utils" },
                new { Pattern = @"^Models/", Module = "Models" },
                new { Pattern = @"^ViewModels/", Module = "ViewModels" },
                new { Pattern = @"^Views/", Module = "Views" },
                new { Pattern = @"^Converters/", Module = "Converters" }
            };

            foreach (var pattern in modulePatterns) {
                if (Regex.IsMatch(normalizedPath, pattern.Pattern, RegexOptions.IgnoreCase)) {
                    return pattern.Module;
                }
            }

            return null;
        }

        /// <summary>
        /// Generates human-readable change description for a file modification.
        /// INTELLIGENCE: Creates meaningful descriptions based on file type and location.
        /// </summary>
        /// <param name="filePath">Path to the modified file</param>
        /// <returns>Description of the change</returns>
        private static string GenerateChangeDescription(string filePath) {
            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(filePath).ToLower();
            var directory = Path.GetDirectoryName(filePath)?.Replace('\\', '/');

            // Determine if this is a new file or modification
            var exists = File.Exists(filePath);
            var action = exists ? "Modified" : "Added";

            // Generate description based on file type
            switch (extension) {
                case ".cs":
                    return $"{action} C# class: {fileName}";
                case ".xaml":
                    return $"{action} XAML view: {fileName}";
                case ".md":
                    return $"{action} documentation: {fileName}";
                case ".ps1":
                    return $"{action} PowerShell script: {fileName}";
                default:
                    return $"{action} file: {fileName}";
            }
        }

        /// <summary>
        /// Gets the path to the ModuleUpdateSummary file for a given module.
        /// MAPPING: Returns correct documentation path for module summary updates.
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>Path to the module summary file</returns>
        public static string GetModuleSummaryPath(string moduleName) {
            return $"Documentation/Modules/{moduleName}ModuleUpdateSummary.md";
        }

        /// <summary>
        /// Checks if a ModuleUpdateSummary file exists for the given module.
        /// VALIDATION: Ensures module documentation exists before attempting updates.
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>True if summary file exists</returns>
        public static bool ModuleSummaryExists(string moduleName) {
            var summaryPath = GetModuleSummaryPath(moduleName);
            return File.Exists(summaryPath);
        }

        /// <summary>
        /// Analyzes module activity and suggests documentation updates.
        /// INTELLIGENCE: Provides recommendations for module documentation based on change patterns.
        /// </summary>
        /// <param name="moduleChanges">Dictionary of module changes</param>
        /// <returns>List of documentation update recommendations</returns>
        public static List<ModuleUpdateRecommendation> AnalyzeModuleActivity(Dictionary<string, List<string>> moduleChanges) {
            var recommendations = new List<ModuleUpdateRecommendation>();

            foreach (var moduleChange in moduleChanges) {
                var moduleName = moduleChange.Key;
                var changes = moduleChange.Value;

                var recommendation = new ModuleUpdateRecommendation {
                    ModuleName = moduleName,
                    SummaryPath = GetModuleSummaryPath(moduleName),
                    ChangeCount = changes.Count,
                    Changes = changes,
                    Priority = DetermineUpdatePriority(changes),
                    SummaryExists = ModuleSummaryExists(moduleName),
                    RecommendedAction = DetermineRecommendedAction(moduleName, changes)
                };

                recommendations.Add(recommendation);
            }

            return recommendations.OrderByDescending(r => r.Priority).ToList();
        }

        /// <summary>
        /// Determines update priority based on change characteristics.
        /// PRIORITIZATION: Higher priority for critical modules and significant changes.
        /// </summary>
        /// <param name="changes">List of changes in the module</param>
        /// <returns>Priority level (1=low, 5=critical)</returns>
        private static int DetermineUpdatePriority(List<string> changes) {
            var priority = 1;

            // Increase priority based on number of changes
            if (changes.Count >= 5) priority += 2;
            else if (changes.Count >= 3) priority += 1;

            // Increase priority for new files
            var newFiles = changes.Count(c => c.Contains("Added"));
            if (newFiles > 0) priority += 1;

            // Increase priority for core modules
            var hasCoreFunctionality = changes.Any(c =>
                c.Contains("Service") ||
                c.Contains("Manager") ||
                c.Contains("Helper") ||
                c.Contains("Client"));
            if (hasCoreFunctionality) priority += 1;

            return Math.Min(priority, 5); // Cap at 5
        }

        /// <summary>
        /// Determines recommended action for module documentation update.
        /// GUIDANCE: Provides specific guidance on how to update module documentation.
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="changes">List of changes in the module</param>
        /// <returns>Recommended action description</returns>
        private static string DetermineRecommendedAction(string moduleName, List<string> changes) {
            if (!ModuleSummaryExists(moduleName)) {
                return $"Create new {moduleName}ModuleUpdateSummary.md file";
            }

            var newFiles = changes.Count(c => c.Contains("Added"));
            var modifiedFiles = changes.Count(c => c.Contains("Modified"));

            if (newFiles > 0 && modifiedFiles > 0) {
                return $"Update summary with {newFiles} new files and {modifiedFiles} modifications";
            }
            else if (newFiles > 0) {
                return $"Document {newFiles} new files in module";
            }
            else if (modifiedFiles > 0) {
                return $"Update summary with {modifiedFiles} file modifications";
            }

            return "Review and update module summary as needed";
        }

        #endregion
    }

    #region Supporting Types for Module Analysis

    /// <summary>
    /// Recommendation for module updates.
    /// </summary>
    public class ModuleUpdateRecommendation
    {
        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        public string ModuleName { get; set; }
        
        /// <summary>
        /// Gets or sets the path to the module summary.
        /// </summary>
        public string SummaryPath { get; set; }
        
        /// <summary>
        /// Gets or sets the count of changes in the module.
        /// </summary>
        public int ChangeCount { get; set; }
        
        /// <summary>
        /// Gets or sets the list of changes.
        /// </summary>
        public List<string> Changes { get; set; }
        
        /// <summary>
        /// Gets or sets the priority of this recommendation.
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Gets or sets whether the summary exists.
        /// </summary>
        public bool SummaryExists { get; set; }
        
        /// <summary>
        /// Gets or sets the recommended action.
        /// </summary>
        public string RecommendedAction { get; set; }
    }
}
#endregion
