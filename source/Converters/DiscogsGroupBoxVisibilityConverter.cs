// ====================================================================
// FILE: DiscogsGroupBoxVisibilityConverter.cs
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
// WPF multi-value converter that controls visibility of Discogs metadata GroupBox.
// Returns Visible when both user preference is enabled AND Discogs metadata exists.
// Provides conditional display logic for external metadata sections in the UI.
//
// FEATURES:
// - Multi-value boolean and object condition evaluation
// - User preference AND data availability logic
// - Discogs metadata section visibility control
// - GroupBox conditional display management
// - XAML MultiBinding compatibility
//
// DEPENDENCIES:
// - System.Windows.Data (IMultiValueConverter interface)
// - System.Windows (Visibility enum)
// - System.Globalization (CultureInfo for localization)
//
// CONVERTER LOGIC:
// - Input[0]: IsDiscogsMetadataVisible (bool) - user preference
// - Input[1]: DiscogsMetadata (object) - data availability indicator
// - Output: Visibility enum (Visible/Collapsed)
// - Transformation: (preference == true AND metadata != null) ? Visible
// - Use case: Show Discogs metadata only when enabled and data loaded
//
// XAML USAGE:
// <conv:DiscogsGroupBoxVisibilityConverter x:Key="DiscogsGroupBoxVisibilityConverter" />
// <GroupBox>
//   <GroupBox.Visibility>
//     <MultiBinding Converter="{StaticResource DiscogsGroupBoxVisibilityConverter}">
//       <Binding Path="IsDiscogsMetadataVisible" />
//       <Binding Path="DiscogsMetadata" />
//     </MultiBinding>
//   </GroupBox.Visibility>
// </GroupBox>
//
// PERFORMANCE NOTES:
// - Efficient boolean and null reference checks
// - Minimal processing for two-input evaluation
// - Optimized for external metadata loading scenarios
// - No complex object property inspection
//
// LIMITATIONS:
// - One-way conversion only (ConvertBack not implemented)
// - Fixed two-input requirement (IsDiscogsMetadataVisible, DiscogsMetadata)
// - No parameter support for customization
// - Binary visibility logic only
//
// FUTURE REFACTORING:
// FUTURE: Add parameter support for custom metadata validation
// FUTURE: Implement ConvertBack for two-way binding scenarios
// FUTURE: Add support for metadata quality/completeness checks
// FUTURE: Consider generic external metadata visibility converter
// FUTURE: Add metadata loading state visualization
// CONSIDER: Extracting to configurable service metadata converter
// IDEA: Parameter-based metadata validation rules
//
// TESTING:
// - Unit tests for all boolean and null object combinations
// - Unit tests for various metadata object states
// - Unit tests with boolean preference variations
// - XAML MultiBinding integration tests
// - Performance tests for metadata loading scenarios
//
// WPF INTEGRATION:
// - Implements IMultiValueConverter interface
// - XAML MultiBinding compatible
// - External metadata section scenarios optimized
// - GroupBox conditional display for API data
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - XAML 2009
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with Discogs metadata GroupBox visibility logic
// ====================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OstPlayer.Converters
{
    /// <summary>
    /// MultiValueConverter for the Visibility of the GroupBox with Discogs metadata.
    /// Displays the GroupBox only if IsDiscogsMetadataVisible == true and metadata exists (DiscogsMetadata != null).
    ///
    /// Usage in OstPlayer solution:
    /// - OstPlayerSidebarView.xaml: Used as a resource with the key "DiscogsGroupBoxVisibilityConverter".
    ///   Used in MultiBinding for the Visibility of the GroupBox with Discogs metadata.
    /// </summary>
    public class DiscogsGroupBoxVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns Visible only if IsDiscogsMetadataVisible == true and DiscogsMetadata != null.
        /// </summary>
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            // values[0] = IsDiscogsMetadataVisible (bool)
            // values[1] = DiscogsMetadata (object)
            bool isVisible = values[0] is bool b && b;
            bool hasMetadata = values[1] != null;
            return (isVisible && hasMetadata) ? Visibility.Visible : Visibility.Collapsed;
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
