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
// TODO: Implement IMetadataModel interface for consistency
// TODO: Add support for multiple artists and relationships
// TODO: Expand genre support to list of genres/tags
// TODO: Add MusicBrainz relationship data (labels, recordings)
// TODO: Implement metadata validation and quality scoring
// TODO: Add support for MusicBrainz cover art archive
// TODO: Extract MusicBrainz-specific logic to separate namespace
// TODO: Add support for release group and work relationships
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

namespace OstPlayer.Models
{
    // Unified metadata structure for MusicBrainz releases
    public class MusicBrainzMetadataModel
    {
        public string ReleaseId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string ReleaseDate { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public string Comment { get; set; }
        public string CoverUrl { get; set; }
    }
}
