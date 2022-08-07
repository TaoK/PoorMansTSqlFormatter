﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.IO;
using System.Reflection;

namespace Kbg.NppPluginNET.PluginInfrastructure
{
    class PluginBase
    {
        internal static NppData nppData;
        internal static FuncItems _funcItems = new FuncItems();

        #region " Set-up of standard supporting assembly location "
        static PluginBase()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSameFolderOrPluginSubFolder);
        }

        static Assembly LoadFromSameFolderOrPluginSubFolder(object sender, ResolveEventArgs args)
        {
            string pluginPath = typeof(PluginBase).Assembly.Location;
            string pluginName = Path.GetFileNameWithoutExtension(pluginPath);
            string pluginFolder = Path.GetDirectoryName(pluginPath);
            string assemblyLocalFolderPath = Path.Combine(pluginFolder, new AssemblyName(args.Name).Name + ".dll");
            string pluginSubFolder = Path.Combine(pluginFolder, pluginName);
            string assemblySubFolderPath = Path.Combine(pluginSubFolder, new AssemblyName(args.Name).Name + ".dll");

            if (File.Exists(assemblyLocalFolderPath))
                return Assembly.LoadFrom(assemblyLocalFolderPath);
            else if (File.Exists(assemblySubFolderPath))
                return Assembly.LoadFrom(assemblySubFolderPath);
            else
                return null;
        }
        #endregion

        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), false);
        }
        
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut)
        {
            SetCommand(index, commandName, functionPointer, shortcut, false);
        }
        
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), checkOnInit);
        }
        
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit)
        {
            FuncItem funcItem = new FuncItem();
            funcItem._cmdID = index;
            funcItem._itemName = commandName;
            if (functionPointer != null)
                funcItem._pFunc = new NppFuncItemDelegate(functionPointer);
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            _funcItems.Add(funcItem);
        }

        internal static IntPtr GetCurrentScintilla()
        {
            int curScintilla;
            Win32.SendMessage(nppData._nppHandle, (uint) NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            return (curScintilla == 0) ? nppData._scintillaMainHandle : nppData._scintillaSecondHandle;
        }


        static readonly Func<IScintillaGateway> gatewayFactory = () => new ScintillaGateway(GetCurrentScintilla());

        public static Func<IScintillaGateway> GetGatewayFactory()
        {
            return gatewayFactory;
        }
    }
}
