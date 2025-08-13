// ====================================================================
// FILE: IUIViewModel.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: ViewModels
// LOCATION: ViewModels/UI/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Interface contracts for UI-related ViewModels in the OstPlayer project.
// Defines the public API for game selection, status management, and UI state
// handling. Part of the critical refactoring to extract the final UI concerns
// from the monolithic OstPlayerSidebarViewModel.
//
// FEATURES:
// - Game selection and filtering functionality
// - Status message management and display
// - UI state coordination and synchronization
// - Error handling and user feedback
// - Game database integration and filtering
//
// DEPENDENCIES:
// - System.Collections.ObjectModel (ObservableCollection support)
// - System (EventHandler, basic types)
// - Playnite.SDK.Models.Game (game entity representation)
// - OstPlayer.Models.TrackListItem (track item representation)
//
// DESIGN PATTERNS:
// - Interface Segregation Principle (focused UI contracts)
// - Observer Pattern (UI state change events)
// - Command Pattern (UI action commands)
//
// REFACTORING CONTEXT:
// These interfaces are part of the final phase of the OstPlayerSidebarViewModel
// refactoring initiative. They enable extraction of the remaining UI concerns
// into specialized ViewModels while maintaining clear contracts and testability.
//
// IMPLEMENTATION TARGETS:
// - GameSelectionViewModel: Game filtering, selection, and database interaction
// - StatusViewModel: Status messages, error handling, and user feedback
// - Main coordinator: Delegates UI operations to implementing ViewModels
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
// - Playnite SDK integration support
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial interface definition for final UI ViewModel extraction
// ====================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using OstPlayer.Models;
using Playnite.SDK.Models;

namespace OstPlayer.ViewModels.UI
{
    /// <summary>
    /// Interface contract for game selection ViewModels providing game filtering,
    /// selection state management, and database integration functionality.
    /// 
    /// Designed to support the extraction of game selection concerns from the monolithic
    /// OstPlayerSidebarViewModel into focused, testable components.
    /// </summary>
    public interface IGameSelectionViewModel : IDisposable
    {
        #region Game Collection Properties

        /// <summary>
        /// Gets the collection of games currently displayed in the UI (filtered).
        /// </summary>
        ObservableCollection<Game> Games { get; }

        /// <summary>
        /// Gets the total count of games with music files (unfiltered).
        /// </summary>
        int TotalGamesCount { get; }

        /// <summary>
        /// Gets the currently selected game.
        /// </summary>
        Game SelectedGame { get; set; }

        /// <summary>
        /// Gets the collection of music files for the currently selected game.
        /// </summary>
        ObservableCollection<TrackListItem> MusicFiles { get; }

        #endregion

        #region State Properties

        /// <summary>
        /// Gets a value indicating whether games are currently being loaded.
        /// </summary>
        bool IsLoadingGames { get; }

        /// <summary>
        /// Gets a value indicating whether music files are currently being loaded.
        /// </summary>
        bool IsLoadingMusicFiles { get; }

        /// <summary>
        /// Gets the current filter text for game searching.
        /// </summary>
        string FilterText { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all games with music files from the database.
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task LoadGamesAsync();

        /// <summary>
        /// Filters the games collection based on the specified search text.
        /// </summary>
        /// <param name="searchText">Text to filter games by</param>
        void FilterGames(string searchText);

        /// <summary>
        /// Finds a game by exact name match.
        /// </summary>
        /// <param name="gameName">Name of the game to find</param>
        /// <returns>Matching game or null if not found</returns>
        Game FindGameByName(string gameName);

        /// <summary>
        /// Loads music files for the currently selected game.
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task LoadMusicFilesAsync();

        /// <summary>
        /// Clears the current game selection and music files.
        /// </summary>
        void ClearSelection();

        #endregion

        #region Events

        /// <summary>
        /// Raised when the game selection changes.
        /// </summary>
        event EventHandler<Game> GameSelectionChanged;

        /// <summary>
        /// Raised when games loading starts or completes.
        /// </summary>
        event EventHandler GamesLoadingStateChanged;

        /// <summary>
        /// Raised when music files loading starts or completes.
        /// </summary>
        event EventHandler MusicFilesLoadingStateChanged;

        /// <summary>
        /// Raised when the games collection is filtered.
        /// </summary>
        event EventHandler GamesFiltered;

        /// <summary>
        /// Raised when music files are loaded for the selected game.
        /// </summary>
        event EventHandler<ObservableCollection<TrackListItem>> MusicFilesLoaded;

        #endregion
    }

    /// <summary>
    /// Interface contract for status management ViewModels providing status messages,
    /// error handling, and user feedback functionality.
    /// 
    /// Designed to support the extraction of status and error handling concerns from
    /// the monolithic OstPlayerSidebarViewModel into focused, testable components.
    /// </summary>
    public interface IStatusViewModel : IDisposable
    {
        #region Status Properties

        /// <summary>
        /// Gets the current status message displayed to the user.
        /// </summary>
        string StatusText { get; }

        /// <summary>
        /// Gets the current status type (info, warning, error).
        /// </summary>
        StatusType StatusType { get; }

        /// <summary>
        /// Gets a value indicating whether a status message is currently displayed.
        /// </summary>
        bool HasStatus { get; }

        /// <summary>
        /// Gets a value indicating whether the current status is an error.
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// Gets a value indicating whether the current status is a warning.
        /// </summary>
        bool IsWarning { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets an informational status message.
        /// </summary>
        /// <param name="message">Status message to display</param>
        void SetInfoStatus(string message);

        /// <summary>
        /// Sets a warning status message.
        /// </summary>
        /// <param name="message">Warning message to display</param>
        void SetWarningStatus(string message);

        /// <summary>
        /// Sets an error status message.
        /// </summary>
        /// <param name="message">Error message to display</param>
        void SetErrorStatus(string message);

        /// <summary>
        /// Clears the current status message.
        /// </summary>
        void ClearStatus();

        /// <summary>
        /// Sets a temporary status message that clears after a delay.
        /// </summary>
        /// <param name="message">Status message to display</param>
        /// <param name="type">Type of status message</param>
        /// <param name="durationMs">Duration to display message in milliseconds</param>
        void SetTemporaryStatus(string message, StatusType type, int durationMs = 3000);

        #endregion

        #region Events

        /// <summary>
        /// Raised when the status message changes.
        /// </summary>
        event EventHandler<StatusChangedEventArgs> StatusChanged;

        /// <summary>
        /// Raised when an error status is set.
        /// </summary>
        event EventHandler<string> ErrorOccurred;

        /// <summary>
        /// Raised when a warning status is set.
        /// </summary>
        event EventHandler<string> WarningOccurred;

        #endregion
    }

    /// <summary>
    /// Enumeration of status message types.
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// Informational status message.
        /// </summary>
        Info,

        /// <summary>
        /// Warning status message.
        /// </summary>
        Warning,

        /// <summary>
        /// Error status message.
        /// </summary>
        Error
    }

    /// <summary>
    /// Event arguments for status change events.
    /// </summary>
    public class StatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new status message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the status type.
        /// </summary>
        public StatusType Type { get; }

        /// <summary>
        /// Gets the previous status message.
        /// </summary>
        public string PreviousMessage { get; }

        /// <summary>
        /// Initializes a new instance of the StatusChangedEventArgs class.
        /// </summary>
        public StatusChangedEventArgs(string message, StatusType type, string previousMessage = null)
        {
            Message = message;
            Type = type;
            PreviousMessage = previousMessage;
        }
    }
}