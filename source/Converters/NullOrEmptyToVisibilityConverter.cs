// ====================================================================
// FILE: NullOrEmptyToVisibilityConverter.cs
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
// WPF value converter that transforms string values to Visibility based on null/empty state.
// Returns Collapsed when string is null or empty, Visible when string contains data.
// Essential for conditional display of UI elements based on string content availability.
//
// FEATURES:
// - String null/empty check to Visibility conversion
// - Non-empty string to Visibility.Visible conversion
// - Empty/null string to Visibility.Collapsed conversion
// - XAML resource compatibility
// - ValueConversion attribute support
//
// DEPENDENCIES:
// - System.Windows.Data (IValueConverter interface)
// - System.Windows (Visibility enum)
// - System.Globalization (CultureInfo for localization)
//
// CONVERTER LOGIC:
// - Input: string (text content)
// - Output: Visibility enum (Visible/Collapsed)
// - Transformation: null/empty ? Collapsed, non-empty ? Visible
// - Use case: Show content only when meaningful text exists
//
// XAML USAGE:
// <conv:NullOrEmptyToVisibilityConverter x:Key="NullOrEmptyToVisibilityConverter" />
// <Image Visibility="{Binding CoverUrl, Converter={StaticResource NullOrEmptyToVisibilityConverter}}" />
//
// PERFORMANCE NOTES:
// - Efficient string.IsNullOrEmpty() check
// - No string parsing or complex operations
// - Minimal memory allocation
// - Optimized for frequent UI updates
//
// LIMITATIONS:
// - One-way conversion only (ConvertBack not implemented)
// - String-specific input type
// - No whitespace-only string handling
// - Binary visibility logic only
//
// FUTURE REFACTORING:
// TODO: Add whitespace trimming option via parameter
// TODO: Implement ConvertBack for two-way binding scenarios
// TODO: Add support for Visibility.Hidden state
// TODO: Consider generic text validation converter
// TODO: Add minimum length parameter support
// CONSIDER: Regex pattern matching capability
// IDEA: Multi-value converter for complex string conditions
//
// TESTING:
// - Unit tests for null string input
// - Unit tests for empty string input
// - Unit tests for non-empty string input
// - XAML binding integration tests
// - Performance tests for UI binding scenarios
//
// WPF INTEGRATION:
// - Implements IValueConverter interface
// - ValueConversion attribute for design-time support
// - XAML resource dictionary compatible
// - Optimized for data binding scenarios
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - XAML 2009
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with string validation to visibility conversion
// ====================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OstPlayer.Converters
{
    /// <summary>
    /// Converts a string to Visibility. Returns Collapsed if the string is null or empty, otherwise Visible.
    /// Used, for example, to display an element only if the text is filled in.
    ///
    /// Usage in OstPlayer solution:
    /// - OstPlayerSidebarView.xaml: Used as a resource with the key "NullOrEmptyToVisibilityConverter".
    ///   Used, for example, to display the cover image from Discogs (Visibility binding to CoverUrl).
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If the input value is null or an empty string, returns Collapsed, otherwise Visible.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Not implemented, backward conversion is not needed.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
