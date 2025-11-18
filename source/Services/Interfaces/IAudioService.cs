// ====================================================================
// FILE: IAudioService.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Services/Interfaces
// LOCATION: Services/Interfaces/
// VERSION: 2.0.0
// CREATED: 2025-08-08
// UPDATED: 2025-08-09
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Interface for audio service providing contract for music playback, volume control, and audio engine management.
// Part of Phase 5 Dependency Injection implementation for clean architecture and testability.
//
// FEATURES:
// - Async audio playback operations
// - Volume and position control
// - Audio engine state management
// - Event-driven audio notifications
// - Multiple audio format support
// - **PHASE 5**: Production-ready audio interface with health monitoring
// - **PHASE 5**: Complete contract for audio service implementations
//
// DEPENDENCIES:
// - System.Threading.Tasks (async operations)
// - System.Threading (cancellation tokens)
// - System (event handling)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle
// - Observer Pattern (events)
// - Command Pattern (audio operations)
// - Strategy Pattern (audio engines)
//
// LIMITATIONS:
// - Single audio stream support only
// - No advanced audio effects interface
// - Basic error reporting through events
// - Platform-specific audio engine dependencies
//
// FUTURE REFACTORING:
// FUTURE: Add multi-stream audio support for mixing
// FUTURE: Implement audio effects and filters interface
// FUTURE: Add advanced error reporting with error codes
// FUTURE: Create audio visualization data interface
// FUTURE: Add audio streaming support for large files
// CONSIDER: Plugin architecture for audio engines
// CONSIDER: Advanced audio processing pipeline
// IDEA: Real-time audio analysis and metadata extraction
// IDEA: Cloud-based audio processing integration
//
// TESTING:
// - Mock implementations for unit testing
// - Audio engine integration tests
// - Event handling validation tests
// - Error condition simulation tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - NAudio integration
// - Cross-platform audio engine support
//
// CHANGELOG:
// 2025-08-09 v2.0.0 - Phase 5 DI Implementation completed: Production-ready audio interface, health monitoring, complete contract
// 2025-08-08 v1.0.0 - Initial interface for Phase 5 DI implementation
// ====================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace OstPlayer.Services.Interfaces {
    /// <summary>
    /// Audio playback states for state management.
    /// </summary>
    public enum AudioState {
        /// <summary>
        /// Audio playback is stopped.
        /// </summary>
        Stopped,
        
        /// <summary>
        /// Audio is currently playing.
        /// </summary>
        Playing,
        
        /// <summary>
        /// Audio playback is paused.
        /// </summary>
        Paused,
        
        /// <summary>
        /// Audio file is loading.
        /// </summary>
        Loading,
        
        /// <summary>
        /// An error occurred with audio playback.
        /// </summary>
        Error
    }

    /// <summary>
    /// Interface for audio service providing comprehensive audio operations.
    /// </summary>
    public interface IAudioService {
        #region Properties

        /// <summary>Current audio playback state.</summary>
        AudioState State { get; }

        /// <summary>Current volume level (0.0 to 1.0).</summary>
        double Volume { get; set; }

        /// <summary>Current playback position in seconds.</summary>
        double Position { get; set; }

        /// <summary>Total duration of current track in seconds.</summary>
        double Duration { get; }

        /// <summary>Currently loaded file path.</summary>
        string CurrentFile { get; }

        /// <summary>Whether user is currently dragging position slider.</summary>
        bool IsUserDragging { get; set; }

        #endregion

        #region Playback Control Methods

        /// <summary>
        /// Loads and plays audio file asynchronously.
        /// </summary>
        Task PlayAsync(string filePath, double? startPosition = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pauses current playback.
        /// </summary>
        Task PauseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Resumes paused playback.
        /// </summary>
        Task ResumeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops current playback and releases resources.
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Seeks to specific position in track.
        /// </summary>
        Task SeekAsync(double position, CancellationToken cancellationToken = default);

        #endregion

        #region Volume and Settings

        /// <summary>
        /// Sets volume level with validation.
        /// </summary>
        Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies audio settings and configuration.
        /// </summary>
        Task ApplySettingsAsync(OstPlayerSettings settings, CancellationToken cancellationToken = default);

        #endregion

        #region Events

        /// <summary>Fired when playback state changes.</summary>
        event EventHandler<AudioState> StateChanged;

        /// <summary>Fired when playback position changes.</summary>
        event EventHandler<double> PositionChanged;

        /// <summary>Fired when track duration is determined.</summary>
        event EventHandler<double> DurationChanged;

        /// <summary>Fired when volume changes.</summary>
        event EventHandler<double> VolumeChanged;

        /// <summary>Fired when current track ends.</summary>
        event EventHandler TrackEnded;

        /// <summary>Fired when audio error occurs.</summary>
        event EventHandler<string> ErrorOccurred;

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets supported audio formats.
        /// </summary>
        string[] GetSupportedFormats();

        /// <summary>
        /// Validates if file format is supported.
        /// </summary>
        bool IsFormatSupported(string filePath);

        /// <summary>
        /// Gets audio engine information.
        /// </summary>
        Task<AudioEngineInfo> GetEngineInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs health check on audio engine.
        /// </summary>
        Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Resource Management

        /// <summary>
        /// Releases audio resources.
        /// </summary>
        void Dispose();

        #endregion
    }

    /// <summary>
    /// Audio engine information for diagnostics.
    /// </summary>
    public class AudioEngineInfo {
        /// <summary>
        /// Gets or sets the name of the audio engine.
        /// </summary>
        public string EngineName { get; set; }
        
        /// <summary>
        /// Gets or sets the version of the audio engine.
        /// </summary>
        public string Version { get; set; }
        
        /// <summary>
        /// Gets or sets the array of supported audio formats.
        /// </summary>
        public string[] SupportedFormats { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the audio engine is initialized.
        /// </summary>
        public bool IsInitialized { get; set; }
        
        /// <summary>
        /// Gets or sets the current status of the audio engine.
        /// </summary>
        public string Status { get; set; }
    }
}
