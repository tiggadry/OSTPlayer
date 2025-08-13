// ====================================================================
// FILE: IPlaylistViewModel.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: ViewModels
// LOCATION: ViewModels/Audio/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Interface contract for playlist management ViewModels in the OstPlayer project.
// Defines the public API for auto-play functionality, track navigation, retry logic,
// and playlist state management. Part of the critical refactoring to extract
// playlist concerns from the monolithic OstPlayerSidebarViewModel.
//
// FEATURES:
// - Auto-play next track functionality
// - Retry logic for failed playback attempts
// - Track navigation (next, previous, seek to track)
// - Playlist state management and synchronization
// - Integration with IAudioViewModel for seamless playback
//
// DEPENDENCIES:
// - System.Collections.ObjectModel (ObservableCollection support)
// - System (EventHandler, basic types)
// - OstPlayer.Models.TrackListItem (playlist item representation)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle (focused playlist contract)
// - Observer Pattern (playlist state events)
// - Strategy Pattern (retry strategies)
//
// REFACTORING CONTEXT:
// This interface is part of the OstPlayerSidebarViewModel refactoring initiative.
// Enables extraction of auto-play and playlist concerns into specialized ViewModels
// while maintaining clear contracts and testability through interface-based design.
//
// IMPLEMENTATION TARGETS:
// - PlaylistViewModel: Auto-play logic, track navigation, retry mechanisms
// - Main coordinator: Delegates playlist operations to implementing ViewModels
//
// THREAD SAFETY:
// - Interface methods should be thread-safe for UI thread operations
// - Event notifications should support cross-thread invocation
// - Implementations must handle concurrent access appropriately
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF data binding compatible
// - Integration with IAudioViewModel
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial interface definition for playlist ViewModel extraction
// ====================================================================

using System;
using System.Collections.ObjectModel;
using OstPlayer.Models;

namespace OstPlayer.ViewModels.Audio
{
    /// <summary>
    /// Interface contract for playlist-related ViewModels providing auto-play functionality,
    /// track navigation, and retry logic for robust playlist management.
    /// 
    /// Designed to support the extraction of playlist concerns from the monolithic
    /// OstPlayerSidebarViewModel into focused, testable components.
    /// </summary>
    public interface IPlaylistViewModel : IDisposable
    {
        #region Playlist State Properties

        /// <summary>
        /// Gets the current playlist of music tracks.
        /// </summary>
        ObservableCollection<TrackListItem> CurrentPlaylist { get; }

        /// <summary>
        /// Gets the currently playing track in the playlist.
        /// </summary>
        TrackListItem CurrentTrack { get; }

        /// <summary>
        /// Gets the index of the currently playing track.
        /// </summary>
        int CurrentTrackIndex { get; }

        /// <summary>
        /// Gets a value indicating whether there is a next track available.
        /// </summary>
        bool HasNextTrack { get; }

        /// <summary>
        /// Gets a value indicating whether there is a previous track available.
        /// </summary>
        bool HasPreviousTrack { get; }

        /// <summary>
        /// Gets a value indicating whether auto-play is enabled.
        /// </summary>
        bool IsAutoPlayEnabled { get; set; }

        /// <summary>
        /// Gets a value indicating whether auto-play operation is currently in progress.
        /// </summary>
        bool IsAutoPlayingNext { get; }

        #endregion

        #region Retry Configuration

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for failed playback.
        /// </summary>
        int MaxRetryAttempts { get; set; }

        /// <summary>
        /// Gets the current retry attempt count.
        /// </summary>
        int CurrentRetryCount { get; }

        #endregion

        #region Playlist Management Methods

        /// <summary>
        /// Sets the current playlist and resets navigation state.
        /// </summary>
        /// <param name="playlist">Collection of tracks to set as current playlist</param>
        void SetPlaylist(ObservableCollection<TrackListItem> playlist);

        /// <summary>
        /// Selects a specific track in the playlist without starting playback.
        /// </summary>
        /// <param name="track">Track to select</param>
        /// <returns>True if track was found and selected</returns>
        bool SelectTrack(TrackListItem track);

        /// <summary>
        /// Selects a track by file path without starting playback.
        /// </summary>
        /// <param name="filePath">File path of track to select</param>
        /// <returns>True if track was found and selected</returns>
        bool SelectTrackByPath(string filePath);

        /// <summary>
        /// Moves to the next track in the playlist.
        /// </summary>
        /// <returns>Next track or null if at end of playlist</returns>
        TrackListItem MoveToNextTrack();

        /// <summary>
        /// Moves to the previous track in the playlist.
        /// </summary>
        /// <returns>Previous track or null if at beginning of playlist</returns>
        TrackListItem MoveToPreviousTrack();

        #endregion

        #region Auto-Play Methods

        /// <summary>
        /// Handles the event when current track playback ends naturally.
        /// Triggers auto-advance to next track if enabled.
        /// </summary>
        void OnTrackPlaybackEnded();

        /// <summary>
        /// Starts auto-play for the next track with retry logic.
        /// </summary>
        /// <returns>True if auto-play was initiated successfully</returns>
        bool StartAutoPlayNext();

        /// <summary>
        /// Cancels any ongoing auto-play operation.
        /// </summary>
        void CancelAutoPlay();

        #endregion

        #region Events

        /// <summary>
        /// Raised when the current track changes in the playlist.
        /// </summary>
        event EventHandler<TrackListItem> CurrentTrackChanged;

        /// <summary>
        /// Raised when auto-play advances to the next track.
        /// </summary>
        event EventHandler<TrackListItem> AutoPlayAdvanced;

        /// <summary>
        /// Raised when auto-play reaches the end of the playlist.
        /// </summary>
        event EventHandler PlaylistEnded;

        /// <summary>
        /// Raised when track playback fails and retry is attempted.
        /// </summary>
        event EventHandler<PlaybackRetryEventArgs> PlaybackRetryAttempted;

        /// <summary>
        /// Raised when maximum retry attempts are exceeded.
        /// </summary>
        event EventHandler<TrackListItem> PlaybackFailed;

        /// <summary>
        /// Raised when the playlist content changes.
        /// </summary>
        event EventHandler PlaylistChanged;

        #endregion
    }

    /// <summary>
    /// Event arguments for playback retry events.
    /// </summary>
    public class PlaybackRetryEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the track that failed to play.
        /// </summary>
        public TrackListItem Track { get; }

        /// <summary>
        /// Gets the current retry attempt number.
        /// </summary>
        public int RetryAttempt { get; }

        /// <summary>
        /// Gets the maximum retry attempts configured.
        /// </summary>
        public int MaxRetryAttempts { get; }

        /// <summary>
        /// Gets the exception that caused the playback failure.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Initializes a new instance of the PlaybackRetryEventArgs class.
        /// </summary>
        public PlaybackRetryEventArgs(TrackListItem track, int retryAttempt, int maxRetryAttempts, Exception exception)
        {
            Track = track;
            RetryAttempt = retryAttempt;
            MaxRetryAttempts = maxRetryAttempts;
            Exception = exception;
        }
    }
}