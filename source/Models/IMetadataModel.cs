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
// TODO: Add dynamic metadata property support
// TODO: Implement metadata versioning and compatibility
// TODO: Add more sophisticated merge strategies
// TODO: Support multiple cover images/artwork types
// TODO: Add metadata quality scoring interface
// TODO: Implement metadata source reliability tracking
// TODO: Add async validation for external metadata
// TODO: Extract merge priority to separate strategy classes
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

using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace OstPlayer.Models
{
    /// <summary>
    /// Common interface for all metadata models (MP3, Discogs, MusicBrainz, etc.)
    /// </summary>
    public interface IMetadataModel
    {
        string Source { get; }
        string Title { get; set; }
        string Artist { get; set; }
        string Album { get; set; }
        string Year { get; set; }
        string Genre { get; set; }
        string Duration { get; set; }
        uint TrackNumber { get; set; }
        uint TotalTracks { get; set; }
        BitmapImage Cover { get; set; }
        
        /// <summary>
        /// Validates if the metadata contains minimum required information
        /// </summary>
        bool IsValid();
        
        /// <summary>
        /// Merges this metadata with another source, using priority rules
        /// </summary>
        IMetadataModel MergeWith(IMetadataModel other, MetadataMergePriority priority = MetadataMergePriority.PreferThis);
    }
    
    public enum MetadataMergePriority
    {
        PreferThis,
        PreferOther,
        PreferMostComplete
    }
}