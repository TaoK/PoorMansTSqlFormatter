/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2012 Tao Klerks

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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
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
            string commonDocsFolder = Context.Parameters["codecomdocfolder"];
            string targetAssemblyFolder = Context.Parameters["codetargetdir"];
            if (commonDocsFolder != null && targetAssemblyFolder != null)
            {
                //find the AddIn file just created, and customize it to point to the correct folder for the assembly
                string addInFilePath = commonDocsFolder + @"\Microsoft\MSEnvShared\AddIns\PoorMansTSqlFormatterSSMSAddIn.AddIn";

                XmlNameTable nt = new NameTable();
                XmlNamespaceManager ns = new XmlNamespaceManager(nt);
                ns.AddNamespace("autoext", "http://schemas.microsoft.com/AutomationExtensibility");

                XmlDocument addInDefinitionFile = new XmlDocument();
                addInDefinitionFile.Load(addInFilePath);
                XmlNode customizableNode = addInDefinitionFile.SelectSingleNode("/autoext:Extensibility/autoext:Addin/autoext:Assembly", ns);
                customizableNode.InnerText = customizableNode.InnerText.Replace("%TARGETDIR%", targetAssemblyFolder);
                addInDefinitionFile.Save(addInFilePath);

            }
        }
    }
}
