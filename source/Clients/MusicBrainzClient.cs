// ====================================================================
// FILE: MusicBrainzClient.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Clients
// LOCATION: Clients/
// VERSION: 1.2.1
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// HTTP client for integrating with the MusicBrainz API to retrieve
// music release metadata. Provides search functionality for album releases
// to supplement metadata from Discogs and local MP3 tags. Now optimized
// with shared HttpClient instance and async/await support.
//
// FEATURES:
// - Release search by artist and album name
// - JSON response parsing and deserialization
// - Basic release information extraction
// - Date, status, and country metadata
// - Simple query string formatting
// - HTTP error handling
// - **v1.2.0**: Shared HttpClient for optimal connection pooling
// - **v1.2.0**: Async/await support for non-blocking operations
// - **v1.2.1**: GZIP compression support for optimal API communication
//
// DEPENDENCIES:
// - System.Net.Http (HTTP client operations)
// - Newtonsoft.Json (JSON deserialization)
// - System (URI escaping and formatting)
// - System.Threading.Tasks (async operations support)
//
// API INTEGRATION:
// - MusicBrainz Web Service v2.0
// - No authentication required (public API)
// - Rate limiting guidelines (1 request/second recommended)
// - User-Agent header requirement
// - Query-based search interface
//
// REQUEST/RESPONSE:
// - Search endpoint: /ws/2/release/
// - Query format: artist:{artist}+release:{album}
// - JSON response format
// - **IMPROVED**: Async HTTP operations with proper error handling
// - **v1.2.1**: Automatic GZIP/Deflate response decompression
//
// PERFORMANCE NOTES:
// - **IMPROVED**: Shared HttpClient instance enables connection pooling
// - **IMPROVED**: Async operations prevent UI thread blocking
// - **IMPROVED**: Eliminates socket exhaustion from per-request clients
// - Lazy initialization with thread-safe singleton pattern
// - Minimal JSON parsing overhead
// - Simple string formatting operations
// - **v1.2.1**: Automatic compression handling for bandwidth efficiency
//
// LIMITATIONS:
// - No caching of API responses
// - No pagination support for large result sets
// - Limited metadata extraction (basic fields only)
// - No retry logic for failed requests
// - No rate limiting implementation
//
// FUTURE REFACTORING:
// ? COMPLETED: Implement async/await pattern for all HTTP operations
// TODO: Add comprehensive error handling with custom exceptions
// TODO: Implement rate limiting (1 request/second)
// TODO: Add response caching with TTL expiration
// TODO: Support pagination for large search results
// TODO: Extract additional metadata fields (genres, labels, etc.)
// TODO: Add retry logic with exponential backoff
// ? COMPLETED: Implement HttpClient shared service pattern
// ? COMPLETED: Add automatic GZIP/Deflate decompression support
// TODO: Add request/response logging for debugging
// TODO: Support advanced search queries and filters
// CONSIDER: Adding circuit breaker pattern for resilience
// IDEA: Fuzzy matching algorithms for better search results
// IDEA: Integration with other MusicBrainz entities (artists, recordings)
//
// CHANGELOG:
// 2025-08-07 v1.2.1 - GZIP compression enhancement: Added automatic decompression for optimal API communication
// 2025-08-07 v1.2.0 - Implemented shared HttpClient pattern and async/await: Lazy<HttpClient> singleton, SearchReleaseAsync method, connection pooling
// 2025-08-06 v1.0.0 - Initial implementation with MusicBrainz API integration
// ====================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OstPlayer.Clients
{
    /// <summary>
    /// Represents the result of a MusicBrainz release search.
    /// </summary>
    public class MusicBrainzReleaseResult
    {
        public List<Release> releases { get; set; }
    }

    /// <summary>
    /// Represents a single release entry from MusicBrainz.
    /// </summary>
    public class Release
    {
        public string title { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string country { get; set; }
        public string id { get; set; }
    }

    /// <summary>
    /// Client for querying the MusicBrainz API for release information.
    /// Uses shared HttpClient instance for optimal performance and connection pooling.
    /// </summary>
    public static class MusicBrainzClient
    {
        #region Private Static Fields

        /// <summary>
        /// Shared HttpClient instance for all MusicBrainz API operations.
        /// Configured once and reused across all requests for optimal performance.
        /// PERFORMANCE: Enables connection pooling and reduces socket exhaustion.
        /// THREAD SAFETY: HttpClient is thread-safe for concurrent use.
        /// </summary>
        private static readonly Lazy<HttpClient> _httpClientLazy = new Lazy<HttpClient>(CreateHttpClient);

        /// <summary>
        /// Gets the shared HttpClient instance with lazy initialization.
        /// Ensures thread-safe singleton pattern for HttpClient creation.
        /// </summary>
        private static HttpClient HttpClient => _httpClientLazy.Value;

        #endregion

        #region Constants

        private const string BaseUrl =
            "https://musicbrainz.org/ws/2/release/?query=artist:{0}+release:{1}&fmt=json";

        #endregion

        #region HttpClient Factory and Configuration

        /// <summary>
        /// Creates and configures HttpClient instance for MusicBrainz API communication.
        /// Called once during lazy initialization to set up shared client.
        /// CONFIGURATION: Sets User-Agent, timeout, and other required headers.
        /// </summary>
        /// <returns>Configured HttpClient instance ready for MusicBrainz API calls</returns>
        private static HttpClient CreateHttpClient()
        {
            // Create HttpClientHandler with automatic decompression
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            
            var client = new HttpClient(handler);
            
            // MusicBrainz API requires User-Agent header for all requests
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OstPlayer/1.0 (TiggAdry/OstPlayer)");
            
            // Set reasonable timeout for API requests (30 seconds)
            client.Timeout = TimeSpan.FromSeconds(30);
            
            // Configure headers for optimal API performance
            // NOTE: Accept-Encoding is handled automatically by AutomaticDecompression
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            return client;
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Searches MusicBrainz for releases matching the given artist and album title (async).
        /// PERFORMANCE: Uses shared HttpClient and async operations for better responsiveness.
        /// </summary>
        /// <param name="artist">Artist name</param>
        /// <param name="album">Album title</param>
        /// <returns>Deserialized MusicBrainzReleaseResult or null if not found</returns>
        public static async Task<MusicBrainzReleaseResult> SearchReleaseAsync(string artist, string album)
        {
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(album))
                return null;

            var url = string.Format(
                BaseUrl,
                Uri.EscapeDataString(artist),
                Uri.EscapeDataString(album)
            );

            try
            {
                // Use shared HttpClient instance for optimal performance
                var response = await HttpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<MusicBrainzReleaseResult>(json);
                return result;
            }
            catch (HttpRequestException)
            {
                // Return null for HTTP failures - graceful degradation
                return null;
            }
            catch (JsonException)
            {
                // Return null for JSON parsing failures - graceful degradation  
                return null;
            }
        }

        /// <summary>
        /// Searches MusicBrainz for releases matching the given artist and album title (synchronous wrapper).
        /// USAGE: Use SearchReleaseAsync for new code, this is for backward compatibility only.
        /// BLOCKING: This method blocks the calling thread - use with caution in UI code.
        /// </summary>
        /// <param name="artist">Artist name</param>
        /// <param name="album">Album title</param>
        /// <returns>Deserialized MusicBrainzReleaseResult or null if not found</returns>
        public static MusicBrainzReleaseResult SearchRelease(string artist, string album)
        {
            // Use GetAwaiter().GetResult() for synchronous execution
            // NOTE: This is not ideal but necessary for backward compatibility
            return SearchReleaseAsync(artist, album).GetAwaiter().GetResult();
        }

        #endregion
    }
}
