// ====================================================================
// FILE: ReadmeUpdateRules.cs
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
// Smart README update rules engine for the OstPlayer project. This utility provides
// intelligent decision-making for when and how README files should be updated based
// on various file change patterns and project structure modifications. Designed for
// AI assistants to automate README maintenance with minimal user intervention.
//
// FEATURES:
// - Intelligent README update triggering based on file change patterns
// - Context-aware decision making for different README types
// - Automated cascade update detection for hierarchical README structures
// - Customizable rule sets for different project scenarios
// - Performance-optimized pattern matching for real-time analysis
//
// DEPENDENCIES:
// - System.IO for file operations
// - System.Text.RegularExpressions for pattern matching
// - System.Collections.Generic for collections
// - System.Linq for LINQ operations
// - OstPlayer.DevTools.DocumentationManager for README type detection
//
// DESIGN PATTERNS:
// - Rules Engine Pattern - Configurable rule evaluation
// - Strategy Pattern - Different strategies for different README types
// - Chain of Responsibility Pattern - Cascading rule evaluation
// - Factory Pattern - Rule creation and management
//
// ALGORITHMS:
// - Pattern matching algorithms for file change detection
// - Hierarchical impact analysis for cascade updates
// - Priority-based rule evaluation for conflicting scenarios
// - Optimized regex compilation for performance
//
// PERFORMANCE NOTES:
// - Compiled regex patterns for fast repeated evaluation
// - Cached rule evaluations for identical scenarios
// - Minimal file I/O operations during rule evaluation
// - Efficient string pattern matching algorithms
//
// FUTURE REFACTORING:
// FUTURE: Add machine learning for adaptive rule refinement
// FUTURE: Implement user-customizable rule configuration
// FUTURE: Add rule performance analytics and optimization
// FUTURE: Create visual rule dependency mapping
// CONSIDER: Integration with version control for change pattern learning
// CONSIDER: Real-time rule effectiveness monitoring
//
// TESTING:
// - Unit tests for each rule type and scenario
// - Integration tests with real project structures
// - Performance tests for rule evaluation speed
// - Accuracy tests for rule trigger reliability
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
//
// CHANGELOG:
// 2025-08-07 v1.3.0 - Initial implementation: Smart README update rules engine with intelligent triggering logic
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// Context information for README updates.
    /// </summary>
    public class UpdateContext
    {
        /// <summary>
        /// Gets or sets the path of the changed file.
        /// </summary>
        public string ChangedFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the README file.
        /// </summary>
        public string ReadmePath { get; set; }

        /// <summary>
        /// Gets or sets the type of README.
        /// </summary>
        public ReadmeType ReadmeType { get; set; }

        /// <summary>
        /// Gets or sets the time of the change.
        /// </summary>
        public DateTime ChangeTime { get; set; }

        /// <summary>
        /// Gets or sets the description of the change.
        /// </summary>
        public string ChangeDescription { get; set; }

        /// <summary>
        /// Gets or sets whether this is a structural change.
        /// </summary>
        public bool IsStructuralChange { get; set; }

        /// <summary>
        /// Gets or sets whether this is a new file.
        /// </summary>
        public bool IsNewFile { get; set; }

        /// <summary>
        /// Gets or sets whether this is a deleted file.
        /// </summary>
        public bool IsDeletedFile { get; set; }
    }

    /// <summary>
    /// Result of rule evaluation for README updates.
    /// </summary>
    public class RuleEvaluationResult
    {
        /// <summary>
        /// Gets or sets whether the README should be updated.
        /// </summary>
        public bool ShouldUpdate { get; set; }

        /// <summary>
        /// Gets or sets the reason for the update decision.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the priority of the update.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the list of suggested actions.
        /// </summary>
        public List<string> SuggestedActions { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the evaluation.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the RuleEvaluationResult class.
        /// </summary>
        public RuleEvaluationResult()
        {
            SuggestedActions = new List<string>();
            Metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Smart README update rules engine for intelligent automation
    /// </summary>
    public static class ReadmeUpdateRules
    {
        // Compiled regex patterns for performance
        private static readonly Dictionary<string, Regex> CompiledPatterns = new Dictionary<
            string,
            Regex
        >
        {
            ["ModuleFile"] = new Regex(
                @"^(ViewModels|Models|Utils|Services|Clients|Views|Converters|DevTools)[/\\].*\.(cs|xaml)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            ),
            ["ModuleSummary"] = new Regex(
                @"Documentation[/\\]Modules[/\\].*ModuleUpdateSummary\.md$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            ),
            ["NavigationReadme"] = new Regex(
                @"Documentation[/\\].*[/\\]README\.md$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            ),
            ["CategoryDocument"] = new Regex(
                @"Documentation[/\\](Development|Analysis|Refactoring|AI-Assistant)[/\\].*\.md$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            ),
            ["DevToolsFile"] = new Regex(
                @"DevTools[/\\].*\.(cs|ps1)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            ),
            ["ConfigurationFile"] = new Regex(
                @".*\.(config|json|xml|csproj)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            ),
        };

        // Rule cache for performance
        private static readonly Dictionary<string, RuleEvaluationResult> RuleCache =
            new Dictionary<string, RuleEvaluationResult>();
        private static DateTime _cacheLastCleaned = DateTime.Now;

        #region Main Rule Evaluation

        /// <summary>
        /// Determines if a README should be updated based on file changes using smart rules
        /// </summary>
        /// <param name="readmePath">Path to the README file</param>
        /// <param name="changedFile">Path to the changed file</param>
        /// <returns>Rule evaluation result with decision and metadata</returns>
        public static RuleEvaluationResult ShouldUpdateReadme(string readmePath, string changedFile)
        {
            try
            {
                var context = CreateUpdateContext(readmePath, changedFile);
                var cacheKey = $"{readmePath}|{changedFile}";

                // Check cache first
                if (RuleCache.ContainsKey(cacheKey))
                {
                    return RuleCache[cacheKey];
                }

                var result = EvaluateUpdateRules(context);

                // Cache result
                CacheResult(cacheKey, result);

                return result;
            }
            catch (Exception ex)
            {
                return new RuleEvaluationResult
                {
                    ShouldUpdate = false,
                    Reason = $"Error evaluating rules: {ex.Message}",
                    Priority = 0,
                };
            }
        }

        /// <summary>
        /// Evaluates multiple README files for updates based on a single file change
        /// </summary>
        /// <param name="changedFile">Path to the changed file</param>
        /// <param name="readmeFiles">List of README files to evaluate</param>
        /// <returns>Dictionary of README paths to evaluation results</returns>
        public static Dictionary<string, RuleEvaluationResult> EvaluateMultipleReadmes(
            string changedFile,
            IEnumerable<string> readmeFiles
        )
        {
            var results = new Dictionary<string, RuleEvaluationResult>();

            foreach (var readmePath in readmeFiles)
            {
                results[readmePath] = ShouldUpdateReadme(readmePath, changedFile);
            }

            return results;
        }

        /// <summary>
        /// Gets README files that should be updated with their priority order
        /// </summary>
        /// <param name="changedFile">Path to the changed file</param>
        /// <returns>List of README paths ordered by update priority</returns>
        public static List<string> GetPrioritizedReadmeUpdates(string changedFile)
        {
            var allReadmes = DocumentationManager.GetAllReadmeFiles();
            var evaluations = EvaluateMultipleReadmes(changedFile, allReadmes);

            return evaluations
                .Where(kvp => kvp.Value.ShouldUpdate)
                .OrderByDescending(kvp => kvp.Value.Priority)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        #endregion

        #region Rule Creation and Context

        /// <summary>
        /// Creates update context from file paths
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <param name="changedFile">Path to changed file</param>
        /// <returns>Update context for rule evaluation</returns>
        private static UpdateContext CreateUpdateContext(string readmePath, string changedFile)
        {
            return new UpdateContext
            {
                ChangedFile = changedFile,
                ReadmePath = readmePath,
                ReadmeType = DocumentationManager.GetReadmeType(readmePath),
                ChangeTime = DateTime.Now,
                IsStructuralChange = IsStructuralChange(changedFile),
                IsNewFile = IsNewFile(changedFile),
                IsDeletedFile = IsDeletedFile(changedFile),
                ChangeDescription = GenerateChangeDescription(changedFile),
            };
        }

        /// <summary>
        /// Evaluates update rules for the given context
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>Rule evaluation result</returns>
        private static RuleEvaluationResult EvaluateUpdateRules(UpdateContext context)
        {
            var result = new RuleEvaluationResult();

            // Evaluate rules based on README type
            switch (context.ReadmeType)
            {
                case ReadmeType.Root:
                    result = EvaluateRootReadmeRules(context);
                    break;
                case ReadmeType.Navigation:
                    result = EvaluateNavigationReadmeRules(context);
                    break;
                case ReadmeType.Technical:
                    result = EvaluateTechnicalReadmeRules(context);
                    break;
                case ReadmeType.Category:
                    result = EvaluateCategoryReadmeRules(context);
                    break;
                default:
                    result = EvaluateDefaultRules(context);
                    break;
            }

            // Add metadata
            result.Metadata["ReadmeType"] = context.ReadmeType.ToString();
            result.Metadata["ChangeTime"] = context.ChangeTime;
            result.Metadata["IsStructural"] = context.IsStructuralChange;

            return result;
        }

        #endregion

        #region Specific Rule Evaluators

        /// <summary>
        /// Evaluates rules for root README (Documentation/README.md)
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>Rule evaluation result</returns>
        private static RuleEvaluationResult EvaluateRootReadmeRules(UpdateContext context)
        {
            var result = new RuleEvaluationResult();

            // High priority: New major documentation structure
            if (CompiledPatterns["NavigationReadme"].IsMatch(context.ChangedFile))
            {
                result.ShouldUpdate = true;
                result.Reason = "Major documentation structure change detected";
                result.Priority = 10;
                result.SuggestedActions = new List<string>
                {
                    "Update documentation structure overview",
                    "Refresh navigation links",
                };
                return result;
            }

            // Medium priority: New module summaries
            if (CompiledPatterns["ModuleSummary"].IsMatch(context.ChangedFile))
            {
                result.ShouldUpdate = true;
                result.Reason = "New module summary added";
                result.Priority = 7;
                result.SuggestedActions = new List<string> { "Update module navigation section" };
                return result;
            }

            // Low priority: New categories
            if (
                CompiledPatterns["CategoryDocument"].IsMatch(context.ChangedFile)
                && context.IsNewFile
            )
            {
                result.ShouldUpdate = true;
                result.Reason = "New documentation category detected";
                result.Priority = 5;
                result.SuggestedActions = new List<string>
                {
                    "Consider adding category to main navigation",
                };
                return result;
            }

            result.ShouldUpdate = false;
            result.Reason = "No root README update required";
            return result;
        }

        /// <summary>
        /// Evaluates rules for navigation README files
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>Rule evaluation result</returns>
        private static RuleEvaluationResult EvaluateNavigationReadmeRules(UpdateContext context)
        {
            var result = new RuleEvaluationResult();
            var readmeCategory = ExtractCategoryFromReadmePath(context.ReadmePath);

            // High priority: New files in same category
            if (IsSameCategoryChange(context.ChangedFile, readmeCategory))
            {
                result.ShouldUpdate = true;
                result.Reason = $"New content added to {readmeCategory} category";
                result.Priority = 9;
                result.SuggestedActions = new List<string>
                {
                    $"Add {Path.GetFileName(context.ChangedFile)} to {readmeCategory} navigation",
                };
                return result;
            }

            // Medium priority: Module summaries for Modules README
            if (
                readmeCategory == "Modules"
                && CompiledPatterns["ModuleSummary"].IsMatch(context.ChangedFile)
            )
            {
                result.ShouldUpdate = true;
                result.Reason = "Module summary updated";
                result.Priority = 8;
                result.SuggestedActions = new List<string> { "Update module summary links" };
                return result;
            }

            // Low priority: Related structural changes
            if (
                context.IsStructuralChange
                && IsRelatedToCategory(context.ChangedFile, readmeCategory)
            )
            {
                result.ShouldUpdate = true;
                result.Reason = "Related structural change detected";
                result.Priority = 4;
                result.SuggestedActions = new List<string> { "Review navigation structure" };
                return result;
            }

            result.ShouldUpdate = false;
            result.Reason = "No navigation README update required";
            return result;
        }

        /// <summary>
        /// Evaluates rules for technical README files (module READMEs)
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>Rule evaluation result</returns>
        private static RuleEvaluationResult EvaluateTechnicalReadmeRules(UpdateContext context)
        {
            var result = new RuleEvaluationResult();
            var moduleFromReadme = ExtractModuleFromReadmePath(context.ReadmePath);
            var moduleFromFile = ExtractModuleFromFilePath(context.ChangedFile);

            // High priority: New files in same module
            if (
                moduleFromReadme == moduleFromFile
                && CompiledPatterns["ModuleFile"].IsMatch(context.ChangedFile)
            )
            {
                result.ShouldUpdate = true;
                result.Reason = $"New file added to {moduleFromReadme} module";
                result.Priority = 10;
                result.SuggestedActions = new List<string>
                {
                    $"Add {Path.GetFileName(context.ChangedFile)} to module file list",
                };
                return result;
            }

            // Medium priority: DevTools changes for DevTools README
            if (
                moduleFromReadme == "DevTools"
                && CompiledPatterns["DevToolsFile"].IsMatch(context.ChangedFile)
            )
            {
                result.ShouldUpdate = true;
                result.Reason = "DevTools utility updated";
                result.Priority = 8;
                result.SuggestedActions = new List<string>
                {
                    "Update DevTools capabilities section",
                };
                return result;
            }

            // Low priority: Configuration changes affecting module
            if (
                CompiledPatterns["ConfigurationFile"].IsMatch(context.ChangedFile)
                && context.ChangedFile.Contains(moduleFromReadme)
            )
            {
                result.ShouldUpdate = true;
                result.Reason = "Module configuration changed";
                result.Priority = 3;
                result.SuggestedActions = new List<string> { "Review module configuration notes" };
            }

            result.ShouldUpdate = false;
            result.Reason = "No technical README update required";
            return result;
        }

        /// <summary>
        /// Evaluates rules for category README files
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>Rule evaluation result</returns>
        private static RuleEvaluationResult EvaluateCategoryReadmeRules(UpdateContext context)
        {
            var result = new RuleEvaluationResult();
            var categoryFromReadme = ExtractCategoryFromReadmePath(context.ReadmePath);

            // High priority: New documents in same category
            if (IsSameCategoryChange(context.ChangedFile, categoryFromReadme) && context.IsNewFile)
            {
                result.ShouldUpdate = true;
                result.Reason = $"New document added to {categoryFromReadme} category";
                result.Priority = 9;
                result.SuggestedActions = new List<string>
                {
                    $"Add {Path.GetFileNameWithoutExtension(context.ChangedFile)} to document list",
                };
                return result;
            }

            // Medium priority: Subcategory changes
            if (IsSubcategoryChange(context.ChangedFile, categoryFromReadme))
            {
                result.ShouldUpdate = true;
                result.Reason = "Subcategory content changed";
                result.Priority = 6;
                result.SuggestedActions = new List<string> { "Review subcategory organization" };
                return result;
            }

            result.ShouldUpdate = false;
            result.Reason = "No category README update required";
            return result;
        }

        /// <summary>
        /// Evaluates default rules for unspecified README types
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>Rule evaluation result</returns>
        private static RuleEvaluationResult EvaluateDefaultRules(UpdateContext context)
        {
            var result = new RuleEvaluationResult();

            // Conservative approach - only update for clear structural changes
            if (context.IsStructuralChange)
            {
                result.ShouldUpdate = true;
                result.Reason = "Structural change detected - manual review recommended";
                result.Priority = 2;
                result.SuggestedActions = new List<string> { "Manual review of README content" };
                return result;
            }

            result.ShouldUpdate = false;
            result.Reason = "No clear update trigger detected";
            return result;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a file change is structural (affects organization)
        /// </summary>
        /// <param name="changedFile">Path to changed file</param>
        /// <returns>True if structural change, false otherwise</returns>
        private static bool IsStructuralChange(string changedFile)
        {
            return changedFile.Contains("/README.md")
                || changedFile.Contains("\\README.md")
                || changedFile.Contains("ModuleUpdateSummary.md")
                || CompiledPatterns["NavigationReadme"].IsMatch(changedFile)
                || CompiledPatterns["CategoryDocument"].IsMatch(changedFile);
        }

        /// <summary>
        /// Checks if a file is newly created (simplified - in real scenario would check git/fs)
        /// </summary>
        /// <param name="changedFile">Path to changed file</param>
        /// <returns>True if new file, false otherwise</returns>
        private static bool IsNewFile(string changedFile)
        {
            // In a real implementation, this would check version control or file system timestamps
            // For now, assume any file change could be new
            return File.Exists(changedFile);
        }

        /// <summary>
        /// Checks if a file was deleted (simplified - in real scenario would check git/fs)
        /// </summary>
        /// <param name="changedFile">Path to changed file</param>
        /// <returns>True if deleted file, false otherwise</returns>
        private static bool IsDeletedFile(string changedFile)
        {
            // In a real implementation, this would check version control
            return !File.Exists(changedFile);
        }

        /// <summary>
        /// Generates description of the change
        /// </summary>
        /// <param name="changedFile">Path to changed file</param>
        /// <returns>Change description</returns>
        private static string GenerateChangeDescription(string changedFile)
        {
            var fileName = Path.GetFileName(changedFile);
            var extension = Path.GetExtension(changedFile).ToLower();

            switch (extension)
            {
                case ".cs":
                    return $"Source code file {fileName} modified";
                case ".md":
                    return $"Documentation file {fileName} updated";
                case ".xaml":
                    return $"UI file {fileName} changed";
                case ".ps1":
                    return $"Script file {fileName} updated";
                default:
                    return $"File {fileName} modified";
            }
        }

        /// <summary>
        /// Extracts category name from README path
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <returns>Category name or empty string</returns>
        private static string ExtractCategoryFromReadmePath(string readmePath)
        {
            var normalizedPath = readmePath.Replace('\\', '/');
            if (normalizedPath.StartsWith("Documentation/"))
            {
                var parts = normalizedPath.Split('/');
                if (parts.Length >= 2)
                {
                    return parts[1];
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Extracts module name from README path
        /// </summary>
        /// <param name="readmePath">Path to README file</param>
        /// <returns>Module name or empty string</returns>
        private static string ExtractModuleFromReadmePath(string readmePath)
        {
            var normalizedPath = readmePath.Replace('\\', '/');
            var parts = normalizedPath.Split('/');
            if (parts.Length >= 1)
            {
                var potentialModule = parts[0];
                if (IsModuleName(potentialModule))
                {
                    return potentialModule;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Extracts module name from file path
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>Module name or empty string</returns>
        private static string ExtractModuleFromFilePath(string filePath)
        {
            var normalizedPath = filePath.Replace('\\', '/');
            var parts = normalizedPath.Split('/');
            foreach (var part in parts)
            {
                if (IsModuleName(part))
                {
                    return part;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks if a name is a recognized module name
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns>True if module name, false otherwise</returns>
        private static bool IsModuleName(string name)
        {
            var modules = new[]
            {
                "ViewModels",
                "Models",
                "Utils",
                "Services",
                "Clients",
                "Views",
                "Converters",
                "DevTools",
            };
            return modules.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if file change is in same category as README
        /// </summary>
        /// <param name="changedFile">Path to changed file</param>
        /// <param name="category">Category name</param>
        /// <returns>True if same category, false otherwise</returns>
        private static bool IsSameCategoryChange(string changedFile, string category)
        {
            if (string.IsNullOrEmpty(category))
                return false;

            var normalizedFile = changedFile.Replace('\\', '/');
            return normalizedFile.StartsWith(
                $"Documentation/{category}/",
                StringComparison.OrdinalIgnoreCase
            );
        }

        /// <summary>
        /// Checks if file change is related to category
        /// </summary>
        /// <param name="changedFile">Path to changed file</param>
        /// <param name="category">Category name</param>
        /// <returns>True if related, false otherwise</returns>
        private static bool IsRelatedToCategory(string changedFile, string category)
        {
            if (string.IsNullOrEmpty(category))
                return false;

            // Check for indirect relationships
            var categoryLower = category.ToLower();
            switch (categoryLower)
            {
                case "modules":
                    return changedFile.Contains("ModuleUpdateSummary.md");
                case "development":
                    return changedFile.Contains("Development/")
                        || IsModuleName(ExtractModuleFromFilePath(changedFile));
                case "ai-assistant":
                    return changedFile.Contains("DevTools/")
                        || changedFile.Contains("AI-Assistant/");
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if change affects subcategory
        /// </summary>
        /// <param name="changedFile">Path to changed file</param>
        /// <param name="category">Category name</param>
        /// <returns>True if subcategory change, false otherwise</returns>
        private static bool IsSubcategoryChange(string changedFile, string category)
        {
            if (string.IsNullOrEmpty(category))
                return false;

            var normalizedFile = changedFile.Replace('\\', '/');
            var categoryPattern = $"Documentation/{category}/";

            if (normalizedFile.StartsWith(categoryPattern, StringComparison.OrdinalIgnoreCase))
            {
                var remainingPath = normalizedFile.Substring(categoryPattern.Length);
                return remainingPath.Contains('/'); // Has subdirectory
            }

            return false;
        }

        /// <summary>
        /// Caches rule evaluation result
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="result">Result to cache</param>
        private static void CacheResult(string cacheKey, RuleEvaluationResult result)
        {
            // Clean cache if it's getting old
            if (DateTime.Now - _cacheLastCleaned > TimeSpan.FromMinutes(10))
            {
                RuleCache.Clear();
                _cacheLastCleaned = DateTime.Now;
            }

            RuleCache[cacheKey] = result;
        }

        #endregion
    }
}
