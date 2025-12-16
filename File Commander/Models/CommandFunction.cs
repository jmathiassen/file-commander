namespace File_Commander.Models;

/// <summary>
/// Defines all logical command functions that can be mapped to keys
/// This enables configurable keybindings and decouples UI from command logic
/// </summary>
public enum CommandFunction
{
    // Navigation
    MOVE_CURSOR_UP,
    MOVE_CURSOR_DOWN,
    MOVE_CURSOR_PAGE_UP,
    MOVE_CURSOR_PAGE_DOWN,
    MOVE_CURSOR_HOME,
    MOVE_CURSOR_END,
    ENTER_DIRECTORY,
    PARENT_DIRECTORY,

    // Pane Management
    SWITCH_PANE,
    TOGGLE_DISPLAY_MODE,

    // File Selection
    TOGGLE_MARK_STAY,
    TOGGLE_MARK_AND_MOVE,
    TOGGLE_MARK_AND_MOVE_UP,
    TOGGLE_MARK_AND_MOVE_DOWN,
    MARK_ALL,
    UNMARK_ALL,
    INVERT_SELECTION,

    // File Operations
    STAGE_COPY,
    STAGE_MOVE,
    EXECUTE_PASTE,
    DELETE_FILES,
    CREATE_DIRECTORY,
    RENAME_FILE,

    // Immediate Operations (Dual-Pane)
    COPY_TO_OPPOSITE,
    MOVE_TO_OPPOSITE,

    // View Operations
    VIEW_FILE,
    EDIT_FILE,
    REFRESH_PANE,
    REFRESH_BOTH_PANES,

    // Tab Management
    CREATE_NEW_TAB,
    CLOSE_CURRENT_TAB,
    SWITCH_TAB_NEXT,
    SWITCH_TAB_PREVIOUS,
    SWITCH_TO_TAB_1,
    SWITCH_TO_TAB_2,
    SWITCH_TO_TAB_3,
    SWITCH_TO_TAB_4,
    SWITCH_TO_TAB_5,
    SWITCH_TO_TAB_6,
    SWITCH_TO_TAB_7,
    SWITCH_TO_TAB_8,
    SWITCH_TO_TAB_9,

    // Diff/Sync Mode
    TOGGLE_DIFF_SYNC_MODE,
    EXECUTE_SYNC,
    SWAP_DIFF_PANES,

    // Status Pane
    TOGGLE_STATUS_PANE_SIZE,
    SWITCH_STATUS_TAB,

    // Application
    QUIT_APPLICATION,
    SHOW_HELP,

    // Options and Configuration
    SHOW_OPTIONS,
    CALCULATE_SIZE,

    // Pane Management
    INCREASE_LEFT_PANE,
    DECREASE_LEFT_PANE,
    RESET_PANE_SPLIT,

    // Queue Control
    PAUSE_QUEUE,
    RESUME_QUEUE,
    CLEAR_QUEUE,

    // Enum management
    NONE,
    UNKNOWN
}

