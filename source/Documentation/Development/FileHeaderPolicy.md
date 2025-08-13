# 📋 File Header Policy for OstPlayer Project

## 🎯 **Purpose**
This document defines the official policy for file headers in the OstPlayer project, specifically addressing which information should be preserved in main architecture files vs. utility files.

## 🏗️ **Main Architecture Files - Extended Headers**

### **Files that require comprehensive headers:**

#### **OstPlayer.cs** - Main Plugin Entry Point
- **RATIONALE**: Core plugin class orchestrating entire system
- **PRESERVE SECTIONS**:
  - 🔮 **FUTURE REFACTORING** - Plugin-level architectural TODOs
  - 🏗️ **DESIGN PATTERNS** - Understanding system architecture
  - 🛡️ **ERROR HANDLING STRATEGY** - Plugin-level error coordination
  - ✨ **FEATURES** - Complete plugin capabilities overview
  - 🔗 **DEPENDENCIES** - High-level system dependencies

#### **Other Main Architecture Files**:
- **ViewModels/OstPlayerSidebarViewModel.cs** - Main UI orchestration
- **Services/ErrorHandlingService.cs** - Centralized error handling
- **Utils/MusicPlaybackService.cs** - Core audio engine

## 🔧 **Utility Files - Simplified Headers**

### **Files with minimal headers:**
- Helper classes (MusicFileHelper.cs, Mp3MetadataReader.cs)
- Data models (DiscogsMetadataModel.cs)
- Configuration classes
- Simple utility classes

### **REMOVE redundant sections:**
- ⚠️ LIMITATIONS (documented in module documentation)
- 🔗 COMPATIBILITY (available in .csproj and module docs)
- 🧪 TESTING (general info without specifics)
- 🎮 Basic PLAYNITE INTEGRATION (self-evident from code)

## 📋 **Header Content Guidelines**

### ✅ **ALWAYS INCLUDE:**
1. **File metadata** (FILE, PROJECT, MODULE, LOCATION, VERSION, dates, AUTHOR)
2. **PURPOSE** - Clear explanation of file's role
3. **FEATURES** - Key capabilities (especially for main files)
4. **DEPENDENCIES** - Important external dependencies
5. **CHANGELOG** - Recent significant changes

### 🏗️ **INCLUDE FOR MAIN ARCHITECTURE FILES:**
6. **DESIGN PATTERNS** - Architectural patterns used
7. **ERROR HANDLING STRATEGY** - How errors are managed
8. **FUTURE REFACTORING** - Specific architectural TODOs
9. **PERFORMANCE NOTES** - Critical performance considerations

### 🚫 **AVOID REDUNDANCY:**
- Don't duplicate information available in:
  - Module documentation files
  - Project configuration files (.csproj)
  - README files
  - Other file headers

## 🎯 **Specific Policy for OstPlayer.cs**

### **APPROVED SECTIONS** (Keep these):
```
PURPOSE, FEATURES, DEPENDENCIES, DESIGN PATTERNS, 
ERROR HANDLING STRATEGY, FUTURE REFACTORING, CHANGELOG
```

### **REMOVED SECTIONS** (Don't add back):
```
LIMITATIONS, TESTING, PLAYNITE INTEGRATION, COMPATIBILITY
```

### **RATIONALE**:
- **FUTURE REFACTORING**: Plugin-specific architectural improvements not documented elsewhere
- **DESIGN PATTERNS**: Essential for understanding main plugin architecture
- **ERROR HANDLING STRATEGY**: New in v1.2.0, plugin-level coordination strategy
- **LIMITATIONS**: Already documented in Utils/MusicPlaybackService.cs and module documentation
- **COMPATIBILITY**: Available in .csproj target framework and module docs

## 🤖 **AI Assistant Guidelines**

### **DO NOT suggest removing from OstPlayer.cs:**
- FUTURE REFACTORING section (plugin-specific TODOs)
- DESIGN PATTERNS section (architectural understanding)
- ERROR HANDLING STRATEGY section (new v1.2.0 feature)

### **DO suggest removing from utility files:**
- Redundant LIMITATIONS sections (if documented elsewhere)
- Generic COMPATIBILITY information (if in .csproj)
- Basic TESTING information (without specifics)

### **When in doubt:**
1. **Check if information is documented elsewhere**
2. **Consider the file's architectural importance**
3. **Preserve information unique to the specific file**
4. **Remove information that's duplicated across multiple files**

## 📊 **Decision Matrix**

| Information Type | OstPlayer.cs | Utility Files | Rationale |
|------------------|--------------|---------------|-----------|
| Plugin-level TODOs | ✅ Keep | ❌ Remove | Unique to main plugin |
| Design Patterns | ✅ Keep | ❌ Remove | Architectural understanding |
| Error Strategy | ✅ Keep | ❌ Remove | Plugin-level coordination |
| File-specific features | ✅ Keep | ✅ Keep | Unique to each file |
| General limitations | ❌ Remove | ❌ Remove | In module documentation |
| Basic compatibility | ❌ Remove | ❌ Remove | In .csproj and docs |

## 🔧 **Maintenance Process**

### **When adding new sections:**
1. **Check for redundancy** with existing documentation
2. **Verify architectural relevance** to the specific file
3. **Document in Documentation/Development/FileHeaderPolicy.md** if it's a new pattern

### **When removing sections:**
1. **Verify information is available elsewhere**
2. **Update this policy document** with rationale
3. **Ensure no unique information is lost**

---

**Created**: 2025-08-07  
**Updated**: 2025-08-08  
**Purpose**: Prevent future redundant header suggestions  
**Scope**: All OstPlayer project files  
**Authority**: Project architectural decisions  
**Review**: When major file reorganization occurs  
**Location**: Documentation/Development/ (proper documentation structure)