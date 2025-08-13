// ====================================================================
// FILE: OstPlayerSidebarView.xaml.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Views
// LOCATION: Views/
// VERSION: 1.2.1
// CREATED: 2025-08-06
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Code-behind for the main sidebar view, implementing MVVM pattern with minimal logic.
// Handles only UI-specific operations and delegates business logic to the ViewModel.
// Enhanced with DataGrid column width persistence functionality for improved user experience.
//
// REFACTORING IMPROVEMENTS:
// - Follows MVVM pattern properly with minimal code-behind
// - Business logic completely moved to ViewModel
// - Better separation of concerns between UI and logic
// - Enhanced metadata refresh functionality
// - Improved event delegation to ViewModel
// - Eliminated redundant Click event handlers
// - Streamlined to UI-specific operations only
//
// COMMAND BINDING INTEGRATION:
// - Removed Click event handlers replaced by Command bindings
// - Maintained only essential UI-specific event handling
// - Clean integration with ViewModel command pattern
// - Preserved complex UI operations requiring code-behind
// - Focus on modal dialogs, external navigation, and filtering
//
// FEATURES:
// - ViewModel event subscription and management
// - UI-specific event handling (mouse, keyboard, focus)
// - Game filtering and search functionality
// - Music playback control delegation
// - Progress slider interaction handling
// - Volume control integration
// - **NEW**: DataGrid column width persistence across sessions
// - Cover image preview functionality
// - Error and information message display
// - Discogs release selection dialog management
//
// EVENT HANDLING:
// - Music file double-click and Enter key playback
// - Game ComboBox filtering and selection
// - Progress slider drag operations
// - Volume slider adjustments
// - External link navigation (Hyperlink.RequestNavigate)
// - Modal dialog management and coordination
//
// VIEW RESPONSIBILITIES:
// - UI event capture and delegation to ViewModel
// - ViewModel event subscription and handling
// - Modal dialog display management
// - External application launching
// - User input validation and routing
// - Visual feedback coordination
// - Complex UI interaction patterns
//
// MVVM INTEGRATION:
// - Clean ViewModel interface through events
// - Command delegation for user actions
// - Property binding for data display
// - Event-driven UI updates
// - Separation of UI and business logic
// - Minimal direct UI manipulation
//
// REFRESH FUNCTIONALITY:
// - All metadata operations delegated to ViewModel commands
// - Clean distinction between load and refresh operations
// - User feedback integration through ViewModel events
// - Error handling delegation to ViewModel
// - No direct API calls or business logic in View
//
// DEPENDENCIES:
// - OstPlayer.ViewModels.OstPlayerSidebarViewModel (business logic)
// - OstPlayer.Views.Dialogs.DiscogsReleaseSelectDialog (metadata selection)
// - OstPlayer.Views.Windows.CoverPreviewWindow (image preview)
// - System.Windows.Controls (WPF framework)
// - System.Diagnostics (external process launching)
//
// DESIGN PATTERNS:
// - MVVM (minimal code-behind with ViewModel delegation)
// - Event Delegation (UI events forwarded to ViewModel)
// - Observer Pattern (ViewModel event subscription)
// - Strategy Pattern (different handlers for different events)
// - Command Pattern (integration with ViewModel commands)
//
// PERFORMANCE NOTES:
// - Minimal UI thread operations
// - Efficient event subscription/unsubscription
// - Lightweight event handlers
// - Proper resource cleanup on unload
// - Reduced overhead from eliminated Click handlers
//
// LIMITATIONS:
// - Single ViewModel instance management
// - Limited direct UI manipulation
// - Dependency on ViewModel for business logic
// - Modal dialog management complexity
// - Some complex UI operations still require code-behind
//
// FUTURE REFACTORING:
// TODO: Extract dialog management to separate service
// TODO: Implement async UI operation indicators
// TODO: Add keyboard shortcut support for refresh operations
// TODO: Extract external process launching to utility
// TODO: Implement UI state persistence across sessions
// TODO: Add drag-and-drop support for music files
// TODO: Extract event handling to behavior classes
// TODO: Implement progressive UI loading for large collections
// TODO: Add context menu support for metadata operations
// TODO: Extract image preview functionality to service
// TODO: Convert remaining event handlers to Behaviors
// TODO: Implement advanced ComboBox filtering with Behaviors
// CONSIDER: Dependency injection for dialog services
// CONSIDER: Complete elimination of code-behind
// IDEA: Real-time UI state synchronization
// IDEA: Visual feedback animations for operations
// IDEA: Integration with system media controls
//
// TESTING:
// - UI automation tests for remaining event handlers
// - ViewModel integration tests
// - Modal dialog interaction tests
// - External process launching tests
// - Memory leak tests for event subscriptions
// - Command binding integration tests
//
// ERROR HANDLING:
// - Graceful ViewModel disposal
// - Safe event unsubscription
// - Exception handling in UI operations
// - User-friendly error message display
// - Robust external navigation handling
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - Windows 7+
// - Playnite SDK 6.x
//
// CHANGELOG:
// 2025-08-07 v1.2.1 - Added DataGrid column width persistence functionality
// 2025-08-06 v1.2.0 - Streamlined after XAML Command Binding refactoring
// 2025-08-06 v1.0.0 - Initial implementation with MVVM pattern
// ====================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using OstPlayer.Models;
using OstPlayer.ViewModels;
using OstPlayer.Views.Dialogs;
using OstPlayer.Views.Windows;
using OstPlayer.Utils;
using Playnite.SDK.Models;

namespace OstPlayer.Views
{
    /// <summary>
    /// Interaction logic for OstPlayerSidebarView.xaml
    /// Enhanced with DataGrid column width persistence functionality.
    /// </summary>
    public partial class OstPlayerSidebarView : UserControl
    {
        #region Private Fields

        private OstPlayerSidebarViewModel viewModel;
        private DataGridColumnPersistence _columnPersistence;

        #endregion

        #region Constructor

        public OstPlayerSidebarView(OstPlayer plugin, Game preselectGame = null)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin), "Plugin parameter is null");

            try
            {
                InitializeComponent();
                viewModel = new OstPlayerSidebarViewModel(plugin, preselectGame);
                DataContext = viewModel;
                ConnectViewModelEvents();
                InitializeColumnPersistence();
                
                Unloaded += OnUnloaded;
                Loaded += OnLoaded;
            }
            catch (Exception ex)
            {
                var logger = Playnite.SDK.LogManager.GetLogger();
                logger.Error(ex, $"OstPlayerSidebarView constructor failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Handles the Loaded event to initialize ViewModel asynchronously.
        /// </summary>
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var logger = Playnite.SDK.LogManager.GetLogger();
            try
            {
                await viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                // Handle initialization errors gracefully
                logger.Error(ex, $"ViewModel initialization failed: {ex.Message}");
                if (viewModel != null)
                {
                    viewModel.StatusText = "Failed to load games. Please try restarting the plugin.";
                }
            }
        }

        /// <summary>
        /// Initializes DataGrid column width persistence functionality.
        /// </summary>
        private void InitializeColumnPersistence()
        {
            try
            {
                // Create column persistence utility
                var plugin = GetPluginFromViewModel();
                if (plugin == null)
                {
                    return;
                }
                
                _columnPersistence = new DataGridColumnPersistence(MusicDataGrid, plugin);
                
                // Load column widths when the control is loaded
                Loaded += (s, e) => {
                    try 
                    {
                        _columnPersistence?.LoadColumnWidths();
                    }
                    catch (Exception ex)
                    {
                        var logger = Playnite.SDK.LogManager.GetLogger();
                        logger.Warn(ex, "Failed to load column widths");
                    }
                };
            }
            catch (Exception ex)
            {
                var logger = Playnite.SDK.LogManager.GetLogger();
                logger.Error(ex, $"Failed to initialize column persistence: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the plugin instance from ViewModel using reflection (since plugin field is private).
        /// </summary>
        private OstPlayer GetPluginFromViewModel()
        {
            try
            {
                var pluginField = typeof(OstPlayerSidebarViewModel).GetField("plugin", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return pluginField?.GetValue(viewModel) as OstPlayer;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Event Subscriptions

        private void ConnectViewModelEvents()
        {
            viewModel.OnShowTrackCoverRequested += ShowTrackCover;
            viewModel.OnShowDiscogsCoverRequested += ShowDiscogsCover;
            viewModel.OnShowErrorRequested += ShowErrorMessage;
            viewModel.OnShowInfoRequested += ShowInfoMessage;
            viewModel.OnSelectDiscogsReleaseRequested += ShowDiscogsReleaseDialog;
        }

        #endregion

        #region UI Event Handlers

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch { }
            e.Handled = true;
        }

        private void DiscogsUrl_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(e.Uri?.AbsoluteUri))
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open link: {ex.Message}");
            }
            e.Handled = true;
        }

        private void GameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.StopCommand.Execute(null);
        }

        private void GameComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Ignore navigation keys
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || 
                e.Key == Key.Tab || e.Key == Key.Escape)
            {
                return;
            }

            FilterAndShowGames();
        }

        private void GameComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var comboBox = GameComboBox;
            var searchText = comboBox.Text?.Trim() ?? "";

            if (!string.IsNullOrEmpty(searchText) && comboBox.SelectedItem == null)
            {
                var matchingGame = viewModel.FindGameByName(searchText);
                if (matchingGame != null)
                {
                    comboBox.SelectedItem = matchingGame;
                }
            }
        }

        private void GameComboBox_DropDownOpened(object sender, EventArgs e)
        {
            FilterGames();
        }

        private void MusicDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedMusicFromDataGrid(sender);
        }

        private void MusicDataGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PlaySelectedMusicFromDataGrid(sender);
            }
        }

        private void PlaySelectedMusicFromDataGrid(object sender)
        {
            var dataGrid = sender as DataGrid;
            var selectedItem = dataGrid?.SelectedItem as TrackListItem;
            if (selectedItem != null)
            {
                viewModel.PlaySelectedMusicFromListBox(selectedItem);
            }
        }

        private void MusicListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedMusicFromListBox(sender);
        }

        private void MusicListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PlaySelectedMusicFromListBox(sender);
            }
        }

        private void PlaySelectedMusicFromListBox(object sender)
        {
            var listBox = sender as ListBox;
            var selectedItem = listBox?.SelectedItem as TrackListItem;
            if (selectedItem != null)
            {
                viewModel.PlaySelectedMusicFromListBox(selectedItem);
            }
        }

        private void ProgressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.SetUserDragging(true);
        }

        private void ProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            viewModel.SetUserDragging(false);
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Position binding handles this automatically
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Volume binding handles this automatically
        }

        #endregion

        #region Game Filtering (UI-specific logic)

        private void FilterGames()
        {
            var comboBox = GameComboBox;
            var searchText = comboBox.Text?.ToLower() ?? "";
            viewModel.FilterGames(searchText);
        }

        private void FilterAndShowGames()
        {
            var comboBox = GameComboBox;
            var searchText = comboBox.Text?.ToLower() ?? "";

            viewModel.FilterGames(searchText);

            if (string.IsNullOrWhiteSpace(searchText))
            {
                comboBox.IsDropDownOpen = false;
            }
            else
            {
                // Reset selection so it doesn't get stuck on the first item
                if (comboBox.SelectedItem != null)
                {
                    var selectedText = comboBox.Text;
                    comboBox.SelectedItem = null;
                    comboBox.Text = selectedText;
                }

                // Open dropdown only if there are results
                if (viewModel.Games.Count > 0)
                {
                    if (!comboBox.IsDropDownOpen)
                    {
                        comboBox.IsDropDownOpen = true;
                    }
                }
                else
                {
                    comboBox.IsDropDownOpen = false;
                }
            }
        }

        #endregion

        #region ViewModel Event Handlers

        private void ShowTrackCover(BitmapImage trackCover)
        {
            var preview = new CoverPreviewWindow(embeddedImage: trackCover);
            preview.Owner = Window.GetWindow(this);
            preview.Show();
        }

        private void ShowDiscogsCover(string coverUrl)
        {
            var preview = new CoverPreviewWindow(imageUrl: coverUrl);
            preview.Owner = Window.GetWindow(this);
            preview.Show();
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private DiscogsMetadataModel ShowDiscogsReleaseDialog(List<DiscogsMetadataModel> results, string token)
        {
            var dialog = new DiscogsReleaseSelectDialog(results, token);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true && dialog.SelectedRelease != null)
            {
                return dialog.SelectedRelease;
            }
            
            return null;
        }

        #endregion

        #region Public Methods (for external access)

        public void StopMusic()
        {
            viewModel?.StopCommand.Execute(null);
        }

        #endregion

        #region Cleanup and Disposal

        /// <summary>
        /// Performs cleanup when the control is being disposed.
        /// Ensures column width persistence is properly saved and disposed.
        /// FIXED: Added music playback stop during cleanup to prevent multiple tracks playing.
        /// </summary>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // FIXED: Stop music playback when the sidebar is being unloaded
                // This prevents multiple tracks playing when user leaves and re-enters OstPlayer
                if (viewModel != null)
                {
                    try
                    {
                        viewModel.StopCommand?.Execute(null);
                        System.Diagnostics.Debug.WriteLine("FIXED: Stopped music playback during sidebar cleanup");
                    }
                    catch (Exception musicEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Failed to stop music during cleanup: {musicEx.Message}");
                        // Continue with other cleanup even if music stop fails
                    }
                }
                
                // Dispose column persistence utility
                _columnPersistence?.Dispose();
                _columnPersistence = null;
                
                // Unsubscribe from events to prevent memory leaks
                if (viewModel != null)
                {
                    viewModel.OnShowTrackCoverRequested -= ShowTrackCover;
                    viewModel.OnShowDiscogsCoverRequested -= ShowDiscogsCover;
                    viewModel.OnShowErrorRequested -= ShowErrorMessage;
                    viewModel.OnShowInfoRequested -= ShowInfoMessage;
                    viewModel.OnSelectDiscogsReleaseRequested -= ShowDiscogsReleaseDialog;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        #endregion
    }
}
