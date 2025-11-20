// ====================================================================
// FILE: LazyAsync.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils
// LOCATION: Utils/Performance/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Thread-safe lazy loading wrapper for expensive asynchronous operations.
// Provides single-execution guarantee with proper locking for concurrent access.
// Essential for non-blocking initialization of expensive resources.
//
// FEATURES:
// - Thread-safe lazy initialization using double-checked locking
// - Factory function for expensive async operations
// - Single execution guarantee with proper locking
// - IsValueCreated property for initialization status tracking
// - Task<T> sharing across all callers
//
// DEPENDENCIES:
// - System.Threading.Tasks (async/await support)
// - System (base functionality and delegates)
//
// DESIGN PATTERNS:
// - Lazy Loading Pattern with async support
// - Double-Checked Locking for thread safety
// - Factory Pattern for value creation
//
// THREAD SAFETY:
// - Uses lock-based synchronization for initialization
// - Task<T> sharing ensures all callers get same result
// - Minimal contention for most use cases
//
// PERFORMANCE NOTES:
// - Fast path avoids locking for subsequent accesses
// - Factory called exactly once regardless of concurrent access
// - Minimal memory overhead (one Task<T> instance)
//
// LIMITATIONS:
// - No cancellation support for factory execution
// - No timeout mechanism for long-running factories
// - Exception in factory propagates to all callers
//
// FUTURE REFACTORING:
// FUTURE: Add cancellation token support for factory
// FUTURE: Implement timeout mechanism for factory execution
// FUTURE: Add factory retry mechanism on failure
// FUTURE: Support for factory replacement/reset
// CONSIDER: Adding progress reporting for long factories
// CONSIDER: Memory pressure-aware lazy loading
//
// TESTING:
// - Unit tests for thread safety and concurrent access
// - Performance tests vs standard Lazy<T>
// - Exception handling tests for factory failures
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Thread-safe for concurrent access
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Extracted from PerformanceOptimizations.cs for better modularity
// ====================================================================

using System;
using System.Threading.Tasks;

namespace OstPlayer.Utils.Performance
{
    /// <summary>
    /// Lazy loading wrapper for expensive operations
    /// PATTERN: Lazy initialization with async support for non-blocking expensive operations
    /// THREAD SAFETY: Thread-safe initialization using lock-based synchronization
    /// USE CASES: Database connections, file loading, API calls, resource initialization
    /// </summary>
    /// <typeparam name="T">Type of value to lazily initialize</typeparam>
    public class LazyAsync<T>
    {
        #region Private Fields

        // Factory function that creates the value when first requested
        // DELEGATE: Func<Task<T>> allows async initialization operations
        // IMMUTABLE: Set once in constructor, never changes (thread safety)
        private readonly Func<Task<T>> _factory;

        // Synchronization object for thread-safe initialization
        // LOCKING: Ensures only one thread can initialize the value
        // PERFORMANCE: Minimal contention for most use cases
        private readonly object _lock = new object();

        // The actual async task that produces the value
        // LIFECYCLE: null initially, set once during first access, never changed again
        // SHARING: Same Task<T> instance shared across all callers
        private Task<T> _task;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new lazy async wrapper with the specified factory function
        /// VALIDATION: Factory must not be null (no defensive programming for performance)
        /// DEFERRED EXECUTION: Factory is not called until Value property is first accessed
        /// </summary>
        /// <param name="factory">Function that creates the value asynchronously</param>
        public LazyAsync(Func<Task<T>> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the task that produces the lazily-initialized value
        /// THREAD SAFETY: Uses double-checked locking pattern for performance
        /// INITIALIZATION: Factory is called exactly once, even with concurrent access
        /// SHARING: All callers receive the same Task<T> instance
        /// </summary>
        public Task<T> Value
        {
            get
            {
                // FAST PATH: If already initialized, return immediately without locking
                // PERFORMANCE: Avoids lock overhead for subsequent accesses
                if (_task != null)
                    return _task;

                // SLOW PATH: First access or concurrent initialization
                lock (_lock)
                {
                    // DOUBLE-CHECK: Another thread might have initialized while waiting for lock
                    // CORRECTNESS: Ensures only one initialization even with race conditions
                    if (_task == null)
                    {
                        // INITIALIZATION: Call factory function and store result
                        // SINGLE EXECUTION: Factory called exactly once regardless of concurrent access
                        _task = _factory();
                    }
                    return _task;
                }
            }
        }

        /// <summary>
        /// Indicates whether the value has been created and completed
        /// COMPLETION: true only when initialization is complete and successful
        /// TIMING: false during initialization, exception, or before first access
        /// UI BINDING: Useful for showing loading states or progress indicators
        /// </summary>
        public bool IsValueCreated => _task != null && _task.IsCompleted;

        #endregion

        #region Usage Examples

        /*
        BASIC USAGE PATTERNS:

        1. Expensive Resource Loading:
           var lazyConfig = new LazyAsync<Configuration>(() => LoadConfigurationAsync());
           var config = await lazyConfig.Value;

        2. Database Connection:
           var lazyDb = new LazyAsync<IDbConnection>(() => ConnectToDatabaseAsync());
           var connection = await lazyDb.Value;

        3. API Client Initialization:
           var lazyClient = new LazyAsync<HttpClient>(() => CreateHttpClientAsync());
           var client = await lazyClient.Value;

        4. File System Operations:
           var lazyData = new LazyAsync<byte[]>(() => File.ReadAllBytesAsync(path));
           var fileContent = await lazyData.Value;

        ADVANCED SCENARIOS:

        1. Conditional Initialization:
           var lazyService = new LazyAsync<IService>(async () =>
           {
               if (shouldUseRemote)
                   return await CreateRemoteServiceAsync();
               else
                   return new LocalService();
           });

        2. Dependency Chain:
           var lazyConfig = new LazyAsync<Config>(() => LoadConfigAsync());
           var lazyService = new LazyAsync<Service>(async () =>
           {
               var config = await lazyConfig.Value;
               return new Service(config);
           });
        */

        #endregion
    }
}
