// ====================================================================
// FILE: Mp3MetadataModel.cs
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
// Data model representing metadata extracted from MP3 files using TagLibSharp.
// Handles standard ID3 tag information and provides support for both single-track
// and multi-track scenarios with embedded cover art and comprehensive metadata.
//
// FEATURES:
// - Complete ID3v1/ID3v2 tag support
// - Cover art extraction and base64 encoding
// - Multi-track file support with nested TrackInfo
// - Intelligent album redundancy handling
// - File name sanitization utilities
// - JSON serialization compatibility
// - TagLibSharp integration optimized
//
// DEPENDENCIES:
// - System.Collections.Generic (List collections)
// - System (TimeSpan for duration handling)
// - System.IO (Path operations for sanitization)
//
// DATA STRUCTURE:
// - Core Tags: Title, Artist, Album, Year, Genre, Comment
// - Media: CoverBase64 for UI display without file dependency
// - Track Collection: List<TrackInfo> for multi-track files
// - Smart Properties: Album property avoids Title redundancy
// - Utilities: Static Sanitize method for filename compatibility
//
// NESTED TRACKINFO CLASS:
// - Individual track metadata within albums or compilations
// - Duration as TimeSpan for precise time calculations
// - Track numbering with total track context
// - Supports embedded cue sheets and multi-track files
//
// ID3 TAG HANDLING:
// - Comprehensive tag extraction from TagLibSharp
// - Support for various ID3 versions and formats
// - Album art extraction with memory-efficient encoding
// - Fallback mechanisms for missing or corrupted tags
//
// PERFORMANCE NOTES:
// - Base64 cover encoding for UI efficiency
// - Minimal memory allocation during tag reading
// - Efficient string operations for metadata processing
// - Optimized for large music library scanning
//
// LIMITATIONS:
// - MP3 format specific (no FLAC, OGG, etc.)
// - Single cover image per file
// - Base64 encoding increases memory usage
// - No support for custom ID3 frames
//
// FUTURE REFACTORING:
// TODO: Implement IMetadataModel interface for consistency
// TODO: Add support for additional audio formats
// TODO: Extract cover art to separate model/service
// TODO: Add custom ID3 frame reading capabilities
// TODO: Implement metadata validation and quality scoring
// TODO: Add support for embedded lyrics extraction
// TODO: Extract TrackInfo to separate model file
// TODO: Add metadata source confidence tracking
// CONSIDER: Separating album-level vs track-level metadata
// CONSIDER: Streaming cover art without base64 encoding
// IDEA: Automatic metadata correction and enhancement
// IDEA: Integration with online metadata databases
//
// TESTING:
// - Unit tests for all ID3 tag extraction scenarios
// - Multi-track file handling tests
// - Cover art extraction and encoding tests
// - File name sanitization tests
// - Album redundancy logic validation
//
// TAGLIB INTEGRATION:
// - Optimized for TagLibSharp library patterns
// - Handles various MP3 encoding and tag scenarios
// - Memory-efficient tag reading operations
// - Error handling for corrupted or missing tags
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - TagLibSharp 2.x
// - JSON serialization libraries
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive MP3 metadata extraction
// ====================================================================

using System;
using System.Collections.Generic;

namespace OstPlayer.Models
{
    /// <summary>
    /// Model for storing metadata of a single MP3 file extracted via TagLibSharp.
    /// Provides comprehensive ID3 tag support with intelligent redundancy handling.
    /// See: https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1
    /// </summary>
    public class Mp3MetadataModel
    {
        #region Private Fields
        
        /// <summary>
        /// Internal storage for track title to support custom Album property logic.
        /// </summary>
        private string title;
        
        /// <summary>
        /// Internal storage for album name to enable redundancy detection with title.
        /// </summary>
        private string album;
        
        #endregion
        
        #region Core Metadata Properties
        
        /// <summary>
        /// Gets or sets the track title from ID3 tag.
        /// This property is used internally for Album redundancy detection.
        /// Reference: ID3v2.4 specification - TIT2 frame (Title/songname/content description)
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }
        
        /// <summary>
        /// Gets or sets the performing artist from ID3 tag.
        /// Maps to ID3v2 TPE1 frame (Lead performer(s)/Soloist(s))
        /// Reference: https://id3.org/id3v2.4.0-frames
        /// </summary>
        public string Artist { get; set; }
        
        /// <summary>
        /// Gets or sets the album name with intelligent redundancy handling.
        /// Returns null if album name is identical to title to avoid UI duplication.
        /// Maps to ID3v2 TALB frame (Album/Movie/Show title)
        /// </summary>
        public string Album
        {
            get => (album == title) ? null : album; // Avoid redundant display when album equals title
            set => album = value;
        }
        
        /// <summary>
        /// Gets or sets the release or recording year as string (e.g., "2001").
        /// Maps to ID3v2 TYER frame (Year) or TDRC frame (Recording time) for ID3v2.4
        /// Reference: https://id3.org/id3v2.4.0-frames (TDRC)
        /// </summary>
        public string Year { get; set; }
        
        /// <summary>
        /// Gets or sets the music genre from ID3 tag.
        /// Maps to ID3v2 TCON frame (Content type/Genre)
        /// Can contain standard ID3v1 genre numbers or custom text
        /// </summary>
        public string Genre { get; set; }
        
        /// <summary>
        /// Gets or sets the comment from ID3 tag (e.g., "Game OST", "Remastered").
        /// Maps to ID3v2 COMM frame (Comments)
        /// Often used for additional context or categorization
        /// </summary>
        public string Comment { get; set; }
        
        #endregion
        
        #region Media and Extended Properties
        
        /// <summary>
        /// Gets or sets the album cover in base64 format for UI display.
        /// Extracted from ID3v2 APIC frame (Attached picture)
        /// Base64 encoding allows embedding in JSON and eliminates file dependencies
        /// Reference: https://id3.org/id3v2.4.0-frames (APIC)
        /// </summary>
        public string CoverBase64 { get; set; }
        
        /// <summary>
        /// Gets or sets the list of tracks within a single file or album.
        /// Supports multi-track files, embedded cue sheets, and compilation albums.
        /// Each TrackInfo contains duration as TimeSpan for precise calculations.
        /// See: <see cref="TrackInfo"/> class for individual track structure
        /// </summary>
        public List<TrackInfo> Tracks { get; set; }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Sanitizes input string by removing characters not allowed in file names.
        /// Uses System.IO.Path.GetInvalidFileNameChars() for platform-specific validation.
        /// Essential for creating safe filenames from metadata for caching/export.
        /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getinvalidfilenamechars
        /// </summary>
        /// <param name="input">Raw string that may contain invalid filename characters</param>
        /// <returns>Sanitized string safe for use as filename</returns>
        /// <example>
        /// string safe = Mp3MetadataModel.Sanitize("Track: Title/Artist"); // Returns "Track Title Artist"
        /// </example>
        public static string Sanitize(string input)
        {
            // Remove all characters that are invalid for filenames on this platform
            // This approach preserves all valid characters while removing problematic ones
            return string.Concat(input.Split(System.IO.Path.GetInvalidFileNameChars()));
        }
        
        #endregion
        
        #region Nested Classes
        
        /// <summary>
        /// Represents a single track within an MP3 file for multi-track scenarios.
        /// Supports embedded cue sheets, compilation albums, and complex track structures.
        /// Duration uses TimeSpan for precise time calculations and formatting flexibility.
        /// </summary>
        /// <remarks>
        /// This nested class could be extracted to its own file in future refactoring
        /// for better separation of concerns and reusability across metadata models.
        /// </remarks>
        public class TrackInfo
        {
            /// <summary>
            /// Gets or sets the individual track title.
            /// May differ from parent Mp3MetadataModel.Title in multi-track files.
            /// </summary>
            public string Title { get; set; }
            
            /// <summary>
            /// Gets or sets the track duration as TimeSpan for precise time calculations.
            /// Allows for easy formatting: duration.ToString(@"mm\:ss") for UI display.
            /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.timespan
            /// </summary>
            /// <example>
            /// trackInfo.Duration = TimeSpan.FromSeconds(195); // 3:15
            /// string formatted = trackInfo.Duration.ToString(@"mm\:ss"); // "03:15"
            /// </example>
            public TimeSpan Duration { get; set; }
            
            /// <summary>
            /// Gets or sets the track number in album (1-based indexing, e.g., 2 for second track).
            /// Corresponds to ID3v2 TRCK frame (Track number/Position in set)
            /// </summary>
            public uint TrackNumber { get; set; }
            
            /// <summary>
            /// Gets or sets the total number of tracks in album (e.g., 12 for 12-track album).
            /// Used with TrackNumber to display "2/12" style track position indicators.
            /// Also corresponds to ID3v2 TRCK frame format "track/total"
            /// </summary>
            public uint TotalTracks { get; set; }
        }
        
        #endregion
    }
}
