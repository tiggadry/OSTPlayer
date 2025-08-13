# OstPlayer - Standard File Header Templates

This document contains standardized templates for file headers across all file types in the OstPlayer project.

## Date Guidelines for Headers

**CRITICAL FOR AI ASSISTANTS - MANDATORY DATE PROTOCOL:**

 **NEVER USE THESE PLACEHOLDER VALUES AS ACTUAL DATES:**
- `[CURRENT_DATE]` = PLACEHOLDER - REPLACE WITH TODAY'S DATE
- `[original_creation_date]` = PLACEHOLDER - REPLACE WITH ACTUAL CREATION DATE  
- `[current_version]` = PLACEHOLDER - REPLACE WITH ACTUAL VERSION

**ALWAYS FOLLOW THESE RULES:**
- **CREATED**: Use the actual date when the file was first created
- **UPDATED**:  ALWAYS ASK USER FOR TODAY'S DATE - NEVER USE OLD DATES
- **CHANGELOG**:  ALWAYS USE TODAY'S DATE FOR NEW ENTRIES

**AI ASSISTANT PROTOCOL:**
1.  BEFORE ANY UPDATE: Ask user "What is today's date for file updates"
2.  Replace [CURRENT_DATE] with confirmed current date
3.  Use format YYYY-MM-DD only
4.  NEVER copy dates from existing files
5.  NEVER use placeholder values as real dates

## Basic Template (for all files)

```csharp
// ====================================================================
// FILE: [FileName.cs]  REPLACE WITH ACTUAL FILENAME
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: [ViewModels|Models|Services|Utils|Clients|Views|Converters]  CHOOSE ACTUAL MODULE
// LOCATION: [relative/path/]  REPLACE WITH ACTUAL PATH
// VERSION: [current_version]  REPLACE WITH ACTUAL VERSION (e.g., 1.2.1)
// CREATED: [original_creation_date]  REPLACE WITH ACTUAL CREATION DATE
// UPDATED: [ASK_USER_FOR_TODAYS_DATE]   ASK USER FOR TODAY'S DATE
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// [Clear description of file purpose and responsibilities]
//
// FEATURES:
// - [List of main features]
// - [What this file provides]
// - [Key capabilities]
//
// DEPENDENCIES:
// - [External libraries]
// - [Internal dependencies]
// - [Playnite SDK components]
//
// [Additional sections based on file type - see below]
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - [Specific library versions]
//
// CHANGELOG:
// [ASK_USER_FOR_TODAYS_DATE] v[version] - [description of changes]
// ====================================================================
```

## Specific Templates by File Type

### ViewModels
```csharp
// DESIGN PATTERNS:
// - MVVM (Model-View-ViewModel)
// - Command Pattern (for UI actions)
// - Observer Pattern (INotifyPropertyChanged)

// PERFORMANCE NOTES:
// - [UI responsiveness considerations]
// - [Data binding optimizations]
// - [Memory usage patterns]

// FUTURE REFACTORING:
// TODO: [Planned ViewModel improvements]
// TODO: [Command optimizations]
// TODO: [Property change optimizations]
```

### Models/Data Classes
```csharp
// DATA STRUCTURE:
// - [Description of data fields]
// - [Relationships to other models]
// - [Validation rules]

// SERIALIZATION:
// - [JSON/XML serialization notes]
// - [Database persistence]
// - [API compatibility]

// FUTURE REFACTORING:
// TODO: [Data validation improvements]
// TODO: [Performance optimizations]
// TODO: [Schema evolution plans]
```

### Services
```csharp
// SERVICE RESPONSIBILITIES:
// - [Primary service functions]
// - [External integrations]
// - [Internal coordination]

// THREAD SAFETY:
// - [Concurrent access patterns]
// - [Synchronization mechanisms]
// - [Async/await usage]

// ERROR HANDLING:
// - [Exception handling strategy]
// - [Retry mechanisms]
// - [Fallback behaviors]

// FUTURE REFACTORING:
// TODO: [Service interface improvements]
// TODO: [Dependency injection migration]
// TODO: [Performance optimizations]
```

### Utils/Helper Classes
```csharp
// UTILITY FUNCTIONS:
// - [List of static methods]
// - [Common operations]
// - [Helper algorithms]

// PERFORMANCE NOTES:
// - [Algorithm complexity]
// - [Memory allocation patterns]
// - [Optimization opportunities]

// FUTURE REFACTORING:
// TODO: [Code reuse improvements]
// TODO: [Performance optimizations]
// TODO: [API simplification]
```

### Clients/External API
```csharp
// API INTEGRATION:
// - [External service details]
// - [Authentication methods]
// - [Rate limiting]

// REQUEST/RESPONSE:
// - [API endpoints used]
// - [Data transformation]
// - [Error handling]

// FUTURE REFACTORING:
// TODO: [API versioning support]
// TODO: [Caching improvements]
// TODO: [Resilience patterns]
```

### Views/UI Components
```csharp
// UI RESPONSIBILITIES:
// - [User interaction handling]
// - [Data display]
// - [Navigation]

// ACCESSIBILITY:
// - [Keyboard navigation]
// - [Screen reader support]
// - [Localization readiness]

// FUTURE REFACTORING:
// TODO: [UI/UX improvements]
// TODO: [Responsiveness enhancements]
// TODO: [Accessibility compliance]
```

## Extended Sections for Complex Files

### For files with significant algorithms:
```csharp
// ALGORITHMS:
// - [Description of key algorithms]
// - [Complexity analysis]
// - [Trade-off decisions]

// OPTIMIZATION NOTES:
// - [Performance characteristics]
// - [Memory usage patterns]
// - [Bottlenecks and solutions]
```

### For files with external dependencies:
```csharp
// EXTERNAL DEPENDENCIES:
// - [Library name] v[version] - [purpose]
// - [API] - [integration details]
// - [Service] - [connection requirements]

// INTEGRATION PATTERNS:
// - [How external services are used]
// - [Error handling for external failures]
// - [Fallback mechanisms]
```

### For experimental or beta features:
```csharp
// EXPERIMENTAL FEATURES:
// - [List of beta/experimental functionality]
// - [Known issues and workarounds]
// - [Timeline for stabilization]

// FEEDBACK:
// - [How to report issues]
// - [Areas needing user feedback]
// - [Testing requirements]
```

## Complete Header Examples

### Example for Service (Updated Template with Clear AI Instructions)
```csharp
// ====================================================================
// FILE: MetadataService.cs  REPLACE WITH ACTUAL FILENAME
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Services  CHOOSE: ViewModels|Models|Services|Utils|Clients|Views|Converters
// LOCATION: Services/  REPLACE WITH ACTUAL RELATIVE PATH
// VERSION: 1.2.1  REPLACE WITH ACTUAL CURRENT VERSION
// CREATED: 2025-08-06  REPLACE WITH ACTUAL FILE CREATION DATE  
// UPDATED: [ASK_USER_FOR_TODAYS_DATE]   ASK USER: "What is today's date"
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Centralized service for metadata operations including loading, merging,
// caching, and synchronization of music metadata from multiple sources
// (MP3 tags, Discogs, MusicBrainz, local cache).
//
// FEATURES:
// - Multi-source metadata aggregation
// - Intelligent caching with LRU eviction
// - Background metadata fetching
// - Conflict resolution between sources
// - Metadata validation and normalization
//
// DEPENDENCIES:
// - OstPlayer.Utils.Mp3MetadataReader (local metadata)
// - OstPlayer.Clients.DiscogsClient (external metadata)
// - OstPlayer.Clients.MusicBrainzClient (external metadata)
// - System.Collections.Concurrent (thread-safe collections)
// - Newtonsoft.Json (serialization)
//
// SERVICE RESPONSIBILITIES:
// - Coordinate metadata loading from all sources
// - Manage metadata cache lifecycle
// - Provide unified metadata interface to UI
// - Handle background processing and updates
//
// THREAD SAFETY:
// - Uses ConcurrentDictionary for cache storage
// - Async/await for I/O operations
// - Thread-safe event handlers
//
// ERROR HANDLING:
// - Graceful degradation when external services fail
// - Retry logic with exponential backoff
// - Comprehensive logging of failures
//
// PERFORMANCE NOTES:
// - LRU cache with configurable size limits
// - Background loading prevents UI blocking
// - Minimal memory allocation in hot paths
// - Efficient metadata merging algorithms
//
// LIMITATIONS:
// - Cache is in-memory only (lost on restart)
// - No persistent storage for aggregated metadata
// - Limited to configured external services
//
// FUTURE REFACTORING:
// TODO: Implement persistent cache storage
// TODO: Add metadata source prioritization
// TODO: Implement real-time metadata updates
// TODO: Add metadata conflict resolution UI
// TODO: Extract caching to separate service
// TODO: Add metadata export/import functionality
// CONSIDER: Plugin architecture for metadata sources
// IDEA: Machine learning for metadata quality scoring
//
// TESTING:
// - Unit tests for cache management
// - Integration tests with external APIs
// - Performance tests for large metadata sets
// - Concurrency tests for thread safety
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Newtonsoft.Json 13.x
//
// CHANGELOG:
// [ASK_USER_FOR_TODAYS_DATE] v1.2.1 - Current changes description   REPLACE WITH ACTUAL DATE
// 2025-08-07 v1.2.0 - Previous changes  KEEP EXISTING ENTRIES AS-IS
// ====================================================================
```

##  FINAL WARNING FOR AI ASSISTANTS:

**THESE ARE EXAMPLE DATES - DO NOT COPY THEM:**
- The dates `2025-08-06`, `2025-08-07` shown above are EXAMPLES ONLY
- You MUST ask the user for the current date
- You MUST NOT use these example dates in real files
- You MUST replace [ASK_USER_FOR_TODAYS_DATE] with user-confirmed date