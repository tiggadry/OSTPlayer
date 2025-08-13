# Fix Incorrect Dates in Project - Comprehensive Solution
# Author: AI Assistant
# Created: 2025-08-07
# Purpose: Fix all instances of incorrect date "2025-01-08" and implement long-term prevention

param(
    [string]$RootPath = ".",
    [switch]$WhatIf = $false,
    [string]$CorrectDate = "2025-08-07"
)

Write-Host "?? Starting comprehensive date fix operation..." -ForegroundColor Green
Write-Host "Root Path: $RootPath" -ForegroundColor Cyan
Write-Host "Correct Date: $CorrectDate" -ForegroundColor Cyan

# Define incorrect dates to fix
$incorrectDates = @(
    "2025-01-08",  # Primary problematic date
    "2025-01-07",
    "2025-01-09",
    "2025-01-06",
    "2025-01-05"
)

# Track changes
$filesModified = 0
$datesFixed = 0
$problemFiles = @()

function Fix-DateInFile {
    param(
        [string]$FilePath,
        [string[]]$IncorrectDates,
        [string]$CorrectDate,
        [switch]$WhatIf
    )
    
    try {
        if (-not (Test-Path $FilePath)) {
            return $false
        }
        
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        $originalContent = $content
        $changesInFile = 0
        
        # Replace each incorrect date
        foreach ($incorrectDate in $IncorrectDates) {
            if ($content.Contains($incorrectDate)) {
                $beforeCount = ($content | Select-String -Pattern $incorrectDate -AllMatches).Matches.Count
                $content = $content -replace [regex]::Escape($incorrectDate), $CorrectDate
                $afterCount = ($content | Select-String -Pattern $incorrectDate -AllMatches).Matches.Count
                $changesInFile += ($beforeCount - $afterCount)
                
                if ($beforeCount -gt 0) {
                    Write-Host "  ?? Found $beforeCount instances of '$incorrectDate' in $($FilePath.Split('\')[-1])" -ForegroundColor Yellow
                }
            }
        }
        
        if ($content -ne $originalContent) {
            if (-not $WhatIf) {
                $utf8BOM = New-Object System.Text.UTF8Encoding($true)
                [System.IO.File]::WriteAllText($FilePath, $content, $utf8BOM)
                Write-Host "  ? Fixed $changesInFile dates in $($FilePath.Split('\')[-1])" -ForegroundColor Green
            } else {
                Write-Host "  ?? Would fix $changesInFile dates in $($FilePath.Split('\')[-1])" -ForegroundColor Cyan
            }
            
            return @{
                Modified = $true
                ChangesCount = $changesInFile
            }
        }
        
        return @{
            Modified = $false
            ChangesCount = 0
        }
    }
    catch {
        Write-Host "  ? Error processing $($FilePath.Split('\')[-1]): $($_.Exception.Message)" -ForegroundColor Red
        return @{
            Modified = $false
            ChangesCount = 0
            Error = $_.Exception.Message
        }
    }
}

# Main execution
Write-Host ""
Write-Host "?? Scanning for files with incorrect dates..." -ForegroundColor Cyan

# Get all relevant files
$filesToProcess = @()
$filesToProcess += Get-ChildItem -Path $RootPath -Recurse -Filter "*.md" | Where-Object { 
    $_.FullName -notlike "*\packages\*" -and 
    $_.FullName -notlike "*\node_modules\*" -and
    $_.FullName -notlike "*\.git\*"
}
$filesToProcess += Get-ChildItem -Path $RootPath -Recurse -Filter "*.cs" | Where-Object { 
    $_.FullName -notlike "*\bin\*" -and 
    $_.FullName -notlike "*\obj\*" -and
    $_.FullName -notlike "*\packages\*"
}

Write-Host "Found $($filesToProcess.Count) files to check" -ForegroundColor Cyan
Write-Host ""

$processedCount = 0

foreach ($file in $filesToProcess) {
    $processedCount++
    Write-Host "[$processedCount/$($filesToProcess.Count)] " -NoNewline -ForegroundColor Gray
    Write-Host "$($file.Name)" -ForegroundColor White
    
    $result = Fix-DateInFile -FilePath $file.FullName -IncorrectDates $incorrectDates -CorrectDate $CorrectDate -WhatIf:$WhatIf
    
    if ($result.Modified) {
        $filesModified++
        $datesFixed += $result.ChangesCount
    }
    
    if ($result.Error) {
        $problemFiles += @{
            File = $file.FullName
            Error = $result.Error
        }
    }
}

# Summary
Write-Host ""
Write-Host "?? **DATE FIX SUMMARY**" -ForegroundColor Green
Write-Host "Files processed: $processedCount" -ForegroundColor Cyan
Write-Host "Files modified: $filesModified" -ForegroundColor Green
Write-Host "Total dates fixed: $datesFixed" -ForegroundColor Yellow

if ($problemFiles.Count -gt 0) {
    Write-Host "Problem files: $($problemFiles.Count)" -ForegroundColor Red
    foreach ($problem in $problemFiles) {
        Write-Host "  - $($problem.File.Split('\')[-1]): $($problem.Error)" -ForegroundColor Red
    }
}

# Show specific fixes made
if ($datesFixed -gt 0) {
    Write-Host ""
    Write-Host "?? **SPECIFIC FIXES APPLIED:**" -ForegroundColor Green
    foreach ($incorrectDate in $incorrectDates) {
        Write-Host "  '$incorrectDate' ? '$CorrectDate'" -ForegroundColor Yellow
    }
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "??  This was a DRY RUN. Use -WhatIf:`$false to apply fixes." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "? All date fixes applied!" -ForegroundColor Green
}

# Additional validation
if (-not $WhatIf -and $datesFixed -gt 0) {
    Write-Host ""
    Write-Host "?? **POST-FIX VALIDATION:**" -ForegroundColor Cyan
    
    # Quick scan to verify no blacklisted dates remain
    $remainingProblems = 0
    foreach ($file in $filesToProcess) {
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8 -ErrorAction SilentlyContinue
        if ($content) {
            foreach ($incorrectDate in $incorrectDates) {
                if ($content.Contains($incorrectDate)) {
                    $remainingProblems++
                    Write-Host "  ??  Still found '$incorrectDate' in $($file.Name)" -ForegroundColor Yellow
                }
            }
        }
    }
    
    if ($remainingProblems -eq 0) {
        Write-Host "  ? All incorrect dates successfully removed!" -ForegroundColor Green
    } else {
        Write-Host "  ??  $remainingProblems remaining issues found" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "?? Incorrect date fix operation completed!" -ForegroundColor Green

# Provide recommendations
Write-Host ""
Write-Host "?? **RECOMMENDATIONS:**" -ForegroundColor Cyan
Write-Host "1. ? Use DateValidationService for future date operations" -ForegroundColor Green
Write-Host "2. ? AI assistants should use Get-Date -Format 'yyyy-MM-dd'" -ForegroundColor Green
Write-Host "3. ? Never copy dates from existing documentation" -ForegroundColor Red
Write-Host "4. ?? Run periodic scans with DateValidationService.ScanProject()" -ForegroundColor Yellow