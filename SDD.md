# File Commander (fcom) - Software Design Document

**Version:** 1.0  
**Date:** December 15, 2025  
**Status:** Phase 1 MVP Complete  
**Author:** Systems Engineering Team

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [System Overview](#2-system-overview)
3. [Architecture](#3-architecture)
4. [Component Specifications](#4-component-specifications)
5. [Data Models](#5-data-models)
6. [User Interface Design](#6-user-interface-design)
7. [File Operations](#7-file-operations)
8. [Keyboard Interface](#8-keyboard-interface)
9. [Threading and Concurrency](#9-threading-and-concurrency)
10. [Error Handling](#10-error-handling)
11. [Configuration and Settings](#11-configuration-and-settings)
12. [Testing Strategy](#12-testing-strategy)
13. [Future Enhancements](#13-future-enhancements)

---

## 1. Introduction

### 1.1 Purpose

This document describes the software design for **File Commander (fcom)**, a high-performance, keyboard-centric Text User Interface (TUI) file manager that adheres to the Orthodox File Manager (OFM) paradigm.

### 1.2 Scope

File Commander is designed to provide:
- Efficient file management through keyboard-driven operations
- Dual-pane interface for intuitive file operations
- Asynchronous I/O with intelligent task queuing
- Cross-platform support (Linux primary, Windows secondary)

### 1.3 Design Goals

1. **Performance**: Non-blocking UI with async operations
2. **Usability**: Keyboard-first with OFM standard keybindings
3. **Reliability**: Robust error handling and data safety
4. **Maintainability**: Clean architecture with clear separation of concerns
5. **Extensibility**: Plugin-ready architecture for future enhancements

### 1.4 Technology Stack

- **Language**: C# 12
- **Runtime**: .NET 8.0
- **UI Framework**: Terminal.Gui 1.19.0
- **Platforms**: Linux (Ubuntu 20.04+), Windows 10+

---

## 2. System Overview

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────┐
│                  Presentation Layer                 │
│              (Terminal.Gui Framework)               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────┐ │
│  │  MainWindow  │  │ FilePaneView │  │ Dialogs  │ │
│  └──────────────┘  └──────────────┘  └──────────┘ │
└────────────────┬────────────────────────────────────┘
                 │ Events/Commands
┌────────────────▼────────────────────────────────────┐
│              Application Layer                      │
│   ┌──────────────┐      ┌──────────────────┐      │
│   │  TabManager  │      │ CommandHandler   │      │
│   └──────────────┘      └──────────────────┘      │
└────────────────┬────────────────────────────────────┘
                 │ Service Calls
┌────────────────▼────────────────────────────────────┐
│                Service Layer                        │
│  ┌──────────────────┐  ┌──────────────────────┐   │
│  │ FileSystemService│  │FileOperationService  │   │
│  └──────────────────┘  └──────────────────────┘   │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│              Operating System                       │
│        (File System, I/O, Threads)                  │
└─────────────────────────────────────────────────────┘
```

### 2.2 Component Interaction Flow

```
User Keypress
    │
    ▼
MainWindow.KeyPress Handler
    │
    ▼
CommandHandler.Handle[Operation]
    │
    ▼
TabManager (Update State)
    │
    ▼
FileSystemService / FileOperationService
    │
    ▼
Async Operation Execution
    │
    ▼
Event Callbacks (Progress, Status)
    │
    ▼
UI Update (SetNeedsDisplay)
```

---

## 3. Architecture

### 3.1 Layered Architecture Pattern

#### 3.1.1 Presentation Layer (UI)
**Responsibility**: Rendering, input handling, user feedback

**Components**:
- `MainWindow`: Top-level window, layout management, key routing
- `FilePaneView`: File list rendering, selection visualization
- `Dialog`: Modal dialogs for confirmations and input

**Key Characteristics**:
- No business logic
- Event-driven communication
- Uses Terminal.Gui primitives

#### 3.1.2 Application Layer
**Responsibility**: Business logic, state management, command orchestration

**Components**:
- `TabManager`: Manages tab lifecycle, state transitions
- `CommandHandler`: Translates user commands to operations

**Key Characteristics**:
- Stateful (maintains application state)
- Coordinates between UI and Services
- Validation and business rules

#### 3.1.3 Service Layer
**Responsibility**: Core operations, file system interaction, async I/O

**Components**:
- `FileSystemService`: Read-only file system operations
- `FileOperationService`: Write operations (Copy/Move/Delete)
- `DirectoryMonitorService`: (Phase 2) File system monitoring

**Key Characteristics**:
- Stateless (except for operation tracking)
- Async/await throughout
- Platform abstraction

### 3.2 Design Patterns

#### 3.2.1 Observer Pattern
**Usage**: Event-driven communication between layers

```csharp
// Example: File operation progress
FileOperationService.StatusChanged += (sender, message) => {
    StatusBar.Text = message;
};
```

#### 3.2.2 Command Pattern
**Usage**: Encapsulating user actions

```csharp
// CommandHandler orchestrates operations
public void HandleCopy() {
    var files = GetSelectedFiles();
    var destination = GetPassivePanePath();
    ConfirmationRequired?.Invoke(this, ("Copy", message, async () => {
        await _fileOperationService.CopyAsync(files, destination);
    }));
}
```

#### 3.2.3 Repository Pattern
**Usage**: File system abstraction

```csharp
public interface IFileSystemService {
    List<FileItem> ListDirectory(string path);
    string NormalizePath(string path);
}
```

### 3.3 Separation of Concerns

| Concern | Layer | Example |
|---------|-------|---------|
| **Rendering** | UI | Terminal.Gui views, layouts |
| **Input Handling** | UI | Keyboard event processing |
| **State Management** | Application | TabState, selection tracking |
| **Business Logic** | Application | Mark file, validate operations |
| **File I/O** | Service | Read directory, copy files |
| **Error Handling** | All | Try-catch, user notification |

---

## 4. Component Specifications

### 4.1 Models (Data Structures)

#### 4.1.1 DisplayMode

```csharp
public enum DisplayMode {
    SinglePane,  // Tree + File List + Preview
    DualPane     // Two file lists (Active/Passive)
}
```

**Purpose**: Defines the visual layout mode

#### 4.1.2 FileItem

```csharp
public class FileItem {
    public string Name { get; set; }
    public string FullPath { get; set; }
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
    public DateTime LastModified { get; set; }
    public FileAttributes Attributes { get; set; }
    public string FormattedSize { get; }      // Property
    public string FormattedDate { get; }      // Property
}
```

**Purpose**: Represents a file or directory entry  
**Key Features**:
- Formatted properties for display
- Lazy calculation of directory sizes (Phase 1.1)
- Caching to avoid repeated I/O

#### 4.1.3 TabState

```csharp
public class TabState {
    public Guid TabId { get; set; }
    public string CurrentPath { get; set; }           // Active pane path
    public string PathPassive { get; set; }           // Passive pane path
    public DisplayMode DisplayMode { get; set; }
    public bool IsDirty { get; set; }                 // Needs refresh?
    
    // Active pane state
    public int SelectedIndexActive { get; set; }
    public int ScrollOffsetActive { get; set; }
    public List<FileItem> FilesActive { get; set; }
    
    // Passive pane state
    public int SelectedIndexPassive { get; set; }
    public int ScrollOffsetPassive { get; set; }
    public List<FileItem> FilesPassive { get; set; }
    
    // Which pane is active?
    public bool IsLeftPaneActive { get; set; }
    
    // Marked files (full paths)
    public HashSet<string> MarkedFiles { get; set; }
}
```

**Purpose**: Complete state snapshot for a tab  
**Design Notes**:
- Immutable ID (Guid)
- Separate state for both panes
- Marked files as HashSet for O(1) lookup

#### 4.1.4 OperationBuffer

```csharp
public enum OperationType {
    Copy,
    Move,
    Delete
}

public class OperationBuffer {
    public OperationType Operation { get; set; }
    public List<string> SourcePaths { get; set; }
    public bool IsEmpty => SourcePaths.Count == 0;
    
    public void Clear();
}
```

**Purpose**: Staged operations for Ctrl+C/X/V workflow (Phase 2)

### 4.2 Service Components

#### 4.2.1 FileSystemService

```csharp
public class FileSystemService {
    // Read operations only
    public List<FileItem> ListDirectory(string path);
    public List<FileItem> GetDrives();
    public string NormalizePath(string path);
    public long CalculateDirectorySize(string path);  // Phase 1.1
}
```

**Responsibilities**:
- Directory enumeration
- Path normalization
- Drive/mount point discovery
- Directory size calculation (recursive)

**Error Handling**:
- Returns error FileItem on exception
- Catches UnauthorizedAccessException
- Handles invalid paths gracefully

#### 4.2.2 FileOperationService

```csharp
public class FileOperationService {
    public event EventHandler<string> StatusChanged;
    public event EventHandler<int> ProgressChanged;
    
    public async Task<bool> CopyAsync(List<string> sources, 
                                      string dest, 
                                      CancellationToken ct);
    public async Task<bool> MoveAsync(List<string> sources, 
                                      string dest, 
                                      CancellationToken ct);
    public async Task<bool> DeleteAsync(List<string> paths, 
                                        CancellationToken ct);
}
```

**Responsibilities**:
- Async file operations
- Progress reporting via events
- Cancellation support
- Recursive directory operations

**Implementation Details**:
- Uses `FileStream` with async APIs
- 80KB buffer for file copies
- Proper resource disposal (using/await using)

### 4.3 Application Components

#### 4.3.1 TabManager

```csharp
public class TabManager {
    private List<TabState> _tabs;
    private int _activeTabIndex;
    
    public event EventHandler TabChanged;
    public event EventHandler TabStateChanged;
    
    public void CreateTab(string path);
    public void SwitchToTab(int index);
    public void CloseTab(int index);
    public void RefreshActivePane();
    public void RefreshBothPanes();
    public void NavigateTo(string path);
    public void SwitchActivePane();
    public void ToggleDisplayMode();
}
```

**Responsibilities**:
- Tab lifecycle management
- State transitions
- Refresh coordination
- Navigation orchestration

**Design Notes**:
- Minimum 1 tab always
- Events for UI synchronization
- Encapsulates TabState complexity

#### 4.3.2 CommandHandler

```csharp
public class CommandHandler {
    public event EventHandler<string> StatusMessage;
    public event EventHandler<(string, string, Action)> ConfirmationRequired;
    public event EventHandler OperationStarted;
    public event EventHandler OperationCompleted;
    
    public void HandleEnter();
    public void HandleBackspace();
    public void HandleCopy();        // F5
    public void HandleMove();        // F6
    public void HandleMkDir(string name);  // F7
    public void HandleDelete();      // F8
    public void ToggleFileMark();    // Insert/Space
    public void ToggleMarkWithMove(int direction);  // Shift+Up/Down (Phase 1.1)
}
```

**Responsibilities**:
- Command validation
- Confirmation flow
- Operation orchestration
- Status reporting

### 4.4 UI Components

#### 4.4.1 MainWindow

```csharp
public class MainWindow : Window {
    private FilePaneView _leftPane;
    private FilePaneView _rightPane;
    private Label _statusBar;
    private Label _helpBar;
    
    private void InitializeUI();
    private void SetupEventHandlers();
    private void UpdateDisplay();
    private void UpdateStatus(string message);
}
```

**Responsibilities**:
- Window layout
- Key binding registration
- Event routing
- UI synchronization

**Layout**:
```
┌─────────────────────────────────────────┐
│  Left Pane: /home/user  │ Right: /tmp  │
│  [File List]            │ [File List]  │
│                         │              │
│  (Active)               │ (Passive)    │
├─────────────────────────────────────────┤
│  Status: Ready                          │
├─────────────────────────────────────────┤
│  F5:Copy F6:Move F7:MkDir F8:Del ...   │
└─────────────────────────────────────────┘
```

#### 4.4.2 FilePaneView

```csharp
public class FilePaneView : FrameView {
    private ListView _listView;
    
    public event EventHandler<FileItem> FileSelected;
    public event EventHandler<FileItem> FileActivated;
    
    public void SetFiles(List<FileItem> files, 
                        HashSet<string> markedFiles, 
                        int selectedIndex);
    public void SetActive(bool isActive);
    public void MoveSelection(int delta);
}
```

**Responsibilities**:
- File list rendering
- Selection visualization
- Mark indicators (*)
- Active/passive styling

**Display Format**:
```
* 📁 Documents              <DIR>  2025-12-15 10:30
  📄 report.pdf         1.5 MB  2025-12-15 09:15
* 📄 notes.txt          5.2 KB  2025-12-14 16:45
```

---

## 5. Data Models

### 5.1 State Management

#### 5.1.1 Application State

```
Application
├── TabManager
│   ├── Tabs: List<TabState>
│   └── ActiveTabIndex: int
└── OperationBuffer (Phase 2)
    ├── Operation: OperationType
    └── SourcePaths: List<string>
```

#### 5.1.2 Tab State Transitions

```
┌──────────────┐
│  Tab Created │
└──────┬───────┘
       │ CreateTab(path)
       ▼
┌──────────────┐
│  Initialize  │ ← Set CurrentPath
│              │   Load FilesActive
└──────┬───────┘
       │
       ▼
┌──────────────┐     Navigate(path)
│   Active     │ ◄─────────────────┐
│              │                   │
└──────┬───────┘                   │
       │                           │
       │ RefreshActivePane()       │
       ▼                           │
┌──────────────┐                   │
│  Refreshing  │ ──────────────────┘
└──────────────┘
```

### 5.2 File Selection State

#### 5.2.1 Selection Types

1. **Current Selection** (SelectedIndex)
   - Single item under cursor
   - Visual highlight
   - Changes with arrow keys

2. **Marked Files** (MarkedFiles HashSet)
   - Multiple items for batch operations
   - Toggle with Insert/Space
   - Visual indicator (*)
   - Persists across navigation

3. **Shift+Arrow Selection** (Phase 1.1)
   - Range selection
   - Visual feedback during selection
   - Adds to MarkedFiles

#### 5.2.2 Selection State Machine

```
[No Selection]
       │
       ├─ Arrow Key ──────► [Current Selection Changed]
       │
       ├─ Space ──────────► [Toggle Mark (Stay)]
       │
       ├─ Insert ─────────► [Toggle Mark (Move Down)]
       │
       └─ Shift+Arrow ────► [Range Mark (Move)]
```

---

## 6. User Interface Design

### 6.1 Screen Layout

#### 6.1.1 Dual-Pane Mode (Default)

```
┌─────────────────────────────────────────────────────────────┐
│ File Commander (fcom)                                       │
├─────────────────────────────────────────────────────────────┤
│ Left: /home/user/docs    │  Right: /home/user/downloads    │
├──────────────────────────┼──────────────────────────────────┤
│                          │                                  │
│ * 📁 project1    <DIR>   │   📄 file1.txt       1.2 KB     │
│   📁 project2    <DIR>   │   📄 file2.pdf       2.5 MB     │
│ * 📄 readme.md   5.0 KB  │   📁 archive         <DIR>      │
│   📄 todo.txt    1.1 KB  │   📄 image.png       856 KB     │
│   ...                    │   ...                            │
│                          │                                  │
│                          │                                  │
├──────────────────────────┴──────────────────────────────────┤
│ Status: 2 files marked                                      │
├─────────────────────────────────────────────────────────────┤
│ F5:Copy F6:Move F7:MkDir F8:Del F9:Mode F10:Quit Tab:Switch│
└─────────────────────────────────────────────────────────────┘
```

#### 6.1.2 Single-Pane Mode (Phase 3)

```
┌─────────────────────────────────────────────────────────────┐
│ File Commander (fcom)                                       │
├─────────────┬───────────────────────────┬───────────────────┤
│ Tree        │  File List                │  Preview          │
├─────────────┼───────────────────────────┼───────────────────┤
│             │                           │                   │
│ ▼ home      │  📄 readme.md    5.0 KB  │ # README          │
│   ▼ user    │  📄 todo.txt     1.1 KB  │                   │
│     ► docs  │  📁 project1     <DIR>   │ This is a test    │
│     ► down  │  📁 project2     <DIR>   │ file...           │
│             │                           │                   │
├─────────────┴───────────────────────────┴───────────────────┤
│ Status: /home/user                                          │
├─────────────────────────────────────────────────────────────┤
│ F3:View F5:Copy F6:Move F7:MkDir F8:Del F9:Mode F10:Quit   │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 Visual Indicators

| Indicator | Meaning | Display |
|-----------|---------|---------|
| `*` | Marked file | `* 📄 file.txt` |
| `📁` | Directory | `📁 Documents` |
| `📄` | Regular file | `📄 report.pdf` |
| Highlight | Current selection | Colored background |
| Border style | Active pane | Bold/colored border |
| `<DIR>` | Directory size | In size column |

### 6.3 Color Scheme

```csharp
// Active pane
ColorScheme = Colors.Base  // Blue/Cyan theme

// Passive pane
ColorScheme = Colors.TopLevel  // Gray theme

// Status bar
ColorScheme = Colors.TopLevel

// Help bar
ColorScheme = Colors.Menu  // Black on white
```

---

## 7. File Operations

### 7.1 Operation Workflow

#### 7.1.1 Copy Operation (F5)

```
1. User presses F5
2. CommandHandler.HandleCopy()
3. Gather files:
   - If MarkedFiles.Count > 0: Use marked files
   - Else: Use current selection
4. Get destination (opposite pane path)
5. Show confirmation dialog
6. On confirm:
   a. Invoke FileOperationService.CopyAsync()
   b. Show progress updates
   c. Refresh both panes
   d. Clear marked files
   e. Show completion status
```

#### 7.1.2 Move Operation (F6)

Similar to Copy, but:
- Uses `FileOperationService.MoveAsync()`
- Source files are removed
- Can be used for renaming (same directory)

#### 7.1.3 Delete Operation (F8)

```
1. User presses F8
2. CommandHandler.HandleDelete()
3. Gather files (marked or selected)
4. Show WARNING confirmation
5. On confirm:
   a. Invoke FileOperationService.DeleteAsync()
   b. Show progress
   c. Refresh active pane
   d. Clear marked files
```

### 7.2 Async Operation Implementation

```csharp
public async Task<bool> CopyAsync(List<string> sources, 
                                  string dest, 
                                  CancellationToken ct) {
    int total = sources.Count;
    int current = 0;
    
    foreach (var source in sources) {
        ct.ThrowIfCancellationRequested();
        
        current++;
        StatusChanged?.Invoke(this, $"Copying {current}/{total}");
        ProgressChanged?.Invoke(this, (current * 100) / total);
        
        if (Directory.Exists(source)) {
            await CopyDirectoryAsync(source, dest, ct);
        } else {
            await CopyFileAsync(source, dest, ct);
        }
    }
    
    return true;
}

private async Task CopyFileAsync(string src, string dst, CancellationToken ct) {
    const int bufferSize = 81920;  // 80KB
    
    await using var srcStream = new FileStream(
        src, FileMode.Open, FileAccess.Read, 
        FileShare.Read, bufferSize, useAsync: true);
        
    await using var dstStream = new FileStream(
        dst, FileMode.Create, FileAccess.Write, 
        FileShare.None, bufferSize, useAsync: true);
        
    await srcStream.CopyToAsync(dstStream, bufferSize, ct);
}
```

### 7.3 Error Handling

| Error Type | Handling Strategy | User Feedback |
|------------|------------------|---------------|
| **FileNotFoundException** | Skip file, continue | Status: "File not found: {name}" |
| **UnauthorizedAccessException** | Skip file, continue | Status: "Access denied: {name}" |
| **IOException** | Prompt user | Dialog: "File exists. Overwrite?" |
| **OutOfDiskSpaceException** | Abort operation | Dialog: "Disk full. Operation cancelled." |
| **OperationCanceledException** | Clean abort | Status: "Operation cancelled by user" |

---

## 8. Keyboard Interface

### 8.1 Key Bindings (OFM Standard)

#### 8.1.1 File Operations

| Key | Action | Confirmation | Marked Files |
|-----|--------|--------------|--------------|
| **F5** | Copy to opposite pane | Yes | Supported |
| **F6** | Move to opposite pane | Yes | Supported |
| **F7** | Create directory | Dialog | N/A |
| **F8** | Delete | Yes (WARNING) | Supported |

#### 8.1.2 Navigation

| Key | Action | Context |
|-----|--------|---------|
| **↑** | Move selection up | Always |
| **↓** | Move selection down | Always |
| **Enter** | Navigate into directory / Open file | On item |
| **Backspace** | Go to parent directory | Always |
| **Tab** | Switch active pane | Dual-pane mode |
| **F9** | Toggle Single/Dual pane | Always |
| **F10** | Quit application | Always |

#### 8.1.3 Selection (Phase 1.1)

| Key | Action | Behavior |
|-----|--------|----------|
| **Space** | Toggle mark (stay) | Mark/unmark, cursor stays |
| **Insert** | Toggle mark (move) | Mark/unmark, cursor moves down |
| **Shift+↑** | Range mark up | Mark and move up |
| **Shift+↓** | Range mark down | Mark and move down |
| **Ctrl+Space** | Toggle mark (alt) | Same as Space |

### 8.2 Key Handling Implementation

```csharp
KeyPress += (e) => {
    var handled = true;
    var shift = e.KeyEvent.Key.HasFlag(Key.ShiftMask);
    
    switch (e.KeyEvent.Key & ~Key.ShiftMask) {
        case Key.CursorUp:
            if (shift) {
                _commandHandler.ToggleMarkWithMove(-1);
            } else {
                GetActivePane().MoveSelection(-1);
            }
            break;
            
        case Key.CursorDown:
            if (shift) {
                _commandHandler.ToggleMarkWithMove(1);
            } else {
                GetActivePane().MoveSelection(1);
            }
            break;
            
        case Key.F5:
            _commandHandler.HandleCopy();
            break;
            
        // ... other keys
    }
    
    if (handled) e.Handled = true;
};
```

---

## 9. Threading and Concurrency

### 9.1 Threading Model (Phase 1)

```
┌─────────────────┐
│   UI Thread     │ ◄─── Terminal.Gui event loop
└────────┬────────┘
         │
         ├─ User Input Events
         │
         ▼
┌─────────────────┐
│ Command Handler │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Task.Run()     │ ◄─── Async file operations
│  (Thread Pool)  │
└────────┬────────┘
         │
         ├─ Progress Events ───┐
         │                      │
         ▼                      ▼
┌─────────────────┐      ┌──────────┐
│  File I/O       │      │ UI Update│
└─────────────────┘      └──────────┘
```

### 9.2 Async/Await Strategy

**Principles**:
1. UI thread never blocks on I/O
2. All file operations use async APIs
3. Progress callbacks on UI thread
4. CancellationToken propagation

**Example**:
```csharp
// In CommandHandler
private async void ExecuteCopyOperation(List<string> sources, string dest) {
    OperationStarted?.Invoke(this, EventArgs.Empty);
    
    try {
        var success = await _fileOperationService.CopyAsync(sources, dest);
        
        if (success) {
            _tabManager.RefreshBothPanes();
            StatusMessage?.Invoke(this, "Copy completed successfully");
        }
    } catch (Exception ex) {
        StatusMessage?.Invoke(this, $"Copy failed: {ex.Message}");
    } finally {
        OperationCompleted?.Invoke(this, EventArgs.Empty);
    }
}
```

### 9.3 Future: Intelligent Task Queue (Phase 2)

```csharp
public class IntelligentTaskQueue {
    private Channel<FileOperationJob> _queue;
    private Dictionary<string, SemaphoreSlim> _driveLocks;
    
    public async Task EnqueueAsync(FileOperationJob job) {
        await _queue.Writer.WriteAsync(job);
    }
    
    private async Task ProcessJobsAsync(CancellationToken ct) {
        await foreach (var job in _queue.Reader.ReadAllAsync(ct)) {
            var drivePair = GetDrivePair(job.Source, job.Destination);
            
            // Acquire lock for this drive pair (sequential)
            await _driveLocks[drivePair].WaitAsync(ct);
            
            try {
                await ExecuteJobAsync(job, ct);
            } finally {
                _driveLocks[drivePair].Release();
            }
        }
    }
}
```

**Drive-Aware Logic**:
- Jobs with same drive pair: Sequential (one at a time)
- Jobs with different drive pairs: Parallel (concurrent execution)
- Benefits: Minimizes disk thrashing, maximizes throughput

---

## 10. Error Handling

### 10.1 Error Hierarchy

```
Exception
├── IOException
│   ├── FileNotFoundException
│   ├── DirectoryNotFoundException
│   ├── PathTooLongException
│   └── DriveNotFoundException
├── UnauthorizedAccessException
├── SecurityException
└── OperationCanceledException
```

### 10.2 Error Handling Strategy

| Layer | Strategy | Example |
|-------|----------|---------|
| **UI** | Display to user | MessageBox, StatusBar |
| **Application** | Log and propagate | Event → UI |
| **Service** | Catch and return status | `Task<bool>` return |

### 10.3 Error Recovery

```csharp
public List<FileItem> ListDirectory(string path) {
    var items = new List<FileItem>();
    
    try {
        var dirInfo = new DirectoryInfo(path);
        
        foreach (var dir in dirInfo.GetDirectories()) {
            try {
                items.Add(CreateFileItem(dir));
            } catch (UnauthorizedAccessException) {
                // Skip inaccessible directories
                continue;
            }
        }
        
        // ... enumerate files
        
    } catch (Exception ex) {
        // Return error indicator
        items.Add(new FileItem {
            Name = $"ERROR: {ex.Message}",
            IsDirectory = false,
            Size = 0
        });
    }
    
    return items;
}
```

---

## 11. Configuration and Settings

### 11.1 Configuration File (Phase 2)

**Location**: `~/.config/fcom/settings.json` (Linux)

```json
{
  "ui": {
    "colorScheme": "default",
    "showHiddenFiles": false,
    "dateFormat": "yyyy-MM-dd HH:mm"
  },
  "behavior": {
    "confirmDelete": true,
    "confirmOverwrite": true,
    "autoRefresh": false,
    "calculateDirSizes": true
  },
  "keybindings": {
    "mode": "default",  // or "vim"
    "custom": {}
  }
}
```

### 11.2 Default Settings

```csharp
public class Settings {
    public bool ShowHiddenFiles { get; set; } = false;
    public bool ConfirmDelete { get; set; } = true;
    public bool CalculateDirectorySizes { get; set; } = true;
    public bool AutoRefresh { get; set; } = false;
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm";
}
```

---

## 12. Testing Strategy

### 12.1 Unit Testing

**Target Coverage**: Service and Application layers

```csharp
[TestFixture]
public class FileSystemServiceTests {
    [Test]
    public void ListDirectory_ValidPath_ReturnsItems() {
        var service = new FileSystemService();
        var items = service.ListDirectory("/tmp");
        
        Assert.That(items, Is.Not.Empty);
        Assert.That(items[0].Name, Is.EqualTo(".."));
    }
    
    [Test]
    public void CalculateDirectorySize_EmptyDir_ReturnsZero() {
        // Arrange
        var tempDir = CreateTempDirectory();
        var service = new FileSystemService();
        
        // Act
        var size = service.CalculateDirectorySize(tempDir);
        
        // Assert
        Assert.That(size, Is.EqualTo(0));
    }
}
```

### 12.2 Integration Testing

**Scenarios**:
1. Copy file from pane A to pane B
2. Move directory with contents
3. Delete marked files
4. Navigate through directory tree
5. Refresh after external changes

### 12.3 Manual Testing Checklist

- [ ] All F-key operations work
- [ ] Selection with Shift+Arrow marks correctly
- [ ] Space toggles without moving
- [ ] Insert toggles and moves down
- [ ] Directory sizes calculate correctly
- [ ] Marked files visual indicator appears
- [ ] Confirmation dialogs appear
- [ ] Operations complete successfully
- [ ] Error messages are user-friendly
- [ ] UI remains responsive during operations

---

## 13. Future Enhancements

### 13.1 Phase 2: Performance & Decoupling

**Intelligent Task Queue**:
```csharp
public class FileOperationJob {
    public Guid JobId { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public OperationType Type { get; set; }
    public JobStatus Status { get; set; }
    public int Progress { get; set; }  // 0-100
}

public class IntelligentTaskQueue {
    private Channel<FileOperationJob> _jobQueue;
    private ConcurrentDictionary<string, SemaphoreSlim> _driveLocks;
    
    public async Task<Guid> EnqueueAsync(FileOperationJob job);
    public IObservable<JobProgress> ObserveJob(Guid jobId);
    public async Task CancelJobAsync(Guid jobId);
}
```

**Status Pane**:
```
┌─────────────────────────────────────┐
│ Active Operations (3)               │
├─────────────────────────────────────┤
│ [▓▓▓▓▓▓▓░░░] 75% Copy file1.iso     │
│ [▓▓░░░░░░░░] 20% Move Documents/    │
│ [████████] 100% Delete old_file.txt │
└─────────────────────────────────────┘
```

### 13.2 Phase 3: Advanced Features

**Diff/Sync Mode**:
- Compare two directories
- Highlight differences (newer, older, unique)
- Batch synchronization

**Preview Pane** (F3):
- Text files: Syntax highlighting
- Images: Sixel/iTerm2 protocol
- Archives: List contents
- Hex view: Binary files

**Search** (F7):
- Name search (glob patterns)
- Content search (grep-like)
- Filter current view

### 13.3 Plugin Architecture

```csharp
public interface IFileCommanderPlugin {
    string Name { get; }
    string Version { get; }
    
    void Initialize(IPluginContext context);
    void RegisterCommands(ICommandRegistry registry);
    void RegisterKeybindings(IKeybindingRegistry registry);
}

public interface IPluginContext {
    IFileSystemService FileSystem { get; }
    ITabManager TabManager { get; }
    void ShowNotification(string message);
}
```

---

## Appendices

### A. Class Diagram

```
┌─────────────────┐
│   MainWindow    │
└────────┬────────┘
         │ owns
         ▼
┌─────────────────┐      ┌─────────────────┐
│  TabManager     │◄────►│ CommandHandler  │
└────────┬────────┘      └────────┬────────┘
         │                         │
         │ manages                 │ uses
         ▼                         ▼
┌─────────────────┐      ┌──────────────────────┐
│    TabState     │      │ FileOperationService │
└─────────────────┘      └──────────────────────┘
         │
         │ contains
         ▼
┌─────────────────┐
│    FileItem     │
└─────────────────┘
```

### B. Sequence Diagram: Copy Operation

```
User  MainWindow  CommandHandler  TabManager  FileOpService
 │         │            │              │            │
 ├─F5─────►│            │              │            │
 │         ├───────────►│              │            │
 │         │            ├─GetState────►│            │
 │         │            │◄─────────────┤            │
 │         │            ├─Confirm?─────┤            │
 │         │◄───────────┤              │            │
 │◄────────┤ Dialog     │              │            │
 ├─Yes────►│            │              │            │
 │         ├───────────►│              │            │
 │         │            ├──────────────┼─CopyAsync─►│
 │         │            │              │            ├─I/O
 │         │            │◄─Progress────┼────────────┤
 │         │◄───Status──┤              │            │
 │◄────────┤            │              │            │
 │         │            │◄─────────────┼────────────┤
 │         │            ├─Refresh─────►│            │
 │         │◄───────────┤              │            │
```

### C. State Diagram: File Selection

```
     ┌──────────────┐
     │ No Selection │
     └──────┬───────┘
            │ Arrow Key
            ▼
     ┌──────────────┐
  ┌──┤   Selected   │◄──┐
  │  └──────┬───────┘   │
  │         │            │
  │ Space   │ Insert     │
  │         │            │
  │  ┌──────▼───────┐   │
  │  │    Marked    │   │
  │  │ (no move)    │   │
  │  └──────────────┘   │
  │                     │
  │  ┌──────────────┐   │
  └─►│   Marked     │───┘
     │ (move down)  │
     └──────────────┘
     
  Shift+Arrow
     ▼
  ┌──────────────┐
  │ Range Marked │
  │ (moves)      │
  └──────────────┘
```

### D. Build and Deployment

**Build Command**:
```bash
dotnet build -c Release
```

**Publish for Linux**:
```bash
dotnet publish -c Release -r linux-x64 --self-contained
```

**Publish for Windows**:
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

**Installation**:
```bash
# Linux
sudo cp ./bin/Release/net8.0/linux-x64/publish/fcom /usr/local/bin/
sudo chmod +x /usr/local/bin/fcom

# Or create symlink
ln -s "$(pwd)/File Commander/bin/Debug/net8.0/File Commander" ~/bin/fcom
```

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-15 | Engineering Team | Initial SDD for Phase 1 MVP |

---

**END OF DOCUMENT**

