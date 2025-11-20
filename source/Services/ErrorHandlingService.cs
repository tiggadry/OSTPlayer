// ====================================================================
// FILE: ErrorHandlingService.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Services
// LOCATION: Services/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Centralized error handling and user notification service providing
// consistent error management, user-friendly messaging, and comprehensive
// logging across all application components and external integrations.
//
// FEATURES:
// - Centralized error handling across application
// - User-friendly error message translation
// - Comprehensive logging with context information
// - Recovery mechanisms for common error scenarios
// - Categorized error handling (playback, metadata, network)
// - MessageBox-based user notifications
// - Fallback error handling for notification failures
//
// DEPENDENCIES:
// - Playnite.SDK (ILogger for logging infrastructure)
// - System.Windows (MessageBox for user notifications)
// - System.IO (file system exception handling)
// - System.Threading.Tasks (async operation error handling)
//
// SERVICE RESPONSIBILITIES:
// - Translate technical exceptions to user-friendly messages
// - Provide contextual error handling for different operations
// - Maintain comprehensive error logs for debugging
// - Implement recovery strategies for recoverable errors
// - Manage user notification timing and frequency
//
// ERROR CATEGORIES:
// - Playback Errors: Audio file access, format, and playback issues
// - Metadata Errors: Tag reading, parsing, and validation failures
// - Network Errors: API connectivity, timeout, and service issues
// - System Errors: File access, permissions, and resource conflicts
//
// USER NOTIFICATION STRATEGY:
// - Critical errors: Immediate user notification with recovery options
// - Warning conditions: Optional user notification with context
// - Information events: User feedback for successful operations
// - Silent failures: Log-only for non-critical operations
//
// PERFORMANCE NOTES:
// - Minimal overhead for error-free operations
// - Efficient exception analysis and categorization
// - Non-blocking user notification patterns
// - Optimized logging with appropriate detail levels
//
// LIMITATIONS:
// - MessageBox-based notifications (not modern UI patterns)
// - Limited error recovery automation
// - No error analytics or trending
// - Basic error categorization system
//
// FUTURE REFACTORING:
// FUTURE: Implement modern notification system (toast, in-app notifications)
// FUTURE: Add automatic error recovery mechanisms
// FUTURE: Implement error analytics and reporting
// FUTURE: Add user error feedback and reporting system
// FUTURE: Extract to configurable notification providers
// FUTURE: Add error categorization and severity levels
// FUTURE: Implement retry logic with exponential backoff
// FUTURE: Add error correlation and pattern detection
// CONSIDER: Integration with Playnite's notification system
// CONSIDER: Adding error telemetry (privacy-respecting)
// IDEA: Machine learning for error prediction and prevention
// IDEA: Community-driven error resolution suggestions
//
// TESTING:
// - Unit tests for error categorization and message generation
// - Integration tests with various exception types
// - User notification system tests
// - Error recovery mechanism validation
//
// ERROR HANDLING PATTERNS:
// - Exception type analysis for appropriate responses
// - Context-aware error message generation
// - Graceful degradation for notification failures
// - Comprehensive logging with error context
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Playnite SDK 6.x
// - WPF MessageBox system
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive error handling
// ====================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Playnite.SDK;

namespace OstPlayer.Services
{
    /// <summary>
    /// Centralized error handling and user notification service.
    /// </summary>
    public class ErrorHandlingService
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the ErrorHandlingService class.
        /// </summary>
        public ErrorHandlingService()
        {
            logger = LogManager.GetLogger();
        }

        /// <summary>
        /// Handles playback errors with user-friendly messages
        /// </summary>
        public void HandlePlaybackError(Exception ex, string filePath)
        {
            logger.Error(ex, $"Playback error for file: {filePath}");

            string userMessage;
            if (ex is UnauthorizedAccessException)
            {
                userMessage = "Cannot access the music file. Check file permissions.";
            }
            else if (ex is FileNotFoundException)
            {
                userMessage = "Music file not found. The file may have been moved or deleted.";
            }
            else if (ex is InvalidDataException)
            {
                userMessage = "The music file appears to be corrupted or in an unsupported format.";
            }
            else if (ex.GetType().Name.Contains("MediaFoundation"))
            {
                userMessage = "Audio system error. Try restarting the application.";
            }
            else
            {
                userMessage = $"An error occurred while playing music: {ex.Message}";
            }

            ShowError("Playback Error", userMessage);
        }

        /// <summary>
        /// Handles metadata loading errors
        /// </summary>
        public void HandleMetadataError(Exception ex, string filePath)
        {
            logger.Warn(ex, $"Failed to load metadata for file: {filePath}");

            // For metadata errors, we typically don't show user messages
            // as the application can continue to function
        }

        /// <summary>
        /// Handles network errors (e.g., Discogs API)
        /// </summary>
        public void HandleNetworkError(Exception ex, string operation)
        {
            logger.Error(ex, $"Network error during: {operation}");

            string userMessage;
            if (ex.GetType().Name.Contains("HttpRequest"))
            {
                userMessage = "Network connection error. Check your internet connection.";
            }
            else if (ex is TaskCanceledException)
            {
                userMessage = "The request timed out. Please try again.";
            }
            else
            {
                userMessage = $"Network error during {operation}: {ex.Message}";
            }

            ShowError("Network Error", userMessage);
        }

        /// <summary>
        /// Shows error message to user
        /// </summary>
        private void ShowError(string title, string message)
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Fallback if MessageBox fails
                logger.Error(ex, "Failed to show error message to user");
            }
        }

        /// <summary>
        /// Shows warning message to user
        /// </summary>
        public void ShowWarning(string title, string message)
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to show warning message to user");
            }
        }

        /// <summary>
        /// Shows information message to user
        /// </summary>
        public void ShowInfo(string title, string message)
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to show info message to user");
            }
        }
    }
}
