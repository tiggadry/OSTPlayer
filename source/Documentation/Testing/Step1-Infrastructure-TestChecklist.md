# Testovací checklist - Safe Refactoring Step 1: Infrastructure

## ?? **Mandatory Testing Protocol**

### **Build Tests**
- [ ] ? **Compilation Success**: Projekt se buildu úspìšnì bez chyb
- [ ] ? **No New Warnings**: ádná nová warnings nebyla pøidána
- [ ] ? **All References Resolve**: Všechny reference se resolved správnì

### **Plugin Load Tests**
- [ ] **Plugin Loads in Playnite**: Plugin se naète bez chyb
- [ ] **Sidebar Appears**: OstPlayer sidebar se zobrazí v Playnite
- [ ] **No Crash on Load**: ádnı crash pøi naèítání pluginu

### **Core Functionality Tests**
- [ ] **Game Selection Works**: Lze vybrat hru z dropdown
- [ ] **Music Files Load**: Muzika files se naètou pro vybranou hru
- [ ] **Audio Playback Works**: Play/Pause/Stop funguje správnì
- [ ] **Volume Control Works**: Volume slider mìní hlasitost
- [ ] **Progress Tracking Works**: Progress bar se aktualizuje bìhem pøehrávání
- [ ] **Metadata Display Works**: MP3 metadata se zobrazují správnì

### **Advanced Feature Tests**
- [ ] **Discogs Integration Works**: Discogs metadata loading funguje
- [ ] **Auto-Play Works**: Auto-play next track funguje
- [ ] **Cover Images Work**: Cover images se zobrazují
- [ ] **Settings Persist**: Nastavení se ukládají a obnovují

### **UI Responsiveness Tests**
- [ ] **No UI Freezing**: UI nezmrzne bìhem operací
- [ ] **Commands Respond**: Všechna tlaèítka reagují
- [ ] **Data Binding Works**: UI se aktualizuje pøi zmìnách dat

### **Memory/Performance Tests**
- [ ] **No Memory Leaks**: ádné pamìové leaky pøi pouívání
- [ ] **Performance Same/Better**: Performance stejnı nebo lepší ne pøed zmìnou
- [ ] **Clean Shutdown**: Plugin se cleanì vypne bez chyb

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

## ?? **Test Results Log**

### **Step 1: Infrastructure - ViewModelBase.cs**
```
Date: [FILL DATE]
Tester: [FILL NAME]
Branch: safe-refactor-step1-infrastructure

Build Tests:
[ ] Compilation Success: 
[ ] No New Warnings: 
[ ] All References Resolve: 

Plugin Load Tests:
[ ] Plugin Loads: 
[ ] Sidebar Appears: 
[ ] No Crash on Load: 

Core Functionality:
[ ] Game Selection: 
[ ] Music Files Load: 
[ ] Audio Playback: 
[ ] Volume Control: 
[ ] Progress Tracking: 
[ ] Metadata Display: 

Advanced Features:
[ ] Discogs Integration: 
[ ] Auto-Play: 
[ ] Cover Images: 
[ ] Settings Persist: 

UI Responsiveness:
[ ] No UI Freezing: 
[ ] Commands Respond: 
[ ] Data Binding: 

Memory/Performance:
[ ] No Memory Leaks: 
[ ] Performance: 
[ ] Clean Shutdown: 

OVERALL RESULT: [ ] PASS / [ ] FAIL
ROLLBACK NEEDED: [ ] YES / [ ] NO

Notes:
[Add any observations or issues here]
```

## ?? **Rollback Procedure** (if needed)

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

## ? **Success Criteria**

Pro úspìšné dokonèení Step 1:
- ? ALL test checklist items pass
- ? NO regressions introduced
- ? ViewModelBase infrastructure ready for use
- ? Confidence in methodology increased

---

**Next Step**: Pokud všechny testy projdou, proceed to Step 2: Helper Utilities
**If Any Fail**: Immediate rollback and problem analysis