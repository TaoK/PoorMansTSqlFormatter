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
using System.Windows.Forms;
using NppPluginNET;
using System.Reflection;

namespace PoorMansTSqlFormatterNppPlugin
{
    class Main
    {
        /* 
         * First draft of Poor Man's T-SQL formatter plugin for Notepad++:
         *  - One command on the menu, no other functionality
         *     - Reformats the selected code as T-SQL
         *     - If there is no selection, reformats the entire file (scintilla buffer/window, rather)
         *     - If a parsing error is encountered, request is aborted with an error
         *  - Keyboards shortcut can be assigned using notepad++ built-in feature: Settings -> Shortcut Mapper...
         *     - If anyone has a suggestion for a default mapping, I'm all ears (the default MS ones are taken I think)
         *  
         * Future functionality (for first "Official" release):
         *  - Option to format even if a parsing error is encountered
         *  - Default file extension check with warning if doesn't appear to be a sql file
         *    - option to customize list file extensions that are expected to be SQL
         *  - Formatting options, like in SSMS plugin or UI program.
         *  - Translation? (don't know how locale information is propagated to plugins in notepad++ yet)
         */

        #region " Fields "
        internal const string PluginName = "PoorMansTSqlFormatter";
        #endregion

        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            //set up assembly loading path
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromPluginSubFolder);

            //set up menu items
            PluginBase.SetCommand(0, "Format T-SQL", formatSqlCommand, new ShortcutKey(false, false, false, Keys.None));
        }

        static Assembly LoadFromPluginSubFolder(object sender, ResolveEventArgs args)
        {
            Assembly pluginAssembly = Assembly.GetExecutingAssembly();
            string pluginPath = pluginAssembly.Location;
            string pluginName = Path.GetFileNameWithoutExtension(pluginPath);
            string pluginSubFolder = Path.Combine(Path.GetDirectoryName(pluginPath), pluginName);
            string assemblyPath = Path.Combine(pluginSubFolder, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath) == false) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        internal static void PluginCleanUp()
        {
        }
        #endregion

        #region " Menu functions "
        internal static void formatSqlCommand()
        {
            StringBuilder textBuffer = null;

            IntPtr currentScintilla = PluginBase.GetCurrentScintilla();

            //apparently calling with null pointer returns selection buffer length: http://www.scintilla.org/ScintillaDoc.html#SCI_GETSELTEXT
            int selectionBufferLength = (int)Win32.SendMessage(currentScintilla, SciMsg.SCI_GETSELTEXT, 0, 0);
            bool isSelection = false;

            if (selectionBufferLength > 1)
            {
                textBuffer = new StringBuilder(selectionBufferLength);
                Win32.SendMessage(currentScintilla, SciMsg.SCI_GETSELTEXT, 0, textBuffer);
                isSelection = true;
            }
            else
            {
                //Do as they say here:
                //http://www.scintilla.org/ScintillaDoc.html#SCI_GETTEXT
                int docBufferLength = (int)Win32.SendMessage(currentScintilla, SciMsg.SCI_GETTEXT, 0, 0);
                textBuffer = new StringBuilder(docBufferLength);
                Win32.SendMessage(currentScintilla, SciMsg.SCI_GETTEXT, docBufferLength, textBuffer);
                isSelection = false;
            }

            bool errorsEncountered = false;
            StringBuilder outBuffer = new StringBuilder(PoorMansTSqlFormatterLib.SqlFormattingManager.DefaultFormat(textBuffer.ToString(), ref errorsEncountered));

            if (!errorsEncountered)
            {
                if (isSelection)
                    Win32.SendMessage(currentScintilla, SciMsg.SCI_REPLACESEL, 0, outBuffer);
                else
                    Win32.SendMessage(currentScintilla, SciMsg.SCI_SETTEXT, 0, outBuffer);
            }
            else
            {
                MessageBox.Show("Encountered a parsing error, sorry!");
            }
        }
        #endregion


    }
}