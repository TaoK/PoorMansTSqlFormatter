/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011-2016 Tao Klerks

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
using System.Windows.Forms;


namespace PoorMansTSqlFormatterSSMSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]  //General VSPackage hookup
    [InstalledProductRegistration("#ProductName", "#ProductDescription", "1.6.1")]  //Package Medatada, references to VSPackage.resx resource keys
    [ProvideAutoLoad(VSConstants.UICONTEXT.NotBuildingAndNotDebugging_string)] // Auto-load for dynamic menu enabling/disabling; this context seems to work for SSMS and VS
    [ProvideMenuResource("Menus.ctmenu", 1)]  //Hook to command definitions / to vsct stuff
    [Guid(guidPoorMansTSqlFormatterSSMSPackagePkgString)] //Arbitrarily/randomly defined guid for this extension
    public sealed class FormatterPackage : Package
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

        protected override void Initialize()
        {
            base.Initialize();

            _SSMSHelper = new GenericVSHelper(true, null, null, null);

            // Add our command handlers for the menu commands defined in the in the .vsct file, and enable them
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
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

            _packageLoadingDisableTimer = new System.Timers.Timer();
            _packageLoadingDisableTimer.Elapsed += new System.Timers.ElapsedEventHandler(PackageDisableLoadingCallback);
            _packageLoadingDisableTimer.Interval = 15000;
            _packageLoadingDisableTimer.Enabled = true;
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

        private void PackageDisableLoadingCallback(object sender, System.Timers.ElapsedEventArgs e)
        {
            _packageLoadingDisableTimer.Enabled = false;
            SetPackageLoadingDisableKeyIfRequired();
        }

        protected override int QueryClose(out bool canClose)
        {
            SetPackageLoadingDisableKeyIfRequired();
            return base.QueryClose(out canClose);
        }

        /// <summary>
        /// For SSMS 2015 and earlier, this will set a registry key to disable the extension. Strangely, extension loading only works for disabled extensions...
        /// </summary>
        private void SetPackageLoadingDisableKeyIfRequired()
        {
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            string fullName = dte.FullName.ToUpperInvariant();
            int majorVersion = int.Parse(dte.Version.Split('.')[0]);

            if ((fullName.Contains("SSMS") || fullName.Contains("MANAGEMENT STUDIO")) && majorVersion <= 2015)
                UserRegistryRoot.CreateSubKey(@"Packages\{" + guidPoorMansTSqlFormatterSSMSPackagePkgString + "}").SetValue("SkipLoading", 1);
        }
    }
}
