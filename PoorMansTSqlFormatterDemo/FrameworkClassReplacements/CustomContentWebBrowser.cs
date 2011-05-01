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
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PoorMansTSqlFormatterDemo.FrameworkClassReplacements
{
    public class CustomContentWebBrowser : System.Windows.Forms.WebBrowser
    {
        // WebBrowser control, modified to allow easy setting of HTML content, based on:
        //http://weblogs.asp.net/gunnarpeipman/archive/2009/08/15/displaying-custom-html-in-webbrowser-control.aspx
        // Also disabling navigation sound, as per:
        //https://connect.microsoft.com/VisualStudio/feedback/details/345528/webbrowser-control-in-wpf-disable-sound

        private const int DISABLE_NAVIGATION_SOUNDS = 21;
        private const int SET_FEATURE_ON_PROCESS = 0x00000002;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(int FeatureEntry, [MarshalAs(UnmanagedType.U4)] int dwFlags, bool fEnable);

        public void SetHTML(string htmlContent)
        {
            bool allowedNavigation = AllowNavigation;
            AllowNavigation = true;
            CoInternetSetFeatureEnabled(DISABLE_NAVIGATION_SOUNDS, SET_FEATURE_ON_PROCESS, true);
            this.Navigate("about:blank");
            if (this.Document != null)
            {
                this.Document.Write(string.Empty);
            }
            this.DocumentText = htmlContent;
            CoInternetSetFeatureEnabled(DISABLE_NAVIGATION_SOUNDS, SET_FEATURE_ON_PROCESS, false);
            AllowNavigation = allowedNavigation;
        }
    }
}
