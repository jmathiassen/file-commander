using Terminal.Gui;
using File_Commander.Application;
using File_Commander.Services;
using File_Commander.UI;

namespace File_Commander;

class Program
{
    static void Main(string[] args)
    {
        // Initialize services
        var configService = new ConfigService();
        var fileSystemService = new FileSystemService();
        var fileOperationExecutor = new FileOperationExecutor();
        var taskQueueService = new IntelligentTaskQueueService(fileOperationExecutor);
        var keymapService = new KeymapService();

        // Initialize application layer
        var tabManager = new TabManager(fileSystemService, configService);
        var commandHandler = new CommandHandler(tabManager, taskQueueService, configService);

        // Initialize UI components
        var statusPane = new StatusPaneView(taskQueueService);

        // Create initial tab with current directory
        var initialPath = args.Length > 0
            ? fileSystemService.NormalizePath(args[0])
            : Environment.CurrentDirectory;

        tabManager.CreateTab(initialPath);

        // Initialize Terminal.Gui
        Terminal.Gui.Application.Init();

        try
        {
            // Create and run main window
            var mainWindow = new MainWindow(tabManager, commandHandler, keymapService, statusPane, configService);
            Terminal.Gui.Application.Run(mainWindow);
        }
        finally
        {
            // Cleanup - shutdown task queue gracefully
            taskQueueService.ShutdownAsync().Wait();
            Terminal.Gui.Application.Shutdown();
        }
    }
}
