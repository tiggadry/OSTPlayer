// ====================================================================
// FILE: DiscogsClient.cs
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
// Static HTTP client for integrating with the Discogs API to retrieve
// music release metadata. Provides search functionality and detailed
// release information for enriching local MP3 metadata with external data,
// and vice versa enriching external data with internal metadata.
//
// Now optimized with shared HttpClient instance for better performance.
//
// FEATURES:
// - Release search with query string matching
// - Detailed release metadata retrieval
// - Image URL extraction for album artwork
// - Tracklist information parsing
// - Artist, label, and format information
// - Genre and style classification
// - Release date and country information
// - **v1.2.0**: Shared HttpClient for optimal connection pooling
// - **v1.2.1**: COMPREHENSIVE BUGFIX - Multiple JSON parsing and HTTP issues resolved
//
// DEPENDENCIES:
// - System.Net.Http (HTTP client operations)
// - Newtonsoft.Json (JSON deserialization)
// - OstPlayer.Models.DiscogsMetadataModel (data model)
//
// API INTEGRATION:
// - Discogs Database API v2.0
// - Personal Access Token authentication
// - Rate limiting compliance (60 requests/minute)
// - User-Agent header requirement
// - RESTful endpoint usage
//
// REQUEST/RESPONSE:
// - Search endpoint: /database/search
// - Release details: /releases/{id}
// - JSON response parsing with error handling
// - Data transformation to internal models
// - **v1.2.1**: Robust JSON cleaning and automatic compression handling
//
// PERFORMANCE NOTES:
// - **IMPROVED**: Shared HttpClient instance enables connection pooling
// - **IMPROVED**: Eliminates socket exhaustion from per-request clients
// - Lazy initialization with thread-safe singleton pattern
// - Minimal JSON parsing overhead
// - Efficient string operations
// - **v1.2.1**: Automatic GZIP/Deflate compression handling for optimal bandwidth
//
// LIMITATIONS:
// - No caching of API responses
// - No pagination support for search results
// - Limited to release-type entries only
// - No rate limiting implementation
// - No retry logic for failed requests
// - Single image extraction only
//
// BUG FIXES v1.2.1 - COMPREHENSIVE JSON AND HTTP FIXES:
// 1. **Fixed critical JSON deserialization error**:
//    - Changed DiscogsSearchResult.Results from `List<r>` to `List<Result>`
//    - Resolves type reference error causing JSON parsing failures
// 2. **Added robust JSON response cleaning**:
//    - CleanJsonResponse() method removes BOM markers and problematic characters
//    - Handles UTF-8 encoding issues and control character conflicts
// 3. **Implemented automatic GZIP compression handling**:
//    - HttpClientHandler with AutomaticDecompression for GZIP/Deflate
//    - Resolves compressed response parsing that caused garbage characters
// 4. **Production-ready error handling**:
//    - Streamlined error messages without debug overhead
//    - Clean exception handling for production environments
// All fixes address "Unexpected character encountered while parsing value" errors
// from various root causes (typos, encoding, compression).
//
// CHANGELOG:
// 2025-08-07 v1.2.1 - COMPREHENSIVE BUGFIX: JSON model fixes, encoding cleanup, GZIP compression support, production-ready error handling
// 2025-08-07 v1.2.0 - Implemented shared HttpClient pattern: Lazy<HttpClient> singleton, connection pooling, performance optimization
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive Discogs API integration
// ====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OstPlayer.Models;

namespace OstPlayer.Clients
{
    /// <summary>
    /// Static HTTP client for comprehensive Discogs API integration.
    /// Provides search and detailed metadata retrieval for music releases.
    /// Implements Discogs API v2.0 specification with proper authentication and rate limiting awareness.
    /// Reference: https://www.discogs.com/developers/
    ///
    /// PERFORMANCE: Uses shared HttpClient instance for optimal connection pooling and resource management.
    /// </summary>
    public static class DiscogsClient
    {
        #region Private Static Fields

        /// <summary>
        /// Shared HttpClient instance for all Discogs API operations.
        /// Configured once and reused across all requests for optimal performance.
        /// PERFORMANCE: Enables connection pooling and reduces socket exhaustion.
        /// THREAD SAFETY: HttpClient is thread-safe for concurrent use.
        /// </summary>
        private static readonly Lazy<HttpClient> _httpClientLazy = new Lazy<HttpClient>(
            CreateHttpClient
        );

        /// <summary>
        /// Gets the shared HttpClient instance with lazy initialization.
        /// Ensures thread-safe singleton pattern for HttpClient creation.
        /// </summary>
        private static HttpClient HttpClient => _httpClientLazy.Value;

        #endregion

        #region API Endpoints and Constants

        /// <summary>
        /// Discogs Database API search endpoint for finding releases by query.
        /// Supports free-text search across titles, artists, labels, and catalog numbers.
        /// Reference: https://www.discogs.com/developers/#page:database,header:database-search
        /// </summary>
        private const string ApiUrlSearch = "https://api.discogs.com/database/search";

        /// <summary>
        /// Discogs Database API endpoint template for retrieving detailed release information.
        /// Requires release ID appended to URL for specific release details.
        /// Reference: https://www.discogs.com/developers/#page:database,header:database-release
        /// </summary>
        private const string ApiUrlReleaseDetails = "https://api.discogs.com/releases/";

        /// <summary>
        /// User-Agent string required by Discogs API for all requests.
        /// Must include application name, version, and contact information.
        /// </summary>
        private const string UserAgent = "OstPlayer/1.0 (TiggAdry/OstPlayer)";

        #endregion

        #region HttpClient Factory and Configuration

        /// <summary>
        /// Creates and configures HttpClient instance for Discogs API communication.
        /// Called once during lazy initialization to set up shared client.
        /// CONFIGURATION: Sets User-Agent, timeout, and other required headers.
        /// </summary>
        /// <returns>Configured HttpClient instance ready for Discogs API calls</returns>
        private static HttpClient CreateHttpClient()
        {
            // Create HttpClientHandler with automatic decompression
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression =
                    System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            };

            var client = new HttpClient(handler);

            // Discogs API requires User-Agent header for all requests
            // Format: ApplicationName/Version (Contact)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

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
        /// Searches Discogs database for releases matching the provided query string.
        /// Performs free-text search across multiple fields and returns simplified metadata.
        /// Filters results to include only release-type entries for consistency.
        /// </summary>
        /// <param name="query">Free-text search query (album title, artist name, or combination)</param>
        /// <param name="token">Discogs Personal Access Token (required for API authentication)</param>
        /// <returns>List of simplified metadata objects from search results, empty list if no matches</returns>
        /// <exception cref="ArgumentException">Thrown when token is null, empty, or whitespace</exception>
        /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
        /// <exception cref="JsonException">Thrown when response cannot be parsed</exception>
        /// <example>
        /// var results = await DiscogsClient.SearchReleaseAsync("Final Fantasy VII", userToken);
        /// foreach (var release in results) { /* Process search results */ }
        /// </example>
        public static async Task<List<DiscogsMetadataModel>> SearchReleaseAsync(
            string query,
            string token
        )
        {
            // Validate required authentication token
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException(
                    "Discogs Personal Access Token is required for API access.",
                    nameof(token)
                );

            // Validate search query parameter
            if (string.IsNullOrWhiteSpace(query))
                return new List<DiscogsMetadataModel>(); // Return empty list for empty query

            // Construct search URL with query parameters and authentication
            // URI escaping prevents injection and handles special characters
            var url =
                $"{ApiUrlSearch}?q={Uri.EscapeDataString(query)}&type=release&token={Uri.EscapeDataString(token)}";

            try
            {
                // Send GET request using shared HttpClient instance
                // PERFORMANCE: Reuses existing connections and enables pooling
                var json = await HttpClient.GetStringAsync(url);

                // Validate response content
                if (string.IsNullOrEmpty(json))
                {
                    throw new InvalidOperationException("Received empty response from Discogs API");
                }

                // Clean JSON response from potential BOM and encoding issues
                json = CleanJsonResponse(json);

                // Deserialize JSON response to strongly-typed model
                var data = JsonConvert.DeserializeObject<DiscogsSearchResult>(json);

                // Process and transform search results to internal metadata model
                return ProcessSearchResults(data);
            }
            catch (HttpRequestException ex)
            {
                // Re-throw with more context for API communication failures
                throw new InvalidOperationException(
                    $"Failed to communicate with Discogs API: {ex.Message}",
                    ex
                );
            }
            catch (JsonException ex)
            {
                // Enhanced error message for JSON parsing failures
                throw new InvalidOperationException(
                    $"Failed to parse Discogs API response: {ex.Message}",
                    ex
                );
            }
        }

        /// <summary>
        /// Retrieves comprehensive metadata for a specific Discogs release by ID.
        /// Includes detailed information such as tracklist, labels, formats, and high-resolution images.
        /// Used for enriching search results with complete metadata information.
        /// </summary>
        /// <param name="releaseId">Discogs release ID (numeric identifier as string)</param>
        /// <param name="token">Discogs Personal Access Token (required for API authentication)</param>
        /// <returns>Detailed metadata object with comprehensive release information, null if not found</returns>
        /// <exception cref="ArgumentException">Thrown when token or releaseId is null, empty, or whitespace</exception>
        /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
        /// <exception cref="JsonException">Thrown when response cannot be parsed</exception>
        /// <example>
        /// var details = await DiscogsClient.GetReleaseDetailsAsync("123456", userToken);
        /// if (details != null) { /* Use detailed metadata */ }
        /// </example>
        public static async Task<DiscogsMetadataModel> GetReleaseDetailsAsync(
            string releaseId,
            string token
        )
        {
            // Validate required parameters
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException(
                    "Discogs Personal Access Token is required for API access.",
                    nameof(token)
                );

            if (string.IsNullOrWhiteSpace(releaseId))
                throw new ArgumentException(
                    "Release ID is required for detailed metadata retrieval.",
                    nameof(releaseId)
                );

            // Construct release details URL with authentication
            var url =
                $"{ApiUrlReleaseDetails}{Uri.EscapeDataString(releaseId)}?token={Uri.EscapeDataString(token)}";

            try
            {
                // Send GET request using shared HttpClient instance
                // PERFORMANCE: Reuses existing connections and enables pooling
                var json = await HttpClient.GetStringAsync(url);

                // Clean JSON response from potential BOM and encoding issues
                json = CleanJsonResponse(json);

                // Deserialize JSON response to strongly-typed model
                var data = JsonConvert.DeserializeObject<DiscogsReleaseDetails>(json);

                // Return null if no data received (release not found)
                if (data == null)
                    return null;

                // Transform API response to internal metadata model
                return MapReleaseDetailsToMetadataModel(data);
            }
            catch (HttpRequestException ex)
            {
                // Re-throw with more context for API communication failures
                throw new InvalidOperationException(
                    $"Failed to retrieve release details from Discogs API: {ex.Message}",
                    ex
                );
            }
            catch (JsonException ex)
            {
                // Enhanced error message for JSON parsing failures
                throw new InvalidOperationException(
                    $"Failed to parse Discogs release details response: {ex.Message}",
                    ex
                );
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Configures HttpClient with required headers for Discogs API communication.
        /// Sets User-Agent header which is mandatory for all Discogs API requests.
        ///
        /// NOTE: This method is deprecated in favor of CreateHttpClient() factory.
        /// Keeping for reference and potential future per-request configuration.
        /// </summary>
        /// <param name="client">HttpClient instance to configure</param>
        [Obsolete(
            "Use CreateHttpClient() factory method instead. This method is kept for reference."
        )]
        private static void ConfigureHttpClient(HttpClient client)
        {
            // Discogs API requires User-Agent header for all requests
            // Format: ApplicationName/Version (Contact)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

            // Set reasonable timeout for API requests (30 seconds)
            client.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Processes raw search results from Discogs API and converts to internal metadata models.
        /// Filters for release-type entries only and handles artist/title parsing for consistency.
        /// Implements intelligent fallbacks for missing or malformed data.
        /// </summary>
        /// <param name="data">Deserialized search result data from Discogs API</param>
        /// <returns>List of processed metadata models ready for UI consumption</returns>
        private static List<DiscogsMetadataModel> ProcessSearchResults(DiscogsSearchResult data)
        {
            var releases = new List<DiscogsMetadataModel>();

            // Validate input data structure
            if (data?.Results == null)
                return releases; // Return empty list for null/invalid data

            // Filter for release-type entries only (exclude masters, artists, labels)
            // This ensures consistent metadata structure across results
            var releaseResults = data
                .Results.Where(r =>
                    string.Equals(r.Type, "release", StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            // Process each release result into internal metadata model
            foreach (var result in releaseResults)
            {
                // Exclude results with missing or blank title/id
                if (string.IsNullOrWhiteSpace(result.Title) || result.Id <= 0)
                    continue; // Skip this result, it's incomplete

                // Extract artist and title with intelligent parsing
                string artist = result.Artist;
                string title = result.Title;

                // Handle cases where artist info is embedded in title field
                // Common format: "Artist - Title" when artist field is empty
                if (string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title))
                {
                    var parts = title.Split(new[] { " - " }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        artist = parts[0].Trim();
                        title = parts[1].Trim();
                    }
                }

                // Create internal metadata model with processed data
                releases.Add(
                    new DiscogsMetadataModel
                    {
                        Title = title ?? string.Empty,
                        Artist = artist ?? string.Empty,
                        Released = result.Year, // Map Year to Released for model consistency
                        CoverUrl = result.Thumb, // Thumbnail image URL from search results
                        DiscogsUrl = $"https://www.discogs.com/release/{result.Id}", // Construct full URL
                    }
                );
            }

            return releases;
        }

        /// <summary>
        /// Maps comprehensive Discogs release detail data to internal metadata model.
        /// Handles complex data structures including tracklists, images, and multi-value fields.
        /// Implements intelligent defaults and null safety throughout the mapping process.
        /// </summary>
        /// <param name="data">Detailed release data from Discogs API</param>
        /// <returns>Comprehensive metadata model with all available release information</returns>
        private static DiscogsMetadataModel MapReleaseDetailsToMetadataModel(
            DiscogsReleaseDetails data
        )
        {
            // Extract cover image URL with fallback priority
            // Priority: Full-size image (Uri) > Medium-size image (150px) > None
            string coverUrl = null;
            if (data.Images != null && data.Images.Count > 0)
            {
                var primaryImage = data.Images[0]; // Use first image as primary
                coverUrl = !string.IsNullOrEmpty(primaryImage.Uri)
                    ? primaryImage.Uri
                    : primaryImage.Uri150; // Fallback to smaller size
            }

            // Extract primary artist name from artists collection
            string primaryArtist = null;
            if (data.Artists != null && data.Artists.Count > 0)
            {
                primaryArtist = data.Artists[0].Name;
            }

            // Combine multiple labels into single comma-separated string
            string labelString = null;
            if (data.Labels != null && data.Labels.Count > 0)
            {
                labelString = string.Join(", ", data.Labels.Select(l => l.Name));
            }

            // Format physical format information with descriptions
            string formatString = null;
            if (data.Formats != null && data.Formats.Count > 0)
            {
                var formatDescriptions = data.Formats.Select(f =>
                    f.Name + (string.IsNullOrEmpty(f.Description) ? "" : $" ({f.Description})")
                );
                formatString = string.Join(", ", formatDescriptions);
            }

            // Convert tracklist to internal model format
            var tracklistModels = new List<DiscogsMetadataModel.DiscogsTrack>();
            if (data.Tracklist != null)
            {
                tracklistModels = data
                    .Tracklist.Select(t => new DiscogsMetadataModel.DiscogsTrack
                    {
                        Title = t.Title ?? string.Empty,
                        Duration = t.Duration ?? string.Empty,
                    })
                    .ToList();
            }

            // Create comprehensive metadata model with all processed data
            return new DiscogsMetadataModel
            {
                Title = data.Title ?? string.Empty,
                Album = data.Title ?? string.Empty, // Alias: treat title as album name
                Artist = primaryArtist ?? string.Empty,
                Country = data.Country ?? string.Empty,
                Label = labelString ?? string.Empty,
                Format = formatString ?? string.Empty,
                Genres = data.Genres ?? new List<string>(),
                Styles = data.Styles ?? new List<string>(),
                Notes = data.Notes ?? string.Empty,
                Released = data.Released ?? data.Year ?? string.Empty, // Prefer 'Released' over 'Year'
                Comment = null, // Reserved for future use or user annotations
                CoverUrl = coverUrl,
                DiscogsUrl = $"https://www.discogs.com/release/{data.Id}",
                Tracklist = tracklistModels,
            };
        }

        /// <summary>
        /// Cleans JSON response from potential encoding issues, BOM markers, and other problematic characters.
        /// This fixes common issues with API responses that contain unexpected characters.
        /// </summary>
        /// <param name="json">Raw JSON string from API response</param>
        /// <returns>Cleaned JSON string ready for deserialization</returns>
        private static string CleanJsonResponse(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;

            // Remove BOM (Byte Order Mark) if present
            // UTF-8 BOM is EF BB BF (239 187 191)
            if (json.Length > 0 && json[0] == '\uFEFF')
            {
                json = json.Substring(1);
            }

            // Remove other common problematic characters
            // Remove leading/trailing whitespace and control characters
            json = json.Trim('\0', '\uFEFF', '\u200B', '\u00A0', ' ', '\t', '\n', '\r');

            // Replace any remaining control characters that might interfere with JSON parsing
            // Keep only printable characters and standard JSON whitespace
            var cleaned = new System.Text.StringBuilder(json.Length);
            foreach (char c in json)
            {
                if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                {
                    // Skip control characters except for standard JSON whitespace
                    continue;
                }

                cleaned.Append(c);
            }

            return cleaned.ToString();
        }

        #endregion

        #region Internal JSON Deserialization Models

        /// <summary>
        /// Internal model for deserializing Discogs search API responses.
        /// Maps directly to JSON structure returned by /database/search endpoint.
        /// Reference: https://www.discogs.com/developers/#page:database,header:database-search
        /// </summary>
        private sealed class DiscogsSearchResult
        {
            /// <summary>Collection of search result items matching the query.</summary>
            [JsonProperty("results")]
            public List<Result> Results { get; set; }

            /// <summary>
            /// Individual search result item containing basic release information.
            /// Minimal data set optimized for search result display and selection.
            /// </summary>
            public sealed class Result
            {
                /// <summary>Release title, may include artist information in "Artist - Title" format.</summary>
                [JsonProperty("title")]
                public string Title { get; set; }

                /// <summary>Release year as string (may be empty for unreleased items).</summary>
                [JsonProperty("year")]
                public string Year { get; set; }

                /// <summary>Unique Discogs release ID for detailed metadata retrieval.</summary>
                [JsonProperty("id")]
                public int Id { get; set; }

                /// <summary>Thumbnail image URL for quick preview (150x150 pixels typically).</summary>
                [JsonProperty("thumb")]
                public string Thumb { get; set; }

                /// <summary>Entry type (release, master, artist, label) for filtering.</summary>
                [JsonProperty("type")]
                public string Type { get; set; }

                /// <summary>Primary artist name (may be null if embedded in title).</summary>
                [JsonProperty("artist")]
                public string Artist { get; set; }
            }
        }

        /// <summary>
        /// Internal model for deserializing detailed Discogs release API responses.
        /// Maps to complete JSON structure returned by /releases/{id} endpoint.
        /// Reference: https://www.discogs.com/developers/#page:database,header:database-release
        /// </summary>
        private sealed class DiscogsReleaseDetails
        {
            /// <summary>Release title (album/single name).</summary>
            [JsonProperty("title")]
            public string Title { get; set; }

            /// <summary>Release year as string.</summary>
            [JsonProperty("year")]
            public string Year { get; set; }

            /// <summary>Unique Discogs release ID.</summary>
            [JsonProperty("id")]
            public int Id { get; set; }

            /// <summary>Country of release.</summary>
            [JsonProperty("country")]
            public string Country { get; set; }

            /// <summary>Musical genres associated with release.</summary>
            [JsonProperty("genres")]
            public List<string> Genres { get; set; }

            /// <summary>Musical styles (sub-genres) associated with release.</summary>
            [JsonProperty("styles")]
            public List<string> Styles { get; set; }

            /// <summary>Record labels that released this item.</summary>
            [JsonProperty("labels")]
            public List<Label> Labels { get; set; }

            /// <summary>Physical formats and media information.</summary>
            [JsonProperty("formats")]
            public List<Format> Formats { get; set; }

            /// <summary>Additional notes and release information.</summary>
            [JsonProperty("notes")]
            public string Notes { get; set; }

            /// <summary>Specific release date (more precise than year).</summary>
            [JsonProperty("released")]
            public string Released { get; set; }

            /// <summary>Album artwork and promotional images.</summary>
            [JsonProperty("images")]
            public List<Image> Images { get; set; }

            /// <summary>Complete track listing with titles and durations.</summary>
            [JsonProperty("tracklist")]
            public List<Track> Tracklist { get; set; }

            /// <summary>All artists credited on this release.</summary>
            [JsonProperty("artists")]
            public List<Artist> Artists { get; set; }

            /// <summary>Record label information including name and catalog number.</summary>
            public sealed class Label
            {
                /// <summary>Record label name.</summary>
                [JsonProperty("name")]
                public string Name { get; set; }
            }

            /// <summary>Physical format details including media type and description.</summary>
            public sealed class Format
            {
                /// <summary>Format name (CD, Vinyl, Cassette, etc.).</summary>
                [JsonProperty("name")]
                public string Name { get; set; }

                /// <summary>Additional format description (12", 180g, Remastered, etc.).</summary>
                [JsonProperty("description")]
                public string Description { get; set; }
            }

            /// <summary>Image metadata including multiple resolution options.</summary>
            public sealed class Image
            {
                /// <summary>Full-resolution image URL.</summary>
                [JsonProperty("uri")]
                public string Uri { get; set; }

                /// <summary>Medium-resolution image URL (150px).</summary>
                [JsonProperty("uri150")]
                public string Uri150 { get; set; }
            }

            /// <summary>Individual track information from release tracklist.</summary>
            public sealed class Track
            {
                /// <summary>Track title including any featuring artists or subtitles.</summary>
                [JsonProperty("title")]
                public string Title { get; set; }

                /// <summary>Track duration in MM:SS format (may be empty).</summary>
                [JsonProperty("duration")]
                public string Duration { get; set; }
            }

            /// <summary>Artist credit information including role and collaboration details.</summary>
            public sealed class Artist
            {
                /// <summary>Artist name as credited on release.</summary>
                [JsonProperty("name")]
                public string Name { get; set; }
            }
        }

        #endregion
    }
}
