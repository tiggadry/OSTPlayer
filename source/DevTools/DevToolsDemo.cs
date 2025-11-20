// ====================================================================
// FILE: DevToolsDemo.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: DevTools
// LOCATION: DevTools/
// VERSION: 1.3.0
// CREATED: 2025-08-07
// UPDATED: 2025-08-07
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Demonstration and testing utility for DevTools AI automation capabilities.
// This file showcases how to use the enhanced DevTools for AI-assisted
// development workflows and serves as an integration test for all components.
//
// FEATURES:
// - Comprehensive DevTools integration examples
// - AI assistant workflow demonstrations
// - Template engine usage patterns
// - Configuration management examples
// - Performance benchmarking utilities
//
// DEPENDENCIES:
// - All DevTools components (CodeGenerator, TemplateEngine, AIAssistantHooks, etc.)
// - System.Diagnostics for performance measurement
//
// DEVELOPMENT UTILITY:
// This file is for development and testing purposes only.
// It demonstrates the capabilities of DevTools v1.3.0 enhancements.
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
//
// CHANGELOG:
// 2025-08-07 v1.3.0 - Initial demonstration implementation
// ====================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// Demonstration and testing utility for DevTools AI automation capabilities
    /// </summary>
    public static class DevToolsDemo
    {
        /// <summary>
        /// Demonstrates all DevTools capabilities
        /// </summary>
        public static async Task RunComprehensiveDemo()
        {
            Console.WriteLine("?? DevTools v1.3.0 - AI Enhancement Demonstration");
            Console.WriteLine("================================================");

            await DemoCodeGeneration();
            await DemoTemplateEngine();
            await DemoAIAssistantHooks();
            DemoConfiguration();
            DemoPerformanceBenchmarks();

            Console.WriteLine("\n? DevTools demonstration completed successfully!");
        }

        /// <summary>
        /// Demonstrates CodeGenerator capabilities
        /// </summary>
        private static Task DemoCodeGeneration()
        {
            Console.WriteLine("\n?? CodeGenerator Demonstration");
            Console.WriteLine("------------------------------");

            // Generate file header
            var header = CodeGenerator.GenerateFileHeader(
                "DemoService.cs",
                "Services",
                "Demonstration service for showcasing DevTools capabilities"
            );

            Console.WriteLine("Generated File Header:");
            Console.WriteLine(header.Substring(0, Math.Min(300, header.Length)) + "...");

            // Generate method documentation
            var methodDoc = CodeGenerator.GenerateMethodDocumentation(
                "public async Task<bool> ProcessDataAsync(string input, int timeout)",
                "Processes input data asynchronously with specified timeout"
            );

            Console.WriteLine("\nGenerated Method Documentation:");
            Console.WriteLine(methodDoc);

            // Generate class documentation
            var classDoc = CodeGenerator.GenerateClassDocumentation(
                "DemoService",
                "Service for demonstrating AI-assisted development workflows",
                new List<string> { "Repository Pattern", "Async/Await", "Dependency Injection" },
                new List<string> { "System.Threading.Tasks", "OstPlayer.Core" }
            );

            Console.WriteLine("Generated Class Documentation:");
            Console.WriteLine(classDoc);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Demonstrates TemplateEngine capabilities
        /// </summary>
        private static Task DemoTemplateEngine()
        {
            Console.WriteLine("\n?? TemplateEngine Demonstration");
            Console.WriteLine("-------------------------------");

            // Get available templates
            var templates = TemplateEngine.GetAvailableTemplates();
            Console.WriteLine($"Available Templates: {templates.Count}");

            foreach (var template in templates)
            {
                Console.WriteLine(
                    $"  - {template.Name} ({template.Category}): {template.Description}"
                );
            }

            // Generate from template
            var parameters = new Dictionary<string, object>
            {
                ["ClassName"] = "DemoViewModel",
                ["Namespace"] = "OstPlayer.ViewModels.Demo",
                ["ViewModelDescription"] = "Demo ViewModel for showcasing DevTools",
                ["HasFields"] = true,
                ["HasProperties"] = true,
                ["HasCommands"] = true,
                ["Fields"] = new[]
                {
                    new { Type = "string", Name = "title" },
                    new { Type = "bool", Name = "isLoading" },
                },
                ["Properties"] = new[]
                {
                    new
                    {
                        Type = "string",
                        Name = "Title",
                        Description = "Gets or sets the demo title",
                    },
                    new
                    {
                        Type = "bool",
                        Name = "IsLoading",
                        Description = "Gets or sets loading state",
                    },
                },
                ["Commands"] = new[]
                {
                    new
                    {
                        Name = "Load",
                        Description = "Loads demo data",
                        ExecuteMethod = "LoadData",
                    },
                },
            };

            var generatedCode = TemplateEngine.GenerateFromTemplate("ViewModel", parameters);

            if (!string.IsNullOrEmpty(generatedCode))
            {
                Console.WriteLine("\nGenerated ViewModel Code:");
                Console.WriteLine(
                    generatedCode.Substring(0, Math.Min(500, generatedCode.Length)) + "..."
                );
            }

            // Validate template
            var sampleTemplate = "{{#if HasTitle}}Title: {{Title}}{{/if}}";
            var isValid = TemplateEngine.ValidateTemplate(sampleTemplate);
            Console.WriteLine($"\nTemplate Validation Test: {(isValid ? "? Valid" : "? Invalid")}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Demonstrates AIAssistantHooks capabilities
        /// </summary>
        private static async Task DemoAIAssistantHooks()
        {
            Console.WriteLine("\n?? AIAssistantHooks Demonstration");
            Console.WriteLine("--------------------------------");

            // Register demo hooks
            AIAssistantHooks.RegisterFileChangeHook(
                "*.demo",
                async (filePath) =>
                {
                    Console.WriteLine($"Demo file change detected: {filePath}");
                    await Task.Delay(10); // Simulate processing
                }
            );

            AIAssistantHooks.RegisterContextHook(
                "Demo",
                async (aiContext) =>
                {
                    Console.WriteLine($"Demo context hook triggered: {aiContext.CurrentOperation}");
                    await Task.Delay(10); // Simulate processing
                }
            );

            // Trigger workflows
            await AIAssistantHooks.TriggerDocumentationUpdate("Demo update", new[] { "demo.cs" });

            await AIAssistantHooks.TriggerCodeGeneration(
                "Demo",
                new Dictionary<string, object> { ["Type"] = "Class", ["Name"] = "DemoClass" }
            );

            // Get context
            var context = AIAssistantHooks.GetCurrentContext();
            Console.WriteLine($"Current AI Context: {context.CurrentOperation}");
            Console.WriteLine($"Last Updated: {context.LastUpdated}");
            Console.WriteLine($"Operation History: {context.OperationHistory.Count} entries");

            // Get action statistics
            var stats = AIAssistantHooks.GetActionStatistics();
            Console.WriteLine("\nAI Action Statistics:");
            foreach (var stat in stats)
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value}");
            }
        }

        /// <summary>
        /// Demonstrates DevToolsConfig capabilities
        /// </summary>
        private static void DemoConfiguration()
        {
            Console.WriteLine("\n?? DevToolsConfig Demonstration");
            Console.WriteLine("-------------------------------");

            // Get current configuration
            var config = DevToolsConfig.GetConfiguration();
            Console.WriteLine($"Auto Update Documentation: {config.AutoUpdateDocumentation}");
            Console.WriteLine($"Enable AI Hooks: {config.EnableAIHooks}");
            Console.WriteLine($"Date Format: {config.DateFormat}");
            Console.WriteLine($"Default Author: {config.DefaultAuthor}");

            // Get specific values
            var enableSuggestions = DevToolsConfig.GetValue("EnableAutomaticSuggestions", false);
            Console.WriteLine($"AI Suggestions: {enableSuggestions}");

            // Set demo value
            DevToolsConfig.SetValue("DemoSetting", "Demo Value");
            var demoValue = DevToolsConfig.GetValue<string>("DemoSetting");
            Console.WriteLine($"Demo Setting: {demoValue}");

            // Show available profiles
            var profiles = DevToolsConfig.GetAvailableProfiles();
            Console.WriteLine($"Available Profiles: {profiles.Count}");
        }

        /// <summary>
        /// Demonstrates performance benchmarks
        /// </summary>
        private static void DemoPerformanceBenchmarks()
        {
            Console.WriteLine("\n?? Performance Benchmarks");
            Console.WriteLine("-------------------------");

            // Benchmark file header generation
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                CodeGenerator.GenerateFileHeader($"TestFile{i}.cs", "Services", "Test service");
            }
            sw.Stop();
            Console.WriteLine($"File Header Generation (100x): {sw.ElapsedMilliseconds}ms");

            // Benchmark template processing
            sw.Restart();
            var templateParams = new Dictionary<string, object>
            {
                ["ClassName"] = "TestClass",
                ["Namespace"] = "Test.Namespace",
            };

            for (int i = 0; i < 50; i++)
            {
                TemplateEngine.GenerateFromTemplate("CSharpClass", templateParams);
            }
            sw.Stop();
            Console.WriteLine($"Template Processing (50x): {sw.ElapsedMilliseconds}ms");

            // Benchmark AI context updates
            sw.Restart();
            for (int i = 0; i < 200; i++)
            {
                AIAssistantHooks.UpdateContext(
                    $"Test{i}",
                    new Dictionary<string, object> { ["Test"] = i }
                );
            }
            sw.Stop();
            Console.WriteLine($"AI Context Updates (200x): {sw.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Demonstrates AI-assisted workflow scenario
        /// </summary>
        public static async Task DemoAIWorkflowScenario()
        {
            Console.WriteLine("\n?? AI-Assisted Workflow Scenario");
            Console.WriteLine("================================");

            // Scenario: AI helps create a new service class
            Console.WriteLine("Scenario: Creating a new service class with AI assistance");

            // Step 1: AI detects intent to create service
            Console.WriteLine("\n1. AI detects service creation intent...");
            await AIAssistantHooks.TriggerCodeGeneration(
                "Service",
                new Dictionary<string, object>
                {
                    ["ClassName"] = "AudioProcessingService",
                    ["Module"] = "Services",
                    ["Purpose"] = "Audio file processing and metadata extraction",
                }
            );

            // Step 2: Generate service template
            Console.WriteLine("2. Generating service template...");
            var serviceParams = new Dictionary<string, object>
            {
                ["ClassName"] = "AudioProcessingService",
                ["Namespace"] = "OstPlayer.Services",
                ["ServiceDescription"] =
                    "Service for processing audio files and extracting metadata",
                ["HasDependencies"] = true,
                ["HasAsync"] = true,
                ["Dependencies"] = new[]
                {
                    new { Type = "IAudioReader", Name = "audioReader" },
                    new { Type = "IMetadataExtractor", Name = "metadataExtractor" },
                },
                ["Methods"] = new[]
                {
                    new
                    {
                        Name = "ProcessAudioFileAsync",
                        ReturnType = "Task<AudioMetadata>",
                        Description = "Processes audio file and extracts metadata",
                        IsAsync = true,
                        ParameterList = "string filePath",
                        Body = "// FUTURE: Implement audio processing logic",
                    },
                },
            };

            var serviceCode = TemplateEngine.GenerateFromTemplate("Service", serviceParams);
            if (!string.IsNullOrEmpty(serviceCode))
            {
                Console.WriteLine("   ? Service template generated successfully");
            }

            // Step 3: Generate comprehensive documentation
            Console.WriteLine("3. Generating documentation...");
            var header = CodeGenerator.GenerateFileHeader(
                "AudioProcessingService.cs",
                "Services",
                "Service for processing audio files and extracting metadata for the OstPlayer plugin",
                "Services/",
                "1.0.0"
            );

            var classDoc = CodeGenerator.GenerateClassDocumentation(
                "AudioProcessingService",
                "Service responsible for processing audio files and extracting metadata",
                new List<string> { "Service Pattern", "Async/Await", "Dependency Injection" },
                new List<string> { "System.Threading.Tasks", "OstPlayer.Models", "TagLibSharp" }
            );

            Console.WriteLine("   ? Documentation generated successfully");

            // Step 4: Update project documentation
            Console.WriteLine("4. Updating project documentation...");
            await AIAssistantHooks.TriggerDocumentationUpdate(
                "Added AudioProcessingService",
                new[] { "Services/AudioProcessingService.cs" }
            );

            Console.WriteLine("   ? Project documentation updated");

            // Step 5: Log completion
            AIAssistantHooks.LogAIAction(
                "DemoWorkflowScenario",
                true,
                "Successfully demonstrated AI-assisted service creation workflow"
            );

            Console.WriteLine("\n?? AI-assisted workflow completed successfully!");
            Console.WriteLine("The AI helped with:");
            Console.WriteLine("  • Template-based code generation");
            Console.WriteLine("  • Comprehensive documentation");
            Console.WriteLine("  • Project structure updates");
            Console.WriteLine("  • Workflow automation");
        }
    }
}
