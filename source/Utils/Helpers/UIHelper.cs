// ====================================================================
// FILE: UIHelper.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils.Helpers
// LOCATION: Utils/Helpers/
// VERSION: 1.0.0
// CREATED: 2025-08-13
// UPDATED: 2025-08-13
// AUTHOR: AI Assistant (Safe Refactoring Step 6)
// ====================================================================
//
// PURPOSE:
// Helper utility class for UI-related operations in OstPlayer.
// Provides static methods for UI formatting, button symbols, and display text.
// This is part of STEP 6 of ultra-safe refactoring - UI helper utilities.
//
// FEATURES:
// - Media control button symbols (play, pause, stop)
// - Button state-based symbol selection
// - Tooltip text generation
// - Toggle text formatting
// - UI state display helpers
//
// SAFETY NOTES:
// - Static utility methods only (no state)
// - Pure functions with predictable outputs
// - No external dependencies
// - Thread-safe operations
//
// DESIGN PATTERNS:
// - Static Utility Pattern (no instance required)
// - Pure Functions (no side effects)
// - Constants Pattern (centralized UI constants)
//
// DEPENDENCIES:
// - System (basic .NET functionality)
// - No external dependencies
//
// USAGE EXAMPLES:
// string buttonSymbol = UIHelper.GetPlayPauseButtonSymbol(isPlaying, isPaused);
// string tooltip = UIHelper.GetPlayPauseTooltip(isPlaying, isPaused);
// string toggleText = UIHelper.GetToggleText("metadata", isVisible);
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - No WPF dependencies (pure utility)
// - Thread-safe (static methods only)
//
// CHANGELOG:
// 2025-08-13 v1.0.0 - Initial UI helper utility creation (Safe Refactoring Step 6)
// ====================================================================

using System;

namespace OstPlayer.Utils.Helpers
{
    /// <summary>
    /// Static helper class for UI-related operations and formatting.
    /// Provides utility methods for button symbols, tooltips, and display text.
    /// 
    /// SAFETY NOTE: This is part of micro-extraction Step 6 - minimal risk UI helper.
    /// Contains only pure functions with no side effects.
    /// </summary>
    public static class UIHelper
    {
        #region UI Symbol Constants

        /// <summary>
        /// Unicode symbol for play button (▶).
        /// </summary>
        public const string PlaySymbol = "\u25B6";

        /// <summary>
        /// Unicode symbol for pause button (⏸).
        /// </summary>
        public const string PauseSymbol = "\u23F8";

        /// <summary>
        /// Unicode symbol for stop button (⏹).
        /// </summary>
        public const string StopSymbol = "\u23F9";

        /// <summary>
        /// Unicode symbol for previous track (⏮).
        /// </summary>
        public const string PreviousSymbol = "\u23EE";

        /// <summary>
        /// Unicode symbol for next track (⏭).
        /// </summary>
        public const string NextSymbol = "\u23ED";

        /// <summary>
        /// Unicode symbol for shuffle (🔀).
        /// </summary>
        public const string ShuffleSymbol = "\uD83D\uDD00";

        /// <summary>
        /// Unicode symbol for repeat (🔁).
        /// </summary>
        public const string RepeatSymbol = "\uD83D\uDD01";

        #endregion

        #region Button Symbol Methods

        /// <summary>
        /// Gets appropriate play/pause button symbol based on current playback state.
        /// Returns pause symbol when playing, play symbol when stopped or paused.
        /// </summary>
        /// <param name="isPlaying">Whether audio is currently playing</param>
        /// <param name="isPaused">Whether audio is currently paused</param>
        /// <returns>Unicode symbol for button display (▶ or ⏸)</returns>
        /// <example>
        /// <code>
        /// string symbol = UIHelper.GetPlayPauseButtonSymbol(true, false);
        /// // Returns: "⏸" (pause symbol when playing)
        /// 
        /// string symbol2 = UIHelper.GetPlayPauseButtonSymbol(false, true);
        /// // Returns: "▶" (play symbol when paused)
        /// </code>
        /// </example>
        public static string GetPlayPauseButtonSymbol(bool isPlaying, bool isPaused)
        {
            return (isPlaying && !isPaused) ? PauseSymbol : PlaySymbol;
        }

        /// <summary>
        /// Gets appropriate tooltip text for play/pause button based on current state.
        /// Provides user-friendly description of what the button will do when clicked.
        /// </summary>
        /// <param name="isPlaying">Whether audio is currently playing</param>
        /// <param name="isPaused">Whether audio is currently paused</param>
        /// <returns>Descriptive tooltip text</returns>
        public static string GetPlayPauseTooltip(bool isPlaying, bool isPaused)
        {
            return (isPlaying && !isPaused) ? "Pause playback" : "Play selected track";
        }

        #endregion

        #region Toggle Text Methods

        /// <summary>
        /// Generates toggle button text based on current visibility state.
        /// Creates "Hide [item]" or "Show [item]" text patterns.
        /// </summary>
        /// <param name="itemName">Name of the item being toggled (e.g., "metadata", "playlist")</param>
        /// <param name="isVisible">Current visibility state</param>
        /// <returns>Formatted toggle text</returns>
        /// <example>
        /// <code>
        /// string text = UIHelper.GetToggleText("MP3 metadata", true);
        /// // Returns: "Hide MP3 metadata"
        /// 
        /// string text2 = UIHelper.GetToggleText("MP3 metadata", false);
        /// // Returns: "Show MP3 metadata"
        /// </code>
        /// </example>
        public static string GetToggleText(string itemName, bool isVisible)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return isVisible ? "Hide" : "Show";

            return isVisible ? $"Hide {itemName}" : $"Show {itemName}";
        }

        /// <summary>
        /// Generates toggle button text with custom action verbs.
        /// Allows for more flexible toggle text patterns beyond show/hide.
        /// </summary>
        /// <param name="itemName">Name of the item being toggled</param>
        /// <param name="isActive">Current active state</param>
        /// <param name="activeVerb">Verb to use when item is active (e.g., "Disable", "Stop")</param>
        /// <param name="inactiveVerb">Verb to use when item is inactive (e.g., "Enable", "Start")</param>
        /// <returns>Formatted toggle text with custom verbs</returns>
        public static string GetToggleTextWithVerbs(string itemName, bool isActive, string activeVerb, string inactiveVerb)
        {
            var verb = isActive ? activeVerb : inactiveVerb;
            
            if (string.IsNullOrWhiteSpace(itemName))
                return verb ?? (isActive ? "Disable" : "Enable");

            return $"{verb} {itemName}";
        }

        #endregion

        #region Status Display Methods

        /// <summary>
        /// Formats playback status text with track name and current state.
        /// Creates consistent status messages for current track display.
        /// </summary>
        /// <param name="trackName">Name of the current track</param>
        /// <param name="isPlaying">Whether audio is currently playing</param>
        /// <param name="isPaused">Whether audio is currently paused</param>
        /// <returns>Formatted status text</returns>
        public static string FormatPlaybackStatus(string trackName, bool isPlaying, bool isPaused)
        {
            if (string.IsNullOrWhiteSpace(trackName))
                return string.Empty;

            if (isPlaying && !isPaused)
                return $"Playing: {trackName}";
            
            if (isPaused)
                return $"Paused: {trackName}";
            
            return string.Empty; // Stopped state shows no status
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates that a UI symbol is a valid Unicode character.
        /// Helps ensure UI symbols will display correctly.
        /// </summary>
        /// <param name="symbol">Unicode symbol to validate</param>
        /// <returns>True if symbol is valid, false otherwise</returns>
        public static bool IsValidUISymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return false;

            try
            {
                // Check if string contains valid Unicode characters
                foreach (char c in symbol)
                {
                    if (char.IsControl(c) && c != '\u200B') // Allow zero-width space
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a safe fallback symbol if the provided symbol is invalid.
        /// Ensures UI always has a displayable symbol.
        /// </summary>
        /// <param name="symbol">Primary symbol to use</param>
        /// <param name="fallback">Fallback symbol if primary is invalid</param>
        /// <returns>Valid symbol for display</returns>
        public static string GetSafeSymbol(string symbol, string fallback = "?")
        {
            return IsValidUISymbol(symbol) ? symbol : (fallback ?? "?");
        }

        #endregion

        #region Command Availability Methods

        /// <summary>
        /// Determines if play/pause command can be executed based on file selection.
        /// Essential for proper MVVM command binding and UI state management.
        /// </summary>
        /// <param name="selectedFile">Currently selected music file path</param>
        /// <returns>True if command can be executed, false otherwise</returns>
        /// <example>
        /// <code>
        /// bool canPlay = UIHelper.CanPlayPause(viewModel.SelectedMusicFile);
        /// // Returns: true if file is selected, false if null/empty
        /// </code>
        /// </example>
        public static bool CanPlayPause(string selectedFile)
        {
            return !string.IsNullOrEmpty(selectedFile);
        }

        /// <summary>
        /// Determines if stop command can be executed based on playback state.
        /// Provides consistent command availability logic for UI binding.
        /// </summary>
        /// <param name="isPlaying">Whether audio is currently playing</param>
        /// <returns>True if command can be executed, false otherwise</returns>
        /// <example>
        /// <code>
        /// bool canStop = UIHelper.CanStop(viewModel.IsPlaying);
        /// // Returns: true if playing, false if stopped/paused
        /// </code>
        /// </example>
        public static bool CanStop(bool isPlaying)
        {
            return isPlaying;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Capitalizes the first letter of a string for UI display.
        /// Useful for consistent text formatting in UI elements.
        /// </summary>
        /// <param name="text">Text to capitalize</param>
        /// <returns>Text with first letter capitalized</returns>
        public static string CapitalizeFirst(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (text.Length == 1)
                return text.ToUpper();

            return char.ToUpper(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// Truncates text to specified length with ellipsis for UI display.
        /// Prevents UI text overflow in constrained spaces.
        /// </summary>
        /// <param name="text">Text to truncate</param>
        /// <param name="maxLength">Maximum length including ellipsis</param>
        /// <returns>Truncated text with ellipsis if needed</returns>
        public static string TruncateWithEllipsis(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || maxLength <= 0)
                return text;

            if (text.Length <= maxLength)
                return text;

            if (maxLength <= 3)
                return text.Substring(0, maxLength);

            return text.Substring(0, maxLength - 3) + "...";
        }

        #endregion
    }
}
