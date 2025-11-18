// ====================================================================
// FILE: IGameService.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Services/Interfaces
// LOCATION: Services/Interfaces/
// VERSION: 2.0.0
// CREATED: 2025-08-08
// UPDATED: 2025-08-09
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Interface for game service providing contract for game data access, music file discovery, and game-related operations.
// Part of Phase 5 Dependency Injection implementation for clean architecture and testability.
//
// FEATURES:
// - Game library access and filtering
// - Music file discovery and validation
// - Game-specific metadata management
// - Async operations with cancellation
// - Progress reporting for batch operations
// - **PHASE 5**: Production-ready game service interface
// - **PHASE 5**: Comprehensive batch operations and statistics
//
// DEPENDENCIES:
// - System.Threading.Tasks (async operations)
// - System.Threading (cancellation tokens)
// - System.Collections.Generic (collections)
// - Playnite.SDK.Models (game entities)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle
// - Repository Pattern
// - Command Pattern (operations)
// - Observer Pattern (progress reporting)
//
// LIMITATIONS:
// - Playnite SDK dependency for game access
// - File system dependent operations
// - No network-based game library support
// - Basic progress reporting structure
//
// FUTURE REFACTORING:
// FUTURE: Add support for custom game library sources
// FUTURE: Implement advanced filtering and sorting options
// FUTURE: Add game music library synchronization
// FUTURE: Create game music organization features
// FUTURE: Add support for network-attached game libraries
// CONSIDER: Plugin architecture for game discovery
// CONSIDER: Advanced game metadata extraction
// IDEA: Machine learning for automatic game music categorization
// IDEA: Cloud-based game music library management
//
// TESTING:
// - Mock implementations for unit testing
// - Game library integration tests
// - File system operation tests
// - Progress reporting validation tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Playnite SDK integration
// - Cross-platform file system support
//
// CHANGELOG:
// 2025-08-09 v2.0.0 - Phase 5 DI Implementation completed: Production-ready game service interface, batch operations, statistics
// 2025-08-08 v1.0.0 - Initial interface for Phase 5 DI implementation
// ====================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace OstPlayer.Services.Interfaces {
    /// <summary>
    /// Information about game music files and metadata.
    /// </summary>
    public class GameMusicInfo
    {
        /// <summary>
        /// Gets or sets the game instance.
        /// </summary>
        public Game Game { get; set; }
        
        /// <summary>
        /// Gets or sets the music directory path.
        /// </summary>
        public string MusicDirectory { get; set; }
        
        /// <summary>
        /// Gets or sets the list of music files.
        /// </summary>
        public List<string> MusicFiles { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of tracks.
        /// </summary>
        public int TotalTracks { get; set; }
        
        /// <summary>
        /// Gets or sets the total size in bytes.
        /// </summary>
        public long TotalSizeBytes { get; set; }
        
        /// <summary>
        /// Gets or sets when the music was last scanned.
        /// </summary>
        public DateTime LastScanned { get; set; }
    }

    /// <summary>
    /// Interface for game service providing comprehensive game and music file operations.
    /// </summary>
    public interface IGameService {
        #region Game Discovery and Access

        /// <summary>
        /// Gets all games that contain music files.
        /// </summary>
        Task<List<Game>> GetGamesWithMusicAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets detailed music information for a specific game.
        /// </summary>
        Task<GameMusicInfo> GetGameMusicInfoAsync(Game game, CancellationToken cancellationToken = default);

        /// <summary>
        /// Filters games by name or other criteria.
        /// </summary>
        Task<List<Game>> FilterGamesAsync(string searchText, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds game by exact name match.
        /// </summary>
        Task<Game> FindGameByNameAsync(string gameName, CancellationToken cancellationToken = default);

        #endregion

        #region Music File Operations

        /// <summary>
        /// Gets music files for a specific game.
        /// </summary>
        Task<List<string>> GetMusicFilesAsync(Game game, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets music directory path for a game.
        /// </summary>
        Task<string> GetMusicDirectoryAsync(Game game, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates music file existence and accessibility.
        /// </summary>
        Task<bool> ValidateMusicFileAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Scans for new music files in game directory.
        /// </summary>
        Task<List<string>> ScanForMusicFilesAsync(Game game, IProgress<int> progress = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Batch Operations

        /// <summary>
        /// Scans all games for music files with progress reporting.
        /// </summary>
        Task<Dictionary<Game, List<string>>> ScanAllGamesForMusicAsync(IProgress<ScanProgress> progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates music files for multiple games.
        /// </summary>
        Task<Dictionary<Game, List<string>>> ValidateAllMusicFilesAsync(IProgress<ValidationProgress> progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets statistics about music library.
        /// </summary>
        Task<MusicLibraryStatistics> GetLibraryStatisticsAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Game Metadata

        /// <summary>
        /// Gets or creates music metadata directory for game.
        /// </summary>
        Task<string> EnsureMusicMetadataDirectoryAsync(Game game, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if game has cached metadata.
        /// </summary>
        Task<bool> HasCachedMetadataAsync(Game game, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans up orphaned metadata files.
        /// </summary>
        Task CleanupOrphanedMetadataAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Configuration and Health

        /// <summary>
        /// Configures game service with settings.
        /// </summary>
        void Configure(OstPlayerSettings settings);

        /// <summary>
        /// Performs health check on service components.
        /// </summary>
        Task<ServiceHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);

        #endregion
    }

    /// <summary>
    /// Progress information for scanning operations.
    /// </summary>
    public class ScanProgress
    {
        /// <summary>
        /// Gets or sets the total number of games to scan.
        /// </summary>
        public int TotalGames { get; set; }
        
        /// <summary>
        /// Gets or sets the number of games processed.
        /// </summary>
        public int ProcessedGames { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the current game being processed.
        /// </summary>
        public string CurrentGame { get; set; }
        
        /// <summary>
        /// Gets or sets the current status message.
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Gets or sets the number of music files found.
        /// </summary>
        public int FoundMusicFiles { get; set; }
    }

    /// <summary>
    /// Progress information for validation operations.
    /// </summary>
    public class ValidationProgress
    {
        /// <summary>
        /// Gets or sets the total number of files to validate.
        /// </summary>
        public int TotalFiles { get; set; }
        
        /// <summary>
        /// Gets or sets the number of files processed.
        /// </summary>
        public int ProcessedFiles { get; set; }
        
        /// <summary>
        /// Gets or sets the current file being validated.
        /// </summary>
        public string CurrentFile { get; set; }
        
        /// <summary>
        /// Gets or sets the current validation status.
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Gets or sets the number of valid files found.
        /// </summary>
        public int ValidFiles { get; set; }
        
        /// <summary>
        /// Gets or sets the number of invalid files found.
        /// </summary>
        public int InvalidFiles { get; set; }
    }

    /// <summary>
    /// Statistics about the music library.
    /// </summary>
    public class MusicLibraryStatistics
    {
        /// <summary>
        /// Gets or sets the total number of games.
        /// </summary>
        public int TotalGames { get; set; }
        
        /// <summary>
        /// Gets or sets the number of games with music.
        /// </summary>
        public int GamesWithMusic { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of music files.
        /// </summary>
        public int TotalMusicFiles { get; set; }
        
        /// <summary>
        /// Gets or sets the total size of all music files in bytes.
        /// </summary>
        public long TotalSizeBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the largest game by music size.
        /// </summary>
        public string LargestGame { get; set; }
        
        /// <summary>
        /// Gets or sets the file count of the largest game.
        /// </summary>
        public int LargestGameFileCount { get; set; }
        
        /// <summary>
        /// Gets or sets when the library was last scanned.
        /// </summary>
        public DateTime LastScanned { get; set; }
        
        /// <summary>
        /// Gets or sets the distribution of file types.
        /// </summary>
        public Dictionary<string, int> FileTypeDistribution { get; set; }
    }
}
