using Terminal.Gui;
using File_Commander.Models;

namespace File_Commander.UI;

/// <summary>
/// Options/Preferences dialog for configuring application settings
/// </summary>
public class OptionsDialog : Dialog
{
    private readonly AppSettings _settings;
    private RadioGroup _updateModeRadio = null!;
    private CheckBox _showSecondsCheckBox = null!;
    private CheckBox _showHiddenFilesCheckBox = null!;
    private CheckBox _followSymlinksCheckBox = null!;
    private CheckBox _useNarrowIconsCheckBox = null!;
    private CheckBox _autoCalculateSizeCheckBox = null!;

    public bool WasSaved { get; private set; }

    public OptionsDialog(AppSettings currentSettings) : base("Options")
    {
        _settings = new AppSettings
        {
            DirectoryUpdateMode = currentSettings.DirectoryUpdateMode,
            ShowSecondsInDate = currentSettings.ShowSecondsInDate,
            ShowHiddenFiles = currentSettings.ShowHiddenFiles,
            FollowSymlinks = currentSettings.FollowSymlinks,
            UseNarrowIcons = currentSettings.UseNarrowIcons,
            AutoCalculateDirectorySize = currentSettings.AutoCalculateDirectorySize
        };

        Width = 70;
        Height = 22;

        InitializeUI();
    }

    private void InitializeUI()
    {
        var y = 1;

        // Directory Update Mode section
        Add(new Label(1, y++, "Directory Update Mode:"));

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
        Add(_updateModeRadio);
        y += 4;

        // Display Options section
        Add(new Label(1, y++, "Display Options:"));

        _showSecondsCheckBox = new CheckBox(3, y++, "Show seconds in file dates")
        {
            Checked = _settings.ShowSecondsInDate
        };
        Add(_showSecondsCheckBox);

        _useNarrowIconsCheckBox = new CheckBox(3, y++, "Use narrow icons (fixes alignment)")
        {
            Checked = _settings.UseNarrowIcons
        };
        Add(_useNarrowIconsCheckBox);

        _showHiddenFilesCheckBox = new CheckBox(3, y++, "Show hidden files")
        {
            Checked = _settings.ShowHiddenFiles
        };
        Add(_showHiddenFilesCheckBox);

        y++;

        // File Operations section
        Add(new Label(1, y++, "File Operations:"));

        _autoCalculateSizeCheckBox = new CheckBox(3, y++, "Auto-calculate directory size when marking")
        {
            Checked = _settings.AutoCalculateDirectorySize
        };
        Add(_autoCalculateSizeCheckBox);

        _followSymlinksCheckBox = new CheckBox(3, y++, "Follow symbolic links")
        {
            Checked = _settings.FollowSymlinks
        };
        Add(_followSymlinksCheckBox);

        // Buttons
        var okButton = new Button("OK")
        {
            X = Pos.Center() - 10,
            Y = Pos.AnchorEnd(2),
            IsDefault = true
        };
        okButton.Clicked += OnOkClicked;
        Add(okButton);

        var cancelButton = new Button("Cancel")
        {
            X = Pos.Center() + 2,
            Y = Pos.AnchorEnd(2)
        };
        cancelButton.Clicked += () => Terminal.Gui.Application.RequestStop();
        Add(cancelButton);
    }

    private void OnOkClicked()
    {
        // Update settings from UI
        _settings.DirectoryUpdateMode = (DirectoryUpdateMode)_updateModeRadio.SelectedItem;
        _settings.ShowSecondsInDate = _showSecondsCheckBox.Checked;
        _settings.ShowHiddenFiles = _showHiddenFilesCheckBox.Checked;
        _settings.FollowSymlinks = _followSymlinksCheckBox.Checked;
        _settings.UseNarrowIcons = _useNarrowIconsCheckBox.Checked;
        _settings.AutoCalculateDirectorySize = _autoCalculateSizeCheckBox.Checked;

        WasSaved = true;
        Terminal.Gui.Application.RequestStop();
    }

    public AppSettings GetSettings() => _settings;
}

