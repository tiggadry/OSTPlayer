// ====================================================================
// FILE: IDiscogsClient.cs
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
// Interface for Discogs API client supporting dependency injection.
// Provides contract for Discogs music database integration operations.
//
// FEATURES:
// - Release search functionality
// - Detailed release metadata retrieval
// - Async operations with cancellation support
// - Health check for service monitoring
// - **PHASE 5**: Production-ready Discogs client interface
// - **PHASE 5**: Enhanced error handling and token management
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
// - Requires Discogs Personal Access Token
// - Limited by Discogs API rate limits
// - Basic error reporting through exceptions
// - No batch operation support
//
// FUTURE REFACTORING:
// FUTURE: Add batch search operations for multiple queries
// FUTURE: Implement rate limiting and retry logic
// FUTURE: Add support for Discogs marketplace data
// FUTURE: Create advanced search filters and options
// FUTURE: Add caching support for repeated queries
// CONSIDER: Pagination support for large result sets
// CONSIDER: Advanced error reporting with status codes
// IDEA: Real-time Discogs data change notifications
// IDEA: Integration with Discogs collection management
//
// TESTING:
// - Mock implementations for unit testing
// - Discogs API integration tests
// - Error handling and network failure tests
// - Token validation tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Discogs API v1 compatibility
//
// CHANGELOG:
// 2025-08-09 v1.1.0 - Phase 5 DI Implementation completed: Production-ready interface, enhanced error handling
// 2025-08-08 v1.0.0 - Initial interface for Phase 5 DI implementation
// ====================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OstPlayer.Models;

namespace OstPlayer.Services.Interfaces {
    /// <summary>
    /// Interface for Discogs API client supporting dependency injection.
    /// </summary>
    public interface IDiscogsClient {
        /// <summary>
        /// Searches Discogs database for releases matching the provided query.
        /// </summary>
        Task<List<DiscogsMetadataModel>> SearchReleaseAsync(string query, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves detailed metadata for a specific Discogs release.
        /// </summary>
        Task<DiscogsMetadataModel> GetReleaseDetailsAsync(string releaseId, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs health check on Discogs API service.
        /// </summary>
        Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
