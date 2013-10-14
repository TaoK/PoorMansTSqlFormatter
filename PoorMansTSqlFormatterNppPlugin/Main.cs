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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Resources;
using System.Windows.Forms;
using NppPluginNET;
using System.Reflection;
using PoorMansTSqlFormatterPluginShared;

namespace PoorMansTSqlFormatterNppPlugin
{
    class Main
    {
        /* 
         * First draft of Poor Man's T-SQL formatter plugin for Notepad++:
         *  - One formatting command on the menu, no other functionality exception options/about
         *     - Reformats the selected code as T-SQL
         *     - If there is no selection, reformats the entire file (scintilla buffer/window, rather)
         *     - If a parsing error is encountered, requests confirmation before continuing
         *  - Keyboards shortcut can be assigned using notepad++ built-in feature: Settings -> Shortcut Mapper...
         *     - If anyone has a suggestion for a default mapping, I'm all ears (the default MS ones are taken I think)
         *  - Formatting options can be set through menu, and "About" dialog is available from options window.
         *  
         * Wishlist:
         *  - Translation per user preference - apparently there is currently no way for plugins to access the UI language setting
         *    - I believe translation should already be working automatically according to user's general locale, just not following Notepad++ setting.
         */

        #region " Fields "
        internal const string PluginName = "Poor Man's T-Sql Formatter";
        static string iniFilePath = null;
        static PoorMansTSqlFormatterLib.SqlFormattingManager _formattingManager = null;
        static ResourceManager _generalResourceManager = new ResourceManager("PoorMansTSqlFormatterNppPlugin.GeneralLanguageContent", Assembly.GetExecutingAssembly());
        #endregion

        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            //this is where I'd really like access to language info from Notepad++ context...
            //MessageBox.Show(string.Format("Cult: {0}; UICult: {1}", System.Threading.Thread.CurrentThread.CurrentCulture.EnglishName, System.Threading.Thread.CurrentThread.CurrentUICulture.EnglishName));

            //get settings from notepad++-assigned plugin data folder, and set for settings provider.
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            string iniFolder = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFolder)) Directory.CreateDirectory(iniFolder);
            iniFilePath = Path.Combine(iniFolder, PluginName + ".ini.xml");
            Properties.Settings.Default.Context["settingsPath"] = iniFilePath;

            _formattingManager = Utils.GetFormattingManager(Properties.Settings.Default);

            //set up menu items
            PluginBase.SetCommand(0, _generalResourceManager.GetString("FormatButtonText"), formatSqlCommand, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, _generalResourceManager.GetString("OptionsButtonText"), formattingOptionsCommand, new ShortcutKey(false, false, false, Keys.None));
        }

        internal static void PluginCleanUp()
        {
        }

        #endregion

        #region " Menu functions "
        internal static void formatSqlCommand()
        {
            StringBuilder textBuffer = null;
			StringBuilder outBuffer = null;

            IntPtr currentScintilla = PluginBase.GetCurrentScintilla();

            //apparently calling with null pointer returns selection buffer length: http://www.scintilla.org/ScintillaDoc.html#SCI_GETSELTEXT
            int selectionBufferLength = (int)Win32.SendMessage(currentScintilla, SciMsg.SCI_GETSELTEXT, 0, 0);

            if (selectionBufferLength > 1)
            {
				//prep the buffer/StringBuilder with the right length
                textBuffer = new StringBuilder(selectionBufferLength);

				//populate the buffer
                Win32.SendMessage(currentScintilla, SciMsg.SCI_GETSELTEXT, 0, textBuffer);

				//if formatting is successful or user chooses to continue despite error
				if (FormatAndWarn(textBuffer, out outBuffer))
				{
					//replace the selection with the formatted content
					Win32.SendMessage(currentScintilla, SciMsg.SCI_REPLACESEL, 0, outBuffer);

					//position of the cursor will automatically be the end of the replaced selection
				}
            }
            else
            {
				//Do as they say here:
                //http://www.scintilla.org/ScintillaDoc.html#SCI_GETTEXT
				int docBufferLength = (int)Win32.SendMessage(currentScintilla, SciMsg.SCI_GETTEXT, 0, 0);
				int docCursorPosition = (int)Win32.SendMessage(currentScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);
				textBuffer = new StringBuilder(docBufferLength);
                Win32.SendMessage(currentScintilla, SciMsg.SCI_GETTEXT, docBufferLength, textBuffer);

				if (FormatAndWarn(textBuffer, out outBuffer))
				{
					//note: the "docBufferLength" always seems to be 1 too high, even for an empty doc it is 1, so am subtracting explicitly to avoid "cursor creep".
					int newPosition = (int)Math.Round(1.0 * docCursorPosition * outBuffer.Length / (docBufferLength - 1), 0, MidpointRounding.AwayFromZero);
					//replace the doc content
					Win32.SendMessage(currentScintilla, SciMsg.SCI_SETTEXT, 0, outBuffer);
					//set the cursor position to somewhere reasonable
					Win32.SendMessage(currentScintilla, SciMsg.SCI_SETSEL, newPosition, newPosition);
				}
			}
        }

		private static bool FormatAndWarn(StringBuilder textBuffer, out StringBuilder outBuffer)
		{
			bool errorsEncountered = false;
			outBuffer = new StringBuilder(_formattingManager.Format(textBuffer.ToString(), ref errorsEncountered));

			if (errorsEncountered)
				if (MessageBox.Show(_generalResourceManager.GetString("ParseErrorWarningMessage"), _generalResourceManager.GetString("ParseErrorWarningMessageTitle"), MessageBoxButtons.OKCancel) != DialogResult.OK)
					return false;

			//true means go ahead
			return true;
		}

        internal static void formattingOptionsCommand()
        {
            SettingsForm settings = new SettingsForm(Properties.Settings.Default, Assembly.GetExecutingAssembly(), _generalResourceManager.GetString("ProjectAboutDescription"));
            if (settings.ShowDialog() == DialogResult.OK)
            {
                _formattingManager = Utils.GetFormattingManager(Properties.Settings.Default);
            }
            settings.Dispose();
        }
        #endregion


    }
}