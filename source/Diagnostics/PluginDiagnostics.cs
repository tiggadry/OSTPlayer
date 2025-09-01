// OstPlayer Plugin Diagnostic Test
// This file can help verify that the plugin loads correctly
// Add this code temporarily to help debug loading issues

using System;
using System.Diagnostics;
using OstPlayer.Services;
using Playnite.SDK;

namespace OstPlayer.Diagnostics
{
    /// <summary>
    /// Simple diagnostic helper for troubleshooting plugin loading issues.
    /// </summary>
    public static class PluginDiagnostics
    {
        /// <summary>
        /// Tests basic plugin functionality and reports status.
        /// Call this from the plugin constructor to verify initialization.
        /// </summary>
        public static void RunBasicDiagnostics(IPlayniteAPI api)
        {
            var logger = LogManager.GetLogger();
            var report = new System.Text.StringBuilder();

            report.AppendLine("=== OstPlayer Plugin Diagnostics ===");

            try
            {
                // Test 1: Basic API access
                report.AppendLine($"? Playnite API available: {api != null}");
                if (api != null)
                {
                    report.AppendLine($"? Games count: {api.Database.Games.Count}");
                    report.AppendLine($"? Config path: {api.Paths.ConfigurationPath}");
                }

                // Test 2: Service container
                try
                {
                    var container = ServiceContainer.Instance;
                    report.AppendLine($"? Service container initialized: {container != null}");

                    // Test basic service registration
                    if (TestServiceRegistration())
                    {
                        report.AppendLine("? Service registration test passed");
                    }
                    else
                    {
                        report.AppendLine("? Service registration test failed");
                    }
                }
                catch (Exception ex)
                {
                    report.AppendLine($"? Service container error: {ex.Message}");
                }

                // Test 3: Settings loading
                try
                {
                    var settings = new OstPlayerSettings();
                    report.AppendLine($"? Settings class instantiable: {settings != null}");

                    // Test settings validation
                    var errors = settings.Validate();
                    if (errors.Count == 0)
                    {
                        report.AppendLine("? Settings validation passed");
                    }
                    else
                    {
                        report.AppendLine($"? Settings validation warnings: {errors.Count}");
                    }
                }
                catch (Exception ex)
                {
                    report.AppendLine($"? Settings error: {ex.Message}");
                }

                // Test 4: NAudio availability
                try
                {
                    var playbackService = new Utils.MusicPlaybackService();
                    playbackService.Dispose();
                    report.AppendLine("? NAudio services available");
                }
                catch (Exception ex)
                {
                    report.AppendLine($"? NAudio error: {ex.Message}");
                }

                // Test 5: Interface types accessibility
                try
                {
                    var interfaceTypes = new Type[]
                    {
                        typeof(Services.Interfaces.IAudioService),
                        typeof(Services.Interfaces.IGameService),
                        typeof(Services.Interfaces.IMetadataService),
                    };

                    report.AppendLine($"? Interface types accessible: {interfaceTypes.Length}");
                }
                catch (Exception ex)
                {
                    report.AppendLine($"? Interface types error: {ex.Message}");
                }

                report.AppendLine("=== Diagnostics Complete ===");

                // Log results
                logger.Info(report.ToString());
                Debug.WriteLine(report.ToString());
            }
            catch (Exception ex)
            {
                var errorMsg = $"Diagnostics failed: {ex.Message}";
                logger.Error(ex, errorMsg);
                Debug.WriteLine(errorMsg);
            }
        }

        /// <summary>
        /// Quick service registration test.
        /// </summary>
        public static bool TestServiceRegistration()
        {
            try
            {
                var container = ServiceContainer.Instance;
                container.Clear(); // Start fresh

                // Register basic service
                container.RegisterSingleton<ILogger>(LogManager.GetLogger());

                // Try to resolve it
                var logger = container.GetService<ILogger>();

                return logger != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Service registration test failed: {ex.Message}");
                return false;
            }
        }
    }
}
