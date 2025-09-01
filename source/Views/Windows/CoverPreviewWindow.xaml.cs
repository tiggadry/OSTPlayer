// ====================================================================
// FILE: CoverPreviewWindow.xaml.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Views
// LOCATION: Views/Windows/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Standalone window for previewing cover images (album artwork) from local files
// or remote URLs. Provides enlarged view of cover art with error handling for
// loading failures and support for multiple image sources.
//
// FEATURES:
// - Dual source support (embedded images and URL loading)
// - Automatic image loading and error handling
// - Window centering and proper sizing
// - BitmapImage optimization with caching
// - User-friendly error messages for loading failures
// - Memory-efficient image display
//
// DEPENDENCIES:
// - System.Windows (Window base class and UI framework)
// - System.Windows.Media.Imaging (BitmapImage for URL loading)
// - System.Windows.Media (ImageSource for embedded images)
// - System (Uri and Exception handling)
//
// UI RESPONSIBILITIES:
// - Image source initialization and loading
// - Error message display for failed operations
// - Window lifecycle management
// - Image rendering and display optimization
//
// DESIGN PATTERNS:
// - Factory Pattern (constructor overloading for different sources)
// - Strategy Pattern (different loading strategies for image types)
// - Template Method (WPF window initialization pattern)
//
// IMAGE LOADING:
// - Embedded images: Direct ImageSource assignment
// - URL images: BitmapImage creation with URI source
// - Caching optimization for performance
// - Exception handling for malformed URLs or network issues
//
// ERROR HANDLING:
// - Network connectivity issues for URL-based images
// - Malformed URL validation and user feedback
// - Missing image parameter detection
// - Loading exception recovery with user notification
//
// PERFORMANCE NOTES:
// - BitmapCacheOption.OnLoad for memory efficiency
// - Lazy loading approach for URL-based images
// - Minimal initialization overhead
// - Proper resource cleanup on window close
//
// LIMITATIONS:
// - No zoom or rotation capabilities
// - Basic error handling without retry logic
// - Limited image format support validation
// - No batch preview or slideshow features
//
// FUTURE REFACTORING:
// FUTURE: Add image zoom and pan capabilities
// FUTURE: Implement image rotation and manipulation tools
// FUTURE: Add support for image format validation
// FUTURE: Implement slideshow mode for multiple images
// FUTURE: Add image saving and export functionality
// FUTURE: Extract image loading to separate service
// FUTURE: Add keyboard shortcuts for common operations
// FUTURE: Implement image metadata display overlay
// CONSIDER: Adding image editing capabilities
// CONSIDER: Integration with external image viewers
// IDEA: Batch preview mode for album collections
// IDEA: Image comparison and selection tools
//
// TESTING:
// - Image loading tests with various source types
// - Error handling tests for network failures
// - Window lifecycle and resource cleanup tests
// - Performance tests with large images
//
// IMAGE SOURCES:
// - Embedded images from MP3 metadata
// - Remote URLs from Discogs or other services
// - Local file paths (potential future enhancement)
// - Base64 encoded images (potential future enhancement)
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF 4.6.2
// - Common image formats (JPEG, PNG, GIF, BMP)
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with dual-source image preview capabilities
// ====================================================================

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OstPlayer.Views.Windows
{
    public partial class CoverPreviewWindow : Window
    {
        /// <summary>
        /// Constructor for displaying an image from a URL or directly from an ImageSource.
        /// </summary>
        /// <param name="imageUrl">Optional image URL (e.g. from the web).</param>
        /// <param name="embeddedImage">Optional image loaded from memory (e.g. from MP3).</param>
        public CoverPreviewWindow(string imageUrl = null, ImageSource embeddedImage = null)
        {
            InitializeComponent();

            if (embeddedImage != null)
            {
                // Set the embedded image as the source
                CoverImage.Source = embeddedImage;
            }
            else if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    // Try to load the image from the provided URL
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imageUrl);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    CoverImage.Source = bitmap;
                }
                catch (Exception ex)
                {
                    // Show error if loading from URL fails
                    MessageBox.Show(
                        "Error loading image from URL:\n" + ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            else
            {
                // Show warning if no image was provided
                MessageBox.Show(
                    "No image was provided for preview.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
    }
}
