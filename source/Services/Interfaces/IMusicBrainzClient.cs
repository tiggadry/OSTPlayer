// ====================================================================
// FILE: IMusicBrainzClient.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Services/Interfaces
// LOCATION: Services/Interfaces/
// VERSION: 1.1.0
// CREATED: 2025-08-08
// UPDATED: 2025-08-09
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Interface for MusicBrainz API client supporting dependency injection.
// Provides contract for MusicBrainz music database integration operations.
//
// FEATURES:
// - Release search functionality
// - Artist and album lookup
// - Async operations with cancellation support
// - Health check for service monitoring
// - **PHASE 5**: Production-ready MusicBrainz client interface
// - **PHASE 5**: Enhanced search capabilities and error handling
//
// DEPENDENCIES:
// - System.Threading.Tasks (async operations)
// - System.Threading (cancellation tokens)
// - OstPlayer.Models (metadata models)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle
// - Dependency Injection
// - Repository Pattern
//
// LIMITATIONS:
// - No authentication required but rate limited
// - Basic search parameters (artist and album only)
// - Single release return (no multiple results)
// - No cover art service integration
//
// FUTURE REFACTORING:
// FUTURE: Add advanced search parameters and filters
// FUTURE: Implement multiple search result support
// FUTURE: Add MusicBrainz cover art service integration
// FUTURE: Create batch search operations
// FUTURE: Add support for recording and work lookups
// CONSIDER: MBID (MusicBrainz ID) based lookups
// CONSIDER: Related entity search (artist, label, etc.)
// IDEA: Real-time MusicBrainz data synchronization
// IDEA: Integration with MusicBrainz Picard for tagging
//
// TESTING:
// - Mock implementations for unit testing
// - MusicBrainz API integration tests
// - Error handling and network failure tests
// - Search parameter validation tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - MusicBrainz Web Service v2 compatibility
//
// CHANGELOG:
// 2025-08-09 v1.1.0 - Phase 5 DI Implementation completed: Production-ready interface, enhanced search capabilities
// 2025-08-08 v1.0.0 - Initial interface for Phase 5 DI implementation
// ====================================================================

using System.Threading;
using System.Threading.Tasks;
using OstPlayer.Models;

namespace OstPlayer.Services.Interfaces {
    /// <summary>
    /// Interface for MusicBrainz API client supporting dependency injection.
    /// </summary>
    public interface IMusicBrainzClient {
        /// <summary>
        /// Searches MusicBrainz database for releases matching the provided artist and album.
        /// </summary>
        Task<MusicBrainzMetadataModel> SearchReleaseAsync(string artist, string album, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs health check on MusicBrainz API service.
        /// </summary>
        Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
