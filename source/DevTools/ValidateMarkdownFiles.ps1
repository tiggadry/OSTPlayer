# Validate Markdown Files Encoding and Symbols
# Author: AI Assistant
# Created: 2025-01-08
# Purpose: Validation script for checking encoding consistency and symbol issues

param(
    [string]$RootPath = ".",
    [switch]$Detailed = $false
)

Write-Host "?? Starting Markdown File Validation..." -ForegroundColor Green
Write-Host "Root Path: $RootPath" -ForegroundColor Cyan

# Define problematic patterns to check for
$problematicPatterns = @(
    "??",
    "???", 
    "?",           # Replacement character
    [char]0xFEFF   # BOM character
)

# Define expected encoding
$expectedEncoding = "UTF-8"

function Test-FileEncoding {
    param([string]$FilePath)
    
    try {
        $bytes = [System.IO.File]::ReadAllBytes($FilePath)
        $result = @{
            FilePath = $FilePath
            FileName = $FilePath.Split('\')[-1]
            Encoding = "Unknown"
            HasBOM = $false
            IsUTF8 = $false
            Issues = @()
        }
        
        # Check encoding
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            $result.Encoding = "UTF-8 BOM"
            $result.HasBOM = $true
            $result.IsUTF8 = $true
        }
        elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
            $result.Encoding = "UTF-16 LE"
            $result.Issues += "Non-UTF8 encoding detected"
        }
        elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
            $result.Encoding = "UTF-16 BE"
            $result.Issues += "Non-UTF8 encoding detected"
        }
        else {
            # Check if it's ASCII compatible or UTF-8 without BOM
            $isAscii = $true
            for ($i = 0; $i -lt [Math]::Min(1000, $bytes.Length); $i++) {
                if ($bytes[$i] -gt 127) {
                    $isAscii = $false
                    break
                }
            }
            
            if ($isAscii) {
                $result.Encoding = "ASCII"
                $result.IsUTF8 = $true  # ASCII is compatible with UTF-8
            } else {
                $result.Encoding = "UTF-8 (no BOM)"
                $result.IsUTF8 = $true
            }
        }
        
        return $result
    }
    catch {
        return @{
            FilePath = $FilePath
            FileName = $FilePath.Split('\')[-1]
            Encoding = "Error"
            HasBOM = $false
            IsUTF8 = $false
            Issues = @("Failed to read file: $($_.Exception.Message)")
        }
    }
}

function Test-FileContent {
    param(
        [string]$FilePath,
        [hashtable]$EncodingResult
    )
    
    try {
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        
        # Check for problematic patterns
        foreach ($pattern in $problematicPatterns) {
            if ($content.Contains($pattern)) {
                $EncodingResult.Issues += "Contains problematic pattern: '$pattern'"
            }
        }
        
        # Check for line ending consistency
        $hasWindows = $content.Contains("`r`n")
        $hasUnix = $content.Contains("`n") -and -not $content.Contains("`r`n")
        $hasMac = $content.Contains("`r") -and -not $content.Contains("`r`n")
        
        $lineEndingCount = 0
        if ($hasWindows) { $lineEndingCount++ }
        if ($hasUnix) { $lineEndingCount++ }
        if ($hasMac) { $lineEndingCount++ }
        
        if ($lineEndingCount -gt 1) {
            $EncodingResult.Issues += "Mixed line endings detected"
        }
        elseif ($hasUnix) {
            $EncodingResult.Issues += "Unix line endings (should be Windows CRLF)"
        }
        elseif ($hasMac) {
            $EncodingResult.Issues += "Mac line endings (should be Windows CRLF)"
        }
        
        # Check for emoticons (should be present, not question marks)
        $emojiCount = ($content | Select-String -Pattern "[\u1F300-\u1F6FF\u2600-\u26FF\u2700-\u27BF]" -AllMatches).Matches.Count
        if ($emojiCount -eq 0 -and $content.Length -gt 1000) {
            $EncodingResult.Issues += "No emoticons found (might still have question marks)"
        }
        
        return $EncodingResult
    }
    catch {
        $EncodingResult.Issues += "Failed to read content: $($_.Exception.Message)"
        return $EncodingResult
    }
}

function Write-ValidationReport {
    param(
        [array]$Results,
        [switch]$Detailed
    )
    
    $totalFiles = $Results.Count
    $validFiles = ($Results | Where-Object { $_.Issues.Count -eq 0 }).Count
    $issueFiles = $totalFiles - $validFiles
    
    Write-Host ""
    Write-Host "?? **VALIDATION REPORT**" -ForegroundColor Green
    Write-Host "=" * 50 -ForegroundColor Gray
    Write-Host "Total files: $totalFiles" -ForegroundColor Cyan
    Write-Host "Valid files: $validFiles" -ForegroundColor Green
    Write-Host "Files with issues: $issueFiles" -ForegroundColor $(if ($issueFiles -eq 0) { "Green" } else { "Red" })
    
    if ($issueFiles -eq 0) {
        Write-Host ""
        Write-Host "?? All files passed validation!" -ForegroundColor Green
        return
    }
    
    # Group results by issue type
    $encodingIssues = $Results | Where-Object { $_.Issues -contains "Non-UTF8 encoding detected" }
    $contentIssues = $Results | Where-Object { $_.Issues | Where-Object { $_ -like "*problematic pattern*" } }
    $lineEndingIssues = $Results | Where-Object { $_.Issues | Where-Object { $_ -like "*line ending*" } }
    $emojiIssues = $Results | Where-Object { $_.Issues | Where-Object { $_ -like "*emoticons*" } }
    
    Write-Host ""
    Write-Host "?? **ISSUE BREAKDOWN**" -ForegroundColor Yellow
    Write-Host "=" * 50 -ForegroundColor Gray
    
    if ($encodingIssues.Count -gt 0) {
        Write-Host "? Encoding Issues: $($encodingIssues.Count) files" -ForegroundColor Red
        if ($Detailed) {
            foreach ($file in $encodingIssues) {
                Write-Host "  - $($file.FileName) ($($file.Encoding))" -ForegroundColor Red
            }
        }
    }
    
    if ($contentIssues.Count -gt 0) {
        Write-Host "? Content Issues: $($contentIssues.Count) files" -ForegroundColor Red
        if ($Detailed) {
            foreach ($file in $contentIssues) {
                Write-Host "  - $($file.FileName)" -ForegroundColor Red
                foreach ($issue in $file.Issues) {
                    if ($issue -like "*problematic pattern*") {
                        Write-Host "    $issue" -ForegroundColor DarkRed
                    }
                }
            }
        }
    }
    
    if ($lineEndingIssues.Count -gt 0) {
        Write-Host "??  Line Ending Issues: $($lineEndingIssues.Count) files" -ForegroundColor Yellow
        if ($Detailed) {
            foreach ($file in $lineEndingIssues) {
                Write-Host "  - $($file.FileName)" -ForegroundColor Yellow
            }
        }
    }
    
    if ($emojiIssues.Count -gt 0) {
        Write-Host "??  Emoji Issues: $($emojiIssues.Count) files" -ForegroundColor Cyan
        if ($Detailed) {
            foreach ($file in $emojiIssues) {
                Write-Host "  - $($file.FileName)" -ForegroundColor Cyan
            }
        }
    }
    
    Write-Host ""
    Write-Host "?? **RECOMMENDATIONS**" -ForegroundColor Green
    Write-Host "=" * 50 -ForegroundColor Gray
    
    if ($encodingIssues.Count -gt 0) {
        Write-Host "1. Run FixEncodingAndSymbols.ps1 to fix encoding issues" -ForegroundColor Green
    }
    
    if ($contentIssues.Count -gt 0) {
        Write-Host "2. Run FixEncodingAndSymbols.ps1 to replace question marks with emoticons" -ForegroundColor Green
    }
    
    if ($lineEndingIssues.Count -gt 0) {
        Write-Host "3. Use Git to normalize line endings: git config core.autocrlf true" -ForegroundColor Green
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

$results = @()
$processedCount = 0

foreach ($file in $markdownFiles) {
    $processedCount++
    Write-Host "[$processedCount/$($markdownFiles.Count)] Validating: $($file.Name)" -ForegroundColor Gray
    
    $encodingResult = Test-FileEncoding -FilePath $file.FullName
    $fullResult = Test-FileContent -FilePath $file.FullName -EncodingResult $encodingResult
    
    $results += $fullResult
    
    # Show immediate status
    if ($fullResult.Issues.Count -eq 0) {
        Write-Host "  ? Valid" -ForegroundColor Green
    } else {
        Write-Host "  ? $($fullResult.Issues.Count) issue(s)" -ForegroundColor Red
    }
}

# Generate report
Write-ValidationReport -Results $results -Detailed:$Detailed

Write-Host ""
Write-Host "?? Validation completed!" -ForegroundColor Green