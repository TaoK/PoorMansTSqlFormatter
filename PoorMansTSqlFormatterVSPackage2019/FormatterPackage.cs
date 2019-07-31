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


//Please note, most of this code is duplicated across the SSMS Package, VS2015 extension, and VS2019 extension. 
// Descriptions, GUIDs, Early SSMS support, and Async loading support differ.
// (it would make sense to improve this at some point)
namespace PoorMansTSqlFormatterSSMSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]  //General VSPackage hookup, 
    [InstalledProductRegistration("#ProductName", "#ProductDescription", "1.6.14")]  //Package Medatada, references to VSPackage.resx resource keys
    [ProvideAutoLoad(VSConstants.UICONTEXT.NotBuildingAndNotDebugging_string, PackageAutoLoadFlags.BackgroundLoad)] // Auto-load for dynamic menu enabling/disabling; this context seems to work for SSMS and VS
    [ProvideMenuResource("Menus.ctmenu", 1)]  //Hook to command definitions / to vsct stuff
    [Guid(guidPoorMansTSqlFormatterSSMSPackagePkgString)] //Arbitrarily/randomly defined guid for this extension
    public sealed class FormatterPackage : AsyncPackage
    {
        //These constants are duplicated in the vsct file
        public const string guidPoorMansTSqlFormatterSSMSPackagePkgString = "5e84b709-1e60-4116-a702-4cdb1a282d6e";
        public const string guidPoorMansTSqlFormatterSSMSPackageCmdSetString = "201bf73c-de53-48a2-a912-c3b8308dacce";
        public const uint cmdidPoorMansFormatSQL = 0x100;
        public const uint cmdidPoorMansSqlOptions = 0x101;

        public static readonly Guid guidPoorMansTSqlFormatterSSMSPackageCmdSet = new Guid(guidPoorMansTSqlFormatterSSMSPackageCmdSetString);

        //TODO: figure out how to deal with signing... where to keep the key, etc.

        private GenericVSHelper _SSMSHelper;
        private System.Timers.Timer _packageLoadingDisableTimer;

        public FormatterPackage()
        {
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            _SSMSHelper = new GenericVSHelper(true, null, null, null);

            //Switch to UI thread, so that we're allowed to get services
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Add our command handlers for the menu commands defined in the in the .vsct file, and enable them
            if (await GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(false) is OleMenuCommandService mcs)
            {
                CommandID menuCommandID;
                OleMenuCommand menuCommand;

                // Create the formatting command / menu item.
                menuCommandID = new CommandID(guidPoorMansTSqlFormatterSSMSPackageCmdSet, (int)cmdidPoorMansFormatSQL);
                menuCommand = new OleMenuCommand(FormatSqlCallback, menuCommandID);
                mcs.AddCommand(menuCommand);
                menuCommand.BeforeQueryStatus += new EventHandler(QueryFormatButtonStatus);

                // Create the options command / menu item.
                menuCommandID = new CommandID(guidPoorMansTSqlFormatterSSMSPackageCmdSet, (int)cmdidPoorMansSqlOptions);
                menuCommand = new OleMenuCommand(SqlOptionsCallback, menuCommandID);
                menuCommand.Enabled = true;
                mcs.AddCommand(menuCommand);
            }

            return;
        }

        private void FormatSqlCallback(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            _SSMSHelper.FormatSqlInTextDoc(dte);
        }

        private void SqlOptionsCallback(object sender, EventArgs e)
        {
            _SSMSHelper.GetUpdatedFormattingOptionsFromUser();
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
    }
}
