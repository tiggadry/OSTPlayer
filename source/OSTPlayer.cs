// <copyright file="OstPlayer.cs" company="OstPlayer">Copyright (c) OstPlayer. All rights reserved.</copyright>
// ====================================================================
// FILE: OstPlayer.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Core
// LOCATION: /
// VERSION: 3.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-09
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Main plugin entry point for OstPlayer - Phase 5 Dependency Injection implementation.
// Complete DI architecture with constructor injection, service container, and interface-based design.
// Handles plugin lifecycle, Playnite integration, and orchestrates all plugin services through DI.
//
// FEATURES:
// - Playnite sidebar integration
// - Settings management and UI
// - PlayniteSound plugin interaction (pause/resume)
// - Game music file discovery
// - Plugin lifecycle management
// - Service container initialization
// - **NEW**: Standardized error handling with user-friendly messages
// - **NEW**: Complete dependency injection architecture
// - **NEW**: Service container with automatic dependency resolution
// - **NEW**: Interface-based service design for testability
// - **ENHANCED**: Plugin lifecycle management with DI
// - **ENHANCED**: Error handling with injected services
// - **ENHANCED**: Sidebar integration with DI-based ViewModels
// - **PHASE 5**: Complete dependency injection implementation
// - **PHASE 5**: Clean logging and performance optimization
// - **PHASE 5**: Settings dialog integration fixed
//
// DI ARCHITECTURE:
// - ServiceContainer for dependency registration and resolution
// - Interface-based service design (IAudioService, IMetadataService, IGameService)
// - Constructor injection throughout application
// - Service lifetime management (singleton, transient, scoped)
// - Testable architecture with mock support
//
// DEPENDENCIES (injected):
// - IPlayniteAPI (plugin framework)
// - ILogger (logging service)
// - ServiceContainer (DI container)
// - All services resolved through DI
//
// ERROR HANDLING STRATEGY:
// - Plugin-level error coordination and recovery
// - User-friendly error messages for plugin failures
// - Graceful degradation when components fail
// - Comprehensive logging for debugging support
// - UI fallback controls for critical errors
//
// SERVICE REGISTRATION:
// - Core services (ErrorHandling, Metadata, Audio, Game)
// - External clients (Discogs, MusicBrainz)
// - ViewModels with proper lifetime management
// - Utility services and helpers
//
// FUTURE REFACTORING:
// FUTURE: Implement proper dependency injection container
// FUTURE: Add plugin health monitoring and diagnostics
// FUTURE: Extract PlayniteSound integration to dedicated service
// FUTURE: Add plugin auto-update mechanism
// FUTURE: Create plugin SDK for third-party extensions
// FUTURE: Add backup/restore functionality for settings and metadata
// CONSIDER: Splitting into multiple plugins for modularity
// CONSIDER: Adding plugin marketplace integration
// IDEA: Integration with music streaming services
// IDEA: Cloud sync for playlists and metadata
// IDEA: Community-driven soundtrack database
//
// PERFORMANCE NOTES:
// - Lazy service initialization through DI
// - Optimized service resolution with caching
// - Memory-efficient service lifetime management
// - Minimal overhead for dependency injection
//
// LIMITATIONS:
// - Single plugin instance per Playnite session
// - Settings require restart for some changes
// - Service container is singleton-based
// - No dynamic service registration after initialization
//
// TESTING:
// - Unit tests for service registration and resolution
// - Integration tests for Playnite API interaction
// - Mock support for all injected dependencies
// - Error handling and fallback scenario tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Playnite SDK 6.x
// - Windows 7+
//
// CHANGELOG:
// 2025-08-09 v3.0.0 - Phase 5 DI Implementation completed: Fixed settings dialog, cleaned logging, final optimizations
// 2025-08-08 v2.0.0 - Phase 5 DI Implementation: Complete dependency injection architecture, service container, interface-based design
// 2025-08-07 v1.2.0 - Standardized error handling integration
// 2025-08-06 v1.0.0 - Initial implementation
// ====================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using OstPlayer.Services;
using OstPlayer.Services.Interfaces;
using OstPlayer.Utils;
using OstPlayer.ViewModels;
using OstPlayer.Views;
using OstPlayer.Views.Settings;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace OstPlayer
{
    /// <summary>
    /// Main plugin class for OstPlayer with complete dependency injection architecture.
    /// Implements Phase 5 DI pattern with service container and interface-based design.
    /// </summary>
    public class OstPlayer : GenericPlugin
    {
        // Private Fields (DI Architecture)
        // Service container for dependency injection
        private readonly ServiceContainer serviceContainer;

        // Core services (resolved through DI)
        private readonly ILogger logger;
        private readonly ErrorHandlingService errorHandler;
        private readonly IGameService gameService;
        private readonly IAudioService audioService;
        private readonly IMetadataService metadataService;

        // Plugin state and UI components
        private OstPlayerSettingsViewModel settings { get; set; }
        internal SidebarItem SidebarItem { get; set; }
        private OstPlayerSidebarView lastSidebarView = null;

        // PlayniteSound integration state
        private bool playniteSoundWasPaused = false;

        // Plugin Properties

        /// <summary>
        /// Gets the unique identifier for this plugin.
        /// </summary>
        public override Guid Id { get; } = Guid.Parse("f3b0c108-5212-4b34-a303-47e859b31a92");

        // Constructor (Dependency Injection)

        /// <summary>
        /// Initializes a new instance of the OstPlayer class with dependency injection.
        /// </summary>
        /// <param name="api">The Playnite API interface.</param>
        public OstPlayer(IPlayniteAPI api)
            : base(api)
        {
            try
            {
                // Initialize service container and register all services
                this.serviceContainer = ServiceContainer.Instance;
                this.InitializeDependencyInjection(api);

                // Resolve core services through DI with defensive handling
                try
                {
                    this.logger =
                        this.serviceContainer.GetService<ILogger>() ?? LogManager.GetLogger();
                    this.errorHandler =
                        this.serviceContainer.GetService<ErrorHandlingService>()
                        ?? new ErrorHandlingService();
                    this.gameService = this.serviceContainer.GetService<IGameService>();
                    this.audioService = this.serviceContainer.GetService<IAudioService>();
                    this.metadataService = this.serviceContainer.GetService<IMetadataService>();
                }
                catch (Exception serviceEx)
                {
                    // Use fallback logger if DI resolution fails
                    var fallbackLogger = LogManager.GetLogger();
                    fallbackLogger.Warn(
                        serviceEx,
                        "Failed to resolve some services through DI, using fallbacks"
                    );
                }

                // Initialize settings with error handling
                this.InitializeSettings();

                // Initialize sidebar item with DI support
                this.InitializeSidebarItem();
            }
            catch (Exception ex)
            {
                var fallbackLogger = LogManager.GetLogger();
                fallbackLogger.Error(ex, "Critical error during OstPlayer DI initialization");

                // Try to create fallback error handler
                try
                {
                    var fallbackErrorHandler = new ErrorHandlingService();
                    fallbackErrorHandler.HandlePlaybackError(ex, "Plugin DI Initialization");
                }
                catch
                {
                    // Last resort - log to debug output
                    System.Diagnostics.Debug.WriteLine(
                        $"CRITICAL: OstPlayer initialization failed: {ex.Message}"
                    );
                }

                throw; // Re-throw critical initialization errors
            }
        }

        // Dependency Injection Initialization

        /// <summary>
        /// Initializes the dependency injection container with all services.
        /// Registers services with appropriate lifetimes and dependencies.
        /// </summary>
        private void InitializeDependencyInjection(IPlayniteAPI api)
        {
            try
            {
                // Clear any existing registrations for clean initialization
                this.serviceContainer.Clear();

                // Register Playnite API and plugin reference as singletons FIRST
                this.serviceContainer.RegisterSingleton<IPlayniteAPI>(api);
                this.serviceContainer.RegisterSingleton<OstPlayer>(this);
                this.serviceContainer.RegisterSingleton<ILogger>(LogManager.GetLogger());

                // Register basic infrastructure services before complex ones
                this.serviceContainer.RegisterSingleton<
                    ErrorHandlingService,
                    ErrorHandlingService
                >();

                // Register utility services
                this.serviceContainer.RegisterTransient<
                    MusicPlaybackService,
                    MusicPlaybackService
                >();

                // Register cache configuration
                this.serviceContainer.RegisterSingleton<MetadataCacheConfig>(provider =>
                {
                    return new MetadataCacheConfig
                    {
                        TrackMetadataTTL = TimeSpan.FromHours(1),
                        AlbumMetadataTTL = TimeSpan.FromHours(6),
                        ExternalMetadataTTL = TimeSpan.FromHours(12),
                        MaxCacheSize = 2000,
                        CleanupInterval = TimeSpan.FromMinutes(5),
                        EnableMemoryPressureAdjustment = true,
                        EnableCacheWarming = true,
                    };
                });

                // Register cache services
                this.serviceContainer.RegisterSingleton<MetadataCache, MetadataCache>();

                // Register external API clients as singletons (single instance for both interface and concrete type)
                this.serviceContainer.RegisterSingleton<
                    DiscogsClientService,
                    DiscogsClientService
                >();
                this.serviceContainer.RegisterSingleton<
                    MusicBrainzClientService,
                    MusicBrainzClientService
                >();

                // Register interfaces to use the same instances
                this.serviceContainer.RegisterSingleton<IDiscogsClient>(provider =>
                    provider.GetService<DiscogsClientService>()
                );
                this.serviceContainer.RegisterSingleton<IMusicBrainzClient>(provider =>
                    provider.GetService<MusicBrainzClientService>()
                );

                // Register service interfaces with their implementations (register after dependencies)
                this.serviceContainer.RegisterSingleton<IGameService, GameService>();
                this.serviceContainer.RegisterSingleton<IMetadataService, MetadataService>();
                this.serviceContainer.RegisterTransient<IAudioService, AudioService>();

                // Validate service registrations (debug only)
#if DEBUG
                try
                {
                    this.serviceContainer.ValidateServices();
                }
                catch (Exception validationEx)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"⚠ DI Container validation warning: {validationEx.Message}"
                    );
                    // Continue anyway - some services might work even if others don't validate
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"✗ Failed to initialize DI container: {ex.Message}"
                );
                throw new InvalidOperationException(
                    "Dependency injection initialization failed",
                    ex
                );
            }
        }

        /// <summary>
        /// Initializes plugin settings with dependency injection support.
        /// </summary>
        private void InitializeSettings()
        {
            try
            {
                // Create settings ViewModel directly instead of through DI container
                // OstPlayerSettingsViewModel is not registered in DI container as it's Playnite-specific
                this.settings = new OstPlayerSettingsViewModel(this);
                this.Properties = new GenericPluginProperties { HasSettings = true };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to initialize plugin settings");
                this.errorHandler.HandlePlaybackError(ex, "Plugin Settings Initialization");

                // Create fallback settings to prevent complete plugin failure
                this.settings = null;
                this.Properties = new GenericPluginProperties { HasSettings = false };
            }
        }

        // Sidebar Management (DI-Enhanced)

        /// <summary>
        /// Initializes the sidebar item with dependency injection support.
        /// </summary>
        private void InitializeSidebarItem()
        {
            try
            {
                this.SidebarItem = new SidebarItem
                {
                    Type = SiderbarItemType.View,
                    Title = "OstPlayer",
                    Icon = this.GetPluginIcon(),
                    Visible = true,
                    Opened = this.CreateSidebarView,
                    Closed = this.CleanupSidebarView,
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to initialize sidebar item with DI");
                this.errorHandler.HandlePlaybackError(ex, "Sidebar DI Initialization");

                // Create minimal fallback sidebar item
                this.SidebarItem = new SidebarItem
                {
                    Type = SiderbarItemType.View,
                    Title = "OstPlayer (DI Error)",
                    Visible = true,
                    Opened = () =>
                        this.CreateErrorView(
                            "Dependency injection failed. Please restart Playnite."
                        ),
                    Closed = () => { },
                };
            }
        }

        /// <summary>
        /// Creates the sidebar view with dependency injection and comprehensive error handling.
        /// </summary>
        private UserControl CreateSidebarView()
        {
            try
            {
                // Pause PlayniteSound music when sidebar is opened
                PausePlayniteSoundMusic(this.PlayniteApi);
                this.playniteSoundWasPaused = true;

                // Get active game simply and safely
                Game activeGame = null;
                try
                {
                    activeGame = this.PlayniteApi.MainView.SelectedGames?.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    this.logger.Warn(
                        ex,
                        "Failed to get selected game - continuing without preselection"
                    );
                    activeGame = null;
                }

                // Create sidebar view directly on UI thread
                OstPlayerSidebarView view;
                if (activeGame != null)
                {
                    view = new OstPlayerSidebarView(this, activeGame);
                }
                else
                {
                    view = new OstPlayerSidebarView(this);
                }

                this.lastSidebarView = view;
                return view;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Critical error in DI-enhanced sidebar view creation");
                this.errorHandler.HandlePlaybackError(ex, "Sidebar DI Creation");
                return this.CreateErrorView("Critical DI error occurred. Please restart Playnite.");
            }
        }

        /// <summary>
        /// Gets the plugin icon path with error handling.
        /// </summary>
        private string GetPluginIcon()
        {
            try
            {
                return Path.Combine(
                    Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location
                    ),
                    "OstPlayer.png"
                );
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, "Failed to get plugin icon path");
                return null; // Playnite will use default icon
            }
        }

        /// <summary>
        /// Creates a fallback error view when the main UI fails to load.
        /// </summary>
        private UserControl CreateErrorView(string errorMessage)
        {
            try
            {
                var errorControl = new UserControl();
                var textBlock = new TextBlock
                {
                    Text = $"OstPlayer DI Error: {errorMessage}",
                    Margin = new System.Windows.Thickness(10),
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Foreground = System.Windows.Media.Brushes.Red,
                };
                errorControl.Content = textBlock;
                return errorControl;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to create error view");
                // Return minimal fallback
                return new UserControl();
            }
        }

        /// <summary>
        /// Cleans up sidebar view resources with dependency injection support.
        /// FIXED: Enhanced music stop with fallback to direct view method.
        /// </summary>
        private void CleanupSidebarView()
        {
            try
            {
                // Stop music playback when the sidebar panel is closed
                if (this.lastSidebarView != null)
                {
                    try
                    {
                        // FIXED: Try DI-based audio service first with timeout
                        if (this.audioService != null)
                        {
                            var stopTask = this.audioService.StopAsync();
                            if (!stopTask.Wait(1000)) // 1 second timeout
                            {
                                this.logger.Warn("Audio service stop operation timed out");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    "FIXED: Stopped music using audioService during sidebar cleanup"
                                );
                            }
                        }

                        // FIXED: Fallback to direct view StopMusic method for reliability
                        // This ensures music stops even if audioService fails or times out
                        try
                        {
                            this.lastSidebarView.StopMusic();
                            System.Diagnostics.Debug.WriteLine(
                                "FIXED: Stopped music using lastSidebarView.StopMusic() as fallback"
                            );
                        }
                        catch (Exception fallbackEx)
                        {
                            this.logger.Warn(
                                fallbackEx,
                                "Fallback StopMusic() also failed during cleanup"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Warn(
                            ex,
                            "Failed to stop music during sidebar cleanup using DI"
                        );

                        // FIXED: Final fallback - try direct StopMusic even if DI completely fails
                        try
                        {
                            this.lastSidebarView.StopMusic();
                            System.Diagnostics.Debug.WriteLine(
                                "FIXED: Used final fallback StopMusic() after DI failure"
                            );
                        }
                        catch (Exception finalEx)
                        {
                            this.logger.Error(
                                finalEx,
                                "All music stop methods failed during cleanup"
                            );
                        }
                    }
                    finally
                    {
                        this.lastSidebarView = null;
                    }
                }

                // Resume PlayniteSound music only if paused by OstPlayer
                if (this.playniteSoundWasPaused)
                {
                    try
                    {
                        this.ResumePlayniteSoundMusic();
                    }
                    catch (Exception ex)
                    {
                        this.logger.Warn(ex, "Failed to resume PlayniteSound music via URI");
                    }
                    finally
                    {
                        this.playniteSoundWasPaused = false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Error during DI-enhanced sidebar cleanup");
                // Don't propagate cleanup errors
            }
        }

        // PlayniteSound Integration

        /// <summary>
        /// Sends a URI request to PlayniteSound to pause music playback with error handling.
        /// </summary>
        public static void PausePlayniteSoundMusic(IPlayniteAPI api)
        {
            try
            {
                Process.Start("playnite://Sounds/pause/OSTPlayer");
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Warn(ex, "Failed to pause PlayniteSound music via URI");
            }
        }

        /// <summary>
        /// Resumes PlayniteSound music with error handling.
        /// </summary>
        private void ResumePlayniteSoundMusic()
        {
            try
            {
                Process.Start("playnite://Sounds/resume/OstPlayer");
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, "Failed to resume PlayniteSound music via URI");
            }
        }

        // Public Plugin Interface (DI-Enhanced)

        /// <summary>
        /// Returns the sidebar item for Playnite's sidebar integration.
        /// </summary>
        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            try
            {
                return new List<SidebarItem> { this.SidebarItem };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to get sidebar items with DI");
                return new List<SidebarItem>(); // Return empty list on error
            }
        }

        /// <summary>
        /// Gets the path to the music files for a specific game using injected game service.
        /// </summary>
        public static string GetGameMusicPath(IPlayniteAPI api, Game game)
        {
            try
            {
                // Try to use DI-based game service if available
                var container = ServiceContainer.Instance;
                if (container.IsRegistered<IGameService>())
                {
                    var gameService = container.GetService<IGameService>();
                    // Use ConfigureAwait(false) to prevent deadlock
                    return gameService
                        .GetMusicDirectoryAsync(game)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }

                // Use existing static helper
                return Utils.MusicFileHelper.GetGameMusicPath(api, game);
            }
            catch (Exception ex)
            {
                LogManager
                    .GetLogger()
                    .Warn(ex, $"Failed to get music path for game: {game?.Name ?? "Unknown"}");
                // Fallback to static helper on error
                try
                {
                    return Utils.MusicFileHelper.GetGameMusicPath(api, game);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets a list of MP3 files for a game using injected game service.
        /// </summary>
        public static List<string> GetGameMusicFiles(IPlayniteAPI api, Game game)
        {
            try
            {
                var container = ServiceContainer.Instance;
                if (container.IsRegistered<IGameService>())
                {
                    var gameService = container.GetService<IGameService>();
                    // Use ConfigureAwait(false) to prevent deadlock
                    return gameService
                        .GetMusicFilesAsync(game)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }

                // Fallback to static helper
                return Utils.MusicFileHelper.GetGameMusicFiles(api, game);
            }
            catch (Exception ex)
            {
                LogManager
                    .GetLogger()
                    .Warn(ex, $"Failed to get music files for game: {game?.Name ?? "Unknown"}");
                // Fallback to static helper on error
                try
                {
                    return Utils.MusicFileHelper.GetGameMusicFiles(api, game);
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        // Plugin Event Hooks

        // Plugin event hooks (not implemented, but required by Playnite)
        /// <summary>
        /// Called when a game is installed. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnGameInstalled(OnGameInstalledEventArgs args) { }

        /// <summary>
        /// Called when a game is started. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnGameStarted(OnGameStartedEventArgs args) { }

        /// <summary>
        /// Called when a game is starting. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnGameStarting(OnGameStartingEventArgs args) { }

        /// <summary>
        /// Called when a game is stopped. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnGameStopped(OnGameStoppedEventArgs args) { }

        /// <summary>
        /// Called when a game is uninstalled. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnGameUninstalled(OnGameUninstalledEventArgs args) { }

        /// <summary>
        /// Called when the application is started. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args) { }

        /// <summary>
        /// Called when the application is stopped. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args) { }

        /// <summary>
        /// Called when the library is updated. Not implemented in this plugin.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args) { }

        // Settings Management (DI-Enhanced)

        /// <summary>
        /// Returns the plugin settings ViewModel for Playnite with dependency injection.
        /// </summary>
        public override ISettings GetSettings(bool firstRunSettings)
        {
            try
            {
                if (this.settings == null)
                {
                    // Create settings ViewModel directly if not already initialized
                    this.settings = new OstPlayerSettingsViewModel(this);
                }
                return this.settings;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to get plugin settings");
                this.errorHandler.HandlePlaybackError(ex, "Settings Retrieval");
                return null; // Playnite will handle null gracefully
            }
        }

        /// <summary>
        /// Returns the settings view (UI) for Playnite with error handling.
        /// </summary>
        public override UserControl GetSettingsView(bool firstRunView)
        {
            try
            {
                var settingsView = new OstPlayerSettingsView();

                // Ensure settings ViewModel is available
                if (this.settings == null)
                {
                    this.settings = new OstPlayerSettingsViewModel(this);
                }

                // Set DataContext explicitly for proper binding
                settingsView.DataContext = this.settings;

                return settingsView;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to create settings view");
                this.errorHandler.HandlePlaybackError(ex, "Settings View Creation");

                // Return fallback settings view
                return this.CreateErrorView(
                    "Failed to load settings interface. Please restart Playnite."
                );
            }
        }

        // Service Container Access

        /// <summary>
        /// Provides access to the service container for external components.
        /// </summary>
        public ServiceContainer GetServiceContainer()
        {
            return this.serviceContainer;
        }

        /// <summary>
        /// Gets a service from the DI container.
        /// </summary>
        public T GetService<T>()
        {
            return this.serviceContainer.GetService<T>();
        }

        /// <summary>
        /// Checks if a service is registered in the DI container.
        /// </summary>
        public bool IsServiceRegistered<T>()
        {
            return this.serviceContainer.IsRegistered<T>();
        }

        // IDisposable Implementation (DI-Enhanced)

        /// <summary>
        /// Disposes resources and cleans up the plugin.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.logger.Info("Disposing OstPlayer with dependency injection...");

                    // Dispose DI-managed services that implement IDisposable
                    if (this.audioService is IDisposable disposableAudio)
                        disposableAudio.Dispose();
                    if (this.metadataService is IDisposable disposableMetadata)
                        disposableMetadata.Dispose();
                    if (this.gameService is IDisposable disposableGame)
                        disposableGame.Dispose();

                    // Dispose service container
                    this.serviceContainer?.Dispose();

                    this.logger.Info("OstPlayer disposed successfully with DI cleanup");
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Error disposing OstPlayer with DI");
                }
            }
        }
    }
}
