# OstPlayerSidebarViewModel Refactoring Plan

##  Current State Analysis (v1.2.0)

### **File Statistics:**
- **Size**: 800+ lines of code (critical threshold exceeded)
- **Classes**: 1 monolithic ViewModel class
- **Responsibilities**: 12+ distinct concerns in single class
- **Dependencies**: 9+ direct dependencies (tight coupling)
- **Methods**: 50+ methods mixing different concerns

### **Identified Responsibilities (SRP Violations):**

| **#** | **Responsibility** | **Lines** | **Complexity** | **Extract Target** |
|-------|-------------------|-----------|----------------|------------------|
| 1 | **Game Selection & Filtering** | ~80 | Medium | GameSelectionViewModel |
| 2 | **Audio Playback Control** | ~120 | High | AudioPlaybackViewModel |
| 3 | **Playlist Management** | ~60 | Medium | PlaylistViewModel |
| 4 | **MP3 Metadata Handling** | ~80 | Medium | Mp3MetadataViewModel |
| 5 | **Discogs Metadata Integration** | ~150 | High | DiscogsMetadataViewModel |
| 6 | **Cache Management** | ~40 | Low | MetadataManagerViewModel |
| 7 | **UI State Management** | ~60 | Medium | StatusViewModel |
| 8 | **Progress Tracking** | ~40 | Low | AudioPlaybackViewModel |
| 9 | **Volume Control** | ~30 | Low | AudioPlaybackViewModel |
| 10 | **Auto-play Logic** | ~50 | Medium | PlaylistViewModel |
| 11 | **Command Binding** | ~60 | Medium | Distributed to specific VMs |
| 12 | **Event Communication** | ~40 | Low | Core coordinator |

### **Dependency Analysis:**

#### **Current Direct Dependencies:**
1. `OstPlayer` plugin reference
2. `MusicPlaybackService` (NAudio wrapper)
3. `DispatcherTimer` (progress updates)
4. `DiscogsClient` (external API)
5. `Mp3MetadataReader` (local metadata)
6. `MusicFileHelper` (file discovery)
7. `MetadataJsonSaver` (cache persistence)
8. `RelayCommand` (MVVM commands)
9. `OstPlayerSettingsViewModel` (configuration)

#### **Tight Coupling Issues:**
- **Direct instantiation** of services (not testable)
- **Mixed concerns** in single constructor
- **Hard dependencies** on concrete classes
- **No interface abstractions**

##  Refactoring Strategy (Performance Pattern Application)

### **Target Architecture:**
```
ViewModels/
 Core/
    OstPlayerSidebarViewModel.cs      # Main coordinator (200 lines max)
    ViewModelBase.cs                  # Shared MVVM infrastructure
    IViewModelCoordinator.cs          # Coordination interface
 Audio/
    AudioPlaybackViewModel.cs         # Playback control & progress
    PlaylistViewModel.cs              # Auto-play & track management
    IAudioViewModel.cs                # Audio abstraction
 Metadata/
    MetadataManagerViewModel.cs       # Metadata coordination
    Mp3MetadataViewModel.cs           # ID3 tag handling
    DiscogsMetadataViewModel.cs       # External metadata
    IMetadataViewModel.cs             # Metadata abstractions
 UI/
    GameSelectionViewModel.cs         # Game filtering & selection
    StatusViewModel.cs                # Status messages & UI state
    IUIViewModel.cs                   # UI state abstractions
 README.md                             # Refactoring documentation
```

### **Implementation Phases:**

#### **Phase 1: Infrastructure Preparation (Week 1)**
1. Create ViewModels subfolder structure
2. Create ViewModelBase with shared functionality
3. Define interface contracts
4. Create namespace bridge for backward compatibility

#### **Phase 2: Extract Audio Concerns (Week 2)**
1. Extract AudioPlaybackViewModel (playback, progress, volume)
2. Extract PlaylistViewModel (auto-play, track navigation)
3. Update main ViewModel to use audio delegates

#### **Phase 3: Extract Metadata Concerns (Week 3)**
1. Extract Mp3MetadataViewModel (ID3 tag handling)
2. Extract DiscogsMetadataViewModel (external API integration)
3. Extract MetadataManagerViewModel (coordination)
4. Update main ViewModel to use metadata delegates

#### **Phase 4: Extract UI Concerns (Week 4)**
1. Extract GameSelectionViewModel (filtering, selection)
2. Extract StatusViewModel (status messages, UI state)
3. Update main ViewModel to coordinate UI state

#### **Phase 5: Final Coordination & Testing (Week 5)**
1. Reduce main ViewModel to pure coordinator role
2. Implement comprehensive unit tests
3. Performance testing and optimization
4. Documentation completion

### **Success Metrics (Based on Performance Pattern):**

| **Metric** | **Current** | **Target** | **Improvement** |
|------------|-------------|------------|-----------------|
| **Main ViewModel Size** | 800+ lines | 200 lines | -75% complexity |
| **SRP Compliance** |  12+ concerns |  1 concern | Fixed violations |
| **Testability** |  Monolithic |  Focused | Isolated testing |
| **Coupling** |  Tight |  Loose | Interface-based |
| **Maintainability** |  Complex |  Modular | Single purpose files |

### **Risk Mitigation Strategy:**

#### **Backward Compatibility:**
- **Namespace bridge** preserves existing API
- **Property delegation** maintains data binding
- **Event forwarding** preserves View communication
- **Command delegation** maintains UI bindings

#### **Incremental Approach:**
- **One concern at a time** to minimize risk
- **Feature flags** for safe rollback
- **Comprehensive testing** after each extraction
- **Build verification** at each step

#### **Breaking Change Prevention:**
- **Public API preservation** through delegation
- **Interface compatibility** maintenance
- **Event signature preservation**
- **Property name consistency**

##  Next Action Items:

### **Immediate (This Session):**
1.  Create ViewModels subfolder structure
2.  Create ViewModelBase infrastructure
3.  Define interface contracts
4.  Begin AudioPlaybackViewModel extraction

### **Phase 1 Goals:**
- [ ] Complete audio concern extraction
- [ ] Maintain 100% functionality
- [ ] Zero breaking changes
- [ ] Build successful verification

---

**Based on Performance refactoring success:**
- **Pattern**: PROVEN effective (53% size reduction achieved)
- **Risk**: LOW (namespace bridge strategy validated)
- **Confidence**: HIGH (methodology established)
- **Timeline**: 4-5 weeks for complete refactoring

**Ready to begin implementation using Performance module as template! **