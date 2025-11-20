// ====================================================================
// FILE: Debouncer.cs
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
// Thread-safe debouncer for reducing frequent method calls and improving UI responsiveness.
// Delays execution until a quiet period without new requests, essential for search inputs,
// window resize events, validation, and other high-frequency user interactions.
//
// FEATURES:
// - Delays execution until quiet period without new requests
// - Thread-safe timer coordination with internal locking
// - Configurable delay period for different use cases
// - Automatic cancellation of previous requests
// - Proper disposal pattern for resource cleanup
//
// DEPENDENCIES:
// - System.Threading.Timer (delayed execution)
// - System (base functionality and delegates)
//
// DESIGN PATTERNS:
// - Debouncing Pattern for UI responsiveness
// - Timer-based delayed execution
// - Disposal Pattern for proper cleanup
//
// USE CASES:
// - Search input debouncing (300-500ms)
// - Window resize events (100-200ms)
// - Scroll event handling
// - Form validation debouncing
// - Auto-save functionality (1-2 seconds)
//
// THREAD SAFETY:
// - Uses internal locking for timer coordination
// - Safe to call from multiple threads
// - Timer callbacks execute on thread pool threads
//
// PERFORMANCE NOTES:
// - Timer creation/disposal has minimal overhead
// - Action executes on thread pool thread
// - Memory usage is minimal (one timer instance max)
// - Suitable for UI responsiveness scenarios
//
// LIMITATIONS:
// - Action executes on thread pool thread (not UI thread)
// - No cancellation token support for action execution
// - Single action support (no batching)
// - No progress reporting for long-running actions
//
// FUTURE REFACTORING:
// FUTURE: Add UI thread marshalling for action execution
// FUTURE: Implement cancellation token support
// FUTURE: Add batching support for multiple pending actions
// FUTURE: Extract interface for different debouncing strategies
// FUTURE: Add progress reporting for long-running actions
// FUTURE: Implement configurable retry mechanism
// CONSIDER: Adding different debouncing strategies (throttling, leading edge)
// CONSIDER: Integration with async/await patterns
//
// TESTING:
// - Unit tests for timing accuracy and cancellation behavior
// - Thread safety tests for concurrent access
// - Performance tests for high-frequency debouncing
// - Memory leak tests for long-running scenarios
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Thread-safe for concurrent access
// - Works with any Action delegate
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Extracted from PerformanceOptimizations.cs for better modularity
// ====================================================================

using System;
using System.Threading;

namespace OstPlayer.Utils.Performance
{
    /// <summary>
    /// Debouncer for reducing frequent method calls
    /// PATTERN: Delays execution until a quiet period without new requests
    /// USE CASES: Search input, window resize, scroll events, validation
    /// THREAD SAFETY: Uses internal locking for timer coordination
    /// </summary>
    public class Debouncer : IDisposable
    {
        #region Private Fields

        // Delay period in milliseconds before executing debounced action
        // TIMING: Countdown resets with each new debounce request
        // CONFIGURATION: Set once in constructor, immutable afterward
        private readonly int _delayMs;

        // Timer for delayed execution of debounced actions
        // LIFECYCLE: Created/disposed for each debounce request
        // THREAD SAFETY: Timer callbacks execute on thread pool threads
        private Timer _timer;

        // Synchronization object for timer management
        // PROTECTION: Prevents race conditions during timer creation/disposal
        private readonly object _lock = new object();

        // Disposal flag for proper cleanup
        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new debouncer with the specified delay
        /// DELAY: Time to wait for quiet period before execution
        /// USAGE: Typically 100-500ms for UI events, longer for expensive operations
        /// </summary>
        /// <param name="delayMs">Delay in milliseconds</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when delay is negative</exception>
        public Debouncer(int delayMs)
        {
            if (delayMs < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(delayMs),
                    "Delay must be non-negative"
                );

            _delayMs = delayMs;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Debounces the specified action
        /// BEHAVIOR: Cancels previous request and schedules new execution
        /// THREAD SAFETY: Safe to call from multiple threads
        /// EXECUTION CONTEXT: Action executes on thread pool thread
        /// </summary>
        /// <param name="action">Action to execute after delay</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when debouncer is disposed</exception>
        public void Debounce(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (_disposed)
                throw new ObjectDisposedException(nameof(Debouncer));

            lock (_lock)
            {
                // CANCELLATION: Dispose previous timer if it exists
                // EFFECT: Prevents previous action from executing
                _timer?.Dispose();

                // SCHEDULING: Create new timer for delayed execution
                // PARAMETERS: action (callback), null (no state), delay, no repeat
                _timer = new Timer(
                    _ =>
                    {
                        try
                        {
                            action();
                        }
                        catch
                        {
                            // SWALLOW EXCEPTIONS: Prevent timer thread crashes
                            // NOTE: Consider adding logging here in future versions
                        }
                    }, // Callback: execute action with exception handling
                    null, // State: not needed
                    _delayMs, // Due time: delay period
                    Timeout.Infinite // Period: no repeat (one-shot)
                );
            }
        }

        /// <summary>
        /// Cancels any pending debounced action without executing it
        /// USAGE: Cancel pending operations when component is being disposed
        /// THREAD SAFETY: Safe to call from multiple threads
        /// </summary>
        public void Cancel()
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Cancels any pending debounced action and releases resources
        /// CLEANUP: Implements proper disposal pattern
        /// TIMING: Call when debouncer is no longer needed
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method for proper disposal pattern
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                lock (_lock)
                {
                    _timer?.Dispose();
                    _timer = null;
                }
                _disposed = true;
            }
        }

        #endregion

        #region Usage Examples

        /*
        COMMON DEBOUNCING SCENARIOS:

        1. Search Input Debouncing:
           private readonly Debouncer searchDebouncer = new Debouncer(300);

           private void OnSearchTextChanged(string searchText)
           {
               searchDebouncer.Debounce(() => PerformSearch(searchText));
           }

        2. Window Resize Debouncing:
           private readonly Debouncer resizeDebouncer = new Debouncer(100);

           private void OnWindowResize()
           {
               resizeDebouncer.Debounce(() => RecalculateLayout());
           }

        3. Validation Debouncing:
           private readonly Debouncer validationDebouncer = new Debouncer(500);

           private void OnInputChanged(string input)
           {
               validationDebouncer.Debounce(() => ValidateInput(input));
           }

        4. Auto-Save Debouncing:
           private readonly Debouncer saveDebouncer = new Debouncer(2000);

           private void OnDocumentChanged()
           {
               saveDebouncer.Debounce(() => SaveDocument());
           }

        5. Using Statement Pattern:
           using (var debouncer = new Debouncer(300))
           {
               // Use debouncer...
           } // Automatic disposal

        PERFORMANCE CONSIDERATIONS:
        - Timer creation/disposal has overhead - don't debounce very frequently
        - Action executes on thread pool thread - consider UI thread marshalling
        - Memory usage is minimal (one timer instance max)
        - Suitable for UI responsiveness scenarios

        THREAD MARSHALLING EXAMPLE:
        debouncer.Debounce(() =>
        {
            Dispatcher.Invoke(() =>
            {
                // UI thread operations here
                UpdateUI();
            });
        });
        */

        #endregion
    }
}
