# Fix Encoding and Question Mark Symbols in Markdown Files
# Author: AI Assistant
# Created: 2025-01-08
# Purpose: Batch conversion of all .md files to UTF-8 and replace question mark symbols

param(
    [string]$RootPath = ".",
    [switch]$WhatIf = $false
)

Write-Host "?? Starting Markdown File Encoding and Symbol Fix..." -ForegroundColor Green
Write-Host "Root Path: $RootPath" -ForegroundColor Cyan

# Define symbol replacements - simple string replacements first
$basicReplacements = @{
    "### ??? " = "### ??? "
    "### **???**" = "### **???**"
    "## ??? " = "## ??? "
    "## **???**" = "## **???**"
    "# ??? " = "# ??? "
    "- ??? " = "- ??? "
    "??? " = "??? "
    
    "### ?? " = "### ?? "
    "### **??**" = "### **??**"
    "## ?? " = "## ?? "
    "## **??**" = "## **??**"
    "# ?? " = "# ?? "
    "- ?? " = "- ?? "
    "?? " = "?? "
    
    "### ? " = "### ? "
    "## ? " = "## ? "
    "# ? " = "# ? "
    "- ? " = "- ? "
}

function Fix-FileEncoding {
    param(
        [string]$FilePath,
        [switch]$WhatIf
    )
    
    try {
        # Read file with UTF-8 encoding
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        
        # Check if file contains problematic characters
        $hasProblems = $content.Contains("??") -or $content.Contains("???") -or $content.Contains("?")
        
        if (-not $hasProblems) {
            Write-Host "  ? $($FilePath.Split('\')[-1]) - No issues found" -ForegroundColor Green
            return $true
        }
        
        Write-Host "  ?? Processing: $($FilePath.Split('\')[-1])" -ForegroundColor Yellow
        
        # Apply basic replacements
        foreach ($old in $basicReplacements.Keys) {
            $new = $basicReplacements[$old]
            $content = $content.Replace($old, $new)
        }
        
        # Clean up encoding issues - use string methods instead of char
        $content = $content.Replace("?", "")  # Remove replacement characters
        
        # Remove BOM if present (using string methods)
        if ($content.Length -gt 0 -and [int][char]$content[0] -eq 65279) {
            $content = $content.Substring(1)
        }
        
        # Convert line endings to Windows format
        $content = $content -replace "`r`n", "`n" -replace "`r", "`n" -replace "`n", "`r`n"
        
        if (-not $WhatIf) {
            # Save with UTF-8 encoding (with BOM for better compatibility)
            $utf8BOM = New-Object System.Text.UTF8Encoding($true)
            [System.IO.File]::WriteAllText($FilePath, $content, $utf8BOM)
            Write-Host "  ? Fixed: $($FilePath.Split('\')[-1])" -ForegroundColor Green
        } else {
            Write-Host "  ?? Would fix: $($FilePath.Split('\')[-1])" -ForegroundColor Cyan
        }
        
        return $true
    }
    catch {
        Write-Host "  ? Error processing $($FilePath.Split('\')[-1]) : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Get-EncodingInfo {
    param([string]$FilePath)
    
    try {
        $bytes = [System.IO.File]::ReadAllBytes($FilePath)
        
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            return "UTF-8 BOM"
        }
        elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
            return "UTF-16 LE"
        }
        elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
            return "UTF-16 BE"
        }
        else {
            # Check if it's ASCII compatible
            $isAscii = $true
            for ($i = 0; $i -lt [Math]::Min(1000, $bytes.Length); $i++) {
                if ($bytes[$i] -gt 127) {
                    $isAscii = $false
                    break
                }
            }
            
            if ($isAscii) {
                return "ASCII"
            } else {
                return "UTF-8 (no BOM)"
            }
        }
    }
    catch {
        return "Unknown"
    }
}

# Main execution
Write-Host ""
Write-Host "?? Scanning for Markdown files..." -ForegroundColor Cyan

# Exclude packages directory and other non-project files
$markdownFiles = Get-ChildItem -Path $RootPath -Recurse -Filter "*.md" | Where-Object { 
    $_.FullName -notlike "*\packages\*" -and 
    $_.FullName -notlike "*\node_modules\*" -and
    $_.FullName -notlike "*\.git\*"
} | Sort-Object FullName

Write-Host "Found $($markdownFiles.Count) Markdown files" -ForegroundColor Cyan
Write-Host ""

$processedCount = 0
$successCount = 0
$problemFiles = @()

foreach ($file in $markdownFiles) {
    $processedCount++
    $encoding = Get-EncodingInfo -FilePath $file.FullName
    
    Write-Host "[$processedCount/$($markdownFiles.Count)] " -NoNewline -ForegroundColor Gray
    Write-Host "$($file.Name) " -NoNewline -ForegroundColor White
    Write-Host "($encoding)" -ForegroundColor DarkGray
    
    $success = Fix-FileEncoding -FilePath $file.FullName -WhatIf:$WhatIf
    
    if ($success) {
        $successCount++
    } else {
        $problemFiles += $file.FullName
    }
}

# Summary
Write-Host ""
Write-Host "?? **SUMMARY**" -ForegroundColor Green
Write-Host "Processed: $processedCount files" -ForegroundColor Cyan
Write-Host "Successful: $successCount files" -ForegroundColor Green

if ($problemFiles.Count -gt 0) {
    Write-Host "Problems: $($problemFiles.Count) files" -ForegroundColor Red
    Write-Host "Problem files:" -ForegroundColor Red
    foreach ($file in $problemFiles) {
        Write-Host "  - $($file.Split('\')[-1])" -ForegroundColor Red
    }
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "??  This was a DRY RUN. Use -WhatIf:`$false to apply changes." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "? All changes applied!" -ForegroundColor Green
}

Write-Host ""
Write-Host "?? Encoding and symbol fix completed!" -ForegroundColor Green