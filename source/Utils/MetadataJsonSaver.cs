// ====================================================================
// FILE: MetadataJsonSaver.cs
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
// Static utility class for saving metadata from multiple sources (MP3, MusicBrainz, Discogs)
// as JSON files in the plugin's ExtensionsData folder structure. Provides persistent caching
// and storage capabilities for metadata management within Playnite's plugin data organization.
//
// FEATURES:
// - Multi-source metadata persistence to JSON files
// - Plugin-specific ExtensionsData folder integration
// - Automatic directory creation and management
// - Game-specific metadata organization by database ID
// - Thread-safe static methods for concurrent access
// - Playnite API integration for path resolution
//
// DEPENDENCIES:
// - Newtonsoft.Json (JSON serialization)
// - System.IO (file system operations)
// - Playnite.SDK (IPlayniteAPI for path resolution)
// - OstPlayer.Models (metadata model classes)
//
// UTILITY FUNCTIONS:
// - SaveMp3MetadataToJson: Persist MP3 tag metadata
// - SaveDiscogsMetadataToJson: Persist Discogs release information
// - SaveMusicBrainzMetadataToJson: Persist MusicBrainz release data
// - GetMusicFilesDirectory: Resolve plugin ExtensionsData path for game
// - EnsureDirectoryExists: Create directory structure if needed
//
// FILE ORGANIZATION:
// ExtensionsData/
//   OstPlayer_f3b0c108-5212-4b34-a303-47e859b31a92/
//     Metadata/
//       {GameId}/
//         {GameId}_mp3.json
//         {GameId}_discogs.json
//         {GameId}_musicbrainz.json
//
// PERFORMANCE NOTES:
// - Static methods eliminate instance allocation overhead
// - Efficient JSON serialization with indented formatting
// - Minimal file I/O operations for metadata size
// - Directory existence caching through file system
//
// LIMITATIONS:
// - Basic error handling (directory existence only)
// - No validation of JSON content or schema
// - Limited to predefined metadata models
// - No atomic operations for multiple file writes
//
// FUTURE REFACTORING:
// FUTURE: Add comprehensive error handling for file I/O failures
// FUTURE: Implement JSON schema validation for data integrity
// FUTURE: Add atomic operations for multiple metadata saves
// FUTURE: Extract to configurable storage provider interface
// FUTURE: Add metadata versioning and migration support
// FUTURE: Implement async file operations for better performance
// FUTURE: Add file locking for concurrent access scenarios
// FUTURE: Extract path resolution to separate utility
// CONSIDER: Database storage as alternative to file-based approach
// CONSIDER: Adding metadata backup and versioning
// IDEA: Metadata compression for large collections
// IDEA: Cloud storage integration for cross-device sync
//
// TESTING:
// - Unit tests for JSON serialization with various metadata types
// - File system operation tests with directory creation
// - Integration tests with Playnite API path resolution
// - Concurrent access scenarios validation
//
// PLAYNITE INTEGRATION:
// - Uses IPlayniteAPI for consistent path resolution
// - Follows plugin ExtensionsData folder conventions
// - Compatible with Playnite's plugin data structure
// - Integrates with plugin-specific data organization
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Newtonsoft.Json 13.x
// - Playnite SDK 6.x
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with ExtraMetadata folder persistence
// ====================================================================

using System;
using System.IO;
using Newtonsoft.Json;
using OstPlayer.Models;
using Playnite.SDK;

namespace OstPlayer.Utils
{
    /// <summary>
    /// Static utility for saving metadata as JSON files in the plugin's ExtensionsData folder.
    /// ARCHITECTURE: Persistence layer for metadata caching and cross-session storage
    /// THREAD SAFETY: All static methods are thread-safe (no shared mutable state)
    /// FILE ORGANIZATION: Follows Playnite plugin data conventions for consistency
    /// </summary>
    public static class MetadataJsonSaver
    {
        #region Public API Methods - Multi-Source Metadata Persistence

        /// <summary>
        /// Saves MP3 metadata to a JSON file in the plugin's ExtensionsData folder.
        /// PURPOSE: Cache ID3 tag data to avoid repeated file reads
        /// FILENAME PATTERN: {GameId}_mp3.json for easy identification
        /// SERIALIZATION: Indented JSON for human readability and debugging
        /// </summary>
        /// <param name="databaseId">Unique game identifier from Playnite database</param>
        /// <param name="metadata">MP3 metadata model to serialize</param>
        /// <param name="playniteApi">Playnite API for path resolution</param>
        public static void SaveMp3MetadataToJson(
            Guid databaseId,
            Mp3MetadataModel metadata,
            IPlayniteAPI playniteApi
        )
        {
            // PATH RESOLUTION: Get standardized music files directory
            var metadataDir = GetMusicFilesDirectory(databaseId, playniteApi);

            // DIRECTORY PREPARATION: Ensure target directory exists
            // CREATION: Creates full directory tree if any part is missing
            EnsureDirectoryExists(metadataDir);

            // FILE NAMING: Consistent pattern for metadata source identification
            // PATTERN: {GameId}_mp3.json distinguishes from other metadata sources
            var fileName = $"{databaseId}_mp3.json";
            var filePath = Path.Combine(metadataDir, fileName);

            // SERIALIZATION: Convert metadata model to formatted JSON
            // FORMATTING: Indented for readability, debugging, and version control
            // ENCODING: Default UTF-8 encoding for international character support
            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Saves Discogs metadata to a JSON file in the plugin's ExtensionsData folder.
        /// PURPOSE: Cache external API data to reduce network requests and improve performance
        /// API RATE LIMITING: Reduces load on Discogs API by caching responses
        /// OFFLINE ACCESS: Enables metadata access when network is unavailable
        /// </summary>
        /// <param name="databaseId">Unique game identifier from Playnite database</param>
        /// <param name="metadata">Discogs metadata model to serialize</param>
        /// <param name="playniteApi">Playnite API for path resolution</param>
        public static void SaveDiscogsMetadataToJson(
            Guid databaseId,
            DiscogsMetadataModel metadata,
            IPlayniteAPI playniteApi
        )
        {
            // IDENTICAL PATTERN: Same directory and serialization logic as MP3 metadata
            var metadataDir = GetMusicFilesDirectory(databaseId, playniteApi);
            EnsureDirectoryExists(metadataDir);

            // SOURCE IDENTIFICATION: _discogs suffix distinguishes from other metadata sources
            var fileName = $"{databaseId}_discogs.json";
            var filePath = Path.Combine(metadataDir, fileName);

            // JSON PERSISTENCE: Store external API response for future access
            // BENEFIT: Dramatically improves application startup time for cached games
            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Saves MusicBrainz metadata to a JSON file in the plugin's ExtensionsData folder.
        /// PURPOSE: Cache MusicBrainz API responses for comprehensive music database integration
        /// FUTURE PROOFING: Prepared for MusicBrainz integration when implemented
        /// METADATA COMPLETENESS: Provides additional data source for comprehensive metadata
        /// </summary>
        /// <param name="databaseId">Unique game identifier from Playnite database</param>
        /// <param name="metadata">MusicBrainz metadata model to serialize</param>
        /// <param name="playniteApi">Playnite API for path resolution</param>
        public static void SaveMusicBrainzMetadataToJson(
            Guid databaseId,
            MusicBrainzMetadataModel metadata,
            IPlayniteAPI playniteApi
        )
        {
            // CONSISTENT IMPLEMENTATION: Same pattern as other metadata sources
            var metadataDir = GetMusicFilesDirectory(databaseId, playniteApi);
            EnsureDirectoryExists(metadataDir);

            // MUSICBRAINZ IDENTIFICATION: Unique filename for MusicBrainz data
            var fileName = $"{databaseId}_musicbrainz.json";
            var filePath = Path.Combine(metadataDir, fileName);

            // FUTURE-READY: Implementation ready for MusicBrainz integration
            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        #endregion

        #region Private Helper Methods - Directory and Path Management

        /// <summary>
        /// Returns the path to the Metadata directory for the given game by DatabaseId
        /// PATH STRUCTURE: {ExtensionsDataPath}/OstPlayer_{PluginId}/Metadata/{GameId}/
        /// PLAYNITE INTEGRATION: Uses official Playnite plugin data conventions
        /// CONSISTENCY: Avoids conflicts with other plugins using ExtraMetadata
        /// </summary>
        /// <param name="databaseId">Unique game identifier (GUID)</param>
        /// <param name="api">Playnite API for path resolution</param>
        /// <returns>Full path to the game's music files directory</returns>
        private static string GetMusicFilesDirectory(Guid databaseId, IPlayniteAPI api)
        {
            // PATH CONSTRUCTION: Build plugin-specific path using ExtensionsData conventions
            // COMPONENTS:
            // - ExtensionsDataPath: Plugin data directory
            // - OstPlayer_f3b0c108-5212-4b34-a303-47e859b31a92: Plugin ID folder
            // - Metadata: Plugin-specific metadata container
            // - {databaseId}: Unique folder per game (prevents conflicts)
            return Path.Combine(
                api.Paths.ExtensionsDataPath, // Base plugin data directory
                "OstPlayer_f3b0c108-5212-4b34-a303-47e859b31a92", // Plugin ID folder
                "Metadata", // OstPlayer metadata container
                databaseId.ToString() // Unique game directory (GUID string)
            );
        }

        /// <summary>
        /// Ensures the directory exists, creates it if it does not
        /// CREATION STRATEGY: Creates entire directory tree if any part is missing
        /// ERROR HANDLING: Directory.CreateDirectory() is idempotent (safe to call multiple times)
        /// PERMISSIONS: Uses current user permissions for directory creation
        /// </summary>
        /// <param name="path">Full directory path to create</param>
        private static void EnsureDirectoryExists(string path)
        {
            // EXISTENCE CHECK: Directory.CreateDirectory() handles existence check internally
            // TREE CREATION: Creates parent directories automatically if they don't exist
            // IDEMPOTENT: Safe to call multiple times - no error if directory already exists
            // EXCEPTION HANDLING: Exceptions bubble up to caller for appropriate handling
            if (!Directory.Exists(path))
            {
                // ATOMIC OPERATION: Directory creation is atomic at the individual directory level
                // RACE CONDITION: Multiple threads could create the same directory - that's okay
                // PERMISSIONS: Uses current process permissions and security context
                Directory.CreateDirectory(path);
            }
        }

        #endregion

        #region Design Notes and Future Considerations

        /*
        CURRENT DESIGN DECISIONS:

        1. STATIC METHODS:
           - No instance state to manage
           - Simple utility-style API
           - Thread-safe by design (no shared mutable state)
           - Easy to use from any calling context

        2. FILENAME CONVENTIONS:
           - {GameId}_{source}.json pattern
           - Source suffix identifies metadata origin
           - GUID prevents naming conflicts between games
           - JSON extension for content identification

        3. DIRECTORY STRUCTURE:
           - Follows Playnite ExtraMetadata conventions
           - Game-isolated storage prevents cross-contamination
           - Centralized in Music Files folder with audio files
           - Consistent with existing OstPlayer architecture

        4. ERROR HANDLING STRATEGY:
           - Exceptions bubble up to callers
           - Callers can decide on error handling approach
           - No silent failures or data loss
           - Consistent with .NET Framework patterns

        FUTURE ENHANCEMENT OPPORTUNITIES:

        1. TRANSACTIONAL OPERATIONS:
           public static void SaveAllMetadata(Guid gameId, Mp3MetadataModel mp3, DiscogsMetadataModel discogs, ...)
           {
               // Atomic save of all metadata types
               // Rollback on partial failure
           }

        2. VERSIONING SUPPORT:
           - Add metadata version numbers to JSON
           - Handle schema migration automatically
           - Backward compatibility with older metadata files

        3. COMPRESSION:
           - Optional gzip compression for large metadata
           - Configurable compression levels
           - Automatic compression for large tracklists

        4. VALIDATION:
           - JSON schema validation before save
           - Metadata completeness checking
           - Data integrity verification

        5. ASYNC OPERATIONS:
           public static async Task SaveDiscogsMetadataToJsonAsync(...)
           {
               // Non-blocking file I/O for better UI responsiveness
               // Useful for large metadata objects or slow storage
           }

        6. BACKUP AND RECOVERY:
           - Automatic backup of previous metadata versions
           - Recovery from corrupted JSON files
           - Metadata export/import functionality

        7. CACHING LAYER:
           - In-memory cache of recently saved/loaded metadata
           - Reduce file I/O for frequently accessed games
           - Cache invalidation strategies

        8. CONFIGURATION:
           - Configurable metadata storage location
           - Optional metadata sources (disable certain APIs)
           - User preferences for metadata priority
        */

        #endregion
    }
}
