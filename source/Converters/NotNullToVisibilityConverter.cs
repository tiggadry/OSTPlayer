// ====================================================================
// FILE: NotNullToVisibilityConverter.cs
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
// WPF value converter that transforms non-null objects to Visibility enum values.
// Returns Visible when input has a value, Collapsed when input is null.
// Used for showing UI elements only when data is available and loaded.
//
// FEATURES:
// - Non-null to Visibility.Visible conversion
// - Null to Visibility.Collapsed conversion
// - Type-agnostic object handling
// - XAML resource compatibility
// - ValueConversion attribute support
//
// DEPENDENCIES:
// - System.Windows.Data (IValueConverter interface)
// - System.Windows (Visibility enum)
// - System.Globalization (CultureInfo for localization)
//
// CONVERTER LOGIC:
// - Input: object (any type)
// - Output: Visibility enum (Visible/Collapsed)
// - Transformation: non-null ? Visible, null ? Collapsed
// - Use case: Show content only when data is available
//
// XAML USAGE:
// <conv:NotNullToVisibilityConverter x:Key="NotNullToVisibilityConverter" />
// <StackPanel Visibility="{Binding DiscogsMetadata, Converter={StaticResource NotNullToVisibilityConverter}}" />
//
// PERFORMANCE NOTES:
// - Single null comparison operation
// - No complex type checking or casting
// - Minimal memory footprint
// - Optimized for high-frequency UI updates
//
// LIMITATIONS:
// - One-way conversion only (ConvertBack not implemented)
// - Binary visibility logic (no Hidden state)
// - No parameter support for customization
// - No culture-specific behavior
//
// FUTURE REFACTORING:
// TODO: Add parameter support for inversion behavior
// TODO: Implement ConvertBack for two-way binding scenarios
// TODO: Add support for Visibility.Hidden state
// TODO: Consider value type vs reference type distinction
// TODO: Add custom null-equivalent value support
// CONSIDER: Combining with other object validation converters
// IDEA: Parameter-based custom visibility states
//
// TESTING:
// - Unit tests for null input scenarios
// - Unit tests for non-null input scenarios
// - Value type vs reference type tests
// - XAML binding integration tests
// - Performance tests for data binding
//
// WPF INTEGRATION:
// - Implements IValueConverter interface
// - ValueConversion attribute for design-time support
// - XAML resource dictionary compatible
// - Data binding pipeline optimized
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - XAML 2009
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with non-null to visibility conversion
// ====================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OstPlayer.Converters
{
    /// <summary>
    /// Converts a non-null object to Visibility. Returns Visible if the object is not null, otherwise Collapsed.
    /// Suitable for displaying elements only if the object is set (e.g., DiscogsMetadata).
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NotNullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If the input value is not null, returns Visible, otherwise Collapsed.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
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
