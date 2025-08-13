// ====================================================================
// FILE: NullToVisibilityConverter.cs
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
// WPF value converter that transforms null objects to Visibility enum values.
// Returns Visible when input is null, Collapsed when input has a value.
// Useful for showing UI elements only when data is missing or unloaded.
//
// FEATURES:
// - Null to Visibility.Visible conversion
// - Non-null to Visibility.Collapsed conversion
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
// - Transformation: null ? Visible, non-null ? Collapsed
// - Use case: Show placeholder when content is missing
//
// XAML USAGE:
// <conv:NullToVisibilityConverter x:Key="NullToVisibilityConverter" />
// <TextBlock Visibility="{Binding SomeProperty, Converter={StaticResource NullToVisibilityConverter}}" />
//
// PERFORMANCE NOTES:
// - Lightweight conversion operation
// - Single null check comparison
// - No memory allocation during conversion
// - Suitable for frequent UI updates
//
// LIMITATIONS:
// - One-way conversion only (ConvertBack not implemented)
// - No parameter support for customization
// - Binary visibility logic (no Hidden state)
// - No culture-specific behavior
//
// FUTURE REFACTORING:
// TODO: Add parameter support for custom visibility behavior
// TODO: Implement ConvertBack for two-way binding scenarios
// TODO: Add support for Visibility.Hidden state
// TODO: Consider generic converter base class
// TODO: Add culture-aware conversion options
// CONSIDER: Combining with other null-checking converters
// IDEA: Parameter-based inversion of conversion logic
//
// TESTING:
// - Unit tests for null input scenarios
// - Unit tests for non-null input scenarios
// - XAML binding integration tests
// - Performance tests for high-frequency updates
//
// WPF INTEGRATION:
// - Implements IValueConverter interface
// - ValueConversion attribute for design-time support
// - XAML resource dictionary compatible
// - Data binding pipeline compatible
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - XAML 2009
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with null-to-visibility conversion
// ====================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OstPlayer.Converters
{
    /// <summary>
    /// WPF value converter that transforms null objects to Visibility enum values for conditional UI display.
    /// Implements the inverse of typical null-to-collapsed behavior by showing elements when data is missing.
    /// Essential for placeholder text, loading indicators, and "no data" messaging scenarios.
    /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.ivalueconverter
    /// </summary>
    /// <remarks>
    /// This converter is particularly useful in metadata display scenarios where you want to show
    /// a "Load from Discogs" button only when no Discogs metadata has been loaded yet.
    /// The inverse logic (visible when null) distinguishes it from standard null converters.
    /// </remarks>
    /// <example>
    /// XAML Usage:
    /// <![CDATA[
    /// <Button Content="Load Metadata" 
    ///         Visibility="{Binding DiscogsMetadata, Converter={StaticResource NullToVisibilityConverter}}" />
    /// ]]>
    /// When DiscogsMetadata is null: Button is Visible
    /// When DiscogsMetadata has value: Button is Collapsed
    /// </example>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts an input value to Visibility based on null state.
        /// Implements inverse null logic: null values become Visible, non-null values become Collapsed.
        /// </summary>
        /// <param name="value">The source value to evaluate for null state (any object type)</param>
        /// <param name="targetType">The target type (should be Visibility, but not enforced)</param>
        /// <param name="parameter">Optional parameter (currently unused, reserved for future enhancements)</param>
        /// <param name="culture">Culture information for localization (unused in this converter)</param>
        /// <returns>
        /// <see cref="Visibility.Visible"/> if value is null,
        /// <see cref="Visibility.Collapsed"/> if value is not null
        /// </returns>
        /// <remarks>
        /// This method performs a simple null check using the == operator, which works correctly
        /// for both reference types and nullable value types. The conversion is culture-agnostic
        /// and does not allocate memory, making it suitable for high-frequency UI updates.
        /// </remarks>
        /// <example>
        /// Direct usage (not typically needed):
        /// <code>
        /// var converter = new NullToVisibilityConverter();
        /// var result1 = converter.Convert(null, typeof(Visibility), null, null);        // Returns Visibility.Visible
        /// var result2 = converter.Convert("data", typeof(Visibility), null, null);      // Returns Visibility.Collapsed
        /// var result3 = converter.Convert(42, typeof(Visibility), null, null);          // Returns Visibility.Collapsed
        /// </code>
        /// </example>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Primary conversion logic: null check with inverse visibility behavior
            // This is the core logic that distinguishes this converter from typical null converters
            return value == null ? Visibility.Visible : Visibility.Collapsed;
            
            // Alternative implementations could support parameters for customization:
            // if (parameter?.ToString() == "Hidden")
            //     return value == null ? Visibility.Visible : Visibility.Hidden;
            // 
            // Future enhancement: parameter-based inversion
            // if (parameter?.ToString() == "Invert")
            //     return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Reverse conversion from Visibility to null/non-null state (not implemented).
        /// Throws NotImplementedException as backward conversion is not meaningful for this converter.
        /// </summary>
        /// <param name="value">The Visibility value to convert back</param>
        /// <param name="targetType">The target type for backward conversion</param>
        /// <param name="parameter">Optional parameter (unused)</param>
        /// <param name="culture">Culture information (unused)</param>
        /// <returns>Nothing - throws NotImplementedException</returns>
        /// <exception cref="NotImplementedException">
        /// Always thrown as this converter is designed for one-way binding only.
        /// Converting from Visibility back to null/non-null state is ambiguous and not supported.
        /// </exception>
        /// <remarks>
        /// ConvertBack is not implemented because:
        /// 1. The conversion is ambiguous (Collapsed could map to any non-null object)
        /// 2. This converter is typically used in read-only binding scenarios
        /// 3. The source property type is unknown at conversion time
        /// 
        /// If two-way binding is needed, consider using a different approach or
        /// implementing a custom converter with knowledge of the source type.
        /// </remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is intentionally not implemented for this converter
            // Reasoning: Converting Visibility back to object state is ambiguous
            // - Visibility.Visible should become null (clear)
            // - Visibility.Collapsed should become... what object? (ambiguous)
            // - The target type and appropriate non-null value are unknown
            
            throw new NotImplementedException(
                "NullToVisibilityConverter does not support backward conversion. " +
                "Use one-way binding or implement a custom two-way converter if needed.");
        }
    }
}
