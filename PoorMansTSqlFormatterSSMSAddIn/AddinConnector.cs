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
using PoorMansTSqlFormatterSSMSLib;

namespace PoorMansTSqlFormatterSSMSAddIn
{
	public class AddinConnector : IDTExtensibility2, IDTCommandTarget
	{
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Command _formatCommand;
        private GenericVSHelper _SSMSHelper;

        public AddinConnector()
        {
        }

		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
            _addInInstance = (AddIn)addInInst;
            _applicationObject = (DTE2)_addInInstance.DTE;

			if(connectMode == ext_ConnectMode.ext_cm_UISetup || connectMode == ext_ConnectMode.ext_cm_Startup)
			{
                _SSMSHelper = new GenericVSHelper(_applicationObject.RegistryRoot.StartsWith(@"Software\Microsoft\VisualStudio"), new GenericVSHelper.GetTextEditorKeyBindingScopeName(GetTextEditorKeyBindingScopeName), new GenericVSHelper.GetKeyBinding(GetFormatCommandKeyBinding), new GenericVSHelper.SetKeyBinding(SetFormatCommandKeyBinding));

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
                    _SSMSHelper.GeneralResourceManager.GetString("FormatButtonText"),
                    _SSMSHelper.GeneralResourceManager.GetString("FormatButtonToolTip"),
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
                _SSMSHelper.UpdateSettingsHotkeyIntoVS();

                Command optionsCommand = commandsList.AddNamedCommand2(
                    _addInInstance,
                    "FormattingOptions",
                    _SSMSHelper.GeneralResourceManager.GetString("OptionsButtonText"),
                    _SSMSHelper.GeneralResourceManager.GetString("OptionsButtonToolTip"),
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
                MessageBox.Show(string.Format(_SSMSHelper.GeneralResourceManager.GetString("TextEditorScopeNameRetrievalFailureMessage"), Environment.NewLine, ex.ToString()));
            }
            return strScope;
        }

        private CommandBarPopup GetMainMenuPopup(string targetMenuEnglishName)
        {
            //template-generated code to find the localized name of the target menu
            //MODIFIED: the template code did not account for cases where the target language is not specified in the "CommandBar" resource
            // file, (eg Portuguese or Russian) so (as a cheap hack-ish solution) we create the menu if it doesn't exist.

            string localMenuName = null;
            try
            {
                string resourceName;
                ResourceManager resourceManager = new ResourceManager(typeof(AddinConnector).Namespace + ".CommandBar", Assembly.GetExecutingAssembly());
                CultureInfo cultureInfo = new CultureInfo(_applicationObject.LocaleID);

                //no idea why the two-letter ISO language name was not specific enough (or too specific??) for chinese... this comes from the VS2008 template
                if (cultureInfo.TwoLetterISOLanguageName == "zh")
                    resourceName = String.Concat(cultureInfo.Parent.Name, targetMenuEnglishName);
                else
                    resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, targetMenuEnglishName);

                localMenuName = resourceManager.GetString(resourceName);
            }
            catch
            {
                //Something went wrong with resource handling. In the absence of logging / error-reporting framework, just 
                // swallow the error and act same as if we didn't have that language in the resources file (leave null).
            }

            //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items; its name is always consistent (apparently)
            Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

            CommandBarPopup targetPopupMenu = null;

            //try to find the target menu entry on the menu bar, using the localized name
            if (!string.IsNullOrEmpty(localMenuName))
                targetPopupMenu = GetCommandBarControlOrNull(menuBarCommandBar.Controls, localMenuName);

            //if there was no local name, or the menu entry was not found using the local name, try English
            if (targetPopupMenu == null)
                targetPopupMenu = GetCommandBarControlOrNull(menuBarCommandBar.Controls, targetMenuEnglishName);

            //if the menu entry still wasn't found, then create it (temporarily) using English. A more elegant long-term solution 
            // might be to ask the user, but that would also be much more complicated. Let's see whether anyone cares.
            if (targetPopupMenu == null)
            {
                int newEntryPosition = menuBarCommandBar.Controls.Count - 2; //leave space for "Window" and "Help".
                targetPopupMenu = (CommandBarPopup)menuBarCommandBar.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, newEntryPosition, true);
                targetPopupMenu.CommandBar.Name = targetMenuEnglishName;
                targetPopupMenu.Caption = targetMenuEnglishName;
            }

            return targetPopupMenu;
        }

        private CommandBarPopup GetCommandBarControlOrNull(CommandBarControls controlsCollection, string controlKey)
        {
            CommandBarPopup found = null;
            foreach (CommandBarControl thisControl in controlsCollection)
                if (thisControl is CommandBarPopup && ((CommandBarPopup)thisControl).CommandBar.Name == controlKey)
                    found = (CommandBarPopup)thisControl;
            return found;
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

		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		public void OnAddInsUpdate(ref Array custom)
		{
		}

		public void OnStartupComplete(ref Array custom)
		{
		}

		public void OnBeginShutdown(ref Array custom)
		{
            _SSMSHelper.GetVSHotkeyIntoSettings();
        }
		
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

		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName.Equals("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormatSelectionOrActiveWindow"))
                    _SSMSHelper.FormatSqlInTextDoc(_applicationObject);
                else if (commandName.Equals("PoorMansTSqlFormatterSSMSAddIn.AddinConnector.FormattingOptions"))
                    _SSMSHelper.GetUpdatedFormattingOptionsFromUser();

                handled = true;
            }
		}

        private string GetFormatCommandKeyBinding()
        {
            //TODO: Add support for multiple keybindings.
            string flatBindingsValue = "";
            var bindingArray = _formatCommand.Bindings as object[];
            if (bindingArray != null && bindingArray.Length > 0)
                flatBindingsValue = bindingArray[0].ToString();

            return flatBindingsValue;
        }

        private void SetFormatCommandKeyBinding(string newBindingValue)
        {
            //TODO: Add support for multiple keybindings.
            if (newBindingValue == null || newBindingValue.Trim() == "")
                _formatCommand.Bindings = new object[0];
            else
                _formatCommand.Bindings = newBindingValue;
        }
    }
}