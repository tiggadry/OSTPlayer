# ====================================================================
# FILE: GetCurrentDate.ps1
# PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management  
# MODULE: DevTools
# LOCATION: DevTools/
# VERSION: 1.2.1
# CREATED: 2025-08-07
# UPDATED: 2025-08-07
# AUTHOR: TiggAdry
# ====================================================================
#
# PURPOSE:
# PowerShell development utility script for generating current date in ISO 8601 format
# for use in OstPlayer documentation and file headers. This tool is designed for AI 
# assistants and development automation, providing consistent date formatting across 
# development and documentation processes.
#
# FEATURES:
# - ISO 8601 date formatting (YYYY-MM-DD)
# - Multiple output formats for different use cases
# - Header formatting for file documentation
# - Changelog formatting for version entries
# - Return value for script integration
#
# USAGE:
# .\DevTools\GetCurrentDate.ps1
# .\DevTools\GetCurrentDate.ps1 -Format "yyyy-MM-dd"
#
# PARAMETERS:
# -Format: Custom date format string (default: yyyy-MM-dd)
#
# OUTPUT FORMATS:
# - Basic ISO 8601 date
# - File header format
# - Changelog version format  
# - Changelog entry format
#
# FUTURE REFACTORING:
# FUTURE: Add timezone parameter support
# FUTURE: Add validation for custom formats
# FUTURE: Add output format selection parameter
# CONSIDER: Integration with CI/CD pipelines
# CONSIDER: Integration with DevTools DateHelper.cs
#
# TESTING:
# - Manual testing with different format parameters
# - Integration testing with documentation updates
# - Validation of ISO 8601 compliance
#
# COMPATIBILITY:
# - PowerShell 5.1+
# - Windows PowerShell and PowerShell Core
#
# CHANGELOG:
# 2025-08-07 v1.2.1 - Moved to DevTools module for better organization
# 2025-08-07 v1.1.2 - Initial implementation for date management system
# ====================================================================

# Get Current Date Script for OstPlayer Documentation
# This development tool provides current date in ISO 8601 format for documentation updates

param(
    [string]$Format = "yyyy-MM-dd"
)

# Get current date in specified format
$CurrentDate = Get-Date -Format $Format

# Output formats for different use cases
Write-Host "=== OstPlayer DevTools Date Helper ==="
Write-Host "Current Date (ISO 8601): $CurrentDate"
Write-Host ""
Write-Host "For file headers:"
Write-Host "// UPDATED: $CurrentDate"
Write-Host ""
Write-Host "For changelog version:"
Write-Host "## [x.x.x] - $CurrentDate"
Write-Host ""
Write-Host "For changelog entry:"
Write-Host "$CurrentDate vx.x.x - [description]"
Write-Host ""
Write-Host "DevTools Integration:"
Write-Host "This script complements DevTools/DateHelper.cs for development automation."
Write-Host ""

# Return the date for use in other scripts
return $CurrentDate