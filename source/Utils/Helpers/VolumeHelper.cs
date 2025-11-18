// ====================================================================
// FILE: VolumeHelper.cs
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
// Helper utility class for volume-related operations in OstPlayer.
// Provides static methods for volume formatting, validation, and conversion.
// This is STEP 2 of ultra-safe refactoring - helper utilities only, NO usage yet.
//
// FEATURES:
// - Volume percentage formatting (e.g., "75%")
// - Volume value validation and clamping (0-100 range)
// - Volume conversion between percentage and normalized values (0.0-1.0)
// - Volume increment/decrement with bounds checking
// - Volume preset values (mute, low, medium, high, max)
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
// - System (Math functions)
// - No external dependencies
//
// FUTURE USAGE:
// When ready for safe extraction, existing volume operations can use these helpers.
// Example: VolumeDisplay => VolumeHelper.FormatPercentage(Volume)
//
// TESTING REQUIREMENTS:
// 1. Project builds successfully with new helper
// 2. No regressions in existing functionality
// 3. Plugin loads and works normally
// 4. All current volume features remain functional
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

namespace OstPlayer.Utils.Helpers {
    /// <summary>
    /// Static helper class for volume-related operations and calculations.
    /// Provides utility methods for volume formatting, validation, and conversion.
    ///
    /// SAFETY NOTE: This is infrastructure preparation - NOT USED by existing code yet.
    /// Can be safely added without affecting current functionality.
    /// </summary>
    public static class VolumeHelper {
        #region Constants

        /// <summary>
        /// Minimum allowed volume level (0%).
        /// </summary>
        public const double MinVolume = 0.0;

        /// <summary>
        /// Maximum allowed volume level (100%).
        /// </summary>
        public const double MaxVolume = 100.0;

        /// <summary>
        /// Default volume level when no preference is set (50%).
        /// </summary>
        public const double DefaultVolume = 50.0;

        /// <summary>
        /// Volume increment/decrement step size (5%).
        /// </summary>
        public const double VolumeStep = 5.0;

        #endregion

        #region Volume Formatting

        /// <summary>
        /// Formats volume as percentage string with % symbol.
        /// </summary>
        /// <param name="volume">Volume level (0-100)</param>
        /// <returns>Formatted string (e.g., "75%")</returns>
        /// <example>
        /// <code>
        /// string display = VolumeHelper.FormatPercentage(75.5);
        /// // Returns: "76%" (rounded to nearest integer)
        /// </code>
        /// </example>
        public static string FormatPercentage(double volume) {
            var clampedVolume = ClampVolume(volume);
            return $"{(int)Math.Round(clampedVolume)}%";
        }

        /// <summary>
        /// Formats volume as percentage string without % symbol.
        /// Useful for numeric input fields or calculations.
        /// </summary>
        /// <param name="volume">Volume level (0-100)</param>
        /// <returns>Formatted string (e.g., "75")</returns>
        public static string FormatPercentageValue(double volume) {
            var clampedVolume = ClampVolume(volume);
            return $"{(int)Math.Round(clampedVolume)}";
        }

        /// <summary>
        /// Formats volume with detailed precision for debugging or advanced display.
        /// </summary>
        /// <param name="volume">Volume level (0-100)</param>
        /// <param name="decimalPlaces">Number of decimal places to show (default: 1)</param>
        /// <returns>Formatted string with specified precision</returns>
        public static string FormatPrecise(double volume, int decimalPlaces = 1) {
            var clampedVolume = ClampVolume(volume);
            var format = $"F{Math.Max(0, Math.Min(10, decimalPlaces))}"; // Limit decimal places
            return $"{clampedVolume.ToString(format)}%";
        }

        #endregion

        #region Volume Validation and Clamping

        /// <summary>
        /// Clamps volume value to valid range (0-100).
        /// Ensures volume values are always within acceptable bounds.
        /// </summary>
        /// <param name="volume">Volume value to clamp</param>
        /// <returns>Clamped volume value between 0 and 100</returns>
        /// <example>
        /// <code>
        /// double safe = VolumeHelper.ClampVolume(150.0);
        /// // Returns: 100.0 (clamped to maximum)
        ///
        /// double safe2 = VolumeHelper.ClampVolume(-10.0);
        /// // Returns: 0.0 (clamped to minimum)
        /// </code>
        /// </example>
        public static double ClampVolume(double volume) {
            if (double.IsNaN(volume) || double.IsInfinity(volume))
                return DefaultVolume;

            return Math.Max(MinVolume, Math.Min(MaxVolume, volume));
        }

        /// <summary>
        /// Validates if volume value is within acceptable range.
        /// </summary>
        /// <param name="volume">Volume value to validate</param>
        /// <returns>True if volume is valid (0-100), false otherwise</returns>
        public static bool IsValidVolume(double volume) {
            return !double.IsNaN(volume) &&
                   !double.IsInfinity(volume) &&
                   volume >= MinVolume &&
                   volume <= MaxVolume;
        }

        /// <summary>
        /// Rounds volume to nearest valid step value.
        /// Useful for ensuring consistent volume levels in UI controls.
        /// </summary>
        /// <param name="volume">Volume value to round</param>
        /// <param name="stepSize">Step size for rounding (default: 5.0)</param>
        /// <returns>Rounded volume value</returns>
        public static double RoundToStep(double volume, double stepSize = VolumeStep) {
            var clampedVolume = ClampVolume(volume);
            var validStepSize = Math.Max(0.1, Math.Min(50.0, stepSize)); // Reasonable step limits

            return Math.Round(clampedVolume / validStepSize) * validStepSize;
        }

        #endregion

        #region Volume Conversion

        /// <summary>
        /// Converts percentage volume (0-100) to normalized value (0.0-1.0).
        /// Used for audio engine APIs that expect normalized values.
        /// </summary>
        /// <param name="percentage">Volume as percentage (0-100)</param>
        /// <returns>Normalized volume value (0.0-1.0)</returns>
        /// <example>
        /// <code>
        /// double normalized = VolumeHelper.PercentageToNormalized(75.0);
        /// // Returns: 0.75
        /// </code>
        /// </example>
        public static double PercentageToNormalized(double percentage) {
            var clampedPercentage = ClampVolume(percentage);
            return clampedPercentage / 100.0;
        }

        /// <summary>
        /// Converts normalized volume (0.0-1.0) to percentage (0-100).
        /// Used when receiving volume from audio engines that use normalized values.
        /// </summary>
        /// <param name="normalized">Normalized volume value (0.0-1.0)</param>
        /// <returns>Volume as percentage (0-100)</returns>
        /// <example>
        /// <code>
        /// double percentage = VolumeHelper.NormalizedToPercentage(0.75);
        /// // Returns: 75.0
        /// </code>
        /// </example>
        public static double NormalizedToPercentage(double normalized) {
            if (double.IsNaN(normalized) || double.IsInfinity(normalized))
                return DefaultVolume;

            var clampedNormalized = Math.Max(0.0, Math.Min(1.0, normalized));
            return clampedNormalized * 100.0;
        }

        #endregion

        #region Volume Adjustment

        /// <summary>
        /// Increases volume by specified amount with bounds checking.
        /// </summary>
        /// <param name="currentVolume">Current volume level</param>
        /// <param name="increment">Amount to increase (default: VolumeStep)</param>
        /// <returns>New volume level (clamped to valid range)</returns>
        public static double IncreaseVolume(double currentVolume, double increment = VolumeStep) {
            var validIncrement = Math.Max(0.0, increment);
            return ClampVolume(currentVolume + validIncrement);
        }

        /// <summary>
        /// Decreases volume by specified amount with bounds checking.
        /// </summary>
        /// <param name="currentVolume">Current volume level</param>
        /// <param name="decrement">Amount to decrease (default: VolumeStep)</param>
        /// <returns>New volume level (clamped to valid range)</returns>
        public static double DecreaseVolume(double currentVolume, double decrement = VolumeStep) {
            var validDecrement = Math.Max(0.0, decrement);
            return ClampVolume(currentVolume - validDecrement);
        }

        /// <summary>
        /// Toggles between mute (0%) and previous volume level.
        /// </summary>
        /// <param name="currentVolume">Current volume level</param>
        /// <param name="previousVolume">Previous volume level to restore (default: DefaultVolume)</param>
        /// <returns>New volume level (0 if not muted, previousVolume if muted)</returns>
        public static double ToggleMute(double currentVolume, double previousVolume = DefaultVolume) {
            var validPreviousVolume = Math.Max(1.0, ClampVolume(previousVolume));

            // If currently muted (or very low), restore previous volume
            if (currentVolume <= 0.5) {
                return validPreviousVolume;
            }

            // Otherwise, mute
            return MinVolume;
        }

        #endregion

        #region Volume Presets

        /// <summary>
        /// Gets mute volume level (0%).
        /// </summary>
        public static double MuteVolume => MinVolume;

        /// <summary>
        /// Gets low volume level (25%).
        /// </summary>
        public static double LowVolume => 25.0;

        /// <summary>
        /// Gets medium volume level (50%).
        /// </summary>
        public static double MediumVolume => DefaultVolume;

        /// <summary>
        /// Gets high volume level (75%).
        /// </summary>
        public static double HighVolume => 75.0;

        /// <summary>
        /// Gets maximum volume level (100%).
        /// </summary>
        public static double MaximumVolume => MaxVolume;

        /// <summary>
        /// Gets array of common volume preset values.
        /// Useful for creating volume selection UI controls.
        /// </summary>
        /// <returns>Array of preset volume values</returns>
        public static double[] GetVolumePresets() {
            return new double[]
            {
                MuteVolume,    // 0%
                LowVolume,     // 25%
                MediumVolume,  // 50%
                HighVolume,    // 75%
                MaximumVolume  // 100%
            };
        }

        /// <summary>
        /// Gets the name of a volume preset level.
        /// </summary>
        /// <param name="volume">Volume level to identify</param>
        /// <returns>Preset name or null if not a standard preset</returns>
        public static string GetPresetName(double volume) {
            var rounded = Math.Round(volume);

            switch (rounded) {
                case 0: return "Mute";
                case 25: return "Low";
                case 50: return "Medium";
                case 75: return "High";
                case 100: return "Maximum";
                default: return null;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Calculates volume difference between two levels.
        /// </summary>
        /// <param name="fromVolume">Starting volume level</param>
        /// <param name="toVolume">Target volume level</param>
        /// <returns>Volume difference (positive for increase, negative for decrease)</returns>
        public static double CalculateVolumeDifference(double fromVolume, double toVolume) {
            return ClampVolume(toVolume) - ClampVolume(fromVolume);
        }

        /// <summary>
        /// Interpolates between two volume levels.
        /// Useful for smooth volume transitions or animations.
        /// </summary>
        /// <param name="fromVolume">Starting volume level</param>
        /// <param name="toVolume">Target volume level</param>
        /// <param name="factor">Interpolation factor (0.0 = from, 1.0 = to)</param>
        /// <returns>Interpolated volume level</returns>
        public static double InterpolateVolume(double fromVolume, double toVolume, double factor) {
            var clampedFrom = ClampVolume(fromVolume);
            var clampedTo = ClampVolume(toVolume);
            var clampedFactor = Math.Max(0.0, Math.Min(1.0, factor));

            return clampedFrom + (clampedTo - clampedFrom) * clampedFactor;
        }

        #endregion
    }
}
