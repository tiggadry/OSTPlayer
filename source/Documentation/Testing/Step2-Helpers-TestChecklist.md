# Testovací checklist - Safe Refactoring Step 2: Helper Utilities

## ?? **Mandatory Testing Protocol**

### **Build Tests**
- [ ] ? **Compilation Success**: Projekt se buildu úspìšnì bez chyb
- [ ] ? **No New Warnings**: ádná nová warnings nebyla pøidána
- [ ] ? **All References Resolve**: Všechny helper utilities se resolved správnì
- [ ] ? **Helper Classes Compile**: VolumeHelper a TimeHelper kompilují bez chyb

### **Plugin Load Tests**
- [ ] **Plugin Loads in Playnite**: Plugin se naète bez chyb
- [ ] **Sidebar Appears**: OstPlayer sidebar se zobrazí v Playnite
- [ ] **No Crash on Load**: ádnı crash pøi naèítání pluginu
- [ ] **Helpers Not Used**: Helper utilities nejsou zatím pouity (zero risk)

### **Core Functionality Tests**
- [ ] **Game Selection Works**: Lze vybrat hru z dropdown
- [ ] **Music Files Load**: Muzika files se naètou pro vybranou hru
- [ ] **Audio Playback Works**: Play/Pause/Stop funguje správnì
- [ ] **Volume Control Works**: Volume slider mìní hlasitost (original code)
- [ ] **Progress Tracking Works**: Progress bar se aktualizuje (original code)
- [ ] **Metadata Display Works**: MP3 metadata se zobrazují správnì

### **Helper Infrastructure Tests**
- [ ] **VolumeHelper Available**: VolumeHelper class je dostupná v namespace
- [ ] **TimeHelper Available**: TimeHelper class je dostupná v namespace
- [ ] **Static Methods Work**: Static helper methods jsou callable
- [ ] **No Side Effects**: Helper utilities neovlivòují existing functionality

### **Advanced Feature Tests**
- [ ] **Discogs Integration Works**: Discogs metadata loading funguje
- [ ] **Auto-Play Works**: Auto-play next track funguje
- [ ] **Cover Images Work**: Cover images se zobrazují
- [ ] **Settings Persist**: Nastavení se ukládají a obnovují

### **UI Responsiveness Tests**
- [ ] **No UI Freezing**: UI nezmrzne bìhem operací
- [ ] **Commands Respond**: Všechna tlaèítka reagují
- [ ] **Data Binding Works**: UI se aktualizuje pøi zmìnách dat
- [ ] **Original Performance**: Performance stejnı jako pøed zmìnou

### **Memory/Performance Tests**
- [ ] **No Memory Leaks**: ádné pamìové leaky pøi pouívání
- [ ] **Performance Same/Better**: Performance stejnı nebo lepší ne pøed zmìnou
- [ ] **Clean Shutdown**: Plugin se cleanì vypne bez chyb
- [ ] **Helper Memory**: Helper utilities nezvyšují memory usage

## ?? **Stop Criteria (Immediate Rollback)**

Pokud nìkterı z následujících problémù se objeví, **okamitì rollback**:

### **Critical Failures**
- ? Build fails s chybami
- ? Plugin se nenaète do Playnite
- ? Crash pøi otevøení sidebar
- ? Audio playback nefunguje
- ? Game selection nefunguje

### **Regressions**
- ? Jakákoli funkènost pøestane fungovat
- ? Nové exception/chyby v logu
- ? UI responds pomalejší ne døíve
- ? Metadata loading fails
- ? Helper utilities zpùsobují problémy

## ?? **Test Results Log**

### **Step 2: Helper Utilities - VolumeHelper.cs + TimeHelper.cs**
```
Date: [FILL DATE]
Tester: [FILL NAME]
Branch: safe-refactor-step2-helpers

Build Tests:
[ ] Compilation Success: 
[ ] No New Warnings: 
[ ] All References Resolve: 
[ ] Helper Classes Compile: 

Plugin Load Tests:
[ ] Plugin Loads: 
[ ] Sidebar Appears: 
[ ] No Crash on Load: 
[ ] Helpers Not Used: 

Core Functionality:
[ ] Game Selection: 
[ ] Music Files Load: 
[ ] Audio Playback: 
[ ] Volume Control: 
[ ] Progress Tracking: 
[ ] Metadata Display: 

Helper Infrastructure:
[ ] VolumeHelper Available: 
[ ] TimeHelper Available: 
[ ] Static Methods Work: 
[ ] No Side Effects: 

Advanced Features:
[ ] Discogs Integration: 
[ ] Auto-Play: 
[ ] Cover Images: 
[ ] Settings Persist: 

UI Responsiveness:
[ ] No UI Freezing: 
[ ] Commands Respond: 
[ ] Data Binding: 
[ ] Original Performance: 

Memory/Performance:
[ ] No Memory Leaks: 
[ ] Performance: 
[ ] Clean Shutdown: 
[ ] Helper Memory: 

OVERALL RESULT: [ ] PASS / [ ] FAIL
ROLLBACK NEEDED: [ ] YES / [ ] NO

Notes:
[Add any observations or issues here]
```

## ?? **Rollback Procedure** (if needed)

```bash
# 1. Immediate return to working state
git checkout safe-refactor-step1-infrastructure

# 2. Verify functionality restored
[Run Step 1 test checklist again]

# 3. Document what went wrong
# Create issue in Documentation/Issues/

# 4. Revise approach
# Update SafeViewModelRefactoringPlan.md

# 5. Try smaller step next time
```

## ? **Success Criteria**

Pro úspìšné dokonèení Step 2:
- ? ALL test checklist items pass
- ? NO regressions introduced
- ? Helper utilities ready for future use
- ? Infrastructure continues to build successfully
- ? ZERO functional changes to existing code

## ?? **Files Added in Step 2**

### **New Helper Utilities**
1. `Utils/Helpers/VolumeHelper.cs` - Volume operations (formatting, validation, conversion)
2. `Utils/Helpers/TimeHelper.cs` - Time operations (formatting, parsing, calculations)

### **Key Features Available (Not Used Yet)**
- **VolumeHelper**: FormatPercentage(), ClampVolume(), PercentageToNormalized()
- **TimeHelper**: FormatTime(), ParseTimeToSeconds(), CalculateProgress()

---

**Next Step**: Pokud všechny testy projdou, proceed to Step 3: First Micro-Extraction
**If Any Fail**: Immediate rollback to Step 1 and problem analysis