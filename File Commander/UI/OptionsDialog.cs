using Terminal.Gui;
using File_Commander.Models;

namespace File_Commander.UI;

/// <summary>
/// Options/Preferences dialog for configuring application settings
/// </summary>
public class OptionsDialog : Dialog
{
    private readonly AppSettings _settings;
    private TabView _tabView = null!;

    // General tab controls
    private RadioGroup _updateModeRadio = null!;
    private RadioGroup _fileSizeFormatRadio = null!;
    private CheckBox _showSecondsCheckBox = null!;
    private CheckBox _showHiddenFilesCheckBox = null!;
    private CheckBox _followSymlinksCheckBox = null!;
    private CheckBox _showFileIconsCheckBox = null!;
    private CheckBox _useNarrowIconsCheckBox = null!;
    private CheckBox _showExtensionsInColumnCheckBox = null!;
    private CheckBox _autoCalculateSizeCheckBox = null!;
    private CheckBox _autoStartQueueCheckBox = null!;

    // Key bindings tab controls
    private ListView _commandListView = null!;
    private ListView _keyListView = null!;
    private Button _addKeyButton = null!;
    private Button _removeKeyButton = null!;
    private string _selectedCommand = string.Empty;

    // Colors tab controls
    private Dictionary<string, RadioGroup> _colorSelectors = new();

    public bool WasSaved { get; private set; }

    public OptionsDialog(AppSettings currentSettings) : base("Options")
    {
        _settings = new AppSettings
        {
            DirectoryUpdateMode = currentSettings.DirectoryUpdateMode,
            ShowSecondsInDate = currentSettings.ShowSecondsInDate,
            ShowHiddenFiles = currentSettings.ShowHiddenFiles,
            FollowSymlinks = currentSettings.FollowSymlinks,
            ShowFileIcons = currentSettings.ShowFileIcons,
            UseNarrowIcons = currentSettings.UseNarrowIcons,
            ShowExtensionsInColumn = currentSettings.ShowExtensionsInColumn,
            AutoCalculateDirectorySize = currentSettings.AutoCalculateDirectorySize,
            AutoStartQueue = currentSettings.AutoStartQueue,
            FileSizeFormat = currentSettings.FileSizeFormat,
            KeyBindings = new Dictionary<string, KeyBinding>(currentSettings.KeyBindings
                .ToDictionary(kv => kv.Key, kv => new KeyBinding(kv.Value.Command, kv.Value.DisplayName, kv.Value.Keys.ToArray()))),
            ColorScheme = new ColorSchemeSettings
            {
                NormalBackground = currentSettings.ColorScheme.NormalBackground,
                NormalForeground = currentSettings.ColorScheme.NormalForeground,
                SelectedBackground = currentSettings.ColorScheme.SelectedBackground,
                SelectedForeground = currentSettings.ColorScheme.SelectedForeground,
                DirectoryForeground = currentSettings.ColorScheme.DirectoryForeground,
                ExecutableForeground = currentSettings.ColorScheme.ExecutableForeground,
                MarkedForeground = currentSettings.ColorScheme.MarkedForeground,
                InactivePaneForeground = currentSettings.ColorScheme.InactivePaneForeground
            }
        };

        Width = 80;
        Height = 30;

        InitializeUI();
    }

    private void InitializeUI()
    {
        _tabView = new TabView()
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3)
        };

        // Create tabs
        var generalTab = new TabView.Tab("General", CreateGeneralTab());
        var keysTab = new TabView.Tab("Key Bindings", CreateKeyBindingsTab());
        var colorsTab = new TabView.Tab("Colors", CreateColorsTab());

        _tabView.AddTab(generalTab, false);
        _tabView.AddTab(keysTab, false);
        _tabView.AddTab(colorsTab, false);

        Add(_tabView);

        // Buttons
        var okButton = new Button("OK")
        {
            X = Pos.Center() - 10,
            Y = Pos.AnchorEnd(1),
            IsDefault = true
        };
        okButton.Clicked += OnOkClicked;
        Add(okButton);

        var cancelButton = new Button("Cancel")
        {
            X = Pos.Center() + 2,
            Y = Pos.AnchorEnd(1)
        };
        cancelButton.Clicked += () => Terminal.Gui.Application.RequestStop();
        Add(cancelButton);
    }

    private View CreateGeneralTab()
    {
        var view = new View()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var y = 1;

        // Directory Update Mode section
        view.Add(new Label(1, y++, "Directory Update Mode:"));

        _updateModeRadio = new RadioGroup()
        {
            X = 3,
            Y = y,
            RadioLabels =
            [
                "Manual only (F5 to refresh)",
                "Automatic for active tab only",
                "Automatic for all tabs"
            ],
            SelectedItem = (int)_settings.DirectoryUpdateMode
        };
        view.Add(_updateModeRadio);
        y += 4;

        // Display Options section
        view.Add(new Label(1, y++, "Display Options:"));

        _showSecondsCheckBox = new CheckBox(3, y++, "Show seconds in file dates")
        {
            Checked = _settings.ShowSecondsInDate
        };
        view.Add(_showSecondsCheckBox);

        _showFileIconsCheckBox = new CheckBox(3, y++, "Show file/directory icons (D/F)")
        {
            Checked = _settings.ShowFileIcons
        };
        view.Add(_showFileIconsCheckBox);

        _useNarrowIconsCheckBox = new CheckBox(3, y++, "Use narrow icons (fixes alignment)")
        {
            Checked = _settings.UseNarrowIcons
        };
        view.Add(_useNarrowIconsCheckBox);

        _showExtensionsInColumnCheckBox = new CheckBox(3, y++, "Show extensions in separate column")
        {
            Checked = _settings.ShowExtensionsInColumn
        };
        view.Add(_showExtensionsInColumnCheckBox);

        _showHiddenFilesCheckBox = new CheckBox(3, y++, "Show hidden files")
        {
            Checked = _settings.ShowHiddenFiles
        };
        view.Add(_showHiddenFilesCheckBox);

        y++;

        // File Size Format section
        view.Add(new Label(1, y++, "File Size Format:"));

        _fileSizeFormatRadio = new RadioGroup()
        {
            X = 3,
            Y = y,
            RadioLabels =
            [
                "Bytes (with ' separator)",
                "KB/MB/GB (1000-based)",
                "KiB/MiB/GiB (1024-based)"
            ],
            SelectedItem = (int)_settings.FileSizeFormat
        };
        view.Add(_fileSizeFormatRadio);
        y += 4;

        // File Operations section
        view.Add(new Label(1, y++, "File Operations:"));

        _autoCalculateSizeCheckBox = new CheckBox(3, y++, "Auto-calculate directory size when marking")
        {
            Checked = _settings.AutoCalculateDirectorySize
        };
        view.Add(_autoCalculateSizeCheckBox);

        _autoStartQueueCheckBox = new CheckBox(3, y++, "Auto-start queue (uncheck for manual queue start)")
        {
            Checked = _settings.AutoStartQueue
        };
        view.Add(_autoStartQueueCheckBox);

        _followSymlinksCheckBox = new CheckBox(3, y++, "Follow symbolic links")
        {
            Checked = _settings.FollowSymlinks
        };
        view.Add(_followSymlinksCheckBox);

        return view;
    }

    private View CreateKeyBindingsTab()
    {
        var view = new View()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        view.Add(new Label(1, 1, "Command:"));
        view.Add(new Label(40, 1, "Assigned Keys:"));

        // Command list
        _commandListView = new ListView()
        {
            X = 1,
            Y = 2,
            Width = 37,
            Height = Dim.Fill(3),
            AllowsMarking = false
        };

        var commands = _settings.KeyBindings.Values
            .OrderBy(kb => kb.DisplayName)
            .Select(kb => kb.DisplayName)
            .ToList();
        _commandListView.SetSource(commands);
        _commandListView.SelectedItemChanged += OnCommandSelected;
        view.Add(_commandListView);

        // Keys list
        _keyListView = new ListView()
        {
            X = 40,
            Y = 2,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3),
            AllowsMarking = false
        };
        view.Add(_keyListView);

        // Add key button
        _addKeyButton = new Button("Add Key...")
        {
            X = 1,
            Y = Pos.AnchorEnd(1)
        };
        _addKeyButton.Clicked += OnAddKey;
        view.Add(_addKeyButton);

        // Remove key button
        _removeKeyButton = new Button("Remove Key")
        {
            X = 15,
            Y = Pos.AnchorEnd(1),
            Enabled = false
        };
        _removeKeyButton.Clicked += OnRemoveKey;
        view.Add(_removeKeyButton);

        return view;
    }

    private View CreateColorsTab()
    {
        var view = new View()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var y = 1;
        var colorNames = Enum.GetNames(typeof(ColorName)).Select(n => (NStack.ustring)n).ToArray();

        // Helper to create color selector
        RadioGroup CreateColorSelector(string label, ColorName current, int yPos)
        {
            view.Add(new Label(1, yPos, label));
            var radio = new RadioGroup()
            {
                X = 30,
                Y = yPos,
                RadioLabels = colorNames,
                SelectedItem = (int)current,
                DisplayMode = DisplayModeLayout.Horizontal
            };
            return radio;
        }

        view.Add(new Label(1, y++, "File List Colors:"));
        y++;

        _colorSelectors["NormalBg"] = CreateColorSelector("Normal Background:", _settings.ColorScheme.NormalBackground, y++);
        view.Add(_colorSelectors["NormalBg"]);
        y++;

        _colorSelectors["NormalFg"] = CreateColorSelector("Normal Foreground:", _settings.ColorScheme.NormalForeground, y++);
        view.Add(_colorSelectors["NormalFg"]);
        y++;

        _colorSelectors["SelectedBg"] = CreateColorSelector("Selected Background:", _settings.ColorScheme.SelectedBackground, y++);
        view.Add(_colorSelectors["SelectedBg"]);
        y++;

        _colorSelectors["SelectedFg"] = CreateColorSelector("Selected Foreground:", _settings.ColorScheme.SelectedForeground, y++);
        view.Add(_colorSelectors["SelectedFg"]);
        y++;

        _colorSelectors["Directory"] = CreateColorSelector("Directory:", _settings.ColorScheme.DirectoryForeground, y++);
        view.Add(_colorSelectors["Directory"]);
        y++;

        _colorSelectors["Executable"] = CreateColorSelector("Executable:", _settings.ColorScheme.ExecutableForeground, y++);
        view.Add(_colorSelectors["Executable"]);
        y++;

        _colorSelectors["Marked"] = CreateColorSelector("Marked Files:", _settings.ColorScheme.MarkedForeground, y++);
        view.Add(_colorSelectors["Marked"]);
        y++;

        _colorSelectors["Inactive"] = CreateColorSelector("Inactive Pane:", _settings.ColorScheme.InactivePaneForeground, y++);
        view.Add(_colorSelectors["Inactive"]);

        return view;
    }

    private void OnCommandSelected(ListViewItemEventArgs args)
    {
        var displayName = args.Value.ToString();
        var binding = _settings.KeyBindings.Values.FirstOrDefault(kb => kb.DisplayName == displayName);

        if (binding != null)
        {
            _selectedCommand = binding.Command;
            _keyListView.SetSource(binding.Keys);
            _removeKeyButton.Enabled = binding.Keys.Count > 0;
        }
    }

    private void OnAddKey()
    {
        if (string.IsNullOrEmpty(_selectedCommand))
            return;

        var dialog = new Dialog("Add Key Binding", 50, 10);

        var label = new Label(1, 1, "Press key combination (F5, Ctrl+O, Shift+F5):");
        dialog.Add(label);

        var textField = new TextField()
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(1)
        };
        dialog.Add(textField);

        var okBtn = new Button("OK")
        {
            X = Pos.Center() - 8,
            Y = Pos.AnchorEnd(2),
            IsDefault = true
        };
        okBtn.Clicked += () =>
        {
            var key = textField.Text.ToString()?.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                var binding = _settings.KeyBindings.Values.First(kb => kb.Command == _selectedCommand);
                if (!binding.Keys.Contains(key))
                {
                    binding.Keys.Add(key);
                    _keyListView.SetSource(binding.Keys);
                    _removeKeyButton.Enabled = true;
                }
            }
            Terminal.Gui.Application.RequestStop();
        };
        dialog.Add(okBtn);

        var cancelBtn = new Button("Cancel")
        {
            X = Pos.Center() + 2,
            Y = Pos.AnchorEnd(2)
        };
        cancelBtn.Clicked += () => Terminal.Gui.Application.RequestStop();
        dialog.Add(cancelBtn);

        Terminal.Gui.Application.Run(dialog);
    }

    private void OnRemoveKey()
    {
        if (string.IsNullOrEmpty(_selectedCommand))
            return;

        var selectedIdx = _keyListView.SelectedItem;
        if (selectedIdx < 0)
            return;

        var binding = _settings.KeyBindings.Values.First(kb => kb.Command == _selectedCommand);
        if (selectedIdx < binding.Keys.Count)
        {
            binding.Keys.RemoveAt(selectedIdx);
            _keyListView.SetSource(binding.Keys);
            _removeKeyButton.Enabled = binding.Keys.Count > 0;
        }
    }

    private void OnOkClicked()
    {
        // Update settings from General tab
        _settings.DirectoryUpdateMode = (DirectoryUpdateMode)_updateModeRadio.SelectedItem;
        _settings.ShowSecondsInDate = _showSecondsCheckBox.Checked;
        _settings.ShowHiddenFiles = _showHiddenFilesCheckBox.Checked;
        _settings.FollowSymlinks = _followSymlinksCheckBox.Checked;
        _settings.ShowFileIcons = _showFileIconsCheckBox.Checked;
        _settings.UseNarrowIcons = _useNarrowIconsCheckBox.Checked;
        _settings.ShowExtensionsInColumn = _showExtensionsInColumnCheckBox.Checked;
        _settings.AutoCalculateDirectorySize = _autoCalculateSizeCheckBox.Checked;
        _settings.AutoStartQueue = _autoStartQueueCheckBox.Checked;
        _settings.FileSizeFormat = (FileSizeFormat)_fileSizeFormatRadio.SelectedItem;

        // Update colors from Colors tab
        _settings.ColorScheme.NormalBackground = (ColorName)_colorSelectors["NormalBg"].SelectedItem;
        _settings.ColorScheme.NormalForeground = (ColorName)_colorSelectors["NormalFg"].SelectedItem;
        _settings.ColorScheme.SelectedBackground = (ColorName)_colorSelectors["SelectedBg"].SelectedItem;
        _settings.ColorScheme.SelectedForeground = (ColorName)_colorSelectors["SelectedFg"].SelectedItem;
        _settings.ColorScheme.DirectoryForeground = (ColorName)_colorSelectors["Directory"].SelectedItem;
        _settings.ColorScheme.ExecutableForeground = (ColorName)_colorSelectors["Executable"].SelectedItem;
        _settings.ColorScheme.MarkedForeground = (ColorName)_colorSelectors["Marked"].SelectedItem;
        _settings.ColorScheme.InactivePaneForeground = (ColorName)_colorSelectors["Inactive"].SelectedItem;

        WasSaved = true;
        Terminal.Gui.Application.RequestStop();
    }

    public AppSettings GetSettings() => _settings;
}

