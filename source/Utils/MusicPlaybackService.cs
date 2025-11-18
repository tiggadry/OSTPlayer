// ====================================================================
// FILE: MusicPlaybackService.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils
// LOCATION: Utils/
// VERSION: 1.2.1
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Core audio playback engine using NAudio for MP3 file playback.
// Provides comprehensive playback control, progress tracking, volume management,
// and event-driven architecture for UI integration. Now includes async/await
// support for improved UI responsiveness and standardized error handling.
// **v1.2.1**: Enhanced position seeking with validation and edge case handling
//
// FEATURES:
// - MP3 playback with NAudio.Wave
// - Real-time progress tracking (100ms precision)
// - Volume control (0.0 - 1.0 range)
// - Position seeking and duration reporting
// - Playback state management (playing/paused/stopped)
// - Event-driven notifications for UI updates
// - Track transition support with suppressed events
// - **v1.1.0**: Async/await playback operations for better UX
// - **v1.2.0**: Standardized error handling with ErrorHandlingService
// - **v1.2.1**: Enhanced position seeking with boundary validation
//
// DEPENDENCIES:
// - NAudio.Wave (audio playbook engine)
// - System.Windows.Threading.DispatcherTimer (progress updates)
// - System.ComponentModel (IDisposable pattern)
// - System.Threading.Tasks (async operations support)
// - OstPlayer.Services.ErrorHandlingService (centralized error handling)
//
// DESIGN PATTERNS:
// - Observer Pattern (events for state changes)
// - Disposable Pattern (proper resource cleanup)
// - State Machine (playback state management)
// - Task-based Asynchronous Pattern (TAP)
// - **NEW**: Centralized Error Handling Pattern
//
// ERROR HANDLING STRATEGY:
// - Categorized error handling (playback, metadata, system)
// - User-friendly error messages with recovery suggestions
// - Comprehensive logging with context information
// - Graceful degradation for non-critical failures
// - Automatic resource cleanup on exceptions
//
// PERFORMANCE NOTES:
// - Minimal CPU usage during playback
// - Efficient memory usage with streaming
// - Low-latency position updates
// - Optimized for single-file playback
// - **NEW**: Non-blocking delays improve UI responsiveness
//
// LIMITATIONS:
// - Single file playback only (no playlists)
// - MP3 format only (no FLAC, OGG, etc.)
// - Windows-only due to NAudio dependency
// - No audio effects or equalizer
// - No gapless playback between tracks
//
// FUTURE REFACTORING:
// FUTURE: Add support for additional audio formats (FLAC, OGG, WAV)
// FUTURE: Implement gapless playback for seamless transitions
// FUTURE: Add audio effects and equalizer functionality
// FUTURE: Extract interface for cross-platform implementations
// FUTURE: Add playlist queue management
// FUTURE: Implement fade-in/fade-out effects
// FUTURE: Add spectrum analysis and visualization support
// FUTURE: Implement audio normalization
// FUTURE: Add crossfade between tracks
// FUTURE: Support for multi-channel audio
// FUTURE: Migrate more operations to async patterns
// CONSIDER: Plugin architecture for audio effects
// CONSIDER: Integration with Windows audio session management
// IDEA: Real-time audio analysis for beat detection
// IDEA: Integration with audio plugins (VST)
//
// TESTING:
// - Unit tests for playback state transitions
// - Integration tests with various MP3 formats
// - Performance tests for memory usage
// - Stress tests for long playback sessions
// - **NEW**: Async operation tests for proper TAP compliance
// - **NEW**: Position seeking edge case tests
//
// EVENTS:
// - PlaybackStarted: Fired when playback begins
// - PlaybackPaused: Fired when playback is paused
// - PlaybackStopped: Fired when playback stops (manual or natural)
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - NAudio 2.x
// - Windows 7+ (DirectSound/WASAPI)
// - **NEW**: Task-based Asynchronous Pattern support
//
// CHANGELOG:
// 2025-08-07 v1.2.1 - Enhanced position seeking: Added boundary validation, improved edge case handling for end-of-track seeks
// 2025-08-07 v1.2.0 - Standardized error handling: Integrated ErrorHandlingService, improved exception handling, user-friendly error messages
// 2025-08-07 v1.1.0 - Added async/await support: PlayAsync method with Task.Delay, improved UI responsiveness
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive audio playback features
// ====================================================================

using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using NAudio.Wave;
using OstPlayer.Services;

namespace OstPlayer.Utils
{
    /// <summary>
    /// Service class that encapsulates all music playback logic using NAudio.
    /// Provides methods for play, pause, stop, volume, and position control.
    /// Raises events for playback state and progress changes.
    /// Now includes standardized error handling for improved user experience.
    /// </summary>
    public class MusicPlaybackService : IDisposable
    {
        // Private Fields - NAudio Components and State Management
        private IWavePlayer waveOut;

        // NAudio audio file reader for MP3 playback
        // MEMORY: Streams file content, so large files don't load entirely into RAM
        // THREAD SAFETY: Not thread-safe - all access must be on main thread
        private AudioFileReader audioFileReader;

        // Timer for updating playback progress (position)
        // FREQUENCY: 100ms intervals provide smooth UI updates (10 FPS)
        // PERFORMANCE: Dispatcher timer runs on UI thread for direct property binding
        private readonly DispatcherTimer progressTimer;

        // Centralized error handling service
        private readonly ErrorHandlingService errorHandler;

        // Private Fields - Playback State Flags
        // Indicates if playback is currently paused
        // DIFFERENCE: Paused != Stopped (paused retains position, stopped resets to 0)
        // UI IMPACT: Affects play/pause button appearance and behavior
        private bool isPaused = false;

        // Indicates if playback is currently active
        // LIFECYCLE: true from Play() until Stop() or natural end
        // EVENT CORRELATION: Changes trigger UI state updates via events
        private bool isPlaying = false;

        // Flag to distinguish manual stop from natural end
        // PURPOSE: Prevents event firing during programmatic operations
        // USE CASE: User clicking Stop vs track ending naturally
        private bool isManualStop = false;

        // Flag to indicate stopping for track change (should not trigger events)
        // PURPOSE: Suppresses UI events during automatic track transitions
        // WORKFLOW: Set before Stop(), cleared after cleanup
        private bool isStoppingForTrackChange = false;

        // True if user is dragging the progress slider (UI)
        // PURPOSE: Prevents timer from updating position during user interaction
        // UI SYNCHRONIZATION: Avoids slider jumping during drag operations
        private bool isUserDragging = false;

        // Private Fields - Audio Parameters
        // Playback volume (0.0 - 1.0)
        // RANGE: NAudio expects float values between 0.0 (silent) and 1.0 (full volume)
        // DEFAULT: 0.5 (50%) provides reasonable starting volume
        private double volume = 0.5;

        // Path to the currently loaded audio file
        // PURPOSE: Enables resume functionality and track change detection
        // COMPARISON: Used to determine if Play() request is for same file (resume) or new file
        private string currentFilePath = null;

        #region Public Events

        /// <summary>
        /// Fired when playback is stopped.
        /// </summary>
        public event EventHandler PlaybackStopped;

        /// <summary>
        /// Fired when playback is started.
        /// </summary>
        public event EventHandler PlaybackStarted;

        /// <summary>
        /// Fired when playback is paused.
        /// </summary>
        public event EventHandler PlaybackPaused;

        /// <summary>
        /// Fired when playback position changes.
        /// </summary>
        public event EventHandler<double> PositionChanged;

        /// <summary>
        /// Fired when track duration is determined.
        /// </summary>
        public event EventHandler<double> DurationChanged;

        /// <summary>
        /// Fired when current track ends naturally.
        /// </summary>
        public event EventHandler PlaybackEnded;

        #endregion

        /// <summary>
        /// Initializes a new instance of the MusicPlaybackService class.
        /// Sets up the timer for progress updates and error handling service.
        /// </summary>
        public MusicPlaybackService()
        {
            errorHandler = new ErrorHandlingService();

            progressTimer = new DispatcherTimer();
            progressTimer.Interval = TimeSpan.FromMilliseconds(100); // 10 FPS update rate
            progressTimer.Tick += ProgressTimer_Tick;
        }

        /// <summary>
        /// Synchronous wrapper for PlayAsync method to maintain backward compatibility.
        /// Uses fire-and-forget pattern to prevent UI thread deadlock.
        /// </summary>
        /// <param name="filePath">Full path to the audio file to play.</param>
        /// <param name="startPosition">Optional start position in seconds.</param>
        public void Play(string filePath, double? startPosition = null)
        {
            // Use fire-and-forget async pattern instead of GetAwaiter().GetResult() to prevent deadlock
            _ = PlayAsync(filePath, startPosition);
        }

        /// <summary>
        /// Starts playback of the specified audio file. If paused and the same file, resumes playback.
        /// SMART RESUME: Automatically detects pause vs new file scenarios
        /// RESOURCE MANAGEMENT: Cleans up previous playback before starting new
        /// ERROR HANDLING: Exceptions bubble up to caller for UI error display
        /// ASYNC IMPROVEMENT: Uses Task.Delay instead of Thread.Sleep for better UI responsiveness
        /// </summary>
        /// <param name="filePath">Full path to the audio file to play.</param>
        /// <param name="startPosition">Optional start position in seconds.</param>
        public async Task PlayAsync(string filePath, double? startPosition = null)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                // RESUME LOGIC: Check if this is a resume operation for the same file
                // CONDITIONS: Must be paused (not stopped) and same file path
                // POSITION: If startPosition specified, it overrides current position
                if (isPaused && waveOut != null && filePath == currentFilePath)
                {
                    // Resume operation with error handling
                    try
                    {
                        if (startPosition.HasValue && audioFileReader != null)
                        {
                            // SEEK OPERATION: Set playback position before resuming with validation
                            // RANGE CHECK: Validate position is within bounds before seeking
                            var validatedPosition = ValidateSeekPosition(startPosition.Value);
                            audioFileReader.CurrentTime = TimeSpan.FromSeconds(validatedPosition);
                        }

                        // RESUME PLAYBACK: Continue from current or specified position
                        waveOut.Play();
                        isPaused = false;
                        isPlaying = true;
                        isManualStop = false;
                        progressTimer.Start();
                        PlaybackStarted?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    catch (Exception ex)
                    {
                        errorHandler.HandlePlaybackError(ex, filePath);
                        return;
                    }
                }

                // NEW FILE PLAYBACK: Stop any previous playback and release resources
                // RESOURCE CLEANUP: Essential to avoid DirectSound/WASAPI conflicts
                // EVENT SUPPRESSION: Suppress events during track change to avoid UI confusion
                if (waveOut != null || audioFileReader != null)
                {
                    isStoppingForTrackChange = true;
                    Stop(true); // suppressPlaybackStoppedEvent = true
                    isStoppingForTrackChange = false;
                }

                // FILE VALIDATION: Check if file exists before attempting to load
                if (!System.IO.File.Exists(filePath))
                {
                    errorHandler.HandlePlaybackError(
                        new System.IO.FileNotFoundException("Audio file not found."),
                        filePath
                    );
                    return;
                }

                // ASYNC DELAY: Non-blocking delay to avoid file lock issues
                // REASON: Previous AudioFileReader disposal might not be complete
                // IMPROVEMENT: Better UX than Thread.Sleep - doesn't block UI thread
                await Task.Delay(50);

                try
                {
                    // AUDIO INITIALIZATION: Create NAudio objects for playback
                    // ORDER: AudioFileReader first (provides audio stream), then WaveOut (plays stream)
                    audioFileReader = new AudioFileReader(filePath);
                    audioFileReader.Volume = (float)volume;

                    // POSITION SETUP: Set start position if specified with validation
                    // TIMING: Must be done after AudioFileReader creation but before playback
                    if (startPosition.HasValue)
                    {
                        var validatedPosition = ValidateSeekPosition(startPosition.Value);
                        audioFileReader.CurrentTime = TimeSpan.FromSeconds(validatedPosition);
                    }

                    // PLAYBACK DEVICE: WaveOutEvent for non-blocking audio output
                    // THREADING: Handles audio output on background thread
                    waveOut = new WaveOutEvent();
                    waveOut.Init(audioFileReader);
                    waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                    waveOut.Play();

                    // STATE MANAGEMENT: Update internal state and notify UI
                    currentFilePath = filePath;
                    isPlaying = true;
                    isPaused = false;
                    isManualStop = false;
                    progressTimer.Start();

                    // UI NOTIFICATIONS: Inform subscribers about new audio state
                    DurationChanged?.Invoke(this, audioFileReader.TotalTime.TotalSeconds);
                    PlaybackStarted?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    // ERROR RECOVERY: Clean up partial initialization on failure
                    // EXAMPLES: Corrupted file, unsupported format, device busy
                    CleanupResources();
                    errorHandler.HandlePlaybackError(ex, filePath);
                }
            }
            catch (Exception ex)
            {
                // Catch-all error handling for unexpected exceptions
                errorHandler.HandlePlaybackError(ex, filePath);
            }
        }

        /// <summary>
        /// Pauses playback if currently playing.
        /// POSITION PRESERVATION: Maintains current playback position for resume
        /// STATE REQUIREMENTS: Only works if currently playing and not already paused
        /// RESOURCE EFFICIENCY: NAudio pause is lightweight (no resource deallocation)
        /// </summary>
        public void Pause()
        {
            try
            {
                if (isPlaying && !isPaused && waveOut != null)
                {
                    waveOut.Pause();
                    isPaused = true;
                    progressTimer.Stop();
                    PlaybackPaused?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                errorHandler.HandlePlaybackError(ex, currentFilePath ?? "Unknown");
            }
        }

        /// <summary>
        /// Stops playback and releases all resources.
        /// COMPLETE CLEANUP: Disposes NAudio objects and resets all state
        /// POSITION RESET: Unlike pause, stop resets position to beginning
        /// EVENT CONTROL: Can suppress events for seamless track transitions
        /// </summary>
        /// <param name="suppressPlaybackStoppedEvent">If true, suppresses PlaybackStopped event</param>
        public void Stop(bool suppressPlaybackStoppedEvent = false)
        {
            try
            {
                isManualStop = true;
                bool wasPlaying = isPlaying;

                CleanupResources();

                // STATE RESET: Clear all playback state variables
                isPlaying = false;
                isPaused = false;
                currentFilePath = null;

                // UI NOTIFICATION: Inform UI that position has reset
                PositionChanged?.Invoke(this, 0);

                // CONDITIONAL EVENT: Only fire PlaybackStopped if requested and was actually playing
                if (wasPlaying && !suppressPlaybackStoppedEvent)
                {
                    PlaybackStopped?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                // Even stop operations can fail - handle gracefully
                errorHandler.HandlePlaybackError(ex, currentFilePath ?? "Unknown");
            }
        }

        /// <summary>
        /// Cleanly releases all NAudio resources
        /// DISPOSAL ORDER: Event unsubscription, stop, dispose
        /// SAFETY: Null checks prevent exceptions during cleanup
        /// COMPLETENESS: Ensures no DirectSound/WASAPI resources leak
        /// </summary>
        private void CleanupResources()
        {
            try
            {
                // WAVEOUT CLEANUP: Stop playback and release audio device
                if (waveOut != null)
                {
                    try
                    {
                        waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
                        waveOut.Stop();
                        waveOut.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Log cleanup errors but don't propagate them
                        System.Diagnostics.Debug.WriteLine($"WaveOut cleanup error: {ex.Message}");
                    }
                    finally
                    {
                        waveOut = null;
                    }
                }

                // AUDIO READER CLEANUP: Close file handle and release memory
                if (audioFileReader != null)
                {
                    try
                    {
                        audioFileReader.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Log cleanup errors but don't propagate them
                        System.Diagnostics.Debug.WriteLine(
                            $"AudioFileReader cleanup error: {ex.Message}"
                        );
                    }
                    finally
                    {
                        audioFileReader = null;
                    }
                }

                progressTimer?.Stop();
            }
            catch (Exception ex)
            {
                // Last resort error handling for cleanup failures
                System.Diagnostics.Debug.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the playback volume (0.0 - 1.0).
        /// RANGE: 0.0 = silent, 1.0 = maximum volume (no amplification)
        /// REAL-TIME: Applied immediately if audio is currently loaded
        /// PERSISTENCE: Volume setting is remembered for future playback
        /// </summary>
        /// <param name="value">Volume value (0.0 - 1.0).</param>
        public void SetVolume(double value)
        {
            try
            {
                volume = value;

                // IMMEDIATE APPLICATION: Apply to currently loaded audio if available
                if (audioFileReader != null)
                {
                    audioFileReader.Volume = (float)volume;
                }
            }
            catch (Exception ex)
            {
                errorHandler.HandlePlaybackError(ex, currentFilePath ?? "Unknown");
            }
        }

        /// <summary>
        /// Gets the current playback volume (0.0 - 1.0).
        /// CONSISTENCY: Returns the internally stored volume, not NAudio's current value
        /// RELIABILITY: Available even when no audio is loaded
        /// </summary>
        public double GetVolume() => volume;

        /// <summary>
        /// Gets the current playback position in seconds.
        /// PRECISION: NAudio provides sub-second precision
        /// SAFETY: Returns 0 if no audio is loaded (prevents null reference)
        /// REAL-TIME: Reflects actual playback position, not timer-cached value
        /// </summary>
        public double GetPosition()
        {
            try
            {
                return audioFileReader?.CurrentTime.TotalSeconds ?? 0;
            }
            catch (Exception)
            {
                // Return safe default on error
                return 0;
            }
        }

        /// <summary>
        /// Gets the total duration of the loaded audio file in seconds.
        /// AVAILABILITY: Only valid after successful audio file loading
        /// METADATA: Duration comes from audio file headers (ID3, etc.)
        /// SAFETY: Returns 0 if no audio is loaded
        /// </summary>
        public double GetDuration()
        {
            try
            {
                return audioFileReader?.TotalTime.TotalSeconds ?? 0;
            }
            catch (Exception)
            {
                // Return safe default on error
                return 0;
            }
        }

        /// <summary>
        /// Sets the playback position (in seconds) with boundary validation.
        /// SEEK OPERATION: Allows jumping to any position within the audio file
        /// RANGE: Validates and clamps position to valid range [0, duration-buffer]
        /// UI NOTIFICATION: Fires PositionChanged event to update UI immediately
        /// EDGE CASE HANDLING: Prevents seeking too close to the end which causes playback issues
        /// </summary>
        /// <param name="seconds">Position in seconds.</param>
        public void SetPosition(double seconds)
        {
            try
            {
                if (audioFileReader != null)
                {
                    var validatedPosition = ValidateSeekPosition(seconds);
                    audioFileReader.CurrentTime = TimeSpan.FromSeconds(validatedPosition);

                    // Fire position changed event with the validated position
                    PositionChanged?.Invoke(this, validatedPosition);
                }
            }
            catch (Exception ex)
            {
                errorHandler.HandlePlaybackError(ex, currentFilePath ?? "Unknown");
            }
        }

        /// <summary>
        /// Validates and clamps seek position to prevent edge case issues.
        /// BOUNDARY CHECKING: Ensures position is within valid playback range
        /// EDGE CASE PREVENTION: Adds buffer before end to prevent playback issues
        /// SAFETY: Always returns a valid position that NAudio can handle
        /// </summary>
        /// <param name="requestedPosition">The requested position in seconds</param>
        /// <returns>Validated position that is safe for seeking</returns>
        private double ValidateSeekPosition(double requestedPosition)
        {
            if (audioFileReader == null)
                return 0;

            var duration = audioFileReader.TotalTime.TotalSeconds;

            // Clamp to minimum of 0
            if (requestedPosition < 0)
                return 0;

            // Add a small buffer before the end to prevent issues with seeking to the very end
            // NAudio can have issues when seeking to the exact end of a file
            const double endBuffer = 0.1; // 100ms buffer before end
            var maxValidPosition = Math.Max(0, duration - endBuffer);

            // If seeking beyond the valid range, clamp to maximum valid position
            if (requestedPosition >= maxValidPosition && duration > endBuffer)
            {
                return maxValidPosition;
            }

            return requestedPosition;
        }

        /// <summary>
        /// Sets whether the user is currently dragging the progress slider (UI).
        /// Prevents timer from updating position during drag.
        /// PURPOSE: Avoids slider position conflicts during user interaction
        /// COORDINATION: UI should call this during MouseDown/MouseUp events
        /// </summary>
        /// <param name="dragging">True if user is dragging.</param>
        public void SetUserDragging(bool dragging)
        {
            isUserDragging = dragging;
        }

        /// <summary>
        /// Timer tick handler: updates playback position if not dragging.
        /// FREQUENCY: Called every 100ms while playing
        /// CONDITION: Only updates position if user is not dragging slider
        /// PURPOSE: Provides smooth progress bar animation during playback
        /// </summary>
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (audioFileReader != null && waveOut != null && !isUserDragging)
                {
                    PositionChanged?.Invoke(this, audioFileReader.CurrentTime.TotalSeconds);
                }
            }
            catch (Exception)
            {
                // Ignore timer errors to prevent timer stopping
                // Position updates are not critical enough to show user errors
            }
        }

        /// <summary>
        /// Handles the end of playback (either manual stop or natural end).
        /// DECISION LOGIC: Determines whether to fire PlaybackEnded or ignore
        /// EVENT ROUTING: Routes to appropriate handler based on stop reason
        /// AUTO-PLAY TRIGGER: Natural end triggers automatic track progression
        /// </summary>
        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                // Handle any exception that might have caused the playback to stop
                if (e.Exception != null)
                {
                    errorHandler.HandlePlaybackError(e.Exception, currentFilePath ?? "Unknown");
                    return;
                }

                // CAPTURE STATE: Read current state before any modifications
                bool wasManualStop = isManualStop;
                bool wasStoppingForTrackChange = isStoppingForTrackChange;
                bool wasPlaying = isPlaying;

                // RESET FLAGS: Clear manual stop flag for next operation
                isManualStop = false;

                // SKIP EVENT CONDITIONS: Don't fire events for internal operations
                // 1. wasManualStop: User explicitly stopped playback (Stop button)
                // 2. wasStoppingForTrackChange: Internal cleanup during track switching
                // 3. !wasPlaying: Spurious event from NAudio (defensive programming)
                if (wasManualStop || wasStoppingForTrackChange || !wasPlaying)
                {
                    return;
                }

                // NATURAL END PROCESSING: Track reached end, trigger auto-play
                // SEQUENCE: Cleanup first, then reset state, then fire event

                // RESOURCE CLEANUP: Clean up NAudio resources immediately
                CleanupResources();

                // STATE RESET: Clear playback state variables
                isPlaying = false;
                isPaused = false;
                currentFilePath = null;

                // UI NOTIFICATION: Reset position display to beginning
                PositionChanged?.Invoke(this, 0);

                // AUTO-PLAY TRIGGER: Fire PlaybackEnded for automatic track progression
                // HANDLER: ViewModel typically handles this to advance to next track
                PlaybackEnded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                errorHandler.HandlePlaybackError(ex, currentFilePath ?? "Unknown");
            }
        }

        /// <summary>
        /// Releases all resources used by the service.
        /// IMPLEMENTATION: IDisposable pattern for proper resource cleanup
        /// USAGE: Called automatically by 'using' statements or manually
        /// SAFETY: Stop() handles cleanup, unsubscribe prevents memory leaks
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop();
                progressTimer.Tick -= ProgressTimer_Tick;
            }
            catch (Exception)
            {
                // Ignore disposal errors
            }
        }
    }
}
