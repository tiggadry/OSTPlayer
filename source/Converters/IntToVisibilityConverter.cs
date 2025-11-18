// ====================================================================
// FILE: IntToVisibilityConverter.cs
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
// WPF value converter that transforms integer values to Visibility based on positive value check.
// Returns Visible when input is greater than 0, Collapsed otherwise.
// Used for displaying numeric data only when it represents meaningful information.
//
// FEATURES:
// - Integer to Visibility conversion based on positive value
// - String to integer parsing with conversion
// - Zero/negative to Visibility.Collapsed conversion
// - Positive integer to Visibility.Visible conversion
// - XAML resource compatibility
//
// DEPENDENCIES:
// - System.Windows.Data (IValueConverter interface)
// - System.Windows (Visibility enum)
// - System.Globalization (CultureInfo for localization)
//
// CONVERTER LOGIC:
// - Input: int or string representing integer
// - Output: Visibility enum (Visible/Collapsed)
// - Transformation: value > 0 ? Visible, value ? 0 ? Collapsed
// - Use case: Show track numbers, counts, indices when meaningful
//
// XAML USAGE:
// <conv:IntToVisibilityConverter x:Key="IntToVisibilityConverter" />
// <TextBlock Text="{Binding TrackNumber}" Visibility="{Binding TrackNumber, Converter={StaticResource IntToVisibilityConverter}}" />
//
// PERFORMANCE NOTES:
// - Efficient integer parsing with TryParse
// - Single comparison operation for conversion
// - Minimal memory allocation
// - Optimized for numeric data binding scenarios
//
// LIMITATIONS:
// - One-way conversion only (ConvertBack not implemented)
// - Fixed threshold at 0 (no parameter customization)
// - No handling of decimal or floating-point numbers
// - Binary visibility logic only
//
// FUTURE REFACTORING:
// FUTURE: Add parameter support for custom threshold values
// FUTURE: Implement ConvertBack for two-way binding scenarios
// FUTURE: Add support for decimal and floating-point numbers
// FUTURE: Add range-based visibility logic (min/max parameters)
// FUTURE: Consider generic numeric converter base class
// CONSIDER: Multi-threshold visibility states
// IDEA: Parameter-based comparison operators (>, >=, <, <=, ==)
//
// TESTING:
// - Unit tests for positive integer input
// - Unit tests for zero and negative integer input
// - Unit tests for string integer parsing
// - Unit tests for invalid string input
// - XAML binding integration tests
// - Performance tests for numeric data scenarios
//
// WPF INTEGRATION:
// - Implements IValueConverter interface
// - XAML resource dictionary compatible
// - Optimized for numeric data binding
// - Track listing and metadata display scenarios
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - XAML 2009
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with integer to visibility conversion
// ====================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OstPlayer.Converters {
    /// <summary>
    /// Converts int (or string representing a number) to Visibility.
    /// Returns Visible if the value > 0, otherwise Collapsed.
    /// Suitable, for example, for displaying numeric data only if it makes sense.
    ///
    /// Usage in OstPlayer solution:
    /// - OstPlayerSidebarView.xaml: Used as a resource with the key "IntToVisibilityConverter".
    ///   Used in DataTemplate for Track List (displaying track number TrackNumber).
    /// </summary>
    public class IntToVisibilityConverter : IValueConverter {
        /// <summary>
        /// If the input value > 0, returns Visible, otherwise Collapsed.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Collapsed;
            int intValue;
            if (int.TryParse(value.ToString(), out intValue)) {
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented, backward conversion is not needed.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
