// ====================================================================
// FILE: IMetadataModel.cs
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
// Common interface defining the contract for all metadata models in the system.
// Enables polymorphic handling of metadata from different sources (MP3, Discogs, MusicBrainz)
// and provides standardized validation and merging capabilities across metadata types.
//
// FEATURES:
// - Unified metadata property interface
// - Source identification for metadata providers
// - Validation method for data completeness
// - Merging capability with priority rules
// - Polymorphic metadata handling support
// - Type-safe metadata operations
//
// DEPENDENCIES:
// - System.Collections.Generic (List collections)
// - System.Windows.Media.Imaging (BitmapImage for cover art)
//
// INTERFACE CONTRACT:
// - Source: Identifies metadata provider (MP3, Discogs, MusicBrainz)
// - Core Properties: Title, Artist, Album, Year, Genre, Duration
// - Track Info: TrackNumber, TotalTracks for collection context
// - Media: Cover property for artwork support
// - Validation: IsValid() for data completeness checking
// - Merging: MergeWith() for combining metadata sources
//
// MERGING STRATEGIES:
// - PreferThis: Current instance takes precedence
// - PreferOther: Parameter instance takes precedence
// - PreferMostComplete: Most complete metadata wins
//
// DESIGN PATTERNS:
// - Strategy Pattern (merge priority strategies)
// - Template Method (validation implementation)
// - Polymorphism (unified metadata handling)
//
// PERFORMANCE NOTES:
// - Interface overhead minimal for metadata operations
// - Efficient polymorphic method dispatch
// - Merge operations designed for minimal allocations
// - Validation methods optimized for frequent calls
//
// LIMITATIONS:
// - Fixed property set (no dynamic metadata)
// - Single cover image support only
// - Limited merge strategy options
// - No metadata versioning support
//
// FUTURE REFACTORING:
// FUTURE: Add dynamic metadata property support
// FUTURE: Implement metadata versioning and compatibility
// FUTURE: Add more sophisticated merge strategies
// FUTURE: Support multiple cover images/artwork types
// FUTURE: Add metadata quality scoring interface
// FUTURE: Implement metadata source reliability tracking
// FUTURE: Add async validation for external metadata
// FUTURE: Extract merge priority to separate strategy classes
// CONSIDER: Generic metadata property collections
// CONSIDER: Metadata change tracking and diff support
// IDEA: Machine learning for optimal merge strategies
// IDEA: Metadata conflict resolution UI integration
//
// TESTING:
// - Unit tests for all implementing classes
// - Validation tests for various metadata completeness scenarios
// - Merge operation tests with different priority strategies
// - Polymorphic behavior tests across metadata types
//
// IMPLEMENTATION GUIDELINES:
// - All metadata models should implement this interface
// - Source property should identify the metadata provider clearly
// - IsValid() should check for minimum required metadata
// - MergeWith() should handle null inputs gracefully
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF BitmapImage support
// - All metadata model implementations
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with unified metadata interface
// ====================================================================

using System.Windows.Media.Imaging;

namespace OstPlayer.Models
{
    /// <summary>
    /// Common interface for all metadata models (MP3, Discogs, MusicBrainz, etc.)
    /// </summary>
    public interface IMetadataModel
    {
        /// <summary>
        /// Gets the source of the metadata (e.g., "MP3", "Discogs", "MusicBrainz").
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Gets or sets the title of the track or album.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the artist name.
        /// </summary>
        string Artist { get; set; }

        /// <summary>
        /// Gets or sets the album name.
        /// </summary>
        string Album { get; set; }

        /// <summary>
        /// Gets or sets the year of release.
        /// </summary>
        string Year { get; set; }

        /// <summary>
        /// Gets or sets the genre.
        /// </summary>
        string Genre { get; set; }

        /// <summary>
        /// Gets or sets the duration of the track.
        /// </summary>
        string Duration { get; set; }

        /// <summary>
        /// Gets or sets the track number.
        /// </summary>
        uint TrackNumber { get; set; }

        /// <summary>
        /// Gets or sets the total number of tracks.
        /// </summary>
        uint TotalTracks { get; set; }

        /// <summary>
        /// Gets or sets the cover art image.
        /// </summary>
        BitmapImage Cover { get; set; }

        /// <summary>
        /// Validates if the metadata contains minimum required information
        /// </summary>
        bool IsValid();

        /// <summary>
        /// Merges this metadata with another source, using priority rules
        /// </summary>
        IMetadataModel MergeWith(
            IMetadataModel other,
            MetadataMergePriority priority = MetadataMergePriority.PreferThis
        );
    }

    /// <summary>
    /// Specifies the priority strategy for merging metadata from different sources.
    /// </summary>
    public enum MetadataMergePriority
    {
        /// <summary>
        /// Prefer the current metadata instance.
        /// </summary>
        PreferThis,
        
        /// <summary>
        /// Prefer the other metadata instance.
        /// </summary>
        PreferOther,
        
        /// <summary>
        /// Prefer the metadata instance that is most complete.
        /// </summary>
        PreferMostComplete,
    }
}
