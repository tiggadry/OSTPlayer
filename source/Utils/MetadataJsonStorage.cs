// ====================================================================
// FILE: MetadataJsonStorage.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils
// LOCATION: Utils/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Static utility class for saving, loading, and merging metadata from multiple sources
// (MP3, MusicBrainz, Discogs) as JSON files. Provides unified metadata access and
// caching capabilities for album-level metadata management.
//
// FEATURES:
// - Multi-source metadata serialization to JSON
// - Unified album metadata loading and merging
// - Thread-safe static methods for concurrent access
// - AlbumMetadataModel creation for UI binding
// - File path management for metadata storage
// - Error-tolerant loading with fallback mechanisms
//
// DEPENDENCIES:
// - Newtonsoft.Json (JSON serialization and deserialization)
// - System.IO (file system operations)
// - System.Windows.Media.Imaging (BitmapImage handling)
// - OstPlayer.Models (metadata model classes)
//
// UTILITY FUNCTIONS:
// - SaveMp3MetadataToJson: Persist MP3 tag metadata
// - SaveMusicBrainzMetadataToJson: Persist MusicBrainz release data
// - SaveDiscogsMetadataToJson: Persist Discogs release information
// - LoadAndMergeAlbumMetadata: Aggregate metadata from all sources
// - SaveJsonToFile: Generic JSON serialization helper
//
// STORAGE STRATEGY:
// - File naming pattern: {databaseId}_{source}.json
// - Folder-based organization by game database ID
// - JSON indented formatting for readability
// - Source-specific file separation for modularity
//
// PERFORMANCE NOTES:
// - Static methods eliminate instance allocation overhead
// - Efficient JSON serialization with minimal memory usage
// - File I/O operations optimized for metadata size
// - Lazy loading approach for metadata merging
//
// LIMITATIONS:
// - Basic error handling (null checks only)
// - No validation of JSON schema or content
// - Limited to predefined metadata models
// - No support for partial metadata updates
//
// FUTURE REFACTORING:
// TODO: Add comprehensive error handling for file I/O operations
// TODO: Implement JSON schema validation for data integrity
// TODO: Add support for partial metadata updates and merging
// TODO: Extract to configurable storage provider interface
// TODO: Add metadata versioning and migration support
// TODO: Implement async file operations for better performance
// TODO: Add metadata compression for large collections
// TODO: Extract unified metadata merging to separate service
// CONSIDER: Database storage as alternative to file-based JSON
// CONSIDER: Adding metadata backup and restore capabilities
// IDEA: Real-time metadata synchronization across instances
// IDEA: Cloud storage integration for metadata sharing
//
// TESTING:
// - Unit tests for JSON serialization/deserialization
// - Integration tests with actual metadata models
// - File system operation tests with various scenarios
// - Metadata merging logic validation tests
//
// MERGING STRATEGY:
// - AlbumMetadataModel provides unified structure
// - Source priority for conflicting data
// - Null-safe property assignment
// - BitmapImage handling for cover artwork
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Newtonsoft.Json 13.x
// - Windows file system
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with multi-source metadata storage
// ====================================================================

using System;
using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using OstPlayer.Models;

namespace OstPlayer.Utils
{
    /// <summary>
    /// Static utility for saving and loading album metadata as JSON files.
    /// ARCHITECTURE: Multi-source metadata aggregation with priority-based merging
    /// THREAD SAFETY: All static methods are thread-safe (no shared mutable state)
    /// STORAGE STRATEGY: File-based JSON storage with source-specific organization
    /// </summary>
    public static class MetadataJsonStorage
    {
        #region Save Methods - Source-Specific JSON Persistence

        /// <summary>
        /// Saves MP3 metadata to a JSON file in the specified folder.
        /// PURPOSE: Cache ID3 tag data extracted from audio files
        /// FILENAME: {databaseId}_mp3.json for source identification
        /// CONTENT: Base64-encoded cover art, track lists, ID3 tag information
        /// </summary>
        /// <param name="databaseId">Unique game identifier for filename generation</param>
        /// <param name="folderPath">Target directory for JSON file storage</param>
        /// <param name="metadata">MP3 metadata model with ID3 tag data</param>
        public static void SaveMp3MetadataToJson(
            Guid databaseId,
            string folderPath,
            Mp3MetadataModel metadata
        )
        {
            // FILE PATH CONSTRUCTION: Build path with source-specific naming pattern
            // PATTERN: {GameId}_mp3.json identifies source and prevents conflicts
            string filePath = Path.Combine(folderPath, $"{databaseId}_mp3.json");
            SaveJsonToFile(filePath, metadata);
        }

        /// <summary>
        /// Saves MusicBrainz metadata to a JSON file in the specified folder.
        /// PURPOSE: Cache MusicBrainz API responses for offline access
        /// CONTENT: Release information, MusicBrainz UUIDs, recording details
        /// FUTURE: Ready for MusicBrainz integration when implemented
        /// </summary>
        /// <param name="databaseId">Unique game identifier for filename generation</param>
        /// <param name="folderPath">Target directory for JSON file storage</param>
        /// <param name="metadata">MusicBrainz metadata model with API response data</param>
        public static void SaveMusicBrainzMetadataToJson(
            Guid databaseId,
            string folderPath,
            MusicBrainzMetadataModel metadata
        )
        {
            // MUSICBRAINZ IDENTIFICATION: Unique filename for MusicBrainz API data
            string filePath = Path.Combine(folderPath, $"{databaseId}_musicbrainz.json");
            SaveJsonToFile(filePath, metadata);
        }

        /// <summary>
        /// Saves Discogs metadata to a JSON file in the specified folder.
        /// PURPOSE: Cache Discogs API responses to reduce network requests
        /// CONTENT: Release information, tracklist, label data, format details
        /// PERFORMANCE: Dramatically improves load times for cached games
        /// </summary>
        /// <param name="databaseId">Unique game identifier for filename generation</param>
        /// <param name="folderPath">Target directory for JSON file storage</param>
        /// <param name="metadata">Discogs metadata model with API response data</param>
        public static void SaveDiscogsMetadataToJson(
            Guid databaseId,
            string folderPath,
            DiscogsMetadataModel metadata
        )
        {
            // DISCOGS IDENTIFICATION: Source-specific filename for API cache
            string filePath = Path.Combine(folderPath, $"{databaseId}_discogs.json");
            SaveJsonToFile(filePath, metadata);
        }

        /// <summary>
        /// Generic JSON serialization helper for consistent formatting
        /// SERIALIZATION: Indented JSON for human readability and debugging
        /// ENCODING: UTF-8 encoding for international character support
        /// ERROR HANDLING: Exceptions bubble up to callers for proper handling
        /// </summary>
        /// <typeparam name="T">Type of metadata model to serialize</typeparam>
        /// <param name="filePath">Full path to target JSON file</param>
        /// <param name="metadata">Metadata model instance to serialize</param>
        private static void SaveJsonToFile<T>(string filePath, T metadata)
        {
            // JSON SERIALIZATION: Convert metadata to formatted JSON string
            // FORMATTING: Indented format for readability and version control
            string json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            
            // FILE PERSISTENCE: Write JSON to specified file path
            // ENCODING: Default UTF-8 encoding handles international characters
            File.WriteAllText(filePath, json);
        }

        #endregion

        #region Load and Merge - Multi-Source Metadata Aggregation

        /// <summary>
        /// Loads and merges album metadata from all available sources (MP3, MusicBrainz, Discogs).
        /// PRIORITY: MP3 > MusicBrainz > Discogs for conflicting data
        /// STRATEGY: Priority-based field merging with null-safe operations
        /// RESULT: Unified AlbumMetadataModel with best available data from all sources
        /// </summary>
        /// <param name="databaseId">Unique game identifier for file location</param>
        /// <param name="folderPath">Directory containing JSON metadata files</param>
        /// <returns>Merged album metadata model with data from all available sources</returns>
        public static AlbumMetadataModel LoadAndMergeAlbumMetadata(
            Guid databaseId,
            string folderPath
        )
        {
            // RESULT INITIALIZATION: Create target model for merged metadata
            var result = new AlbumMetadataModel();

            #region Local Helper Function - JSON Loading and Deserialization

            // Helper local function for loading JSON and deserializing to given type
            // ENCAPSULATION: Reduces code duplication for multiple source loading
            // ERROR HANDLING: Returns null for missing files or deserialization failures
            T LoadJson<T>(string fileName) where T : class
            {
                // PATH CONSTRUCTION: Build full path to JSON file
                string path = Path.Combine(folderPath, fileName);
                
                // EXISTENCE CHECK: Return null if file doesn't exist
                if (!File.Exists(path))
                    return null;
                    
                try
                {
                    // FILE READING: Load JSON content from file
                    string json = File.ReadAllText(path);
                    
                    // DESERIALIZATION: Convert JSON back to typed object
                    // NULL HANDLING: JsonConvert handles null/empty JSON gracefully
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    // ERROR HANDLING: Return null for any deserialization failure
                    // REASONS: Corrupted JSON, schema changes, file access issues
                    // PHILOSOPHY: Graceful degradation better than complete failure
                    return null;
                }
            }

            #endregion

            #region Source Data Loading - Multi-Source File Reading

            // LOAD SOURCE DATA: Read all available metadata source files
            // FILE NAMING: Uses corrected string interpolation for proper filename construction
            // BUG FIX: Previously used literal "{databaseId}" instead of interpolated value
            var mp3Metadata = LoadJson<Mp3MetadataModel>($"{databaseId}_mp3.json");
            var mbMetadata = LoadJson<MusicBrainzMetadataModel>($"{databaseId}_musicbrainz.json");
            var discogsMetadata = LoadJson<DiscogsMetadataModel>($"{databaseId}_discogs.json");

            #endregion

            #region Priority-Based Merging - MP3 Metadata (Highest Priority)

            // MP3 METADATA PROCESSING: Highest priority due to local file authority
            // REASONING: Local files are authoritative for track-specific information
            if (mp3Metadata != null)
            {
                // BASIC FIELDS: Direct assignment from MP3 metadata
                result.Title = mp3Metadata.Title;
                result.Artist = mp3Metadata.Artist;
                result.Album = mp3Metadata.Album;
                
                // NOTE: Mp3MetadataModel typically doesn't have Released, Genres, Styles
                // DESIGN: ID3 tags focus on track information rather than release metadata
                result.Comment = mp3Metadata.Comment;
                
                // COVER ART: Convert from base64 encoding to BitmapImage
                // PRIORITY: Local cover art preferred over external URLs
                result.Cover = LoadBitmapImageFromBase64(mp3Metadata.CoverBase64);
                
                // TRACKLIST: Extract track titles from MP3 track collection
                // STRUCTURE: Convert TrackInfo list to simple string list for UI
                if (mp3Metadata.Tracks != null)
                    result.Tracklist = mp3Metadata.Tracks.ConvertAll(t => t.Title);
            }

            #endregion

            #region Priority-Based Merging - MusicBrainz Metadata (Medium Priority)

            // MUSICBRAINZ METADATA PROCESSING: Fill gaps not covered by MP3 metadata
            // REASONING: MusicBrainz provides authoritative release-level information
            if (mbMetadata != null)
            {
                // CONDITIONAL ASSIGNMENT: Only set if not already populated by MP3
                // PATTERN: Preserve higher priority data while filling gaps
                if (string.IsNullOrEmpty(result.Title))
                    result.Title = mbMetadata.Title;
                if (string.IsNullOrEmpty(result.Artist))
                    result.Artist = mbMetadata.Artist;
                if (string.IsNullOrEmpty(result.Album))
                    result.Album = mbMetadata.Album;
                    
                // RELEASE DATE: MusicBrainz specializes in release information
                // FIELD MAPPING: ReleaseDate maps to Released field
                if (string.IsNullOrEmpty(result.Released) && mbMetadata.ReleaseDate != null)
                    result.Released = mbMetadata.ReleaseDate;
                    
                if (string.IsNullOrEmpty(result.Comment))
                    result.Comment = mbMetadata.Comment;
                    
                // COVER ART FALLBACK: Use URL-based image if no local cover available
                // PRIORITY: Local base64 images preferred over remote URLs
                if (result.Cover == null && !string.IsNullOrEmpty(mbMetadata.CoverUrl))
                    result.Cover = LoadBitmapImageFromUrl(mbMetadata.CoverUrl);
            }

            #endregion

            #region Priority-Based Merging - Discogs Metadata (Lowest Priority)

            // DISCOGS METADATA PROCESSING: Final fallback for missing information
            // REASONING: Discogs provides comprehensive release data but lowest priority
            if (discogsMetadata != null)
            {
                // BASIC FIELDS: Fill remaining gaps with Discogs data
                if (string.IsNullOrEmpty(result.Title))
                    result.Title = discogsMetadata.Title;
                if (string.IsNullOrEmpty(result.Artist))
                    result.Artist = discogsMetadata.Artist;
                if (string.IsNullOrEmpty(result.Album))
                    result.Album = discogsMetadata.Album;
                if (string.IsNullOrEmpty(result.Released))
                    result.Released = discogsMetadata.Released;
                    
                // GENRE AND STYLE: Discogs specializes in detailed categorization
                // COLLECTION CHECK: Only assign if current collections are empty
                if ((result.Genres == null || result.Genres.Count == 0) && discogsMetadata.Genres != null)
                    result.Genres = discogsMetadata.Genres;
                if ((result.Styles == null || result.Styles.Count == 0) && discogsMetadata.Styles != null)
                    result.Styles = discogsMetadata.Styles;
                    
                if (string.IsNullOrEmpty(result.Comment))
                    result.Comment = discogsMetadata.Comment;
                    
                // COVER ART: Final fallback to Discogs cover URL
                if (result.Cover == null && !string.IsNullOrEmpty(discogsMetadata.CoverUrl))
                    result.Cover = LoadBitmapImageFromUrl(discogsMetadata.CoverUrl);
                
                // DISCOGS-SPECIFIC FIELDS: Unique to Discogs metadata
                if (string.IsNullOrEmpty(result.Country))
                    result.Country = discogsMetadata.Country;
                if (string.IsNullOrEmpty(result.DiscogsUrl))
                    result.DiscogsUrl = discogsMetadata.DiscogsUrl;
                
                // TRACKLIST FALLBACK: Use Discogs tracklist if no other source available
                // CONDITION: Only if no existing tracklist and Discogs has valid data
                if ((result.Tracklist == null || result.Tracklist.Count == 0)
                    && discogsMetadata.Tracklist != null
                    && discogsMetadata.Tracklist.Count > 0)
                {
                    // TRACK TITLE EXTRACTION: Convert DiscogsTrack objects to simple string list
                    result.Tracklist = discogsMetadata.Tracklist.ConvertAll(t => t.Title);
                }
            }

            #endregion

            return result;
        }

        #endregion

        #region Image Loading Helpers - BitmapImage Creation from Various Sources

        /// <summary>
        /// Helper for loading image from base64 string
        /// SOURCE: Embedded cover art from MP3 ID3 tags
        /// MEMORY MANAGEMENT: Proper disposal pattern with using statement
        /// THREAD SAFETY: Freeze() makes BitmapImage thread-safe for cross-thread access
        /// </summary>
        /// <param name="base64">Base64-encoded image data from MP3 metadata</param>
        /// <returns>WPF-compatible BitmapImage or null if loading fails</returns>
        private static BitmapImage LoadBitmapImageFromBase64(string base64)
        {
            // INPUT VALIDATION: Check for null or empty base64 data
            if (string.IsNullOrEmpty(base64))
                return null;
                
            try
            {
                // BASE64 DECODING: Convert base64 string to byte array
                byte[] bytes = Convert.FromBase64String(base64);
                
                // MEMORY STREAM: Create stream from byte array for BitmapImage
                using (var ms = new MemoryStream(bytes))
                {
                    // BITMAPIMAGE CREATION: WPF-compatible image format
                    var img = new BitmapImage();
                    img.BeginInit();
                    
                    // CACHING STRATEGY: OnLoad loads image data immediately
                    // BENEFIT: Allows MemoryStream disposal while preserving image
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = ms;
                    img.EndInit();
                    
                    // THREAD SAFETY: Freeze makes object thread-safe and improves performance
                    img.Freeze();
                    return img;
                }
            }
            catch
            {
                // ERROR HANDLING: Return null for any failure
                // REASONS: Invalid base64, corrupted image data, memory issues
                // PHILOSOPHY: Graceful degradation - missing image better than crash
                return null;
            }
        }

        /// <summary>
        /// Helper for loading image from URL
        /// SOURCE: External cover art from MusicBrainz or Discogs APIs
        /// NETWORK DEPENDENCY: Requires internet connection for initial load
        /// CACHING: WPF automatically caches downloaded images
        /// </summary>
        /// <param name="url">HTTP/HTTPS URL to cover art image</param>
        /// <returns>WPF-compatible BitmapImage or null if loading fails</returns>
        private static BitmapImage LoadBitmapImageFromUrl(string url)
        {
            // INPUT VALIDATION: Check for null or empty URL
            if (string.IsNullOrEmpty(url))
                return null;
                
            try
            {
                // BITMAPIMAGE CREATION: Direct from URL
                var img = new BitmapImage();
                img.BeginInit();
                
                // URL ASSIGNMENT: Set source to provided URL
                img.UriSource = new Uri(url, UriKind.Absolute);
                
                // CACHING STRATEGY: OnLoad downloads and caches image immediately
                // BENEFIT: Subsequent access doesn't require network requests
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                
                // THREAD SAFETY: Freeze makes object thread-safe
                img.Freeze();
                return img;
            }
            catch
            {
                // ERROR HANDLING: Return null for any failure
                // REASONS: Network issues, invalid URL, unsupported format
                // PHILOSOPHY: Continue with missing image rather than crash
                return null;
            }
        }

        #endregion

        #region Design Patterns and Architecture Notes

        /*
        MERGING STRATEGY EXPLANATION:

        1. PRIORITY-BASED MERGING:
           - MP3 metadata: Highest priority (local, authoritative)
           - MusicBrainz: Medium priority (authoritative release data)
           - Discogs: Lowest priority (comprehensive but community-driven)

        2. FIELD-LEVEL PRECEDENCE:
           - Each field checked independently
           - Higher priority sources always win
           - Null/empty checks prevent overwriting valid data

        3. COLLECTION HANDLING:
           - Genres/Styles: Keep existing non-empty collections
           - Tracklist: First non-empty collection wins
           - Cover art: Local base64 preferred over URLs

        4. ERROR RESILIENCE:
           - Individual source failures don't break merging
           - Partial data better than no data
           - Graceful degradation throughout

        PERFORMANCE CHARACTERISTICS:

        1. FILE I/O:
           - Single read per source file
           - Lazy loading (only when method called)
           - No caching (each call re-reads files)

        2. MEMORY USAGE:
           - Temporary objects for each source
           - BitmapImage objects may be large
           - Garbage collection handles cleanup

        3. NETWORK DEPENDENCY:
           - URL-based images require network access
           - WPF handles caching automatically
           - Failures are graceful

        FUTURE ENHANCEMENT OPPORTUNITIES:

        1. CACHING LAYER:
           - In-memory cache for recently loaded metadata
           - File system watcher for cache invalidation
           - Configurable cache expiration

        2. ASYNC OPERATIONS:
           - Async file reading for large JSON files
           - Async image loading for URLs
           - Progress reporting for long operations

        3. VALIDATION:
           - JSON schema validation
           - Metadata completeness scoring
           - Conflict detection and resolution

        4. EXTENSIBILITY:
           - Plugin architecture for additional sources
           - Configurable merge priorities
           - Custom merge strategies per field
        */

        #endregion
    }
}
