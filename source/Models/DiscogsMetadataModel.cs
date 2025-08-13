// ====================================================================
// FILE: DiscogsMetadataModel.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Models
// LOCATION: Models/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Data model representing music release metadata retrieved from Discogs API.
// Designed for serialization, UI binding, and merging with metadata from
// multiple sources (MP3 tags, MusicBrainz, local cache).
//
// FEATURES:
// - Complete Discogs release information structure
// - Automatic redundancy removal (Album vs Title)
// - Nested tracklist with duration information
// - Support for multiple genres and styles
// - Direct web links to Discogs pages
// - Cover image URL handling
// - Serialization-friendly design
//
// DEPENDENCIES:
// - System.Collections.Generic (List collections)
// - No external dependencies (POCO design)
//
// DATA STRUCTURE:
// - Title: Primary release title
// - Artist: Main performing artist
// - Album: Album name (only if different from Title)
// - Country: Country of release
// - Label: Record label information
// - Format: Physical format description
// - Genres: List of musical genres
// - Styles: List of musical styles
// - Notes: Additional release information
// - Released: Release date (ISO format)
// - Comment: Plugin-specific notes
// - CoverUrl: Direct link to cover artwork
// - DiscogsUrl: Link to Discogs release page
// - Tracklist: Collection of tracks with durations
//
// NESTED DISCOGSTRACK CLASS:
// - Individual track representation within releases
// - Title and Duration properties for track information
// - Supports album and compilation track listings
// - Compatible with Discogs API response structure
//
// SERIALIZATION:
// - JSON-compatible structure
// - Newtonsoft.Json friendly
// - No circular references
// - Null-safe property access
//
// UI BINDING:
// - Public properties for WPF data binding
// - Computed properties for display optimization
// - TrackTitles convenience property
//
// PERFORMANCE NOTES:
// - Lightweight POCO structure
// - Minimal memory allocation
// - Efficient property access
// - Lazy evaluation where possible
//
// LIMITATIONS:
// - Single-disc releases only
// - No position information for tracks
// - Limited format normalization
// - No metadata validation
//
// FUTURE REFACTORING:
// TODO: Implement IMetadataModel interface for consistency
// TODO: Add support for multi-disc releases
// TODO: Implement metadata validation attributes
// TODO: Add position information to tracks (A1, B2, etc.)
// TODO: Normalize format and label values
// TODO: Extract to shared metadata namespace
// TODO: Add INotifyPropertyChanged for live binding
// TODO: Implement metadata quality scoring
// TODO: Add metadata merging capabilities
// CONSIDER: Using record types for immutability (C# 9+)
// CONSIDER: Adding validation attributes
// IDEA: Automatic metadata conflict resolution
// IDEA: Machine learning for metadata improvement
//
// TESTING:
// - Serialization/deserialization tests
// - Property access tests
// - Null safety validation
// - Performance benchmarks
//
// EXTENSIBILITY:
// - Designed for inheritance and composition
// - Easy addition of new metadata fields
// - Compatible with other metadata sources
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - JSON serialization libraries
// - WPF data binding
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive Discogs metadata
// ====================================================================

using System.Collections.Generic;

namespace OstPlayer.Models
{
    // Unified metadata structure for Discogs releases
    public class DiscogsMetadataModel
    {
        // Release title (e.g., "The Dark Side of the Moon")
        public string Title { get; set; }
        // Main artist name (e.g., "Pink Floyd")
        public string Artist { get; set; }
        // Country of release (e.g., "UK")
        public string Country { get; set; }
        private string _album;
        // Album title, only returned if different from Title
        public string Album
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_album))
                    return null;
                if (string.Equals(_album, Title))
                    return null;
                return _album;
            }
            set { _album = value; }
        }
        // Record label(s) (e.g., "Harvest Records")
        public string Label { get; set; }
        // Format string describing the medium (e.g., "LP, Album, Reissue")
        public string Format { get; set; }
        // List of genres (e.g., ["Rock", "Progressive Rock"])
        public List<string> Genres { get; set; }
        // List of styles (e.g., ["Psychedelic Rock", "Art Rock"])
        public List<string> Styles { get; set; }
        // Optional notes about the release
        public string Notes { get; set; }
        // Release date in ISO format (e.g., "1973-03-01")
        public string Released { get; set; }
        // Reserved for plugin-specific notes
        public string Comment { get; set; }
        // URL to cover image
        public string CoverUrl { get; set; }
        // Direct link to the Discogs release page
        public string DiscogsUrl { get; set; }
        // List of tracks with title and duration
        public List<DiscogsTrack> Tracklist { get; set; }
        // Nested class representing a single track
        public class DiscogsTrack
        {
            // Track title
            public string Title { get; set; }
            // Duration in "mm:ss" format (optional)
            public string Duration { get; set; }
        }
        // Convenience property: list of track titles only
        public List<string> TrackTitles =>
            Tracklist?.ConvertAll(t => t.Title) ?? new List<string>();
    }
}
