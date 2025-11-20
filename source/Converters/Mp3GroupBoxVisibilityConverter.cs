// ====================================================================
// FILE: Mp3GroupBoxVisibilityConverter.cs
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
// WPF multi-value converter that controls visibility of MP3 metadata GroupBox.
// Returns Visible when both user preference is enabled AND track title exists.
// Provides conditional display logic for MP3 metadata sections in the UI.
//
// FEATURES:
// - Multi-value boolean and string condition evaluation
// - User preference AND data availability logic
// - MP3 metadata section visibility control
// - GroupBox conditional display management
// - XAML MultiBinding compatibility
//
// DEPENDENCIES:
// - System.Windows.Data (IMultiValueConverter interface)
// - System.Windows (Visibility enum)
// - System.Globalization (CultureInfo for localization)
//
// CONVERTER LOGIC:
// - Input[0]: IsMp3MetadataVisible (bool) - user preference
// - Input[1]: TrackTitle (string) - data availability indicator
// - Output: Visibility enum (Visible/Collapsed)
// - Transformation: (preference == true AND title exists) ? Visible
// - Use case: Show MP3 metadata only when enabled and data present
//
// XAML USAGE:
// <conv:Mp3GroupBoxVisibilityConverter x:Key="Mp3GroupBoxVisibilityConverter" />
// <GroupBox>
//   <GroupBox.Visibility>
//     <MultiBinding Converter="{StaticResource Mp3GroupBoxVisibilityConverter}">
//       <Binding Path="IsMp3MetadataVisible" />
//       <Binding Path="TrackTitle" />
//     </MultiBinding>
//   </GroupBox.Visibility>
// </GroupBox>
//
// PERFORMANCE NOTES:
// - Efficient boolean and string null/empty checks
// - Minimal processing for two-input evaluation
// - Optimized for frequent metadata updates
// - No complex string operations or parsing
//
// LIMITATIONS:
// - One-way conversion only (ConvertBack not implemented)
// - Fixed two-input requirement (IsMp3MetadataVisible, TrackTitle)
// - No parameter support for customization
// - Binary visibility logic only
//
// FUTURE REFACTORING:
// FUTURE: Add parameter support for custom field validation
// FUTURE: Implement ConvertBack for two-way binding scenarios
// FUTURE: Add support for additional metadata field validation
// FUTURE: Consider generic metadata section visibility converter
// FUTURE: Add minimum data threshold configuration
// CONSIDER: Extracting to configurable multi-condition converter
// IDEA: Parameter-based field priority and validation rules
//
// TESTING:
// - Unit tests for all boolean and string combinations
// - Unit tests for null/empty string scenarios
// - Unit tests with various boolean states
// - XAML MultiBinding integration tests
// - Performance tests for metadata update scenarios
//
// WPF INTEGRATION:
// - Implements IMultiValueConverter interface
// - XAML MultiBinding compatible
// - GroupBox visibility scenarios optimized
// - Metadata section conditional display
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - XAML 2009
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with MP3 metadata GroupBox visibility logic
// ====================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OstPlayer.Converters
{
    /// <summary>
    /// MultiValueConverter for the Visibility of the GroupBox with MP3 metadata.
    /// Displays the GroupBox only if IsMp3MetadataVisible == true and TrackTitle is filled in.
    ///
    /// Usage in OstPlayer solution:
    /// - OstPlayerSidebarView.xaml: Used as a resource with the key "Mp3GroupBoxVisibilityConverter".
    ///   Used in MultiBinding for the Visibility of the GroupBox with MP3 metadata.
    /// </summary>
    public class Mp3GroupBoxVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns Visible only if IsMp3MetadataVisible == true and TrackTitle is not empty.
        /// </summary>
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            // values[0] = IsMp3MetadataVisible (bool)
            // values[1] = TrackTitle (string)
            bool isVisible = values[0] is bool b && b;
            bool hasTitle = values[1] is string s && !string.IsNullOrEmpty(s);
            return (isVisible && hasTitle) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented, backward conversion is not needed.
        /// </summary>
        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
