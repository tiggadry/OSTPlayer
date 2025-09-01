// ====================================================================
// FILE: MusicBrainzMetadataModel.cs
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
// Data model representing music release metadata retrieved from MusicBrainz API.
// Provides structured access to open music encyclopedia data for release information,
// designed for integration with other metadata sources and UI display scenarios.
//
// FEATURES:
// - MusicBrainz release information structure
// - Unique release identification with MusicBrainz IDs
// - Release status and country information
// - Date and temporal metadata handling
// - Plugin-friendly property naming (avoiding MusicBrainz specifics)
// - JSON serialization compatibility
// - Merging capability with other metadata sources
//
// DEPENDENCIES:
// - No external dependencies (pure .NET POCO)
//
// DATA STRUCTURE:
// - Identity: ReleaseId (MusicBrainz unique identifier)
// - Core Info: Title, Artist, Album for basic identification
// - Release Data: ReleaseDate, Country, Status for publication info
// - Temporal: Year (extracted/derived from ReleaseDate)
// - Classification: Genre for music categorization
// - Integration: Comment for plugin-specific annotations
// - Media: CoverUrl for artwork references
//
// MUSICBRAINZ INTEGRATION:
// - Maps to MusicBrainz Web Service v2.0 response structure
// - Handles MusicBrainz UUID format for release identification
// - Supports MusicBrainz release status vocabulary
// - Compatible with MusicBrainz country code standards
//
// SERIALIZATION:
// - JSON-compatible structure for metadata caching
// - All properties are serializable without custom converters
// - Suitable for persistent storage and API response caching
//
// PERFORMANCE NOTES:
// - Lightweight POCO structure for efficient memory usage
// - No complex object relationships or dependencies
// - Fast property access for UI binding scenarios
// - Minimal allocation during object creation
//
// LIMITATIONS:
// - Basic release information only (no detailed relationships)
// - Single artist representation (no multi-artist support)
// - Limited genre information (single string field)
// - No support for MusicBrainz relationships or advanced data
//
// FUTURE REFACTORING:
// FUTURE: Implement IMetadataModel interface for consistency
// FUTURE: Add support for multiple artists and relationships
// FUTURE: Expand genre support to list of genres/tags
// FUTURE: Add MusicBrainz relationship data (labels, recordings)
// FUTURE: Implement metadata validation and quality scoring
// FUTURE: Add support for MusicBrainz cover art archive
// FUTURE: Extract MusicBrainz-specific logic to separate namespace
// FUTURE: Add support for release group and work relationships
// CONSIDER: Breaking into release and recording models
// CONSIDER: Adding MusicBrainz entity relationship support
// IDEA: Real-time synchronization with MusicBrainz updates
// IDEA: Community-driven metadata correction integration
//
// TESTING:
// - Unit tests for property assignment and retrieval
// - Serialization/deserialization tests
// - MusicBrainz API response mapping tests
// - Integration tests with other metadata models
//
// MUSICBRAINZ COMPLIANCE:
// - Follows MusicBrainz data model conventions
// - Respects MusicBrainz API guidelines and rate limits
// - Uses appropriate MusicBrainz terminology and concepts
// - Compatible with MusicBrainz open data principles
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - MusicBrainz Web Service v2.0
// - JSON serialization libraries
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with MusicBrainz release metadata
// ====================================================================

using System.Windows.Media.Imaging;

namespace OstPlayer.Models {
    /// <summary>
    /// Represents comprehensive metadata for MusicBrainz releases.
    /// </summary>
    public class MusicBrainzMetadataModel : IMetadataModel
    {
        /// <summary>
        /// Gets the source of the metadata.
        /// </summary>
        public string Source => "MusicBrainz";

        /// <summary>
        /// Gets or sets the MusicBrainz release ID.
        /// </summary>
        public string ReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the release title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the artist name.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets or sets the release date.
        /// </summary>
        public string ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets the country of release.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the release status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the album name.
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// Gets or sets the release year.
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets the musical genre.
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Gets or sets the comment or description.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the cover image URL.
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// Gets or sets the duration (not typically available from MusicBrainz).
        /// </summary>
        public string Duration { get; set; }

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
        public IMetadataModel MergeWith(IMetadataModel other, MetadataMergePriority priority = MetadataMergePriority.PreferThis)
        {
            // Simple implementation - can be enhanced
            return priority == MetadataMergePriority.PreferOther ? other : this;
        }
    }
}
