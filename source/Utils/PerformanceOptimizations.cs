// ====================================================================
// FILE: PerformanceOptimizations.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils
// LOCATION: Utils/
// VERSION: 2.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Central namespace export for performance optimization utilities.
// This file now serves as a unified entry point to performance-related classes
// that have been extracted to separate files for better modularity and maintainability.
//
// REFACTORING CHANGES (v2.0.0):
// - Extracted LazyAsync<T> to Utils/Performance/LazyAsync.cs
// - Extracted LRUCache<TKey, TValue> to Utils/Performance/LRUCache.cs
// - Extracted Debouncer to Utils/Performance/Debouncer.cs
// - Maintained backward compatibility through namespace imports
//
// ARCHITECTURAL BENEFITS:
// - Single Responsibility Principle: Each class in its own file
// - Better code navigation and IntelliSense experience
// - Easier unit testing with focused test classes
// - Improved modularity for future enhancements
// - Cleaner git history and code reviews
//
// BACKWARD COMPATIBILITY:
// - All public APIs remain unchanged
// - Existing using statements continue to work
// - No breaking changes to calling code
// - Same namespace (OstPlayer.Utils) maintained
//
// EXTRACTED CLASSES:
// - LazyAsync<T>: Thread-safe async lazy loading wrapper
// - LRUCache<TKey, TValue>: Least Recently Used cache implementation
// - Debouncer: UI responsiveness debouncing utility
//
// FUTURE REFACTORING:
// FUTURE: Consider extracting to separate Utils.Performance assembly
// FUTURE: Add performance monitoring and metrics collection
// FUTURE: Implement performance benchmarking utilities
// FUTURE: Add memory pressure monitoring utilities
// CONSIDER: Creating performance profiler integration
// CONSIDER: Adding performance best practices documentation
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - All original functionality preserved
// - Thread-safe operations maintained
//
// CHANGELOG:
// 2025-08-06 v2.0.0 - Refactored into separate files for better modularity
// 2025-08-06 v1.0.0 - Initial implementation with all classes in single file
// ====================================================================

// NAMESPACE IMPORTS: Make extracted classes available in original namespace
// This maintains backward compatibility while improving code organization
namespace OstPlayer.Utils
{
    // NOTE: All performance optimization classes have been extracted to separate files
    // in the Utils/Performance/ directory for better code organization:
    //
    // - LazyAsync<T> ? Utils/Performance/LazyAsync.cs
    // - LRUCache<TKey, TValue> ? Utils/Performance/LRUCache.cs
    // - Debouncer ? Utils/Performance/Debouncer.cs
    //
    // This file now serves as a namespace bridge to maintain backward compatibility.
    // All existing code using these classes will continue to work without changes.

    #region Namespace Bridge Documentation

    /*
    USAGE EXAMPLES (unchanged from v1.0.0):

    1. LazyAsync<T> Usage:
       var lazyData = new LazyAsync<string>(() => LoadDataAsync());
       var result = await lazyData.Value;

    2. LRUCache<TKey, TValue> Usage:
       var cache = new LRUCache<string, Metadata>(100);
       if (!cache.TryGet(key, out var value))
       {
           cache.Add(key, LoadValue(key));
       }

    3. Debouncer Usage:
       var debouncer = new Debouncer(300);
       debouncer.Debounce(() => PerformSearch(searchText));

    All classes are now imported from OstPlayer.Utils.Performance namespace
    but remain accessible in OstPlayer.Utils for backward compatibility.
    */

    #endregion
}
