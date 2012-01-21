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
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PoorMansTSqlFormatterSSMSAddIn
{
	/// <summary>This is the class that will be instantiated by the VS environment to load the add-in.</summary>
	public class AddinConnector : IDTExtensibility2, IDTCommandTarget
	{
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Command _formatCommand;

        private ResourceManager _generalResourceManager = new ResourceManager("PoorMansTSqlFormatterSSMSAddIn.GeneralLanguageContent", Assembly.GetExecutingAssembly());

        /// <summary>Constructor - non-environment-related initialization here.</summary>
		public AddinConnector()
		{
            //upgrade settings if necessary.
            if (!Properties.Settings.Default.UpgradeCompleted)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeCompleted = true;
                Properties.Settings.Default.Save();
            }
		}

		/// <summary>Environment situation established here.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
            _addInInstance = (AddIn)addInInst;
            _applicationObject = (DTE2)_addInInstance.DTE;
			if(connectMode == ext_ConnectMode.ext_cm_UISetup || connectMode == ext_ConnectMode.ext_cm_Startup)
			{
				object []contextGUIDS = new object[] { };
                Commands2 commandsList = (Commands2)_applicationObject.Commands;
                CommandBarPopup targetPopup = GetMainMenuPopup("Tools");

                //remove old commands
                List<string> oldCommandNames = new List<string>();
                oldCommandNames.Add("PoorMansTSqlFormatterSSMSAddIn.Connect.PoorMansTSqlFormatterSSMSAddIn");
                oldCommandNames.Add("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormatSelectionOrActiveWindow");
                oldCommandNames.Add("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormattingOptions");
                RemoveCommands(commandsList, oldCommandNames);

                //add new commands
                Command formatCommand = commandsList.AddNamedCommand2(
                    _addInInstance,
                    "FormatSelectionOrActiveWindow",
                    _generalResourceManager.GetString("FormatButtonText"),
                    _generalResourceManager.GetString("FormatButtonToolTip"),
                    true,
                    59,
                    ref contextGUIDS,
                    (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                    (int)vsCommandStyle.vsCommandStyleText,
                    vsCommandControlType.vsCommandControlTypeButton
                    );
                if ((formatCommand != null) && (targetPopup != null))
                    formatCommand.AddControl(targetPopup.CommandBar, 1);
                _formatCommand = formatCommand;
                SetFormatHotkey();

                Command optionsCommand = commandsList.AddNamedCommand2(
                    _addInInstance,
                    "FormattingOptions",
                    _generalResourceManager.GetString("OptionsButtonText"),
                    _generalResourceManager.GetString("OptionsButtonToolTip"),
                    true,
                    59,
                    ref contextGUIDS,
                    (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                    (int)vsCommandStyle.vsCommandStyleText,
                    vsCommandControlType.vsCommandControlTypeButton
                    );
                if ((optionsCommand != null) && (targetPopup != null))
                    optionsCommand.AddControl(targetPopup.CommandBar, 2);

            }
		}

        private CommandBarPopup GetMainMenuPopup(string targetMenuEnglishName)
        {
            //template-generated code to find the localized name of the target menu
            string toolsMenuName;
            try
            {
                string resourceName;
                ResourceManager resourceManager = new ResourceManager(typeof(AddinConnector).Namespace + ".CommandBar", Assembly.GetExecutingAssembly());
                CultureInfo cultureInfo = new CultureInfo(_applicationObject.LocaleID);

                if (cultureInfo.TwoLetterISOLanguageName == "zh")
                {
                    System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
                    resourceName = String.Concat(parentCultureInfo.Name, targetMenuEnglishName);
                }
                else
                {
                    resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, targetMenuEnglishName);
                }
                toolsMenuName = resourceManager.GetString(resourceName);
            }
            catch
            {
                //none found, use english as default.
                toolsMenuName = targetMenuEnglishName;
            }

            //Place the command on the tools menu.
            //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

            //Find the Tools command bar on the MenuBar command bar:
            CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
            CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;
            return toolsPopup;
        }

        private static void RemoveCommands(Commands2 commandsList, List<string> oldCommandNames)
        {
            List<Command> oldCommands = new List<Command>();
            foreach (Command candidate in commandsList)
                if (oldCommandNames.Contains(candidate.Name))
                    oldCommands.Add(candidate);

            foreach (Command oldEntry in oldCommands)
                oldEntry.Delete();
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{

                if (commandName.Equals("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormatSelectionOrActiveWindow")
                    || commandName.Equals("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormattingOptions")
                    )
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName.Equals("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormatSelectionOrActiveWindow"))
                {
                    string fileExtension = System.IO.Path.GetExtension(_applicationObject.ActiveDocument.FullName);
                    bool isSqlFile = fileExtension.ToUpper().Equals(".SQL");

                    if (isSqlFile ||
                        MessageBox.Show(_generalResourceManager.GetString("FileTypeWarningMessage"), _generalResourceManager.GetString("FileTypeWarningMessageTitle"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        string fullText = SelectAllCodeFromDocument(_applicationObject.ActiveDocument);
                        TextSelection selection = (TextSelection)_applicationObject.ActiveDocument.Selection;
                        if (!selection.IsActiveEndGreater) 
                            selection.SwapAnchor();
                        if (selection.Text.EndsWith(Environment.NewLine) || selection.Text.EndsWith(" ")) 
                            selection.CharLeft(true, 1); //newline counts as a distance of one.
                        string selectionText = selection.Text;
                        bool formatSelectionOnly = selectionText.Length > 0 && selectionText.Length != fullText.Length;


                        string textToFormat = formatSelectionOnly ? selectionText : fullText;
                        var formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(
                            Properties.Settings.Default.IndentString.Replace("\\t", "\t"),
                            Properties.Settings.Default.SpacesPerTab,
                            Properties.Settings.Default.MaxLineWidth,
                            Properties.Settings.Default.ExpandCommaLists,
                            Properties.Settings.Default.TrailingCommas,
                            Properties.Settings.Default.SpaceAfterExpandedComma,
                            Properties.Settings.Default.ExpandBooleanExpressions, 
                            Properties.Settings.Default.ExpandCaseStatements,
                            Properties.Settings.Default.ExpandBetweenConditions,
                            Properties.Settings.Default.BreakJoinOnSections,
                            Properties.Settings.Default.UppercaseKeywords, 
                            false,
                            Properties.Settings.Default.KeywordStandardization
                            );
                        formatter.ErrorOutputPrefix = _generalResourceManager.GetString("ParseErrorWarningPrefix");
                        var formattingManager = new PoorMansTSqlFormatterLib.SqlFormattingManager(formatter);
                        bool errorsFound = false;
                        string formattedText = formattingManager.Format(textToFormat, ref errorsFound);

                        bool abortFormatting = false;
                        if (errorsFound)
                            abortFormatting = MessageBox.Show(_generalResourceManager.GetString("ParseErrorWarningMessage"), _generalResourceManager.GetString("ParseErrorWarningMessageTitle"), MessageBoxButtons.YesNo) != DialogResult.Yes;

                        if (!abortFormatting)
                        {
                            if (formatSelectionOnly)
                            {
                                selection.Delete(1);
                                selection.Insert(formattedText, (int)EnvDTE.vsInsertFlags.vsInsertFlagsContainNewText);
                            }
                            else
                                ReplaceAllCodeInDocument(_applicationObject.ActiveDocument, formattedText);
                        }
                    }

                    handled = true;
                    return;
                }
                if (commandName.Equals("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormattingOptions"))
                {
                    SettingsForm settings = new SettingsForm();
                    if (settings.ShowDialog() == DialogResult.OK)
                    {
                        SetFormatHotkey();
                    }
                    settings.Dispose();
                }
            }
		}

        private void SetFormatHotkey()
        {
            try
            {
                _formatCommand.Bindings = Properties.Settings.Default.Hotkey;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(_generalResourceManager.GetString("HotkeyBindingFailureMessage"), Environment.NewLine, e.ToString()));
            }
        }

        //Nice clean methods avoiding slow selection-editing, from online post at:
        //  http://www.visualstudiodev.com/visual-studio-extensibility/how-can-i-edit-documents-programatically-22319.shtml
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
                textDoc.StartPoint.CreateEditPoint().Delete(textDoc.EndPoint);
                textDoc.StartPoint.CreateEditPoint().Insert(newText);
            }
        }
    }
}