/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting
library for .Net 2.0 and JS, written in C#.
Copyright (C) 2011-2019 Tao Klerks

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using PoorMansTSqlFormatterSSMSLib;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


//Please note, most of this code is duplicated across the SSMS Package, VS2015 extension, and VS2019 extension.
// Descriptions, GUIDs, Early SSMS support, and Async loading support differ.
// (it would make sense to improve this at some point)
namespace PoorMansTSqlFormatterSSMSPackage
{
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]  //General VSPackage hookup,
  [InstalledProductRegistration("#ProductName", "#ProductDescription", "1.6.16")]  //Package Medatada, references to VSPackage.resx resource keys
  [ProvideAutoLoad(VSConstants.UICONTEXT.NotBuildingAndNotDebugging_string, PackageAutoLoadFlags.BackgroundLoad)] // Auto-load for dynamic menu enabling/disabling; this context seems to work for SSMS and VS
                                                                                                                  //[ProvideMenuResource("Menus.ctmenu", 1)]  //Hook to command definitions / to vsct stuff
  [Guid(guidPoorMansTSqlFormatterSSMSPackagePkgString)] //Arbitrarily/randomly defined guid for this extension
  [ProvideMenuResource("Menus.ctmenu", 1)]
  public sealed class FormatterPackage : AsyncPackage
  {
    //These constants are duplicated in the vsct file
    public const string guidPoorMansTSqlFormatterSSMSPackagePkgString = "5e84b709-1e60-4116-a702-4cdb1a282d6e";
    public const string guidPoorMansTSqlFormatterSSMSPackageCmdSetString = "201bf73c-de53-48a2-a912-c3b8308dacce";
    public const uint cmdidPoorMansFormatSQL = 0x100;
    public const uint cmdidPoorMansSqlOptions = 0x101;

    public static readonly Guid guidPoorMansTSqlFormatterSSMSPackageCmdSet = new Guid(guidPoorMansTSqlFormatterSSMSPackageCmdSetString);

    //TODO: figure out how to deal with signing... where to keep the key, etc.

    internal static GenericVSHelper SSMSHelper;

    public FormatterPackage()
    {
    }


    private void FormatSqlCallback(object sender, EventArgs e)
    {
      DTE2 dte = (DTE2)GetService(typeof(DTE));
      SSMSHelper.FormatSqlInTextDoc(dte);
    }

    private void SqlOptionsCallback(object sender, EventArgs e)
    {
      SSMSHelper.GetUpdatedFormattingOptionsFromUser();
    }

    private void QueryFormatButtonStatus(object sender, EventArgs e)
    {
      var queryingCommand = sender as OleMenuCommand;
      DTE2 dte = (DTE2)GetService(typeof(DTE));
      if (queryingCommand != null && dte.ActiveDocument != null && !dte.ActiveDocument.ReadOnly)
        queryingCommand.Enabled = true;
      else
        queryingCommand.Enabled = false;
    }

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      await base.InitializeAsync(cancellationToken, progress);

      SSMSHelper = new GenericVSHelper(true, null, null, null);

      // When initialized asynchronously, the current thread may be a background thread at this point.
      // Do any initialization that requires the UI thread after switching to the UI thread.
      await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

      // Add our command handlers for the menu commands defined in the in the .vsct file, and enable them
      if (await GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(false) is OleMenuCommandService mcs)
      {
        //CommandID menuCommandID;
        //OleMenuCommand menuCommand;

        //// Create the formatting command / menu item.
        //menuCommandID = new CommandID(guidPoorMansTSqlFormatterSSMSPackageCmdSet, (int)cmdidPoorMansFormatSQL);
        //menuCommand = new OleMenuCommand(FormatSqlCallback, menuCommandID);
        //mcs.AddCommand(menuCommand);
        //menuCommand.BeforeQueryStatus += new EventHandler(QueryFormatButtonStatus);

        //// Create the options command / menu item.
        //menuCommandID = new CommandID(guidPoorMansTSqlFormatterSSMSPackageCmdSet, (int)cmdidPoorMansSqlOptions);
        //menuCommand = new OleMenuCommand(SqlOptionsCallback, menuCommandID);
        //menuCommand.Enabled = true;
        //mcs.AddCommand(menuCommand);
      }

      await PoorMansTSqlFormatterVSPackage2022.FormatSqlCommand.InitializeAsync(this);
        await PoorMansTSqlFormatterVSPackage2022.SqlOptionsCommand.InitializeAsync(this);
    }
  }
}
