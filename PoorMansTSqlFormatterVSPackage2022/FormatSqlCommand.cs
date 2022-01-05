using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using PoorMansTSqlFormatterSSMSPackage;
using Task = System.Threading.Tasks.Task;

namespace PoorMansTSqlFormatterVSPackage2022
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class FormatSqlCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("78736cf3-a27d-487e-ad47-d77a1f0ae065");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    private static DTE2 _dte;

    private void QueryFormatButtonStatus(object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var queryingCommand = sender as OleMenuCommand;
      if (queryingCommand != null && _dte.ActiveDocument != null && !_dte.ActiveDocument.ReadOnly)
        queryingCommand.Enabled = true;
      else
        queryingCommand.Enabled = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatSqlCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private FormatSqlCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
      menuItem.BeforeQueryStatus += new EventHandler(QueryFormatButtonStatus);
      commandService.AddCommand(menuItem);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static FormatSqlCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
    {
      get
      {
        return this.package;
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in FormatSqlCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new FormatSqlCommand(package, commandService);

      _dte = (DTE2)await package.GetServiceAsync(typeof(DTE));

    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void Execute(object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      FormatterPackage.SSMSHelper.FormatSqlInTextDoc(_dte);
    }
  }
}
