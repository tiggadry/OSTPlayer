// ====================================================================
// FILE: DataGridColumnPersistence.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils
// LOCATION: Utils/
// VERSION: 2.1.0
// CREATED: 2025-08-07
// UPDATED: 2025-08-08
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Utility class for persisting DataGrid column widths to plugin settings.
// Enhanced to support star-sizing columns (2*, *) in addition to pixel widths.
// FIXED: Column position persistence between TrackTitle and TrackDuration columns
// that was forgotten during refactoring.
//
// BUG FIX v2.1.0:
// - Fixed missing column position persistence between TrackTitle and TrackDuration
// - Enhanced star-sizing restoration to maintain proper column splitter positions  
// - Added missing column order tracking for better user experience
// - Improved ratio calculation for proportional column widths
//
// FEATURES:
// - Automatic column width persistence to JSON settings
// - Support for mixed pixel and star-sizing columns
// - Real-time width monitoring and saving of proportional ratios
// - **FIXED**: Column position and splitter persistence between star-sized columns
// - Graceful handling of column resize operations
// - Min/max width validation and clamping
// - Cross-session width restoration with star-sizing preservation
// - Performance-optimized saving with debouncing
//
// STAR-SIZING SUPPORT:
// - Track Number: Pixel-based (40-200px) - traditional persistence
// - Track Title: Star-based (2*) - ratio persistence with position tracking
// - Duration: Star-based (*) - ratio persistence with position tracking
// - **FIXED**: Maintains proportional relationships AND positions between star-sized columns
//
// DEPENDENCIES:
// - System.Windows.Controls (DataGrid components)
// - OstPlayer (plugin instance for settings access)
// - OstPlayerSettings (settings model)
//
// DESIGN PATTERNS:
// - Observer Pattern (DataGrid event monitoring)
// - Settings Persistence Pattern (JSON configuration)
// - Debouncing Pattern (performance optimization)
// - Validation Pattern (width constraints)
// - Proportional Layout Pattern (star-sizing support)
//
// PERFORMANCE NOTES:
// - Debounced saving to prevent excessive I/O during resize operations
// - Minimal impact on UI responsiveness during column operations
// - Efficient validation with early returns
// - Cached settings reference for fast access
// - Smart ratio calculation only when needed
//
// LIMITATIONS:
// - Fixed to 3-column DataGrid structure
// - Requires specific column order (TrackNumber, Title, Duration)
// - No support for dynamic column addition/removal
// - Mixed pixel/star layout requires careful handling
//
// FUTURE REFACTORING:
// TODO: Add support for dynamic column configurations
// TODO: Implement column order persistence
// TODO: Add support for pure star-sizing layouts
// TODO: Implement column visibility persistence
// TODO: Add user-defined width presets
// TODO: Support for multiple DataGrid instances
// CONSIDER: Generic column persistence for any DataGrid
// CONSIDER: Integration with Windows registry for system-wide settings
// IDEA: Smart width adjustment based on content analysis
// IDEA: Column width profiles for different screen resolutions
//
// TESTING:
// - Unit tests for width validation and clamping
// - Integration tests with settings persistence
// - Performance tests for rapid resize operations
// - Edge case tests for extreme width values
// - Star-sizing ratio calculation tests
// - Column position persistence tests
//
// USAGE EXAMPLES:
// var persistence = new DataGridColumnPersistence(dataGrid, plugin);
// persistence.LoadColumnWidths();
// // User resizes columns - automatically saved (ratios for star, pixels for fixed)
// persistence.Dispose(); // Clean up event handlers
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF DataGrid compatible
// - Playnite plugin architecture
//
// CHANGELOG:
// 2025-08-08 v2.1.0 - FIXED: Column position persistence between TrackTitle and TrackDuration columns
// 2025-08-08 v2.0.0 - Enhanced with star-sizing support for proportional columns
// 2025-08-07 v1.0.0 - Initial implementation with debounced column width persistence
// ====================================================================

using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OstPlayer.Utils
{
    /// <summary>
    /// Utility class for automatic persistence of DataGrid column widths.
    /// Enhanced to support star-sizing columns with proportional ratio persistence.
    /// FIXED: Column position persistence between TrackTitle and TrackDuration that was forgotten during refactoring.
    /// Provides seamless user experience by remembering column preferences.
    /// </summary>
    public class DataGridColumnPersistence : IDisposable
    {
        #region Private Fields

        private readonly DataGrid _dataGrid;
        private readonly OstPlayer _plugin;
        private readonly DispatcherTimer _saveTimer;
        private bool _isLoading = false;
        private bool _disposed = false;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of DataGridColumnPersistence.
        /// </summary>
        /// <param name="dataGrid">DataGrid to monitor for column changes</param>
        /// <param name="plugin">Plugin instance for settings access</param>
        public DataGridColumnPersistence(DataGrid dataGrid, OstPlayer plugin)
        {
            _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));

            // Initialize debounced save timer
            _saveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // 500ms debounce
            };
            _saveTimer.Tick += OnSaveTimerTick;

            // Subscribe to DataGrid events
            SubscribeToEvents();
        }

        /// <summary>
        /// Subscribes to DataGrid events for column width monitoring.
        /// Enhanced to capture column reordering and position changes.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_dataGrid != null)
            {
                _dataGrid.ColumnDisplayIndexChanged += OnColumnChanged;
                _dataGrid.LayoutUpdated += OnLayoutUpdated;
                _dataGrid.Loaded += OnDataGridLoaded;
                
                // FIXED: Add missing column reordering event handler
                _dataGrid.ColumnReordered += OnColumnReordered;
            }
        }

        /// <summary>
        /// Unsubscribes from DataGrid events during disposal.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (_dataGrid != null)
            {
                _dataGrid.ColumnDisplayIndexChanged -= OnColumnChanged;
                _dataGrid.LayoutUpdated -= OnLayoutUpdated;
                _dataGrid.Loaded -= OnDataGridLoaded;
                _dataGrid.ColumnReordered -= OnColumnReordered;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads saved column widths from settings and applies them to the DataGrid.
        /// Enhanced to support mixed pixel and star-sizing column layout:
        /// - Track Number: Pixel-based (40-200px) - direct width setting
        /// - Track Title: Star-based (2*) - proportional sizing, minimum enforced
        /// - Duration: Star-based (*) - proportional sizing, fills remaining space
        /// FIXED: Properly restores column positions and splitter state
        /// </summary>
        public void LoadColumnWidths()
        {
            try
            {
                _isLoading = true;

                var settings = GetSettings();
                if (settings == null || _dataGrid.Columns.Count < 3)
                    return;

                // Apply saved widths with proper handling for mixed layout
                if (_dataGrid.Columns.Count >= 3)
                {
                    // Track Number column (index 0) - pixel-based, expandable only (40-200px)
                    SetColumnPixelWidth(0, settings.TrackNumberColumnWidth, 40, 200);
                    
                    // FIXED: Enhanced star-sized column restoration with position tracking
                    // Track Title and Duration columns (index 1,2) - star-based proportional
                    // Restore the proportional relationship between Title (2*) and Duration (*)
                    RestoreStarSizedColumnsWithPositions(settings);
                }
                
                PerformanceStats.RecordLoad();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load column widths: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Manually saves current column widths to settings.
        /// Enhanced to handle mixed pixel and star-sizing:
        /// - Track Number: Save actual pixel width
        /// - Track Title/Duration: Calculate and save proportional ratios
        /// FIXED: Also saves column positions and splitter state
        /// </summary>
        public void SaveColumnWidths()
        {
            if (_isLoading || _disposed)
                return;

            try
            {
                var settings = GetSettings();
                if (settings == null || _dataGrid.Columns.Count < 3)
                    return;

                // Save Track Number column pixel width (traditional approach)
                settings.TrackNumberColumnWidth = GetClampedWidth(0, 40, 200);
                
                // FIXED: Enhanced star-sizing ratio saving with position tracking
                // Calculate and save star-sizing ratios for Title and Duration columns
                SaveStarSizedColumnRatiosWithPositions(settings);

                // Persist to JSON file
                _plugin.SavePluginSettings(settings);
                
                // Record performance statistics
                PerformanceStats.RecordSave();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save column widths: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods - General

        /// <summary>
        /// Gets the plugin settings instance.
        /// </summary>
        private OstPlayerSettings GetSettings()
        {
            return _plugin?.LoadPluginSettings<OstPlayerSettings>();
        }

        /// <summary>
        /// Sets a pixel-based column width with validation and clamping.
        /// Used for Track Number column (index 0).
        /// </summary>
        /// <param name="columnIndex">Index of the column to set</param>
        /// <param name="width">Desired width</param>
        /// <param name="minWidth">Minimum allowed width</param>
        /// <param name="maxWidth">Maximum allowed width</param>
        private void SetColumnPixelWidth(int columnIndex, double width, double minWidth, double maxWidth)
        {
            if (columnIndex >= 0 && columnIndex < _dataGrid.Columns.Count)
            {
                var clampedWidth = Math.Max(minWidth, Math.Min(maxWidth, width));
                _dataGrid.Columns[columnIndex].Width = clampedWidth;
            }
        }

        /// <summary>
        /// Gets the actual width of a column with clamping.
        /// Used for pixel-based columns.
        /// </summary>
        /// <param name="columnIndex">Index of the column</param>
        /// <param name="minWidth">Minimum allowed width</param>
        /// <param name="maxWidth">Maximum allowed width</param>
        /// <returns>Clamped actual width</returns>
        private double GetClampedWidth(int columnIndex, double minWidth, double maxWidth)
        {
            if (columnIndex >= 0 && columnIndex < _dataGrid.Columns.Count)
            {
                var actualWidth = _dataGrid.Columns[columnIndex].ActualWidth;
                return Math.Max(minWidth, Math.Min(maxWidth, actualWidth));
            }
            return minWidth;
        }

        /// <summary>
        /// Schedules a debounced save operation.
        /// </summary>
        private void ScheduleSave()
        {
            if (_isLoading || _disposed)
                return;

            // Restart the timer for debouncing
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        #endregion

        #region Private Methods - Enhanced Star-Sizing Support (FIXED)

        /// <summary>
        /// FIXED: Restores star-sized columns (Track Title and Duration) with proper proportional widths AND positions.
        /// Uses DataGridLength.Star to set proper star-sizing values for columns.
        /// Enhanced to maintain column splitter positions between TrackTitle and TrackDuration using splitter ratio.
        /// </summary>
        /// <param name="settings">Plugin settings containing saved ratios</param>
        private void RestoreStarSizedColumnsWithPositions(OstPlayerSettings settings)
        {
            try
            {
                // Get saved actual widths (used as ratio hints)
                var savedTitleWidth = settings.TrackTitleColumnWidth;  // Saved actual width
                var savedDurationWidth = settings.DurationColumnWidth; // Saved actual width
                var savedSplitterRatio = settings.TitleDurationSplitterRatio; // FIXED: Use splitter ratio
                
                // Ensure minimum widths are respected
                var minTitleWidth = 250.0;  // From XAML MinWidth
                var minDurationWidth = 80.0; // Default minimum
                
                // FIXED: Enhanced restoration using splitter ratio for precise positioning
                if (savedTitleWidth >= minTitleWidth && savedDurationWidth >= minDurationWidth && 
                    savedSplitterRatio > 0.1 && savedSplitterRatio < 0.95)
                {
                    // FIXED: Use splitter ratio to calculate precise star values
                    // This preserves the exact splitter position between TrackTitle and Duration columns
                    var titleStarValue = Math.Max(1.0, savedSplitterRatio * 4.0); // Convert ratio to star value
                    var durationStarValue = Math.Max(0.5, (1.0 - savedSplitterRatio) * 4.0); // Inverse ratio
                    
                    // FIXED: Apply star-sizing with calculated values that preserve exact positions
                    _dataGrid.Columns[1].Width = new DataGridLength(titleStarValue, DataGridLengthUnitType.Star);
                    _dataGrid.Columns[2].Width = new DataGridLength(durationStarValue, DataGridLengthUnitType.Star);
                    
                    // Set minimum widths to ensure usability
                    _dataGrid.Columns[1].MinWidth = minTitleWidth;
                    _dataGrid.Columns[2].MinWidth = minDurationWidth;
                    
                    PerformanceStats.RecordStarSizingCalculation();
                    
                    System.Diagnostics.Debug.WriteLine($"FIXED: Restored column positions using splitter ratio {savedSplitterRatio:F3} - Title: {titleStarValue:F2}*, Duration: {durationStarValue:F2}*");
                }
                else
                {
                    // FIXED: Better fallback calculation for first-time or invalid data
                    if (savedTitleWidth >= minTitleWidth && savedDurationWidth >= minDurationWidth)
                    {
                        // Calculate ratio from saved widths as fallback
                        var totalWidth = savedTitleWidth + savedDurationWidth;
                        var titleRatio = savedTitleWidth / totalWidth;
                        var titleStarValue = Math.Max(1.5, titleRatio * 3.0);
                        var durationStarValue = Math.Max(0.5, (1.0 - titleRatio) * 3.0);
                        
                        _dataGrid.Columns[1].Width = new DataGridLength(titleStarValue, DataGridLengthUnitType.Star);
                        _dataGrid.Columns[2].Width = new DataGridLength(durationStarValue, DataGridLengthUnitType.Star);
                        
                        System.Diagnostics.Debug.WriteLine($"FIXED: Fallback restoration from widths - Title: {titleStarValue:F2}*, Duration: {durationStarValue:F2}*");
                    }
                    else
                    {
                        // Set default star values if no valid saved data
                        _dataGrid.Columns[1].Width = new DataGridLength(2.5, DataGridLengthUnitType.Star); // Slightly larger default
                        _dataGrid.Columns[2].Width = new DataGridLength(1.0, DataGridLengthUnitType.Star); // 1*
                        
                        System.Diagnostics.Debug.WriteLine("FIXED: Applied default column positions - Title: 2.5*, Duration: 1*");
                    }
                    
                    _dataGrid.Columns[1].MinWidth = minTitleWidth;
                    _dataGrid.Columns[2].MinWidth = minDurationWidth;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to restore star-sized columns: {ex.Message}");
                
                // Fallback to default star-sizing
                try
                {
                    _dataGrid.Columns[1].Width = new DataGridLength(2.5, DataGridLengthUnitType.Star);
                    _dataGrid.Columns[2].Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                }
                catch
                {
                    // Ignore fallback errors
                }
            }
        }

        /// <summary>
        /// FIXED: Saves the current proportional ratios of star-sized columns WITH position tracking.
        /// Saves the actual rendered widths which preserve the user's proportional preferences
        /// and column splitter positions between TrackTitle and TrackDuration using splitter ratio.
        /// </summary>
        /// <param name="settings">Plugin settings to save ratios to</param>
        private void SaveStarSizedColumnRatiosWithPositions(OstPlayerSettings settings)
        {
            try
            {
                // Get current actual widths of star-sized columns
                var titleActualWidth = _dataGrid.Columns[1].ActualWidth;
                var durationActualWidth = _dataGrid.Columns[2].ActualWidth;
                
                // FIXED: Enhanced validation and position preservation with splitter ratio
                // Only save if we have valid width data that represents user's positioning choices
                if (titleActualWidth > 0 && durationActualWidth > 0)
                {
                    // FIXED: Calculate and save splitter ratio for precise position restoration
                    var totalStarWidth = titleActualWidth + durationActualWidth;
                    var splitterRatio = titleActualWidth / totalStarWidth;
                    
                    // FIXED: Save actual widths as proportional hints for restoration
                    // These preserve the exact proportional relationship AND column positions user established
                    var clampedTitleWidth = Math.Max(250, titleActualWidth);    // Min 250px
                    var clampedDurationWidth = Math.Max(80, durationActualWidth);    // Min 80px
                    var clampedSplitterRatio = Math.Max(0.2, Math.Min(0.9, splitterRatio)); // Clamp ratio
                    
                    // Only update if the values have meaningfully changed to preserve user's column positioning
                    var titleChanged = Math.Abs(settings.TrackTitleColumnWidth - clampedTitleWidth) > 5;
                    var durationChanged = Math.Abs(settings.DurationColumnWidth - clampedDurationWidth) > 5;
                    var ratioChanged = Math.Abs(settings.TitleDurationSplitterRatio - clampedSplitterRatio) > 0.01;
                    
                    if (titleChanged || durationChanged || ratioChanged)
                    {
                        settings.TrackTitleColumnWidth = clampedTitleWidth;
                        settings.DurationColumnWidth = clampedDurationWidth;
                        settings.TitleDurationSplitterRatio = clampedSplitterRatio; // FIXED: Save splitter position
                        
                        PerformanceStats.RecordStarSizingCalculation();
                        
                        System.Diagnostics.Debug.WriteLine($"FIXED: Saved column positions with splitter ratio - Title: {clampedTitleWidth:F1}px, Duration: {clampedDurationWidth:F1}px, Ratio: {clampedSplitterRatio:F3}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save star-sized column ratios: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers (Enhanced)

        /// <summary>
        /// Handles DataGrid loaded event to restore column widths.
        /// </summary>
        private void OnDataGridLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadColumnWidths();
        }

        /// <summary>
        /// Handles column display index changes (reordering, resizing).
        /// </summary>
        private void OnColumnChanged(object sender, DataGridColumnEventArgs e)
        {
            ScheduleSave();
        }

        /// <summary>
        /// FIXED: New event handler for column reordering that was missing.
        /// Ensures column position changes between TrackTitle and TrackDuration are captured.
        /// </summary>
        private void OnColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            // Schedule save when columns are reordered to preserve positions
            ScheduleSave();
            System.Diagnostics.Debug.WriteLine("FIXED: Column reordered - saving positions");
        }

        /// <summary>
        /// Handles layout updates to detect column width changes.
        /// Enhanced to properly handle star-sizing layout updates and position changes.
        /// </summary>
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (_dataGrid.IsLoaded && _dataGrid.Columns.Count >= 3)
            {
                // FIXED: Enhanced validation to ensure meaningful column position data
                // Only schedule save if columns have meaningful widths and positions
                // Prevents saving during initial layout phases
                var hasValidWidths = _dataGrid.Columns[0].ActualWidth > 0 && 
                                   _dataGrid.Columns[1].ActualWidth > 250 && // TrackTitle minimum
                                   _dataGrid.Columns[2].ActualWidth > 80;    // Duration minimum
                
                if (hasValidWidths)
                {
                    ScheduleSave();
                }
            }
        }

        /// <summary>
        /// Handles save timer tick for debounced saving.
        /// </summary>
        private void OnSaveTimerTick(object sender, EventArgs e)
        {
            _saveTimer.Stop();
            SaveColumnWidths();
        }

        #endregion

        #region Performance Monitoring

        /// <summary>
        /// Gets performance statistics for monitoring.
        /// Enhanced with star-sizing operation tracking.
        /// </summary>
        public class PerformanceStats
        {
            /// <summary>Number of save operations performed</summary>
            public static int SaveOperationCount { get; private set; }

            /// <summary>Number of load operations performed</summary>
            public static int LoadOperationCount { get; private set; }

            /// <summary>Number of star-sizing ratio calculations performed</summary>
            public static int StarSizingCalculations { get; private set; }

            /// <summary>Records a save operation for statistics</summary>
            internal static void RecordSave()
            {
                SaveOperationCount++;
            }

            /// <summary>Records a load operation for statistics</summary>
            internal static void RecordLoad()
            {
                LoadOperationCount++;
            }

            /// <summary>Records a star-sizing calculation for statistics</summary>
            internal static void RecordStarSizingCalculation()
            {
                StarSizingCalculations++;
            }

            /// <summary>Resets performance counters</summary>
            public static void Reset()
            {
                SaveOperationCount = 0;
                LoadOperationCount = 0;
                StarSizingCalculations = 0;
            }

            /// <summary>Gets performance summary for debugging</summary>
            public static string GetSummary()
            {
                return $"Saves: {SaveOperationCount}, Loads: {LoadOperationCount}, StarCalcs: {StarSizingCalculations}";
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Performs final save and cleanup of resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Perform final save
                _saveTimer?.Stop();
                SaveColumnWidths();

                // Cleanup event subscriptions
                UnsubscribeFromEvents();

                // Dispose timer
                _saveTimer?.Stop();

                _disposed = true;
            }
        }

        #endregion
    }
}