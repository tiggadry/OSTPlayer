# Oprava zastavení pøehrávání pøi odchodu z OstPlayer

## ?? **Popis problému**

Pøed refaktoringem se vždy pøi odchodu z OstPlayer zastavila jedoucí skladba (pokud nìjaká jela), teï se tak nedìje a já to chci opravit. Dùvod je že když se skladba nezastaví a já znova zapnu OstPlayer a zapnu další skladbu, tak hrají dvì skladby zároveò a tak dále.

**Problém:** 
- Pøed refaktoringem se skladba automaticky zastavovala pøi zavøení sidebar
- Po refaktoringu se skladba nezastavuje pøi odchodu z pluginu
- To zpùsobuje pøehrávání více skladeb souèasnì
- Uživatel musí ruènì zastavit pøehrávání pøed odchodem

## ? **Implementované øešení**

### **1. Oprava v OstPlayerSidebarView.xaml.cs**

Pøidáno zastavení pøehrávání do `OnUnloaded` metody:

```csharp
/// <summary>
/// Performs cleanup when the control is being disposed.
/// Ensures column width persistence is properly saved and disposed.
/// FIXED: Added music playback stop during cleanup to prevent multiple tracks playing.
/// </summary>
private void OnUnloaded(object sender, RoutedEventArgs e)
{
    try
    {
        // FIXED: Stop music playback when the sidebar is being unloaded
        // This prevents multiple tracks playing when user leaves and re-enters OstPlayer
        if (viewModel != null)
        {
            try
            {
                viewModel.StopCommand?.Execute(null);
                System.Diagnostics.Debug.WriteLine("FIXED: Stopped music playback during sidebar cleanup");
            }
            catch (Exception musicEx)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to stop music during cleanup: {musicEx.Message}");
                // Continue with other cleanup even if music stop fails
            }
        }
        
        // ... existing cleanup code ...
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
    }
}
```

### **2. Vylepšení v OstPlayer.cs**

Posílena `CleanupSidebarView` metoda s fallback mechanismem:

```csharp
/// <summary>
/// Cleans up sidebar view resources with dependency injection support.
/// FIXED: Enhanced music stop with fallback to direct view method.
/// </summary>
private void CleanupSidebarView()
{
    try
    {
        // Stop music playback when the sidebar panel is closed
        if (lastSidebarView != null)
        {
            try
            {
                // FIXED: Try DI-based audio service first with timeout
                if (audioService != null)
                {
                    var stopTask = audioService.StopAsync();
                    if (!stopTask.Wait(1000)) // 1 second timeout
                    {
                        logger.Warn("Audio service stop operation timed out");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("FIXED: Stopped music using audioService during sidebar cleanup");
                    }
                }
                
                // FIXED: Fallback to direct view StopMusic method for reliability
                // This ensures music stops even if audioService fails or times out
                try
                {
                    lastSidebarView.StopMusic();
                    System.Diagnostics.Debug.WriteLine("FIXED: Stopped music using lastSidebarView.StopMusic() as fallback");
                }
                catch (Exception fallbackEx)
                {
                    logger.Warn(fallbackEx, "Fallback StopMusic() also failed during cleanup");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to stop music during sidebar cleanup using DI");
                
                // FIXED: Final fallback - try direct StopMusic even if DI completely fails
                try
                {
                    lastSidebarView.StopMusic();
                    System.Diagnostics.Debug.WriteLine("FIXED: Used final fallback StopMusic() after DI failure");
                }
                catch (Exception finalEx)
                {
                    logger.Error(finalEx, "All music stop methods failed during cleanup");
                }
            }
            finally
            {
                lastSidebarView = null;
            }
        }
        
        // ... existing PlayniteSound cleanup ...
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error during DI-enhanced sidebar cleanup");
    }
}
```

### **3. Klíèová vylepšení**

1. **Dvojí zastavení**: Zastavení v sidebar view + hlavním plugin cleanup
2. **Fallback mechanismus**: audioService ? StopMusic() ? final fallback
3. **Timeout protection**: 1 sekunda timeout pro audioService
4. **Error handling**: Graceful handling s pokraèováním cleanup
5. **Debug logging**: Sledování úspìšnosti zastavení

## ?? **Technické detaily**

### **Algoritmus zastavení:**
1. **OnUnloaded trigger**: WPF Unloaded event aktivuje cleanup
2. **ViewModel StopCommand**: Primární zastavení pøes MVVM command
3. **Plugin CleanupSidebarView**: Sekundární zastavení pøi zavøení sidebar
4. **Fallback metody**: Více úrovní fallback pro spolehlivost

### **Bezpeènostní opatøení:**
1. **Null checks**: Kontrola viewModel a audioService existence
2. **Exception handling**: Try-catch pro každou metodu zastavení
3. **Continue on failure**: Cleanup pokraèuje i pøi selhání zastavení
4. **Timeout protection**: Timeout pro asynchronní operace

## ?? **Pøed a po implementaci**

### **Pøed opravou:**
```
Odchod z OstPlayer:
???????????????????????????
? ? Skladba hraje dál    ?
? ? Multiple playback    ?
? ? Musí ruèní zastavení ?
???????????????????????????
```

### **Po opravì:**
```
Odchod z OstPlayer:
???????????????????????????
? ? Skladba se zastaví   ?
? ? Clean shutdown       ?
? ? No multiple tracks   ?
? ? Fallback protection  ?
???????????????????????????
```

## ?? **Testování**

### **Test scénáøe:**
1. **Pøehrávání + odchod** ? Skladba se zastaví automaticky
2. **Zpìt do OstPlayer** ? Nová skladba hraje bez konfliktu
3. **Selhání audioService** ? Fallback StopMusic() funguje
4. **Selhání všech metod** ? Cleanup pokraèuje bezpeènì
5. **Rychlý in/out** ? Timeout protection funguje

### **Validace:**
- Zastavení v OnUnloaded event handler
- Zastavení v CleanupSidebarView metoda
- Fallback na pøímé StopMusic() volání
- Error handling neblokuje cleanup
- Debug logging pro monitoring

## ?? **Zmìnìné soubory**

1. **`Views/OstPlayerSidebarView.xaml.cs`**
   - Pøidáno volání `viewModel.StopCommand?.Execute(null)` do `OnUnloaded`
   - Pøidán try-catch pro graceful error handling
   - Debug logging pro sledování úspìšnosti

2. **`OstPlayer.cs`**
   - Vylepšena `CleanupSidebarView` metoda
   - Pøidán fallback mechanismus s `lastSidebarView.StopMusic()`
   - Timeout protection pro audioService operations
   - Enhanced error handling a debug logging

## ?? **Výsledek**

**Plugin nyní správnì zastavuje pøehrávání pøi odchodu z OstPlayer, èímž se pøedchází pøehrávání více skladeb souèasnì. Implementován robustní fallback mechanismus pro maximální spolehlivost.**

**Workflow:**
1. **Uživatel odchází** z OstPlayer sidebar ? `OnUnloaded` triggered
2. **Sidebar cleanup** ? `viewModel.StopCommand.Execute()` 
3. **Plugin cleanup** ? `CleanupSidebarView()` with fallbacks
4. **Výsledek** ? Zaruèené zastavení pøehrávání

**Status:** ? **OPRAVENO** - Music stop on exit plnì funkèní  
**Version:** ?? **v1.2.2** (OstPlayerSidebarView, OstPlayer)  
**Dopady:** ?? **Vysoké** - Eliminuje konflikt multiple playback