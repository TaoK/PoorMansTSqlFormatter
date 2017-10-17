/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
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
using PoorMansTSqlFormatterLib;
using System;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using PluginShared = PoorMansTSqlFormatterPluginShared;

namespace PoorMansTSqlFormatterSSMSLib
{
    public class GenericVSHelper
    {
        public delegate string GetTextEditorKeyBindingScopeName();
        public delegate string GetKeyBinding();
        public delegate void SetKeyBinding(string KeyShortcut);

        private ResourceManager _generalResourceManager = new ResourceManager("PoorMansTSqlFormatterSSMSLib.GeneralLanguageContent", Assembly.GetExecutingAssembly());
        private SqlFormattingManager _formattingManager = null;
        private bool _isVisualStudio = false;
        private GetTextEditorKeyBindingScopeName _getKeyBindingScopeNameDelegate = null;
        private GetKeyBinding _getKeyBindingDelegate = null;
        private SetKeyBinding _setKeyBindingDelegate = null;

        public ResourceManager GeneralResourceManager { get { return _generalResourceManager; } }
        public SqlFormattingManager FormattingManager { get { return _formattingManager; } }
        public Properties.Settings Settings { get { return Properties.Settings.Default; } }

        public GenericVSHelper(bool isVisualStudio, GetTextEditorKeyBindingScopeName keyBindingScopeNameDelegate, GetKeyBinding getKeyBindingDelegate, SetKeyBinding setKeyBindingDelegate)
		{
            _isVisualStudio = isVisualStudio;
            _getKeyBindingScopeNameDelegate = keyBindingScopeNameDelegate;
            _getKeyBindingDelegate = getKeyBindingDelegate;
            _setKeyBindingDelegate = setKeyBindingDelegate;

            //upgrade settings if necessary.    
            if (!Properties.Settings.Default.UpgradeCompleted)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeCompleted = true;
                Properties.Settings.Default.Save();
            }

            if (!Properties.Settings.Default.FirstInstallCompleted)
            {
                if (_isVisualStudio || _getKeyBindingScopeNameDelegate == null)
                    //no default shortcut in VS, nor do we store it in settings if VSPackage context / managed through VS Shell only
                    Properties.Settings.Default.Hotkey = "";
                else
                    Properties.Settings.Default.Hotkey = FixHotkeyDefault(Properties.Settings.Default.Hotkey);

                Properties.Settings.Default.FirstInstallCompleted = true;
                Properties.Settings.Default.Save();
            }

            //set up formatter (note - after changes to Settings through the UI this line will appear to error, 
            // with settings not implementing the necessary interface, but a prebuild search & replace step will 
            // automatically fix the settings file)
            _formattingManager = PluginShared.Utils.GetFormattingManager(Properties.Settings.Default);
        }

        public string FixHotkeyDefault(string rawDefault)
        {
            string scopeName = _getKeyBindingScopeNameDelegate();

            if (rawDefault == null || scopeName == null)
                return rawDefault;

            return rawDefault.Replace("Text Editor", scopeName);
        }
        
        public void FormatSqlInTextDoc(DTE2 dte)
        {

            //TODO: Add check for no active doc (with translation, etc)
            //TODO: Add check for single-line doc, causes silly eror

            string fileExtension = System.IO.Path.GetExtension(dte.ActiveDocument.FullName);
            bool isSqlFile = fileExtension.ToUpper().Equals(".SQL");

            if (isSqlFile ||
                MessageBox.Show(_generalResourceManager.GetString("FileTypeWarningMessage"), _generalResourceManager.GetString("FileTypeWarningMessageTitle"), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string fullText = SelectAllCodeFromDocument(dte.ActiveDocument);
                TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;
                if (!selection.IsActiveEndGreater)
                    selection.SwapAnchor();
                if (selection.Text.EndsWith(Environment.NewLine) || selection.Text.EndsWith(" "))
                    selection.CharLeft(true, 1); //newline counts as a distance of one.
                string selectionText = selection.Text;
                bool formatSelectionOnly = selectionText.Length > 0 && selectionText.Length != fullText.Length;
                int cursorPoint = selection.ActivePoint.AbsoluteCharOffset;

                string textToFormat = formatSelectionOnly ? selectionText : fullText;
                bool errorsFound = false;
                string formattedText = _formattingManager.Format(textToFormat, ref errorsFound);

                bool abortFormatting = false;
                if (errorsFound)
                    abortFormatting = MessageBox.Show(_generalResourceManager.GetString("ParseErrorWarningMessage"), _generalResourceManager.GetString("ParseErrorWarningMessageTitle"), MessageBoxButtons.YesNo) != DialogResult.Yes;

                if (!abortFormatting)
                {
                    if (formatSelectionOnly)
                    {
                        //if selection just delete/insert, so the active point is at the end of the selection
                        selection.Delete(1);
                        selection.Insert(formattedText, (int)EnvDTE.vsInsertFlags.vsInsertFlagsContainNewText);
                    }
                    else
                    {
                        //if whole doc then replace all text, and put the cursor approximately where it was (using proportion of text total length before and after)
                        int newPosition = (int)Math.Round(1.0 * cursorPoint * formattedText.Length / textToFormat.Length, 0, MidpointRounding.AwayFromZero);
                        ReplaceAllCodeInDocument(dte.ActiveDocument, formattedText);
                        ((TextSelection)(dte.ActiveDocument.Selection)).MoveToAbsoluteOffset(newPosition, false);
                    }
                }
            }
        }

        //Nice clean methods avoiding slow selection-editing, from online post at:
        //  http://www.visualstudiodev.com/visual-studio-extensibility/how-can-i-edit-documents-programatically-22319.shtml
        public static string SelectAllCodeFromDocument(Document targetDoc)
        {
            string outText = "";
            TextDocument textDoc = targetDoc.Object("TextDocument") as TextDocument;
            if (textDoc != null)
                outText = textDoc.StartPoint.CreateEditPoint().GetText(textDoc.EndPoint);
            return outText;
        }

        public static void ReplaceAllCodeInDocument(Document targetDoc, string newText)
        {
            TextDocument textDoc = targetDoc.Object("TextDocument") as TextDocument;
            if (textDoc != null)
            {
                textDoc.StartPoint.CreateEditPoint().Delete(textDoc.EndPoint);
                textDoc.StartPoint.CreateEditPoint().Insert(newText);
            }
        }

        public void GetUpdatedFormattingOptionsFromUser()
        {
            if (_getKeyBindingScopeNameDelegate != null)
                GetVSHotkeyIntoSettings();

            PluginShared.SettingsForm settings = new PluginShared.SettingsForm(Properties.Settings.Default, Assembly.GetExecutingAssembly(), _generalResourceManager.GetString("ProjectAboutDescription"), _getKeyBindingScopeNameDelegate == null ? null : new PluginShared.SettingsForm.FixHotkeyDefault(FixHotkeyDefault));

            if (settings.ShowDialog() == DialogResult.OK)
            {
                _formattingManager = PluginShared.Utils.GetFormattingManager(Properties.Settings.Default);
                if (_getKeyBindingScopeNameDelegate != null)
                    UpdateSettingsHotkeyIntoVS();
            }
            
            settings.Dispose();
        }

        public void GetVSHotkeyIntoSettings()
        {
            try
            {
                string flatBindingsValue = _getKeyBindingDelegate();

                if (Properties.Settings.Default.Hotkey != flatBindingsValue)
                {
                    Properties.Settings.Default.Hotkey = flatBindingsValue;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(_generalResourceManager.GetString("HotkeyRetrievalFailureMessage"), Environment.NewLine, e.ToString()));
            }
        }

        public void UpdateSettingsHotkeyIntoVS()
        {
            try
            {
                _setKeyBindingDelegate(Properties.Settings.Default.Hotkey);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(_generalResourceManager.GetString("HotkeyBindingFailureMessage"), Environment.NewLine, e.ToString()));
            }
        }
    }
}
