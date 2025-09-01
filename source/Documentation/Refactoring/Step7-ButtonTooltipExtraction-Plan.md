# 🚀 STEP 7: Button Tooltip Micro-Extraction - PlayPauseButtonToolTip Helper

## 🎯 **Cíl Step 7**
Provést **pátou micro-extraction** - nahradit `PlayPauseButtonToolTip` computed property helper metodou z UIHelper.

## 🔍 **Current State Analysis**

### **NALEZENÝ TARGET v OstPlayerSidebarViewModel.cs:**
```csharp
// Řádek ~686 - Computed Properties for UI Binding section:
/// <summary>Play/pause button tooltip based on current state.</summary>
public string PlayPauseButtonToolTip => (IsPlaying && !IsPaused) ? "Pause playback" : "Play selected track";
```

### **INFRASTRUKTURA READY:**
- ✅ **Steps 3-6 Completed** - VolumeHelper + TimeHelper + UIHelper integrations successful
- ✅ **UIHelper exists** - obsahuje `GetPlayPauseTooltip(bool isPlaying, bool isPaused)` metodu
- ✅ **Using statement exists** - `using OstPlayer.Utils.Helpers;` already added
- ✅ **Build stable** - all previous steps passed tests

## 🎯 **Micro-Extraction Plan**

### **HELPER ALREADY EXISTS:**
UIHelper.cs už obsahuje potřebnou metodu:

```csharp
/// <summary>
/// Gets appropriate tooltip text for play/pause button based on current state.
/// </summary>
/// <param name="isPlaying">Whether audio is currently playing</param>
/// <param name="isPaused">Whether audio is currently paused</param>
/// <returns>Descriptive tooltip text</returns>
public static string GetPlayPauseTooltip(bool isPlaying, bool isPaused)
{
    return (isPlaying && !isPaused) ? "Pause playback" : "Play selected track";
}
```

### **ZMĚNA:**
```csharp
// PŘED (current inline logic):
public string PlayPauseButtonToolTip => (IsPlaying && !IsPaused) ? "Pause playback" : "Play selected track";

// PO (with helper):
public string PlayPauseButtonToolTip => UIHelper.GetPlayPauseTooltip(IsPlaying, IsPaused);
```

### **REQUIRED CHANGES:**

#### **1. Using Statement** 
✅ **Already Added** - `using OstPlayer.Utils.Helpers;` from previous steps

#### **2. UIHelper Method**
✅ **Already Created** - `GetPlayPauseTooltip()` method exists in UIHelper from Step 6

#### **3. Replace Computed Property**
```csharp
// Nahradit existující property:
/// <summary>Play/pause button tooltip based on current state.</summary>
public string PlayPauseButtonToolTip => UIHelper.GetPlayPauseTooltip(IsPlaying, IsPaused);
```

## 🧪 **Testing Strategy**

### **PRE-CHANGE TESTS:**
1. ✅ Build successful (carried from Step 6)
2. ✅ Play/pause tooltip displays correctly ("Pause playback" / "Play selected track")
3. ✅ Tooltip changes during play/pause operations
4. ✅ Tooltip shows immediately on hover

### **POST-CHANGE TESTS:**
1. 🧪 Build successful
2. 🧪 Play/pause tooltip still displays correctly
3. 🧪 Tooltip changes still work during operations
4. 🧪 Same tooltip text as before
5. 🧪 UIHelper integration working (second usage)
6. 🧪 No regressions anywhere

### **VALIDATION POINTS:**
- Play tooltip shows "Play selected track" when stopped/paused
- Pause tooltip shows "Pause playback" when playing
- Tooltip updates immediately on state changes
- Hover functionality unchanged
- No visual differences in UI

## ⚡ **Risk Assessment**

### **🟢 MINIMAL RISK FACTORS:**
- **Same pattern as Step 6** - proven UIHelper approach works
- **Single property change** - smallest possible modification
- **Helper method exists** - GetPlayPauseTooltip() already created and tested
- **Using statement exists** - no additional imports needed
- **Easy rollback** - single line change to revert

### **🔍 POTENTIAL ISSUES:**
- Tooltip text different → Visual validation will catch
- State timing → Tooltip updates should be immediate
- Performance difference → Minimal (static method call)

## 📋 **Step-by-Step Execution**

### **Step 7.1: Pre-Change Validation**
1. Verify current build status
2. Test play/pause tooltip functionality
3. Document current tooltip behavior

### **Step 7.2: Make Change**
1. Replace PlayPauseButtonToolTip property implementation
2. Verify file saves correctly

### **Step 7.3: Post-Change Validation**
1. Run build test
2. Test play/pause tooltip functionality
3. Compare with documented behavior
4. Verify UI state changes work
5. Run full plugin test

### **Step 7.4: Commit or Rollback**
- If all tests pass → Commit change
- If any test fails → Immediate rollback

## 🎖️ **Success Criteria**

### **MUST HAVE:**
- ✅ Build successful
- ✅ Play/pause tooltip displays correct text
- ✅ Tooltip changes work identically
- ✅ No visual differences in UI
- ✅ No performance degradation

### **BONUS POINTS:**
- ✅ UIHelper second usage confirmed (validates helper utility)
- ✅ Consistent UI pattern (symbol + tooltip both use UIHelper)
- ✅ Parallel property extraction (Step 6 & 7 related)
- ✅ Foundation for more UI extractions

## 📁 **Files to Modify**

### **SINGLE FILE CHANGE:**
- `ViewModels/OstPlayerSidebarViewModel.cs`
  - Change: PlayPauseButtonToolTip property implementation only

### **NO OTHER FILES:**
- ✅ Zero changes to any other files
- ✅ Zero changes to UI/XAML
- ✅ Zero changes to helpers (method already exists)
- ✅ Using statement already exists

## 🔄 **Rollback Procedure**

### **IMMEDIATE ROLLBACK IF:**
- ❌ Build fails
- ❌ Play/pause tooltip broken
- ❌ Different tooltip text shown
- ❌ Tooltip state changes broken
- ❌ Any UI regression detected

### **ROLLBACK STEPS:**
```csharp
// 1. Revert property change:
public string PlayPauseButtonToolTip => (IsPlaying && !IsPaused) ? "Pause playback" : "Play selected track";

// 2. Verify build
// 3. Verify functionality
```

## 🚀 **Next Steps After Success**

### **IF STEP 7 SUCCESSFUL:**
- Document lessons learned
- Plan Step 8: Toggle text extraction (Mp3MetadataToggleText)
- Consider metadata toggle patterns
- Build more UI helper usage

### **TARGET for Step 8:**
```csharp
// Natural next candidate (toggle text pattern):
public string Mp3MetadataToggleText => UIHelper.GetToggleText("MP3 metadata", IsMp3MetadataVisible);

// Alternative - parallel toggle pattern:
public string DiscogsMetadataToggleText => UIHelper.GetToggleText("Discogs metadata", IsDiscogsMetadataVisible);
```

## 🎯 **Expected UIHelper Benefits Validation**

### **Step 6 + 7 Combined Benefits:**
- ✅ Centralized UI logic (symbol + tooltip)
- ✅ Consistent button behavior patterns
- ✅ Reusable helper methods proven
- ✅ Foundation for more UI extractions

### **UIHelper Usage After Step 7:**
```csharp
// Two methods used:
UIHelper.GetPlayPauseButtonSymbol(IsPlaying, IsPaused)  // Step 6
UIHelper.GetPlayPauseTooltip(IsPlaying, IsPaused)      // Step 7

// Available for future steps:
UIHelper.GetToggleText(itemName, isVisible)
UIHelper.FormatPlaybackStatus(trackName, isPlaying, isPaused)
// + many more utility methods
```

## 📈 **Progress Tracking**

### **Steps Completed:**
- ✅ **Step 1**: Infrastructure (ViewModelBase)
- ✅ **Step 2**: Helper Utilities (VolumeHelper + TimeHelper)
- ✅ **Step 3**: First Micro-Extraction (VolumeDisplay - VolumeHelper)
- ✅ **Step 4**: Second Micro-Extraction (CurrentTime - TimeHelper)
- ✅ **Step 5**: Third Micro-Extraction (DurationTime - TimeHelper)
- ✅ **Step 6**: Fourth Micro-Extraction (PlayPauseButtonContent - UIHelper)
- 🎯 **Step 7**: Fifth Micro-Extraction (PlayPauseButtonToolTip - UIHelper) ← **CURRENT**

### **Confidence Metrics:**
- 🟢 **Methodology**: Proven through 6 successful steps
- 🟢 **Build stability**: 100% success rate
- 🟢 **Helper integration**: Multiple helpers + multiple usages working
- 🟢 **UI Helper validation**: Second usage will prove utility value

---

**Status**: 🎯 **READY TO EXECUTE**  
**Risk Level**: 🟢 **MINIMAL** (established pattern, existing helper method)  
**Confidence**: 🟢 **MAXIMUM** (methodology perfected)  
**Expected Time**: ⏱️ **2 minutes** (faster due to existing helper method)  
**Pattern**: 🔄 **PARALLEL** (Step 6 + 7 both use UIHelper for play/pause button)

**Ready to proceed with Step 7.1: Pre-Change Validation?** 🚀
