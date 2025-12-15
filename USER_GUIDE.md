# File Commander - Complete Feature List & User Guide

**Version:** 1.1 (Phase 1.1 Complete)  
**Date:** December 15, 2025

---

## 🎯 Quick Start

```bash
# Run File Commander
cd "/home/jmathias/RiderProjects/File Commander"
./fcom.sh

# Or specify starting directory
./fcom.sh /path/to/directory
```

---

## 📋 Complete Feature List

### ✅ Core Navigation
- **Dual-Pane Layout**: Active and passive panes (OFM standard)
- **Single-Pane Mode**: Toggle with F9 (simplified in MVP)
- **Directory Navigation**: Enter to descend, Backspace to parent
- **Arrow Keys**: Up/down to move selection
- **Tab Key**: Switch between left and right panes
- **Parent Directory**: Automatic ".." entry in each directory

### ✅ File Selection System (Phase 1.1 - NEW!)

#### Three Selection Methods:

**1. Space - Toggle Mark (Stay)**
- Press `Space` to mark/unmark current file
- Cursor stays on the file for review
- Best for: Careful, precise selection

**2. Insert - Toggle Mark (Move Down)**
- Press `Insert` to mark/unmark and move to next file
- Automatically advances cursor
- Best for: Quick sequential marking

**3. Shift+Arrow - Range Selection**
- Hold `Shift` + press `↑` or `↓`
- Marks files while moving cursor
- Best for: Selecting ranges of files

#### Selection Indicators:
- Marked files show `*` prefix
- Active pane has colored border
- Current selection highlighted
- Marked count shown in status bar

### ✅ File Operations (OFM Standard)

**F5 - Copy**
- Copies marked files OR current selection
- From active pane to passive pane
- Shows confirmation dialog
- Displays progress during operation
- Recursive for directories

**F6 - Move**
- Moves marked files OR current selection
- From active pane to passive pane
- Shows confirmation dialog
- Can rename if same directory
- Removes source after success

**F7 - Make Directory**
- Creates new directory in active pane
- Shows dialog for name input
- Validates directory name
- Creates immediately

**F8 - Delete**
- Deletes marked files OR current selection
- Shows WARNING confirmation
- Recursive for directories
- Permanent deletion (no trash)

### ✅ Directory Size Calculation (Phase 1.1 - NEW!)
- Automatic when directory is marked
- Recursive calculation (all subdirectories)
- Asynchronous (non-blocking UI)
- Cached until refresh
- Displays formatted size (B/KB/MB/GB/TB)
- Handles permission errors gracefully

### ✅ Display Features
- **File Icons**: 📁 for directories, 📄 for files
- **Size Formatting**: Automatic KB/MB/GB conversion
- **Date Display**: YYYY-MM-DD HH:MM format
- **Path Display**: Full path in pane title
- **Status Bar**: Current operation status
- **Help Bar**: Key binding reference

### ✅ User Interface
- **Dual-Pane View**: Side-by-side file lists
- **Active/Passive Indicator**: Color-coded borders
- **Status Messages**: Real-time feedback
- **Confirmation Dialogs**: Safe operations
- **Progress Updates**: During file operations

### ✅ Error Handling
- **Permission Denied**: Gracefully skipped with message
- **File Not Found**: Continues with next file
- **Disk Full**: Aborts with clear error
- **Access Errors**: Displays user-friendly messages
- **Invalid Paths**: Falls back to current directory

---

## ⌨️ Complete Keyboard Reference

### File Operations
| Key | Action | Requires Confirmation | Works with Marked Files |
|-----|--------|----------------------|------------------------|
| **F5** | Copy to opposite pane | Yes | Yes |
| **F6** | Move to opposite pane | Yes | Yes |
| **F7** | Create directory | Dialog | N/A |
| **F8** | Delete files | Yes (WARNING) | Yes |

### Navigation
| Key | Action | Context |
|-----|--------|---------|
| **↑** | Move selection up | Always |
| **↓** | Move selection down | Always |
| **Enter** | Open directory / Show file info | On item |
| **Backspace** | Go to parent directory | Always |
| **Tab** | Switch active pane | Dual-pane mode |
| **F9** | Toggle Single/Dual pane | Always |
| **F10** | Quit application | Always |

### File Selection (NEW!)
| Key | Action | Cursor Movement | Use Case |
|-----|--------|-----------------|----------|
| **Space** | Toggle mark | Stays | Precise selection |
| **Insert** | Toggle mark | Moves down | Sequential selection |
| **Shift+↑** | Toggle mark | Moves up | Range selection up |
| **Shift+↓** | Toggle mark | Moves down | Range selection down |

---

## 📚 Usage Workflows

### Workflow 1: Copy Files Between Directories

```
1. Navigate to source directory (Enter/Backspace)
2. Mark files to copy:
   - For range: Hold Shift + press ↓ repeatedly
   - For specific: Press Space on each file
   - For sequence: Press Insert repeatedly
3. Press Tab to switch to other pane
4. Navigate to destination directory
5. Press Tab to return to source pane
6. Press F5 (Copy)
7. Confirm in dialog (press Tab to Yes, Enter)
8. Wait for completion
```

### Workflow 2: Move Files to Another Location

```
1. Navigate to source directory
2. Mark files (Space/Insert/Shift+Arrow)
3. Ensure other pane shows destination
4. Press F6 (Move)
5. Confirm operation
6. Files moved, marked files cleared
```

### Workflow 3: Delete Old Files

```
1. Navigate to directory
2. Mark files to delete:
   - Shift+↓ for ranges
   - Space for specific files
3. Press F8 (Delete)
4. Read WARNING carefully
5. Confirm deletion
6. Files permanently deleted
```

### Workflow 4: Check Directory Size Before Copying

```
1. Navigate to directory
2. Press Space to mark it
3. Wait for size calculation (async)
4. Check displayed size
5. If acceptable:
   - Press F5 to copy
6. If too large:
   - Press Space to unmark
   - Navigate elsewhere
```

### Workflow 5: Organize Files

```
1. Left pane: Source directory
2. Right pane: Destination directory (Tab, navigate)
3. Mark files in left pane:
   - Documents: Mark with Space
   - Images: Mark with Insert
   - Archives: Mark with Shift+↓
4. Press F6 to move marked files
5. Repeat for different file types
```

---

## 🎯 Selection Best Practices

### When to Use Each Method

**Space (Stay in Place)**
- ✅ Reviewing files one by one
- ✅ Non-sequential selection
- ✅ Need to examine before marking
- ❌ Not for large ranges

**Insert (Move Down)**
- ✅ Marking files as you scan
- ✅ Sequential marking
- ✅ Quick batch selection
- ❌ Not when you need to skip files

**Shift+Arrow (Range)**
- ✅ Large ranges of files
- ✅ Consecutive files
- ✅ Fastest for bulk selection
- ❌ Not for scattered files

### Selection Tips

1. **Combine Methods**: Use Shift+↓ for ranges, Space for gaps
2. **Review Before Acting**: Check marked count in status
3. **Use Directory Sizes**: Know what you're copying
4. **Unmark All**: Navigate away and back, or manually unmark
5. **Mark Persists**: Marked files stay marked when switching panes

---

## 📊 File Display Format

```
[*] [Icon] [Filename (40 chars)]  [Size (12 chars)]  [Date]
 │    │                              │                  │
 │    │                              │                  └─ YYYY-MM-DD HH:MM
 │    │                              └─ Auto-formatted (B/KB/MB/GB/TB)
 │    └─ 📁 for directories, 📄 for files
 └─ Asterisk if marked
```

**Example Display:**
```
* 📁 Documents          125.5 MB  2025-12-15 10:30
  📄 report.pdf           1.5 MB  2025-12-15 09:15
* 📄 notes.txt            5.2 KB  2025-12-14 16:45
  📁 Archive             <DIR>    2025-12-10 14:20
```

---

## 🔧 Advanced Features

### Directory Size Calculation

**How It Works:**
1. Mark a directory (Space/Insert/Shift+Arrow)
2. Calculation starts automatically in background
3. UI remains fully responsive
4. Size appears when calculation completes
5. Cached until directory is refreshed

**What It Calculates:**
- All files in directory
- All files in subdirectories (recursive)
- Total size in appropriate unit

**Limitations:**
- Skips inaccessible subdirectories
- May take time for large directories
- Shows progress in status bar

### Multi-Tab Support (Foundation)

**Current State:**
- Single tab active in Phase 1
- TabManager supports multiple tabs
- Tab switching infrastructure ready

**Future (Phase 2):**
- Multiple independent tabs
- Quick tab switching
- Per-tab state preservation

---

## 🐛 Known Limitations (Phase 1.1)

1. **Single Pane Mode**: Simplified implementation
   - Shows placeholder
   - Full tree+preview in Phase 3

2. **No File Preview**: F3 key reserved
   - Will show file preview in Phase 3
   - Text, images, hex view planned

3. **Sequential Operations**: One at a time
   - No parallel execution yet
   - Intelligent queue in Phase 2

4. **Manual Refresh**: No auto-refresh
   - FileSystemWatcher in Phase 2
   - Must refresh manually (navigate away/back)

5. **No Search**: Not implemented
   - File search in Phase 3
   - Content search planned

---

## 💡 Tips & Tricks

### Efficiency Tips

1. **Use Shift+Arrow for Ranges**
   - Fastest way to select many files
   - Hold Shift, press arrow keys

2. **Space for Review**
   - When unsure about files
   - Cursor stays for examination

3. **Insert for Quick Batch**
   - Scanning and marking together
   - Natural workflow

4. **Check Sizes First**
   - Mark directories before copying
   - Avoid disk space surprises

5. **Tab is Your Friend**
   - Quick pane switching
   - Set up source and destination

### Keyboard Memory Aids

**F-Keys = File Operations**
- F5: Copy (5 = "Copy" mnemonic)
- F6: Move (6 = "Six" sounds like "fix")
- F7: Make (7 = lucky, creating)
- F8: Delete (8 = "ate" it)

**Selection Mnemonics**
- Space = Stay (both start with 'S')
- Insert = Insert yourself down the list
- Shift = Shift through a range

---

## 🎓 Example Scenarios

### Scenario 1: Backup Important Documents

```
Goal: Copy all .pdf files to backup drive

Steps:
1. Left pane: ~/Documents
2. Right pane: /media/backup/docs (Tab, navigate)
3. In left pane, mark all PDFs:
   - Navigate to first PDF
   - Press Insert to mark and advance
   - Repeat for all PDFs
4. Press F5 (Copy)
5. Confirm
6. Done!
```

### Scenario 2: Clean Up Downloads Folder

```
Goal: Delete old files from Downloads

Steps:
1. Navigate to ~/Downloads
2. Sort by date (manually scan)
3. Mark old files:
   - Use Shift+↓ for ranges of old files
   - Use Space to skip files to keep
4. Check marked count in status
5. Press F8 (Delete)
6. Carefully confirm WARNING
7. Old files deleted
```

### Scenario 3: Organize Photos

```
Goal: Move photos to dated folders

Steps:
1. Left pane: ~/Pictures/unsorted
2. Right pane: ~/Pictures/2025-12 (create with F7)
3. In left pane, mark December photos:
   - Use Shift+↓ for ranges
   - Use Space for scattered photos
4. Press F6 (Move)
5. Photos moved to dated folder
6. Repeat for other months
```

### Scenario 4: Check Project Size

```
Goal: See how large project directory is

Steps:
1. Navigate to projects directory
2. Press Space on project folder
3. Wait for size calculation
4. Size displays next to folder name
5. Decide if backup needed
6. Press F5 to copy if needed
```

---

## 📖 Reference Tables

### File Size Units

| Unit | Value | Example |
|------|-------|---------|
| **B** | Bytes | 256 B |
| **KB** | 1024 B | 5.2 KB |
| **MB** | 1024 KB | 125.5 MB |
| **GB** | 1024 MB | 2.3 GB |
| **TB** | 1024 GB | 1.5 TB |

### Operation Results

| Operation | Source | Destination | Source After | Destination After |
|-----------|--------|-------------|--------------|-------------------|
| **Copy** | Exists | New copy | Unchanged | Copy created |
| **Move** | Exists | New item | Deleted | Item created |
| **Delete** | Exists | N/A | Deleted | N/A |

---

## 🚀 Getting Started Checklist

- [ ] Read QUICKSTART.md
- [ ] Launch: `./fcom.sh`
- [ ] Practice navigation (arrows, Enter, Backspace)
- [ ] Try Space key (mark without moving)
- [ ] Try Insert key (mark and move)
- [ ] Try Shift+↓ (range selection)
- [ ] Mark a directory (see size calculation)
- [ ] Switch panes with Tab
- [ ] Try F5 (copy some files)
- [ ] Try F7 (create directory)
- [ ] Try F8 (delete test files)
- [ ] Read SDD.md for architecture
- [ ] Explore advanced workflows

---

## 📞 Documentation Quick Links

- **QUICKSTART.md** - Quick start guide
- **README.md** - Full user manual
- **SDD.md** - Software Design Document (complete replication)
- **PHASE_1.1_UPDATE.md** - New features details
- **IMPLEMENTATION_SUMMARY.md** - Technical summary

---

## ✅ Verification Steps

After installation, verify all features:

```bash
# 1. Launch
./fcom.sh

# 2. Test Navigation
- Press arrows (move selection)
- Press Enter (open directory)
- Press Backspace (parent)
- Press Tab (switch panes)

# 3. Test Selection
- Press Space (mark, stay)
- Press Insert (mark, move)
- Hold Shift, press ↓ (range)

# 4. Test Directory Size
- Navigate to directory
- Press Space to mark
- Wait for size display

# 5. Test Operations
- Mark some files
- Press F5 (copy)
- Confirm
- Verify copy succeeded

# 6. Test Mode Toggle
- Press F9 (toggle mode)
- Press F9 again (back to dual)

# 7. Exit
- Press F10 (quit)
```

---

## 🎊 You're Ready!

File Commander is now fully operational with:

✅ Dual-pane file browsing  
✅ Three selection methods (Space/Insert/Shift+Arrow)  
✅ Directory size calculation  
✅ Copy/Move/Delete operations  
✅ Directory creation  
✅ Full keyboard control  
✅ Professional documentation

**Start managing files efficiently!**

```bash
./fcom.sh
```

Happy file commanding! 🚀

