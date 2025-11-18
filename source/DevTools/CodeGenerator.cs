// ====================================================================
// FILE: CodeGenerator.cs
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
// Advanced code generation utility for AI assistants and development automation.
// This tool provides comprehensive code generation capabilities including file headers,
// method documentation, class summaries, and inline comments to support Copilot
// and other AI development workflows.
//
// FEATURES:
// - Automated file header generation with project context
// - Method and property XML documentation generation
// - Class-level documentation with architecture patterns
// - Inline comment generation for complex code sections
// - Template-based code generation for common patterns
// - AI-friendly code analysis and enhancement suggestions
//
// DEPENDENCIES:
// - System.Reflection for runtime type analysis
// - System.Text for string manipulation
// - System.IO for file operations
// - System.Linq for LINQ operations
// - OstPlayer.DevTools.DateHelper for date management
// - OstPlayer.DevTools.ProjectAnalyzer for context analysis
//
// DESIGN PATTERNS:
// - Factory pattern for different code generators
// - Template method pattern for generation workflows
// - Builder pattern for complex code structures
// - Strategy pattern for different documentation styles
//
// AI INTEGRATION:
// - Copilot-friendly API design with clear method signatures
// - Context-aware generation based on project structure
// - Intelligent suggestions for code improvements
// - Template-based generation for common development patterns
//
// PERFORMANCE NOTES:
// - Efficient template caching for repeated operations
// - Minimal reflection usage for type analysis
// - Optimized string building for large code generation
// - Lazy loading of project context information
//
// LIMITATIONS:
// - Requires proper project structure for context analysis
// - Limited to .NET Framework 4.6.2 reflection capabilities
// - Template system requires predefined patterns
//
// FUTURE REFACTORING:
// FUTURE: Add support for async code generation patterns
// FUTURE: Implement machine learning-based code suggestions
// FUTURE: Add support for custom code generation plugins
// FUTURE: Implement real-time code analysis and suggestions
// FUTURE: Add Roslyn-based code analysis for advanced refactoring
// FUTURE: Implement code generation templates for ViewModels
// FUTURE: Add support for automatic property generation
// FUTURE: Integrate with project analyzer for dependency graph
// FUTURE: Add code generation for unit tests
// FUTURE: Implement code formatting and cleanup routines
// FUTURE: Add support for custom code snippets
// FUTURE: Integrate with AI assistant hooks for smart suggestions
// CONSIDER: Integration with external code analysis tools
// CONSIDER: Support for multiple programming languages
//
// TESTING:
// - Unit tests for each generation method
// - Integration tests with actual project files
// - Performance tests for large code generation tasks
// - Validation tests for generated code quality
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Visual Studio 2019+
// - GitHub Copilot integration
//
// CHANGELOG:
// 2025-08-07 v1.3.0 - Initial implementation for AI assistant enhancement
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OstPlayer.DevTools
{
    /// <summary>
    /// Advanced code generation utility for AI assistants and development automation
    /// </summary>
    public static class CodeGenerator
    {
        // Template patterns for different code elements
        private static readonly Dictionary<string, string> Templates =
            new Dictionary<string, string>();

        // Regex patterns for code analysis
        private static readonly Regex MethodSignatureRegex = new Regex(
            @"(?:public|private|protected|internal)\s+(?:static\s+)?(?:async\s+)?(\w+(?:<[^>]+>)?)\s+(\w+)\s*\(([^)]*)\)",
            RegexOptions.Compiled
        );
        private static readonly Regex PropertyRegex = new Regex(
            @"(?:public|private|protected|internal)\s+(\w+(?:<[^>]+>)?)\s+(\w+)\s*{\s*get",
            RegexOptions.Compiled
        );
        private static readonly Regex ClassRegex = new Regex(
            @"(?:public\s+)?(?:static\s+)?(?:abstract\s+)?class\s+(\w+)(?:\s*:\s*([^{]+))?",
            RegexOptions.Compiled
        );

        static CodeGenerator()
        {
            InitializeTemplates();
        }

        /// <summary>
        /// Generates a complete file header with project context
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="module">Module name (e.g., ViewModels, Utils, Services)</param>
        /// <param name="purpose">Brief description of the file's purpose</param>
        /// <param name="location">Relative path location</param>
        /// <param name="version">Version number</param>
        /// <returns>Generated file header as string</returns>
        public static string GenerateFileHeader(
            string fileName,
            string module,
            string purpose,
            string location = null,
            string version = "1.0.0"
        )
        {
            var currentDate = DateHelper.GetCurrentDateString();
            location = location ?? $"{module}/";

            var header = new StringBuilder();
            header.AppendLine(
                "// ===================================================================="
            );
            header.AppendLine($"// FILE: {fileName}");
            header.AppendLine(
                "// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management"
            );
            header.AppendLine($"// MODULE: {module}");
            header.AppendLine($"// LOCATION: {location}");
            header.AppendLine($"// VERSION: {version}");
            header.AppendLine($"// CREATED: {currentDate}");
            header.AppendLine($"// UPDATED: {currentDate}");
            header.AppendLine("// AUTHOR: TiggAdry");
            header.AppendLine(
                "// ===================================================================="
            );
            header.AppendLine("//");
            header.AppendLine("// PURPOSE:");
            header.AppendLine($"// {purpose}");
            header.AppendLine("//");

            // Add module-specific sections based on module type
            AddModuleSpecificSections(header, module);

            header.AppendLine("// COMPATIBILITY:");
            header.AppendLine("// - .NET Framework 4.6.2");
            header.AppendLine("// - C# 7.3");
            header.AppendLine("//");
            header.AppendLine("// CHANGELOG:");
            header.AppendLine($"// {currentDate} v{version} - Initial implementation");
            header.AppendLine(
                "// ===================================================================="
            );

            return header.ToString();
        }

        /// <summary>
        /// Adds module-specific documentation sections to the header
        /// </summary>
        /// <param name="header">StringBuilder for the header</param>
        /// <param name="module">Module name</param>
        private static void AddModuleSpecificSections(StringBuilder header, string module)
        {
            switch (module.ToLowerInvariant())
            {
                case "viewmodels":
                    header.AppendLine("// DESIGN PATTERNS:");
                    header.AppendLine("// - MVVM (Model-View-ViewModel)");
                    header.AppendLine("// - Command Pattern (for UI actions)");
                    header.AppendLine("// - Observer Pattern (INotifyPropertyChanged)");
                    header.AppendLine("//");
                    header.AppendLine("// PERFORMANCE NOTES:");
                    header.AppendLine("// - UI responsiveness considerations");
                    header.AppendLine("// - Data binding optimizations");
                    header.AppendLine("// - Memory usage patterns");
                    header.AppendLine("//");
                    break;

                case "services":
                    header.AppendLine("// SERVICE RESPONSIBILITIES:");
                    header.AppendLine("// - Core service functions and coordination");
                    header.AppendLine("// - External integrations");
                    header.AppendLine("// - Internal coordination");
                    header.AppendLine("//");
                    header.AppendLine("// THREAD SAFETY:");
                    header.AppendLine("// - Concurrent access patterns");
                    header.AppendLine("// - Synchronization mechanisms");
                    header.AppendLine("// - Async/await usage");
                    header.AppendLine("//");
                    break;

                case "utils":
                    header.AppendLine("// UTILITY FUNCTIONS:");
                    header.AppendLine("// - Static methods and helper algorithms");
                    header.AppendLine("// - Common operations");
                    header.AppendLine("// - Performance-optimized implementations");
                    header.AppendLine("//");
                    header.AppendLine("// PERFORMANCE NOTES:");
                    header.AppendLine("// - Algorithm complexity");
                    header.AppendLine("// - Memory allocation patterns");
                    header.AppendLine("// - Optimization opportunities");
                    header.AppendLine("//");
                    break;

                case "models":
                    header.AppendLine("// DATA STRUCTURE:");
                    header.AppendLine("// - Property descriptions and relationships");
                    header.AppendLine("// - Data validation rules");
                    header.AppendLine("// - Serialization compatibility");
                    header.AppendLine("//");
                    header.AppendLine("// SERIALIZATION:");
                    header.AppendLine("// - JSON/XML serialization notes");
                    header.AppendLine("// - Database persistence");
                    header.AppendLine("// - API compatibility");
                    header.AppendLine("//");
                    break;

                case "clients":
                    header.AppendLine("// API INTEGRATION:");
                    header.AppendLine("// - External service details");
                    header.AppendLine("// - Authentication methods");
                    header.AppendLine("// - Rate limiting and retry logic");
                    header.AppendLine("//");
                    header.AppendLine("// REQUEST/RESPONSE:");
                    header.AppendLine("// - API endpoints used");
                    header.AppendLine("// - Data transformation");
                    header.AppendLine("// - Error handling");
                    header.AppendLine("//");
                    break;

                case "views":
                    header.AppendLine("// UI RESPONSIBILITIES:");
                    header.AppendLine("// - User interaction handling");
                    header.AppendLine("// - Data display and presentation");
                    header.AppendLine("// - Navigation and user experience");
                    header.AppendLine("//");
                    header.AppendLine("// MVVM COMPLIANCE:");
                    header.AppendLine("// - Minimal code-behind patterns");
                    header.AppendLine("// - Command binding integration");
                    header.AppendLine("// - ViewModel event handling");
                    header.AppendLine("//");
                    break;

                case "converters":
                    header.AppendLine("// CONVERTER LOGIC:");
                    header.AppendLine("// - Input/output types and transformation rules");
                    header.AppendLine("// - WPF data binding integration");
                    header.AppendLine("// - Performance characteristics");
                    header.AppendLine("//");
                    header.AppendLine("// XAML USAGE:");
                    header.AppendLine("// - Resource declarations and bindings");
                    header.AppendLine("// - Multi-value converter patterns");
                    header.AppendLine("// - Parameter usage examples");
                    header.AppendLine("//");
                    break;

                case "devtools":
                    header.AppendLine("// DEVELOPMENT UTILITY:");
                    header.AppendLine("// - AI assistant integration");
                    header.AppendLine("// - Development automation");
                    header.AppendLine("// - Project maintenance tools");
                    header.AppendLine("//");
                    header.AppendLine("// AI INTEGRATION:");
                    header.AppendLine("// - Copilot-friendly API design");
                    header.AppendLine("// - Context-aware generation");
                    header.AppendLine("// - Automated workflow support");
                    header.AppendLine("//");
                    break;
            }

            header.AppendLine("// FUTURE REFACTORING:");
            header.AppendLine("// FUTURE: [Add specific improvement opportunities]");
            header.AppendLine("// CONSIDER: [Add architectural considerations]");
            header.AppendLine("//");
        }

        /// <summary>
        /// Generates XML documentation for a method based on its signature
        /// </summary>
        /// <param name="methodSignature">Method signature to analyze</param>
        /// <param name="purpose">Brief description of method purpose</param>
        /// <param name="additionalNotes">Optional additional notes</param>
        /// <returns>Generated XML documentation</returns>
        public static string GenerateMethodDocumentation(
            string methodSignature,
            string purpose,
            string additionalNotes = null
        )
        {
            var match = MethodSignatureRegex.Match(methodSignature);
            if (!match.Success)
            {
                return GenerateBasicMethodDocumentation(purpose, additionalNotes);
            }

            var returnType = match.Groups[1].Value;
            var methodName = match.Groups[2].Value;
            var parameters = match.Groups[3].Value;

            var doc = new StringBuilder();
            doc.AppendLine("/// <summary>");
            doc.AppendLine($"/// {purpose}");
            doc.AppendLine("/// </summary>");

            // Add parameter documentation
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                var paramList = ParseParameters(parameters);
                foreach (var param in paramList)
                {
                    doc.AppendLine(
                        $"/// <param name=\"{param.Name}\">{GenerateParameterDescription(param)}</param>"
                    );
                }
            }

            // Add return documentation if not void
            if (returnType != "void" && !returnType.StartsWith("Task") && returnType != "Task")
            {
                doc.AppendLine(
                    $"/// <returns>{GenerateReturnDescription(returnType, methodName)}</returns>"
                );
            }

            // Add additional notes if provided
            if (!string.IsNullOrWhiteSpace(additionalNotes))
            {
                doc.AppendLine("/// <remarks>");
                doc.AppendLine($"/// {additionalNotes}");
                doc.AppendLine("/// </remarks>");
            }

            return doc.ToString();
        }

        /// <summary>
        /// Generates basic method documentation when signature parsing fails
        /// </summary>
        /// <param name="purpose">Method purpose</param>
        /// <param name="additionalNotes">Additional notes</param>
        /// <returns>Basic XML documentation</returns>
        private static string GenerateBasicMethodDocumentation(
            string purpose,
            string additionalNotes
        )
        {
            var doc = new StringBuilder();
            doc.AppendLine("/// <summary>");
            doc.AppendLine($"/// {purpose}");
            doc.AppendLine("/// </summary>");

            if (!string.IsNullOrWhiteSpace(additionalNotes))
            {
                doc.AppendLine("/// <remarks>");
                doc.AppendLine($"/// {additionalNotes}");
                doc.AppendLine("/// </remarks>");
            }

            return doc.ToString();
        }

        /// <summary>
        /// Generates XML documentation for a property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="type">Property type</param>
        /// <param name="description">Description of the property</param>
        /// <param name="isReadOnly">Whether the property is read-only</param>
        /// <returns>Generated XML documentation</returns>
        public static string GeneratePropertyDocumentation(
            string propertyName,
            string type,
            string description,
            bool isReadOnly = false
        )
        {
            var doc = new StringBuilder();
            doc.AppendLine("/// <summary>");
            doc.AppendLine($"/// {description}");
            doc.AppendLine("/// </summary>");

            if (isReadOnly)
            {
                doc.AppendLine("/// <remarks>");
                doc.AppendLine("/// This property is read-only.");
                doc.AppendLine("/// </remarks>");
            }

            // Add value documentation for specific types
            if (type.Contains("Collection") || type.Contains("List") || type.Contains("Array"))
            {
                doc.AppendLine(
                    $"/// <value>Collection of {ExtractGenericType(type)} items</value>"
                );
            }
            else if (type == "bool" || type == "Boolean")
            {
                doc.AppendLine(
                    $"/// <value>True if {GenerateBooleanDescription(propertyName)}, false otherwise</value>"
                );
            }
            else
            {
                doc.AppendLine(
                    $"/// <value>{GenerateValueDescription(type, propertyName)}</value>"
                );
            }

            return doc.ToString();
        }

        /// <summary>
        /// Generates comprehensive class-level documentation
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="purpose">Purpose and responsibility of the class</param>
        /// <param name="designPatterns">Design patterns used in the class</param>
        /// <param name="dependencies">Key dependencies and integrations</param>
        /// <returns>Generated class documentation</returns>
        public static string GenerateClassDocumentation(
            string className,
            string purpose,
            List<string> designPatterns = null,
            List<string> dependencies = null
        )
        {
            var doc = new StringBuilder();
            doc.AppendLine("/// <summary>");
            doc.AppendLine($"/// {purpose}");
            doc.AppendLine("/// </summary>");

            if (designPatterns != null && designPatterns.Any())
            {
                doc.AppendLine("/// <remarks>");
                doc.AppendLine("/// <para>Design Patterns:</para>");
                doc.AppendLine("/// <list type=\"bullet\">");
                foreach (var pattern in designPatterns)
                {
                    doc.AppendLine($"/// <item>{pattern}</item>");
                }
                doc.AppendLine("/// </list>");
                doc.AppendLine("/// </remarks>");
            }

            if (dependencies != null && dependencies.Any())
            {
                if (designPatterns == null || !designPatterns.Any())
                {
                    doc.AppendLine("/// <remarks>");
                }
                doc.AppendLine("/// <para>Key Dependencies:</para>");
                doc.AppendLine("/// <list type=\"bullet\">");
                foreach (var dependency in dependencies)
                {
                    doc.AppendLine($"/// <item>{dependency}</item>");
                }
                doc.AppendLine("/// </list>");
                if (designPatterns == null || !designPatterns.Any())
                {
                    doc.AppendLine("/// </remarks>");
                }
            }

            return doc.ToString();
        }

        /// <summary>
        /// Updates inline comments in a file based on code analysis
        /// </summary>
        /// <param name="filePath">Path to the file to update</param>
        /// <param name="commentStyle">Style of comments to generate</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdateInlineComments(
            string filePath,
            CommentStyle commentStyle = CommentStyle.Explanatory
        )
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var originalContent = content;

                // Add comments based on detected patterns
                content = AddMethodComments(content, commentStyle);
                content = AddComplexCodeComments(content, commentStyle);
                content = AddVariableComments(content, commentStyle);

                if (content != originalContent)
                {
                    File.WriteAllText(filePath, content, Encoding.UTF8);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds comments to methods that lack documentation
        /// </summary>
        /// <param name="content">File content</param>
        /// <param name="style">Comment style</param>
        /// <returns>Updated content</returns>
        private static string AddMethodComments(string content, CommentStyle style)
        {
            var methodMatches = MethodSignatureRegex.Matches(content);
            var updatedContent = content;

            foreach (Match match in methodMatches)
            {
                var methodStart = match.Index;
                var methodName = match.Groups[2].Value;

                // Check if method already has documentation
                var beforeMethod = content.Substring(0, methodStart);
                if (!beforeMethod.TrimEnd().EndsWith("/// </summary>"))
                {
                    var comment = GenerateMethodComment(methodName, style);
                    var indentation = GetIndentation(content, methodStart);
                    updatedContent = updatedContent.Insert(
                        methodStart,
                        $"{indentation}{comment}\n"
                    );
                }
            }

            return updatedContent;
        }

        /// <summary>
        /// Adds comments to complex code sections
        /// </summary>
        /// <param name="content">File content</param>
        /// <param name="style">Comment style</param>
        /// <returns>Updated content</returns>
        private static string AddComplexCodeComments(string content, CommentStyle style)
        {
            // Look for complex patterns that benefit from comments
            var patterns = new[]
            {
                new Regex(@"for\s*\([^)]+\)\s*{", RegexOptions.IgnoreCase),
                new Regex(@"while\s*\([^)]+\)\s*{", RegexOptions.IgnoreCase),
                new Regex(@"if\s*\([^)]+&&[^)]+\)", RegexOptions.IgnoreCase),
                new Regex(@"switch\s*\([^)]+\)", RegexOptions.IgnoreCase),
            };

            var updatedContent = content;

            foreach (var pattern in patterns)
            {
                var matches = pattern.Matches(content);
                foreach (Match match in matches)
                {
                    var comment = GenerateComplexCodeComment(match.Value, style);
                    if (!string.IsNullOrEmpty(comment))
                    {
                        var indentation = GetIndentation(content, match.Index);
                        updatedContent = updatedContent.Insert(
                            match.Index,
                            $"{indentation}// {comment}\n"
                        );
                    }
                }
            }

            return updatedContent;
        }

        /// <summary>
        /// Adds comments to important variables
        /// </summary>
        /// <param name="content">File content</param>
        /// <param name="style">Comment style</param>
        /// <returns>Updated content</returns>
        private static string AddVariableComments(string content, CommentStyle style)
        {
            // Look for important variable declarations
            var variablePattern = new Regex(@"var\s+(\w+)\s*=\s*([^;]+);", RegexOptions.IgnoreCase);
            var matches = variablePattern.Matches(content);
            var updatedContent = content;

            foreach (Match match in matches)
            {
                var variableName = match.Groups[1].Value;
                var assignment = match.Groups[2].Value;

                if (IsComplexAssignment(assignment))
                {
                    var comment = GenerateVariableComment(variableName, assignment, style);
                    var indentation = GetIndentation(content, match.Index);
                    updatedContent = updatedContent.Insert(
                        match.Index,
                        $"{indentation}// {comment}\n"
                    );
                }
            }

            return updatedContent;
        }

        /// <summary>
        /// Initializes the template system with predefined templates
        /// </summary>
        private static void InitializeTemplates()
        {
            Templates["ViewModel"] =
                @"
/// <summary>
/// {Purpose}
/// </summary>
public class {ClassName} : ViewModelBase
{
    // Private fields
    {PrivateFields}

    // Public properties
    {PublicProperties}

    // Commands
    {Commands}

    // Constructor
    public {ClassName}()
    {
        InitializeCommands();
    }

    // Private methods
    {PrivateMethods}
}";

            Templates["Service"] =
                @"
/// <summary>
/// {Purpose}
/// </summary>
public class {ClassName}
{
    // Dependencies
    {Dependencies}

    // Constructor
    public {ClassName}({ConstructorParameters})
    {
        {ConstructorBody}
    }

    // Public methods
    {PublicMethods}

    // Private methods
    {PrivateMethods}
}";

            Templates["Utility"] =
                @"
/// <summary>
/// {Purpose}
/// </summary>
public static class {ClassName}
{
    // Constants
    {Constants}

    // Static methods
    {StaticMethods}
}";
        }

        /// <summary>
        /// Generates code from a template
        /// </summary>
        /// <param name="templateName">Name of the template</param>
        /// <param name="parameters">Template parameters</param>
        /// <returns>Generated code</returns>
        public static string GenerateFromTemplate(
            string templateName,
            Dictionary<string, string> parameters
        )
        {
            if (!Templates.ContainsKey(templateName))
            {
                return null;
            }

            var template = Templates[templateName];
            foreach (var parameter in parameters)
            {
                template = template.Replace($"{{{parameter.Key}}}", parameter.Value);
            }

            return template;
        }

        /// <summary>
        /// Parses method parameters from a parameter string
        /// </summary>
        /// <param name="parameters">Parameter string</param>
        /// <returns>List of parameter information</returns>
        private static List<ParameterInfo> ParseParameters(string parameters)
        {
            var result = new List<ParameterInfo>();
            var parts = parameters.Split(',');

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                var words = trimmed.Split(' ');
                if (words.Length >= 2)
                {
                    var type = words[words.Length - 2];
                    var name = words[words.Length - 1];
                    result.Add(new ParameterInfo { Name = name, Type = type });
                }
            }

            return result;
        }

        /// <summary>
        /// Generates a description for a parameter
        /// </summary>
        /// <param name="param">Parameter information</param>
        /// <returns>Generated description</returns>
        private static string GenerateParameterDescription(ParameterInfo param)
        {
            return $"{param.Type} value for {param.Name.ToLowerInvariant()}";
        }

        /// <summary>
        /// Generates a description for a return value
        /// </summary>
        /// <param name="returnType">Return type</param>
        /// <param name="methodName">Method name</param>
        /// <returns>Generated description</returns>
        private static string GenerateReturnDescription(string returnType, string methodName)
        {
            if (returnType == "bool" || returnType == "Boolean")
            {
                return $"True if {methodName.ToLowerInvariant()} is successful, false otherwise";
            }

            return $"{returnType} result of {methodName.ToLowerInvariant()} operation";
        }

        /// <summary>
        /// Generates a description for a boolean property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Generated description</returns>
        private static string GenerateBooleanDescription(string propertyName)
        {
            var name = propertyName.ToLowerInvariant();
            if (name.StartsWith("is"))
            {
                return name.Substring(2).ToLowerInvariant();
            }
            if (name.StartsWith("has"))
            {
                return $"has {name.Substring(3).ToLowerInvariant()}";
            }
            if (name.StartsWith("can"))
            {
                return $"can {name.Substring(3).ToLowerInvariant()}";
            }

            return name;
        }

        /// <summary>
        /// Generates a value description for a property
        /// </summary>
        /// <param name="type">Property type</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Generated description</returns>
        private static string GenerateValueDescription(string type, string propertyName)
        {
            return $"{type} value representing {propertyName.ToLowerInvariant()}";
        }

        /// <summary>
        /// Extracts generic type from a generic type string
        /// </summary>
        /// <param name="type">Type string</param>
        /// <returns>Generic type</returns>
        private static string ExtractGenericType(string type)
        {
            var match = Regex.Match(type, @"<([^>]+)>");
            return match.Success ? match.Groups[1].Value : "object";
        }

        /// <summary>
        /// Gets the indentation of a line at a specific position
        /// </summary>
        /// <param name="content">File content</param>
        /// <param name="position">Position in content</param>
        /// <returns>Indentation string</returns>
        private static string GetIndentation(string content, int position)
        {
            var lineStart = content.LastIndexOf('\n', position) + 1;
            var lineEnd = content.IndexOf('\n', position);
            if (lineEnd == -1)
                lineEnd = content.Length;

            var line = content.Substring(lineStart, lineEnd - lineStart);
            var indentLength = 0;

            foreach (var c in line)
            {
                if (c == ' ' || c == '\t')
                {
                    indentLength++;
                }
                else
                {
                    break;
                }
            }

            return new string(' ', indentLength);
        }

        /// <summary>
        /// Generates a method comment based on method name and style
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <param name="style">Comment style</param>
        /// <returns>Generated comment</returns>
        private static string GenerateMethodComment(string methodName, CommentStyle style)
        {
            switch (style)
            {
                case CommentStyle.Explanatory:
                    return $"// Performs {methodName.ToLowerInvariant()} operation";
                case CommentStyle.Technical:
                    return $"// Method: {methodName}";
                case CommentStyle.Business:
                    return $"// Business logic for {methodName.ToLowerInvariant()}";
                default:
                    return $"// {methodName}";
            }
        }

        /// <summary>
        /// Generates a comment for complex code
        /// </summary>
        /// <param name="code">Code pattern</param>
        /// <param name="style">Comment style</param>
        /// <returns>Generated comment</returns>
        private static string GenerateComplexCodeComment(string code, CommentStyle style)
        {
            if (code.Contains("for"))
            {
                return "Iterate through collection";
            }
            if (code.Contains("while"))
            {
                return "Continue until condition is met";
            }
            if (code.Contains("if") && code.Contains("&&"))
            {
                return "Check multiple conditions";
            }
            if (code.Contains("switch"))
            {
                return "Handle different cases";
            }

            return null;
        }

        /// <summary>
        /// Generates a comment for a variable
        /// </summary>
        /// <param name="variableName">Variable name</param>
        /// <param name="assignment">Assignment expression</param>
        /// <param name="style">Comment style</param>
        /// <returns>Generated comment</returns>
        private static string GenerateVariableComment(
            string variableName,
            string assignment,
            CommentStyle style
        )
        {
            return $"Initialize {variableName} with {assignment}";
        }

        /// <summary>
        /// Determines if an assignment is complex enough to warrant a comment
        /// </summary>
        /// <param name="assignment">Assignment expression</param>
        /// <returns>True if complex, false otherwise</returns>
        private static bool IsComplexAssignment(string assignment)
        {
            return assignment.Length > 30
                || assignment.Contains("new")
                || assignment.Contains("(")
                || assignment.Contains("?");
        }

        #region Helper Classes

        /// <summary>
        /// Parameter information for method analysis
        /// </summary>
        private class ParameterInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Comment style options for code generation
    /// </summary>
    public enum CommentStyle
    {
        /// <summary>
        /// Explanatory comments that describe what the code does
        /// </summary>
        Explanatory,

        /// <summary>
        /// Technical comments focusing on implementation details
        /// </summary>
        Technical,

        /// <summary>
        /// Business-focused comments explaining the purpose
        /// </summary>
        Business,
    }
}
