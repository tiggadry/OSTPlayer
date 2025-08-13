// ====================================================================
// FILE: AlbumMetadataModel.cs
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
// Unified data model for album-level metadata aggregation and display.
// Consolidates information from multiple sources (MP3, Discogs, MusicBrainz)
// into a cohesive album representation for UI binding and metadata management.
//
// FEATURES:
// - Album-level metadata aggregation
// - Multi-source data consolidation
// - Cover art support with BitmapImage
// - Genre and style classification
// - External service integration (Discogs URL)
// - Track listing compilation
// - JSON serialization compatibility
// - UI data binding optimization
//
// DEPENDENCIES:
// - System.Collections.Generic (List collections)
// - System.Windows.Media.Imaging (BitmapImage for cover art)
//
// DATA STRUCTURE:
// - Core Album Info: Title, Artist, Album name
// - Release Data: Released date, Country information
// - Classification: Genres, Styles for music categorization
// - Media: Cover image for album artwork
// - Integration: DiscogsUrl for external reference
// - Content: Tracklist for album composition
// - Annotation: Comment for user or plugin notes
//
// AGGREGATION SOURCES:
// - MP3 Tags: Album-level information from audio files
// - Discogs API: Comprehensive album metadata and artwork
// - MusicBrainz API: Release information and classification
// - Manual Input: User corrections and additional metadata
//
// UI INTEGRATION:
// - Optimized for album detail views
// - Cover art display in galleries
// - Album browser and search interfaces
// - Metadata editing and management screens
//
// PERFORMANCE NOTES:
// - Lightweight structure for album collections
// - Efficient property access for UI binding
// - Minimal memory allocation for metadata operations
// - Suitable for large music library scenarios
//
// LIMITATIONS:
// - Single cover image support only
// - No multi-disc album organization
// - Limited metadata source tracking
// - No validation or quality scoring
//
// FUTURE REFACTORING:
// TODO: Implement IMetadataModel interface for consistency
// TODO: Add metadata source tracking per property
// TODO: Implement multi-disc album support
// TODO: Add metadata validation and quality scoring
// TODO: Support multiple cover images and artwork types
// TODO: Add metadata merging rules and conflict resolution
// TODO: Extract album/release distinction for clarity
// TODO: Add metadata change tracking and history
// CONSIDER: Separating release metadata from album metadata
// CONSIDER: Adding metadata confidence and reliability scoring
// IDEA: Real-time metadata synchronization across sources
// IDEA: Community-driven metadata correction and enhancement
//
// TESTING:
// - Unit tests for property assignment and retrieval
// - Serialization/deserialization tests
// - Multi-source aggregation scenario tests
// - UI binding integration tests
//
// METADATA MERGING:
// - Designed for consolidation from multiple metadata sources
// - Priority-based field selection from different providers
// - Conflict resolution for disagreeing metadata values
// - Completeness optimization for missing information
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF BitmapImage support
// - JSON serialization libraries
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with unified album metadata
// ====================================================================

using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace OstPlayer.Models
{
    /// <summary>
    /// Unified model for displaying album metadata in the plugin UI.
    /// </summary>
    public class AlbumMetadataModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Released { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Styles { get; set; }
        public string Comment { get; set; }
        public string Country { get; set; }
        public string DiscogsUrl { get; set; }
        public BitmapImage Cover { get; set; }
        public List<string> Tracklist { get; set; }
        // Add more properties as needed
    }
}
