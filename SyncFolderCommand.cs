using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using EnvDTE;

namespace SyncFolder {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SyncFolderCommand {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        private System.IServiceProvider serviceProvider;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a927b0bc-c5a4-4d5d-8178-036d07a67692");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncFolderCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SyncFolderCommand(AsyncPackage package, OleMenuCommandService commandService) {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SyncFolderCommand Instance {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider {
            get {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package) {
            // Switch to the main thread - the call to AddCommand in SyncFolderCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SyncFolderCommand(package, commandService);



        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e) {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = "";
            string title = "SyncFolder";

            try {
                var item = package as System.IServiceProvider;
                EnvDTE80.DTE2 dte = item.GetService(typeof(DTE)) as EnvDTE80.DTE2;
                dte.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
            } catch (Exception exc) {
                message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", exc.ToString());
                // Show a message box to prove we were here
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    message,
                    title,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            //var temp = dte.ActiveDocument.FullName;
            //if(dte.ActiveDocument != null) {
            //    dte.Application.MainWindow.Caption = dte.ActiveDocument.FullName;
            //    //dte.ActiveDocument.ActiveWindow.Caption = dte.ActiveDocument.FullName;
            //}

            //EnvDTE80.DTE2 dte2 = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.17.6.4");
            //dte2.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
        }
    }
}