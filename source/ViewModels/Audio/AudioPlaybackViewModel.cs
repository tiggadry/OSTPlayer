// ====================================================================
// FILE: AudioPlaybackViewModel.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: ViewModels
// LOCATION: ViewModels/Audio/
// VERSION: 1.2.1
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Specialized ViewModel for audio playback control extracted from the monolithic
// OstPlayerSidebarViewModel. Handles audio engine operations, progress tracking,
// volume control, and playback state management. Now includes async/await support
// for improved UI responsiveness, standardized error handling, and volume persistence.
//
// EXTRACTED RESPONSIBILITIES:
// - Audio playback control (play, pause, stop)
// - Progress tracking and position updates
// - Volume control and audio state management
// - NAudio engine coordination and event handling
// - User interaction support (dragging, seeking)
// - Async audio operations for better UX
// - **NEW**: Standardized error handling and user notifications
// - **NEW**: Volume persistence to settings for user preference retention
//
// FEATURES:
// - Clean separation of audio concerns from UI and metadata logic
// - Interface-based design for testability and loose coupling
// - Event-driven architecture for ViewModel communication
// - Thread-safe operations for UI synchronization
// - Comprehensive error handling for audio operations
// - Async/await playback methods with error handling
// - **NEW**: User-friendly error messages and recovery guidance
// - **NEW**: Volume persistence across plugin sessions via JSON settings
//
// DEPENDENCIES:
// - OstPlayer.Utils.MusicPlaybackService (NAudio wrapper)
// - OstPlayer.Utils.RelayCommand (MVVM command implementation)
// - OstPlayer.ViewModels.Core.ViewModelBase (shared infrastructure)
// - System.Windows.Threading.DispatcherTimer (progress updates)
// - System.Windows.Input (ICommand interface)
// - System.Threading.Tasks (async operations support)
// - OstPlayer.Services.ErrorHandlingService (centralized error handling)
// - **NEW**: OstPlayer main plugin instance (settings access)
//
// DESIGN PATTERNS:
// - Single Responsibility Principle (audio concerns only)
// - Interface Segregation (IAudioViewModel contract)
/// - Observer Pattern (event-driven communication)
/// - Command Pattern (MVVM command binding)
/// - Facade Pattern (simplifies NAudio complexity)
/// - Task-based Asynchronous Pattern (TAP)
/// - **NEW**: Centralized Error Handling Pattern
/// - **NEW**: Settings Persistence Pattern
//
// ERROR HANDLING STRATEGY:
// - ViewModel-level error coordination and user notification
// - Graceful degradation for audio operation failures
// - State recovery and cleanup on errors
// - User-friendly error messaging with action suggestions
// - Comprehensive logging for debugging support
//
// CHANGELOG:
// 2025-08-07 v1.2.1 - Volume persistence: Added settings integration, volume restored on plugin startup, automatic save on volume changes
// 2025-08-07 v1.2.0 - Standardized error handling: Integrated ErrorHandlingService, improved user error experience, enhanced state management
// 2025-08-07 v1.1.0 - Added async/await support: PlayAsync method, ErrorOccurred event, improved error handling
// 2025-08-06 v1.0.0 - Initial extraction from OstPlayerSidebarViewModel
// ====================================================================

using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using OstPlayer.Utils;
using OstPlayer.ViewModels.Core;
using OstPlayer.Services;

namespace OstPlayer.ViewModels.Audio
{
    /// <summary>
    /// Specialized ViewModel for audio playback control and state management.
    /// Extracted from OstPlayerSidebarViewModel to implement Single Responsibility Principle.
    /// 
    /// Handles all audio-related concerns including playback control, progress tracking,
    /// volume management, NAudio engine coordination, and comprehensive error handling.
    /// </summary>
    public class AudioPlaybackViewModel : ViewModelBase, IAudioViewModel
    {
        #region Private Fields

        /// <summary>
        /// NAudio-based service for audio playback operations.
        /// </summary>
        private MusicPlaybackService _playbackService;

        /// <summary>
        /// Timer for real-time progress updates during playback.
        /// </summary>
        private DispatcherTimer _progressTimer;

        /// <summary>
        /// Centralized error handling service for user notifications.
        /// </summary>
        private readonly ErrorHandlingService _errorHandler;

        /// <summary>
        /// Plugin instance for accessing settings and saving configuration.
        /// </summary>
        private readonly OstPlayer _plugin;

        /// <summary>
        /// Flag indicating user is dragging the progress slider.
        /// </summary>
        private bool _isUserDragging = false;

        /// <summary>
        /// Current playback position in seconds.
        /// </summary>
        private double _position = 0;

        /// <summary>
        /// Total track duration in seconds.
        /// </summary>
        private double _duration = 0;

        /// <summary>
        /// Volume level as percentage (0-100).
        /// </summary>
        private double _volume = 50;

        /// <summary>
        /// Audio playback active state.
        /// </summary>
        private bool _isPlaying = false;

        /// <summary>
        /// Audio playback paused state (distinct from stopped).
        /// </summary>
        private bool _isPaused = false;

        /// <summary>
        /// Currently selected track file path.
        /// </summary>
        private string _currentTrackPath = string.Empty;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the AudioPlaybackViewModel class.
        /// Sets up audio engine, progress timer, command bindings, and error handling.
        /// </summary>
        /// <param name="plugin">Plugin instance for accessing settings (optional for backward compatibility)</param>
        public AudioPlaybackViewModel(OstPlayer plugin = null)
        {
            _errorHandler = new ErrorHandlingService();
            _plugin = plugin;
            // Initialization handled by base class Initialize method
        }

        /// <summary>
        /// Initializes audio engine, timer, and commands.
        /// Called by base class constructor for proper initialization order.
        /// </summary>
        protected override void Initialize()
        {
            LoadVolumeFromSettings();
            InitializeAudioEngine();
            InitializeProgressTimer();
            InitializeCommands();
        }

        /// <summary>
        /// Initializes the NAudio-based playback service and subscribes to events.
        /// Sets up event handlers for audio state changes and progress notifications.
        /// </summary>
        private void InitializeAudioEngine()
        {
            try
            {
                _playbackService = new MusicPlaybackService();

                // Subscribe to audio engine events for state synchronization
                _playbackService.PlaybackStarted += OnAudioPlaybackStarted;
                _playbackService.PlaybackPaused += OnAudioPlaybackPaused;
                _playbackService.PlaybackStopped += OnAudioPlaybackStopped;
                _playbackService.PlaybackEnded += OnAudioPlaybackEnded;
                _playbackService.PositionChanged += OnAudioPositionChanged;
                _playbackService.DurationChanged += OnAudioDurationChanged;
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, "Audio Engine Initialization");
                ErrorOccurred?.Invoke(this, "Failed to initialize audio engine. Please restart the application.");
            }
        }

        /// <summary>
        /// Initializes the progress update timer for real-time position tracking.
        /// </summary>
        private void InitializeProgressTimer()
        {
            try
            {
                _progressTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100) // 10 FPS for smooth progress
                };
                _progressTimer.Tick += OnProgressTimerTick;
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, "Progress Timer Initialization");
            }
        }

        /// <summary>
        /// Initializes MVVM commands for audio control.
        /// </summary>
        private void InitializeCommands()
        {
            try
            {
                PlayPauseCommand = new RelayCommand(_ => ExecutePlayPause(), _ => CanExecutePlayPause());
                StopCommand = new RelayCommand(_ => ExecuteStop(), _ => CanExecuteStop());
                MuteCommand = new RelayCommand(_ => ExecuteMute());
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, "Command Initialization");
                ErrorOccurred?.Invoke(this, "Failed to initialize audio controls.");
            }
        }

        #endregion

        #region Public Properties (IAudioViewModel Implementation)

        /// <summary>
        /// Gets a value indicating whether audio is currently playing.
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (SetProperty(ref _isPlaying, value))
                {
                    // Update dependent command states
                    OnPropertyChanged(nameof(CanPlay));
                    OnPropertyChanged(nameof(CanPause));
                    OnPropertyChanged(nameof(CanStop));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether audio is currently paused.
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            private set => SetProperty(ref _isPaused, value);
        }

        /// <summary>
        /// Gets or sets the current playback position in seconds.
        /// Setting the position performs a seek operation.
        /// </summary>
        public double Position
        {
            get => _position;
            set
            {
                // Don't update position if user is currently dragging to prevent conflicts
                if (_isUserDragging)
                {
                    // Just update the internal value for UI binding
                    SetProperty(ref _position, value);
                    OnPropertyChanged(nameof(CurrentTimeDisplay));
                    return;
                }

                if (SetProperty(ref _position, value))
                {
                    OnPropertyChanged(nameof(CurrentTimeDisplay));
                    
                    // Apply seek operation through the audio service
                    try
                    {
                        _playbackService?.SetPosition(value);
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total duration of the current track in seconds.
        /// </summary>
        public double Duration
        {
            get => _duration;
            private set
            {
                if (SetProperty(ref _duration, value))
                {
                    OnPropertyChanged(nameof(DurationDisplay));
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume level (0-100).
        /// Automatically applies volume changes to the audio engine and saves to settings.
        /// </summary>
        public double Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, value))
                {
                    OnPropertyChanged(nameof(VolumeDisplay));
                    try
                    {
                        _playbackService?.SetVolume(value / 100.0); // Convert to 0.0-1.0 range
                        VolumeChanged?.Invoke(this, value);
                        
                        // Save volume to settings for persistence
                        SaveVolumeToSettings(value);
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the currently selected or playing track file path.
        /// </summary>
        public string CurrentTrackPath
        {
            get => _currentTrackPath;
            private set
            {
                if (SetProperty(ref _currentTrackPath, value))
                {
                    OnPropertyChanged(nameof(CanPlay));
                }
            }
        }

        #endregion

        #region Computed Properties for UI Binding

        /// <summary>
        /// Gets a value indicating whether playback can be started or resumed.
        /// </summary>
        public bool CanPlay => !string.IsNullOrEmpty(CurrentTrackPath) && (!IsPlaying || IsPaused);

        /// <summary>
        /// Gets a value indicating whether playback can be paused.
        /// </summary>
        public bool CanPause => IsPlaying && !IsPaused;

        /// <summary>
        /// Gets a value indicating whether playback can be stopped.
        /// </summary>
        public bool CanStop => IsPlaying;

        /// <summary>
        /// Gets the formatted current time string (MM:SS).
        /// </summary>
        public string CurrentTimeDisplay => Position > 0 
            ? TimeSpan.FromSeconds(Position).ToString(@"mm\:ss") 
            : "00:00";

        /// <summary>
        /// Gets the formatted duration string (MM:SS).
        /// </summary>
        public string DurationDisplay => Duration > 0 
            ? TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss") 
            : "--:--";

        /// <summary>
        /// Gets the volume display string (e.g., "75%").
        /// </summary>
        public string VolumeDisplay => $"{(int)Volume}%";

        #endregion

        #region MVVM Commands

        /// <summary>
        /// Gets the command for play/pause toggle functionality.
        /// </summary>
        public ICommand PlayPauseCommand { get; private set; }

        /// <summary>
        /// Gets the command for stopping playback.
        /// </summary>
        public ICommand StopCommand { get; private set; }

        /// <summary>
        /// Gets the command for muting/unmuting audio.
        /// </summary>
        public ICommand MuteCommand { get; private set; }

        #endregion

        #region Command Implementations

        /// <summary>
        /// Executes play/pause toggle logic with error handling.
        /// </summary>
        private void ExecutePlayPause()
        {
            try
            {
                if (IsPlaying && !IsPaused)
                {
                    Pause();
                }
                else
                {
                    if (!string.IsNullOrEmpty(CurrentTrackPath))
                    {
                        // C# 7.3 compatible - explicit null handling
                        double? startPosition = IsPaused ? (double?)Position : null;
                        Play(CurrentTrackPath, startPosition);
                    }
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                ErrorOccurred?.Invoke(this, "Failed to toggle playback. Please try again.");
            }
        }

        /// <summary>
        /// Determines if play/pause command can execute.
        /// </summary>
        private bool CanExecutePlayPause() => !string.IsNullOrEmpty(CurrentTrackPath);

        /// <summary>
        /// Executes stop command with error handling.
        /// </summary>
        private void ExecuteStop()
        {
            try
            {
                Stop();
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                ErrorOccurred?.Invoke(this, "Failed to stop playback.");
            }
        }

        /// <summary>
        /// Determines if stop command can execute.
        /// </summary>
        private bool CanExecuteStop() => IsPlaying;

        /// <summary>
        /// Executes mute/unmute toggle with error handling.
        /// </summary>
        private void ExecuteMute()
        {
            try
            {
                Volume = Volume > 0 ? 0 : 50; // Toggle between current and muted
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                ErrorOccurred?.Invoke(this, "Failed to change volume.");
            }
        }

        #endregion

        #region Public Methods (IAudioViewModel Implementation)

        /// <summary>
        /// Starts playback of the specified track with comprehensive error handling.
        /// </summary>
        /// <param name="trackPath">Full path to the audio file</param>
        /// <param name="startPosition">Optional start position in seconds</param>
        public async Task PlayAsync(string trackPath, double? startPosition = null)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(trackPath))
            {
                ErrorOccurred?.Invoke(this, "No track selected for playback.");
                return;
            }

            if (!File.Exists(trackPath))
            {
                _errorHandler.HandlePlaybackError(new FileNotFoundException("Audio file not found."), trackPath);
                ErrorOccurred?.Invoke(this, "Selected audio file not found. Please check if the file still exists.");
                return;
            }

            try
            {
                CurrentTrackPath = trackPath;
                await _playbackService.PlayAsync(trackPath, startPosition);
                
                if (!_progressTimer.IsEnabled)
                {
                    _progressTimer.Start();
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, trackPath);
                ErrorOccurred?.Invoke(this, "Failed to start audio playback. Please check the audio file and try again.");
                
                // Reset state on error
                CurrentTrackPath = string.Empty;
                IsPlaying = false;
                IsPaused = false;
            }
        }

        /// <summary>
        /// Starts playback of the specified track (synchronous wrapper for backward compatibility).
        /// Uses fire-and-forget pattern to prevent UI thread deadlock.
        /// </summary>
        /// <param name="trackPath">Full path to the audio file</param>
        /// <param name="startPosition">Optional start position in seconds</param>
        public void Play(string trackPath, double? startPosition = null)
        {
            // Use fire-and-forget async pattern instead of GetAwaiter().GetResult() to prevent deadlock
            _ = PlayAsync(trackPath, startPosition);
        }

        /// <summary>
        /// Pauses the current playback with error handling.
        /// </summary>
        public void Pause()
        {
            ThrowIfDisposed();
            
            try
            {
                _playbackService?.Pause();
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                ErrorOccurred?.Invoke(this, "Failed to pause playback.");
            }
        }

        /// <summary>
        /// Stops the current playback and resets position with error handling.
        /// </summary>
        public void Stop()
        {
            ThrowIfDisposed();
            
            try
            {
                _playbackService?.Stop();
                _progressTimer?.Stop();
                Position = 0;
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                ErrorOccurred?.Invoke(this, "Failed to stop playback.");
            }
        }

        /// <summary>
        /// Sets the playback position to the specified time with error handling.
        /// </summary>
        /// <param name="position">Position in seconds</param>
        public void Seek(double position)
        {
            ThrowIfDisposed();
            
            try
            {
                Position = position;
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                ErrorOccurred?.Invoke(this, "Failed to seek to position.");
            }
        }

        /// <summary>
        /// Sets the volume level with error handling.
        /// </summary>
        /// <param name="volume">Volume level (0-100)</param>
        public void SetVolume(double volume)
        {
            ThrowIfDisposed();
            
            try
            {
                Volume = Math.Max(0, Math.Min(100, volume)); // Clamp to valid range
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                ErrorOccurred?.Invoke(this, "Failed to set volume.");
            }
        }

        /// <summary>
        /// Notifies the audio system that the user is dragging the progress slider.
        /// </summary>
        /// <param name="isDragging">True when dragging starts, false when ends</param>
        public void SetUserDragging(bool isDragging)
        {
            ThrowIfDisposed();
            
            try
            {
                _isUserDragging = isDragging;
                _playbackService?.SetUserDragging(isDragging);
                
                if (!isDragging)
                {
                    // When dragging ends, apply the current position from the slider
                    // This ensures the audio position matches the UI slider position
                    _playbackService?.SetPosition(_position);
                    
                    // Force a position update to synchronize UI with actual audio position
                    if (_playbackService != null)
                    {
                        var actualPosition = _playbackService.GetPosition();
                        if (Math.Abs(actualPosition - _position) > 0.1) // Only update if significantly different
                        {
                            // Update the position property without triggering another seek
                            SetProperty(ref _position, actualPosition);
                            OnPropertyChanged(nameof(CurrentTimeDisplay));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
                // Don't show user error for dragging issues as they're minor
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles audio engine playback started event.
        /// </summary>
        private void OnAudioPlaybackStarted(object sender, EventArgs e)
        {
            try
            {
                IsPlaying = true;
                IsPaused = false;
                PlaybackStarted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
            }
        }

        /// <summary>
        /// Handles audio engine playback paused event.
        /// </summary>
        private void OnAudioPlaybackPaused(object sender, EventArgs e)
        {
            try
            {
                IsPaused = true;
                PlaybackPaused?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
            }
        }

        /// <summary>
        /// Handles audio engine playback stopped event.
        /// </summary>
        private void OnAudioPlaybackStopped(object sender, EventArgs e)
        {
            try
            {
                IsPlaying = false;
                IsPaused = false;
                _progressTimer?.Stop();
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
            }
        }

        /// <summary>
        /// Handles audio engine playback ended event.
        /// </summary>
        private void OnAudioPlaybackEnded(object sender, EventArgs e)
        {
            try
            {
                IsPlaying = false;
                IsPaused = false;
                _progressTimer?.Stop();
                PlaybackEnded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
            }
        }

        /// <summary>
        /// Handles audio engine position changed event.
        /// </summary>
        private void OnAudioPositionChanged(object sender, double position)
        {
            try
            {
                if (!_isUserDragging)
                {
                    // Update position from audio engine
                    SetProperty(ref _position, position);
                    OnPropertyChanged(nameof(CurrentTimeDisplay));
                    PositionChanged?.Invoke(this, position);
                }
            }
            catch (Exception ex)
            {
                // Don't propagate position update errors as they're not critical
                System.Diagnostics.Debug.WriteLine($"Position update error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles audio engine duration changed event.
        /// </summary>
        private void OnAudioDurationChanged(object sender, double duration)
        {
            try
            {
                Duration = duration;
                DurationChanged?.Invoke(this, duration);
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
            }
        }

        /// <summary>
        /// Handles progress timer tick for position updates.
        /// </summary>
        private void OnProgressTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (!_isUserDragging && _playbackService != null && IsPlaying)
                {
                    var currentPosition = _playbackService.GetPosition();
                    
                    // Only update if position has changed significantly to reduce UI updates
                    if (Math.Abs(currentPosition - _position) > 0.1)
                    {
                        SetProperty(ref _position, currentPosition);
                        OnPropertyChanged(nameof(CurrentTimeDisplay));
                    }
                }
            }
            catch (Exception)
            {
                // Ignore timer errors to prevent timer stopping
                // Position updates are not critical enough to show user errors
            }
        }

        #endregion

        #region Events (IAudioViewModel Implementation)

        /// <summary>
        /// Raised when playback starts.
        /// </summary>
        public event EventHandler PlaybackStarted;

        /// <summary>
        /// Raised when playback is paused.
        /// </summary>
        public event EventHandler PlaybackPaused;

        /// <summary>
        /// Raised when playback is stopped.
        /// </summary>
        public event EventHandler PlaybackStopped;

        /// <summary>
        /// Raised when a track finishes playing naturally.
        /// </summary>
        public event EventHandler PlaybackEnded;

        /// <summary>
        /// Raised when the playback position changes.
        /// </summary>
        public event EventHandler<double> PositionChanged;

        /// <summary>
        /// Raised when the track duration is determined.
        /// </summary>
        public event EventHandler<double> DurationChanged;

        /// <summary>
        /// Raised when the volume level changes.
        /// </summary>
        public event EventHandler<double> VolumeChanged;

        /// <summary>
        /// Raised when an error occurs during audio operations.
        /// </summary>
        public event EventHandler<string> ErrorOccurred;

        #endregion

        #region Cleanup and Disposal

        /// <summary>
        /// Performs cleanup of audio resources and event subscriptions.
        /// Called by base class disposal pattern.
        /// </summary>
        protected override void Cleanup()
        {
            try
            {
                // Stop timer
                _progressTimer?.Stop();

                // Unsubscribe from audio engine events
                if (_playbackService != null)
                {
                    _playbackService.PlaybackStarted -= OnAudioPlaybackStarted;
                    _playbackService.PlaybackPaused -= OnAudioPlaybackPaused;
                    _playbackService.PlaybackStopped -= OnAudioPlaybackStopped;
                    _playbackService.PlaybackEnded -= OnAudioPlaybackEnded;
                    _playbackService.PositionChanged -= OnAudioPositionChanged;
                    _playbackService.DurationChanged -= OnAudioDurationChanged;

                    // Dispose audio engine
                    _playbackService.Dispose();
                    _playbackService = null;
                }

                // Clear event handlers
                PlaybackStarted = null;
                PlaybackPaused = null;
                PlaybackStopped = null;
                PlaybackEnded = null;
                PositionChanged = null;
                DurationChanged = null;
                VolumeChanged = null;
                ErrorOccurred = null;
            }
            catch (Exception)
            {
                // Ignore cleanup errors during disposal
            }

            base.Cleanup();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the initial volume setting from the plugin's configuration.
        /// </summary>
        private void LoadVolumeFromSettings()
        {
            try
            {
                if (_plugin != null)
                {
                    var settings = _plugin.LoadPluginSettings<OstPlayerSettings>();
                    if (settings != null)
                    {
                        // Set volume without triggering save to avoid circular update
                        _volume = settings.DefaultVolume;
                        OnPropertyChanged(nameof(Volume));
                        OnPropertyChanged(nameof(VolumeDisplay));
                        
                        // Apply volume to audio engine if available
                        _playbackService?.SetVolume(_volume / 100.0);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to default volume if loading fails
                _volume = 50;
                System.Diagnostics.Debug.WriteLine($"Failed to load volume from settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current volume level to the plugin's settings for persistence.
        /// </summary>
        /// <param name="volume">The volume level to save (0-100)</param>
        private void SaveVolumeToSettings(double volume)
        {
            try
            {
                if (_plugin != null)
                {
                    var settings = _plugin.LoadPluginSettings<OstPlayerSettings>() ?? new OstPlayerSettings();
                    settings.DefaultVolume = volume;
                    _plugin.SavePluginSettings(settings);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't interrupt user experience
                System.Diagnostics.Debug.WriteLine($"Failed to save volume to settings: {ex.Message}");
            }
        }

        #endregion
    }
}