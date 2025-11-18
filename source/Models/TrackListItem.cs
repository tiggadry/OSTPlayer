// ====================================================================
// FILE: TrackListItem.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Models
// LOCATION: Models/
// VERSION: 1.1.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Simple data model representing a track item for UI display in lists and controls.
// Provides essential track information for data binding in WPF views without
// the complexity of full metadata models. Optimized for ListBox and DataGrid scenarios.
// **NEW**: Enhanced with sorting support and duration parsing for DataGrid scenarios.
//
// FEATURES:
// - Essential track display properties (title, duration, file path)
// - Track numbering support for ordered lists
// - File path reference for playback operations
// - Lightweight structure for UI performance
// - WPF data binding compatibility
// - **NEW**: IComparable implementation for natural sorting
// - **NEW**: Duration parsing for proper time-based sorting
//
// DEPENDENCIES:
// - No external dependencies (pure .NET)
// - No WPF dependencies for cross-platform compatibility
// - System.ComponentModel (INotifyPropertyChanged for live updates)
//
// DATA STRUCTURE:
// - TrackTitle: Display name for the track
// - TrackDuration: Formatted duration string (e.g., "03:21")
// - FilePath: Full file system path for audio file
// - TrackNumber: Position in album/playlist (1-based)
// - TotalTracks: Total number of tracks in collection
// - **NEW**: DurationSeconds: Parsed duration for sorting
//
// UI BINDING:
// - Designed for ListBox ItemsSource binding
// - **ENHANCED**: DataGrid row representation with sorting
// - Track listing controls
// - Playlist management interfaces
//
// PERFORMANCE NOTES:
// - Minimal memory footprint per instance
// - No complex object references
// - String-based duration for UI efficiency
// - **NEW**: Cached duration parsing for sorting performance
// - Suitable for large track collections
//
// SORTING FEATURES:
// - Natural track number sorting (1, 2, 10 vs 1, 10, 2)
// - Proper duration sorting by time value
// - Case-insensitive title sorting
// - Multi-column sorting support
//
// FUTURE REFACTORING:
// FUTURE: Add validation for file path existence
// FUTURE: Consider TimeSpan property for duration calculations
// FUTURE: Add metadata preview properties (artist, album snippet)
// FUTURE: Implement INotifyPropertyChanged for live updates
// FUTURE: Add file format/codec information
// FUTURE: Extract to shared UI models namespace
// FUTURE: Add thumbnail/artwork support for rich lists
// FUTURE: Consider implementing IComparable for sorting
// CONSIDER: Adding metadata summary properties
// CONSIDER: File validation and health checking
// IDEA: Integration with playlist management systems
// IDEA: Track rating and user preference support
//
// TESTING:
// - Unit tests for property assignment and retrieval
// - UI binding tests with various data scenarios
// - Performance tests with large track collections
// - Memory usage validation for collections
//
// UI SCENARIOS:
// - Track listing in sidebar
// - Playlist management interfaces
// - Search result displays
// - Album track ordering
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF data binding compatible
// - Cross-platform data structure
//
// CHANGELOG:
// 2025-08-07 v1.1.0 - Enhanced with sorting support: IComparable implementation, duration parsing, natural sorting
// 2025-08-06 v1.0.0 - Initial implementation with essential track display properties
// ====================================================================

using System;
using System.ComponentModel;

namespace OstPlayer.Models {
    /// <summary>
    /// Model used to represent a track in the UI (e.g., track list in the sidebar).
    /// Enhanced with sorting capabilities for DataGrid scenarios.
    /// </summary>
    public class TrackListItem : INotifyPropertyChanged, IComparable<TrackListItem> {
        // Private Fields
        private string _trackTitle;
        private string _trackDuration;
        private uint _trackNumber;
        private double? _durationSeconds; // Cached parsed duration

        // Public Properties

        /// <summary>
        /// Track title with property change notification.
        /// </summary>
        public string TrackTitle {
            get => _trackTitle;
            set {
                if (_trackTitle != value) {
                    _trackTitle = value;
                    OnPropertyChanged(nameof(TrackTitle));
                }
            }
        }

        /// <summary>
        /// Track duration as a formatted string (e.g., "03:21").
        /// Setting this property also parses and caches the duration in seconds for sorting.
        /// </summary>
        public string TrackDuration {
            get => _trackDuration;
            set {
                if (_trackDuration != value) {
                    _trackDuration = value;
                    _durationSeconds = ParseDurationToSeconds(value);
                    OnPropertyChanged(nameof(TrackDuration));
                }
            }
        }

        /// <summary>
        /// Full file path (for playback or display).
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Track number with property change notification.
        /// </summary>
        public uint TrackNumber {
            get => _trackNumber;
            set {
                if (_trackNumber != value) {
                    _trackNumber = value;
                    OnPropertyChanged(nameof(TrackNumber));
                }
            }
        }

        /// <summary>
        /// Total number of tracks.
        /// </summary>
        public uint TotalTracks { get; set; }

        /// <summary>
        /// Parsed duration in seconds for sorting purposes.
        /// Returns 0 if duration cannot be parsed.
        /// </summary>
        public double DurationSeconds => _durationSeconds ?? 0;

        // Duration Parsing

        /// <summary>
        /// Parses duration string to seconds for sorting purposes.
        /// Supports formats: "mm:ss", "h:mm:ss", "mm:ss.fff"
        /// </summary>
        /// <param name="durationString">Duration string to parse</param>
        /// <returns>Duration in seconds or null if parsing fails</returns>
        private static double? ParseDurationToSeconds(string durationString) {
            if (string.IsNullOrWhiteSpace(durationString))
                return null;

            try {
                // Try parsing as TimeSpan (supports h:mm:ss format)
                if (TimeSpan.TryParse("00:" + durationString, out TimeSpan duration) ||
                    TimeSpan.TryParse(durationString, out duration)) {
                    return duration.TotalSeconds;
                }

                // Fallback: Try manual parsing for "mm:ss" format
                var parts = durationString.Split(':');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int minutes) &&
                    double.TryParse(parts[1], out double seconds)) {
                    return (minutes * 60) + seconds;
                }
            }
            catch {
                // Ignore parsing errors
            }

            return null;
        }

        // IComparable Implementation

        /// <summary>
        /// Compares tracks for default sorting (by track number, then by title).
        /// </summary>
        /// <param name="other">Other track to compare with</param>
        /// <returns>Comparison result for sorting</returns>
        public int CompareTo(TrackListItem other) {
            if (other == null)
                return 1;

            // Primary sort: Track number (natural order)
            if (TrackNumber != other.TrackNumber) {
                // Handle case where one track has no number (0)
                if (TrackNumber == 0 && other.TrackNumber > 0)
                    return 1; // No number goes to end
                if (other.TrackNumber == 0 && TrackNumber > 0)
                    return -1; // No number goes to end

                return TrackNumber.CompareTo(other.TrackNumber);
            }

            // Secondary sort: Track title (case-insensitive)
            return string.Compare(TrackTitle ?? "", other.TrackTitle ?? "",
                StringComparison.OrdinalIgnoreCase);
        }

        // Static Comparison Methods for DataGrid Sorting

        /// <summary>
        /// Compares tracks by track number for DataGrid column sorting.
        /// </summary>
        public static int CompareByTrackNumber(TrackListItem a, TrackListItem b) {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            // Handle tracks without numbers
            if (a.TrackNumber == 0 && b.TrackNumber == 0)
                return string.Compare(a.TrackTitle ?? "", b.TrackTitle ?? "", StringComparison.OrdinalIgnoreCase);
            if (a.TrackNumber == 0) return 1;  // No number goes to end
            if (b.TrackNumber == 0) return -1; // No number goes to end

            return a.TrackNumber.CompareTo(b.TrackNumber);
        }

        /// <summary>
        /// Compares tracks by title for DataGrid column sorting.
        /// </summary>
        public static int CompareByTitle(TrackListItem a, TrackListItem b) {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            return string.Compare(a.TrackTitle ?? "", b.TrackTitle ?? "", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares tracks by duration for DataGrid column sorting.
        /// </summary>
        public static int CompareByDuration(TrackListItem a, TrackListItem b) {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            return a.DurationSeconds.CompareTo(b.DurationSeconds);
        }

        // INotifyPropertyChanged Implementation

        /// <summary>
        /// Property changed event for data binding updates.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the changed property</param>
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ToString Override for Debugging

        /// <summary>
        /// String representation for debugging purposes.
        /// </summary>
        public override string ToString() {
            var trackNum = TrackNumber > 0 ? $"{TrackNumber:00}" : "--";
            return $"[{trackNum}] {TrackTitle ?? "Unknown"} ({TrackDuration ?? "Unknown"})";
        }
    }
}
