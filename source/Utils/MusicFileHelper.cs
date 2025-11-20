// ====================================================================
// FILE: MusicFileHelper.cs
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
// Static utility class for discovering and managing music files associated
// with games in Playnite. Handles path resolution and file enumeration
// for the plugin's ExtensionsData folder structure used by OstPlayer.
//
// FEATURES:
// - Game music path resolution
// - MP3 file discovery and enumeration
// - Alphabetical sorting of music files
// - Error-tolerant file operations
// - Plugin ExtensionsData folder structure support
//
// DEPENDENCIES:
// - Playnite.SDK (IPlayniteAPI, Game model)
// - System.IO (file system operations)
// - System.Linq (LINQ operations)
//
// UTILITY FUNCTIONS:
// - GetGameMusicPath: Resolves music folder path for specific game
// - GetGameMusicFiles: Enumerates MP3 files for a game
// - Alphabetical sorting by filename without extension
// - Defensive programming against missing directories
//
// FILE STRUCTURE:
// ExtensionsData/
//   OstPlayer_f3b0c108-5212-4b34-a303-47e859b31a92/
//     Metadata/
//       {GameId}/
//         *.mp3
//
// PERFORMANCE NOTES:
// - Efficient directory existence checks
// - Single-pass file enumeration
// - Minimal memory allocation
// - Fast filename-based sorting
//
// LIMITATIONS:
// - Hard-coded to MP3 format only
// - No recursive subdirectory scanning
// - No file validation beyond extension
// - Limited error reporting
//
// FUTURE REFACTORING:
// FUTURE: Add support for additional audio formats (FLAC, OGG, WAV)
// FUTURE: Implement recursive subdirectory scanning
// FUTURE: Add file integrity validation
// FUTURE: Implement async file operations for large directories
// FUTURE: Add progress reporting for long operations
// FUTURE: Extract path configuration to settings
// FUTURE: Add file metadata caching for performance
// FUTURE: Implement file watcher for real-time updates
// CONSIDER: Creating interface for testability
// CONSIDER: Adding configurable file filters
// IDEA: Support for multiple music folder locations
// IDEA: Integration with music library management tools
//
// TESTING:
// - Unit tests with mock file system
// - Integration tests with actual file structure
// - Performance tests with large music collections
// - Edge case testing (missing folders, empty directories)
//
// ERROR HANDLING:
// - Graceful handling of missing directories
// - Silent failure for inaccessible files
// - Empty list return for error cases
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Playnite SDK 6.x
// - Windows file system
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive file discovery
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace OstPlayer.Utils
{
    /// <summary>
    /// Static utility class for discovering and managing music files associated with games in Playnite.
    /// ARCHITECTURE: File system abstraction layer for game music discovery
    /// PERFORMANCE: Optimized for single-pass operations with minimal I/O
    /// EXTENSIBILITY: Designed for easy addition of new audio format support
    /// </summary>
    public static class MusicFileHelper
    {
        #region Public API Methods

        /// <summary>
        /// Gets the path to the music files for a specific game.
        /// FOLDER STRUCTURE: Uses Playnite's plugin ExtensionsData convention for organization
        /// PATH VALIDATION: Verifies directory existence before returning path
        /// RETURN STRATEGY: Returns null for non-existent paths (caller-friendly)
        /// </summary>
        /// <param name="api">Playnite API instance for path resolution</param>
        /// <param name="game">Game entity from Playnite database</param>
        /// <returns>Full path to music directory, or null if directory doesn't exist</returns>
        public static string GetGameMusicPath(IPlayniteAPI api, Game game)
        {
            // INPUT VALIDATION: Early exit for null game prevents downstream errors
            if (game == null)
                return null;

            // PATH CONSTRUCTION: Build path using original ExtraMetadata structure for MP3 files
            // STRUCTURE: {ConfigPath}/ExtraMetadata/games/{GameId}/Music Files/
            // REASON: MP3 files stay in original location, only JSON metadata moves to ExtensionsData
            var gameMusicPath = Path.Combine(
                api.Paths.ConfigurationPath,
                "ExtraMetadata", // Playnite's extra metadata folder
                "games", // Games subfolder
                game.Id.ToString(), // Unique game identifier (GUID)
                "Music Files" // Music files subfolder
            );

            // EXISTENCE VALIDATION: Only return path if directory actually exists
            // PHILOSOPHY: Null return is cleaner than empty list for non-existent locations
            // CALLER BENEFIT: Avoids need for additional Directory.Exists() calls
            return Directory.Exists(gameMusicPath) ? gameMusicPath : null;
        }

        /// <summary>
        /// Gets a list of MP3 files for a game.
        /// ALGORITHM: Path resolution -> File enumeration -> Alphabetical sorting
        /// ERROR HANDLING: Returns empty list rather than throwing exceptions
        /// SORTING: Filename-based for consistent user experience
        /// </summary>
        /// <param name="api">Playnite API instance for path resolution</param>
        /// <param name="game">Game entity from Playnite database</param>
        /// <returns>Sorted list of full file paths to MP3 files</returns>
        public static List<string> GetGameMusicFiles(IPlayniteAPI api, Game game)
        {
            // PATH RESOLUTION: Delegate to GetGameMusicPath for consistent logic
            // EARLY EXIT: If no music path exists, return empty list immediately
            var musicPath = GetGameMusicPath(api, game);
            if (string.IsNullOrEmpty(musicPath))
                return new List<string>(); // Consistent empty result

            try
            {
                // FILE ENUMERATION: Search for MP3 files in music directory
                // PATTERN: "*.mp3" - case sensitive on Linux, case insensitive on Windows
                // SCOPE: TopDirectoryOnly - doesn't search subdirectories for performance
                // CONSIDERATION: Could extend to support subdirectory scanning for organized collections
                var files = Directory.GetFiles(musicPath, "*.mp3", SearchOption.TopDirectoryOnly);

                // SORTING STRATEGY: Alphabetical by filename without extension
                // REASONING:
                // - More user-friendly than full path sorting
                // - Handles track numbering naturally ("01-Title", "02-Title")
                // - Consistent across different path lengths
                // ALTERNATIVE: Could implement natural sorting for better number handling
                return files
                    .OrderBy(f => Path.GetFileNameWithoutExtension(f)) // Remove .mp3 for cleaner sort
                    .ToList();
            }
            catch (Exception)
            {
                // EXCEPTION HANDLING: Comprehensive catch for various failure scenarios
                // POSSIBLE EXCEPTIONS:
                // - UnauthorizedAccessException: Insufficient permissions
                // - DirectoryNotFoundException: Directory deleted after existence check
                // - SecurityException: Security policy restrictions
                // - IOException: General I/O errors (network drives, etc.)
                // - PathTooLongException: Extremely long file paths

                // GRACEFUL DEGRADATION: Return empty list to maintain application stability
                // ALTERNATIVE: Could log specific exception types for debugging
                // BENEFIT: Caller doesn't need to handle exceptions, can focus on business logic
                return new List<string>();
            }
        }

        #endregion

        #region Future Enhancement Areas

        /*
        POTENTIAL IMPROVEMENTS (for future implementation):

        1. MULTI-FORMAT SUPPORT:
           public static List<string> GetGameAudioFiles(IPlayniteAPI api, Game game, params string[] extensions)
           {
               var patterns = extensions.Length > 0 ? extensions : new[] { "*.mp3", "*.flac", "*.ogg", "*.wav" };
               // Implementation would enumerate multiple patterns
           }

        2. RECURSIVE DIRECTORY SCANNING:
           public static List<string> GetGameMusicFilesRecursive(IPlayniteAPI api, Game game)
           {
               // SearchOption.AllDirectories for organized music collections
               // Could include folder structure in sort order
           }

        3. FILE VALIDATION:
           public static List<string> GetValidMusicFiles(IPlayniteAPI api, Game game)
           {
               // Validate file headers, check for corruption
               // Filter out broken or inaccessible files
           }

        4. METADATA INTEGRATION:
           public static List<FileInfo> GetMusicFileDetails(IPlayniteAPI api, Game game)
           {
               // Return file size, modification date, metadata preview
               // Useful for file management and duplicate detection
           }

        5. ASYNC OPERATIONS:
           public static async Task<List<string>> GetGameMusicFilesAsync(IPlayniteAPI api, Game game, CancellationToken cancellationToken)
           {
               // Non-blocking file enumeration for large directories
               // Progress reporting for long operations
           }

        6. CONFIGURABLE PATHS:
           - Support for custom music folder locations
           - Multiple music folder support per game
           - User-configurable folder structure

        7. CACHING LAYER:
           - Cache file lists to avoid repeated disk I/O
           - File system watcher for automatic cache invalidation
           - Memory-efficient caching for large collections

        8. NATURAL SORTING:
           - Handle track numbers properly ("2-Title" before "10-Title")
           - Support various numbering schemes
           - Configurable sort preferences
        */

        #endregion

        #region Performance Notes and Best Practices

        /*
        CURRENT PERFORMANCE CHARACTERISTICS:

        1. TIME COMPLEXITY:
           - Path resolution: O(1) - simple string operations
           - File enumeration: O(n) where n = number of files in directory
           - Sorting: O(n log n) for filename-based alphabetical sort
           - Overall: O(n log n) dominated by sorting operation

        2. SPACE COMPLEXITY:
           - O(n) for file list storage
           - Minimal additional memory for string operations
           - No intermediate collections (LINQ streams results)

        3. I/O OPERATIONS:
           - Single Directory.Exists() call for path validation
           - Single Directory.GetFiles() call for enumeration
           - No individual file access or metadata reading
           - Efficient for directories with reasonable file counts (<1000 files)

        4. OPTIMIZATION OPPORTUNITIES:
           - Parallel enumeration for very large directories
           - Streaming results for memory efficiency
           - Caching for frequently accessed games
           - Async enumeration for UI responsiveness

        5. BOTTLENECKS:
           - Network drives: I/O latency can be significant
           - Large directories: Memory usage and enumeration time
           - File system fragmentation: Affects enumeration performance
           - Antivirus software: May scan each file during enumeration

        USAGE RECOMMENDATIONS:
        - Call once and cache results for repeated access
        - Consider async alternatives for large music collections
        - Implement progress reporting for user feedback on slow operations
        - Add file count limits for very large directories
        */

        #endregion
    }
}
