// ====================================================================
// FILE: IMetadataViewModel.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: ViewModels
// LOCATION: ViewModels/Metadata/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Interface contract for metadata-related ViewModels in the OstPlayer project.
// Defines the public API for MP3 metadata handling, external metadata integration,
// cache management, and metadata coordination. Part of the critical refactoring to
// extract metadata concerns from the monolithic OstPlayerSidebarViewModel.
//
// FEATURES:
// - MP3 metadata loading and management (ID3 tags)
// - External metadata integration (Discogs, MusicBrainz)
// - Metadata caching and persistence
// - Multi-source metadata merging and coordination
// - Track-level and game-level metadata separation
//
// DEPENDENCIES:
// - System.Windows.Media.Imaging (BitmapImage for cover art)
// - System (EventHandler, basic types)
// - OstPlayer.Models (metadata model types)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle (focused metadata contract)
// - Observer Pattern (metadata change events)
// - Strategy Pattern (metadata source strategies)
//
// REFACTORING CONTEXT:
// This interface is part of the OstPlayerSidebarViewModel refactoring initiative.
// Enables extraction of metadata concerns into specialized ViewModels while
// maintaining clear contracts and testability through interface-based design.
//
// IMPLEMENTATION TARGETS:
// - Mp3MetadataViewModel: Local ID3 tag handling
// - DiscogsMetadataViewModel: External metadata integration
// - MetadataManagerViewModel: Multi-source coordination
// - Main coordinator: Delegates metadata operations to implementing ViewModels
//
// THREAD SAFETY:
// - Interface methods should be thread-safe for UI thread operations
// - Event notifications should support cross-thread invocation
// - Implementations must handle async operations appropriately
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF data binding compatible
// - TagLibSharp integration support
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial interface definition for metadata ViewModel extraction
// ====================================================================

using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OstPlayer.Models;

namespace OstPlayer.ViewModels.Metadata
{
    /// <summary>
    /// Interface contract for MP3 metadata ViewModels providing ID3 tag loading,
    /// metadata management, and track information handling.
    /// 
    /// Designed to support the extraction of MP3 metadata concerns from the monolithic
    /// OstPlayerSidebarViewModel into focused, testable components.
    /// </summary>
    public interface IMp3MetadataViewModel : IDisposable
    {
        #region Track Metadata Properties

        /// <summary>
        /// Gets the cover art image loaded from MP3 metadata.
        /// </summary>
        BitmapImage TrackCover { get; }

        /// <summary>
        /// Gets the track title from ID3 metadata.
        /// </summary>
        string TrackTitle { get; }

        /// <summary>
        /// Gets the performing artist from ID3 metadata.
        /// </summary>
        string TrackArtist { get; }

        /// <summary>
        /// Gets the album name from ID3 metadata.
        /// </summary>
        string TrackAlbum { get; }

        /// <summary>
        /// Gets the release year from ID3 metadata.
        /// </summary>
        string TrackYear { get; }

        /// <summary>
        /// Gets the musical genre from ID3 metadata.
        /// </summary>
        string TrackGenre { get; }

        /// <summary>
        /// Gets the comment text from ID3 metadata.
        /// </summary>
        string TrackComment { get; }

        /// <summary>
        /// Gets the formatted duration string from ID3 metadata.
        /// </summary>
        string TrackDuration { get; }

        /// <summary>
        /// Gets the track number within album from ID3 metadata.
        /// </summary>
        uint TrackNumber { get; }

        /// <summary>
        /// Gets the total tracks in album from ID3 metadata.
        /// </summary>
        uint TotalTracks { get; }

        #endregion

        #region State Properties

        /// <summary>
        /// Gets the file path of the currently loaded track.
        /// </summary>
        string CurrentTrackPath { get; }

        /// <summary>
        /// Gets a value indicating whether metadata is currently being loaded.
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Gets a value indicating whether metadata has been successfully loaded.
        /// </summary>
        bool HasMetadata { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads metadata for the specified track file.
        /// </summary>
        /// <param name="trackPath">Full path to the MP3 file</param>
        /// <returns>Task representing the async operation</returns>
        Task LoadTrackMetadataAsync(string trackPath);

        /// <summary>
        /// Clears all current metadata.
        /// </summary>
        void ClearMetadata();

        /// <summary>
        /// Gets the duration of the track for progress slider.
        /// </summary>
        /// <returns>Duration in seconds or 0 if unavailable</returns>
        double GetTrackDurationSeconds();

        #endregion

        #region Events

        /// <summary>
        /// Raised when metadata loading begins.
        /// </summary>
        event EventHandler MetadataLoadingStarted;

        /// <summary>
        /// Raised when metadata loading completes successfully.
        /// </summary>
        event EventHandler<TrackMetadataModel> MetadataLoaded;

        /// <summary>
        /// Raised when metadata loading fails.
        /// </summary>
        event EventHandler<Exception> MetadataLoadingFailed;

        /// <summary>
        /// Raised when metadata is cleared.
        /// </summary>
        event EventHandler MetadataCleared;

        #endregion
    }

    /// <summary>
    /// Interface contract for external metadata ViewModels providing Discogs integration,
    /// external API management, and game-level metadata handling.
    /// 
    /// Designed to support the extraction of external metadata concerns from the monolithic
    /// OstPlayerSidebarViewModel into focused, testable components.
    /// </summary>
    public interface IDiscogsMetadataViewModel : IDisposable
    {
        #region Discogs Metadata Properties

        /// <summary>
        /// Gets the current Discogs metadata.
        /// </summary>
        DiscogsMetadataModel DiscogsMetadata { get; }

        /// <summary>
        /// Gets the game-level cached Discogs metadata.
        /// </summary>
        DiscogsMetadataModel GameDiscogsMetadata { get; }

        #endregion

        #region State Properties

        /// <summary>
        /// Gets the currently selected game ID.
        /// </summary>
        Guid CurrentGameId { get; }

        /// <summary>
        /// Gets a value indicating whether Discogs metadata is currently being loaded.
        /// </summary>
        bool IsLoadingDiscogs { get; }

        /// <summary>
        /// Gets a value indicating whether cached metadata exists for current game.
        /// </summary>
        bool HasCachedMetadata { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads Discogs metadata for the specified game.
        /// </summary>
        /// <param name="gameId">Game ID to load metadata for</param>
        /// <param name="gameName">Game name for search queries</param>
        /// <returns>Task representing the async operation</returns>
        Task LoadDiscogsMetadataAsync(Guid gameId, string gameName);

        /// <summary>
        /// Refreshes Discogs metadata by clearing cache and reloading.
        /// </summary>
        /// <param name="gameId">Game ID to refresh metadata for</param>
        /// <param name="gameName">Game name for search queries</param>
        /// <returns>Task representing the async operation</returns>
        Task RefreshDiscogsMetadataAsync(Guid gameId, string gameName);

        /// <summary>
        /// Loads cached Discogs metadata for the specified game.
        /// </summary>
        /// <param name="gameId">Game ID to load cached metadata for</param>
        /// <returns>True if cached metadata was found and loaded</returns>
        bool LoadCachedDiscogsMetadata(Guid gameId);

        /// <summary>
        /// Clears all Discogs metadata.
        /// </summary>
        void ClearDiscogsMetadata();

        #endregion

        #region Events

        /// <summary>
        /// Raised when Discogs metadata loading begins.
        /// </summary>
        event EventHandler DiscogsLoadingStarted;

        /// <summary>
        /// Raised when Discogs metadata loading completes successfully.
        /// </summary>
        event EventHandler<DiscogsMetadataModel> DiscogsMetadataLoaded;

        /// <summary>
        /// Raised when Discogs metadata loading fails.
        /// </summary>
        event EventHandler<Exception> DiscogsLoadingFailed;

        /// <summary>
        /// Raised when cached Discogs metadata is loaded.
        /// </summary>
        event EventHandler<DiscogsMetadataModel> CachedDiscogsMetadataLoaded;

        /// <summary>
        /// Raised when user needs to select from multiple Discogs results.
        /// </summary>
        event EventHandler<DiscogsSelectionRequestedEventArgs> DiscogsSelectionRequested;

        #endregion
    }

    /// <summary>
    /// Interface contract for metadata manager ViewModels providing coordination
    /// between multiple metadata sources and unified metadata management.
    /// 
    /// Designed to support metadata coordination and cache management extracted
    /// from the monolithic OstPlayerSidebarViewModel.
    /// </summary>
    public interface IMetadataManagerViewModel : IDisposable
    {
        #region Coordination Properties

        /// <summary>
        /// Gets the MP3 metadata ViewModel.
        /// </summary>
        IMp3MetadataViewModel Mp3Metadata { get; }

        /// <summary>
        /// Gets the Discogs metadata ViewModel.
        /// </summary>
        IDiscogsMetadataViewModel DiscogsMetadata { get; }

        #endregion

        #region State Properties

        /// <summary>
        /// Gets a value indicating whether any metadata is currently being loaded.
        /// </summary>
        bool IsLoadingAnyMetadata { get; }

        /// <summary>
        /// Gets a value indicating whether MP3 metadata section is visible.
        /// </summary>
        bool IsMp3MetadataVisible { get; set; }

        /// <summary>
        /// Gets a value indicating whether Discogs metadata section is visible.
        /// </summary>
        bool IsDiscogsMetadataVisible { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all metadata for the specified track and game.
        /// </summary>
        /// <param name="trackPath">Path to the track file</param>
        /// <param name="gameId">Game ID for external metadata</param>
        /// <param name="gameName">Game name for search queries</param>
        /// <returns>Task representing the async operation</returns>
        Task LoadAllMetadataAsync(string trackPath, Guid gameId, string gameName);

        /// <summary>
        /// Clears all metadata from all sources.
        /// </summary>
        void ClearAllMetadata();

        /// <summary>
        /// Resets only track-level metadata while preserving game-level data.
        /// </summary>
        void ResetTrackMetadata();

        #endregion

        #region Events

        /// <summary>
        /// Raised when any metadata loading state changes.
        /// </summary>
        event EventHandler MetadataLoadingStateChanged;

        /// <summary>
        /// Raised when metadata visibility settings change.
        /// </summary>
        event EventHandler MetadataVisibilityChanged;

        #endregion
    }

    /// <summary>
    /// Event arguments for Discogs selection requests.
    /// </summary>
    public class DiscogsSelectionRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the list of Discogs search results to choose from.
        /// </summary>
        public System.Collections.Generic.List<DiscogsMetadataModel> Results { get; }

        /// <summary>
        /// Gets the Discogs API token for detailed requests.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Gets or sets the selected Discogs metadata result.
        /// </summary>
        public DiscogsMetadataModel SelectedResult { get; set; }

        /// <summary>
        /// Initializes a new instance of the DiscogsSelectionRequestedEventArgs class.
        /// </summary>
        public DiscogsSelectionRequestedEventArgs(System.Collections.Generic.List<DiscogsMetadataModel> results, string token)
        {
            Results = results;
            Token = token;
        }
    }
}