/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2017 Tao Klerks

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
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Xml;

namespace PoorMansTSqlFormatterSSMSAddIn.Setup.action
{
    public class SSMSAddInActions
    {
        const string APPLICATIONFOLDERKEY = "APPLICATIONFOLDER";
        const string SSMS11FILEKEY = "SSMS11FILE";
        const string VS8FILEKEY = "VS8FILE";
        const string VS9FILEKEY = "VS9FILE";
        const string VS10FILEKEY = "VS10FILE";
        const string VS11FILEKEY = "VS11FILE";

        [CustomAction]
        public static ActionResult AddinUpdateAction(Session session)
        {
            var extensionInstallFolder = session.CustomActionData[APPLICATIONFOLDERKEY];
            session.Log("Begin AddinUpdateAction with install folder " + extensionInstallFolder);

            try
            {
                extensionInstallFolder = extensionInstallFolder.Trim();
                extensionInstallFolder = extensionInstallFolder.Replace(@"\\", @"\");
                //find the AddIn file(s) just created, and if they exist then customize them to point to the correct folder for the assembly
                FixAddInFileIfExists(extensionInstallFolder, session.CustomActionData[SSMS11FILEKEY], "Microsoft SQL Server Management Studio");
                FixAddInFileIfExists(extensionInstallFolder, session.CustomActionData[VS8FILEKEY], "Microsoft Visual Studio");
                FixAddInFileIfExists(extensionInstallFolder, session.CustomActionData[VS9FILEKEY], "Microsoft Visual Studio");
                FixAddInFileIfExists(extensionInstallFolder, session.CustomActionData[VS10FILEKEY], "Microsoft Visual Studio");
                FixAddInFileIfExists(extensionInstallFolder, session.CustomActionData[VS11FILEKEY], "Microsoft Visual Studio");
            }
            catch (Exception ex)
            {
                session.Log("Failed AddinUpdateAction");
                session.Log("Exception: " + ex.ToString());
                return ActionResult.Failure;
            }

            session.Log("Finished AddinUpdateAction successfully");
            return ActionResult.Success;
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
