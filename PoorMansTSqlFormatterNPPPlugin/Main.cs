/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
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


/* 
-------------
TESTING NOTES
-------------

Unfortunately I haven't devised (or found) any method of automating integration tests against NPP/Scintilla,
so testing is manual. The important things that I have found to test so far include:
 * Long documents (hundreds of kb)
 * Documents with "interesting" characters (CP-1252, Arabic, Chinese, and Smileys for example)
 * Documents in other encodings, eg UTF-16
 * Documents with different line ending schemes - CrLf, Lf, and Cr
 * Settings storage & retrieval
 * "About" dialog display
 * ...

For different types of documents, things to check include:
 * Try a first-format for both selection and no selection
 * That the first format results in a reasonable length wrt the original
 * That no interesting characters have been mangled
 * That line endings are consistent with the doc type
 * That the first format without selection leaves the cursor in a reasonable place
 * That later formats have no impact / make no change
 * That later formats without selection leave the cursor in the *same* place

*/


using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using PoorMansTSqlFormatterPluginShared;

namespace Kbg.NppPluginNET
{
    class Main
    {
        #region " Fields "
        internal const string PluginName = "Poor Man's T-Sql Formatter";
        static string iniFilePath = null;
        static PoorMansTSqlFormatterLib.SqlFormattingManager _formattingManager = null;
        static ResourceManager _generalResourceManager = new ResourceManager("PoorMansTSqlFormatterNppPlugin.GeneralLanguageContent", Assembly.GetExecutingAssembly());
        static IScintillaGateway editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
        #endregion

        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            //this is where I'd really like access to language info from Notepad++ context...
            //MessageBox.Show(string.Format("Cult: {0}; UICult: {1}", System.Threading.Thread.CurrentThread.CurrentCulture.EnglishName, System.Threading.Thread.CurrentThread.CurrentUICulture.EnglishName));

            //get settings from notepad++-assigned plugin data folder, and set for settings provider.
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            string iniFolder = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFolder)) Directory.CreateDirectory(iniFolder);
            iniFilePath = Path.Combine(iniFolder, PluginName + ".ini.xml");
            PoorMansTSqlFormatterNPPPlugin.Properties.Settings.Default.Context["settingsPath"] = iniFilePath;

            _formattingManager = Utils.GetFormattingManager(PoorMansTSqlFormatterNPPPlugin.Properties.Settings.Default);

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
            string inputText;
            string outputText;

            int selectionBufferLength = editor.GetSelectionLength();
            if (selectionBufferLength > 0)
            {
                //auto-generated "editor.GetSelText()" implementation in ScintillaGateway is broken, bottoms out at 10,000 utf-8 bytes
                inputText = editor.GetSelText_Fixed();

                //if formatting is successful or user chooses to continue despite error
                if (FormatAndWarn(inputText, out outputText))
                {
                    //replace the selection with the formatted content
                    editor.ReplaceSel(outputText);
                    //position of the cursor will automatically be the end of the replaced selection
                }
            }
            else
            {
                /* Scintilla's idea of text length is a little weird; a multibyte character like 😀
                is recorded as length 4 in this "text length" measurement... but counted as 1 in a
                "selection length" measurement!
                You can see this inconsistency in NPP if you have a short doc, you look at the doc
                length in the status bar, you select the whole doc, and you look at your selection
                length - the more smileys in the text, the greater the discrepancy.

                It's tempting to assume it's a "bytes" number then, but it's definitely not - a
                utf-16 doc still shows up as the same number of characters as a utf-8 doc. It looks
                like it's probably a "bytes when encoded in utf-8" number.

                Scintilla's number should only be used in its text-targeting APIs; for text length
                arithmetic, use .Net's own understanding of string lengths.
                */
                int editorTextLength = editor.GetTextLength();

                int cursorPosition = editor.GetCurrentPos();
                //auto-generated "editor.GetText()" implementation in ScintillaGateway is broken, bottoms out at 10,000 utf-8 bytes
                inputText = editor.GetText_Fixed(editorTextLength);

                if (FormatAndWarn(inputText, out outputText))
                {
                    int newPosition = (int)Math.Round(1.0 * cursorPosition * outputText.Length / inputText.Length, 0, MidpointRounding.AwayFromZero);
                    editor.SetText(outputText);
                    editor.SetEmptySelection(newPosition);
                    editor.ScrollCaret();
                }
            }
        }

        private static bool FormatAndWarn(string inputString, out string outputString)
        {
            bool errorsEncountered = false;
            outputString = _formattingManager.Format(inputString, ref errorsEncountered);

            // The formatting library uses Environment.NewLine. In .Net on Windows that's CrLf.
            var lineEnding = editor.GetEOLMode();
            if (lineEnding == EndOfLine.CR)
                outputString = outputString.Replace("\r\n", "\r");
            else if (lineEnding == EndOfLine.LF)
                outputString = outputString.Replace("\r\n", "\n");

            if (errorsEncountered)
                if (MessageBox.Show(_generalResourceManager.GetString("ParseErrorWarningMessage"), _generalResourceManager.GetString("ParseErrorWarningMessageTitle"), MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return false;

            //true means go ahead
            return true;
        }

        internal static void formattingOptionsCommand()
        {
            SettingsForm settings = new SettingsForm(PoorMansTSqlFormatterNPPPlugin.Properties.Settings.Default, Assembly.GetExecutingAssembly(), _generalResourceManager.GetString("ProjectAboutDescription"));
            if (settings.ShowDialog() == DialogResult.OK)
            {
                _formattingManager = Utils.GetFormattingManager(PoorMansTSqlFormatterNPPPlugin.Properties.Settings.Default);
            }
            settings.Dispose();
        }
        #endregion

    }
}