/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011 Tao Klerks

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
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace PoorMansTSqlFormatterDemo
{
    partial class AboutBox : Form
    {
        private FrameworkClassReplacements.SingleAssemblyResourceManager _generalResourceManager = new FrameworkClassReplacements.SingleAssemblyResourceManager("GeneralLanguageContent", System.Reflection.Assembly.GetExecutingAssembly(), typeof(Program));

        public AboutBox()
        {

            InitializeComponent();
            this.Text = String.Format(_generalResourceManager.GetString("AboutTitleLabel"), AssemblyTitle);
            this.labelProductName.Text = String.Format("{0}, v{1}", AssemblyProduct, AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.textBoxDescription.Text = _generalResourceManager.GetString("ProjectAboutDescription");

            string GPLText = "";

            using (Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".LICENSE.txt"))
            using (StreamReader textReader = new StreamReader(fileStream, System.Text.Encoding.ASCII))
            {
                GPLText = textReader.ReadToEnd();
            }

            this.textBoxDescription.Text += System.Environment.NewLine + System.Environment.NewLine + GPLText;
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

    }
}
