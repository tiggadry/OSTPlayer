// ====================================================================
// FILE: IAudioViewModel.cs (REFERENCE COPY)
// PROJECT: OstPlayer - Reference Interface for Future Extractions
// MODULE: References
// LOCATION: Documentation/References/Interfaces/
// VERSION: 1.1.0
// STATUS: REFERENCE ONLY - NOT USED IN CURRENT BUILD
// ====================================================================
//
// PURPOSE:
// REFERENCE COPY of high-quality interface design for future audio extractions.
// This interface demonstrates excellent separation of concerns and MVVM patterns
// that can be used as inspiration for Step 3+ micro-extractions.
//
// LESSONS LEARNED:
// - Clean interface segregation
// - Comprehensive audio control API
// - Async/await pattern integration
// - Event-driven state notifications
// - Proper IDisposable pattern
//
// FUTURE USE:
// When we're ready for larger extractions, this interface provides:
// - Template for audio ViewModel contracts
// - Guidance for method signatures
// - Event pattern examples
// - Property organization patterns
//
// EXTRACTION READINESS:
// This interface is ready to be used when we reach the audio extraction phase
// of our safe micro-extraction plan.
//
// ====================================================================

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OstPlayer.References.Interfaces
{
    /// <summary>
    /// REFERENCE: Interface contract for audio-related ViewModels.
    /// 
    /// This interface demonstrates excellent design patterns for future use
    /// in our safe micro-extraction process.
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
