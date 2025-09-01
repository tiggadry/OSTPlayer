// ====================================================================
// FILE: AIAssistantHooks.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: DevTools
// LOCATION: DevTools/
// VERSION: 1.3.1
// CREATED: 2025-08-07
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// AI Assistant integration hooks and automation utilities for GitHub Copilot
// and other AI development tools. This component provides intelligent context
// management, automated workflow triggers, and seamless integration points
// for AI-assisted development workflows.
//
// FEATURES:
// - Intelligent context management for AI assistants
// - Automated workflow triggers based on code changes
// - Hook system for file change notifications
// - AI action logging and analytics
// - Context-aware suggestions and automation
// - Workflow state management and persistence
//
// DEPENDENCIES:
// - System.IO for file system operations
// - System.Collections.Generic for collections
// - System.Threading.Tasks for async operations
// - OstPlayer.DevTools.DateHelper for date management
// - OstPlayer.DevTools.ProjectAnalyzer for change analysis
// - OstPlayer.DevTools.DocumentationManager for doc updates
//
// DESIGN PATTERNS:
// - Observer pattern for file change notifications
// - Strategy pattern for different AI assistant types
// - Command pattern for automated actions
// - Singleton pattern for context management
// - Factory pattern for hook creation
//
// AI INTEGRATION:
// - GitHub Copilot context management
// - Automated documentation triggers
// - Intelligent suggestion system
// - Workflow automation hooks
// - Context-aware code generation
//
// PERFORMANCE NOTES:
// - Efficient event handling with minimal overhead
// - Lazy loading of context information
// - Optimized hook execution for real-time performance
// - Memory-efficient action logging
//
// LIMITATIONS:
// - Requires proper project structure for context analysis
// - Limited to predefined hook types and actions
// - No external AI service integration (local only)
//
// FUTURE REFACTORING:
// FUTURE: Add support for external AI service integrations
// FUTURE: Implement machine learning for context prediction
// FUTURE: Add support for custom hook plugins
// FUTURE: Implement advanced workflow orchestration
// CONSIDER: Integration with cloud-based AI services
// CONSIDER: Real-time collaboration features
//
// TESTING:
// - Unit tests for hook registration and execution
// - Integration tests with actual file changes
// - Performance tests for high-frequency events
// - Validation tests for context accuracy
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - GitHub Copilot
// - Visual Studio 2019+
//
// CHANGELOG:
// 2025-08-07 v1.3.1 - Fixed async/await warnings for clean compilation
// 2025-08-07 v1.3.0 - Initial implementation for AI assistant enhancement
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// AI Assistant integration hooks and automation utilities
    /// </summary>
    public static class AIAssistantHooks
    {
        // Hook registry for different event types
        private static readonly Dictionary<string, List<Func<string, Task>>> FileChangeHooks =
            new Dictionary<string, List<Func<string, Task>>>();
        private static readonly Dictionary<string, List<Func<AIContext, Task>>> ContextHooks =
            new Dictionary<string, List<Func<AIContext, Task>>>();

        // AI action log for analytics
        private static readonly List<AIAction> ActionLog = new List<AIAction>();

        // Current AI context
        private static AIContext _currentContext;

        // Hook execution settings
        private static bool _hooksEnabled = true;
        private static int _maxLogEntries = 1000;

        #region Hook Registration

        /// <summary>
        /// Registers a file change hook for specific file patterns
        /// </summary>
        /// <param name="pattern">File pattern to watch (e.g., "*.cs", "ViewModels/*")</param>
        /// <param name="handler">Handler function to execute on file changes</param>
        public static void RegisterFileChangeHook(string pattern, Func<string, Task> handler)
        {
            if (!FileChangeHooks.ContainsKey(pattern))
            {
                FileChangeHooks[pattern] = new List<Func<string, Task>>();
            }

            FileChangeHooks[pattern].Add(handler);

            LogAIAction("RegisterFileChangeHook", true, $"Registered hook for pattern: {pattern}");
        }

        /// <summary>
        /// Registers a context change hook for AI assistant events
        /// </summary>
        /// <param name="eventType">Type of context event (e.g., "DocumentationUpdate", "CodeGeneration")</param>
        /// <param name="handler">Handler function to execute on context changes</param>
        public static void RegisterContextHook(string eventType, Func<AIContext, Task> handler)
        {
            if (!ContextHooks.ContainsKey(eventType))
            {
                ContextHooks[eventType] = new List<Func<AIContext, Task>>();
            }

            ContextHooks[eventType].Add(handler);

            LogAIAction(
                "RegisterContextHook",
                true,
                $"Registered hook for event type: {eventType}"
            );
        }

        /// <summary>
        /// Unregisters all hooks for a specific pattern
        /// </summary>
        /// <param name="pattern">Pattern to unregister</param>
        public static void UnregisterFileChangeHooks(string pattern)
        {
            if (FileChangeHooks.ContainsKey(pattern))
            {
                FileChangeHooks.Remove(pattern);
                LogAIAction(
                    "UnregisterFileChangeHooks",
                    true,
                    $"Unregistered hooks for pattern: {pattern}"
                );
            }
        }

        #endregion

        #region Workflow Triggers

        /// <summary>
        /// Triggers documentation update workflow
        /// </summary>
        /// <param name="reason">Reason for the documentation update</param>
        /// <param name="affectedFiles">Files that triggered the update</param>
        public static async Task TriggerDocumentationUpdate(string reason, string[] affectedFiles)
        {
            if (!_hooksEnabled)
            {
                return;
            }

            try
            {
                LogAIAction(
                    "TriggerDocumentationUpdate",
                    true,
                    $"Triggered by: {reason}, Files: {string.Join(", ", affectedFiles)}"
                );

                // Update context with current operation
                UpdateContext(
                    "DocumentationUpdate",
                    new Dictionary<string, object>
                    {
                        ["Reason"] = reason,
                        ["AffectedFiles"] = affectedFiles,
                        ["Timestamp"] = DateTime.Now,
                    }
                );

                // Execute registered context hooks
                await ExecuteContextHooks("DocumentationUpdate");

                // Perform automatic documentation updates
                await PerformAutomaticDocumentationUpdates(affectedFiles, reason);
            }
            catch (Exception ex)
            {
                LogAIAction("TriggerDocumentationUpdate", false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Triggers code generation workflow
        /// </summary>
        /// <param name="generationType">Type of code generation (e.g., "Class", "Method", "Property")</param>
        /// <param name="context">Generation context</param>
        public static async Task TriggerCodeGeneration(
            string generationType,
            Dictionary<string, object> context
        )
        {
            if (!_hooksEnabled)
            {
                return;
            }

            try
            {
                LogAIAction("TriggerCodeGeneration", true, $"Type: {generationType}");

                // Update AI context
                UpdateContext("CodeGeneration", context);

                // Execute code generation hooks
                await ExecuteContextHooks("CodeGeneration");

                // Provide intelligent suggestions
                await ProvideSuggestions(generationType, context);
            }
            catch (Exception ex)
            {
                LogAIAction("TriggerCodeGeneration", false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Triggers file analysis workflow
        /// </summary>
        /// <param name="filePath">Path to the file being analyzed</param>
        /// <param name="analysisType">Type of analysis being performed</param>
        public static async Task TriggerFileAnalysis(
            string filePath,
            string analysisType = "General"
        )
        {
            if (!_hooksEnabled)
            {
                return;
            }

            try
            {
                LogAIAction("TriggerFileAnalysis", true, $"File: {filePath}, Type: {analysisType}");

                // Execute file change hooks
                await ExecuteFileChangeHooks(filePath);

                // Update project context
                await UpdateProjectContext(filePath);
            }
            catch (Exception ex)
            {
                LogAIAction("TriggerFileAnalysis", false, $"Error: {ex.Message}");
            }
        }

        #endregion

        #region Context Management

        /// <summary>
        /// Gets the current AI context
        /// </summary>
        /// <returns>Current AI context</returns>
        public static AIContext GetCurrentContext()
        {
            if (_currentContext == null)
            {
                _currentContext = CreateDefaultContext();
            }

            return _currentContext;
        }

        /// <summary>
        /// Updates the AI context with new information
        /// </summary>
        /// <param name="operation">Current operation being performed</param>
        /// <param name="data">Additional context data</param>
        public static void UpdateContext(string operation, Dictionary<string, object> data)
        {
            if (_currentContext == null)
            {
                _currentContext = CreateDefaultContext();
            }

            _currentContext.CurrentOperation = operation;
            _currentContext.LastUpdated = DateTime.Now;

            // Merge new data with existing context
            foreach (var item in data)
            {
                _currentContext.Data[item.Key] = item.Value;
            }

            // Add operation to history
            _currentContext.OperationHistory.Add(
                new ContextOperation
                {
                    Operation = operation,
                    Timestamp = DateTime.Now,
                    Data = new Dictionary<string, object>(data),
                }
            );

            // Keep history manageable
            if (_currentContext.OperationHistory.Count > 50)
            {
                _currentContext.OperationHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears the current AI context
        /// </summary>
        public static void ClearContext()
        {
            _currentContext = CreateDefaultContext();
            LogAIAction("ClearContext", true, "AI context cleared");
        }

        #endregion

        #region Action Logging

        /// <summary>
        /// Logs an AI action for analytics and debugging
        /// </summary>
        /// <param name="action">Action that was performed</param>
        /// <param name="success">Whether the action was successful</param>
        /// <param name="details">Additional details about the action</param>
        public static void LogAIAction(string action, bool success, string details = null)
        {
            var aiAction = new AIAction
            {
                Action = action,
                Success = success,
                Details = details,
                Timestamp = DateTime.Now,
                Context = _currentContext?.CurrentOperation,
            };

            ActionLog.Add(aiAction);

            // Keep log manageable
            if (ActionLog.Count > _maxLogEntries)
            {
                ActionLog.RemoveAt(0);
            }
        }

        /// <summary>
        /// Gets the AI action log for analysis
        /// </summary>
        /// <param name="count">Number of recent actions to return</param>
        /// <returns>List of recent AI actions</returns>
        public static List<AIAction> GetActionLog(int count = 100)
        {
            if (ActionLog.Count <= count)
            {
                return new List<AIAction>(ActionLog);
            }

            return ActionLog.Skip(ActionLog.Count - count).ToList();
        }

        /// <summary>
        /// Gets AI action statistics
        /// </summary>
        /// <returns>Dictionary containing action statistics</returns>
        public static Dictionary<string, object> GetActionStatistics()
        {
            var stats = new Dictionary<string, object>();

            if (ActionLog.Any())
            {
                stats["TotalActions"] = ActionLog.Count;
                stats["SuccessfulActions"] = ActionLog.Count(a => a.Success);
                stats["FailedActions"] = ActionLog.Count(a => !a.Success);
                stats["SuccessRate"] =
                    (double)ActionLog.Count(a => a.Success) / ActionLog.Count * 100;
                stats["MostCommonAction"] = ActionLog
                    .GroupBy(a => a.Action)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()
                    ?.Key;
                stats["LastAction"] = ActionLog.LastOrDefault()?.Action;
                stats["LastActionTime"] = ActionLog.LastOrDefault()?.Timestamp;
            }
            else
            {
                stats["TotalActions"] = 0;
                stats["SuccessfulActions"] = 0;
                stats["FailedActions"] = 0;
                stats["SuccessRate"] = 0.0;
            }

            return stats;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Enables or disables AI assistant hooks
        /// </summary>
        /// <param name="enabled">Whether hooks should be enabled</param>
        public static void SetHooksEnabled(bool enabled)
        {
            _hooksEnabled = enabled;
            LogAIAction("SetHooksEnabled", true, $"Hooks enabled: {enabled}");
        }

        /// <summary>
        /// Sets the maximum number of log entries to keep
        /// </summary>
        /// <param name="maxEntries">Maximum number of log entries</param>
        public static void SetMaxLogEntries(int maxEntries)
        {
            _maxLogEntries = Math.Max(100, maxEntries);

            // Trim current log if necessary
            while (ActionLog.Count > _maxLogEntries)
            {
                ActionLog.RemoveAt(0);
            }

            LogAIAction("SetMaxLogEntries", true, $"Max entries set to: {maxEntries}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a default AI context
        /// </summary>
        /// <returns>Default AI context</returns>
        private static AIContext CreateDefaultContext()
        {
            return new AIContext
            {
                ProjectName = "OstPlayer",
                Framework = ".NET Framework 4.6.2",
                Language = "C# 7.3",
                LastUpdated = DateTime.Now,
                CurrentOperation = "None",
                Data = new Dictionary<string, object>(),
                OperationHistory = new List<ContextOperation>(),
            };
        }

        /// <summary>
        /// Executes file change hooks for a specific file
        /// </summary>
        /// <param name="filePath">Path to the changed file</param>
        private static async Task ExecuteFileChangeHooks(string filePath)
        {
            foreach (var pattern in FileChangeHooks.Keys)
            {
                if (MatchesPattern(filePath, pattern))
                {
                    var hooks = FileChangeHooks[pattern];
                    foreach (var hook in hooks)
                    {
                        try
                        {
                            await hook(filePath);
                        }
                        catch (Exception ex)
                        {
                            LogAIAction(
                                "ExecuteFileChangeHook",
                                false,
                                $"Hook error for {filePath}: {ex.Message}"
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes context hooks for a specific event type
        /// </summary>
        /// <param name="eventType">Type of context event</param>
        private static async Task ExecuteContextHooks(string eventType)
        {
            if (ContextHooks.ContainsKey(eventType))
            {
                var hooks = ContextHooks[eventType];
                foreach (var hook in hooks)
                {
                    try
                    {
                        await hook(_currentContext);
                    }
                    catch (Exception ex)
                    {
                        LogAIAction(
                            "ExecuteContextHook",
                            false,
                            $"Context hook error for {eventType}: {ex.Message}"
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Performs automatic documentation updates
        /// </summary>
        /// <param name="affectedFiles">Files that were changed</param>
        /// <param name="reason">Reason for the update</param>
        private static Task PerformAutomaticDocumentationUpdates(
            string[] affectedFiles,
            string reason
        )
        {
            try
            {
                // Get affected documentation files
                var docsToUpdate = ProjectAnalyzer.GetAffectedDocumentationFiles(affectedFiles);

                foreach (var docFile in docsToUpdate)
                {
                    // Update file headers if needed
                    if (docFile.EndsWith(".cs"))
                    {
                        DateHelper.UpdateFileHeader(docFile);
                    }
                }

                // Update module summaries
                var modules = affectedFiles
                    .Select(f => GetModuleFromPath(f))
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Distinct();

                foreach (var module in modules)
                {
                    DocumentationManager.UpdateModuleSummary(module, reason);
                }

                LogAIAction(
                    "PerformAutomaticDocumentationUpdates",
                    true,
                    $"Updated docs for {docsToUpdate.Count} files"
                );
            }
            catch (Exception ex)
            {
                LogAIAction("PerformAutomaticDocumentationUpdates", false, $"Error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Provides intelligent suggestions based on context
        /// </summary>
        /// <param name="generationType">Type of generation</param>
        /// <param name="context">Generation context</param>
        private static Task ProvideSuggestions(
            string generationType,
            Dictionary<string, object> context
        )
        {
            // This would integrate with AI services to provide suggestions
            // For now, we'll just log the suggestion opportunity
            LogAIAction("ProvideSuggestions", true, $"Suggestions available for {generationType}");

            // Update context with suggestion metadata
            _currentContext.Data["LastSuggestionType"] = generationType;
            _currentContext.Data["SuggestionContext"] = context;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates project context based on file analysis
        /// </summary>
        /// <param name="filePath">Path to the analyzed file</param>
        private static Task UpdateProjectContext(string filePath)
        {
            try
            {
                var module = GetModuleFromPath(filePath);
                var fileType = Path.GetExtension(filePath);

                _currentContext.Data["LastAnalyzedFile"] = filePath;
                _currentContext.Data["LastAnalyzedModule"] = module;
                _currentContext.Data["LastAnalyzedFileType"] = fileType;

                // Update project statistics
                if (!_currentContext.Data.ContainsKey("AnalyzedFilesCount"))
                {
                    _currentContext.Data["AnalyzedFilesCount"] = 0;
                }

                _currentContext.Data["AnalyzedFilesCount"] =
                    (int)_currentContext.Data["AnalyzedFilesCount"] + 1;

                LogAIAction("UpdateProjectContext", true, $"Updated context for {filePath}");
            }
            catch (Exception ex)
            {
                LogAIAction("UpdateProjectContext", false, $"Error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the module name from a file path
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Module name or null</returns>
        private static string GetModuleFromPath(string filePath)
        {
            var pathParts = filePath.Replace('\\', '/').Split('/');
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

            return pathParts.FirstOrDefault(part =>
                modules.Contains(part, StringComparer.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Checks if a file path matches a pattern
        /// </summary>
        /// <param name="filePath">File path to check</param>
        /// <param name="pattern">Pattern to match against</param>
        /// <returns>True if matches, false otherwise</returns>
        private static bool MatchesPattern(string filePath, string pattern)
        {
            // Simple pattern matching - could be enhanced with regex
            if (pattern == "*")
            {
                return true;
            }

            if (pattern.StartsWith("*."))
            {
                var extension = pattern.Substring(1);
                return filePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
            }

            if (pattern.EndsWith("/*"))
            {
                var directory = pattern.Substring(0, pattern.Length - 2);
                return filePath.Contains(directory);
            }

            return filePath.Contains(pattern);
        }

        #endregion
    }

    /// <summary>
    /// Represents the current AI context
    /// </summary>
    public class AIContext
    {
        /// <summary>
        /// Project name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Target framework
        /// </summary>
        public string Framework { get; set; }

        /// <summary>
        /// Programming language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Current operation being performed
        /// </summary>
        public string CurrentOperation { get; set; }

        /// <summary>
        /// Context data dictionary
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// Operation history
        /// </summary>
        public List<ContextOperation> OperationHistory { get; set; }
    }

    /// <summary>
    /// Represents a context operation
    /// </summary>
    public class ContextOperation
    {
        /// <summary>
        /// Operation name
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Operation timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Operation data
        /// </summary>
        public Dictionary<string, object> Data { get; set; }
    }

    /// <summary>
    /// Represents an AI action for logging
    /// </summary>
    public class AIAction
    {
        /// <summary>
        /// Action that was performed
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Whether the action was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Additional details about the action
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Action timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Context when action was performed
        /// </summary>
        public string Context { get; set; }
    }
}
// Test modification - 08/08/2025 00:17:36
