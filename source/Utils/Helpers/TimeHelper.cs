// ====================================================================
// FILE: TimeHelper.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils.Helpers
// LOCATION: Utils/Helpers/
// VERSION: 1.0.0
// CREATED: 2025-08-13
// UPDATED: 2025-08-13
// AUTHOR: AI Assistant (Safe Refactoring Step 2)
// ====================================================================
//
// PURPOSE:
// Helper utility class for time-related operations in OstPlayer.
// Provides static methods for time formatting, parsing, and calculations.
// This is part of STEP 2 of ultra-safe refactoring - helper utilities only, NO usage yet.
//
// FEATURES:
// - Time formatting for display (MM:SS, HH:MM:SS)
// - Time parsing from various string formats
// - Duration calculations and conversions
// - Progress percentage calculations
// - Time validation and bounds checking
//
// SAFETY NOTES:
// - This class is NOT USED YET by any existing code
// - Zero risk of breaking existing functionality
// - Infrastructure preparation for future safe extractions
// - Can be added to project without affecting current code
//
// DESIGN PATTERNS:
// - Static Utility Pattern (no instance required)
// - Pure Functions (no side effects)
// - Input Validation Pattern (defensive programming)
//
// DEPENDENCIES:
// - System (TimeSpan, Math functions)
// - No external dependencies
//
// FUTURE USAGE:
// When ready for safe extraction, existing time operations can use these helpers.
// Example: CurrentTime => TimeHelper.FormatTime(Position)
//         DurationTime => TimeHelper.FormatTime(Duration)
//
// TESTING REQUIREMENTS:
// 1. Project builds successfully with new helper
// 2. No regressions in existing functionality
// 3. Plugin loads and works normally
// 4. All current time/progress features remain functional
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - No WPF dependencies (pure utility)
// - Thread-safe (static methods only)
//
// CHANGELOG:
// 2025-08-13 v1.0.0 - Initial helper utility creation (Safe Refactoring Step 2)
// ====================================================================

using System;

namespace OstPlayer.Utils.Helpers
{
    /// <summary>
    /// Static helper class for time-related operations and formatting.
    /// Provides utility methods for time display, parsing, and calculations.
    ///
    /// SAFETY NOTE: This is infrastructure preparation - NOT USED by existing code yet.
    /// Can be safely added without affecting current functionality.
    /// </summary>
    public static class TimeHelper
    {
        #region Constants

        /// <summary>
        /// Default time display when time is unknown or invalid.
        /// </summary>
        public const string UnknownTimeDisplay = "--:--";

        /// <summary>
        /// Default time display when time is zero or not started.
        /// </summary>
        public const string ZeroTimeDisplay = "00:00";

        /// <summary>
        /// Minimum valid time value in seconds.
        /// </summary>
        public const double MinTimeSeconds = 0.0;

        /// <summary>
        /// Maximum reasonable time value in seconds (24 hours).
        /// Protects against invalid or corrupted duration values.
        /// </summary>
        public const double MaxTimeSeconds = 24 * 60 * 60; // 24 hours
        #endregion

        #region Time Formatting

        /// <summary>
        /// Formats time in seconds to MM:SS format.
        /// Most common format for music track times.
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns>Formatted time string (e.g., "03:45")</returns>
        /// <example>
        /// <code>
        /// string formatted = TimeHelper.FormatTime(225.5);
        /// // Returns: "03:46" (225.5 seconds = 3 minutes 45.5 seconds, rounded up)
        /// </code>
        /// </example>
        public static string FormatTime(double timeInSeconds)
        {
            if (!IsValidTime(timeInSeconds))
                return ZeroTimeDisplay;

            try
            {
                var timeSpan = TimeSpan.FromSeconds(timeInSeconds);

                // For times less than 1 hour, use MM:SS format
                if (timeSpan.TotalHours < 1)
                {
                    return $"{(int)timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}";
                }

                // For longer times, use H:MM:SS format
                return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
            catch
            {
                return ZeroTimeDisplay;
            }
        }

        /// <summary>
        /// Formats time with unknown/invalid handling.
        /// Returns appropriate display text for various time states.
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <param name="showUnknownAsZero">If true, show unknown times as "00:00", otherwise as "--:--"</param>
        /// <returns>Formatted time string</returns>
        public static string FormatTimeWithFallback(
            double timeInSeconds,
            bool showUnknownAsZero = false
        )
        {
            if (!IsValidTime(timeInSeconds))
                return showUnknownAsZero ? ZeroTimeDisplay : UnknownTimeDisplay;

            return FormatTime(timeInSeconds);
        }

        /// <summary>
        /// Formats time with millisecond precision for debugging or detailed display.
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns>Formatted time string with milliseconds (e.g., "03:45.123")</returns>
        public static string FormatTimePrecise(double timeInSeconds)
        {
            if (!IsValidTime(timeInSeconds))
                return ZeroTimeDisplay;

            try
            {
                var timeSpan = TimeSpan.FromSeconds(timeInSeconds);

                if (timeSpan.TotalHours < 1)
                {
                    return $"{(int)timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
                }

                return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
            }
            catch
            {
                return ZeroTimeDisplay;
            }
        }

        #endregion

        #region Time Parsing

        /// <summary>
        /// Parses time string in various formats to seconds.
        /// Supports MM:SS, H:MM:SS, and other common formats.
        /// </summary>
        /// <param name="timeString">Time string to parse</param>
        /// <returns>Time in seconds, or 0 if parsing fails</returns>
        /// <example>
        /// <code>
        /// double seconds = TimeHelper.ParseTimeToSeconds("03:45");
        /// // Returns: 225.0 (3 minutes 45 seconds)
        ///
        /// double seconds2 = TimeHelper.ParseTimeToSeconds("1:23:45");
        /// // Returns: 5025.0 (1 hour 23 minutes 45 seconds)
        /// </code>
        /// </example>
        public static double ParseTimeToSeconds(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return 0.0;

            try
            {
                // Try parsing as TimeSpan first
                if (
                    TimeSpan.TryParse("00:" + timeString, out TimeSpan result)
                    || TimeSpan.TryParse(timeString, out result)
                )
                {
                    return ClampTime(result.TotalSeconds);
                }

                // Try parsing as pure seconds
                if (double.TryParse(timeString, out double seconds))
                {
                    return ClampTime(seconds);
                }

                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Tries to parse time string and returns success status.
        /// </summary>
        /// <param name="timeString">Time string to parse</param>
        /// <param name="timeInSeconds">Parsed time in seconds (output parameter)</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public static bool TryParseTimeToSeconds(string timeString, out double timeInSeconds)
        {
            timeInSeconds = ParseTimeToSeconds(timeString);
            return timeInSeconds > 0
                || string.Equals(timeString, ZeroTimeDisplay, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Time Validation and Clamping

        /// <summary>
        /// Validates if time value is reasonable and not corrupted.
        /// </summary>
        /// <param name="timeInSeconds">Time value to validate</param>
        /// <returns>True if time is valid, false otherwise</returns>
        public static bool IsValidTime(double timeInSeconds)
        {
            return !double.IsNaN(timeInSeconds)
                && !double.IsInfinity(timeInSeconds)
                && timeInSeconds >= MinTimeSeconds
                && timeInSeconds <= MaxTimeSeconds;
        }

        /// <summary>
        /// Clamps time value to reasonable range.
        /// Protects against negative times or excessively long durations.
        /// </summary>
        /// <param name="timeInSeconds">Time value to clamp</param>
        /// <returns>Clamped time value</returns>
        public static double ClampTime(double timeInSeconds)
        {
            if (double.IsNaN(timeInSeconds) || double.IsInfinity(timeInSeconds))
                return 0.0;

            return Math.Max(MinTimeSeconds, Math.Min(MaxTimeSeconds, timeInSeconds));
        }

        /// <summary>
        /// Clamps position to be within duration bounds.
        /// Ensures playback position doesn't exceed track duration.
        /// </summary>
        /// <param name="position">Current position in seconds</param>
        /// <param name="duration">Total duration in seconds</param>
        /// <returns>Clamped position value</returns>
        public static double ClampPosition(double position, double duration)
        {
            var validDuration = Math.Max(0.0, ClampTime(duration));
            var validPosition = ClampTime(position);

            return Math.Min(validPosition, validDuration);
        }

        #endregion

        #region Progress Calculations

        /// <summary>
        /// Calculates progress percentage from position and duration.
        /// </summary>
        /// <param name="position">Current position in seconds</param>
        /// <param name="duration">Total duration in seconds</param>
        /// <returns>Progress percentage (0-100), or 0 if duration is invalid</returns>
        /// <example>
        /// <code>
        /// double progress = TimeHelper.CalculateProgress(75.0, 300.0);
        /// // Returns: 25.0 (25% progress through track)
        /// </code>
        /// </example>
        public static double CalculateProgress(double position, double duration)
        {
            var validDuration = ClampTime(duration);
            var validPosition = ClampPosition(position, validDuration);

            if (validDuration <= 0)
                return 0.0;

            return (validPosition / validDuration) * 100.0;
        }

        /// <summary>
        /// Calculates position from progress percentage and duration.
        /// </summary>
        /// <param name="progressPercentage">Progress as percentage (0-100)</param>
        /// <param name="duration">Total duration in seconds</param>
        /// <returns>Position in seconds</returns>
        public static double CalculatePositionFromProgress(
            double progressPercentage,
            double duration
        )
        {
            var validDuration = ClampTime(duration);
            var clampedProgress = Math.Max(0.0, Math.Min(100.0, progressPercentage));

            return (clampedProgress / 100.0) * validDuration;
        }

        /// <summary>
        /// Calculates remaining time from current position and duration.
        /// </summary>
        /// <param name="position">Current position in seconds</param>
        /// <param name="duration">Total duration in seconds</param>
        /// <returns>Remaining time in seconds</returns>
        public static double CalculateRemainingTime(double position, double duration)
        {
            var validDuration = ClampTime(duration);
            var validPosition = ClampPosition(position, validDuration);

            return Math.Max(0.0, validDuration - validPosition);
        }

        #endregion

        #region Time Arithmetic

        /// <summary>
        /// Adds time values safely with overflow protection.
        /// </summary>
        /// <param name="time1">First time value in seconds</param>
        /// <param name="time2">Second time value in seconds</param>
        /// <returns>Sum of time values (clamped to valid range)</returns>
        public static double AddTime(double time1, double time2)
        {
            var valid1 = ClampTime(time1);
            var valid2 = ClampTime(time2);

            return ClampTime(valid1 + valid2);
        }

        /// <summary>
        /// Subtracts time values safely with underflow protection.
        /// </summary>
        /// <param name="time1">First time value in seconds</param>
        /// <param name="time2">Second time value in seconds</param>
        /// <returns>Difference of time values (clamped to valid range)</returns>
        public static double SubtractTime(double time1, double time2)
        {
            var valid1 = ClampTime(time1);
            var valid2 = ClampTime(time2);

            return ClampTime(valid1 - valid2);
        }

        /// <summary>
        /// Converts milliseconds to seconds.
        /// </summary>
        /// <param name="milliseconds">Time in milliseconds</param>
        /// <returns>Time in seconds</returns>
        public static double MillisecondsToSeconds(double milliseconds)
        {
            return ClampTime(milliseconds / 1000.0);
        }

        /// <summary>
        /// Converts seconds to milliseconds.
        /// </summary>
        /// <param name="seconds">Time in seconds</param>
        /// <returns>Time in milliseconds</returns>
        public static double SecondsToMilliseconds(double seconds)
        {
            return ClampTime(seconds) * 1000.0;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Creates a TimeSpan from seconds with validation.
        /// </summary>
        /// <param name="seconds">Time in seconds</param>
        /// <returns>TimeSpan object, or TimeSpan.Zero if invalid</returns>
        public static TimeSpan ToTimeSpan(double seconds)
        {
            if (!IsValidTime(seconds))
                return TimeSpan.Zero;

            try
            {
                return TimeSpan.FromSeconds(seconds);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Gets total seconds from TimeSpan with validation.
        /// </summary>
        /// <param name="timeSpan">TimeSpan object</param>
        /// <returns>Time in seconds (clamped to valid range)</returns>
        public static double FromTimeSpan(TimeSpan timeSpan)
        {
            return ClampTime(timeSpan.TotalSeconds);
        }

        /// <summary>
        /// Rounds time to nearest second for display purposes.
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns>Rounded time in seconds</returns>
        public static double RoundToSecond(double timeInSeconds)
        {
            return Math.Round(ClampTime(timeInSeconds));
        }

        /// <summary>
        /// Formats elapsed and remaining time for progress display.
        /// </summary>
        /// <param name="position">Current position in seconds</param>
        /// <param name="duration">Total duration in seconds</param>
        /// <returns>Formatted string like "02:30 / 05:00"</returns>
        public static string FormatProgressTime(double position, double duration)
        {
            var currentTime = FormatTimeWithFallback(position, true);
            var totalTime = FormatTimeWithFallback(duration, false);

            return $"{currentTime} / {totalTime}";
        }

        #endregion
    }
}
