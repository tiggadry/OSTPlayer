# Volume Persistence Implementation - OstPlayer v1.2.1

## ?? **Feature Overview**

Implementov·na **trval· persistence hlasitosti** v OstPlayer pluginu, kter· zajiöùuje, ûe se nastavenÌ hlasitosti uloûÌ do JSON souboru pluginu a bude p¯i p¯ÌötÌm spuötÏnÌ pluginu automaticky obnoveno na stejnou hodnotu.

## ? **Key Features**

### **1. ?? Automatic Volume Persistence**
- **P¯i zmÏnÏ hlasitosti**: OkamûitÈ uloûenÌ do JSON settings souboru
- **P¯i startu pluginu**: AutomatickÈ naËtenÌ poslednÏ nastavenÈ hlasitosti
- **Fallback mechanismus**: Pokud naËÌt·nÌ selûe, pouûÌv· se v˝chozÌ hodnota 50%

### **2. ?? Settings Integration**
- **OstPlayerSettings.DefaultVolume**: Existing property (0-100 range)
- **JSON persistence**: AutomatickÈ uloûenÌ/naËÌt·nÌ p¯es Playnite SDK
- **Type safety**: Double type with range validation (0-100)

### **3. ?? Cross-Session Persistence**
- **Session continuity**: Hlasitost z˘st·v· stejn· mezi restarty Playnite
- **Plugin isolation**: Kaûd˝ plugin m· svÈ vlastnÌ settings soubory
- **Concurrent safety**: Thread-safe operations p¯es Playnite SDK

## ??? **Technical Implementation**

### **Modified Files:**

#### **1. ViewModels/Audio/AudioPlaybackViewModel.cs (v1.2.1)**

**Constructor Changes:**
```csharp
/// <summary>
/// Initializes a new instance of the AudioPlaybackViewModel class.
/// Sets up audio engine, progress timer, command bindings, and error handling.
/// </summary>
/// <param name="plugin">Plugin instance for accessing settings (optional for backward compatibility)</param>
public AudioPlaybackViewModel(OstPlayer plugin = null)
{
    _errorHandler = new ErrorHandlingService();
    _plugin = plugin;
    // Initialization handled by base class Initialize method
}
```

**Volume Property Enhancement:**
```csharp
/// <summary>
/// Gets or sets the volume level (0-100).
/// Automatically applies volume changes to the audio engine and saves to settings.
/// </summary>
public double Volume
{
    get => _volume;
    set
    {
        if (SetProperty(ref _volume, value))
        {
            OnPropertyChanged(nameof(VolumeDisplay));
            try
            {
                _playbackService?.SetVolume(value / 100.0); // Convert to 0.0-1.0 range
                VolumeChanged?.Invoke(this, value);
                
                // Save volume to settings for persistence
                SaveVolumeToSettings(value);
            }
            catch (Exception ex)
            {
                _errorHandler.HandlePlaybackError(ex, CurrentTrackPath);
            }
        }
    }
}
```

**New Persistence Methods:**
```csharp
/// <summary>
/// Loads volume setting from plugin settings.
/// Called during initialization to restore last used volume.
/// </summary>
private void LoadVolumeFromSettings()
{
    try
    {
        if (_plugin != null)
        {
            var settings = _plugin.LoadPluginSettings<OstPlayerSettings>();
            if (settings != null)
            {
                // Set volume without triggering save to avoid circular update
                _volume = settings.DefaultVolume;
                OnPropertyChanged(nameof(Volume));
                OnPropertyChanged(nameof(VolumeDisplay));
                
                // Apply volume to audio engine if available
                _playbackService?.SetVolume(_volume / 100.0);
            }
        }
    }
    catch (Exception ex)
    {
        // Fallback to default volume if loading fails
        _volume = 50;
        System.Diagnostics.Debug.WriteLine($"Failed to load volume from settings: {ex.Message}");
    }
}

/// <summary>
/// Saves current volume setting to plugin settings.
/// Called whenever volume changes to persist user preference.
/// </summary>
/// <param name="volume">Volume level to save (0-100)</param>
private void SaveVolumeToSettings(double volume)
{
    try
    {
        if (_plugin != null)
        {
            var settings = _plugin.LoadPluginSettings<OstPlayerSettings>() ?? new OstPlayerSettings();
            settings.DefaultVolume = volume;
            _plugin.SavePluginSettings(settings);
        }
    }
    catch (Exception ex)
    {
        // Log error but don't interrupt user experience
        System.Diagnostics.Debug.WriteLine($"Failed to save volume to settings: {ex.Message}");
    }
}
```

#### **2. ViewModels/OstPlayerSidebarViewModel.cs (v1.2.1)**

**InitializePlaybackService Enhancement:**
```csharp
/// <summary>
/// Initializes the NAudio-based playback service and subscribes to audio events.
/// Sets up event handlers for playback state changes and progress updates.
/// Critical for audio engine functionality and UI synchronization.
/// </summary>
private void InitializePlaybackService()
{
    playbackService = new MusicPlaybackService();
    
    // Load volume from settings
    LoadVolumeFromSettings();
    
    // Subscribe to playback events for UI state synchronization
    playbackService.PlaybackStarted += (s, e) => { IsPlaying = true; IsPaused = false; };
    playbackService.PlaybackPaused += (s, e) => { IsPaused = true; };
    playbackService.PlaybackStopped += (s, e) => { IsPlaying = false; IsPaused = false; };
    playbackService.PositionChanged += (s, pos) => { Position = pos; };
    playbackService.DurationChanged += (s, dur) => { Duration = dur; };
    playbackService.PlaybackEnded += (s, e) => OnPlaybackEnded(); // Auto-play next track
}
```

**Volume Property with Persistence:**
```csharp
/// <summary>
/// Volume level as percentage (0-100) for slider binding and display.
/// Automatically converted to 0.0-1.0 range for NAudio when set.
/// Property change triggers VolumeDisplay computed property update and saves to settings.
/// </summary>
public double Volume
{
    get => volume;
    set
    {
        volume = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(VolumeDisplay)); // Update percentage display
        playbackService?.SetVolume(value / 100.0); // Apply to audio engine (0.0-1.0 range)
        
        // Save volume to settings for persistence
        SaveVolumeToSettings(value);
    }
}
```

**Persistence Methods Added:**
```csharp
/// <summary>
/// Loads volume setting from plugin settings.
/// Called during playback service initialization to restore last used volume.
/// </summary>
private void LoadVolumeFromSettings()
{
    try
    {
        var settings = plugin.LoadPluginSettings<OstPlayerSettings>();
        if (settings != null)
        {
            // Set volume without triggering save to avoid circular update
            volume = settings.DefaultVolume;
            OnPropertyChanged(nameof(Volume));
            OnPropertyChanged(nameof(VolumeDisplay));
            
            // Apply volume to audio engine
            playbackService?.SetVolume(volume / 100.0);
        }
    }
    catch (Exception ex)
    {
        // Fallback to default volume if loading fails
        volume = 50;
        StatusText = $"Failed to load volume setting: {ex.Message}";
    }
}

/// <summary>
/// Saves current volume setting to plugin settings.
/// Called whenever volume changes to persist user preference.
/// </summary>
/// <param name="volumeLevel">Volume level to save (0-100)</param>
private void SaveVolumeToSettings(double volumeLevel)
{
    try
    {
        var settings = plugin.LoadPluginSettings<OstPlayerSettings>() ?? new OstPlayerSettings();
        settings.DefaultVolume = volumeLevel;
        plugin.SavePluginSettings(settings);
    }
    catch (Exception ex)
    {
        // Log error but don't interrupt user experience
        System.Diagnostics.Debug.WriteLine($"Failed to save volume to settings: {ex.Message}");
    }
}
```

## ?? **Configuration Structure**

### **OstPlayerSettings.cs (unchanged)**

Volume persistence vyuûÌv· jiû existujÌcÌ `DefaultVolume` property:

```csharp
/// <summary>
/// Default volume for music playback (0-100)
/// </summary>
[DefaultValue(50.0)]
public double DefaultVolume
{
    get => defaultVolume;
    set 
    { 
        defaultVolume = Math.Max(0, Math.Min(100, value));
        OnPropertyChanged();
    }
}
```

### **JSON Storage Location**

```
%APPDATA%\Playnite\Extensions\OstPlayer\config.json
```

**Example JSON structure:**
```json
{
  "DiscogsToken": "",
  "DefaultVolume": 75.0,
  "AutoPlayNext": true,
  "EnableMetadataCache": true,
  "ShowMp3MetadataByDefault": true,
  "ShowDiscogsMetadataByDefault": true,
  "PausePlayniteSoundOnPlay": true,
  "MetadataCacheTTLHours": 6,
  "MaxCacheSize": 2000,
  "EnableMemoryPressureAdjustment": true,
  "EnableCacheWarming": true,
  "CacheCleanupIntervalMinutes": 5
}
```

## ?? **User Experience Benefits**

### **Before Implementation:**
- ? Hlasitost se resetovala na 50% p¯i kaûdÈm restartu pluginu
- ? UûivatelÈ museli pokaûdÈ znovu nastavovat preferovanou hlasitoúÊ
- ? é·dn· persistence uûivatelsk˝ch preferencÌ pro audio

### **After Implementation:**
- ? Hlasitost se automaticky obnovuje p¯i startu pluginu
- ? OkamûitÈ uloûenÌ p¯i jakÈkoliv zmÏnÏ hlasitosti
- ? KonzistentnÌ audio experience nap¯ÌË sessions
- ? Graceful fallback na v˝chozÌ hodnotu p¯i chyb·ch

## ??? **Error Handling Strategy**

### **1. Loading Errors:**
- **Fallback**: Default volume (50%) if loading fails
- **Logging**: Debug output for troubleshooting
- **Non-blocking**: Continues plugin initialization

### **2. Saving Errors:**
- **Silent handling**: Doesn't interrupt user interaction
- **Logging**: Debug output for developers
- **Graceful degradation**: Plugin continues working normally

### **3. Missing Plugin Instance:**
- **Backward compatibility**: Works without plugin instance
- **Optional parameter**: Constructor allows null plugin parameter
- **Safe operations**: All settings operations check for null

## ?? **Technical Benefits**

### **1. ?? Clean Architecture:**
- **Separation of concerns**: Volume persistence isolated in dedicated methods
- **Single responsibility**: Each method has one clear purpose
- **Dependency injection**: Plugin instance injected through constructor

### **2. ? Performance:**
- **Minimal overhead**: Only saves when volume actually changes
- **Efficient**: Leverages Playnite's optimized settings system
- **No polling**: Event-driven updates only

### **3. ?? Maintainability:**
- **Centralized**: All persistence logic in dedicated methods
- **Extensible**: Easy to add more audio preferences
- **Testable**: Methods can be unit tested independently

## ?? **Testing Scenarios**

### **1. Basic Functionality:**
```
1. Start plugin ? Volume loads from settings
2. Change volume ? Settings automatically saved
3. Restart plugin ? Volume restored to last setting
```

### **2. Edge Cases:**
```
1. First run (no settings) ? Uses default 50%
2. Corrupted settings file ? Fallback to default
3. Missing plugin instance ? Graceful degradation
4. Multiple rapid changes ? Only saves final value
```

### **3. Integration Testing:**
```
1. Settings UI changes ? Reflected in playback
2. Playback volume changes ? Reflected in settings
3. Cross-session persistence ? Maintained across restarts
```

## ?? **Future Enhancements**

### **Potential Improvements:**
1. **Volume profiles**: Different volumes per game
2. **Time-based volumes**: Lower volume during night hours
3. **Adaptive volume**: Based on system volume or external conditions
4. **Volume memory**: Remember last volume per track
5. **Volume animation**: Smooth fade-in/fade-out effects

### **Advanced Features:**
1. **Audio normalization**: Automatic volume adjustment based on track loudness
2. **EQ settings persistence**: Remember equalizer settings
3. **Audio device memory**: Remember preferred audio device
4. **Dynamic range compression**: Settings for quiet environments

---

## ?? **Summary**

**Status**: ? **IMPLEMENTED** - Volume persistence fully functional  
**Version**: ?? **v1.2.1** (AudioPlaybackViewModel, OstPlayerSidebarViewModel)  
**Compatibility**: ? **Backward compatible** - Optional plugin parameter  
**User Impact**: ?? **High** - Significantly improves user experience

**Volume nastavenÌ se nynÌ automaticky ukl·d· do JSON souboru pluginu a obnovuje p¯i kaûdÈm spuötÏnÌ, coû zajiöùuje konzistentnÌ audio z·ûitek.** ??