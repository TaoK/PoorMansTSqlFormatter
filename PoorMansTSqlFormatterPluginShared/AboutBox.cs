﻿/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
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

using System.Reflection;

namespace PoorMansTSqlFormatterPluginShared
{
    partial class AboutBox : Form
    {
        private readonly System.Resources.ResourceManager _generalResourceManager = new("PoorMansTSqlFormatterPluginShared.GeneralLanguageContent", Assembly.GetExecutingAssembly());

        public AboutBox(Assembly ProductAssembly, string ProductAboutDescription)
        {
            InitializeComponent();
            Text = FormatTranslation("AboutTitleLabel", AssemblyTitle(ProductAssembly));
            labelProductName.Text = String.Format("{0}, v{1}", AssemblyProduct(ProductAssembly), AssemblyVersion(ProductAssembly));
            labelCopyright.Text = AssemblyCopyright(ProductAssembly);
            textBoxDescription.Text = ProductAboutDescription;

            string GPLText = "";

            using (Stream? fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(AboutBox).Namespace + ".LICENSE.txt"))
            {
                if (fileStream == null)
                    throw new Exception("License file not found!");

                using (StreamReader textReader = new StreamReader(fileStream, System.Text.Encoding.ASCII))
                {
                    GPLText = textReader.ReadToEnd();
                }
            }
            this.textBoxDescription.Text += System.Environment.NewLine + System.Environment.NewLine + GPLText;
        }
        string FormatTranslation(string key, params object[] args)
        {
            return String.Format(_generalResourceManager?.GetString(key) ?? "", args);
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle(Assembly ProductAssembly)
        {
            object[] attributes = ProductAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title != "")
                {
                    return titleAttribute.Title;
                }
            }
            return "";
        }

        public string AssemblyVersion(Assembly ProductAssembly)
        {
            return ProductAssembly.GetName().Version?.ToString() ?? "";
        }

        public string AssemblyProduct(Assembly ProductAssembly)
        {
            object[] attributes = ProductAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyProductAttribute)attributes[0]).Product;
        }

        public string AssemblyCopyright(Assembly ProductAssembly)
        {
            object[] attributes = ProductAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        public string AssemblyCompany(Assembly ProductAssembly)
        {
            object[] attributes = ProductAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
        #endregion

    }
}
