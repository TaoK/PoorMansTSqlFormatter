/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net and JS, written in C#. 
Copyright (C) 2011-2022 Tao Klerks

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

namespace PoorMansTSqlFormatterDemo
{
    partial class AboutBox : Form
    {
        private readonly System.Resources.ResourceManager _generalResourceManager = new("PoorMansTSqlFormatterDemo.GeneralLanguageContent", Assembly.GetExecutingAssembly());

        public AboutBox()
        {

            InitializeComponent();
            Text = FormatTranslation("AboutTitleLabel", AssemblyTitle);
            labelProductName.Text = String.Format("{0}, v{1}", AssemblyProduct, AssemblyVersion);
            labelCopyright.Text = AssemblyCopyright;
            textBoxDescription.Text = _generalResourceManager.GetString("ProjectAboutDescription");

            string GPLText = "";

            using (Stream? fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(AboutBox).Namespace + ".LICENSE.txt"))
            {
                if (fileStream == null)
                    throw new Exception("License file not found!");

                using StreamReader textReader = new(fileStream, System.Text.Encoding.ASCII);
                GPLText = textReader.ReadToEnd();
            }

            this.textBoxDescription.Text += Environment.NewLine + Environment.NewLine + GPLText;
        }

        string FormatTranslation(string key, params object[] args)
        {
            return String.Format(_generalResourceManager?.GetString(key) ?? "", args);
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
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
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

        #endregion

    }
}
