// ====================================================================
// FILE: LRUCache.cs
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
// Thread-safe LRU (Least Recently Used) cache implementation for performance optimization.
// Provides O(1) access time with automatic eviction of least recently used items.
// Essential for caching expensive computations and frequently accessed data.
//
// FEATURES:
// - LRU (Least Recently Used) eviction policy
// - Configurable maximum cache size
// - Thread-safe operations with concurrent dictionary
// - O(1) access time for cache operations
// - Automatic cleanup of old cache entries
//
// DEPENDENCIES:
// - System.Collections.Concurrent (thread-safe collections)
// - System.Collections.Generic (LinkedList for LRU order)
// - System (base functionality)
//
// ALGORITHM:
// - Maintains access order using linked list (most recent first, least recent last)
// - Uses ConcurrentDictionary for O(1) key lookup
// - Move-to-front strategy on access for LRU ordering
//
// DESIGN PATTERNS:
// - Cache Pattern with LRU eviction
// - Thread-Safe Collections usage
// - Hybrid data structure (Dictionary + LinkedList)
//
// THREAD SAFETY:
// - Uses locking for coordinating linked list operations
// - ConcurrentDictionary provides thread-safe dictionary operations
// - Suitable for moderate concurrent access scenarios
//
// PERFORMANCE NOTES:
// - O(1) average case for TryGet and Add operations
// - Bounded memory usage through automatic eviction
// - Lock contention increases with concurrent access
//
// LIMITATIONS:
// - LRU cache eviction is synchronous (blocking)
// - No cache persistence between application sessions
// - Limited cache statistics and monitoring
// - Basic eviction policy (no TTL or advanced algorithms)
//
// FUTURE REFACTORING:
// FUTURE: Add cache statistics and performance monitoring
// FUTURE: Implement multiple eviction policies (LFU, FIFO, TTL)
// FUTURE: Add async cache eviction for better performance
// FUTURE: Extract cache interface for pluggable implementations
// FUTURE: Add cache persistence and restore capabilities
// FUTURE: Implement cache warming strategies
// FUTURE: Add memory pressure-aware cache sizing
// CONSIDER: Adding cache events for monitoring and debugging
// CONSIDER: Implementing distributed cache support
//
// TESTING:
// - Unit tests for LRU behavior with various access patterns
// - Performance benchmarks against standard collections
// - Memory usage tests for large cache scenarios
// - Concurrent access thread safety tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Thread-safe for concurrent access
// - Memory-efficient for large datasets
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Extracted from PerformanceOptimizations.cs for better modularity
// ====================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OstPlayer.Utils.Performance
{
    /// <summary>
    /// LRU (Least Recently Used) cache implementation
    /// ALGORITHM: Maintains access order using linked list, provides O(1) access via dictionary
    /// EVICTION: Automatically removes least recently used items when capacity is exceeded
    /// THREAD SAFETY: Uses locking for thread-safe operations (suitable for moderate concurrency)
    /// </summary>
    /// <typeparam name="TKey">Type of cache keys (must be hashable)</typeparam>
    /// <typeparam name="TValue">Type of cached values</typeparam>
    public class LRUCache<TKey, TValue>
    {
        #region Private Fields - Cache Data Structures

        // Maximum number of items the cache can hold
        // CAPACITY: Fixed at construction time, determines eviction behavior
        // PERFORMANCE: Larger capacity = less eviction overhead, more memory usage
        private readonly int _maxSize;

        // Fast lookup table mapping keys to linked list nodes
        // PERFORMANCE: O(1) average case lookup, uses TKey's GetHashCode() and Equals()
        // THREAD SAFETY: ConcurrentDictionary provides thread-safe operations
        private readonly ConcurrentDictionary<TKey, LinkedListNode<CacheItem>> _cache;

        // Linked list maintaining access order (most recent first, least recent last)
        // ORDER: Head = most recently used, Tail = least recently used (eviction candidate)
        // OPERATIONS: Add to front, remove from back, move to front on access
        private readonly LinkedList<CacheItem> _lruList;

        // Synchronization object for coordinating linked list operations
        // NECESSITY: LinkedList<T> is not thread-safe, requires external synchronization
        // SCOPE: Protects both linked list and eviction logic
        private readonly object _lock = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new LRU cache with the specified maximum capacity
        /// CAPACITY: Must be positive (no validation for performance - caller responsibility)
        /// INITIALIZATION: Empty cache, ready for immediate use
        /// </summary>
        /// <param name="maxSize">Maximum number of items to cache</param>
        public LRUCache(int maxSize)
        {
            _maxSize = maxSize;
            _cache = new ConcurrentDictionary<TKey, LinkedListNode<CacheItem>>();
            _lruList = new LinkedList<CacheItem>();
        }

        #endregion

        #region Public Methods - Cache Operations

        /// <summary>
        /// Attempts to retrieve a value from the cache
        /// PERFORMANCE: O(1) average case due to dictionary lookup
        /// SIDE EFFECT: Successful access moves item to front (most recently used)
        /// THREAD SAFETY: Coordinated access to dictionary and linked list
        /// </summary>
        /// <param name="key">Key to look up</param>
        /// <param name="value">Retrieved value (valid only if method returns true)</param>
        /// <returns>true if key was found, false otherwise</returns>
        public bool TryGet(TKey key, out TValue value)
        {
            value = default(TValue);

            // FAST LOOKUP: Check if key exists in cache
            if (_cache.TryGetValue(key, out var node))
            {
                // CRITICAL SECTION: Update access order atomically
                lock (_lock)
                {
                    // LRU UPDATE: Move accessed item to front (most recently used position)
                    // ALGORITHM: Remove from current position, add to head
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                }

                // SUCCESS: Return cached value
                value = node.Value.Value;
                return true;
            }

            // CACHE MISS: Key not found
            return false;
        }

        /// <summary>
        /// Adds or updates an item in the cache
        /// BEHAVIOR: Update existing items, add new items with eviction if necessary
        /// EVICTION: Removes least recently used item when at capacity
        /// PERFORMANCE: O(1) for all operations (dictionary and linked list)
        /// </summary>
        /// <param name="key">Key to add or update</param>
        /// <param name="value">Value to cache</param>
        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                // UPDATE EXISTING: If key already exists, update value and move to front
                if (_cache.TryGetValue(key, out var existingNode))
                {
                    // VALUE UPDATE: Modify existing cache item
                    existingNode.Value.Value = value;

                    // LRU UPDATE: Move to front (most recently used)
                    _lruList.Remove(existingNode);
                    _lruList.AddFirst(existingNode);
                }
                else
                {
                    // ADD NEW ITEM: Create new cache entry
                    var newNode = new LinkedListNode<CacheItem>(
                        new CacheItem { Key = key, Value = value }
                    );

                    // CAPACITY CHECK: Evict least recently used item if at maximum capacity
                    if (_cache.Count >= _maxSize)
                    {
                        // EVICTION: Remove least recently used item (tail of linked list)
                        var lastNode = _lruList.Last;
                        if (lastNode != null)
                        {
                            // CLEANUP: Remove from both data structures
                            _lruList.RemoveLast(); // Remove from LRU order
                            _cache.TryRemove(lastNode.Value.Key, out _); // Remove from lookup table
                        }
                    }

                    // INSERTION: Add new item to front (most recently used position)
                    _lruList.AddFirst(newNode); // Add to LRU order (front = most recent)
                    _cache[key] = newNode; // Add to lookup table
                }
            }
        }

        /// <summary>
        /// Removes all items from the cache
        /// ATOMICITY: Clears both data structures atomically
        /// USAGE: Memory pressure relief, cache invalidation, testing
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _cache.Clear(); // Clear lookup table
                _lruList.Clear(); // Clear LRU order list
            }
        }

        #endregion

        #region Public Properties - Cache Statistics

        /// <summary>
        /// Gets the current number of items in the cache
        /// THREAD SAFETY: May be slightly inaccurate due to concurrent access
        /// USAGE: Monitoring, debugging, capacity planning
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// Gets the maximum capacity of the cache
        /// IMMUTABLE: Set at construction time, never changes
        /// USAGE: Capacity planning, configuration validation
        /// </summary>
        public int MaxSize => _maxSize;

        #endregion

        #region Private Helper Classes

        /// <summary>
        /// Internal cache item structure
        /// PURPOSE: Stores key-value pair in linked list nodes
        /// DESIGN: Simple data holder, no behavior
        /// </summary>
        private class CacheItem
        {
            public TKey Key { get; set; } // Cache key (for eviction tracking)
            public TValue Value { get; set; } // Cached value
        }

        #endregion

        #region Algorithm Analysis and Performance Notes

        /*
        COMPLEXITY ANALYSIS:

        1. TryGet Operation:
           - Dictionary lookup: O(1) average, O(n) worst case (hash collisions)
           - Linked list update: O(1) (direct node manipulation)
           - Overall: O(1) average case

        2. Add Operation:
           - Dictionary operations: O(1) average case
           - Linked list operations: O(1) (head/tail manipulation)
           - Overall: O(1) average case

        3. Space Complexity:
           - Dictionary storage: O(n) where n = number of cached items
           - Linked list storage: O(n) for nodes
           - Overall: O(n) space usage

        PERFORMANCE CHARACTERISTICS:

        1. Cache Hit Performance:
           - Fast dictionary lookup
           - Minimal linked list manipulation
           - Low memory allocation (no new objects)

        2. Cache Miss Performance:
           - Dictionary lookup (failed)
           - Possible eviction and cleanup
           - New object allocation for cache item

        3. Memory Usage:
           - Fixed overhead per item (dictionary entry + linked list node + cache item)
           - Bounded by maxSize parameter
           - Automatic cleanup via LRU eviction

        4. Concurrency Performance:
           - Lock contention increases with concurrent access
           - Dictionary provides some lock-free operations
           - Consider lock-free alternatives for high-concurrency scenarios

        USAGE RECOMMENDATIONS:

        1. Appropriate Cache Sizes:
           - Small cache (< 100 items): Excellent performance
           - Medium cache (100-1000 items): Good performance
           - Large cache (> 1000 items): Consider alternatives

        2. Key Design:
           - Use types with fast GetHashCode() implementation
           - Avoid mutable keys (undefined behavior)
           - Consider string interning for string keys

        3. Value Considerations:
           - Cache expensive-to-compute values
           - Consider memory usage of cached objects
           - Implement IDisposable for cached resources if needed

        4. Concurrency Guidelines:
           - Suitable for moderate concurrent access
           - Consider ConcurrentLRU alternatives for high concurrency
           - Monitor lock contention in performance-critical scenarios
        */

        #endregion
    }
}
