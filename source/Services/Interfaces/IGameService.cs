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
// TODO: Add support for custom game library sources
// TODO: Implement advanced filtering and sorting options
// TODO: Add game music library synchronization
// TODO: Create game music organization features
// TODO: Add support for network-attached game libraries
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

namespace OstPlayer.Services.Interfaces
{
    /// <summary>
    /// Game music file information for UI display.
    /// </summary>
    public class GameMusicInfo
    {
        public Game Game { get; set; }
        public string MusicDirectory { get; set; }
        public List<string> MusicFiles { get; set; } = new List<string>();
        public int TotalTracks { get; set; }
        public long TotalSizeBytes { get; set; }
        public DateTime LastScanned { get; set; }
    }
    
    /// <summary>
    /// Interface for game service providing comprehensive game and music file operations.
    /// </summary>
    public interface IGameService
    {
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
        public int TotalGames { get; set; }
        public int ProcessedGames { get; set; }
        public Game CurrentGame { get; set; }
        public string Status { get; set; }
        public int FoundMusicFiles { get; set; }
    }
    
    /// <summary>
    /// Progress information for validation operations.
    /// </summary>
    public class ValidationProgress
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public string CurrentFile { get; set; }
        public string Status { get; set; }
        public int ValidFiles { get; set; }
        public int InvalidFiles { get; set; }
    }
    
    /// <summary>
    /// Music library statistics for monitoring.
    /// </summary>
    public class MusicLibraryStatistics
    {
        public int TotalGames { get; set; }
        public int GamesWithMusic { get; set; }
        public int TotalMusicFiles { get; set; }
        public long TotalSizeBytes { get; set; }
        public string LargestGame { get; set; }
        public int LargestGameFileCount { get; set; }
        public DateTime LastScanned { get; set; }
        public Dictionary<string, int> FileTypeDistribution { get; set; } = new Dictionary<string, int>();
    }
}