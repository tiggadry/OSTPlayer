// ====================================================================
// FILE: DiscogsReleaseSelectDialog.xaml.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Views
// LOCATION: Views/Dialogs/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Modal dialog window for searching and selecting music releases from Discogs API.
// Provides user interface for browsing search results and confirming metadata selection
// with async API communication and error handling.
//
// FEATURES:
// - Modal dialog with search and selection capabilities
// - Async Discogs API integration with real-time search
// - List-based result display with detailed metadata
// - Keyboard and mouse interaction support
// - Error handling with user-friendly messages
// - Owner window centering and proper modal behavior
//
// DEPENDENCIES:
// - OstPlayer.Models.DiscogsMetadataModel (search result data)
// - OstPlayer.Clients.DiscogsClient (API communication)
// - System.Windows (Window base class and UI framework)
// - System.Threading.Tasks (async operation support)
//
// UI RESPONSIBILITIES:
// - Search input handling and validation
// - API result display and formatting
// - User selection confirmation and dialog result
// - Error message presentation to user
// - Keyboard shortcut handling (Enter for search)
//
// DESIGN PATTERNS:
// - Modal Dialog Pattern (ShowDialog with result)
// - Command Pattern (button click handlers)
// - Observer Pattern (async/await for API calls)
//
// USER INTERACTION:
// - Text input for search queries
// - List selection for result browsing
// - Double-click or OK button for confirmation
// - Escape or Cancel for dismissal
// - Enter key for quick search execution
//
// ASYNC OPERATIONS:
// - Non-blocking API search calls
// - UI responsiveness during network operations
// - Exception handling for network failures
// - Progress indication through UI updates
//
// PERFORMANCE NOTES:
// - Efficient list rendering for search results
// - Minimal memory allocation during searches
// - Async operations prevent UI freezing
// - Proper resource cleanup on dialog close
//
// LIMITATIONS:
// - No pagination for large result sets
// - Limited search filtering options
// - Basic error handling without retry logic
// - No caching of previous search results
//
// FUTURE REFACTORING:
// FUTURE: Add loading spinner for async search operations
// FUTURE: Implement pagination for large result sets
// FUTURE: Add advanced search filters (year, format, etc.)
// FUTURE: Implement search result caching
// FUTURE: Add retry logic for failed API calls
// FUTURE: Extract search logic to separate service
// FUTURE: Add keyboard navigation for result list
// FUTURE: Implement search history and suggestions
// CONSIDER: Adding preview images for releases
// CONSIDER: Implementing batch selection capabilities
// IDEA: Integration with user's Discogs collection
// IDEA: Advanced search with multiple criteria
//
// TESTING:
// - Dialog workflow tests with mock API responses
// - User interaction tests for keyboard and mouse
// - Async operation tests with network simulation
// - Error handling tests for various failure scenarios
//
// ERROR HANDLING:
// - Network connectivity issues
// - Invalid API token scenarios
// - Empty search result handling
// - Malformed API response recovery
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - Discogs API v2.0
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive Discogs search dialog
// ====================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OstPlayer.Clients;
using OstPlayer.Models;

namespace OstPlayer.Views.Dialogs
{
    /// <summary>
    /// Modal dialog window for searching and selecting music releases from Discogs API.
    /// </summary>
    public partial class DiscogsReleaseSelectDialog : Window
    {
        /// <summary>
        /// Gets the release selected by the user.
        /// </summary>
        public DiscogsMetadataModel SelectedRelease { get; private set; }

        // Personal Discogs API token used for authenticated requests.
        private readonly string _discogsToken;

        // Most recent search results displayed in the list.
        private List<DiscogsMetadataModel> _lastResults;

        /// <summary>
        /// Initializes the dialog with a list of releases and the Discogs token.
        /// </summary>
        /// <param name="releases">Initial search results to display (optional).</param>
        /// <param name="discogsToken">Personal Discogs API token.</param>
        public DiscogsReleaseSelectDialog(List<DiscogsMetadataModel> releases, string discogsToken)
        {
            InitializeComponent();
            ReleaseListBox.ItemsSource = releases;
            _lastResults = releases;
            _discogsToken = discogsToken;
        }

        // Handler for clicking the OK button. Stores the selected release and closes the dialog.
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedRelease = ReleaseListBox.SelectedItem as DiscogsMetadataModel;
            DialogResult = SelectedRelease != null;
            Close();
        }

        // Handler for double-clicking an item in the list. Triggers the same action as OK.
        private void ReleaseListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ReleaseListBox.SelectedItem != null)
            {
                OkButton_Click(sender, e);
            }
        }

        // Handler for clicking the search button. Initiates a new search.
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchAndUpdateResults();
        }

        // Handler for pressing Enter in the search box. Triggers a search.
        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await SearchAndUpdateResults();
            }
        }

        // Performs a Discogs search using the provided query and updates the result list.
        private async Task SearchAndUpdateResults()
        {
            var query = SearchTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return;

            if (string.IsNullOrWhiteSpace(_discogsToken))
            {
                MessageBox.Show("Discogs API token is not set.");
                return;
            }

            ReleaseListBox.ItemsSource = null;
            ReleaseListBox.Items.Refresh();

            try
            {
                var results = await DiscogsClient.SearchReleaseAsync(query, _discogsToken);
                _lastResults = results;
                ReleaseListBox.ItemsSource = results;

                if (results == null || results.Count == 0)
                {
                    MessageBox.Show("No results found on Discogs.");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error while searching Discogs: " + ex.Message);
            }
        }
    }
}
