// ====================================================================
// FILE: OstPlayerSettings.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Models
// LOCATION: /
// VERSION: 1.3.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Defines the settings model for the OstPlayer plugin.
// Handles serializable plugin settings including Discogs API integration,
// playback preferences, UI configuration, and advanced metadata caching options.
// Now enhanced with TTL cache configuration and memory management settings.
//
// FEATURES:
// - Discogs API token management
// - Playback behavior configuration
// - UI visibility preferences
// - **NEW**: Advanced metadata caching settings with TTL configuration
// - **NEW**: Memory pressure management settings
// - **NEW**: Cache warming and optimization preferences
// - PlayniteSound integration options
// - Settings validation with comprehensive error reporting
//
// DEPENDENCIES:
// - Playnite.SDK (ObservableObject base class)
// - System.ComponentModel (DefaultValue attributes)
// - System (TimeSpan for TTL configuration)
//
// DATA STRUCTURE:
// - DiscogsToken: Personal API token for Discogs integration
// - DefaultVolume: Initial volume level (0-100%)
// - AutoPlayNext: Automatic track progression setting
// - **ENHANCED**: EnableMetadataCache: Performance optimization toggle
// - **NEW**: MetadataCacheTTLHours: TTL configuration for metadata cache
// - **NEW**: MaxCacheSize: Maximum cache size with memory management
// - **NEW**: EnableMemoryPressureAdjustment: Automatic cache size adjustment
// - **NEW**: EnableCacheWarming: Intelligent cache pre-loading
// - ShowMp3MetadataByDefault: UI visibility setting
// - ShowDiscogsMetadataByDefault: UI visibility setting
// - PausePlayniteSoundOnPlay: Integration behavior setting
//
// PERFORMANCE NOTES:
// - Property change notifications for data binding
// - Validation on value assignment with range limiting
// - TTL configuration in hours for user-friendly settings
// - Memory management settings for optimal resource usage
//
// LIMITATIONS:
// - No encryption for sensitive data (Discogs token)
// - Limited validation granularity
// - No settings migration between versions
//
// FUTURE REFACTORING:
// FUTURE: Add settings encryption for API tokens
// FUTURE: Implement settings migration for version upgrades
// FUTURE: Add more granular validation with localized messages
// FUTURE: Add settings backup/restore functionality
// FUTURE: Implement settings profiles for different use cases
// FUTURE: Add real-time settings validation UI feedback
// CONSIDER: Moving validation to separate validator class
// IDEA: Cloud sync for settings across Playnite installations
//
// TESTING:
// - Unit tests for property validation
// - Range testing for numeric properties
// - Serialization/deserialization tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Playnite SDK 6.x
//
// CHANGELOG:
// 2025-08-07 v1.3.0 - Enhanced with advanced cache settings: TTL configuration, memory management, cache warming options
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive settings
// ====================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OstPlayer
{
    /// <summary>
    /// Serializable settings model for the OstPlayer plugin with advanced cache configuration.
    /// Enhanced with TTL cache settings, memory management, and performance optimization options.
    /// </summary>
    public class OstPlayerSettings : ObservableObject
    {
        // Private Fields
        private string discogsToken = "";
        private double defaultVolume = 50.0;
        private bool autoPlayNext = true;
        private bool enableMetadataCache = true;
        private bool showMp3MetadataByDefault = true;
        private bool showDiscogsMetadataByDefault = true;
        private bool pausePlayniteSoundOnPlay = true;

        // Advanced cache settings
        private int metadataCacheTTLHours = 6;
        private int maxCacheSize = 2000;
        private bool enableMemoryPressureAdjustment = true;
        private bool enableCacheWarming = true;
        private int cacheCleanupIntervalMinutes = 5;

        // DataGrid column width settings
        private double trackNumberColumnWidth = 40.0;
        private double trackTitleColumnWidth = 300.0;
        private double durationColumnWidth = 100.0;

        // Track column splitter position
        private double titleDurationSplitterRatio = 0.75;

        #region Basic Plugin Settings

        /// <summary>
        /// Personal Discogs API token for metadata lookups.
        /// </summary>
        public string DiscogsToken
        {
            get => this.discogsToken;
            set
            {
                this.discogsToken = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Default volume for music playback (0-100)
        /// </summary>
        [DefaultValue(50.0)]
        public double DefaultVolume
        {
            get => this.defaultVolume;
            set
            {
                this.defaultVolume = Math.Max(0, Math.Min(100, value));
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether to automatically play the next track when current finishes
        /// </summary>
        [DefaultValue(true)]
        public bool AutoPlayNext
        {
            get => this.autoPlayNext;
            set
            {
                this.autoPlayNext = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether to pause PlayniteSound when starting music playback
        /// </summary>
        [DefaultValue(true)]
        public bool PausePlayniteSoundOnPlay
        {
            get => this.pausePlayniteSoundOnPlay;
            set
            {
                this.pausePlayniteSoundOnPlay = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region DataGrid Column Width Settings

        /// <summary>
        /// Stored width for the track number column (default: 40px)
        /// Can be expanded but not shrunk below default for better readability
        /// </summary>
        [DefaultValue(40.0)]
        public double TrackNumberColumnWidth
        {
            get => this.trackNumberColumnWidth;
            set
            {
                this.trackNumberColumnWidth = Math.Max(40, Math.Min(200, value)); // Clamp between 40-200px (expandable only)
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Stored width for the track title column (default: proportional sizing)
        /// Uses star-sizing with 2* ratio, minimum width enforced at 250px
        /// FIXED: Enhanced to preserve column position between TrackTitle and TrackDuration
        /// </summary>
        [DefaultValue(300.0)]
        public double TrackTitleColumnWidth
        {
            get => this.trackTitleColumnWidth;
            set
            {
                this.trackTitleColumnWidth = Math.Max(250, value); // Minimum 250px, no maximum limit for star-sizing
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Stored width for the duration column (default: proportional sizing)
        /// Uses star-sizing with 1* ratio, anchored to right edge with dynamic width
        /// FIXED: Enhanced to preserve column position between TrackTitle and TrackDuration
        /// </summary>
        [DefaultValue(100.0)]
        public double DurationColumnWidth
        {
            get => this.durationColumnWidth;
            set
            {
                this.durationColumnWidth = Math.Max(80, value); // Minimum 80px, no maximum limit for star-sizing
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// FIXED: New property to track column splitter position ratio between TrackTitle and TrackDuration
        /// Stores the relative position of the splitter as a ratio (0.0 to 1.0)
        /// This preserves the exact user positioning between the two star-sized columns
        /// </summary>
        [DefaultValue(0.75)]
        public double TitleDurationSplitterRatio
        {
            get => this.titleDurationSplitterRatio;
            set
            {
                this.titleDurationSplitterRatio = Math.Max(0.2, Math.Min(0.9, value)); // Clamp between 20% and 90%
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region UI Visibility Settings

        /// <summary>
        /// Whether to show MP3 metadata section by default
        /// </summary>
        [DefaultValue(true)]
        public bool ShowMp3MetadataByDefault
        {
            get => this.showMp3MetadataByDefault;
            set
            {
                this.showMp3MetadataByDefault = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether to show Discogs metadata section by default
        /// </summary>
        [DefaultValue(true)]
        public bool ShowDiscogsMetadataByDefault
        {
            get => this.showDiscogsMetadataByDefault;
            set
            {
                this.showDiscogsMetadataByDefault = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Advanced Cache Settings

        /// <summary>
        /// Whether to enable metadata caching for better performance
        /// **ENHANCED**: Now controls advanced TTL caching system
        /// </summary>
        [DefaultValue(true)]
        public bool EnableMetadataCache
        {
            get => this.enableMetadataCache;
            set
            {
                this.enableMetadataCache = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsCacheConfigurationEnabled));
            }
        }

        /// <summary>
        /// TTL (Time To Live) for metadata cache entries in hours.
        /// **NEW**: Configurable cache expiration for optimal performance
        /// Range: 1-72 hours (1 hour to 3 days)
        /// </summary>
        [DefaultValue(6)]
        public int MetadataCacheTTLHours
        {
            get => this.metadataCacheTTLHours;
            set
            {
                this.metadataCacheTTLHours = Math.Max(1, Math.Min(72, value));
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Maximum number of metadata entries to cache.
        /// **ENHANCED**: Increased default size for better performance
        /// Range: 100-10000 entries
        /// </summary>
        [DefaultValue(2000)]
        public int MaxCacheSize
        {
            get => this.maxCacheSize;
            set
            {
                this.maxCacheSize = Math.Max(100, Math.Min(10000, value));
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether to enable automatic cache size adjustment based on memory pressure.
        /// **NEW**: Intelligent memory management for optimal system performance
        /// </summary>
        [DefaultValue(true)]
        public bool EnableMemoryPressureAdjustment
        {
            get => this.enableMemoryPressureAdjustment;
            set
            {
                this.enableMemoryPressureAdjustment = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether to enable intelligent cache warming for frequently accessed files.
        /// **NEW**: Pre-loading optimization for better user experience
        /// </summary>
        [DefaultValue(true)]
        public bool EnableCacheWarming
        {
            get => this.enableCacheWarming;
            set
            {
                this.enableCacheWarming = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Interval for background cache cleanup operations in minutes.
        /// **NEW**: Configurable cleanup frequency for performance tuning
        /// Range: 1-60 minutes
        /// </summary>
        [DefaultValue(5)]
        public int CacheCleanupIntervalMinutes
        {
            get => this.cacheCleanupIntervalMinutes;
            set
            {
                this.cacheCleanupIntervalMinutes = Math.Max(1, Math.Min(60, value));
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Computed Properties

        /// <summary>
        /// Gets whether cache configuration settings are enabled.
        /// Used for UI binding to enable/disable cache options
        /// </summary>
        public bool IsCacheConfigurationEnabled => this.EnableMetadataCache;

        /// <summary>
        /// Gets the TTL as TimeSpan for internal use.
        /// Converts hours setting to TimeSpan for cache configuration
        /// </summary>
        public TimeSpan MetadataCacheTTL => TimeSpan.FromHours(this.MetadataCacheTTLHours);

        /// <summary>
        /// Gets the cleanup interval as TimeSpan for internal use.
        /// Converts minutes setting to TimeSpan for cache configuration
        /// </summary>
        public TimeSpan CacheCleanupInterval =>
            TimeSpan.FromMinutes(this.CacheCleanupIntervalMinutes);

        /// <summary>
        /// Gets estimated memory usage for current cache settings.
        /// Provides user feedback on memory impact
        /// </summary>
        public string EstimatedMemoryUsage
        {
            get
            {
                if (!this.EnableMetadataCache)
                    return "0 MB (Cache Disabled)";

                // Rough estimation: ~1KB per metadata entry
                var estimatedMB = (this.MaxCacheSize * 1024) / (1024 * 1024);
                return $"~{estimatedMB} MB";
            }
        }

        #endregion

        #region Settings Validation

        /// <summary>
        /// Validates the current settings including new cache configurations.
        /// **ENHANCED**: Now includes validation for advanced cache settings
        /// </summary>
        /// <returns>List of validation errors (empty if all valid)</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            // Basic settings validation
            if (this.DefaultVolume < 0 || this.DefaultVolume > 100)
                errors.Add("Default volume must be between 0 and 100");

            // **NEW**: Advanced cache settings validation
            if (this.EnableMetadataCache)
            {
                if (this.MetadataCacheTTLHours < 1 || this.MetadataCacheTTLHours > 72)
                    errors.Add("Cache TTL must be between 1 and 72 hours");

                if (this.MaxCacheSize < 100 || this.MaxCacheSize > 10000)
                    errors.Add("Max cache size must be between 100 and 10,000 entries");

                if (this.CacheCleanupIntervalMinutes < 1 || this.CacheCleanupIntervalMinutes > 60)
                    errors.Add("Cache cleanup interval must be between 1 and 60 minutes");

                // Memory usage warning (not an error, but helpful feedback)
                var estimatedMB = (this.MaxCacheSize * 1024) / (1024 * 1024);
                if (estimatedMB > 50)
                {
                    errors.Add($"Warning: Large cache size may use ~{estimatedMB}MB of memory");
                }
            }

            return errors;
        }

        /// <summary>
        /// Gets performance recommendations based on current settings.
        /// **NEW**: Provides optimization suggestions for cache configuration
        /// </summary>
        /// <returns>List of performance recommendations</returns>
        public List<string> GetPerformanceRecommendations()
        {
            var recommendations = new List<string>();

            if (!this.EnableMetadataCache)
            {
                recommendations.Add("Enable metadata caching for significantly better performance");
            }
            else
            {
                if (this.MetadataCacheTTLHours < 3)
                {
                    recommendations.Add(
                        "Consider longer cache TTL (6+ hours) for external metadata to reduce API calls"
                    );
                }

                if (this.MaxCacheSize < 1000)
                {
                    recommendations.Add(
                        "Consider larger cache size (2000+ entries) for better hit ratio with large music libraries"
                    );
                }

                if (!this.EnableMemoryPressureAdjustment)
                {
                    recommendations.Add(
                        "Enable memory pressure adjustment for better system performance"
                    );
                }

                if (!this.EnableCacheWarming)
                {
                    recommendations.Add(
                        "Enable cache warming for faster access to frequently played music"
                    );
                }

                if (this.CacheCleanupIntervalMinutes > 15)
                {
                    recommendations.Add(
                        "Consider more frequent cleanup (5-10 minutes) for optimal memory usage"
                    );
                }
            }

            return recommendations;
        }

        #endregion

        #region Cache Configuration Helper

        /// <summary>
        /// Creates a MetadataCacheConfig from current settings.
        /// **NEW**: Converts UI settings to cache configuration object
        /// </summary>
        /// <returns>Cache configuration based on current settings</returns>
        public Services.MetadataCacheConfig CreateCacheConfig()
        {
            if (!this.EnableMetadataCache)
            {
                return new Services.MetadataCacheConfig
                {
                    MaxCacheSize = 0,
                    EnableMemoryPressureAdjustment = false,
                    EnableCacheWarming = false,
                };
            }

            return new Services.MetadataCacheConfig
            {
                TrackMetadataTTL = TimeSpan.FromHours(this.MetadataCacheTTLHours / 2), // Shorter for tracks
                AlbumMetadataTTL = this.MetadataCacheTTL, // User setting
                ExternalMetadataTTL = TimeSpan.FromHours(this.MetadataCacheTTLHours * 2), // Longer for external data
                MaxCacheSize = this.MaxCacheSize,
                CleanupInterval = this.CacheCleanupInterval,
                EnableMemoryPressureAdjustment = this.EnableMemoryPressureAdjustment,
                EnableCacheWarming = this.EnableCacheWarming,
            };
        }

        #endregion

        #region Backward Compatibility

        /// <summary>
        /// Legacy property for backward compatibility.
        /// **DEPRECATED**: Use MaxCacheSize instead.
        /// </summary>
        [Obsolete("Use MaxCacheSize instead")]
        [DefaultValue(100)]
        public int MetadataCacheSize
        {
            get => Math.Min(this.MaxCacheSize, 1000); // Clamp to old maximum
            set
            {
                // Convert old setting to new setting
                this.MaxCacheSize = Math.Max(100, Math.Min(1000, value));
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.MaxCacheSize));
            }
        }

        #endregion
    }
}
