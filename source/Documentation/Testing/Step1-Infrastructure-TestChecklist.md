# Testovací checklist - Safe Refactoring Step 1: Infrastructure

## 🧪 **Mandatory Testing Protocol**

### **Build Tests**
- [ ] ✅ **Compilation Success**: Projekt se buildu úspěšně bez chyb
- [ ] ✅ **No New Warnings**: Žádná nová warnings nebyla přidána
- [ ] ✅ **All References Resolve**: Všechny reference se resolved správně

### **Plugin Load Tests**
- [ ] **Plugin Loads in Playnite**: Plugin se načte bez chyb
- [ ] **Sidebar Appears**: OstPlayer sidebar se zobrazí v Playnite
- [ ] **No Crash on Load**: Žádný crash při načítání pluginu

### **Core Functionality Tests**
- [ ] **Game Selection Works**: Lze vybrat hru z dropdown
- [ ] **Music Files Load**: Muzika files se načtou pro vybranou hru
- [ ] **Audio Playback Works**: Play/Pause/Stop funguje správně
- [ ] **Volume Control Works**: Volume slider mění hlasitost
- [ ] **Progress Tracking Works**: Progress bar se aktualizuje během přehrávání
- [ ] **Metadata Display Works**: MP3 metadata se zobrazují správně

### **Advanced Feature Tests**
- [ ] **Discogs Integration Works**: Discogs metadata loading funguje
- [ ] **Auto-Play Works**: Auto-play next track funguje
- [ ] **Cover Images Work**: Cover images se zobrazují
- [ ] **Settings Persist**: Nastavení se ukládají a obnovují

### **UI Responsiveness Tests**
- [ ] **No UI Freezing**: UI nezmrzne během operací
- [ ] **Commands Respond**: Všechna tlačítka reagují
- [ ] **Data Binding Works**: UI se aktualizuje při změnách dat

### **Memory/Performance Tests**
- [ ] **No Memory Leaks**: Žádné paměťové leaky při používání
- [ ] **Performance Same/Better**: Performance stejný nebo lepší než před změnou
- [ ] **Clean Shutdown**: Plugin se cleaně vypne bez chyb

## 🚨 **Stop Criteria (Immediate Rollback)**

Pokud některý z následujících problémů se objeví, **okamžitě rollback**:

### **Critical Failures**
- ❌ Build fails s chybami
- ❌ Plugin se nenačte do Playnite
- ❌ Crash při otevření sidebar
- ❌ Audio playback nefunguje
- ❌ Game selection nefunguje

### **Regressions**
- ❌ Jakákoli funkčnost přestane fungovat
- ❌ Nové exception/chyby v logu
- ❌ UI responds pomalejší než dříve
- ❌ Metadata loading fails

## 📋 **Test Results Log**

### **Step 1: Infrastructure - ViewModelBase.cs**
```
Date: 2025-01-20
Tester: User + AI Assistant
Branch: safe-refactor-step1-infrastructure

Build Tests:
[✅] Compilation Success: ✅ PASS - Project builds without errors
[✅] No New Warnings: ✅ PASS - No additional warnings introduced  
[✅] All References Resolve: ✅ PASS - ViewModelBase compiles correctly

Plugin Load Tests:
[✅] Plugin Loads: ✅ PASS - Plugin loads in Playnite without issues
[✅] Sidebar Appears: ✅ PASS - OstPlayer sidebar displays correctly
[✅] No Crash on Load: ✅ PASS - No crashes during plugin loading

Core Functionality:
[✅] Game Selection: ✅ PASS - Game dropdown works correctly
[✅] Music Files Load: ✅ PASS - Music files load for selected games
[✅] Audio Playback: ✅ PASS - Play/Pause/Stop functions normally
[✅] Volume Control: ✅ PASS - Volume slider changes audio level
[✅] Progress Tracking: ✅ PASS - Progress bar updates during playback
[✅] Metadata Display: ✅ PASS - MP3 metadata displays correctly

Advanced Features:
[✅] Discogs Integration: ✅ PASS - Discogs metadata loading works
[✅] Auto-Play: ✅ PASS - Auto-play next track functions
[✅] Cover Images: ✅ PASS - Cover images display properly
[✅] Settings Persist: ✅ PASS - Settings save and restore correctly

UI Responsiveness:
[✅] No UI Freezing: ✅ PASS - UI remains responsive during operations
[✅] Commands Respond: ✅ PASS - All buttons and commands work
[✅] Data Binding: ✅ PASS - UI updates correctly with data changes

Memory/Performance:
[✅] No Memory Leaks: ✅ PASS - No observable memory leaks
[✅] Performance: ✅ PASS - Performance equivalent to before changes
[✅] Clean Shutdown: ✅ PASS - Plugin shuts down cleanly

OVERALL RESULT: [✅] PASS
ROLLBACK NEEDED: [❌] NO

Notes:
- ViewModelBase infrastructure successfully added
- Zero impact on existing functionality confirmed
- All original features working as expected
- Ready to proceed with Step 2: Helper Utilities
```

## 🔄 **Rollback Procedure** (if needed)

```bash
# 1. Immediate return to working state
git checkout backup-working-viewmodel

# 2. Verify functionality restored
[Run test checklist again]

# 3. Document what went wrong
# Create issue in Documentation/Issues/

# 4. Revise approach
# Update SafeViewModelRefactoringPlan.md

# 5. Try smaller step next time
```

## ✅ **Success Criteria**

Pro úspěšné dokončení Step 1:
- ✅ ALL test checklist items pass
- ✅ NO regressions introduced
- ✅ ViewModelBase infrastructure ready for use
- ✅ Confidence in methodology increased

---

**Next Step**: Pokud všechny testy projdou, proceed to Step 2: Helper Utilities
**If Any Fail**: Immediate rollback and problem analysis