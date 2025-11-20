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
// FUTURE: Implement IMetadataModel interface for consistency
// FUTURE: Add support for multi-disc releases
// FUTURE: Implement metadata validation attributes
// FUTURE: Add position information to tracks (A1, B2, etc.)
// FUTURE: Normalize format and label values
// FUTURE: Extract to shared metadata namespace
// FUTURE: Add INotifyPropertyChanged for live binding
// FUTURE: Implement metadata quality scoring
// FUTURE: Add metadata merging capabilities
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
using System.Windows.Media.Imaging;

namespace OstPlayer.Models
{
    /// <summary>
    /// Represents comprehensive metadata from Discogs database.
    /// </summary>
    public class DiscogsMetadataModel : IMetadataModel
    {
        /// <summary>
        /// Gets the source of the metadata.
        /// </summary>
        public string Source => "Discogs";

        /// <summary>
        /// Gets or sets the release title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the artist name.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets or sets the country of release.
        /// </summary>
        public string Country { get; set; }

        private string _album;

        /// <summary>
        /// Gets or sets the album name (only if different from Title).
        /// </summary>
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

        /// <summary>
        /// Gets or sets the record label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the format of the release.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the list of musical genres.
        /// </summary>
        public List<string> Genres { get; set; }

        /// <summary>
        /// Gets or sets the list of musical styles.
        /// </summary>
        public List<string> Styles { get; set; }

        /// <summary>
        /// Gets or sets the notes or description.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the release date.
        /// </summary>
        public string Released { get; set; }

        /// <summary>
        /// Gets or sets the comment text.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the cover image URL.
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// Gets or sets the Discogs URL for this release.
        /// </summary>
        public string DiscogsUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of tracks in the release.
        /// </summary>
        public List<DiscogsTrack> Tracklist { get; set; }

        /// <summary>
        /// Represents a track in a Discogs release.
        /// </summary>
        public class DiscogsTrack
        {
            /// <summary>
            /// Gets or sets the track title.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the track duration.
            /// </summary>
            public string Duration { get; set; }
        }

        /// <summary>
        /// Gets the list of track titles from the tracklist.
        /// </summary>
        public List<string> TrackTitles =>
            Tracklist?.ConvertAll(t => t.Title) ?? new List<string>();

        // IMetadataModel interface implementation
        /// <summary>
        /// Gets or sets the release year.
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets the genre.
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Gets or sets the duration (not typically available from Discogs).
        /// </summary>
        string IMetadataModel.Duration { get; set; }

        /// <summary>
        /// Gets or sets the track number.
        /// </summary>
        public uint TrackNumber { get; set; }

        /// <summary>
        /// Gets or sets the total number of tracks.
        /// </summary>
        public uint TotalTracks { get; set; }

        /// <summary>
        /// Gets or sets the cover art image.
        /// </summary>
        public BitmapImage Cover { get; set; }

        /// <summary>
        /// Validates if the metadata contains minimum required information.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Artist);
        }

        /// <summary>
        /// Merges this metadata with another source.
        /// </summary>
        public IMetadataModel MergeWith(
            IMetadataModel other,
            MetadataMergePriority priority = MetadataMergePriority.PreferThis
        )
        {
            // Simple implementation - can be enhanced
            return priority == MetadataMergePriority.PreferOther ? other : this;
        }
    }
}
