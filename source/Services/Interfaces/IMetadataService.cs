// ====================================================================
// FILE: IMetadataService.cs
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
// Interface for metadata service providing contract for metadata loading, caching, and external API integration.
// Part of Phase 5 Dependency Injection implementation for clean architecture and testability.
//
// FEATURES:
// - Async metadata loading with cancellation support
// - Multi-source metadata integration (MP3, Discogs, MusicBrainz)
// - Cache management with TTL and invalidation
// - Error handling with comprehensive logging
// - Progress reporting for long-running operations
// - **PHASE 5**: Complete interface contract for production
// - **PHASE 5**: Service health monitoring and statistics
//
// DEPENDENCIES:
// - System.Threading.Tasks (async operations)
// - System.Threading (cancellation tokens)
// - OstPlayer.Models (metadata models)
// - Playnite.SDK.Models (game entities)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle
// - Dependency Injection
// - Repository Pattern
// - Command Pattern (for operations)
//
// LIMITATIONS:
// - Async-only operations (no synchronous alternatives)
// - Single metadata service implementation assumed
// - No batch operations for performance optimization
// - Limited configuration options in interface
//
// FUTURE REFACTORING:
// TODO: Add batch metadata operations for performance
// TODO: Implement metadata source prioritization
// TODO: Add metadata validation and quality scoring
// TODO: Create specialized interfaces for each metadata source
// TODO: Add metadata streaming for large datasets
// CONSIDER: Splitting into smaller, more focused interfaces
// CONSIDER: Adding event-based metadata notifications
// IDEA: Real-time metadata synchronization
// IDEA: Collaborative metadata editing
//
// TESTING:
// - Mock implementations for unit testing
// - Integration tests with actual metadata sources
// - Performance tests for large metadata sets
// - Cancellation token handling tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Async/await patterns
//
// CHANGELOG:
// 2025-08-09 v2.0.0 - Phase 5 DI Implementation completed: Complete interface contract, service health monitoring, production-ready
// 2025-08-08 v1.0.0 - Initial interface for Phase 5 DI implementation
// ====================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OstPlayer.Models;
using Playnite.SDK.Models;

namespace OstPlayer.Services.Interfaces
{
    /// <summary>
    /// Interface for metadata service providing comprehensive metadata operations.
    /// </summary>
    public interface IMetadataService
    {
        #region Track Metadata Operations
        
        /// <summary>
        /// Loads metadata for a specific track asynchronously.
        /// </summary>
        Task<TrackMetadataModel> LoadTrackMetadataAsync(string filePath, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Loads metadata for multiple tracks with progress reporting.
        /// </summary>
        Task<List<TrackMetadataModel>> LoadMultipleTracksMetadataAsync(IEnumerable<string> filePaths, 
            IProgress<int> progress = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Saves track metadata to cache storage.
        /// </summary>
        Task SaveTrackMetadataAsync(string filePath, TrackMetadataModel metadata, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Album/Game Metadata Operations
        
        /// <summary>
        /// Loads album metadata for a game from external sources.
        /// </summary>
        Task<AlbumMetadataModel> LoadAlbumMetadataAsync(Game game, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Loads Discogs metadata for a specific game.
        /// </summary>
        Task<DiscogsMetadataModel> LoadDiscogsMetadataAsync(Game game, string discogsToken, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Loads MusicBrainz metadata for a specific game.
        /// </summary>
        Task<MusicBrainzMetadataModel> LoadMusicBrainzMetadataAsync(Game game, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Cache Management
        
        /// <summary>
        /// Invalidates cache for specific game or track.
        /// </summary>
        Task InvalidateCacheAsync(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Clears all cached metadata.
        /// </summary>
        Task ClearAllCacheAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets cache statistics and performance metrics.
        /// </summary>
        Task<CacheStatistics> GetCacheStatisticsAsync(CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Configuration and Health
        
        /// <summary>
        /// Configures metadata service with settings.
        /// </summary>
        void Configure(OstPlayerSettings settings);
        
        /// <summary>
        /// Performs health check on service components.
        /// </summary>
        Task<ServiceHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
        
        #endregion
    }
    
    /// <summary>
    /// Cache statistics for monitoring and optimization.
    /// </summary>
    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public double HitRatio => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
        public int TotalRequests => HitCount + MissCount;
        public long MemoryUsageBytes { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
    }
    
    /// <summary>
    /// Service health status for monitoring.
    /// </summary>
    public class ServiceHealthStatus
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; }
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
        public TimeSpan ResponseTime { get; set; }
    }
}