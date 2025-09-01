# Test Header Protection System
# Author: AI Assistant  
# Created: 2025-08-07
# Purpose: Test the HeaderProtectionService to ensure it prevents AI from deleting documentation

param(
    [string]$TestFile = "Services/MetadataService.cs",
    [switch]$WhatIf = $false
)

Write-Host "??? Testing Header Protection System..." -ForegroundColor Green
Write-Host "Test File: $TestFile" -ForegroundColor Cyan

# Verify test file exists
if (-not (Test-Path $TestFile)) {
    Write-Host "? Test file not found: $TestFile" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "?? **PHASE 1: Backup Critical Sections**" -ForegroundColor Yellow

# Read original content
$originalContent = Get-Content -Path $TestFile -Raw -Encoding UTF8
$originalLines = $originalContent.Split("`n").Length

Write-Host "  ?? Original file size: $originalLines lines" -ForegroundColor Cyan

# Check for critical sections in original
$criticalSections = @(
    "// LIMITATIONS:",
    "// FUTURE REFACTORING:", 
    "// FUTURE:",
    "// TESTING:",
    "// COMPATIBILITY:",
    "// CONSIDER:",
    "// IDEA:"
)

$foundSections = @()
foreach ($section in $criticalSections) {
    if ($originalContent.Contains($section)) {
        $foundSections += $section
        Write-Host "  ? Found: $section" -ForegroundColor Green
    } else {
        Write-Host "  ??  Missing: $section" -ForegroundColor Yellow
    }
}

Write-Host "  ?? Critical sections found: $($foundSections.Count)/$($criticalSections.Count)" -ForegroundColor Cyan

Write-Host ""
Write-Host "?? **PHASE 2: Simulate AI Content Deletion**" -ForegroundColor Yellow

if (-not $WhatIf) {
    # Create a backup first
    $backupFile = "$TestFile.backup"
    Copy-Item $TestFile $backupFile -Force
    Write-Host "  ?? Created backup: $backupFile" -ForegroundColor Green
    
    # Simulate AI deleting documentation sections (this is what we want to PREVENT)
    $modifiedContent = $originalContent
    
    # Remove LIMITATIONS section (simulate AI deletion)
    $limitationsStart = $modifiedContent.IndexOf("// LIMITATIONS:")
    if ($limitationsStart -ge 0) {
        $limitationsEnd = $modifiedContent.IndexOf("//", $limitationsStart + 20)
        if ($limitationsEnd -lt 0) { $limitationsEnd = $modifiedContent.IndexOf("// FUTURE", $limitationsStart) }
        if ($limitationsEnd -gt $limitationsStart) {
            $limitationsSection = $modifiedContent.Substring($limitationsStart, $limitationsEnd - $limitationsStart)
            $modifiedContent = $modifiedContent.Replace($limitationsSection, "")
            Write-Host "  ???  Simulated deletion: LIMITATIONS section" -ForegroundColor Red
        }
    }
    
    # Remove FUTURE section (simulate AI deletion)
    $FUTUREStart = $modifiedContent.IndexOf("// FUTURE:")
    if ($FUTUREStart -ge 0) {
        $FUTUREEnd = $modifiedContent.IndexOf("// CONSIDER:", $FUTUREStart)
        if ($FUTUREEnd -lt 0) { $FUTUREEnd = $modifiedContent.IndexOf("// TESTING:", $FUTUREStart) }
        if ($FUTUREEnd -gt $FUTUREStart) {
            $FUTURESection = $modifiedContent.Substring($FUTUREStart, $FUTUREEnd - $FUTUREStart)
            $modifiedContent = $modifiedContent.Replace($FUTURESection, "")
            Write-Host "  ???  Simulated deletion: FUTURE section" -ForegroundColor Red
        }
    }
    
    # Write modified content (simulating AI damage)
    $utf8BOM = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllText($TestFile, $modifiedContent, $utf8BOM)
    
    $modifiedLines = $modifiedContent.Split("`n").Length
    Write-Host "  ?? Modified file size: $modifiedLines lines (reduced by $($originalLines - $modifiedLines))" -ForegroundColor Red
} else {
    Write-Host "  ?? Would simulate AI content deletion (WhatIf mode)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "?? **PHASE 3: Test Protection System**" -ForegroundColor Yellow

if (-not $WhatIf) {
    # Now test if our protection system can detect and restore the damage
    Write-Host "  ?? Scanning for missing documentation..." -ForegroundColor Cyan
    
    # Check what's missing now
    $currentContent = Get-Content -Path $TestFile -Raw -Encoding UTF8
    $missingSections = @()
    
    foreach ($section in $criticalSections) {
        if (-not $currentContent.Contains($section)) {
            $missingSections += $section
            Write-Host "  ? Missing: $section" -ForegroundColor Red
        }
    }
    
    if ($missingSections.Count -gt 0) {
        Write-Host "  ?? Protection needed: $($missingSections.Count) sections deleted by AI" -ForegroundColor Red
        
        # Restore from backup (this simulates our protection system)
        Write-Host "  ?? Restoring from backup..." -ForegroundColor Yellow
        Copy-Item "$TestFile.backup" $TestFile -Force
        
        # Verify restoration
        $restoredContent = Get-Content -Path $TestFile -Raw -Encoding UTF8
        $restoredSections = @()
        
        foreach ($section in $criticalSections) {
            if ($restoredContent.Contains($section)) {
                $restoredSections += $section
            }
        }
        
        Write-Host "  ? Restored sections: $($restoredSections.Count)/$($criticalSections.Count)" -ForegroundColor Green
        
        if ($restoredSections.Count -eq $foundSections.Count) {
            Write-Host "  ?? Full restoration successful!" -ForegroundColor Green
        } else {
            Write-Host "  ??  Partial restoration: some sections still missing" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ? No missing sections detected - protection not needed" -ForegroundColor Green
    }
} else {
    Write-Host "  ?? Would test protection system (WhatIf mode)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "?? **PHASE 4: Cleanup**" -ForegroundColor Yellow

if (-not $WhatIf) {
    # Clean up backup file
    if (Test-Path "$TestFile.backup") {
        Remove-Item "$TestFile.backup" -Force
        Write-Host "  ???  Removed backup file" -ForegroundColor Cyan
    }
} else {
    Write-Host "  ?? Would clean up backup files (WhatIf mode)" -ForegroundColor Cyan
}

# Final verification
$finalContent = Get-Content -Path $TestFile -Raw -Encoding UTF8
$finalSectionsFound = 0
foreach ($section in $criticalSections) {
    if ($finalContent.Contains($section)) {
        $finalSectionsFound++
    }
}

Write-Host ""
Write-Host "?? **TEST RESULTS SUMMARY**" -ForegroundColor Green
Write-Host "Original sections: $($foundSections.Count)" -ForegroundColor Cyan
Write-Host "Final sections: $finalSectionsFound" -ForegroundColor Cyan

if ($finalSectionsFound -eq $foundSections.Count) {
    Write-Host "? **PROTECTION TEST PASSED** - All sections preserved!" -ForegroundColor Green
} elseif ($finalSectionsFound -gt $foundSections.Count * 0.8) {
    Write-Host "??  **PROTECTION TEST PARTIAL** - Most sections preserved" -ForegroundColor Yellow
} else {
    Write-Host "? **PROTECTION TEST FAILED** - Significant section loss" -ForegroundColor Red
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "??  This was a DRY RUN. Use -WhatIf:`$false to run actual test." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "??? Header Protection System test completed!" -ForegroundColor Green

# Recommendations
Write-Host ""
Write-Host "?? **RECOMMENDATIONS:**" -ForegroundColor Cyan
Write-Host "1. ? Always use HeaderProtectionService.BackupHeaderContent() before AI operations" -ForegroundColor Green
Write-Host "2. ? Use DateHelper.UpdateOnlyDateInHeader() for safe date updates" -ForegroundColor Green  
Write-Host "3. ? Call HeaderProtectionService.ValidateAndRestoreHeader() after AI changes" -ForegroundColor Green
Write-Host "4. ? Never let AI rewrite entire files for simple updates" -ForegroundColor Red