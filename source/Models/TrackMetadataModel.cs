// ====================================================================
// FILE: TrackMetadataModel.cs
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
// Unified data model representing comprehensive metadata for a single track.
// Aggregates information from multiple sources (MP3 tags, Discogs, MusicBrainz)
// into a single cohesive model for UI binding and data persistence.
//
// FEATURES:
// - Complete track metadata aggregation
// - Multi-source data consolidation
// - Cover art support with BitmapImage
// - Track numbering and collection context
// - External service integration (Discogs URL, Release ID)
// - JSON serialization compatibility
// - UI data binding optimization
//
// DEPENDENCIES:
// - System.Collections.Generic (List collections)
// - System.Windows.Media.Imaging (BitmapImage for cover art)
//
// DATA STRUCTURE:
// - Core Metadata: Title, Artist, Album, Year, Genre, Comment
// - Media Info: Cover, Duration, Source information
// - Track Context: TrackNumber, TotalTracks, Tracklist
// - External Data: DiscogsUrl, ReleaseId, Country, Status, Style
// - Collection Info: Album-level metadata and track listings
//
// AGGREGATION SOURCES:
// - MP3 Tags: ID3v1/ID3v2 metadata from audio files
// - Discogs API: External music database information
// - MusicBrainz API: Open music encyclopedia data
// - User Input: Manual metadata corrections and additions
//
// SERIALIZATION:
// - JSON-compatible structure for caching
// - All properties serializable without custom converters
// - BitmapImage requires special handling for persistence
// - Suitable for metadata cache storage
//
// PERFORMANCE NOTES:
// - Optimized for frequent UI updates
// - Minimal memory allocation for property access
// - Efficient for data binding scenarios
// - Cover image lazy loading compatible
//
// LIMITATIONS:
// - Single cover image only
// - No metadata versioning or history
// - Limited validation of aggregated data
// - No conflict resolution between sources
//
// FUTURE REFACTORING:
// TODO: Implement IMetadataModel interface
// TODO: Add metadata source tracking per property
// TODO: Implement validation and data quality scoring
// TODO: Add conflict resolution between metadata sources
// TODO: Support multiple cover images and artwork types
// TODO: Add metadata change tracking and audit trail
// TODO: Implement lazy loading for expensive properties
// TODO: Add metadata merging rules and priority system
// CONSIDER: Breaking into separate track and album models
// CONSIDER: Adding metadata confidence scoring
// IDEA: Real-time metadata synchronization across sources
// IDEA: User preference learning for metadata conflicts
//
// TESTING:
// - Unit tests for property assignment and retrieval
// - Serialization/deserialization tests
// - Data binding integration tests
// - Multi-source aggregation scenarios
//
// UI INTEGRATION:
// - Optimized for WPF data binding
// - Track detail display scenarios
// - Metadata editing interfaces
// - Collection and album views
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF BitmapImage support
// - JSON serialization libraries
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive track metadata
// ====================================================================

using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace OstPlayer.Models
{
    // Represents unified metadata for a single track, aggregating data from various sources.
    public class TrackMetadataModel
    {
        public BitmapImage Cover { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public string Comment { get; set; }
        public string Duration { get; set; }
        public uint TrackNumber { get; set; }
        public uint TotalTracks { get; set; }
        public string Source { get; set; }
        public string ReleaseId { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public string Style { get; set; }
        public string DiscogsUrl { get; set; }
        public List<string> Tracklist { get; set; }
    }
}
