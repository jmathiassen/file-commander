using Terminal.Gui;
using File_Commander.Models;
using static File_Commander.Models.CommandFunction;

namespace File_Commander.Services;

/// <summary>
/// Service for mapping Terminal.Gui.Key events to CommandFunction actions.
/// This decouples input from command execution logic.
/// Phase 2: Core configurable keymap implementation
/// </summary>
public class KeymapService
{
    private readonly Dictionary<Key, CommandFunction> _keymap = new();

    public KeymapService()
    {
        SetDefaults();
    }

    private void SetDefaults()
    {
        // --- Navigation ---
        _keymap[Key.CursorUp] = MOVE_CURSOR_UP;
        _keymap[Key.CursorDown] = MOVE_CURSOR_DOWN;
        _keymap[Key.Home] = MOVE_CURSOR_HOME;
        _keymap[Key.End] = MOVE_CURSOR_END;
        _keymap[Key.PageUp] = MOVE_CURSOR_PAGE_UP;
        _keymap[Key.PageDown] = MOVE_CURSOR_PAGE_DOWN;
        _keymap[Key.Enter] = ENTER_DIRECTORY;
        _keymap[Key.Backspace] = PARENT_DIRECTORY;
        _keymap[Key.Tab] = SWITCH_PANE;
        _keymap[Key.F9] = TOGGLE_DISPLAY_MODE;

        // --- File Operations (OFM Standard & Staging) ---
        _keymap[Key.F5] = STAGE_COPY;
        _keymap[Key.F6] = STAGE_MOVE;
        _keymap[Key.F7] = CREATE_DIRECTORY;
        _keymap[Key.F8] = DELETE_FILES;

        // --- Staged Execution (Paste) ---
        _keymap[(Key)'c' | Key.CtrlMask] = STAGE_COPY;
        _keymap[(Key)'x' | Key.CtrlMask] = STAGE_MOVE;
        _keymap[(Key)'v' | Key.CtrlMask] = EXECUTE_PASTE;

        // --- Selection ---
        _keymap[Key.Space] = TOGGLE_MARK_STAY;
        _keymap[Key.InsertChar] = TOGGLE_MARK_AND_MOVE;
        _keymap[Key.CursorUp | Key.ShiftMask] = TOGGLE_MARK_AND_MOVE_UP;
        _keymap[Key.CursorDown | Key.ShiftMask] = TOGGLE_MARK_AND_MOVE_DOWN;
        _keymap[(Key)'+'] = MARK_ALL;
        _keymap[(Key)'-'] = UNMARK_ALL;
        _keymap[(Key)'*'] = INVERT_SELECTION;

        // --- Tab Management ---
        _keymap[(Key)'t' | Key.CtrlMask] = CREATE_NEW_TAB;
        _keymap[(Key)'w' | Key.CtrlMask] = CLOSE_CURRENT_TAB;
        _keymap[Key.PageUp | Key.CtrlMask] = SWITCH_TAB_PREVIOUS;
        _keymap[Key.PageDown | Key.CtrlMask] = SWITCH_TAB_NEXT;
        _keymap[Key.Tab | Key.CtrlMask] = SWITCH_TAB_NEXT;
        _keymap[Key.Tab | Key.ShiftMask | Key.CtrlMask] = SWITCH_TAB_PREVIOUS;

        // --- Numbered Tab Switching (Alt+1 through Alt+9) ---
        for (int i = 1; i <= 9; i++)
        {
            // Maps Alt+1 to SWITCH_TO_TAB_1, etc.
            var altNumber = (Key)('0' + i) | Key.AltMask;
            _keymap[altNumber] = SWITCH_TO_TAB_1 + (i - 1);
        }

        // --- View Operations ---
        _keymap[Key.F3] = VIEW_FILE;
        _keymap[Key.F4] = EDIT_FILE;
        _keymap[Key.F5 | Key.CtrlMask] = REFRESH_PANE;
        _keymap[(Key)'r' | Key.CtrlMask] = REFRESH_BOTH_PANES;

        // --- Status/App ---
        _keymap[(Key)'z' | Key.CtrlMask] = TOGGLE_STATUS_PANE_SIZE;
        _keymap[(Key)'i' | Key.CtrlMask] = SWITCH_STATUS_TAB;
        _keymap[(Key)'q' | Key.CtrlMask] = QUIT_APPLICATION;
        _keymap[Key.F10] = QUIT_APPLICATION;
        _keymap[Key.F1] = SHOW_HELP;

        // --- Diff/Sync Mode ---
        _keymap[Key.F11] = TOGGLE_DIFF_SYNC_MODE;
        _keymap[Key.F12] = EXECUTE_SYNC;
        _keymap[(Key)'s' | Key.CtrlMask] = SWAP_DIFF_PANES;

        // --- Options and Configuration ---
        _keymap[(Key)'o' | Key.CtrlMask] = SHOW_OPTIONS;
        _keymap[Key.F2] = CALCULATE_SIZE;  // F2 or Space to calculate directory size

        // --- Pane Management ---
        _keymap[(Key)'+' | Key.CtrlMask] = INCREASE_LEFT_PANE;
        _keymap[(Key)'-' | Key.CtrlMask] = DECREASE_LEFT_PANE;
        _keymap[(Key)'=' | Key.CtrlMask] = RESET_PANE_SPLIT;

        // --- Queue Control ---
        _keymap[(Key)'p' | Key.CtrlMask] = PAUSE_QUEUE;
        _keymap[(Key)'r' | Key.CtrlMask] = RESUME_QUEUE;
        _keymap[Key.DeleteChar | Key.CtrlMask] = CLEAR_QUEUE;
    }

    /// <summary>
    /// Resolves a key to its mapped command function
    /// </summary>
    public CommandFunction Resolve(Key key)
    {
        if (_keymap.TryGetValue(key, out var function))
        {
            return function;
        }
        return CommandFunction.UNKNOWN;
    }

    /// <summary>
    /// Gets a human-readable description of a command function
    /// </summary>
    public string GetDescription(CommandFunction function)
    {
        return function switch
        {
            MOVE_CURSOR_UP => "Move cursor up",
            MOVE_CURSOR_DOWN => "Move cursor down",
            MOVE_CURSOR_PAGE_UP => "Page up",
            MOVE_CURSOR_PAGE_DOWN => "Page down",
            MOVE_CURSOR_HOME => "Jump to first item",
            MOVE_CURSOR_END => "Jump to last item",
            ENTER_DIRECTORY => "Enter directory / Open file",
            PARENT_DIRECTORY => "Go to parent directory",
            SWITCH_PANE => "Switch active pane",
            TOGGLE_DISPLAY_MODE => "Toggle single/dual pane mode",
            TOGGLE_MARK_STAY => "Toggle mark (stay)",
            TOGGLE_MARK_AND_MOVE => "Toggle mark (move down)",
            TOGGLE_MARK_AND_MOVE_UP => "Toggle mark (move up)",
            TOGGLE_MARK_AND_MOVE_DOWN => "Toggle mark (move down)",
            MARK_ALL => "Mark all files",
            UNMARK_ALL => "Unmark all files",
            STAGE_COPY => "Stage copy operation",
            STAGE_MOVE => "Stage move operation",
            EXECUTE_PASTE => "Execute staged operation",
            DELETE_FILES => "Delete files",
            CREATE_DIRECTORY => "Create directory",
            CREATE_NEW_TAB => "Create new tab",
            CLOSE_CURRENT_TAB => "Close current tab",
            SWITCH_TAB_NEXT => "Next tab",
            SWITCH_TAB_PREVIOUS => "Previous tab",
            TOGGLE_STATUS_PANE_SIZE => "Toggle status pane size",
            SWITCH_STATUS_TAB => "Switch status tab",
            QUIT_APPLICATION => "Quit application",
            SHOW_HELP => "Show help",
            VIEW_FILE => "View file",
            EDIT_FILE => "Edit file",
            REFRESH_PANE => "Refresh active pane",
            REFRESH_BOTH_PANES => "Refresh both panes",
            TOGGLE_DIFF_SYNC_MODE => "Toggle diff/sync mode",
            EXECUTE_SYNC => "Execute sync",
            _ => function.ToString()
        };
    }

    /// <summary>
    /// Gets the key(s) mapped to a command function
    /// </summary>
    public List<Key> GetKeysForFunction(CommandFunction function)
    {
        return _keymap
            .Where(kvp => kvp.Value == function)
            .Select(kvp => kvp.Key)
            .ToList();
    }
}

