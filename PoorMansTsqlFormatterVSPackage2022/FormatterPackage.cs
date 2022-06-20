using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;
using System.ComponentModel.Design;

using EnvDTE;

using EnvDTE80;
using Microsoft;

using PoorMansTSqlFormatterSSMSLib;
using System.Threading;

namespace PoorMansTSqlFormatterVSPackage2022
{
	using System.Diagnostics.CodeAnalysis;

	using PoorMansTsqlFormatterVSPackage2022;

	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuids.guidPoorMansTSqlFormatterSSMSVSPackagePkgString)]
	public sealed class FormatterPackage : AsyncPackage
    {

        //TODO: figure out how to deal with signing... where to keep the key, etc.
        private GenericVSHelper ssmsHelper;

        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
            await base.InitializeAsync(cancellationToken, progress);

            ssmsHelper = new GenericVSHelper(true, null, null, null);

            //Switch to UI thread, so that we're allowed to get services
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			// Add our command handlers for the menu commands defined in the in the .vsct file, and enable them
			if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs){

				// Create the formatting command / menu item.
				var formatCommandId = new CommandID(PackageGuids.guidPoorMansTSqlFormatterSSMSVSPackageCmdSet, (int)PackageIds.cmdidPoorMansFormatSQL);
				var formatCommand = new OleMenuCommand(FormatSqlCallback, formatCommandId);
				formatCommand.BeforeQueryStatus += QueryFormatButtonStatus;
				mcs.AddCommand(formatCommand);

				// Create the options command / menu item.
				var settingsCommandId = new CommandID(PackageGuids.guidPoorMansTSqlFormatterSSMSVSPackageCmdSet, (int)PackageIds.cmdidPoorMansSqlOptions);
				var settingsCommand = new OleMenuCommand(SqlOptionsCallback, settingsCommandId)
				{
					Enabled = true
				};
				mcs.AddCommand(settingsCommand);
			}
        }

        private void FormatSqlCallback(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            ssmsHelper.FormatSqlInTextDoc(dte);
        }

        private void SqlOptionsCallback(object sender, EventArgs e)
        {
            ssmsHelper.GetUpdatedFormattingOptionsFromUser();
        }

        private void QueryFormatButtonStatus(object sender, EventArgs e)
        {
			ThreadHelper.ThrowIfNotOnUIThread();
			var dte = (DTE2)GetService(typeof(DTE));
			Assumes.Present(dte);

			if (sender is OleMenuCommand queryingCommand && dte.ActiveDocument != null && !dte.ActiveDocument.ReadOnly)
                queryingCommand.Enabled = true;
        }
    }
}
