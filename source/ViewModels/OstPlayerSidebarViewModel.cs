// ====================================================================
// FILE: OstPlayerSidebarViewModel.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: ViewModels
// LOCATION: ViewModels/
// VERSION: 1.2.1
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Main ViewModel for the OstPlayer sidebar interface, implementing MVVM pattern.
// Manages game selection, music file discovery, metadata loading, playback control,
// and integration with external services (Discogs, MusicBrainz). Enhanced with
// comprehensive Discogs metadata refresh capabilities, improved cache management,
// complete XAML Command Binding refactoring for consistent MVVM architecture,
// and volume persistence across plugin sessions.
//
// FEATURES:
// - Game library browsing and filtering
// - Music file discovery and listing
// - Metadata extraction from MP3 files
// - Discogs metadata integration with caching
// - Discogs metadata refresh and cache invalidation
// - Music playback with progress tracking
// - Volume control and playlist management
// - Auto-play next track functionality
// - Metadata visibility toggling
// - Persistent metadata caching with timestamps
// - Game-level metadata persistence across track changes
// - Complete XAML Command Binding implementation
// - **NEW**: Volume persistence to settings for user preference retention
//
// COMMAND BINDING ARCHITECTURE:
// - All user interactions handled through RelayCommand instances
// - Consistent command pattern implementation across all UI elements
// - Proper IsEnabled binding through ViewModel properties
// - Clean separation between UI events and business logic
// - Minimal code-behind with UI-specific operations only
//
// REFRESH CAPABILITIES:
// - Load vs Refresh command distinction
// - Cache invalidation and JSON file management
// - Force API refresh for outdated metadata
// - Timestamp tracking for cache freshness
// - User-controlled metadata updates
// - Seamless transition between load and refresh modes
//
// CACHE MANAGEMENT:
// - Game-level Discogs metadata persistence
// - Track-level metadata separation
// - JSON file storage and retrieval
// - Cache invalidation strategies
// - Timestamp-based freshness tracking
// - Cross-session metadata persistence
//
// DEPENDENCIES:
// - NAudio.Wave (audio playback)
// - Playnite.SDK (game database access)
// - OstPlayer.Clients.DiscogsClient (external metadata)
// - OstPlayer.Utils.Mp3MetadataReader (local metadata)
// - OstPlayer.Utils.MusicPlaybackService (audio engine)
// - OstPlayer.Utils.MetadataJsonSaver (cache persistence)
// - OstPlayer.Utils.RelayCommand (command pattern implementation)
// - System.Windows.Threading (UI updates)
// - Newtonsoft.Json (metadata serialization)
//
// DESIGN PATTERNS:
// - MVVM (Model-View-ViewModel)
// - Command Pattern (RelayCommand for UI actions)
// - Observer Pattern (INotifyPropertyChanged)
// - Repository Pattern (for metadata caching)
// - Strategy Pattern (for metadata loading and refresh)
// - Cache-Aside Pattern (for metadata persistence)
//
// PERFORMANCE NOTES:
// - Lazy loading of game music files
// - Metadata caching to avoid repeated file reads
// - Background loading of Discogs metadata
// - Efficient ObservableCollection updates
// - Timer-based progress updates (100ms intervals)
// - Game-level cache to prevent redundant API calls
// - Selective cache invalidation for refresh operations
// - Command execution optimization through proper CanExecute implementation
//
// METADATA ARCHITECTURE:
// - Track-Level: MP3 metadata (title, artist, cover, etc.)
// - Game-Level: Discogs metadata (album info, release details)
// - Persistent: JSON cache files for cross-session storage
// - Separation: Track changes preserve game-level metadata
//
// REFRESH WORKFLOW:
// 1. User triggers refresh command
// 2. Clear in-memory game-level cache
// 3. Delete JSON cache file
// 4. Reload from Discogs API
// 5. Update UI and save new cache
// 6. Preserve during track changes within same game
//
// COMMAND ARCHITECTURE:
// - PlayPauseCommand: Toggle playback state with proper UI binding
// - StopCommand: Stop playback with IsEnabled state management
// - RefreshDiscogsMetadataCommand: Cache invalidation and API refresh
// - PlaySelectedTrackCommand: Direct track selection and playback
// - All commands use RelayCommand with inline lambda implementations
//
// LIMITATIONS:
// - Only supports MP3 files currently
// - Single-threaded metadata loading (UI blocking)
// - No batch operations for multiple games
// - Limited error recovery for playback issues
// - Hard-coded retry logic (3 attempts)
//
// FUTURE REFACTORING:
// FUTURE: Extract metadata operations to dedicated service
// FUTURE: Implement async/await pattern for all I/O operations
// FUTURE: Add support for additional audio formats (FLAC, OGG)
// FUTURE: Implement proper dependency injection container
// FUTURE: Add comprehensive error handling with user notifications
// FUTURE: Extract game filtering to separate service
// FUTURE: Implement background task manager for metadata loading
// FUTURE: Add playlist management and saving capabilities
// FUTURE: Implement proper cancellation tokens for async operations
// FUTURE: Add audio effects and equalizer support
// FUTURE: Implement batch metadata refresh for multiple games
// FUTURE: Add automatic cache expiration and freshness checking
// FUTURE: Implement metadata conflict resolution UI
// FUTURE: Add metadata source prioritization and weighting
// FUTURE: Extract cache management to separate service
// FUTURE: Implement metadata backup and restore functionality
// FUTURE: Add user preferences for refresh behavior
// FUTURE: Implement progressive metadata loading for large libraries
// FUTURE: Implement proper CanExecuteChanged notifications in RelayCommand
// FUTURE: Extract remaining event handlers to InputBindings
// CONSIDER: Splitting into multiple smaller ViewModels
// CONSIDER: Using reactive extensions (Rx.NET) for event handling
// CONSIDER: Implementing metadata versioning and migration
// CONSIDER: Adding metadata quality scoring and validation
// IDEA: Integration with Last.fm or other music services
// IDEA: Automatic playlist generation based on game genres
// IDEA: Cross-fade between tracks
// IDEA: Visualization/spectrum analyzer
// IDEA: Community-driven metadata correction system
// IDEA: Machine learning for metadata quality improvement
//
// TESTING:
// - Unit tests for playback state management
// - Integration tests for metadata loading and refresh
// - Cache management and invalidation tests
// - Command execution and CanExecute validation tests
// - UI automation tests for user interactions
// - Performance tests for large game libraries
// - API integration tests for Discogs client
// - JSON serialization and persistence tests
//
// USAGE EXAMPLES:
// var viewModel = new OstPlayerSidebarViewModel(plugin, preselectedGame);
// viewModel.SelectedGame = gameFromDatabase;
// await viewModel.LoadDiscogsMetadataCommand.ExecuteAsync(null);
// await viewModel.RefreshDiscogsMetadataCommand.ExecuteAsync(null);
// viewModel.PlayPauseCommand.Execute(null);
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Playnite SDK 6.x
// - NAudio 2.x
// - Newtonsoft.Json 13.x
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with full MVVM pattern and features
// 2025-08-06 v1.1.0 - Added Discogs metadata refresh capabilities, cache invalidation, and improved metadata persistence
// 2025-08-06 v1.2.0 - Completed XAML Command Binding refactoring, eliminated Click event handlers, implemented consistent command pattern
// 2025-08-07 v1.2.1 - Volume persistence: Added settings integration for volume persistence, restored on plugin startup, automatic save on changes
// ====================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAudio.Wave;
using OstPlayer.Clients;
using OstPlayer.Models;
using OstPlayer.Utils;
using OstPlayer.Utils.Helpers;
using Playnite.SDK.Models;

namespace OstPlayer.ViewModels {
    /// <summary>
    /// Main ViewModel for the OstPlayer sidebar interface implementing comprehensive MVVM pattern.
    /// Manages game selection, music discovery, playback control, and external metadata integration.
    /// Inherits from ObservableObject to provide INotifyPropertyChanged implementation.
    /// Reference: https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/data-binding-overview
    /// </summary>
    public class OstPlayerSidebarViewModel : ObservableObject {
        #region Private Fields and Constants

        /// <summary>
        /// Reference to the main plugin instance for accessing Playnite API and services.
        /// Essential for game database access and plugin lifecycle management.
        /// </summary>
        private readonly OstPlayer plugin;

        /// <summary>
        /// NAudio-based service for audio playback operations (play, pause, stop, volume control).
        /// Handles all audio engine operations and provides event notifications for UI updates.
        /// See: <see cref="MusicPlaybackService"/> for implementation details
        /// </summary>
        private MusicPlaybackService playbackService;

        /// <summary>
        /// WPF timer for real-time progress updates during audio playback.
        /// Updates every 100ms to provide smooth progress bar animation without excessive CPU usage.
        /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatchertimer
        /// </summary>
        private DispatcherTimer progressTimer;

        /// <summary>
        /// Flag indicating auto-play operation is in progress (prevents position reset during track transitions).
        /// Critical for seamless playlist playback experience.
        /// </summary>
        private bool isAutoPlayingNext = false;

        /// <summary>
        /// Flag indicating user is currently dragging the progress slider (prevents timer position updates).
        /// Ensures smooth user interaction without UI conflicts during seek operations.
        /// </summary>
        private bool isUserDragging = false;

        /// <summary>
        /// Current retry attempt count for failed playback operations.
        /// Used with MaxPlayRetries for automatic error recovery during auto-play.
        /// </summary>
        private int playRetryCount = 0;

        /// <summary>
        /// Maximum number of retry attempts for failed playback operations.
        /// Prevents infinite retry loops while providing reasonable error recovery.
        /// </summary>
        private const int MaxPlayRetries = 3;

        #endregion

        #region Collection Properties

        /// <summary>
        /// Observable collection of games currently displayed in the UI (filtered from allGamesWithMusic).
        /// Bound to ComboBox ItemsSource for game selection functionality.
        /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1
        /// </summary>
        private ObservableCollection<Game> games;

        /// <summary>
        /// Master list of all games that contain music files (unfiltered).
        /// Source for filtering operations and contains complete game database results.
        /// </summary>
        private List<Game> allGamesWithMusic;

        /// <summary>
        /// Observable collection of music tracks for the currently selected game.
        /// Bound to ListBox ItemsSource for track listing display.
        /// Contains <see cref="TrackListItem"/> instances with essential track information.
        /// </summary>
        private ObservableCollection<TrackListItem> musicFiles;

        #endregion

        #region Selection State Fields

        /// <summary>
        /// Currently selected game from the games collection.
        /// Triggers music file loading and metadata operations when changed.
        /// Maps to Playnite.SDK.Models.Game entity from game database.
        /// </summary>
        private Game selectedGame;

        /// <summary>
        /// File path of currently selected music file for playback operations.
        /// Used by audio engine and metadata loading services.
        /// </summary>
        private string selectedMusicFile;

        #endregion

        #region Playback State Fields

        /// <summary>
        /// Indicates whether audio playback is currently active.
        /// Affects UI button states and auto-play logic.
        /// </summary>
        private bool isPlaying = false;

        /// <summary>
        /// Indicates whether playback is currently paused (different from stopped).
        /// Allows resume functionality and proper UI state management.
        /// </summary>
        private bool isPaused = false;

        /// <summary>
        /// Current playback position in seconds for progress tracking.
        /// Updated by timer during playback and by user during seek operations.
        /// </summary>
        private double position = 0;

        /// <summary>
        /// Total track duration in seconds for progress bar maximum value.
        /// Loaded from metadata or directly from audio file analysis.
        /// </summary>
        private double duration = 0;

        /// <summary>
        /// Current volume level as percentage (0-100) for user interface.
        /// Converted to 0.0-1.0 range for NAudio playback service.
        /// </summary>
        private double volume = 50; // Default to 50% volume

        #endregion

        #region UI State Fields

        /// <summary>
        /// Status message displayed to user about current operations or errors.
        /// Provides feedback for loading, errors, and system state changes.
        /// </summary>
        private string statusText = "Select a game to see available music files";

        /// <summary>
        /// Display text for currently playing track including play/pause state.
        /// Format: "Playing: TrackName" or "Paused: TrackName"
        /// </summary>
        private string currentTrack = "";

        /// <summary>
        /// Visibility state for MP3 metadata section in UI.
        /// Allows users to hide/show sections based on preference.
        /// </summary>
        private bool isMp3MetadataVisible = true;

        /// <summary>
        /// Visibility state for Discogs metadata section in UI.
        /// Controls display of external metadata information.
        /// </summary>
        private bool isDiscogsMetadataVisible = true;

        #endregion

        #region Track-Level Metadata Fields

        /// <summary>
        /// Cover art image for currently selected track loaded from MP3 metadata.
        /// BitmapImage format for direct WPF data binding compatibility.
        /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.bitmapimage
        /// </summary>
        private BitmapImage trackCover;

        /// <summary>Track title from ID3 metadata.</summary>
        private string trackTitle;

        /// <summary>Performing artist from ID3 metadata.</summary>
        private string trackArtist;

        /// <summary>Album name from ID3 metadata (with redundancy handling).</summary>
        private string trackAlbum;

        /// <summary>Release year from ID3 metadata.</summary>
        private string trackYear;

        /// <summary>Musical genre from ID3 metadata.</summary>
        private string trackGenre;

        /// <summary>Comment text from ID3 metadata.</summary>
        private string trackComment;

        /// <summary>Formatted duration string from ID3 metadata.</summary>
        private string trackDuration;

        /// <summary>Track number within album from ID3 metadata.</summary>
        private uint trackNumber;

        /// <summary>Total tracks in album from ID3 metadata.</summary>
        private uint totalTracks;

        #endregion

        #region Game-Level Metadata Fields

        /// <summary>
        /// Discogs metadata for currently selected track (track-specific).
        /// Updated when track selection changes or metadata is loaded.
        /// </summary>
        private DiscogsMetadataModel discogsMetadata;

        /// <summary>
        /// Discogs metadata cached at game level (persists across track changes within same game).
        /// Prevents redundant API calls and provides consistent album-level information.
        /// Restored to discogsMetadata when switching tracks within the same game.
        /// </summary>
        private DiscogsMetadataModel gameDiscogsMetadata;

        /// <summary>
        /// Game to preselect after loading completes.
        /// Stored from constructor parameter for delayed loading.
        /// </summary>
        private Game preselectGameForLoading;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the ViewModel with plugin reference and optional game preselection.
        /// Sets up all services, timers, commands, and loads initial data.
        /// Optimized for non-blocking construction to prevent UI deadlock.
        /// </summary>
        /// <param name="plugin">Main plugin instance for API access (required)</param>
        /// <param name="preselectGame">Optional game to select initially (typically from context)</param>
        /// <exception cref="ArgumentNullException">Thrown when plugin parameter is null</exception>
        public OstPlayerSidebarViewModel(OstPlayer plugin, Game preselectGame = null) {
            // Validate required plugin dependency
            this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));

            // Initialize core services and infrastructure (lightweight operations)
            InitializePlaybackService();   // NAudio audio engine setup
            InitializeTimer();            // Progress update timer configuration
            InitializeCommands();         // MVVM command binding setup

            // Initialize empty collections for immediate UI binding
            Games = new ObservableCollection<Game>();
            MusicFiles = new ObservableCollection<TrackListItem>();
            allGamesWithMusic = new List<Game>();

            // Set initial status
            StatusText = "Click to load games...";

            // Connect to plugin settings for configuration access
            SettingsViewModel = GetSettingsViewModel();

            // Store preselect game for later loading
            preselectGameForLoading = preselectGame;

            // Don't load games automatically in constructor - prevents UI blocking
            // LoadGamesAsync will be called from View's Loaded event
        }

        #endregion

        #region Public Properties for Data Binding

        /// <summary>
        /// Observable collection of games with music files for ComboBox binding.
        /// Filtered subset of allGamesWithMusic based on user search input.
        /// </summary>
        public ObservableCollection<Game> Games {
            get => games;
            set { games = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Observable collection of music tracks for ListBox binding.
        /// Contains TrackListItem instances with display-optimized track information.
        /// </summary>
        public ObservableCollection<TrackListItem> MusicFiles {
            get => musicFiles;
            set { musicFiles = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Currently selected game triggering music file enumeration and metadata operations.
        /// Property change triggers OnGameSelectionChanged() for cascading updates.
        /// </summary>
        public Game SelectedGame {
            get => selectedGame;
            set {
                selectedGame = value;
                OnPropertyChanged();
                OnGameSelectionChanged(); // Cascade to load music files and metadata
            }
        }

        /// <summary>
        /// File path of currently selected music file for playback operations.
        /// Property change triggers OnMusicFileSelectionChanged() and updates CanPlayPause.
        /// </summary>
        public string SelectedMusicFile {
            get => selectedMusicFile;
            set {
                selectedMusicFile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanPlayPause)); // Update command availability
                OnMusicFileSelectionChanged(); // Load track metadata
            }
        }

        /// <summary>
        /// Audio playback active state affecting UI button states and auto-play logic.
        /// Property change triggers updates to CanStop, PlayPauseButtonContent, and PlayPauseButtonToolTip.
        /// </summary>
        public bool IsPlaying {
            get => isPlaying;
            set {
                isPlaying = value;
                OnPropertyChanged();
                // Cascade updates to dependent computed properties
                OnPropertyChanged(nameof(CanStop));
                OnPropertyChanged(nameof(PlayPauseButtonContent));
                OnPropertyChanged(nameof(PlayPauseButtonToolTip));
            }
        }

        /// <summary>
        /// Audio playback paused state (distinct from stopped) for resume functionality.
        /// Affects play/pause button appearance and tooltip text.
        /// </summary>
        public bool IsPaused {
            get => isPaused;
            set {
                isPaused = value;
                OnPropertyChanged();
                // Update UI elements that depend on pause state
                OnPropertyChanged(nameof(PlayPauseButtonContent));
                OnPropertyChanged(nameof(PlayPauseButtonToolTip));
            }
        }

        /// <summary>
        /// Current playback position in seconds for progress slider binding.
        /// Updated by timer during playback and by user during seek operations.
        /// Property change triggers CurrentTime computed property update.
        /// </summary>
        public double Position {
            get => position;
            set {
                position = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentTime)); // Update time display
            }
        }

        /// <summary>
        /// Total track duration in seconds for progress slider maximum and duration display.
        /// Loaded from metadata or direct audio file analysis.
        /// Property change triggers DurationTime computed property update.
        /// </summary>
        public double Duration {
            get => duration;
            set {
                duration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DurationTime)); // Update duration display
            }
        }

        /// <summary>
        /// Volume level as percentage (0-100) for slider binding and display.
        /// Automatically converted to 0.0-1.0 range for NAudio when set.
        /// Property change triggers VolumeDisplay computed property update and saves to settings.
        /// </summary>
        public double Volume {
            get => volume;
            set {
                volume = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VolumeDisplay)); // Update percentage display
                playbackService?.SetVolume(value / 100.0); // Apply to audio engine (0.0-1.0 range)

                // Save volume to settings for persistence
                SaveVolumeToSettings(value);
            }
        }

        /// <summary>
        /// Status message for user feedback about operations, loading, and errors.
        /// Displayed in status area of UI for user awareness.
        /// </summary>
        public string StatusText {
            get => statusText;
            set { statusText = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Currently playing track display text with play/pause state indication.
        /// Format examples: "Playing: TrackName", "Paused: TrackName", or empty when stopped.
        /// </summary>
        public string CurrentTrack {
            get => currentTrack;
            set { currentTrack = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Visibility state for MP3 metadata section allowing user control over UI complexity.
        /// Property change triggers Mp3MetadataToggleText computed property update.
        /// </summary>
        public bool IsMp3MetadataVisible {
            get => isMp3MetadataVisible;
            set {
                isMp3MetadataVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Mp3MetadataToggleText)); // Update toggle button text
            }
        }

        /// <summary>
        /// Visibility state for Discogs metadata section for external data display control.
        /// Property change triggers DiscogsMetadataToggleText computed property update.
        /// </summary>
        public bool IsDiscogsMetadataVisible {
            get => isDiscogsMetadataVisible;
            set {
                isDiscogsMetadataVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiscogsMetadataToggleText)); // Update toggle button text
            }
        }

        // Metadata properties with concise documentation due to straightforward nature
        /// <summary>
        /// Cover art image for currently selected track loaded from MP3 metadata.
        /// BitmapImage format for direct WPF data binding compatibility.
        /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.bitmapimage
        /// </summary>
        public BitmapImage TrackCover { get => trackCover; set { trackCover = value; OnPropertyChanged(); } }
        
        /// <summary>Track title from ID3 metadata.</summary>
        public string TrackTitle { get => trackTitle; set { trackTitle = value; OnPropertyChanged(); } }
        
        /// <summary>Performing artist from ID3 metadata.</summary>
        public string TrackArtist { get => trackArtist; set { trackArtist = value; OnPropertyChanged(); } }
        
        /// <summary>Album name from ID3 metadata.</summary>
        public string TrackAlbum { get => trackAlbum; set { trackAlbum = value; OnPropertyChanged(); } }
        
        /// <summary>Release year from ID3 metadata.</summary>
        public string TrackYear { get => trackYear; set { trackYear = value; OnPropertyChanged(); } }
        
        /// <summary>Musical genre from ID3 metadata.</summary>
        public string TrackGenre { get => trackGenre; set { trackGenre = value; OnPropertyChanged(); } }
        
        /// <summary>Comment text from ID3 metadata.</summary>
        public string TrackComment { get => trackComment; set { trackComment = value; OnPropertyChanged(); } }
        
        /// <summary>Formatted duration string from ID3 metadata.</summary>
        public string TrackDuration { get => trackDuration; set { trackDuration = value; OnPropertyChanged(); } }
        
        /// <summary>Track number within album from ID3 metadata.</summary>
        public uint TrackNumber { get => trackNumber; set { trackNumber = value; OnPropertyChanged(); } }
        
        /// <summary>Total tracks in album from ID3 metadata.</summary>
        public uint TotalTracks { get => totalTracks; set { totalTracks = value; OnPropertyChanged(); } }
        
        /// <summary>Discogs metadata for currently selected track.</summary>
        public DiscogsMetadataModel DiscogsMetadata { get => discogsMetadata; set { discogsMetadata = value; OnPropertyChanged(); } }

        #endregion

        #region Computed Properties for UI Binding

        /// <summary>Volume percentage display string (e.g., "75%").</summary>
        public string VolumeDisplay => VolumeHelper.FormatPercentage(Volume);

        /// <summary>Current position formatted as MM:SS or "00:00" when not playing.</summary>
        public string CurrentTime => TimeHelper.FormatTime(Position);

        /// <summary>Total duration formatted as MM:SS or "--:--" when unknown.</summary>
        public string DurationTime => TimeHelper.FormatTimeWithFallback(Duration, false);

        /// <summary>Command availability: true when a music file is selected.</summary>
        public bool CanPlayPause => !string.IsNullOrEmpty(SelectedMusicFile);

        /// <summary>Command availability: true when playback is active.</summary>
        public bool CanStop => IsPlaying;

        /// <summary>Play/pause button symbol: pause (?) when playing, play (?) when stopped/paused.</summary>
        public string PlayPauseButtonContent => UIHelper.GetPlayPauseButtonSymbol(IsPlaying, IsPaused);

        /// <summary>Play/pause button tooltip based on current state.</summary>
        public string PlayPauseButtonToolTip => UIHelper.GetPlayPauseTooltip(IsPlaying, IsPaused);

        /// <summary>MP3 metadata toggle button text based on current visibility.</summary>
        public string Mp3MetadataToggleText => UIHelper.GetToggleText("MP3 metadata", IsMp3MetadataVisible);

        /// <summary>Discogs metadata toggle button text based on current visibility.</summary>
        public string DiscogsMetadataToggleText => UIHelper.GetToggleText("Discogs metadata", IsDiscogsMetadataVisible);

        /// <summary>Settings ViewModel for configuration access and binding.</summary>
        public OstPlayerSettingsViewModel SettingsViewModel { get; private set; }

        #endregion

        #region MVVM Commands

        /// <summary>
        /// Command for play/pause toggle button binding.
        /// </summary>
        public ICommand PlayPauseCommand { get; private set; }

        /// <summary>
        /// Command for stop button binding.
        /// </summary>
        public ICommand StopCommand { get; private set; }

        /// <summary>
        /// Command for MP3 metadata visibility toggle.
        /// </summary>
        public ICommand Mp3MetadataToggleCommand { get; private set; }

        /// <summary>
        /// Command for Discogs metadata visibility toggle.
        /// </summary>
        public ICommand DiscogsMetadataToggleCommand { get; private set; }

        /// <summary>
        /// Command for hiding metadata sections with parameter.
        /// </summary>
        public ICommand HideMetadataSectionCommand { get; private set; }

        /// <summary>
        /// Command for loading Discogs metadata asynchronously.
        /// </summary>
        public ICommand LoadDiscogsMetadataCommand { get; private set; }

        /// <summary>
        /// Command for showing track cover in preview window.
        /// </summary>
        public ICommand ShowTrackCoverCommand { get; private set; }

        /// <summary>
        /// Command for showing Discogs cover in preview window.
        /// </summary>
        public ICommand ShowDiscogsCoverCommand { get; private set; }

        /// <summary>
        /// Command for refreshing Discogs metadata asynchronously.
        /// </summary>
        public ICommand RefreshDiscogsMetadataCommand { get; private set; }

        /// <summary>
        /// Command for playing the selected track from the list.
        /// </summary>
        public ICommand PlaySelectedTrackCommand { get; private set; }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Initializes the NAudio-based playback service and subscribes to audio events.
        /// Sets up event handlers for playback state changes and progress updates.
        /// Critical for audio engine functionality and UI synchronization.
        /// </summary>
        private void InitializePlaybackService() {
            playbackService = new MusicPlaybackService();

            // Load volume from settings
            LoadVolumeFromSettings();

            // Subscribe to playback events for UI state synchronization
            playbackService.PlaybackStarted += (s, e) => { IsPlaying = true; IsPaused = false; };
            playbackService.PlaybackPaused += (s, e) => { IsPaused = true; };
            playbackService.PlaybackStopped += (s, e) => { IsPlaying = false; IsPaused = false; };
            playbackService.PositionChanged += (s, pos) => { Position = pos; };
            playbackService.DurationChanged += (s, dur) => { Duration = dur; };
            playbackService.PlaybackEnded += (s, e) => OnPlaybackEnded(); // Auto-play next track
        }

        /// <summary>
        /// Initializes the WPF timer for real-time progress updates during playback.
        /// 100ms interval provides smooth progress without excessive CPU usage.
        /// Timer only updates position when user is not dragging the slider.
        /// </summary>
        private void InitializeTimer() {
            progressTimer = new DispatcherTimer();
            progressTimer.Interval = TimeSpan.FromMilliseconds(100); // 10 FPS for smooth progress
            progressTimer.Tick += (s, e) => UpdatePosition(); // Update progress bar
        }

        /// <summary>
        /// Initializes all MVVM commands with appropriate execute and canExecute logic.
        /// Uses RelayCommand for delegate-based command implementation.
        /// Commands provide the bridge between UI interactions and ViewModel logic.
        /// Updated to support async operations properly.
        /// </summary>
        private void InitializeCommands() {
            // Use async command wrapper to prevent UI deadlock
            PlayPauseCommand = new Utils.RelayCommand(async _ => await PlayPauseAsync());
            StopCommand = new Utils.RelayCommand(_ => Stop());
            Mp3MetadataToggleCommand = new Utils.RelayCommand(_ => IsMp3MetadataVisible = !IsMp3MetadataVisible);
            DiscogsMetadataToggleCommand = new Utils.RelayCommand(_ => IsDiscogsMetadataVisible = !IsDiscogsMetadataVisible);
            HideMetadataSectionCommand = new Utils.RelayCommand(HideMetadataSection);
            LoadDiscogsMetadataCommand = new Utils.RelayCommand(async _ => await LoadDiscogsMetadataAsync());
            ShowTrackCoverCommand = new Utils.RelayCommand(_ => ShowTrackCover());
            ShowDiscogsCoverCommand = new Utils.RelayCommand(_ => ShowDiscogsCover());

            // NEW COMMANDS FOR XAML BINDING REFACTORING - using inline lambdas with async support
            RefreshDiscogsMetadataCommand = new Utils.RelayCommand(async _ => {
                if (SelectedGame == null) {
                    ShowError("Please select a game first.");
                    return;
                }
                // Clear existing metadata to force refresh
                DiscogsMetadata = null;
                // Load fresh metadata from Discogs API
                await LoadDiscogsMetadataAsync();
            });

            PlaySelectedTrackCommand = new Utils.RelayCommand(async parameter => {
                if (parameter is TrackListItem selectedItem) {
                    await PlaySelectedMusicFromListBoxAsync(selectedItem);
                }
            });
        }

        #endregion

        #region Public Methods (View Interface)

        /// <summary>
        /// Plays selected track from ListBox interaction (double-click or Enter key).
        /// Synchronous wrapper for backward compatibility.
        /// </summary>
        /// <param name="selectedItem">TrackListItem from ListBox selection</param>
        public void PlaySelectedMusicFromListBox(TrackListItem selectedItem) {
            // Use fire-and-forget async pattern to avoid blocking UI
            _ = PlaySelectedMusicFromListBoxAsync(selectedItem);
        }

        /// <summary>
        /// Plays selected track from ListBox interaction (double-click or Enter key).
        /// Stops current playback, updates selection, and starts new track.
        /// Provides direct track selection and immediate playback functionality.
        /// Now async to prevent UI freezing.
        /// </summary>
        /// <param name="selectedItem">TrackListItem from ListBox selection</param>
        public async Task PlaySelectedMusicFromListBoxAsync(TrackListItem selectedItem) {
            if (selectedItem != null) {
                Stop(); // Ensure clean state
                SelectedMusicFile = selectedItem.FilePath; // Update selection (triggers metadata loading)
                try {
                    await playbackService.PlayAsync(selectedItem.FilePath, 0); // Start from beginning
                    CurrentTrack = $"Playing: {Path.GetFileNameWithoutExtension(selectedItem.FilePath)}";
                }
                catch (Exception ex) {
                    StatusText = $"Error playing music: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Sets user dragging state for progress slider to prevent timer conflicts.
        /// When dragging ends, applies the new position to the audio engine.
        /// Essential for smooth seek operation without UI glitches.
        /// </summary>
        /// <param name="dragging">True when user starts dragging, false when releasing</param>
        public void SetUserDragging(bool dragging) {
            isUserDragging = dragging;
            playbackService?.SetUserDragging(dragging); // Notify audio service
            if (!dragging) {
                playbackService?.SetPosition(Position); // Apply seek position
            }
        }

        /// <summary>
        /// Filters the games collection based on search text for ComboBox functionality.
        /// Updates Games property with filtered results while preserving original data.
        /// Case-insensitive search across game names for user convenience.
        /// </summary>
        /// <param name="searchText">User input for filtering games</param>
        public void FilterGames(string searchText) {
            if (allGamesWithMusic == null)
                return;

            if (string.IsNullOrWhiteSpace(searchText)) {
                // No filter - show all games
                Games = new ObservableCollection<Game>(allGamesWithMusic);
            }
            else {
                // Apply case-insensitive name filter
                var filteredGames = allGamesWithMusic
                    .Where(g => g.Name.ToLower().Contains(searchText.ToLower()))
                    .ToList();
                Games = new ObservableCollection<Game>(filteredGames);
            }
        }

        /// <summary>
        /// Finds game by exact name match for ComboBox text input functionality.
        /// Supports typing game names directly in editable ComboBox.
        /// </summary>
        /// <param name="gameName">Exact game name to locate</param>
        /// <returns>Matching Game instance or null if not found</returns>
        public Game FindGameByName(string gameName) {
            return allGamesWithMusic?.FirstOrDefault(g =>
                string.Equals(g.Name, gameName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Initiates loading of games with music files.
        /// Called by View after UI is fully loaded to prevent constructor blocking.
        /// </summary>
        public async Task InitializeAsync() {
            await LoadGamesAsync(preselectGameForLoading);
        }
        #endregion

        #region Private Methods (Internal Logic)

        /// <summary>
        /// Handles game selection changes by resetting state and loading new game data.
        /// Cascading operation: resets metadata ? loads cached Discogs data ? loads music files.
        /// Critical for maintaining consistent state during game transitions.
        /// </summary>
        private void OnGameSelectionChanged() {
            // Reset all metadata and playback state for clean transition
            SelectedMusicFile = null;
            ResetMetadata();
            Position = 0;
            Duration = 0;

            // Try to load cached Discogs metadata for the selected game
            // This preserves external metadata across plugin sessions
            LoadCachedDiscogsMetadata();

            // Load music files for the new game selection
            LoadMusicFiles();
        }

        /// <summary>
        /// Handles music file selection changes by loading track-specific metadata.
        /// Resets position for new tracks but preserves position during auto-play transitions.
        /// Ensures metadata consistency and proper UI state management.
        /// </summary>
        private void OnMusicFileSelectionChanged() {
            // Reset position when selecting a different track (but not during auto-play)
            if (!string.IsNullOrEmpty(SelectedMusicFile) && !isAutoPlayingNext) {
                Position = 0; // Start from beginning for manual selection
                LoadSelectedTrackMetadata(); // Load ID3 tags and metadata
            }
        }

        /// <summary>
        /// Resets all metadata properties to null/default values.
        /// Complete reset including both track-level and game-level metadata.
        /// Used during game selection changes for clean state transitions.
        /// </summary>
        private void ResetMetadata() {
            // Reset track-level metadata
            TrackCover = null;
            TrackTitle = null;
            TrackArtist = null;
            TrackAlbum = null;
            TrackYear = null;
            TrackGenre = null;
            TrackDuration = null;
            TrackComment = null;
            DiscogsMetadata = null;
            TrackNumber = 0;
            TotalTracks = 0;

            // Reset game-level metadata cache
            gameDiscogsMetadata = null;
        }

        /// <summary>
        /// Loads all games with music files from Playnite database asynchronously.
        /// Prevents UI blocking during initial construction.
        /// </summary>
        /// <param name="preselectGame">Optional game to select after loading</param>
        private async Task LoadGamesAsync(Game preselectGame = null) {
            try {
                await Task.Run(() => {
                    try {
                        // Validate Playnite API availability
                        if (plugin?.PlayniteApi?.Database?.Games == null) {
                            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                                StatusText = "Error: Cannot access Playnite database";
                            });
                            return;
                        }

                        // Query database for games with music files
                        var gamesWithMusic = plugin.PlayniteApi.Database.Games.Where(g => {
                            try {
                                var gameMusicFiles = MusicFileHelper.GetGameMusicFiles(plugin.PlayniteApi, g);
                                return gameMusicFiles != null && gameMusicFiles.Count > 0;
                            }
                            catch {
                                return false; // Skip games that cause errors
                            }
                        })
                        .OrderBy(g => g.Name) // Alphabetical sorting for user convenience
                        .ToList();

                        // Update UI on main thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            // Update both master and filtered collections
                            allGamesWithMusic = gamesWithMusic; // Unfiltered master list
                            Games = new ObservableCollection<Game>(gamesWithMusic); // Initial filtered view

                            // Provide user feedback based on results
                            if (Games.Count == 0) {
                                StatusText = "No games with MP3 files found. Make sure games have .mp3 files in " +
                                           "ExtraMetadata/games/{GameId}/Music Files/ folder";
                            }
                            else {
                                StatusText = $"Found {Games.Count} games with MP3 music files";
                            }

                            // Apply preselection if provided
                            if (preselectGame != null) {
                                SelectedGame = Games?.FirstOrDefault(g => g.Id == preselectGame.Id);
                            }
                        });
                    }
                    catch (Exception ex) {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            StatusText = $"Error loading games: {ex.Message}";
                        });
                    }
                });
            }
            catch (Exception ex) {
                StatusText = $"Critical error during game loading: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads all games with music files from Playnite database.
        /// Filters games to only include those with MP3 files in expected directory structure.
        /// Updates both master list and filtered collection for UI binding.
        /// DEPRECATED: Use LoadGamesAsync() instead to prevent UI blocking.
        /// </summary>
        private void LoadGames() {
            // Redirect to async version for backward compatibility
            _ = LoadGamesAsync();
        }

        /// <summary>
        /// Loads music files for the currently selected game using MusicFileHelper.
        /// Creates TrackListItem instances with metadata preview for UI display.
        /// Handles both successful loading and error scenarios with user feedback.
        /// </summary>
        private void LoadMusicFiles() {
            if (SelectedGame == null) {
                // Clear collection and show instructional message
                MusicFiles = new ObservableCollection<TrackListItem>();
                StatusText = "Select a game to see available music files";
                return;
            }

            try {
                // Get MP3 files for selected game from file system
                var files = MusicFileHelper.GetGameMusicFiles(plugin.PlayniteApi, SelectedGame);
                var items = new List<TrackListItem>();

                // Process each file to create UI-optimized track items
                foreach (var file in files) {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var metadata = Mp3MetadataReader.ReadMetadata(file); // Quick metadata read

                    items.Add(new TrackListItem {
                        TrackTitle = metadata?.Title ?? fileName, // Fallback to filename
                        TrackDuration = metadata?.Duration,
                        FilePath = file,
                        TrackNumber = metadata?.TrackNumber ?? 0,
                        TotalTracks = metadata?.TotalTracks ?? 0,
                    });
                }

                // Update UI collection with new track items
                MusicFiles = new ObservableCollection<TrackListItem>(items);

                // Provide user feedback based on results
                if (MusicFiles.Count == 0) {
                    StatusText = $"No music files found for {SelectedGame.Name}";
                }
                else {
                    StatusText = $"Found {MusicFiles.Count} music files for {SelectedGame.Name}";
                }
            }
            catch (Exception ex) {
                StatusText = $"Error loading music files: {ex.Message}";
                MusicFiles = new ObservableCollection<TrackListItem>(); // Ensure clean state
            }
        }

        /// <summary>
        /// Loads comprehensive metadata for the currently selected track.
        /// Resets only track-level metadata while preserving game-level Discogs data.
        /// Handles both ID3 tag extraction and duration loading for progress slider.
        /// </summary>
        private void LoadSelectedTrackMetadata() {
            ResetTrackMetadata(); // Only reset track-level metadata

            if (SelectedGame == null || string.IsNullOrEmpty(SelectedMusicFile))
                return;

            // Load metadata from MP3 file using TagLibSharp
            var metadata = Mp3MetadataReader.ReadMetadata(SelectedMusicFile);
            if (metadata == null)
                return;

            // Populate track metadata properties for UI binding
            TrackCover = metadata.Cover;
            TrackTitle = metadata.Title;
            TrackArtist = metadata.Artist;
            TrackAlbum = metadata.Album; // Includes redundancy handling
            TrackYear = metadata.Year;
            TrackGenre = metadata.Genre;
            TrackComment = metadata.Comment;
            TrackDuration = metadata.Duration;
            TrackNumber = metadata.TrackNumber;
            TotalTracks = metadata.TotalTracks;

            // Restore Discogs metadata from game-level cache
            // This preserves external metadata across track changes within the same game
            if (gameDiscogsMetadata != null) {
                DiscogsMetadata = gameDiscogsMetadata;
            }

            // Load duration for progress slider if not currently playing
            if (!IsPlaying) {
                LoadDurationFromMetadata(metadata);
            }
        }

        /// <summary>
        /// Resets only track-level metadata properties while preserving game-level data.
        /// Used during track selection changes to avoid losing Discogs album information.
        /// Maintains consistency between track and game metadata contexts.
        /// </summary>
        private void ResetTrackMetadata() {
            // Reset track-specific properties
            TrackCover = null;
            TrackTitle = null;
            TrackArtist = null;
            TrackAlbum = null;
            TrackYear = null;
            TrackGenre = null;
            TrackDuration = null;
            TrackComment = null;
            TrackNumber = 0;
            TotalTracks = 0;

            // Preserve Discogs metadata at game level - it's album/game-specific, not track-specific
            DiscogsMetadata = gameDiscogsMetadata;
        }

        /// <summary>
        /// Loads track duration from metadata or direct file analysis for progress slider.
        /// Attempts multiple parsing strategies for duration string formats.
        /// Falls back to NAudio file analysis when metadata duration is unavailable.
        /// </summary>
        /// <param name="metadata">Track metadata containing potential duration information</param>
        private void LoadDurationFromMetadata(TrackMetadataModel metadata) {
            if (!string.IsNullOrEmpty(metadata.Duration)) {
                // Try parsing various duration formats from metadata
                if (TimeSpan.TryParse("00:" + metadata.Duration, out TimeSpan duration) ||
                    TimeSpan.TryParse(metadata.Duration, out duration)) {
                    Duration = duration.TotalSeconds;
                    return;
                }
            }

            // Fallback to reading directly from file using NAudio
            try {
                using (var audioFileReader = new AudioFileReader(SelectedMusicFile)) {
                    Duration = audioFileReader.TotalTime.TotalSeconds;
                }
            }
            catch {
                Duration = 0; // Unable to determine duration
            }
        }

        #endregion

        #region Playback Control Methods

        /// <summary>
        /// Toggles between play and pause states based on current playback status.
        /// Central method for play/pause button functionality.
        /// Now async to support non-blocking playback operations.
        /// </summary>
        private async Task PlayPauseAsync() {
            if (IsPlaying && !IsPaused) {
                Pause();
            }
            else {
                await PlayAsync();
            }
        }

        /// <summary>
        /// Starts audio playback with position handling for resume functionality.
        /// Validates file selection and handles playback errors gracefully.
        /// Now fully async to prevent UI thread deadlock.
        /// </summary>
        private async Task PlayAsync() {
            if (string.IsNullOrEmpty(SelectedMusicFile) || SelectedGame == null)
                return;

            try {
                // Determine start position (resume from current position if previously playing)
                double? startPosition = null;
                if (!IsPlaying && Position > 0) {
                    startPosition = Position; // Resume from previous position
                }

                await playbackService.PlayAsync(SelectedMusicFile, startPosition);
                CurrentTrack = $"Playing: {Path.GetFileNameWithoutExtension(SelectedMusicFile)}";
            }
            catch (Exception ex) {
                StatusText = $"Error playing music: {ex.Message}";
            }
        }

        /// <summary>
        /// Pauses current playback and updates track display to show paused state.
        /// Maintains position for resume functionality.
        /// </summary>
        private void Pause() {
            playbackService.Pause();
            if (IsPlaying)
                CurrentTrack = CurrentTrack.Replace("Playing:", "Paused:");
        }

        /// <summary>
        /// Stops playback completely and clears current track display.
        /// Resets position to beginning for next play operation.
        /// </summary>
        private void Stop() {
            playbackService.Stop();
            CurrentTrack = "";
        }

        /// <summary>
        /// Updates position from playback service when user is not dragging slider.
        /// Called by timer every 100ms during playback for smooth progress updates.
        /// </summary>
        private void UpdatePosition() {
            if (!isUserDragging) {
                Position = playbackService.GetPosition();
            }
        }

        #endregion

        #region Auto-Play and Retry Logic

        /// <summary>
        /// Handles automatic advancement to next track when current track ends.
        /// Implements playlist-style behavior with retry logic for failed tracks.
        /// Resets state when reaching end of track list.
        /// Now async to prevent blocking during auto-play.
        /// </summary>
        private async void OnPlaybackEnded() {
            if (MusicFiles != null && SelectedMusicFile != null) {
                // Find current track index in the playlist
                var currentIndex = MusicFiles.ToList().FindIndex(item => item.FilePath == SelectedMusicFile);

                if (currentIndex >= 0 && currentIndex < MusicFiles.Count - 1) {
                    // Auto-advance to next track with retry capability
                    var nextFile = MusicFiles[currentIndex + 1].FilePath;
                    playRetryCount = 0; // Reset retry counter for new track
                    await TryPlayNextTrackWithRetryAsync(nextFile);
                }
                else {
                    // End of playlist - stop playback
                    IsPlaying = false;
                    IsPaused = false;
                    CurrentTrack = "";
                }
            }
            else {
                // No playlist or invalid state - stop playback
                IsPlaying = false;
                IsPaused = false;
                CurrentTrack = "";
            }
        }

        /// <summary>
        /// Attempts to play next track with retry logic for error recovery.
        /// Uses timer delay to prevent rapid retry attempts and system overload.
        /// Implements exponential backoff through MaxPlayRetries constant.
        /// Now async to prevent blocking during auto-play transitions.
        /// </summary>
        /// <param name="nextFile">File path of next track to attempt</param>
        private async Task TryPlayNextTrackWithRetryAsync(string nextFile) {
            isAutoPlayingNext = true; // Prevent position reset during transition
            SelectedMusicFile = nextFile; // Update selection (triggers metadata loading)

            try {
                // Small delay to allow UI updates before starting playback
                await Task.Delay(100);

                await playbackService.PlayAsync(nextFile, 0); // Start from beginning
                CurrentTrack = $"Playing: {Path.GetFileNameWithoutExtension(nextFile)}";
                playRetryCount = 0; // Success - reset retry counter

                // Update playback state
                IsPlaying = true;
                IsPaused = false;

                LoadSelectedTrackMetadata(); // Load metadata for new track
                isAutoPlayingNext = false; // Allow normal position updates
            }
            catch (Exception ex) {
                if (playRetryCount < MaxPlayRetries) {
                    playRetryCount++;
                    await TryPlayNextTrackWithRetryAsync(nextFile); // Recursive retry
                }
                else {
                    // Max retries exceeded - give up and stop
                    StatusText = $"Error playing next track: {ex.Message}";
                    playRetryCount = 0;
                    isAutoPlayingNext = false;
                    IsPlaying = false;
                    IsPaused = false;
                    CurrentTrack = "";
                }
            }
        }

        #endregion

        #region UI Command Implementations

        /// <summary>
        /// Handles metadata section hide commands with parameter-based section selection.
        /// Supports both "mp3" and "discogs" parameters for section-specific hiding.
        /// </summary>
        /// <param name="parameter">Section identifier ("mp3" or "discogs")</param>
        private void HideMetadataSection(object parameter) {
            var section = parameter as string;
            if (string.Equals(section, "mp3", StringComparison.OrdinalIgnoreCase)) {
                IsMp3MetadataVisible = false;
            }
            else if (string.Equals(section, "discogs", StringComparison.OrdinalIgnoreCase)) {
                IsDiscogsMetadataVisible = false;
            }
        }

        #endregion

        #region Discogs Integration (Async Operations)

        /// <summary>
        /// Loads Discogs metadata asynchronously with user selection for multiple results.
        /// Handles API communication, result selection dialog, and metadata caching.
        /// Provides comprehensive error handling and user feedback.
        /// </summary>
        private async Task LoadDiscogsMetadataAsync() {
            if (SelectedGame == null) {
                ShowError("Please select a game first.");
                return;
            }

            string query = SelectedGame.Name; // Search by game name
            string token = SettingsViewModel?.Settings?.DiscogsToken; // User's API token

            // Check if token is available
            if (string.IsNullOrWhiteSpace(token)) {
                ShowError("Discogs Personal Access Token is required for API access.");
                return;
            }

            try {
                // Search Discogs database for releases matching game name
                var results = await DiscogsClient.SearchReleaseAsync(query, token);
                if (results != null && results.Count > 0) {
                    DiscogsMetadataModel selected = null;
                    if (results.Count == 1) {
                        // Single result - auto-select
                        selected = results[0];
                    }
                    else {
                        // Multiple results - show selection dialog to user
                        // This event is handled by the View to show modal dialog
                        selected = OnSelectDiscogsReleaseRequested?.Invoke(results, token);
                    }

                    if (selected != null) {
                        await LoadDiscogsDetails(selected, token); // Get detailed information
                    }
                }
                else {
                    ShowInfo("No results found on Discogs.");
                }
            }
            catch (Exception ex) {
                ShowError("Error loading from Discogs: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads detailed Discogs metadata for selected release including tracklist and extended info.
        /// Extracts release ID from URL and fetches comprehensive metadata.
        /// Caches results at game level for persistence across tracks and sessions.
        /// </summary>
        /// <param name="selected">Basic release info from search results</param>
        /// <param name="token">Discogs API authentication token</param>
        private async Task LoadDiscogsDetails(DiscogsMetadataModel selected, string token) {
            string releaseId = null;

            // Extract release ID from Discogs URL for detailed API call
            if (!string.IsNullOrEmpty(selected.DiscogsUrl)) {
                var urlParts = selected.DiscogsUrl.Split('/');
                releaseId = urlParts.LastOrDefault(); // ID is typically the last URL segment
            }

            if (!string.IsNullOrEmpty(releaseId)) {
                // Fetch detailed release information from Discogs API
                var details = await DiscogsClient.GetReleaseDetailsAsync(releaseId, token);
                if (details != null) {
                    // Merge search result data with detailed data (fallback for missing fields)
                    details.Released = details.Released ?? selected.Released;
                    details.Genres = details.Genres ?? selected.Genres;
                    details.Styles = details.Styles ?? selected.Styles;
                    DiscogsMetadata = details;

                    // Cache at game level for persistence across track changes
                    gameDiscogsMetadata = details;

                    // Save to JSON file for persistence across plugin sessions
                    SaveDiscogsMetadataToCache(details);
                }
                else {
                    // Fallback to search result data if detailed fetch fails
                    DiscogsMetadata = selected;
                    gameDiscogsMetadata = selected;
                    SaveDiscogsMetadataToCache(selected);
                }
            }
            else {
                // No release ID available - use search result data directly
                DiscogsMetadata = selected;
                gameDiscogsMetadata = selected;
                SaveDiscogsMetadataToCache(selected);
            }
        }

        /// <summary>
        /// Loads cached Discogs metadata from JSON file for selected game.
        /// Provides persistence across plugin sessions and reduces API calls.
        /// Silently handles errors to avoid disrupting user experience.
        /// </summary>
        private void LoadCachedDiscogsMetadata() {
            if (SelectedGame == null)
                return;

            try {
                // Construct path to cached Discogs metadata JSON file
                var musicDir = Path.Combine(
                    plugin.PlayniteApi.Paths.ConfigurationPath,
                    "ExtraMetadata",
                    "games",
                    SelectedGame.Id.ToString(),
                    "Music Files"
                );

                var discogsJsonPath = Path.Combine(musicDir, $"{SelectedGame.Id}_discogs.json");

                // Load and deserialize cached metadata if file exists
                if (File.Exists(discogsJsonPath)) {
                    var json = File.ReadAllText(discogsJsonPath);
                    var cachedMetadata = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscogsMetadataModel>(json);

                    if (cachedMetadata != null) {
                        gameDiscogsMetadata = cachedMetadata; // Cache at game level
                        DiscogsMetadata = cachedMetadata; // Display immediately
                    }
                }
            }
            catch {
                // Silently ignore errors when loading cached metadata - it's not critical
                // Could add debug logging here if needed for troubleshooting
            }
        }

        /// <summary>
        /// Saves Discogs metadata to JSON cache file for persistence across sessions.
        /// Uses MetadataJsonSaver utility for consistent file management.
        /// Silently handles errors to avoid disrupting user workflow.
        /// </summary>
        /// <param name="metadata">Discogs metadata to cache</param>
        private void SaveDiscogsMetadataToCache(DiscogsMetadataModel metadata) {
            if (SelectedGame == null || metadata == null)
                return;

            try {
                // Add timestamp to metadata for cache tracking
                metadata.Comment = $"Cached on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                MetadataJsonSaver.SaveDiscogsMetadataToJson(SelectedGame.Id, metadata, plugin.PlayniteApi);
            }
            catch {
                // Silently ignore errors when saving metadata cache - it's not critical
                // Could add debug logging here if needed for troubleshooting
            }
        }

        #endregion

        #region Image Preview Methods

        /// <summary>
        /// Shows track cover image in preview window if available.
        /// Delegates to View through event for proper UI separation.
        /// </summary>
        private void ShowTrackCover() {
            if (TrackCover != null) {
                // Delegate to View to handle window creation and display
                OnShowTrackCoverRequested?.Invoke(TrackCover);
            }
            else {
                ShowInfo("Cover image is not available.");
            }
        }

        /// <summary>
        /// Shows Discogs cover image in preview window if URL is available.
        /// Delegates to View for window management and URL-based image loading.
        /// </summary>
        private void ShowDiscogsCover() {
            var coverUrl = DiscogsMetadata?.CoverUrl;
            if (!string.IsNullOrEmpty(coverUrl)) {
                // Delegate to View to handle URL-based image loading and display
                OnShowDiscogsCoverRequested?.Invoke(coverUrl);
            }
            else {
                ShowInfo("Discogs cover is not available.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves settings ViewModel from plugin using reflection.
        /// Necessary for accessing plugin configuration without tight coupling.
        /// </summary>
        /// <returns>Settings ViewModel or null if unavailable</returns>
        private OstPlayerSettingsViewModel GetSettingsViewModel() {
            var settingsProp = plugin.GetType().GetProperty("settings",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return settingsProp?.GetValue(plugin) as OstPlayerSettingsViewModel;
        }

        /// <summary>
        /// Shows error message to user through View event delegation.
        /// Maintains MVVM separation by not directly accessing UI components.
        /// </summary>
        /// <param name="message">Error message to display</param>
        private void ShowError(string message) {
            OnShowErrorRequested?.Invoke(message);
        }

        /// <summary>
        /// Shows information message to user through View event delegation.
        /// Maintains MVVM separation by not directly accessing UI components.
        /// </summary>
        /// <param name="message">Information message to display</param>
        private void ShowInfo(string message) {
            OnShowInfoRequested?.Invoke(message);
        }

        #endregion

        #region Events for View Communication

        /// <summary>Event for requesting track cover image preview display.</summary>
        public event Action<BitmapImage> OnShowTrackCoverRequested;

        /// <summary>Event for requesting Discogs cover image preview display.</summary>
        public event Action<string> OnShowDiscogsCoverRequested;

        /// <summary>Event for requesting error message display to user.</summary>
        public event Action<string> OnShowErrorRequested;

        /// <summary>Event for requesting information message display to user.</summary>
        public event Action<string> OnShowInfoRequested;

        /// <summary>Event for requesting Discogs release selection dialog when multiple results found.</summary>
        public event Func<List<DiscogsMetadataModel>, string, DiscogsMetadataModel> OnSelectDiscogsReleaseRequested;

        #endregion

        #region Volume Persistence Methods

        /// <summary>
        /// Loads volume setting from plugin settings.
        /// Called during playback service initialization to restore last used volume.
        /// </summary>
        private void LoadVolumeFromSettings() {
            try {
                var settings = plugin.LoadPluginSettings<OstPlayerSettings>();
                if (settings != null) {
                    // Set volume without triggering save to avoid circular update
                    volume = settings.DefaultVolume;
                    OnPropertyChanged(nameof(Volume));
                    OnPropertyChanged(nameof(VolumeDisplay));

                    // Apply volume to audio engine
                    playbackService?.SetVolume(volume / 100.0);
                }
            }
            catch (Exception ex) {
                // Fallback to default volume if loading fails
                volume = 50;
                StatusText = $"Failed to load volume setting: {ex.Message}";
            }
        }

        /// <summary>
        /// Saves current volume setting to plugin settings.
        /// Called whenever volume changes to persist user preference.
        /// </summary>
        /// <param name="volumeLevel">Volume level to save (0-100)</param>
        private void SaveVolumeToSettings(double volumeLevel) {
            try {
                var settings = plugin.LoadPluginSettings<OstPlayerSettings>() ?? new OstPlayerSettings();
                settings.DefaultVolume = volumeLevel;
                plugin.SavePluginSettings(settings);
            }
            catch (Exception ex) {
                // Log error but don't interrupt user experience
                System.Diagnostics.Debug.WriteLine($"Failed to save volume to settings: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes resources including audio playback service and timer.
        /// Essential for proper cleanup when ViewModel is no longer needed.
        /// Prevents memory leaks and audio resource conflicts.
        /// </summary>
        public void Dispose() {
            playbackService?.Dispose(); // Clean up NAudio resources
            progressTimer?.Stop(); // Stop timer to prevent continued execution
        }

        #endregion
    }
}
