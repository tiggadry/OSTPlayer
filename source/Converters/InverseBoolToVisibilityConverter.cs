// ====================================================================
// FILE: InverseBoolToVisibilityConverter.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Converters
// LOCATION: Converters/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// WPF value converter that transforms boolean values to inverted Visibility states.
// Returns Collapsed when input is true, Visible when input is false.
// Used for toggle buttons and inverse visibility logic in UI elements.
//
// FEATURES:
// - Inverse boolean to Visibility conversion
// - True to Visibility.Collapsed conversion
// - False to Visibility.Visible conversion
// - Two-way conversion support (ConvertBack implemented)
// - XAML resource compatibility
//
// DEPENDENCIES:
// - System.Windows.Data (IValueConverter interface)
// - System.Windows (Visibility enum)
// - System.Globalization (CultureInfo for localization)
//
// CONVERTER LOGIC:
// - Input: bool (true/false)
// - Output: Visibility enum (Collapsed/Visible)
// - Forward: true ? Collapsed, false ? Visible
// - Backward: Collapsed ? true, Visible ? false
// - Use case: Show/hide toggle buttons with inverse logic
//
// XAML USAGE:
// <conv:InverseBoolToVisibilityConverter x:Key="InverseBoolToVisibilityConverter" />
// <Button Content="Show" Visibility="{Binding IsVisible, Converter={StaticResource InverseBoolToVisibilityConverter}}" />
//
// PERFORMANCE NOTES:
// - Simple boolean comparison operations
// - No complex type checking or parsing
// - Minimal memory allocation
// - Efficient for frequent UI state changes
//
// LIMITATIONS:
// - Boolean input type only
// - Binary visibility logic (no Hidden state)
// - No parameter support for customization
// - Fixed inverse logic behavior
//
// FUTURE REFACTORING:
// TODO: Add parameter support for configurable inversion
// TODO: Add support for Visibility.Hidden state
// TODO: Consider nullable boolean handling
// TODO: Add custom true/false value mapping
// TODO: Extract to generic boolean converter base
// CONSIDER: Multi-value converter for complex boolean logic
// IDEA: Parameter-based visibility state mapping
//
// TESTING:
// - Unit tests for true input (should return Collapsed)
// - Unit tests for false input (should return Visible)
// - ConvertBack tests for both directions
// - XAML binding integration tests
// - Performance tests for toggle scenarios
//
// WPF INTEGRATION:
// - Implements IValueConverter interface
// - Two-way binding support through ConvertBack
// - XAML resource dictionary compatible
// - Toggle button scenarios optimized
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - XAML 2009
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with inverse boolean to visibility conversion
// ====================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OstPlayer.Converters
{
    /// <summary>
    /// Converts bool to Visibility, but inversely.
    /// true ? Collapsed, false ? Visible.
    /// Suitable for buttons like "Show", which should only be displayed when the section is hidden.
    ///
    /// Usage in OstPlayer solution:
    /// - OstPlayerSidebarView.xaml: Used as a resource with the key "InverseBoolToVisibilityConverter".
    ///   Used for buttons "Show MP3 metadata" and "Show Discogs metadata" (Visibility binding).
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// true ? Collapsed, false ? Visible
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        /// <summary>
        /// Collapsed ? true, Visible ? false
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v != Visibility.Visible;
            return true;
        }
    }
}
