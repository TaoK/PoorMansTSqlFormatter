/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2012-2017 Tao Klerks

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

using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Xml;


namespace PoorMansTSqlFormatterSSMSAddIn
{
    [RunInstaller(true)]
    public partial class AddInFileSetupFinalizer : Installer
    {
        public AddInFileSetupFinalizer()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            string targetAssemblyFolder = Context.Parameters["codetargetdir"];
            if (targetAssemblyFolder != null)
            {
                //AddIn handling
                string commonAppDataFolder = Context.Parameters["codecomappdatafolder"];
                if (commonAppDataFolder != null)
                {
                    commonAppDataFolder = commonAppDataFolder.Trim();
                    targetAssemblyFolder = targetAssemblyFolder.Trim();
                    targetAssemblyFolder = targetAssemblyFolder.Replace(@"\\", @"\");
                    //find the AddIn file(s) just created, and customize them to point to the correct folder for the assembly
                    FixAddInFileIfExists(targetAssemblyFolder, commonAppDataFolder + @"\Microsoft\SQL Server Management Studio\11.0\Addins\PoorMansTSqlFormatterSSMSAddIn.AddIn", "Microsoft SQL Server Management Studio");
                    FixAddInFileIfExists(targetAssemblyFolder, commonAppDataFolder + @"\Microsoft\VisualStudio\8.0\Addins\PoorMansTSqlFormatterSSMSAddIn.AddIn", "Microsoft Visual Studio");
                    FixAddInFileIfExists(targetAssemblyFolder, commonAppDataFolder + @"\Microsoft\VisualStudio\9.0\Addins\PoorMansTSqlFormatterSSMSAddIn.AddIn", "Microsoft Visual Studio");
                    FixAddInFileIfExists(targetAssemblyFolder, commonAppDataFolder + @"\Microsoft\VisualStudio\10.0\Addins\PoorMansTSqlFormatterSSMSAddIn.AddIn", "Microsoft Visual Studio");
                    FixAddInFileIfExists(targetAssemblyFolder, commonAppDataFolder + @"\Microsoft\VisualStudio\11.0\Addins\PoorMansTSqlFormatterSSMSAddIn.AddIn", "Microsoft Visual Studio");
                }


                //VsPackage Handling (SSMS 2014 & 2016 - Manual file-based registration required
                string ssms2014ExtensionsFolder = Context.Parameters["ssms2014extensions"];
                string ssms2016ExtensionsFolder = Context.Parameters["ssms2016extensions"];

                if (ssms2014ExtensionsFolder != null && ssms2014ExtensionsFolder.Trim() != "")
                    InstallPackageDef(targetAssemblyFolder, ssms2014ExtensionsFolder);

                if (ssms2016ExtensionsFolder != null && ssms2016ExtensionsFolder.Trim() != "")
                    InstallPackageDef(targetAssemblyFolder, ssms2016ExtensionsFolder);
            }
        }

        private void InstallPackageDef(string targetAssemblyFolder, string ssms2016ExtensionsFolder)
        {
            //then read the PoorMansTSqlFormatterSSMSPackage.pkgdef file from the target install folder
            //and write it, replacing $PackageFolder$ with the target install folder.
        }

        private static void FixAddInFileIfExists(string TargetAssemblyFolder, string AddInFilePath, string HostAppName)
        {

            if (System.IO.File.Exists(AddInFilePath))
            {
                XmlNameTable nt = new NameTable();
                XmlNamespaceManager ns = new XmlNamespaceManager(nt);
                ns.AddNamespace("autoext", "http://schemas.microsoft.com/AutomationExtensibility");

                XmlDocument addInDefinitionFile = new XmlDocument();
                addInDefinitionFile.Load(AddInFilePath);

                XmlNode assemblyPathNode = addInDefinitionFile.SelectSingleNode("/autoext:Extensibility/autoext:Addin/autoext:Assembly", ns);
                assemblyPathNode.InnerText = assemblyPathNode.InnerText.Replace("%TARGETDIR%", TargetAssemblyFolder);

                XmlNode hostAppNamePath = addInDefinitionFile.SelectSingleNode("/autoext:Extensibility/autoext:HostApplication/autoext:Name", ns);
                hostAppNamePath.InnerText = hostAppNamePath.InnerText.Replace("%HOSTAPPNAME%", HostAppName);

                addInDefinitionFile.Save(AddInFilePath);
            }
        }
    }
}
