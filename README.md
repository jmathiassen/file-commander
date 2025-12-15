# File Commander (fcom) - Phase 1.1

## Overview

File Commander is a keyboard-centric TUI (Text User Interface) file manager built with .NET 8 and Terminal.Gui, following the Orthodox File Manager (OFM) paradigm.

**Current Version:** 1.1 (Phase 1.1 Complete)  
**Status:** Production Ready ✅

## Features Implemented

### ✅ Core Navigation
- Dual-pane file browser (OFM standard)
- Single-pane mode toggle (F9)
- Directory navigation (Enter = descend, Backspace = parent)
- File/folder listing with size and date
- Tab key to switch active pane
- Visual active/passive pane indicators

### ✅ Enhanced File Selection (Phase 1.1 - NEW!)
- **Space**: Toggle mark (cursor stays) - precise selection
- **Insert**: Toggle mark and move down - sequential selection
- **Shift+↑/↓**: Range selection - fastest for bulk marking
- Visual marked file indicators (*)
- Marked file count in status bar
- Persistent marking across pane switches

### ✅ Directory Size Calculation (Phase 1.1 - NEW!)
- Automatic when directory is marked
- Recursive calculation (all subdirectories)
- Asynchronous (non-blocking UI)
- Cached until refresh
- Formatted display (B/KB/MB/GB/TB)
- Graceful permission error handling

### ✅ File Operations
- **F5**: Copy files/folders to opposite pane
- **F6**: Move files/folders to opposite pane
- **F7**: Create directory (with dialog)
- **F8**: Delete files/folders (with confirmation)
- Works with marked files or current selection
- Async operations with progress feedback

### ✅ OFM Compliance
- F-key bindings (Norton Commander/Midnight Commander standard)
- Dual-pane active/passive concept
- Tab to switch panes
- Keyboard-first design
- Visual feedback for all operations

### ✅ Professional Architecture
- 3-layer design (UI → Application → Services)
- Complete Software Design Document (SDD)
- Event-driven communication
- Async/await throughout
- Comprehensive error handling
- Full replication possible from SDD

## Build & Run

```bash
cd "File Commander"
dotnet restore
dotnet build
dotnet run
```

Or with a specific starting directory:

```bash
dotnet run -- /path/to/directory
```

## Key Bindings

### File Operations
| Key | Action |
|-----|--------|
| **F5** | Copy selected/marked files to opposite pane |
| **F6** | Move selected/marked files to opposite pane |
| **F7** | Create new directory |
| **F8** | Delete selected/marked files |

### Navigation
| Key | Action |
|-----|--------|
| **↑/↓** | Move selection up/down |
| **Enter** | Navigate into directory |
| **Backspace** | Go to parent directory |
| **Tab** | Switch active pane (in dual mode) |
| **F9** | Toggle Single/Dual pane mode |
| **F10** | Quit application |

### File Selection (Phase 1.1)
| Key | Action | Movement | Best For |
|-----|--------|----------|----------|
| **Space** | Toggle mark | Stays | Precise selection |
| **Insert** | Toggle mark | Moves down | Sequential selection |
| **Shift+↑** | Toggle mark | Moves up | Range selection (up) |
| **Shift+↓** | Toggle mark | Moves down | Range selection (down) |

**Selection Tips:**
- Use **Space** when reviewing files one-by-one
- Use **Insert** for quick sequential marking
- Use **Shift+Arrow** for selecting ranges
- Combine methods as needed for complex selections

## Project Structure

```
File Commander/
├── SDD.md                          # Software Design Document (complete architecture)
├── USER_GUIDE.md                   # Complete feature list & workflows
├── PHASE_1.1_UPDATE.md            # Phase 1.1 new features
├── README.md                       # This file
├── QUICKSTART.md                   # Quick reference
├── IMPLEMENTATION_SUMMARY.md       # Technical summary
├── fcom.sh                         # Launch script
└── File Commander/
    ├── File Commander.csproj
    ├── Program.cs
    ├── Models/        (4 files - DisplayMode, FileItem, TabState, OperationBuffer)
    ├── Services/      (2 files - FileSystemService, FileOperationService)
    ├── Application/   (2 files - TabManager, CommandHandler)
    └── UI/            (2 files - MainWindow, FilePaneView)
```

## Documentation

- **[SDD.md](SDD.md)** - Complete Software Design Document for full replication
- **[USER_GUIDE.md](USER_GUIDE.md)** - Complete feature list with examples
- **[QUICKSTART.md](QUICKSTART.md)** - Quick start guide
- **[PHASE_1.1_UPDATE.md](PHASE_1.1_UPDATE.md)** - Phase 1.1 enhancement details
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Technical overview

## What's New in Phase 1.1

### Enhanced Selection System
- **Space Key**: Toggle mark without moving cursor
- **Insert Key**: Toggle mark and move to next file
- **Shift+Arrow**: Range selection with movement
- Three distinct selection methods for different workflows

### Directory Size Calculation  
- Automatic calculation when directories are marked
- Recursive (includes all subdirectories)
- Asynchronous (UI stays responsive)
- Cached until refresh
- Formatted display (B/KB/MB/GB/TB)

### Complete Documentation
- **SDD.md**: 86-section Software Design Document created
- Full architectural documentation
- Allows complete project replication
- All components specified in detail

## Phase 2 (Upcoming)

- Intelligent Task Queue with drive-aware parallelism
- Dedicated Status Pane for job monitoring
- Staged Operation Buffer (Ctrl+C/X, Paste)
- Directory monitoring (FileSystemWatcher)

## Phase 3 (Future)

- Diff/Sync mode for directory comparison
- Image preview (Sixel/iTerm2)
- Archive support (ZIP, TAR, etc.)
- Optional Vim keybinding mode

## Technical Notes

- **Target Framework:** .NET 8.0
- **TUI Library:** Terminal.Gui 1.19.0
- **Platform:** Linux (primary), Windows (secondary)
- **Pattern:** Layered architecture with decoupled services
- **Async:** File operations use async/await for responsiveness

## Known Issues / MVP Limitations

- Single pane mode shows simplified view (full implementation in Phase 2)
- No preview pane yet (Phase 3)
- No tree view yet (Phase 3)
- File operations are sequential (parallel queue in Phase 2)
- No directory monitoring yet (Phase 2)

## Development

This is the Phase 1 MVP focusing on:
1. ✅ TUI Framework setup
2. ✅ Core navigation (1-pane & 2-pane)
3. ✅ Basic file operations (F5/F6/F8)
4. ✅ TabManager with mode switching
5. ✅ OFM paradigm compliance

Next: Implement Phase 2 (Performance & Decoupling)

