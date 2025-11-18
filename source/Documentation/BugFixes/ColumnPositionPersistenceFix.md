# Oprava zapomenuté pozice sloupcù mezi TrackTitle a TrackDuration

## ?? **Popis problému**

Bìhem refactoringu byla již podruhé zapomenuta jedna vlastnost: plugin si pamatuje pouze pozici mezi sloupci TrackNumber a Track Title, ale zapomíná pozici mezi sloupci TrackTitle a TrackDuration.

**Problém:** 
- Sloupec TrackNumber (pixel-based) se ukládal správnì
- Pozice splitteru mezi TrackTitle a TrackDuration sloupci (star-sized) se neukládala
- Uživatelé ztráceli své nastavení pozice sloupcù pøi restartu

## ? **Implementované øešení**

### **1. Nová vlastnost v OstPlayerSettings.cs**

Pøidána nová vlastnost pro sledování pozice splitteru:

```csharp
/// <summary>
/// FIXED: New property to track column splitter position ratio between TrackTitle and TrackDuration
/// Stores the relative position of the splitter as a ratio (0.0 to 1.0)
/// This preserves the exact user positioning between the two star-sized columns
/// </summary>
[DefaultValue(0.75)]
public double TitleDurationSplitterRatio
{
    get => titleDurationSplitterRatio;
    set
    {
        titleDurationSplitterRatio = Math.Max(0.2, Math.Min(0.9, value)); // Clamp between 20% and 90%
        OnPropertyChanged();
    }
}
```

### **2. Vylepšený DataGridColumnPersistence.cs**

#### **A) Nový event handler pro column reordering:**
```csharp
/// <summary>
/// FIXED: New event handler for column reordering that was missing.
/// Ensures column position changes between TrackTitle and TrackDuration are captured.
/// </summary>
private void OnColumnReordered(object sender, DataGridColumnEventArgs e)
{
    // Schedule save when columns are reordered to preserve positions
    ScheduleSave();
    System.Diagnostics.Debug.WriteLine("FIXED: Column reordered - saving positions");
}
```

#### **B) Vylepšená metoda RestoreStarSizedColumnsWithPositions:**
```csharp
// FIXED: Use splitter ratio to calculate precise star values
// This preserves the exact splitter position between TrackTitle and Duration columns
var titleStarValue = Math.Max(1.0, savedSplitterRatio * 4.0); // Convert ratio to star value
var durationStarValue = Math.Max(0.5, (1.0 - savedSplitterRatio) * 4.0); // Inverse ratio

// FIXED: Apply star-sizing with calculated values that preserve exact positions
_dataGrid.Columns[1].Width = new DataGridLength(titleStarValue, DataGridLengthUnitType.Star);
_dataGrid.Columns[2].Width = new DataGridLength(durationStarValue, DataGridLengthUnitType.Star);
```

#### **C) Vylepšená metoda SaveStarSizedColumnRatiosWithPositions:**
```csharp
// FIXED: Calculate and save splitter ratio for precise position restoration
var totalStarWidth = titleActualWidth + durationActualWidth;
var splitterRatio = titleActualWidth / totalStarWidth;

// FIXED: Save splitter position
settings.TitleDurationSplitterRatio = clampedSplitterRatio;
```

### **3. Klíèová vylepšení**

1. **Sledování pozice splitteru**: Pøidán `TitleDurationSplitterRatio` (0.0-1.0)
2. **Lepší event handling**: Pøidán `ColumnReordered` event handler
3. **Pøesnìjší výpoèet star hodnot**: Použití splitter ratio pro zachování pozice
4. **Vylepšená validace**: Kontrola zmìn pøed uložením
5. **Lepší fallback logika**: Více úrovní fallback pøi neplatných datech

## ?? **Technické detaily**

### **Algoritmus ukládání pozice:**
1. **Zachycení zmìny**: Event handlery monitorují resize/reorder operace
2. **Výpoèet ratio**: `titleWidth / (titleWidth + durationWidth)`
3. **Validace a clamping**: Ratio mezi 0.2 a 0.9
4. **Uložení do JSON**: Persistent storage pøes Playnite SDK

### **Algoritmus obnovení pozice:**
1. **Naètení saved ratio**: Z plugin settings JSON
2. **Konverze na star hodnoty**: `ratio * 4.0` a `(1-ratio) * 4.0`
3. **Aplikace DataGridLength.Star**: WPF star-sizing s preserved pozicí
4. **Fallback strategie**: Nìkolik úrovní fallback pøi chybách

## ?? **Pøed a po implementaci**

### **Pøed opravou:**
```
TrackNumber: ? Ukládá se (pixel-based)
TrackTitle:  ? Ztrácí se pozice (star-sizing)
Duration:    ? Ztrácí se pozice (star-sizing)
```

### **Po opravì:**
```
TrackNumber: ? Ukládá se (pixel-based)
TrackTitle:  ? Ukládá se pozice (star-sizing + splitter ratio)
Duration:    ? Ukládá se pozice (star-sizing + splitter ratio)
```

## ?? **Testování**

### **Test scénáøe:**
1. **Resize TrackTitle sloupce** ? Pozice se uloží a obnoví
2. **Resize Duration sloupce** ? Pozice se uloží a obnoví
3. **Restart pluginu** ? Všechny pozice se obnoví
4. **Reorder sloupcù** ? Nové pozice se uloží
5. **Invalid data** ? Fallback na sensible defaults

### **Validace:**
- Splitter ratio 0.2-0.9 (20%-90%)
- Minimum widths: TrackTitle 250px, Duration 80px
- Star values minimum: Title 1.0*, Duration 0.5*

## ?? **Zmìnìné soubory**

1. **`OstPlayerSettings.cs`**
   - Pøidána `TitleDurationSplitterRatio` property
   - Pøidán private field `titleDurationSplitterRatio`
   - Zvýšený default pro `DurationColumnWidth` z 80 na 100

2. **`Utils/DataGridColumnPersistence.cs`**
   - Pøidán `ColumnReordered` event handler
   - Vylepšena `RestoreStarSizedColumnsWithPositions` metoda
   - Vylepšena `SaveStarSizedColumnRatiosWithPositions` metoda
   - Pøidáno splitter ratio tracking
   - Lepší debugging output
   - Enhanced validation a fallback logika

## ?? **Výsledek**

**Plugin nyní správnì ukládá a obnovuje pozice všech sloupcù DataGrid, vèetnì pozice splitteru mezi TrackTitle a TrackDuration sloupci. Uživatelé si zachovají svá nastavení i po restartu pluginu.**

**Status:** ? **OPRAVENO** - Column position persistence plnì funkèní  
**Version:** ?? **v2.1.0** (DataGridColumnPersistence, OstPlayerSettings)  
**Dopady:** ?? **Vysoké** - Výraznì zlepšuje uživatelský zážitek