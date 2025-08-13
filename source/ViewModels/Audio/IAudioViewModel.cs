// ====================================================================
// FILE: IAudioViewModel.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: ViewModels
// LOCATION: ViewModels/Audio/
// VERSION: 1.1.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Interface contract for audio-related ViewModels in the OstPlayer project.
// Defines the public API for audio playbook control, progress tracking, and
// volume management. Part of the critical refactoring to extract audio concerns
// from the monolithic OstPlayerSidebarViewModel. Now includes async/await 
// support for improved UI responsiveness.
//
// FEATURES:
// - Audio playback control interface (play, pause, stop)
// - Progress tracking and position management
// - Volume control and mute functionality
// - Track selection and playlist navigation
// - Event-driven audio state notifications
// - **NEW**: Async audio operations for non-blocking UI
//
// DEPENDENCIES:
// - System.Windows.Input (ICommand for MVVM)
// - System (EventHandler, TimeSpan, basic types)
// - System.Threading.Tasks (async operations support)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle (focused audio contract)
// - Command Pattern (audio control commands)
// - Observer Pattern (audio state events)
// - **NEW**: Task-based Asynchronous Pattern (TAP)
//
// REFACTORING CONTEXT:
// This interface is part of the OstPlayerSidebarViewModel refactoring initiative.
// Enables extraction of audio concerns into specialized ViewModels while maintaining
// clear contracts and testability through interface-based design.
//
// IMPLEMENTATION TARGETS:
// - AudioPlaybackViewModel: Core playback control and state
// - PlaylistViewModel: Track navigation and auto-play logic
// - Main coordinator: Delegates audio operations to implementing ViewModels
//
// THREAD SAFETY:
// - Interface methods should be thread-safe for UI thread operations
// - Event notifications should support cross-thread invocation
// - Implementations must handle concurrent access appropriately
// - **NEW**: Async methods must properly handle thread context
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF data binding compatible
// - NAudio integration support
// - **NEW**: Task-based Asynchronous Pattern support
//
// CHANGELOG:
// 2025-08-07 v1.1.0 - Added async/await support: PlayAsync method, ErrorOccurred event
// 2025-08-06 v1.0.0 - Initial interface definition for audio ViewModel extraction
// ====================================================================

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OstPlayer.ViewModels.Audio
{
    /// <summary>
    /// Interface contract for audio-related ViewModels providing playback control,
    /// progress tracking, and volume management functionality.
    /// 
    /// Designed to support the extraction of audio concerns from the monolithic
    /// OstPlayerSidebarViewModel into focused, testable components.
    /// </summary>
    public interface IAudioViewModel : IDisposable
    {
        #region Playback Control Properties

        /// <summary>
        /// Gets a value indicating whether audio is currently playing.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Gets a value indicating whether audio is currently paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Gets the current playback position in seconds.
        /// </summary>
        double Position { get; set; }

        /// <summary>
        /// Gets the total duration of the current track in seconds.
        /// </summary>
        double Duration { get; }

        /// <summary>
        /// Gets or sets the volume level (0-100).
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// Gets the currently selected or playing track file path.
        /// </summary>
        string CurrentTrackPath { get; }

        #endregion

        #region Computed Properties for UI Binding

        /// <summary>
        /// Gets a value indicating whether playback can be started or resumed.
        /// </summary>
        bool CanPlay { get; }

        /// <summary>
        /// Gets a value indicating whether playback can be paused.
        /// </summary>
        bool CanPause { get; }

        /// <summary>
        /// Gets a value indicating whether playback can be stopped.
        /// </summary>
        bool CanStop { get; }

        /// <summary>
        /// Gets the formatted current time string (MM:SS).
        /// </summary>
        string CurrentTimeDisplay { get; }

        /// <summary>
        /// Gets the formatted duration string (MM:SS).
        /// </summary>
        string DurationDisplay { get; }

        /// <summary>
        /// Gets the volume display string (e.g., "75%").
        /// </summary>
        string VolumeDisplay { get; }

        #endregion

        #region MVVM Commands

        /// <summary>
        /// Gets the command for play/pause toggle functionality.
        /// </summary>
        ICommand PlayPauseCommand { get; }

        /// <summary>
        /// Gets the command for stopping playback.
        /// </summary>
        ICommand StopCommand { get; }

        /// <summary>
        /// Gets the command for muting/unmuting audio.
        /// </summary>
        ICommand MuteCommand { get; }

        #endregion

        #region Playback Control Methods

        /// <summary>
        /// Starts playback of the specified track (async version).
        /// </summary>
        /// <param name="trackPath">Full path to the audio file</param>
        /// <param name="startPosition">Optional start position in seconds</param>
        Task PlayAsync(string trackPath, double? startPosition = null);

        /// <summary>
        /// Starts playback of the specified track.
        /// </summary>
        /// <param name="trackPath">Full path to the audio file</param>
        /// <param name="startPosition">Optional start position in seconds</param>
        void Play(string trackPath, double? startPosition = null);

        /// <summary>
        /// Pauses the current playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops the current playback and resets position.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets the playback position to the specified time.
        /// </summary>
        /// <param name="position">Position in seconds</param>
        void Seek(double position);

        /// <summary>
        /// Sets the volume level.
        /// </summary>
        /// <param name="volume">Volume level (0-100)</param>
        void SetVolume(double volume);

        #endregion

        #region User Interaction Support

        /// <summary>
        /// Notifies the audio system that the user is dragging the progress slider.
        /// Prevents automatic position updates during user interaction.
        /// </summary>
        /// <param name="isDragging">True when dragging starts, false when ends</param>
        void SetUserDragging(bool isDragging);

        #endregion

        #region Events

        /// <summary>
        /// Raised when playback starts.
        /// </summary>
        event EventHandler PlaybackStarted;

        /// <summary>
        /// Raised when playback is paused.
        /// </summary>
        event EventHandler PlaybackPaused;

        /// <summary>
        /// Raised when playback is stopped.
        /// </summary>
        event EventHandler PlaybackStopped;

        /// <summary>
        /// Raised when a track finishes playing naturally.
        /// </summary>
        event EventHandler PlaybackEnded;

        /// <summary>
        /// Raised when the playback position changes.
        /// </summary>
        event EventHandler<double> PositionChanged;

        /// <summary>
        /// Raised when the track duration is determined.
        /// </summary>
        event EventHandler<double> DurationChanged;

        /// <summary>
        /// Raised when the volume level changes.
        /// </summary>
        event EventHandler<double> VolumeChanged;

        /// <summary>
        /// Raised when an error occurs during audio operations.
        /// </summary>
        event EventHandler<string> ErrorOccurred;

        #endregion
    }
}