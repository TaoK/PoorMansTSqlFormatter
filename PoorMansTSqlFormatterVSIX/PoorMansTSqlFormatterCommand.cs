//------------------------------------------------------------------------------
// <copyright file="PoorMansTSqlFormatterCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PoorMansTSqlFormatterPluginShared;

namespace PoorMansTSqlFormatterVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PoorMansTSqlFormatterCommand
    {
        private const int OLEMSGRESULT_YES = 6;
        private PoorMansTSqlFormatterLib.SqlFormattingManager _formattingManager = null;
        private ResourceManager _generalResourceManager = new ResourceManager("PoorMansTSqlFormatterVSIX.GeneralLanguageContent", Assembly.GetExecutingAssembly());
        private DTE _applicationObject;
        private Command _formatCommand;
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int FormatCommandId = 0x0100;
        public const int OptionsCommandId = 0x0200;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("dce24cb1-eba1-4a3d-a34e-82ffa04492ef");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoorMansTSqlFormatterCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private PoorMansTSqlFormatterCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                _applicationObject = Package.GetGlobalService(typeof(DTE)) as DTE;
                _formattingManager = PoorMansTSqlFormatterPluginShared.Utils.GetFormattingManager(Properties.Settings.Default);
                //Command cmd = _applicationObject.Commands.Item("Tools.FormatTSQLCode", -1);
                //cmd.Bindings = "Text Editor::Ctrl+Shift+D";

                var menuFormatCommandID = new CommandID(CommandSet, FormatCommandId);
                var menuFormatItem = new MenuCommand(this.MenuFormatCallback, menuFormatCommandID);
                commandService.AddCommand(menuFormatItem);

                var menuOptionsCommandID = new CommandID(CommandSet, OptionsCommandId);
                var menuOptionsItem = new MenuCommand(this.MenuOptionsCallback, menuOptionsCommandID);
                commandService.AddCommand(menuOptionsItem);

                _formatCommand = _applicationObject.Commands.Item("Tools.FormatTSQLCode", -1);
                SetFormatHotkey();

            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static PoorMansTSqlFormatterCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new PoorMansTSqlFormatterCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuFormatCallback(object sender, EventArgs e)
        {
            if (_applicationObject.ActiveDocument == null) return;

            string fileExtension = System.IO.Path.GetExtension(_applicationObject.ActiveDocument.FullName);
            bool isSqlFile = fileExtension.ToUpper().Equals(".SQL");

            if (isSqlFile ||

                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    _generalResourceManager.GetString("FileTypeWarningMessage"),
                    _generalResourceManager.GetString("FileTypeWarningMessageTitle"),
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
                    ) == OLEMSGRESULT_YES)
            {
                string fullText = SelectAllCodeFromDocument(_applicationObject.ActiveDocument);
                TextSelection selection = (TextSelection)_applicationObject.ActiveDocument.Selection;
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
                    abortFormatting =
                         VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    _generalResourceManager.GetString("ParseErrorWarningMessage"),
                    _generalResourceManager.GetString("ParseErrorWarningMessageTitle"),
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
                    ) != OLEMSGRESULT_YES;

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
                        //int newPosition = (int)Math.Round(1.0 * cursorPoint * formattedText.Length / textToFormat.Length, 0, MidpointRounding.AwayFromZero);
                        ReplaceAllCodeInDocument(_applicationObject.ActiveDocument, formattedText);
                        //((TextSelection)(_applicationObject.ActiveDocument.Selection)).MoveToAbsoluteOffset(newPosition, false);
                    }
                }
            }
        }

        private string GetTextEditorKeyBindingScopeName()
        {
            string strScope = null;
            try
            {
                //"dirty hack" (as its author puts it) to get localized Text Editor scope name in 
                // non-english instalations - but it works! (without having access to "IVsShell")
                // Thank you Roland Weigelt! http://weblogs.asp.net/rweigelt/archive/2006/07/16/458634.aspx
                Command cmd = _applicationObject.Commands.Item("Edit.DeleteBackwards", -1);
                object[] arrBindings = (object[])cmd.Bindings;
                string strBinding = (string)arrBindings[0];
                strScope = strBinding.Substring(0, strBinding.IndexOf("::"));
            }
            catch (Exception ex)
            {
                //I know, general catch blocks are evil - but honestly, if that failed, what can we do?? I have no idea what types of issues to expect!
                MessageBox.Show(string.Format(_generalResourceManager.GetString("TextEditorScopeNameRetrievalFailureMessage"), Environment.NewLine, ex.ToString()));
            }
            return strScope;
        }
        private void MenuOptionsCallback(object sender, EventArgs e)
        {
            GetFormatHotkey();
            SettingsForm settings = new SettingsForm(Properties.Settings.Default, Assembly.GetExecutingAssembly(), _generalResourceManager.GetString("ProjectAboutDescription"), new SettingsForm.GetTextEditorKeyBindingScopeName(GetTextEditorKeyBindingScopeName));
            if (settings.ShowDialog() == DialogResult.OK)
            {
                SetFormatHotkey();
                _formattingManager = Utils.GetFormattingManager(Properties.Settings.Default);
            }
        }

        private static string SelectAllCodeFromDocument(Document targetDoc)
        {
            string outText = "";
            TextDocument textDoc = targetDoc.Object("TextDocument") as TextDocument;
            if (textDoc != null)
                outText = textDoc.StartPoint.CreateEditPoint().GetText(textDoc.EndPoint);
            return outText;
        }

        private static void ReplaceAllCodeInDocument(Document targetDoc, string newText)
        {
            TextDocument textDoc = targetDoc.Object("TextDocument") as TextDocument;
            if (textDoc != null)
            {
                textDoc.StartPoint.CreateEditPoint().ReplaceText(textDoc.EndPoint, newText, (int)vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers);
            }
        }

        private void SetFormatHotkey()
        {
            try
            {
                
                //TODO: Add support for multiple keybindings.
                if (Properties.Settings.Default.Hotkey == null || Properties.Settings.Default.Hotkey.Trim() == "")
                    _formatCommand.Bindings = new object[0];
                else
                    _formatCommand.Bindings = Properties.Settings.Default.Hotkey;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(_generalResourceManager.GetString("HotkeyBindingFailureMessage"), Environment.NewLine, e.ToString()));
            }
        }
        private void GetFormatHotkey()
        {
            try
            {
                //TODO: Add support for multiple keybindings.
                string flatBindingsValue = "";
                var bindingArray = _formatCommand.Bindings as object[];
                if (bindingArray != null && bindingArray.Length > 0)
                    flatBindingsValue = bindingArray[0].ToString();

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
    }
}
