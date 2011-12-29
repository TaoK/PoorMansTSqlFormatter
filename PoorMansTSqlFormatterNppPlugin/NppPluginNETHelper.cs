using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NppPluginNET
{
    #region " Notepad++ "
    [StructLayout(LayoutKind.Sequential)]
    public struct NppData
    {
        public IntPtr _nppHandle;
        public IntPtr _scintillaMainHandle;
        public IntPtr _scintillaSecondHandle;
    }

    public delegate void NppFuncItemDelegate();

    [StructLayout(LayoutKind.Sequential)]
    public struct ShortcutKey
    {
        public ShortcutKey(bool isCtrl, bool isAlt, bool isShift, Keys key)
        {
            // the types 'bool' and 'char' have a size of 1 byte only!
            _isCtrl = Convert.ToByte(isCtrl);
            _isAlt = Convert.ToByte(isAlt);
            _isShift = Convert.ToByte(isShift);
            _key = Convert.ToByte(key);
        }
        public byte _isCtrl;
        public byte _isAlt;
        public byte _isShift;
        public byte _key;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FuncItem
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string _itemName;
        public NppFuncItemDelegate _pFunc;
        public int _cmdID;
        public bool _init2Check;
        public ShortcutKey _pShKey;
    }

    public class FuncItems : IDisposable
    {
        List<FuncItem> _funcItems;
        int _sizeFuncItem;
        List<IntPtr> _shortCutKeys;
        IntPtr _nativePointer;
        bool _disposed = false;

        public FuncItems()
        {
            _funcItems = new List<FuncItem>();
            _sizeFuncItem = Marshal.SizeOf(typeof(FuncItem));
            _shortCutKeys = new List<IntPtr>();
        }

        [DllImport("kernel32")]
        static extern void RtlMoveMemory(IntPtr Destination, IntPtr Source, int Length);
        public void Add(FuncItem funcItem)
        {
            int oldSize = _funcItems.Count * _sizeFuncItem;
            _funcItems.Add(funcItem);
            int newSize = _funcItems.Count * _sizeFuncItem;
            IntPtr newPointer = Marshal.AllocHGlobal(newSize);

            if (_nativePointer != IntPtr.Zero)
            {
                RtlMoveMemory(newPointer, _nativePointer, oldSize);
                Marshal.FreeHGlobal(_nativePointer);
            }
            IntPtr ptrPosNewItem = (IntPtr)((int)newPointer + oldSize);
            byte[] aB = Encoding.Unicode.GetBytes(funcItem._itemName + "\0");
            Marshal.Copy(aB, 0, ptrPosNewItem, aB.Length);
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 128);
            IntPtr p = (funcItem._pFunc != null) ? Marshal.GetFunctionPointerForDelegate(funcItem._pFunc) : IntPtr.Zero;
            Marshal.WriteIntPtr(ptrPosNewItem, p);
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + IntPtr.Size);
            Marshal.WriteInt32(ptrPosNewItem, funcItem._cmdID);
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 4);
            Marshal.WriteInt32(ptrPosNewItem, Convert.ToInt32(funcItem._init2Check));
            ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 4);
            if (funcItem._pShKey._key != 0)
            {
                IntPtr newShortCutKey = Marshal.AllocHGlobal(4);
                Marshal.StructureToPtr(funcItem._pShKey, newShortCutKey, false);
                Marshal.WriteIntPtr(ptrPosNewItem, newShortCutKey);
            }
            else Marshal.WriteIntPtr(ptrPosNewItem, IntPtr.Zero);

            _nativePointer = newPointer;
        }

        public void RefreshItems()
        {
            IntPtr ptrPosItem = _nativePointer;
            for (int i = 0; i < _funcItems.Count; i++)
            {
                FuncItem updatedItem = new FuncItem();
                updatedItem._itemName = _funcItems[i]._itemName;
                ptrPosItem = (IntPtr)((int)ptrPosItem + 128);
                updatedItem._pFunc = _funcItems[i]._pFunc;
                ptrPosItem = (IntPtr)((int)ptrPosItem + IntPtr.Size);
                updatedItem._cmdID = Marshal.ReadInt32(ptrPosItem);
                ptrPosItem = (IntPtr)((int)ptrPosItem + 4);
                updatedItem._init2Check = _funcItems[i]._init2Check;
                ptrPosItem = (IntPtr)((int)ptrPosItem + 4);
                updatedItem._pShKey = _funcItems[i]._pShKey;
                ptrPosItem = (IntPtr)((int)ptrPosItem + IntPtr.Size);

                _funcItems[i] = updatedItem;
            }
        }

        public IntPtr NativePointer { get { return _nativePointer; } }
        public List<FuncItem> Items { get { return _funcItems; } }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (IntPtr ptr in _shortCutKeys) Marshal.FreeHGlobal(ptr);
                if (_nativePointer != IntPtr.Zero) Marshal.FreeHGlobal(_nativePointer);
                _disposed = true;
            }
        }
        ~FuncItems()
        {
            Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public RECT(int left, int top, int right, int bottom)
        {
            Left = left; Top = top; Right = right; Bottom = bottom;
        }
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [Flags]
    public enum NppTbMsg : uint
    {
        // styles for containers
        //CAPTION_TOP                = 1,
        //CAPTION_BOTTOM            = 0,

        // defines for docking manager
        CONT_LEFT        = 0,
        CONT_RIGHT        = 1,
        CONT_TOP        = 2,
        CONT_BOTTOM        = 3,
        DOCKCONT_MAX    = 4,

        // mask params for plugins of internal dialogs
        DWS_ICONTAB            = 0x00000001,            // Icon for tabs are available
        DWS_ICONBAR            = 0x00000002,            // Icon for icon bar are available (currently not supported)
        DWS_ADDINFO            = 0x00000004,            // Additional information are in use
        DWS_PARAMSALL        = (DWS_ICONTAB|DWS_ICONBAR|DWS_ADDINFO),

        // default docking values for first call of plugin
        DWS_DF_CONT_LEFT    = (CONT_LEFT    << 28),    // default docking on left
        DWS_DF_CONT_RIGHT    = (CONT_RIGHT    << 28),    // default docking on right
        DWS_DF_CONT_TOP        = (CONT_TOP    << 28),        // default docking on top
        DWS_DF_CONT_BOTTOM    = (CONT_BOTTOM << 28),    // default docking on bottom
        DWS_DF_FLOATING        = 0x80000000            // default state is floating
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NppTbData
    {
        public IntPtr hClient;            // HWND: client Window Handle
        public string pszName;            // TCHAR*: name of plugin (shown in window)
        public int dlgID;                // int: a funcItem provides the function pointer to start a dialog. Please parse here these ID
        // user modifications
        public NppTbMsg uMask;                // UINT: mask params: look to above defines
        public uint hIconTab;            // HICON: icon for tabs
        public string pszAddInfo;        // TCHAR*: for plugin to display additional informations
        // internal data, do not use !!!
        public RECT rcFloat;            // RECT: floating position
        public int iPrevCont;           // int: stores the privious container (toggling between float and dock)
        public string pszModuleName;    // const TCHAR*: it's the plugin file name. It's used to identify the plugin
    }

    public enum LangType
    {
        L_TEXT, L_PHP , L_C, L_CPP, L_CS, L_OBJC, L_JAVA, L_RC,
        L_HTML, L_XML, L_MAKEFILE, L_PASCAL, L_BATCH, L_INI, L_ASCII, L_USER,
        L_ASP, L_SQL, L_VB, L_JS, L_CSS, L_PERL, L_PYTHON, L_LUA,
        L_TEX, L_FORTRAN, L_BASH, L_FLASH, L_NSIS, L_TCL, L_LISP, L_SCHEME,
        L_ASM, L_DIFF, L_PROPS, L_PS, L_RUBY, L_SMALLTALK, L_VHDL, L_KIX, L_AU3,
        L_CAML, L_ADA, L_VERILOG, L_MATLAB, L_HASKELL, L_INNO, L_SEARCHRESULT,
        L_CMAKE, L_YAML, L_COBOL, L_GUI4CLI, L_D, L_POWERSHELL, L_R, L_JSP,
        // The end of enumated language type, so it should be always at the end
        L_EXTERNAL
    }

    [Flags]
    public enum NppMsg : uint
    {
        //Here you can find how to use these messages : http://notepad-plus.sourceforge.net/uk/plugins-HOWTO.php 
        NPPMSG = (0x400/*WM_USER*/ + 1000),

        NPPM_GETCURRENTSCINTILLA = (NPPMSG + 4),
        NPPM_GETCURRENTLANGTYPE = (NPPMSG + 5),
        NPPM_SETCURRENTLANGTYPE = (NPPMSG + 6),

        NPPM_GETNBOPENFILES = (NPPMSG + 7),
            ALL_OPEN_FILES = 0,
            PRIMARY_VIEW = 1,
            SECOND_VIEW = 2,

        NPPM_GETOPENFILENAMES = (NPPMSG + 8),

        NPPM_MODELESSDIALOG = (NPPMSG + 12),
            MODELESSDIALOGADD = 0,
            MODELESSDIALOGREMOVE = 1,

        NPPM_GETNBSESSIONFILES = (NPPMSG + 13),
        NPPM_GETSESSIONFILES = (NPPMSG + 14),
        NPPM_SAVESESSION = (NPPMSG + 15),
        NPPM_SAVECURRENTSESSION = (NPPMSG + 16),
            //struct sessionInfo {
            //    TCHAR* sessionFilePathName;
            //    int nbFile;
            //    TCHAR** files;
            //};

        NPPM_GETOPENFILENAMESPRIMARY = (NPPMSG + 17),
        NPPM_GETOPENFILENAMESSECOND = (NPPMSG + 18),
        
        NPPM_CREATESCINTILLAHANDLE = (NPPMSG + 20),
        NPPM_DESTROYSCINTILLAHANDLE = (NPPMSG + 21),
        NPPM_GETNBUSERLANG = (NPPMSG + 22),

        NPPM_GETCURRENTDOCINDEX = (NPPMSG + 23),
            MAIN_VIEW = 0,
            SUB_VIEW = 1,

        NPPM_SETSTATUSBAR = (NPPMSG + 24),
            STATUSBAR_DOC_TYPE = 0,
            STATUSBAR_DOC_SIZE = 1,
            STATUSBAR_CUR_POS = 2,
            STATUSBAR_EOF_FORMAT = 3,
            STATUSBAR_UNICODE_TYPE = 4,
            STATUSBAR_TYPING_MODE = 5,

        NPPM_GETMENUHANDLE = (NPPMSG + 25),
            NPPPLUGINMENU = 0,

        NPPM_ENCODESCI = (NPPMSG + 26),
        //ascii file to unicode
        //int NPPM_ENCODESCI(MAIN_VIEW/SUB_VIEW, 0)
        //return new unicodeMode
        
        NPPM_DECODESCI = (NPPMSG + 27),
        //unicode file to ascii
        //int NPPM_DECODESCI(MAIN_VIEW/SUB_VIEW, 0)
        //return old unicodeMode

        NPPM_ACTIVATEDOC = (NPPMSG + 28),
        //void NPPM_ACTIVATEDOC(int view, int index2Activate)

        NPPM_LAUNCHFINDINFILESDLG = (NPPMSG + 29),
        //void NPPM_LAUNCHFINDINFILESDLG(TCHAR * dir2Search, TCHAR * filtre)

        NPPM_DMMSHOW = (NPPMSG + 30),
        NPPM_DMMHIDE = (NPPMSG + 31),
        NPPM_DMMUPDATEDISPINFO = (NPPMSG + 32),
        //void NPPM_DMMxxx(0, tTbData->hClient)

        NPPM_DMMREGASDCKDLG = (NPPMSG + 33),
        //void NPPM_DMMREGASDCKDLG(0, &tTbData)

        NPPM_LOADSESSION = (NPPMSG + 34),
        //void NPPM_LOADSESSION(0, const TCHAR* file name)

        NPPM_DMMVIEWOTHERTAB = (NPPMSG + 35),
        //void WM_DMM_VIEWOTHERTAB(0, tTbData->pszName)

        NPPM_RELOADFILE = (NPPMSG + 36),
        //BOOL NPPM_RELOADFILE(BOOL withAlert, TCHAR *filePathName2Reload)

        NPPM_SWITCHTOFILE = (NPPMSG + 37),
        //BOOL NPPM_SWITCHTOFILE(0, TCHAR *filePathName2switch)

        NPPM_SAVECURRENTFILE = (NPPMSG + 38),
        //BOOL NPPM_SAVECURRENTFILE(0, 0)

        NPPM_SAVEALLFILES = (NPPMSG + 39),
        //BOOL NPPM_SAVEALLFILES(0, 0)

        NPPM_SETMENUITEMCHECK = (NPPMSG + 40),
        //void WM_PIMENU_CHECK(UINT    funcItem[X]._cmdID, TRUE/FALSE)

        NPPM_ADDTOOLBARICON = (NPPMSG + 41),
        //void WM_ADDTOOLBARICON(UINT funcItem[X]._cmdID, toolbarIcons icon)
            //struct toolbarIcons {
            //    HBITMAP    hToolbarBmp;
            //    HICON    hToolbarIcon;
            //};

        NPPM_GETWINDOWSVERSION = (NPPMSG + 42),
        //winVer NPPM_GETWINDOWSVERSION(0, 0)

        NPPM_DMMGETPLUGINHWNDBYNAME = (NPPMSG + 43),
        //HWND WM_DMM_GETPLUGINHWNDBYNAME(const TCHAR *windowName, const TCHAR *moduleName)
        // if moduleName is NULL, then return value is NULL
        // if windowName is NULL, then the first found window handle which matches with the moduleName will be returned
        
        NPPM_MAKECURRENTBUFFERDIRTY = (NPPMSG + 44),
        //BOOL NPPM_MAKECURRENTBUFFERDIRTY(0, 0)

        NPPM_GETENABLETHEMETEXTUREFUNC = (NPPMSG + 45),
        //BOOL NPPM_GETENABLETHEMETEXTUREFUNC(0, 0)

        NPPM_GETPLUGINSCONFIGDIR = (NPPMSG + 46),
        //void NPPM_GETPLUGINSCONFIGDIR(int strLen, TCHAR *str)

        NPPM_MSGTOPLUGIN = (NPPMSG + 47),
        //BOOL NPPM_MSGTOPLUGIN(TCHAR *destModuleName, CommunicationInfo *info)
        // return value is TRUE when the message arrive to the destination plugins.
        // if destModule or info is NULL, then return value is FALSE
            //struct CommunicationInfo {
            //    long internalMsg;
            //    const TCHAR * srcModuleName;
            //    void * info; // defined by plugin
            //};

        NPPM_MENUCOMMAND = (NPPMSG + 48),
        //void NPPM_MENUCOMMAND(0, int cmdID)
        // uncomment //#include "menuCmdID.h"
        // in the beginning of this file then use the command symbols defined in "menuCmdID.h" file
        // to access all the Notepad++ menu command items
        
        NPPM_TRIGGERTABBARCONTEXTMENU = (NPPMSG + 49),
        //void NPPM_TRIGGERTABBARCONTEXTMENU(int view, int index2Activate)

        NPPM_GETNPPVERSION = (NPPMSG + 50),
        // int NPPM_GETNPPVERSION(0, 0)
        // return version 
        // ex : v4.6
        // HIWORD(version) == 4
        // LOWORD(version) == 6

        NPPM_HIDETABBAR = (NPPMSG + 51),
        // BOOL NPPM_HIDETABBAR(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then tab bar will be hidden
        // otherwise it'll be shown.
        // return value : the old status value

        NPPM_ISTABBARHIDDEN = (NPPMSG + 52),
        // BOOL NPPM_ISTABBARHIDDEN(0, 0)
        // returned value : TRUE if tab bar is hidden, otherwise FALSE

        NPPM_GETPOSFROMBUFFERID = (NPPMSG + 57),
        // INT NPPM_GETPOSFROMBUFFERID(INT bufferID, 0)
        // Return VIEW|INDEX from a buffer ID. -1 if the bufferID non existing
        //
        // VIEW takes 2 highest bits and INDEX (0 based) takes the rest (30 bits) 
        // Here's the values for the view :
        //  MAIN_VIEW 0
        //  SUB_VIEW  1

        NPPM_GETFULLPATHFROMBUFFERID = (NPPMSG + 58),
        // INT NPPM_GETFULLPATHFROMBUFFERID(INT bufferID, TCHAR *fullFilePath)
        // Get full path file name from a bufferID. 
        // Return -1 if the bufferID non existing, otherwise the number of TCHAR copied/to copy
        // User should call it with fullFilePath be NULL to get the number of TCHAR (not including the nul character),
        // allocate fullFilePath with the return values + 1, then call it again to get  full path file name

        NPPM_GETBUFFERIDFROMPOS = (NPPMSG + 59),
        //wParam: Position of document
        //lParam: View to use, 0 = Main, 1 = Secondary
        //Returns 0 if invalid

        NPPM_GETCURRENTBUFFERID = (NPPMSG + 60),
        //Returns active Buffer

        NPPM_RELOADBUFFERID = (NPPMSG + 61),
        //Reloads Buffer
        //wParam: Buffer to reload
        //lParam: 0 if no alert, else alert


        NPPM_GETBUFFERLANGTYPE = (NPPMSG + 64),
        //wParam: BufferID to get LangType from
        //lParam: 0
        //Returns as int, see LangType. -1 on error

        NPPM_SETBUFFERLANGTYPE = (NPPMSG + 65),
        //wParam: BufferID to set LangType of
        //lParam: LangType
        //Returns TRUE on success, FALSE otherwise
        //use int, see LangType for possible values
        //L_USER and L_EXTERNAL are not supported

        NPPM_GETBUFFERENCODING = (NPPMSG + 66),
        //wParam: BufferID to get encoding from
        //lParam: 0
        //returns as int, see UniMode. -1 on error

        NPPM_SETBUFFERENCODING = (NPPMSG + 67),
        //wParam: BufferID to set encoding of
        //lParam: format
        //Returns TRUE on success, FALSE otherwise
        //use int, see UniMode
        //Can only be done on new, unedited files

        NPPM_GETBUFFERFORMAT = (NPPMSG + 68),
        //wParam: BufferID to get format from
        //lParam: 0
        //returns as int, see formatType. -1 on error

        NPPM_SETBUFFERFORMAT = (NPPMSG + 69),
        //wParam: BufferID to set format of
        //lParam: format
        //Returns TRUE on success, FALSE otherwise
        //use int, see formatType

        /*
        NPPM_ADDREBAR = (NPPMSG + 57),
        // BOOL NPPM_ADDREBAR(0, REBARBANDINFO *)
        // Returns assigned ID in wID value of struct pointer
        NPPM_UPDATEREBAR = (NPPMSG + 58),
        // BOOL NPPM_ADDREBAR(INT ID, REBARBANDINFO *)
        //Use ID assigned with NPPM_ADDREBAR
        NPPM_REMOVEREBAR = (NPPMSG + 59),
        // BOOL NPPM_ADDREBAR(INT ID, 0)
        //Use ID assigned with NPPM_ADDREBAR
        */

        NPPM_HIDETOOLBAR = (NPPMSG + 70),
        // BOOL NPPM_HIDETOOLBAR(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then tool bar will be hidden
        // otherwise it'll be shown.
        // return value : the old status value

        NPPM_ISTOOLBARHIDDEN = (NPPMSG + 71),
        // BOOL NPPM_ISTOOLBARHIDDEN(0, 0)
        // returned value : TRUE if tool bar is hidden, otherwise FALSE

        NPPM_HIDEMENU = (NPPMSG + 72),
        // BOOL NPPM_HIDEMENU(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then menu will be hidden
        // otherwise it'll be shown.
        // return value : the old status value

        NPPM_ISMENUHIDDEN = (NPPMSG + 73),
        // BOOL NPPM_ISMENUHIDDEN(0, 0)
        // returned value : TRUE if menu is hidden, otherwise FALSE

        NPPM_HIDESTATUSBAR = (NPPMSG + 74),
        // BOOL NPPM_HIDESTATUSBAR(0, BOOL hideOrNot)
        // if hideOrNot is set as TRUE then STATUSBAR will be hidden
        // otherwise it'll be shown.
        // return value : the old status value

        NPPM_ISSTATUSBARHIDDEN = (NPPMSG + 75),
        // BOOL NPPM_ISSTATUSBARHIDDEN(0, 0)
        // returned value : TRUE if STATUSBAR is hidden, otherwise FALSE

        NPPM_GETSHORTCUTBYCMDID = (NPPMSG + 76),
        // BOOL NPPM_GETSHORTCUTBYCMDID(int cmdID, ShortcutKey *sk)
        // get your plugin command current mapped shortcut into sk via cmdID
        // You may need it after getting NPPN_READY notification
        // returned value : TRUE if this function call is successful and shorcut is enable, otherwise FALSE

        NPPM_DOOPEN = (NPPMSG + 77),
        // BOOL NPPM_DOOPEN(0, const TCHAR *fullPathName2Open)
        // fullPathName2Open indicates the full file path name to be opened.
        // The return value is TRUE (1) if the operation is successful, otherwise FALSE (0).

        NPPM_SAVECURRENTFILEAS = (NPPMSG + 78),
        // BOOL NPPM_SAVECURRENTFILEAS (BOOL asCopy, const TCHAR* filename)

        NPPM_GETCURRENTNATIVELANGENCODING = (NPPMSG + 79),
        // INT NPPM_GETCURRENTNATIVELANGENCODING(0, 0)
        // returned value : the current native language enconding

        NPPM_ALLOCATESUPPORTED = (NPPMSG + 80),
        // returns TRUE if NPPM_ALLOCATECMDID is supported
        // Use to identify if subclassing is necessary

        NPPM_ALLOCATECMDID = (NPPMSG + 81),
        // BOOL NPPM_ALLOCATECMDID(int numberRequested, int* startNumber)
        // sets startNumber to the initial command ID if successful
        // Returns: TRUE if successful, FALSE otherwise. startNumber will also be set to 0 if unsuccessful

        NPPM_ALLOCATEMARKER = (NPPMSG + 82),
        // BOOL NPPM_ALLOCATEMARKER(int numberRequested, int* startNumber)
        // sets startNumber to the initial command ID if successful
        // Allocates a marker number to a plugin
        // Returns: TRUE if successful, FALSE otherwise. startNumber will also be set to 0 if unsuccessful

        RUNCOMMAND_USER = (0x400/*WM_USER*/ + 3000),
        NPPM_GETFULLCURRENTPATH     = (RUNCOMMAND_USER + FULL_CURRENT_PATH),
        NPPM_GETCURRENTDIRECTORY    = (RUNCOMMAND_USER + CURRENT_DIRECTORY),
        NPPM_GETFILENAME            = (RUNCOMMAND_USER + FILE_NAME),
        NPPM_GETNAMEPART            = (RUNCOMMAND_USER + NAME_PART),
        NPPM_GETEXTPART                = (RUNCOMMAND_USER + EXT_PART),
        NPPM_GETCURRENTWORD            = (RUNCOMMAND_USER + CURRENT_WORD),
        NPPM_GETNPPDIRECTORY        = (RUNCOMMAND_USER + NPP_DIRECTORY),
        // BOOL NPPM_GETXXXXXXXXXXXXXXXX(size_t strLen, TCHAR *str)
        // where str is the allocated TCHAR array,
        //         strLen is the allocated array size
        // The return value is TRUE when get generic_string operation success
        // Otherwise (allocated array size is too small) FALSE

        NPPM_GETCURRENTLINE = (RUNCOMMAND_USER + CURRENT_LINE),
        // INT NPPM_GETCURRENTLINE(0, 0)
        // return the caret current position line
        NPPM_GETCURRENTCOLUMN = (RUNCOMMAND_USER + CURRENT_COLUMN),
        // INT NPPM_GETCURRENTCOLUMN(0, 0)
        // return the caret current position column
            VAR_NOT_RECOGNIZED = 0,
            FULL_CURRENT_PATH = 1,
            CURRENT_DIRECTORY = 2,
            FILE_NAME = 3,
            NAME_PART = 4,
            EXT_PART = 5,
            CURRENT_WORD = 6,
            NPP_DIRECTORY = 7,
            CURRENT_LINE = 8,
            CURRENT_COLUMN = 9,

        // Notification code
        NPPN_FIRST = 1000,
        NPPN_READY = (NPPN_FIRST + 1), // To notify plugins that all the procedures of launchment of notepad++ are done.
        //scnNotification->nmhdr.code = NPPN_READY;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = 0;

        NPPN_TBMODIFICATION = (NPPN_FIRST + 2), // To notify plugins that toolbar icons can be registered
        //scnNotification->nmhdr.code = NPPN_TB_MODIFICATION;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = 0;

        NPPN_FILEBEFORECLOSE = (NPPN_FIRST + 3), // To notify plugins that the current file is about to be closed
        //scnNotification->nmhdr.code = NPPN_FILEBEFORECLOSE;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;

        NPPN_FILEOPENED = (NPPN_FIRST + 4), // To notify plugins that the current file is just opened
        //scnNotification->nmhdr.code = NPPN_FILEOPENED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;

        NPPN_FILECLOSED = (NPPN_FIRST + 5), // To notify plugins that the current file is just closed
        //scnNotification->nmhdr.code = NPPN_FILECLOSED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;

        NPPN_FILEBEFOREOPEN = (NPPN_FIRST + 6), // To notify plugins that the current file is about to be opened
        //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        
        NPPN_FILEBEFORESAVE = (NPPN_FIRST + 7), // To notify plugins that the current file is about to be saved
        //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;
        
        NPPN_FILESAVED = (NPPN_FIRST + 8), // To notify plugins that the current file is just saved
        //scnNotification->nmhdr.code = NPPN_FILESAVED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;

        NPPN_SHUTDOWN = (NPPN_FIRST + 9), // To notify plugins that Notepad++ is about to be shutdowned.
        //scnNotification->nmhdr.code = NPPN_SHUTDOWN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = 0;

        NPPN_BUFFERACTIVATED = (NPPN_FIRST + 10), // To notify plugins that a buffer was activated (put to foreground).
        //scnNotification->nmhdr.code = NPPN_BUFFERACTIVATED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = activatedBufferID;

        NPPN_LANGCHANGED = (NPPN_FIRST + 11), // To notify plugins that the language in the current doc is just changed.
        //scnNotification->nmhdr.code = NPPN_LANGCHANGED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = currentBufferID;

        NPPN_WORDSTYLESUPDATED = (NPPN_FIRST + 12), // To notify plugins that user initiated a WordStyleDlg change.
        //scnNotification->nmhdr.code = NPPN_WORDSTYLESUPDATED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = currentBufferID;

        NPPN_SHORTCUTREMAPPED = (NPPN_FIRST + 13), // To notify plugins that plugin command shortcut is remapped.
        //scnNotification->nmhdr.code = NPPN_SHORTCUTSREMAPPED;
        //scnNotification->nmhdr.hwndFrom = ShortcutKeyStructurePointer;
        //scnNotification->nmhdr.idFrom = cmdID;
            //where ShortcutKeyStructurePointer is pointer of struct ShortcutKey:
            //struct ShortcutKey {
            //    bool _isCtrl;
            //    bool _isAlt;
            //    bool _isShift;
            //    UCHAR _key;
            //};

        NPPN_FILEBEFORELOAD = (NPPN_FIRST + 14), // To notify plugins that the current file is about to be loaded
        //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = NULL;

        NPPN_FILELOADFAILED = (NPPN_FIRST + 15),  // To notify plugins that file open operation failed
        //scnNotification->nmhdr.code = NPPN_FILEOPENFAILED;
        //scnNotification->nmhdr.hwndFrom = hwndNpp;
        //scnNotification->nmhdr.idFrom = BufferID;

        NPPN_READONLYCHANGED = (NPPN_FIRST + 16),  // To notify plugins that current document change the readonly status,
        //scnNotification->nmhdr.code = NPPN_READONLYCHANGED;
        //scnNotification->nmhdr.hwndFrom = bufferID;
        //scnNotification->nmhdr.idFrom = docStatus;
            // where bufferID is BufferID
            //       docStatus can be combined by DOCSTAUS_READONLY and DOCSTAUS_BUFFERDIRTY

            DOCSTAUS_READONLY = 1,
            DOCSTAUS_BUFFERDIRTY = 2,

        NPPN_DOCORDERCHANGED = (NPPN_FIRST + 16)  // To notify plugins that document order is changed
        //scnNotification->nmhdr.code = NPPN_DOCORDERCHANGED;
        //scnNotification->nmhdr.hwndFrom = newIndex;
        //scnNotification->nmhdr.idFrom = BufferID;
    }

    public enum NppMenuCmd : uint
    {
        IDM    = 40000,

        IDM_FILE    = (IDM + 1000),
            IDM_FILE_NEW                     = (IDM_FILE + 1),
            IDM_FILE_OPEN                    = (IDM_FILE + 2),
            IDM_FILE_CLOSE                   = (IDM_FILE + 3),
            IDM_FILE_CLOSEALL                = (IDM_FILE + 4),
            IDM_FILE_CLOSEALL_BUT_CURRENT    = (IDM_FILE + 5),
            IDM_FILE_SAVE                    = (IDM_FILE + 6),
            IDM_FILE_SAVEALL                 = (IDM_FILE + 7),
            IDM_FILE_SAVEAS                  = (IDM_FILE + 8),
            //IDM_FILE_ASIAN_LANG              = (IDM_FILE + 9), 
            IDM_FILE_PRINT                   = (IDM_FILE + 10),
            IDM_FILE_PRINTNOW                = 1001,
            IDM_FILE_EXIT                    = (IDM_FILE + 11),
            IDM_FILE_LOADSESSION             = (IDM_FILE + 12),
            IDM_FILE_SAVESESSION             = (IDM_FILE + 13),
            IDM_FILE_RELOAD                  = (IDM_FILE + 14),
            IDM_FILE_SAVECOPYAS              = (IDM_FILE + 15),
            IDM_FILE_DELETE                  = (IDM_FILE + 16),
            IDM_FILE_RENAME                  = (IDM_FILE + 17),
 
         // A mettre à jour si on ajoute nouveau menu item dans le menu "File"
            IDM_FILEMENU_LASTONE             = IDM_FILE_RENAME,
         
        IDM_EDIT       = (IDM + 2000),
            IDM_EDIT_CUT                         = (IDM_EDIT + 1),
            IDM_EDIT_COPY                        = (IDM_EDIT + 2),
            IDM_EDIT_UNDO                        = (IDM_EDIT + 3),
            IDM_EDIT_REDO                        = (IDM_EDIT + 4),
            IDM_EDIT_PASTE                       = (IDM_EDIT + 5),
            IDM_EDIT_DELETE                      = (IDM_EDIT + 6),
            IDM_EDIT_SELECTALL                   = (IDM_EDIT + 7),
            
            IDM_EDIT_INS_TAB                     = (IDM_EDIT + 8),
            IDM_EDIT_RMV_TAB                     = (IDM_EDIT + 9),
            IDM_EDIT_DUP_LINE                    = (IDM_EDIT + 10),
            IDM_EDIT_TRANSPOSE_LINE              = (IDM_EDIT + 11),
            IDM_EDIT_SPLIT_LINES                 = (IDM_EDIT + 12),
            IDM_EDIT_JOIN_LINES                  = (IDM_EDIT + 13),
            IDM_EDIT_LINE_UP                     = (IDM_EDIT + 14),
            IDM_EDIT_LINE_DOWN                   = (IDM_EDIT + 15),
            IDM_EDIT_UPPERCASE                   = (IDM_EDIT + 16),
            IDM_EDIT_LOWERCASE                   = (IDM_EDIT + 17),

        // Menu macro
            IDM_MACRO_STARTRECORDINGMACRO        = (IDM_EDIT + 18),
            IDM_MACRO_STOPRECORDINGMACRO         = (IDM_EDIT + 19),
            IDM_MACRO_PLAYBACKRECORDEDMACRO      = (IDM_EDIT + 21),
        //-----------

            IDM_EDIT_BLOCK_COMMENT               = (IDM_EDIT + 22),
            IDM_EDIT_STREAM_COMMENT              = (IDM_EDIT + 23),
            IDM_EDIT_TRIMTRAILING                = (IDM_EDIT + 24),
            IDM_EDIT_TRIMLINEHEAD                = (IDM_EDIT + 42),
            IDM_EDIT_TRIM_BOTH                   = (IDM_EDIT + 43),
            IDM_EDIT_EOL2WS                      = (IDM_EDIT + 44),
            IDM_EDIT_TRIMALL                     = (IDM_EDIT + 45),
            IDM_EDIT_TAB2SW                      = (IDM_EDIT + 46),
            IDM_EDIT_SW2TAB                      = (IDM_EDIT + 47),
            
        // Menu macro
            IDM_MACRO_SAVECURRENTMACRO           = (IDM_EDIT + 25),
        //-----------

            IDM_EDIT_RTL                         = (IDM_EDIT + 26),
            IDM_EDIT_LTR                         = (IDM_EDIT + 27),
            IDM_EDIT_SETREADONLY                 = (IDM_EDIT + 28),
            IDM_EDIT_FULLPATHTOCLIP              = (IDM_EDIT + 29),
            IDM_EDIT_FILENAMETOCLIP              = (IDM_EDIT + 30),
            IDM_EDIT_CURRENTDIRTOCLIP            = (IDM_EDIT + 31),

        // Menu macro
            IDM_MACRO_RUNMULTIMACRODLG           = (IDM_EDIT + 32),
        //-----------

            IDM_EDIT_CLEARREADONLY               = (IDM_EDIT + 33),
            IDM_EDIT_COLUMNMODE                  = (IDM_EDIT + 34),
            IDM_EDIT_BLOCK_COMMENT_SET           = (IDM_EDIT + 35),
            IDM_EDIT_BLOCK_UNCOMMENT             = (IDM_EDIT + 36),

            IDM_EDIT_AUTOCOMPLETE                = (50000 + 0),
            IDM_EDIT_AUTOCOMPLETE_CURRENTFILE    = (50000 + 1),
            IDM_EDIT_FUNCCALLTIP                 = (50000 + 2),
    
            //Belong to MENU FILE
            IDM_OPEN_ALL_RECENT_FILE             = (IDM_EDIT + 40),
            IDM_CLEAN_RECENT_FILE_LIST           = (IDM_EDIT + 41),
            
        IDM_SEARCH    = (IDM + 3000),

            IDM_SEARCH_FIND                 = (IDM_SEARCH + 1),
            IDM_SEARCH_FINDNEXT             = (IDM_SEARCH + 2),
            IDM_SEARCH_REPLACE              = (IDM_SEARCH + 3),
            IDM_SEARCH_GOTOLINE             = (IDM_SEARCH + 4),
            IDM_SEARCH_TOGGLE_BOOKMARK      = (IDM_SEARCH + 5),
            IDM_SEARCH_NEXT_BOOKMARK        = (IDM_SEARCH + 6),
            IDM_SEARCH_PREV_BOOKMARK        = (IDM_SEARCH + 7),
            IDM_SEARCH_CLEAR_BOOKMARKS      = (IDM_SEARCH + 8),
            IDM_SEARCH_GOTOMATCHINGBRACE    = (IDM_SEARCH + 9),
            IDM_SEARCH_FINDPREV             = (IDM_SEARCH + 10),
            IDM_SEARCH_FINDINCREMENT        = (IDM_SEARCH + 11),
            IDM_SEARCH_FINDINFILES          = (IDM_SEARCH + 13),
            IDM_SEARCH_VOLATILE_FINDNEXT    = (IDM_SEARCH + 14),
            IDM_SEARCH_VOLATILE_FINDPREV    = (IDM_SEARCH + 15),
            IDM_SEARCH_CUTMARKEDLINES       = (IDM_SEARCH + 18),
            IDM_SEARCH_COPYMARKEDLINES      = (IDM_SEARCH + 19),
            IDM_SEARCH_PASTEMARKEDLINES     = (IDM_SEARCH + 20),
            IDM_SEARCH_DELETEMARKEDLINES    = (IDM_SEARCH + 21),
            IDM_SEARCH_MARKALLEXT1          = (IDM_SEARCH + 22),
            IDM_SEARCH_UNMARKALLEXT1        = (IDM_SEARCH + 23),
            IDM_SEARCH_MARKALLEXT2          = (IDM_SEARCH + 24),
            IDM_SEARCH_UNMARKALLEXT2        = (IDM_SEARCH + 25),
            IDM_SEARCH_MARKALLEXT3          = (IDM_SEARCH + 26),
            IDM_SEARCH_UNMARKALLEXT3        = (IDM_SEARCH + 27),
            IDM_SEARCH_MARKALLEXT4          = (IDM_SEARCH + 28),
            IDM_SEARCH_UNMARKALLEXT4        = (IDM_SEARCH + 29),
            IDM_SEARCH_MARKALLEXT5          = (IDM_SEARCH + 30),
            IDM_SEARCH_UNMARKALLEXT5        = (IDM_SEARCH + 31),
            IDM_SEARCH_CLEARALLMARKS        = (IDM_SEARCH + 32),

            IDM_SEARCH_GOPREVMARKER1        = (IDM_SEARCH + 33),
            IDM_SEARCH_GOPREVMARKER2        = (IDM_SEARCH + 34),
            IDM_SEARCH_GOPREVMARKER3        = (IDM_SEARCH + 35),
            IDM_SEARCH_GOPREVMARKER4        = (IDM_SEARCH + 36),
            IDM_SEARCH_GOPREVMARKER5        = (IDM_SEARCH + 37),
            IDM_SEARCH_GOPREVMARKER_DEF     = (IDM_SEARCH + 38),

            IDM_SEARCH_GONEXTMARKER1        = (IDM_SEARCH + 39),
            IDM_SEARCH_GONEXTMARKER2        = (IDM_SEARCH + 40),
            IDM_SEARCH_GONEXTMARKER3        = (IDM_SEARCH + 41),
            IDM_SEARCH_GONEXTMARKER4        = (IDM_SEARCH + 42),
            IDM_SEARCH_GONEXTMARKER5        = (IDM_SEARCH + 43),
            IDM_SEARCH_GONEXTMARKER_DEF     = (IDM_SEARCH + 44),

            IDM_FOCUS_ON_FOUND_RESULTS      = (IDM_SEARCH + 45),
            IDM_SEARCH_GOTONEXTFOUND        = (IDM_SEARCH + 46),
            IDM_SEARCH_GOTOPREVFOUND        = (IDM_SEARCH + 47),
            
            IDM_SEARCH_SETANDFINDNEXT       = (IDM_SEARCH + 48),
            IDM_SEARCH_SETANDFINDPREV       = (IDM_SEARCH + 49),
            IDM_SEARCH_INVERSEMARKS         = (IDM_SEARCH + 50),
            
        IDM_VIEW    = (IDM + 4000),
            //IDM_VIEW_TOOLBAR_HIDE            = (IDM_VIEW + 1),
            IDM_VIEW_TOOLBAR_REDUCE            = (IDM_VIEW + 2),   
            IDM_VIEW_TOOLBAR_ENLARGE           = (IDM_VIEW + 3),
            IDM_VIEW_TOOLBAR_STANDARD          = (IDM_VIEW + 4),
            IDM_VIEW_REDUCETABBAR              = (IDM_VIEW + 5),
            IDM_VIEW_LOCKTABBAR                = (IDM_VIEW + 6),
            IDM_VIEW_DRAWTABBAR_TOPBAR         = (IDM_VIEW + 7),
            IDM_VIEW_DRAWTABBAR_INACIVETAB     = (IDM_VIEW + 8),
            IDM_VIEW_POSTIT                    = (IDM_VIEW + 9),
            IDM_VIEW_TOGGLE_FOLDALL            = (IDM_VIEW + 10),
            IDM_VIEW_USER_DLG                  = (IDM_VIEW + 11),
            IDM_VIEW_LINENUMBER                = (IDM_VIEW + 12),
            IDM_VIEW_SYMBOLMARGIN              = (IDM_VIEW + 13),
            IDM_VIEW_FOLDERMAGIN               = (IDM_VIEW + 14),
            IDM_VIEW_FOLDERMAGIN_SIMPLE        = (IDM_VIEW + 15),
            IDM_VIEW_FOLDERMAGIN_ARROW         = (IDM_VIEW + 16),
            IDM_VIEW_FOLDERMAGIN_CIRCLE        = (IDM_VIEW + 17),
            IDM_VIEW_FOLDERMAGIN_BOX           = (IDM_VIEW + 18),
            IDM_VIEW_ALL_CHARACTERS            = (IDM_VIEW + 19),
            IDM_VIEW_INDENT_GUIDE              = (IDM_VIEW + 20),
            IDM_VIEW_CURLINE_HILITING          = (IDM_VIEW + 21),
            IDM_VIEW_WRAP                      = (IDM_VIEW + 22),
            IDM_VIEW_ZOOMIN                    = (IDM_VIEW + 23),
            IDM_VIEW_ZOOMOUT                   = (IDM_VIEW + 24),
            IDM_VIEW_TAB_SPACE                 = (IDM_VIEW + 25),
            IDM_VIEW_EOL                       = (IDM_VIEW + 26),
            IDM_VIEW_EDGELINE                  = (IDM_VIEW + 27),
            IDM_VIEW_EDGEBACKGROUND            = (IDM_VIEW + 28),
            IDM_VIEW_TOGGLE_UNFOLDALL          = (IDM_VIEW + 29),
            IDM_VIEW_FOLD_CURRENT              = (IDM_VIEW + 30),
            IDM_VIEW_UNFOLD_CURRENT            = (IDM_VIEW + 31),
            IDM_VIEW_FULLSCREENTOGGLE          = (IDM_VIEW + 32),
            IDM_VIEW_ZOOMRESTORE               = (IDM_VIEW + 33),
            IDM_VIEW_ALWAYSONTOP               = (IDM_VIEW + 34),
            IDM_VIEW_SYNSCROLLV                = (IDM_VIEW + 35),
            IDM_VIEW_SYNSCROLLH                = (IDM_VIEW + 36),
            IDM_VIEW_EDGENONE                  = (IDM_VIEW + 37),
            IDM_VIEW_DRAWTABBAR_CLOSEBOTTUN    = (IDM_VIEW + 38),
            IDM_VIEW_DRAWTABBAR_DBCLK2CLOSE    = (IDM_VIEW + 39),
            IDM_VIEW_REFRESHTABAR              = (IDM_VIEW + 40),
            IDM_VIEW_WRAP_SYMBOL               = (IDM_VIEW + 41),
            IDM_VIEW_HIDELINES                 = (IDM_VIEW + 42),
            IDM_VIEW_DRAWTABBAR_VERTICAL       = (IDM_VIEW + 43),
            IDM_VIEW_DRAWTABBAR_MULTILINE      = (IDM_VIEW + 44),
            IDM_VIEW_DOCCHANGEMARGIN           = (IDM_VIEW + 45),
            IDM_VIEW_LWDEF                     = (IDM_VIEW + 46),
            IDM_VIEW_LWALIGN                   = (IDM_VIEW + 47),
            IDM_VIEW_LWINDENT                  = (IDM_VIEW + 48),
            IDM_VIEW_SUMMARY                   = (IDM_VIEW + 49),
            
            IDM_VIEW_FOLD                      = (IDM_VIEW + 50),
                IDM_VIEW_FOLD_1    = (IDM_VIEW_FOLD + 1),
                IDM_VIEW_FOLD_2    = (IDM_VIEW_FOLD + 2),
                IDM_VIEW_FOLD_3    = (IDM_VIEW_FOLD + 3),
                IDM_VIEW_FOLD_4    = (IDM_VIEW_FOLD + 4),
                IDM_VIEW_FOLD_5    = (IDM_VIEW_FOLD + 5),
                IDM_VIEW_FOLD_6    = (IDM_VIEW_FOLD + 6),
                IDM_VIEW_FOLD_7    = (IDM_VIEW_FOLD + 7),
                IDM_VIEW_FOLD_8    = (IDM_VIEW_FOLD + 8),

            IDM_VIEW_UNFOLD                    = (IDM_VIEW + 60), 
                IDM_VIEW_UNFOLD_1    = (IDM_VIEW_UNFOLD + 1),
                IDM_VIEW_UNFOLD_2    = (IDM_VIEW_UNFOLD + 2),
                IDM_VIEW_UNFOLD_3    = (IDM_VIEW_UNFOLD + 3),
                IDM_VIEW_UNFOLD_4    = (IDM_VIEW_UNFOLD + 4),
                IDM_VIEW_UNFOLD_5    = (IDM_VIEW_UNFOLD + 5),
                IDM_VIEW_UNFOLD_6    = (IDM_VIEW_UNFOLD + 6),
                IDM_VIEW_UNFOLD_7    = (IDM_VIEW_UNFOLD + 7),
                IDM_VIEW_UNFOLD_8    = (IDM_VIEW_UNFOLD + 8),
        
            IDM_VIEW_GOTO_ANOTHER_VIEW        = 10001,
            IDM_VIEW_CLONE_TO_ANOTHER_VIEW    = 10002,
            IDM_VIEW_GOTO_NEW_INSTANCE        = 10003,
            IDM_VIEW_LOAD_IN_NEW_INSTANCE     = 10004,

            IDM_VIEW_SWITCHTO_OTHER_VIEW       = (IDM_VIEW + 72),
            
        IDM_FORMAT    = (IDM + 5000),
            IDM_FORMAT_TODOS             = (IDM_FORMAT + 1),
            IDM_FORMAT_TOUNIX            = (IDM_FORMAT + 2),
            IDM_FORMAT_TOMAC             = (IDM_FORMAT + 3),
            IDM_FORMAT_ANSI              = (IDM_FORMAT + 4),
            IDM_FORMAT_UTF_8             = (IDM_FORMAT + 5),
            IDM_FORMAT_UCS_2BE           = (IDM_FORMAT + 6),
            IDM_FORMAT_UCS_2LE           = (IDM_FORMAT + 7),
            IDM_FORMAT_AS_UTF_8          = (IDM_FORMAT + 8),
            IDM_FORMAT_CONV2_ANSI        = (IDM_FORMAT + 9),
            IDM_FORMAT_CONV2_AS_UTF_8    = (IDM_FORMAT + 10),
            IDM_FORMAT_CONV2_UTF_8       = (IDM_FORMAT + 11),
            IDM_FORMAT_CONV2_UCS_2BE     = (IDM_FORMAT + 12),
            IDM_FORMAT_CONV2_UCS_2LE     = (IDM_FORMAT + 13),

            IDM_FORMAT_ENCODE            = (IDM_FORMAT + 20),
            IDM_FORMAT_WIN_1250          = (IDM_FORMAT_ENCODE + 0),
            IDM_FORMAT_WIN_1251          = (IDM_FORMAT_ENCODE + 1),
            IDM_FORMAT_WIN_1252          = (IDM_FORMAT_ENCODE + 2),
            IDM_FORMAT_WIN_1253          = (IDM_FORMAT_ENCODE + 3),
            IDM_FORMAT_WIN_1254          = (IDM_FORMAT_ENCODE + 4),
            IDM_FORMAT_WIN_1255          = (IDM_FORMAT_ENCODE + 5),
            IDM_FORMAT_WIN_1256          = (IDM_FORMAT_ENCODE + 6),
            IDM_FORMAT_WIN_1257          = (IDM_FORMAT_ENCODE + 7),
            IDM_FORMAT_WIN_1258          = (IDM_FORMAT_ENCODE + 8),
            IDM_FORMAT_ISO_8859_1        = (IDM_FORMAT_ENCODE + 9),
            IDM_FORMAT_ISO_8859_2        = (IDM_FORMAT_ENCODE + 10),
            IDM_FORMAT_ISO_8859_3        = (IDM_FORMAT_ENCODE + 11),
            IDM_FORMAT_ISO_8859_4        = (IDM_FORMAT_ENCODE + 12),
            IDM_FORMAT_ISO_8859_5        = (IDM_FORMAT_ENCODE + 13),
            IDM_FORMAT_ISO_8859_6        = (IDM_FORMAT_ENCODE + 14),
            IDM_FORMAT_ISO_8859_7        = (IDM_FORMAT_ENCODE + 15),
            IDM_FORMAT_ISO_8859_8        = (IDM_FORMAT_ENCODE + 16),
            IDM_FORMAT_ISO_8859_9        = (IDM_FORMAT_ENCODE + 17),
            IDM_FORMAT_ISO_8859_10       = (IDM_FORMAT_ENCODE + 18),
            IDM_FORMAT_ISO_8859_11       = (IDM_FORMAT_ENCODE + 19),
            IDM_FORMAT_ISO_8859_13       = (IDM_FORMAT_ENCODE + 20),
            IDM_FORMAT_ISO_8859_14       = (IDM_FORMAT_ENCODE + 21),
            IDM_FORMAT_ISO_8859_15       = (IDM_FORMAT_ENCODE + 22),
            IDM_FORMAT_ISO_8859_16       = (IDM_FORMAT_ENCODE + 23),
            IDM_FORMAT_DOS_437           = (IDM_FORMAT_ENCODE + 24),
            IDM_FORMAT_DOS_720           = (IDM_FORMAT_ENCODE + 25),
            IDM_FORMAT_DOS_737           = (IDM_FORMAT_ENCODE + 26),
            IDM_FORMAT_DOS_775           = (IDM_FORMAT_ENCODE + 27),
            IDM_FORMAT_DOS_850           = (IDM_FORMAT_ENCODE + 28),
            IDM_FORMAT_DOS_852           = (IDM_FORMAT_ENCODE + 29),
            IDM_FORMAT_DOS_855           = (IDM_FORMAT_ENCODE + 30),
            IDM_FORMAT_DOS_857           = (IDM_FORMAT_ENCODE + 31),
            IDM_FORMAT_DOS_858           = (IDM_FORMAT_ENCODE + 32),
            IDM_FORMAT_DOS_860           = (IDM_FORMAT_ENCODE + 33),
            IDM_FORMAT_DOS_861           = (IDM_FORMAT_ENCODE + 34),
            IDM_FORMAT_DOS_862           = (IDM_FORMAT_ENCODE + 35),
            IDM_FORMAT_DOS_863           = (IDM_FORMAT_ENCODE + 36),
            IDM_FORMAT_DOS_865           = (IDM_FORMAT_ENCODE + 37),
            IDM_FORMAT_DOS_866           = (IDM_FORMAT_ENCODE + 38),
            IDM_FORMAT_DOS_869           = (IDM_FORMAT_ENCODE + 39),
            IDM_FORMAT_BIG5              = (IDM_FORMAT_ENCODE + 40),
            IDM_FORMAT_GB2312            = (IDM_FORMAT_ENCODE + 41),
            IDM_FORMAT_SHIFT_JIS         = (IDM_FORMAT_ENCODE + 42),
            IDM_FORMAT_KOREAN_WIN        = (IDM_FORMAT_ENCODE + 43),
            IDM_FORMAT_EUC_KR            = (IDM_FORMAT_ENCODE + 44),
            IDM_FORMAT_TIS_620           = (IDM_FORMAT_ENCODE + 45),
            IDM_FORMAT_MAC_CYRILLIC      = (IDM_FORMAT_ENCODE + 46),
            IDM_FORMAT_KOI8U_CYRILLIC    = (IDM_FORMAT_ENCODE + 47),
            IDM_FORMAT_KOI8R_CYRILLIC    = (IDM_FORMAT_ENCODE + 48),
            IDM_FORMAT_ENCODE_END        = IDM_FORMAT_KOI8R_CYRILLIC,
            
            //#define    IDM_FORMAT_CONVERT            200

        IDM_LANG    = (IDM + 6000),
            IDM_LANGSTYLE_CONFIG_DLG    = (IDM_LANG + 1),
            IDM_LANG_C                  = (IDM_LANG + 2),
            IDM_LANG_CPP                = (IDM_LANG + 3),
            IDM_LANG_JAVA               = (IDM_LANG + 4),
            IDM_LANG_HTML               = (IDM_LANG + 5),       
            IDM_LANG_XML                = (IDM_LANG + 6),
            IDM_LANG_JS                 = (IDM_LANG + 7),
            IDM_LANG_PHP                = (IDM_LANG + 8),
            IDM_LANG_ASP                = (IDM_LANG + 9),
            IDM_LANG_CSS                = (IDM_LANG + 10),
            IDM_LANG_PASCAL             = (IDM_LANG + 11),
            IDM_LANG_PYTHON             = (IDM_LANG + 12),
            IDM_LANG_PERL               = (IDM_LANG + 13),
            IDM_LANG_OBJC               = (IDM_LANG + 14),
            IDM_LANG_ASCII              = (IDM_LANG + 15),
            IDM_LANG_TEXT               = (IDM_LANG + 16),
            IDM_LANG_RC                 = (IDM_LANG + 17),
            IDM_LANG_MAKEFILE           = (IDM_LANG + 18),
            IDM_LANG_INI                = (IDM_LANG + 19),
            IDM_LANG_SQL                = (IDM_LANG + 20),
            IDM_LANG_VB                 = (IDM_LANG + 21),
            IDM_LANG_BATCH              = (IDM_LANG + 22),
            IDM_LANG_CS                 = (IDM_LANG + 23),
            IDM_LANG_LUA                = (IDM_LANG + 24),
            IDM_LANG_TEX                = (IDM_LANG + 25),
            IDM_LANG_FORTRAN            = (IDM_LANG + 26),
            IDM_LANG_BASH               = (IDM_LANG + 27),
            IDM_LANG_FLASH              = (IDM_LANG + 28),
            IDM_LANG_NSIS               = (IDM_LANG + 29),
            IDM_LANG_TCL                = (IDM_LANG + 30),
            IDM_LANG_LISP               = (IDM_LANG + 31),
            IDM_LANG_SCHEME             = (IDM_LANG + 32),
            IDM_LANG_ASM                = (IDM_LANG + 33),
            IDM_LANG_DIFF               = (IDM_LANG + 34),
            IDM_LANG_PROPS              = (IDM_LANG + 35),
            IDM_LANG_PS                 = (IDM_LANG + 36),
            IDM_LANG_RUBY               = (IDM_LANG + 37),
            IDM_LANG_SMALLTALK          = (IDM_LANG + 38),
            IDM_LANG_VHDL               = (IDM_LANG + 39),
            IDM_LANG_CAML               = (IDM_LANG + 40),
            IDM_LANG_KIX                = (IDM_LANG + 41),
            IDM_LANG_ADA                = (IDM_LANG + 42),
            IDM_LANG_VERILOG            = (IDM_LANG + 43),
            IDM_LANG_AU3                = (IDM_LANG + 44),
            IDM_LANG_MATLAB             = (IDM_LANG + 45),
            IDM_LANG_HASKELL            = (IDM_LANG + 46),
            IDM_LANG_INNO               = (IDM_LANG + 47),
            IDM_LANG_CMAKE              = (IDM_LANG + 48),
            IDM_LANG_YAML               = (IDM_LANG + 49),
            IDM_LANG_COBOL              = (IDM_LANG + 50),
            IDM_LANG_D                  = (IDM_LANG + 51),
            IDM_LANG_GUI4CLI            = (IDM_LANG + 52),
            IDM_LANG_POWERSHELL         = (IDM_LANG + 53),
            IDM_LANG_R                  = (IDM_LANG + 54),
            IDM_LANG_JSP                = (IDM_LANG + 55),

            IDM_LANG_EXTERNAL           = (IDM_LANG + 65),
            IDM_LANG_EXTERNAL_LIMIT     = (IDM_LANG + 79),

            IDM_LANG_USER               = (IDM_LANG + 80),     //46080
            IDM_LANG_USER_LIMIT         = (IDM_LANG + 110),    //46110
            
            
        IDM_ABOUT    = (IDM  + 7000),
            IDM_HOMESWEETHOME    = (IDM_ABOUT  + 1),
            IDM_PROJECTPAGE      = (IDM_ABOUT  + 2),
            IDM_ONLINEHELP       = (IDM_ABOUT  + 3),
            IDM_FORUM            = (IDM_ABOUT  + 4),
            IDM_PLUGINSHOME      = (IDM_ABOUT  + 5),
            IDM_UPDATE_NPP       = (IDM_ABOUT  + 6),
            IDM_WIKIFAQ          = (IDM_ABOUT  + 7),
            IDM_HELP             = (IDM_ABOUT  + 8),


        IDM_SETTING    = (IDM + 8000),
            IDM_SETTING_TAB_SIZE                 = (IDM_SETTING + 1),
            IDM_SETTING_TAB_REPLCESPACE          = (IDM_SETTING + 2),
            IDM_SETTING_HISTORY_SIZE             = (IDM_SETTING + 3),
            IDM_SETTING_EDGE_SIZE                = (IDM_SETTING + 4),
            IDM_SETTING_IMPORTPLUGIN             = (IDM_SETTING + 5),
            IDM_SETTING_IMPORTSTYLETHEMS         = (IDM_SETTING + 6),
            IDM_SETTING_TRAYICON                 = (IDM_SETTING + 8),
            IDM_SETTING_SHORTCUT_MAPPER          = (IDM_SETTING + 9),
            IDM_SETTING_REMEMBER_LAST_SESSION    = (IDM_SETTING + 10),
            IDM_SETTING_PREFERECE                = (IDM_SETTING + 11),
            IDM_SETTING_AUTOCNBCHAR              = (IDM_SETTING + 15),
            IDM_SETTING_SHORTCUT_MAPPER_MACRO    = (IDM_SETTING + 16),
            IDM_SETTING_SHORTCUT_MAPPER_RUN      = (IDM_SETTING + 17),
            IDM_SETTING_EDITCONTEXTMENU          = (IDM_SETTING + 18),

        IDM_EXECUTE  = (IDM + 9000),

        IDM_SYSTRAYPOPUP     = (IDM + 3100),
            IDM_SYSTRAYPOPUP_ACTIVATE         = (IDM_SYSTRAYPOPUP + 1),
            IDM_SYSTRAYPOPUP_NEWDOC           = (IDM_SYSTRAYPOPUP + 2),
            IDM_SYSTRAYPOPUP_NEW_AND_PASTE    = (IDM_SYSTRAYPOPUP + 3),
            IDM_SYSTRAYPOPUP_OPENFILE         = (IDM_SYSTRAYPOPUP + 4),
            IDM_SYSTRAYPOPUP_CLOSE            = (IDM_SYSTRAYPOPUP + 5)
    }

    [Flags]
    public enum DockMgrMsg : uint
    {
        IDB_CLOSE_DOWN = 137,
        IDB_CLOSE_UP                    = 138,
        IDD_CONTAINER_DLG               = 139,

        IDC_TAB_CONT                    = 1027,
        IDC_CLIENT_TAB                  = 1028,
        IDC_BTN_CAPTION                 = 1050,

        DMM_MSG                         = 0x5000,
            DMM_CLOSE                   = (DMM_MSG + 1),
            DMM_DOCK                    = (DMM_MSG + 2),
            DMM_FLOAT                   = (DMM_MSG + 3),
            DMM_DOCKALL                 = (DMM_MSG + 4),
            DMM_FLOATALL                = (DMM_MSG + 5),
            DMM_MOVE                    = (DMM_MSG + 6),
            DMM_UPDATEDISPINFO          = (DMM_MSG + 7),
            DMM_GETIMAGELIST            = (DMM_MSG + 8),
            DMM_GETICONPOS              = (DMM_MSG + 9),
            DMM_DROPDATA                = (DMM_MSG + 10),
            DMM_MOVE_SPLITTER            = (DMM_MSG + 11),
            DMM_CANCEL_MOVE                = (DMM_MSG + 12),
            DMM_LBUTTONUP                = (DMM_MSG + 13),

        DMN_FIRST = 1050,
            DMN_CLOSE                    = (DMN_FIRST + 1),
            //nmhdr.code = DWORD(DMN_CLOSE, 0));
            //nmhdr.hwndFrom = hwndNpp;
            //nmhdr.idFrom = ctrlIdNpp;

            DMN_DOCK                    = (DMN_FIRST + 2),
            DMN_FLOAT                    = (DMN_FIRST + 3)
            //nmhdr.code = DWORD(DMN_XXX, int newContainer);
            //nmhdr.hwndFrom = hwndNpp;
            //nmhdr.idFrom = ctrlIdNpp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct toolbarIcons
    {
        public IntPtr hToolbarBmp;
        public IntPtr hToolbarIcon;
    }
    #endregion

    #region " Scintilla "
    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_NotifyHeader
    {
        /* Compatible with Windows NMHDR.
         * hwndFrom is really an environment specific window handle or pointer
         * but most clients of Scintilla.h do not have this type visible. */
        public IntPtr hwndFrom;
        public uint idFrom;
        public uint code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SCNotification
    {
        public Sci_NotifyHeader nmhdr;
        public int position;            /* SCN_STYLENEEDED, SCN_MODIFIED, SCN_DWELLSTART, SCN_DWELLEND */
        public int ch;                    /* SCN_CHARADDED, SCN_KEY */
        public int modifiers;            /* SCN_KEY */
        public int modificationType;    /* SCN_MODIFIED */
        public IntPtr text;                /* SCN_MODIFIED, SCN_USERLISTSELECTION, SCN_AUTOCSELECTION */
        public int length;                /* SCN_MODIFIED */
        public int linesAdded;            /* SCN_MODIFIED */
        public int message;                /* SCN_MACRORECORD */
        public uint wParam;                /* SCN_MACRORECORD */
        public int lParam;                /* SCN_MACRORECORD */
        public int line;                /* SCN_MODIFIED */
        public int foldLevelNow;        /* SCN_MODIFIED */
        public int foldLevelPrev;        /* SCN_MODIFIED */
        public int margin;                /* SCN_MARGINCLICK */
        public int listType;            /* SCN_USERLISTSELECTION */
        public int x;                    /* SCN_DWELLSTART, SCN_DWELLEND */
        public int y;                    /* SCN_DWELLSTART, SCN_DWELLEND */
        public int token;                /* SCN_MODIFIED with SC_MOD_CONTAINER */
        public int annotationLinesAdded;/* SC_MOD_CHANGEANNOTATION */
    }

    [Flags]
    public enum SciMsg : uint
    {
        INVALID_POSITION = 0xFFFFFFFF,
        SCI_START = 2000,
        SCI_OPTIONAL_START = 3000,
        SCI_LEXER_START = 4000,
        SCI_ADDTEXT = 2001,
        SCI_ADDSTYLEDTEXT = 2002,
        SCI_INSERTTEXT = 2003,
        SCI_CLEARALL = 2004,
        SCI_CLEARDOCUMENTSTYLE = 2005,
        SCI_GETLENGTH = 2006,
        SCI_GETCHARAT = 2007,
        SCI_GETCURRENTPOS = 2008,
        SCI_GETANCHOR = 2009,
        SCI_GETSTYLEAT = 2010,
        SCI_REDO = 2011,
        SCI_SETUNDOCOLLECTION = 2012,
        SCI_SELECTALL = 2013,
        SCI_SETSAVEPOINT = 2014,
        SCI_GETSTYLEDTEXT = 2015,
        SCI_CANREDO = 2016,
        SCI_MARKERLINEFROMHANDLE = 2017,
        SCI_MARKERDELETEHANDLE = 2018,
        SCI_GETUNDOCOLLECTION = 2019,
        SCWS_INVISIBLE = 0,
        SCWS_VISIBLEALWAYS = 1,
        SCWS_VISIBLEAFTERINDENT = 2,
        SCI_GETVIEWWS = 2020,
        SCI_SETVIEWWS = 2021,
        SCI_POSITIONFROMPOINT = 2022,
        SCI_POSITIONFROMPOINTCLOSE = 2023,
        SCI_GOTOLINE = 2024,
        SCI_GOTOPOS = 2025,
        SCI_SETANCHOR = 2026,
        SCI_GETCURLINE = 2027,
        SCI_GETENDSTYLED = 2028,
        SC_EOL_CRLF = 0,
        SC_EOL_CR = 1,
        SC_EOL_LF = 2,
        SCI_CONVERTEOLS = 2029,
        SCI_GETEOLMODE = 2030,
        SCI_SETEOLMODE = 2031,
        SCI_STARTSTYLING = 2032,
        SCI_SETSTYLING = 2033,
        SCI_GETBUFFEREDDRAW = 2034,
        SCI_SETBUFFEREDDRAW = 2035,
        SCI_SETTABWIDTH = 2036,
        SCI_GETTABWIDTH = 2121,
        SC_CP_UTF8 = 65001,
        SC_CP_DBCS = 1,
        SCI_SETCODEPAGE = 2037,
        SCI_SETUSEPALETTE = 2039,
        MARKER_MAX = 31,
        SC_MARK_CIRCLE = 0,
        SC_MARK_ROUNDRECT = 1,
        SC_MARK_ARROW = 2,
        SC_MARK_SMALLRECT = 3,
        SC_MARK_SHORTARROW = 4,
        SC_MARK_EMPTY = 5,
        SC_MARK_ARROWDOWN = 6,
        SC_MARK_MINUS = 7,
        SC_MARK_PLUS = 8,
        SC_MARK_VLINE = 9,
        SC_MARK_LCORNER = 10,
        SC_MARK_TCORNER = 11,
        SC_MARK_BOXPLUS = 12,
        SC_MARK_BOXPLUSCONNECTED = 13,
        SC_MARK_BOXMINUS = 14,
        SC_MARK_BOXMINUSCONNECTED = 15,
        SC_MARK_LCORNERCURVE = 16,
        SC_MARK_TCORNERCURVE = 17,
        SC_MARK_CIRCLEPLUS = 18,
        SC_MARK_CIRCLEPLUSCONNECTED = 19,
        SC_MARK_CIRCLEMINUS = 20,
        SC_MARK_CIRCLEMINUSCONNECTED = 21,
        SC_MARK_BACKGROUND = 22,
        SC_MARK_DOTDOTDOT = 23,
        SC_MARK_ARROWS = 24,
        SC_MARK_PIXMAP = 25,
        SC_MARK_FULLRECT = 26,
        SC_MARK_LEFTRECT = 27,
        SC_MARK_AVAILABLE = 28,
        SC_MARK_UNDERLINE = 29,
        SC_MARK_CHARACTER = 10000,
        SC_MARKNUM_FOLDEREND = 25,
        SC_MARKNUM_FOLDEROPENMID = 26,
        SC_MARKNUM_FOLDERMIDTAIL = 27,
        SC_MARKNUM_FOLDERTAIL = 28,
        SC_MARKNUM_FOLDERSUB = 29,
        SC_MARKNUM_FOLDER = 30,
        SC_MARKNUM_FOLDEROPEN = 31,
        SC_MASK_FOLDERS = 0xFE000000,
        SCI_MARKERDEFINE = 2040,
        SCI_MARKERSETFORE = 2041,
        SCI_MARKERSETBACK = 2042,
        SCI_MARKERADD = 2043,
        SCI_MARKERDELETE = 2044,
        SCI_MARKERDELETEALL = 2045,
        SCI_MARKERGET = 2046,
        SCI_MARKERNEXT = 2047,
        SCI_MARKERPREVIOUS = 2048,
        SCI_MARKERDEFINEPIXMAP = 2049,
        SCI_MARKERADDSET = 2466,
        SCI_MARKERSETALPHA = 2476,
        SC_MARGIN_SYMBOL = 0,
        SC_MARGIN_NUMBER = 1,
        SC_MARGIN_BACK = 2,
        SC_MARGIN_FORE = 3,
        SC_MARGIN_TEXT = 4,
        SC_MARGIN_RTEXT = 5,
        SCI_SETMARGINTYPEN = 2240,
        SCI_GETMARGINTYPEN = 2241,
        SCI_SETMARGINWIDTHN = 2242,
        SCI_GETMARGINWIDTHN = 2243,
        SCI_SETMARGINMASKN = 2244,
        SCI_GETMARGINMASKN = 2245,
        SCI_SETMARGINSENSITIVEN = 2246,
        SCI_GETMARGINSENSITIVEN = 2247,
        STYLE_DEFAULT = 32,
        STYLE_LINENUMBER = 33,
        STYLE_BRACELIGHT = 34,
        STYLE_BRACEBAD = 35,
        STYLE_CONTROLCHAR = 36,
        STYLE_INDENTGUIDE = 37,
        STYLE_CALLTIP = 38,
        STYLE_LASTPREDEFINED = 39,
        STYLE_MAX = 255,
        SC_CHARSET_ANSI = 0,
        SC_CHARSET_DEFAULT = 1,
        SC_CHARSET_BALTIC = 186,
        SC_CHARSET_CHINESEBIG5 = 136,
        SC_CHARSET_EASTEUROPE = 238,
        SC_CHARSET_GB2312 = 134,
        SC_CHARSET_GREEK = 161,
        SC_CHARSET_HANGUL = 129,
        SC_CHARSET_MAC = 77,
        SC_CHARSET_OEM = 255,
        SC_CHARSET_RUSSIAN = 204,
        SC_CHARSET_CYRILLIC = 1251,
        SC_CHARSET_SHIFTJIS = 128,
        SC_CHARSET_SYMBOL = 2,
        SC_CHARSET_TURKISH = 162,
        SC_CHARSET_JOHAB = 130,
        SC_CHARSET_HEBREW = 177,
        SC_CHARSET_ARABIC = 178,
        SC_CHARSET_VIETNAMESE = 163,
        SC_CHARSET_THAI = 222,
        SC_CHARSET_8859_15 = 1000,
        SCI_STYLECLEARALL = 2050,
        SCI_STYLESETFORE = 2051,
        SCI_STYLESETBACK = 2052,
        SCI_STYLESETBOLD = 2053,
        SCI_STYLESETITALIC = 2054,
        SCI_STYLESETSIZE = 2055,
        SCI_STYLESETFONT = 2056,
        SCI_STYLESETEOLFILLED = 2057,
        SCI_STYLERESETDEFAULT = 2058,
        SCI_STYLESETUNDERLINE = 2059,
        SC_CASE_MIXED = 0,
        SC_CASE_UPPER = 1,
        SC_CASE_LOWER = 2,
        SCI_STYLEGETFORE = 2481,
        SCI_STYLEGETBACK = 2482,
        SCI_STYLEGETBOLD = 2483,
        SCI_STYLEGETITALIC = 2484,
        SCI_STYLEGETSIZE = 2485,
        SCI_STYLEGETFONT = 2486,
        SCI_STYLEGETEOLFILLED = 2487,
        SCI_STYLEGETUNDERLINE = 2488,
        SCI_STYLEGETCASE = 2489,
        SCI_STYLEGETCHARACTERSET = 2490,
        SCI_STYLEGETVISIBLE = 2491,
        SCI_STYLEGETCHANGEABLE = 2492,
        SCI_STYLEGETHOTSPOT = 2493,
        SCI_STYLESETCASE = 2060,
        SCI_STYLESETCHARACTERSET = 2066,
        SCI_STYLESETHOTSPOT = 2409,
        SCI_SETSELFORE = 2067,
        SCI_SETSELBACK = 2068,
        SCI_GETSELALPHA = 2477,
        SCI_SETSELALPHA = 2478,
        SCI_GETSELEOLFILLED = 2479,
        SCI_SETSELEOLFILLED = 2480,
        SCI_SETCARETFORE = 2069,
        SCI_ASSIGNCMDKEY = 2070,
        SCI_CLEARCMDKEY = 2071,
        SCI_CLEARALLCMDKEYS = 2072,
        SCI_SETSTYLINGEX = 2073,
        SCI_STYLESETVISIBLE = 2074,
        SCI_GETCARETPERIOD = 2075,
        SCI_SETCARETPERIOD = 2076,
        SCI_SETWORDCHARS = 2077,
        SCI_BEGINUNDOACTION = 2078,
        SCI_ENDUNDOACTION = 2079,
        INDIC_PLAIN = 0,
        INDIC_SQUIGGLE = 1,
        INDIC_TT = 2,
        INDIC_DIAGONAL = 3,
        INDIC_STRIKE = 4,
        INDIC_HIDDEN = 5,
        INDIC_BOX = 6,
        INDIC_ROUNDBOX = 7,
        INDIC_MAX = 31,
        INDIC_CONTAINER = 8,
        INDIC0_MASK = 0x20,
        INDIC1_MASK = 0x40,
        INDIC2_MASK = 0x80,
        INDICS_MASK = 0xE0,
        SCI_INDICSETSTYLE = 2080,
        SCI_INDICGETSTYLE = 2081,
        SCI_INDICSETFORE = 2082,
        SCI_INDICGETFORE = 2083,
        SCI_INDICSETUNDER = 2510,
        SCI_INDICGETUNDER = 2511,
        SCI_GETCARETLINEVISIBLEALWAYS = 3095,
        SCI_SETCARETLINEVISIBLEALWAYS = 3096,
        SCI_SETWHITESPACEFORE = 2084,
        SCI_SETWHITESPACEBACK = 2085,
        SCI_SETSTYLEBITS = 2090,
        SCI_GETSTYLEBITS = 2091,
        SCI_SETLINESTATE = 2092,
        SCI_GETLINESTATE = 2093,
        SCI_GETMAXLINESTATE = 2094,
        SCI_GETCARETLINEVISIBLE = 2095,
        SCI_SETCARETLINEVISIBLE = 2096,
        SCI_GETCARETLINEBACK = 2097,
        SCI_SETCARETLINEBACK = 2098,
        SCI_STYLESETCHANGEABLE = 2099,
        SCI_AUTOCSHOW = 2100,
        SCI_AUTOCCANCEL = 2101,
        SCI_AUTOCACTIVE = 2102,
        SCI_AUTOCPOSSTART = 2103,
        SCI_AUTOCCOMPLETE = 2104,
        SCI_AUTOCSTOPS = 2105,
        SCI_AUTOCSETSEPARATOR = 2106,
        SCI_AUTOCGETSEPARATOR = 2107,
        SCI_AUTOCSELECT = 2108,
        SCI_AUTOCSETCANCELATSTART = 2110,
        SCI_AUTOCGETCANCELATSTART = 2111,
        SCI_AUTOCSETFILLUPS = 2112,
        SCI_AUTOCSETCHOOSESINGLE = 2113,
        SCI_AUTOCGETCHOOSESINGLE = 2114,
        SCI_AUTOCSETIGNORECASE = 2115,
        SCI_AUTOCGETIGNORECASE = 2116,
        SCI_USERLISTSHOW = 2117,
        SCI_AUTOCSETAUTOHIDE = 2118,
        SCI_AUTOCGETAUTOHIDE = 2119,
        SCI_AUTOCSETDROPRESTOFWORD = 2270,
        SCI_AUTOCGETDROPRESTOFWORD = 2271,
        SCI_REGISTERIMAGE = 2405,
        SCI_CLEARREGISTEREDIMAGES = 2408,
        SCI_AUTOCGETTYPESEPARATOR = 2285,
        SCI_AUTOCSETTYPESEPARATOR = 2286,
        SCI_AUTOCSETMAXWIDTH = 2208,
        SCI_AUTOCGETMAXWIDTH = 2209,
        SCI_AUTOCSETMAXHEIGHT = 2210,
        SCI_AUTOCGETMAXHEIGHT = 2211,
        SCI_SETINDENT = 2122,
        SCI_GETINDENT = 2123,
        SCI_SETUSETABS = 2124,
        SCI_GETUSETABS = 2125,
        SCI_SETLINEINDENTATION = 2126,
        SCI_GETLINEINDENTATION = 2127,
        SCI_GETLINEINDENTPOSITION = 2128,
        SCI_GETCOLUMN = 2129,
        SCI_SETHSCROLLBAR = 2130,
        SCI_GETHSCROLLBAR = 2131,
        SC_IV_NONE = 0,
        SC_IV_REAL = 1,
        SC_IV_LOOKFORWARD = 2,
        SC_IV_LOOKBOTH = 3,
        SCI_SETINDENTATIONGUIDES = 2132,
        SCI_GETINDENTATIONGUIDES = 2133,
        SCI_SETHIGHLIGHTGUIDE = 2134,
        SCI_GETHIGHLIGHTGUIDE = 2135,
        SCI_GETLINEENDPOSITION = 2136,
        SCI_GETCODEPAGE = 2137,
        SCI_GETCARETFORE = 2138,
        SCI_GETUSEPALETTE = 2139,
        SCI_GETREADONLY = 2140,
        SCI_SETCURRENTPOS = 2141,
        SCI_SETSELECTIONSTART = 2142,
        SCI_GETSELECTIONSTART = 2143,
        SCI_SETSELECTIONEND = 2144,
        SCI_GETSELECTIONEND = 2145,
        SCI_SETPRINTMAGNIFICATION = 2146,
        SCI_GETPRINTMAGNIFICATION = 2147,
        SC_PRINT_NORMAL = 0,
        SC_PRINT_INVERTLIGHT = 1,
        SC_PRINT_BLACKONWHITE = 2,
        SC_PRINT_COLOURONWHITE = 3,
        SC_PRINT_COLOURONWHITEDEFAULTBG = 4,
        SCI_SETPRINTCOLOURMODE = 2148,
        SCI_GETPRINTCOLOURMODE = 2149,
        SCFIND_WHOLEWORD = 2,
        SCFIND_MATCHCASE = 4,
        SCFIND_WORDSTART = 0x00100000,
        SCFIND_REGEXP = 0x00200000,
        SCFIND_POSIX = 0x00400000,
        SCI_FINDTEXT = 2150,
        SCI_FORMATRANGE = 2151,
        SCI_GETFIRSTVISIBLELINE = 2152,
        SCI_GETLINE = 2153,
        SCI_GETLINECOUNT = 2154,
        SCI_SETMARGINLEFT = 2155,
        SCI_GETMARGINLEFT = 2156,
        SCI_SETMARGINRIGHT = 2157,
        SCI_GETMARGINRIGHT = 2158,
        SCI_GETMODIFY = 2159,
        SCI_SETSEL = 2160,
        SCI_GETSELTEXT = 2161,
        SCI_GETTEXTRANGE = 2162,
        SCI_HIDESELECTION = 2163,
        SCI_POINTXFROMPOSITION = 2164,
        SCI_POINTYFROMPOSITION = 2165,
        SCI_LINEFROMPOSITION = 2166,
        SCI_POSITIONFROMLINE = 2167,
        SCI_LINESCROLL = 2168,
        SCI_SCROLLCARET = 2169,
        SCI_REPLACESEL = 2170,
        SCI_SETREADONLY = 2171,
        SCI_NULL = 2172,
        SCI_CANPASTE = 2173,
        SCI_CANUNDO = 2174,
        SCI_EMPTYUNDOBUFFER = 2175,
        SCI_UNDO = 2176,
        SCI_CUT = 2177,
        SCI_COPY = 2178,
        SCI_PASTE = 2179,
        SCI_CLEAR = 2180,
        SCI_SETTEXT = 2181,
        SCI_GETTEXT = 2182,
        SCI_GETTEXTLENGTH = 2183,
        SCI_GETDIRECTFUNCTION = 2184,
        SCI_GETDIRECTPOINTER = 2185,
        SCI_SETOVERTYPE = 2186,
        SCI_GETOVERTYPE = 2187,
        SCI_SETCARETWIDTH = 2188,
        SCI_GETCARETWIDTH = 2189,
        SCI_SETTARGETSTART = 2190,
        SCI_GETTARGETSTART = 2191,
        SCI_SETTARGETEND = 2192,
        SCI_GETTARGETEND = 2193,
        SCI_REPLACETARGET = 2194,
        SCI_REPLACETARGETRE = 2195,
        SCI_SEARCHINTARGET = 2197,
        SCI_SETSEARCHFLAGS = 2198,
        SCI_GETSEARCHFLAGS = 2199,
        SCI_CALLTIPSHOW = 2200,
        SCI_CALLTIPCANCEL = 2201,
        SCI_CALLTIPACTIVE = 2202,
        SCI_CALLTIPPOSSTART = 2203,
        SCI_CALLTIPSETHLT = 2204,
        SCI_CALLTIPSETBACK = 2205,
        SCI_CALLTIPSETFORE = 2206,
        SCI_CALLTIPSETFOREHLT = 2207,
        SCI_CALLTIPUSESTYLE = 2212,
        SCI_VISIBLEFROMDOCLINE = 2220,
        SCI_DOCLINEFROMVISIBLE = 2221,
        SCI_WRAPCOUNT = 2235,
        SC_FOLDLEVELBASE = 0x400,
        SC_FOLDLEVELWHITEFLAG = 0x1000,
        SC_FOLDLEVELHEADERFLAG = 0x2000,
        SC_FOLDLEVELNUMBERMASK = 0x0FFF,
        SCI_SETFOLDLEVEL = 2222,
        SCI_GETFOLDLEVEL = 2223,
        SCI_GETLASTCHILD = 2224,
        SCI_GETFOLDPARENT = 2225,
        SCI_SHOWLINES = 2226,
        SCI_HIDELINES = 2227,
        SCI_GETLINEVISIBLE = 2228,
        SCI_SETFOLDEXPANDED = 2229,
        SCI_GETFOLDEXPANDED = 2230,
        SCI_TOGGLEFOLD = 2231,
        SCI_ENSUREVISIBLE = 2232,
        SC_FOLDFLAG_LINEBEFORE_EXPANDED = 0x0002,
        SC_FOLDFLAG_LINEBEFORE_CONTRACTED = 0x0004,
        SC_FOLDFLAG_LINEAFTER_EXPANDED = 0x0008,
        SC_FOLDFLAG_LINEAFTER_CONTRACTED = 0x0010,
        SC_FOLDFLAG_LEVELNUMBERS = 0x0040,
        SCI_SETFOLDFLAGS = 2233,
        SCI_ENSUREVISIBLEENFORCEPOLICY = 2234,
        SCI_SETTABINDENTS = 2260,
        SCI_GETTABINDENTS = 2261,
        SCI_SETBACKSPACEUNINDENTS = 2262,
        SCI_GETBACKSPACEUNINDENTS = 2263,
        SC_TIME_FOREVER = 10000000,
        SCI_SETMOUSEDWELLTIME = 2264,
        SCI_GETMOUSEDWELLTIME = 2265,
        SCI_WORDSTARTPOSITION = 2266,
        SCI_WORDENDPOSITION = 2267,
        SC_WRAP_NONE = 0,
        SC_WRAP_WORD = 1,
        SC_WRAP_CHAR = 2,
        SCI_SETWRAPMODE = 2268,
        SCI_GETWRAPMODE = 2269,
        SC_WRAPVISUALFLAG_NONE = 0x0000,
        SC_WRAPVISUALFLAG_END = 0x0001,
        SC_WRAPVISUALFLAG_START = 0x0002,
        SCI_SETWRAPVISUALFLAGS = 2460,
        SCI_GETWRAPVISUALFLAGS = 2461,
        SC_WRAPVISUALFLAGLOC_DEFAULT = 0x0000,
        SC_WRAPVISUALFLAGLOC_END_BY_TEXT = 0x0001,
        SC_WRAPVISUALFLAGLOC_START_BY_TEXT = 0x0002,
        SCI_SETWRAPVISUALFLAGSLOCATION = 2462,
        SCI_GETWRAPVISUALFLAGSLOCATION = 2463,
        SCI_SETWRAPSTARTINDENT = 2464,
        SCI_GETWRAPSTARTINDENT = 2465,
        SC_WRAPINDENT_FIXED = 0,
        SC_WRAPINDENT_SAME = 1,
        SC_WRAPINDENT_INDENT = 2,
        SCI_SETWRAPINDENTMODE = 2472,
        SCI_GETWRAPINDENTMODE = 2473,
        SC_CACHE_NONE = 0,
        SC_CACHE_CARET = 1,
        SC_CACHE_PAGE = 2,
        SC_CACHE_DOCUMENT = 3,
        SCI_SETLAYOUTCACHE = 2272,
        SCI_GETLAYOUTCACHE = 2273,
        SCI_SETSCROLLWIDTH = 2274,
        SCI_GETSCROLLWIDTH = 2275,
        SCI_SETSCROLLWIDTHTRACKING = 2516,
        SCI_GETSCROLLWIDTHTRACKING = 2517,
        SCI_TEXTWIDTH = 2276,
        SCI_SETENDATLASTLINE = 2277,
        SCI_GETENDATLASTLINE = 2278,
        SCI_TEXTHEIGHT = 2279,
        SCI_SETVSCROLLBAR = 2280,
        SCI_GETVSCROLLBAR = 2281,
        SCI_APPENDTEXT = 2282,
        SCI_GETTWOPHASEDRAW = 2283,
        SCI_SETTWOPHASEDRAW = 2284,
        SCI_TARGETFROMSELECTION = 2287,
        SCI_LINESJOIN = 2288,
        SCI_LINESSPLIT = 2289,
        SCI_SETFOLDMARGINCOLOUR = 2290,
        SCI_SETFOLDMARGINHICOLOUR = 2291,
        SCI_LINEDOWN = 2300,
        SCI_LINEDOWNEXTEND = 2301,
        SCI_LINEUP = 2302,
        SCI_LINEUPEXTEND = 2303,
        SCI_CHARLEFT = 2304,
        SCI_CHARLEFTEXTEND = 2305,
        SCI_CHARRIGHT = 2306,
        SCI_CHARRIGHTEXTEND = 2307,
        SCI_WORDLEFT = 2308,
        SCI_WORDLEFTEXTEND = 2309,
        SCI_WORDRIGHT = 2310,
        SCI_WORDRIGHTEXTEND = 2311,
        SCI_HOME = 2312,
        SCI_HOMEEXTEND = 2313,
        SCI_LINEEND = 2314,
        SCI_LINEENDEXTEND = 2315,
        SCI_DOCUMENTSTART = 2316,
        SCI_DOCUMENTSTARTEXTEND = 2317,
        SCI_DOCUMENTEND = 2318,
        SCI_DOCUMENTENDEXTEND = 2319,
        SCI_PAGEUP = 2320,
        SCI_PAGEUPEXTEND = 2321,
        SCI_PAGEDOWN = 2322,
        SCI_PAGEDOWNEXTEND = 2323,
        SCI_EDITTOGGLEOVERTYPE = 2324,
        SCI_CANCEL = 2325,
        SCI_DELETEBACK = 2326,
        SCI_TAB = 2327,
        SCI_BACKTAB = 2328,
        SCI_NEWLINE = 2329,
        SCI_FORMFEED = 2330,
        SCI_VCHOME = 2331,
        SCI_VCHOMEEXTEND = 2332,
        SCI_ZOOMIN = 2333,
        SCI_ZOOMOUT = 2334,
        SCI_DELWORDLEFT = 2335,
        SCI_DELWORDRIGHT = 2336,
        SCI_DELWORDRIGHTEND = 2518,
        SCI_LINECUT = 2337,
        SCI_LINEDELETE = 2338,
        SCI_LINETRANSPOSE = 2339,
        SCI_LINEDUPLICATE = 2404,
        SCI_LOWERCASE = 2340,
        SCI_UPPERCASE = 2341,
        SCI_LINESCROLLDOWN = 2342,
        SCI_LINESCROLLUP = 2343,
        SCI_DELETEBACKNOTLINE = 2344,
        SCI_HOMEDISPLAY = 2345,
        SCI_HOMEDISPLAYEXTEND = 2346,
        SCI_LINEENDDISPLAY = 2347,
        SCI_LINEENDDISPLAYEXTEND = 2348,
        SCI_HOMEWRAP = 2349,
        SCI_HOMEWRAPEXTEND = 2450,
        SCI_LINEENDWRAP = 2451,
        SCI_LINEENDWRAPEXTEND = 2452,
        SCI_VCHOMEWRAP = 2453,
        SCI_VCHOMEWRAPEXTEND = 2454,
        SCI_LINECOPY = 2455,
        SCI_MOVECARETINSIDEVIEW = 2401,
        SCI_LINELENGTH = 2350,
        SCI_BRACEHIGHLIGHT = 2351,
        SCI_BRACEBADLIGHT = 2352,
        SCI_BRACEMATCH = 2353,
        SCI_GETVIEWEOL = 2355,
        SCI_SETVIEWEOL = 2356,
        SCI_GETDOCPOINTER = 2357,
        SCI_SETDOCPOINTER = 2358,
        SCI_SETMODEVENTMASK = 2359,
        EDGE_NONE = 0,
        EDGE_LINE = 1,
        EDGE_BACKGROUND = 2,
        SCI_GETEDGECOLUMN = 2360,
        SCI_SETEDGECOLUMN = 2361,
        SCI_GETEDGEMODE = 2362,
        SCI_SETEDGEMODE = 2363,
        SCI_GETEDGECOLOUR = 2364,
        SCI_SETEDGECOLOUR = 2365,
        SCI_SEARCHANCHOR = 2366,
        SCI_SEARCHNEXT = 2367,
        SCI_SEARCHPREV = 2368,
        SCI_LINESONSCREEN = 2370,
        SCI_USEPOPUP = 2371,
        SCI_SELECTIONISRECTANGLE = 2372,
        SCI_SETZOOM = 2373,
        SCI_GETZOOM = 2374,
        SCI_CREATEDOCUMENT = 2375,
        SCI_ADDREFDOCUMENT = 2376,
        SCI_RELEASEDOCUMENT = 2377,
        SCI_GETMODEVENTMASK = 2378,
        SCI_SETFOCUS = 2380,
        SCI_GETFOCUS = 2381,
        SC_STATUS_OK = 0,
        SC_STATUS_FAILURE = 1,
        SC_STATUS_BADALLOC = 2,
        SCI_SETSTATUS = 2382,
        SCI_GETSTATUS = 2383,
        SCI_SETMOUSEDOWNCAPTURES = 2384,
        SCI_GETMOUSEDOWNCAPTURES = 2385,
        SC_CURSORNORMAL = 0xFFFFFFFF,
        SC_CURSORWAIT = 4,
        SCI_SETCURSOR = 2386,
        SCI_GETCURSOR = 2387,
        SCI_SETCONTROLCHARSYMBOL = 2388,
        SCI_GETCONTROLCHARSYMBOL = 2389,
        SCI_WORDPARTLEFT = 2390,
        SCI_WORDPARTLEFTEXTEND = 2391,
        SCI_WORDPARTRIGHT = 2392,
        SCI_WORDPARTRIGHTEXTEND = 2393,
        VISIBLE_SLOP = 0x01,
        VISIBLE_STRICT = 0x04,
        SCI_SETVISIBLEPOLICY = 2394,
        SCI_DELLINELEFT = 2395,
        SCI_DELLINERIGHT = 2396,
        SCI_SETXOFFSET = 2397,
        SCI_GETXOFFSET = 2398,
        SCI_CHOOSECARETX = 2399,
        SCI_GRABFOCUS = 2400,
        CARET_SLOP = 0x01,
        CARET_STRICT = 0x04,
        CARET_JUMPS = 0x10,
        CARET_EVEN = 0x08,
        SCI_SETXCARETPOLICY = 2402,
        SCI_SETYCARETPOLICY = 2403,
        SCI_SETPRINTWRAPMODE = 2406,
        SCI_GETPRINTWRAPMODE = 2407,
        SCI_SETHOTSPOTACTIVEFORE = 2410,
        SCI_GETHOTSPOTACTIVEFORE = 2494,
        SCI_SETHOTSPOTACTIVEBACK = 2411,
        SCI_GETHOTSPOTACTIVEBACK = 2495,
        SCI_SETHOTSPOTACTIVEUNDERLINE = 2412,
        SCI_GETHOTSPOTACTIVEUNDERLINE = 2496,
        SCI_SETHOTSPOTSINGLELINE = 2421,
        SCI_GETHOTSPOTSINGLELINE = 2497,
        SCI_PARADOWN = 2413,
        SCI_PARADOWNEXTEND = 2414,
        SCI_PARAUP = 2415,
        SCI_PARAUPEXTEND = 2416,
        SCI_POSITIONBEFORE = 2417,
        SCI_POSITIONAFTER = 2418,
        SCI_COPYRANGE = 2419,
        SCI_COPYTEXT = 2420,
        SC_SEL_STREAM = 0,
        SC_SEL_RECTANGLE = 1,
        SC_SEL_LINES = 2,
        SC_SEL_THIN = 3,
        SCI_SETSELECTIONMODE = 2422,
        SCI_GETSELECTIONMODE = 2423,
        SCI_GETLINESELSTARTPOSITION = 2424,
        SCI_GETLINESELENDPOSITION = 2425,
        SCI_LINEDOWNRECTEXTEND = 2426,
        SCI_LINEUPRECTEXTEND = 2427,
        SCI_CHARLEFTRECTEXTEND = 2428,
        SCI_CHARRIGHTRECTEXTEND = 2429,
        SCI_HOMERECTEXTEND = 2430,
        SCI_VCHOMERECTEXTEND = 2431,
        SCI_LINEENDRECTEXTEND = 2432,
        SCI_PAGEUPRECTEXTEND = 2433,
        SCI_PAGEDOWNRECTEXTEND = 2434,
        SCI_STUTTEREDPAGEUP = 2435,
        SCI_STUTTEREDPAGEUPEXTEND = 2436,
        SCI_STUTTEREDPAGEDOWN = 2437,
        SCI_STUTTEREDPAGEDOWNEXTEND = 2438,
        SCI_WORDLEFTEND = 2439,
        SCI_WORDLEFTENDEXTEND = 2440,
        SCI_WORDRIGHTEND = 2441,
        SCI_WORDRIGHTENDEXTEND = 2442,
        SCI_SETWHITESPACECHARS = 2443,
        SCI_SETCHARSDEFAULT = 2444,
        SCI_AUTOCGETCURRENT = 2445,
        SCI_ALLOCATE = 2446,
        SCI_TARGETASUTF8 = 2447,
        SCI_SETLENGTHFORENCODE = 2448,
        SCI_ENCODEDFROMUTF8 = 2449,
        SCI_FINDCOLUMN = 2456,
        SCI_GETCARETSTICKY = 2457,
        SCI_SETCARETSTICKY = 2458,
        SCI_TOGGLECARETSTICKY = 2459,
        SCI_SETPASTECONVERTENDINGS = 2467,
        SCI_GETPASTECONVERTENDINGS = 2468,
        SCI_SELECTIONDUPLICATE = 2469,
        SC_ALPHA_TRANSPARENT = 0,
        SC_ALPHA_OPAQUE = 255,
        SC_ALPHA_NOALPHA = 256,
        SCI_SETCARETLINEBACKALPHA = 2470,
        SCI_GETCARETLINEBACKALPHA = 2471,
        CARETSTYLE_INVISIBLE = 0,
        CARETSTYLE_LINE = 1,
        CARETSTYLE_BLOCK = 2,
        SCI_SETCARETSTYLE = 2512,
        SCI_GETCARETSTYLE = 2513,
        SCI_SETINDICATORCURRENT = 2500,
        SCI_GETINDICATORCURRENT = 2501,
        SCI_SETINDICATORVALUE = 2502,
        SCI_GETINDICATORVALUE = 2503,
        SCI_INDICATORFILLRANGE = 2504,
        SCI_INDICATORCLEARRANGE = 2505,
        SCI_INDICATORALLONFOR = 2506,
        SCI_INDICATORVALUEAT = 2507,
        SCI_INDICATORSTART = 2508,
        SCI_INDICATOREND = 2509,
        SCI_SETPOSITIONCACHE = 2514,
        SCI_GETPOSITIONCACHE = 2515,
        SCI_COPYALLOWLINE = 2519,
        SCI_GETCHARACTERPOINTER = 2520,
        SCI_SETKEYSUNICODE = 2521,
        SCI_GETKEYSUNICODE = 2522,
        SCI_INDICSETALPHA = 2523,
        SCI_INDICGETALPHA = 2524,
        SCI_SETEXTRAASCENT = 2525,
        SCI_GETEXTRAASCENT = 2526,
        SCI_SETEXTRADESCENT = 2527,
        SCI_GETEXTRADESCENT = 2528,
        SCI_MARKERSYMBOLDEFINED = 2529,
        SCI_MARGINSETTEXT = 2530,
        SCI_MARGINGETTEXT = 2531,
        SCI_MARGINSETSTYLE = 2532,
        SCI_MARGINGETSTYLE = 2533,
        SCI_MARGINSETSTYLES = 2534,
        SCI_MARGINGETSTYLES = 2535,
        SCI_MARGINTEXTCLEARALL = 2536,
        SCI_MARGINSETSTYLEOFFSET = 2537,
        SCI_MARGINGETSTYLEOFFSET = 2538,
        SCI_ANNOTATIONSETTEXT = 2540,
        SCI_ANNOTATIONGETTEXT = 2541,
        SCI_ANNOTATIONSETSTYLE = 2542,
        SCI_ANNOTATIONGETSTYLE = 2543,
        SCI_ANNOTATIONSETSTYLES = 2544,
        SCI_ANNOTATIONGETSTYLES = 2545,
        SCI_ANNOTATIONGETLINES = 2546,
        SCI_ANNOTATIONCLEARALL = 2547,
        ANNOTATION_HIDDEN = 0,
        ANNOTATION_STANDARD = 1,
        ANNOTATION_BOXED = 2,
        SCI_ANNOTATIONSETVISIBLE = 2548,
        SCI_ANNOTATIONGETVISIBLE = 2549,
        SCI_ANNOTATIONSETSTYLEOFFSET = 2550,
        SCI_ANNOTATIONGETSTYLEOFFSET = 2551,
        UNDO_MAY_COALESCE = 1,
        SCI_ADDUNDOACTION = 2560,
        SCI_CHARPOSITIONFROMPOINT = 2561,
        SCI_CHARPOSITIONFROMPOINTCLOSE = 2562,
        SCI_SETMULTIPLESELECTION = 2563,
        SCI_GETMULTIPLESELECTION = 2564,
        SCI_SETADDITIONALSELECTIONTYPING = 2565,
        SCI_GETADDITIONALSELECTIONTYPING = 2566,
        SCI_SETADDITIONALCARETSBLINK = 2567,
        SCI_GETADDITIONALCARETSBLINK = 2568,
        SCI_GETSELECTIONS = 2570,
        SCI_CLEARSELECTIONS = 2571,
        SCI_SETSELECTION = 2572,
        SCI_ADDSELECTION = 2573,
        SCI_SETMAINSELECTION = 2574,
        SCI_GETMAINSELECTION = 2575,
        SCI_SETSELECTIONNCARET = 2576,
        SCI_GETSELECTIONNCARET = 2577,
        SCI_SETSELECTIONNANCHOR = 2578,
        SCI_GETSELECTIONNANCHOR = 2579,
        SCI_SETSELECTIONNCARETVIRTUALSPACE = 2580,
        SCI_GETSELECTIONNCARETVIRTUALSPACE = 2581,
        SCI_SETSELECTIONNANCHORVIRTUALSPACE = 2582,
        SCI_GETSELECTIONNANCHORVIRTUALSPACE = 2583,
        SCI_SETSELECTIONNSTART = 2584,
        SCI_GETSELECTIONNSTART = 2585,
        SCI_SETSELECTIONNEND = 2586,
        SCI_GETSELECTIONNEND = 2587,
        SCI_SETRECTANGULARSELECTIONCARET = 2588,
        SCI_GETRECTANGULARSELECTIONCARET = 2589,
        SCI_SETRECTANGULARSELECTIONANCHOR = 2590,
        SCI_GETRECTANGULARSELECTIONANCHOR = 2591,
        SCI_SETRECTANGULARSELECTIONCARETVIRTUALSPACE = 2592,
        SCI_GETRECTANGULARSELECTIONCARETVIRTUALSPACE = 2593,
        SCI_SETRECTANGULARSELECTIONANCHORVIRTUALSPACE = 2594,
        SCI_GETRECTANGULARSELECTIONANCHORVIRTUALSPACE = 2595,
        SCVS_NONE = 0,
        SCVS_RECTANGULARSELECTION = 1,
        SCVS_USERACCESSIBLE = 2,
        SCI_SETVIRTUALSPACEOPTIONS = 2596,
        SCI_GETVIRTUALSPACEOPTIONS = 2597,
        SCI_SETRECTANGULARSELECTIONMODIFIER = 2598,
        SCI_GETRECTANGULARSELECTIONMODIFIER = 2599,
        SCI_SETADDITIONALSELFORE = 2600,
        SCI_SETADDITIONALSELBACK = 2601,
        SCI_SETADDITIONALSELALPHA = 2602,
        SCI_GETADDITIONALSELALPHA = 2603,
        SCI_SETADDITIONALCARETFORE = 2604,
        SCI_GETADDITIONALCARETFORE = 2605,
        SCI_ROTATESELECTION = 2606,
        SCI_SWAPMAINANCHORCARET = 2607,
        SCI_STARTRECORD = 3001,
        SCI_STOPRECORD = 3002,
        SCI_SETLEXER = 4001,
        SCI_GETLEXER = 4002,
        SCI_COLOURISE = 4003,
        SCI_SETPROPERTY = 4004,
        KEYWORDSET_MAX = 8,
        SCI_SETKEYWORDS = 4005,
        SCI_SETLEXERLANGUAGE = 4006,
        SCI_LOADLEXERLIBRARY = 4007,
        SCI_GETPROPERTY = 4008,
        SCI_GETPROPERTYEXPANDED = 4009,
        SCI_GETPROPERTYINT = 4010,
        SCI_GETSTYLEBITSNEEDED = 4011,
        SC_MOD_INSERTTEXT = 0x1,
        SC_MOD_DELETETEXT = 0x2,
        SC_MOD_CHANGESTYLE = 0x4,
        SC_MOD_CHANGEFOLD = 0x8,
        SC_PERFORMED_USER = 0x10,
        SC_PERFORMED_UNDO = 0x20,
        SC_PERFORMED_REDO = 0x40,
        SC_MULTISTEPUNDOREDO = 0x80,
        SC_LASTSTEPINUNDOREDO = 0x100,
        SC_MOD_CHANGEMARKER = 0x200,
        SC_MOD_BEFOREINSERT = 0x400,
        SC_MOD_BEFOREDELETE = 0x800,
        SC_MULTILINEUNDOREDO = 0x1000,
        SC_STARTACTION = 0x2000,
        SC_MOD_CHANGEINDICATOR = 0x4000,
        SC_MOD_CHANGELINESTATE = 0x8000,
        SC_MOD_CHANGEMARGIN = 0x10000,
        SC_MOD_CHANGEANNOTATION = 0x20000,
        SC_MOD_CONTAINER = 0x40000,
        SC_MODEVENTMASKALL = 0x7FFFF,
        SC_SEARCHRESULT_LINEBUFFERMAXLENGTH = 1024,
        SCEN_CHANGE = 768,
        SCEN_SETFOCUS = 512,
        SCEN_KILLFOCUS = 256,
        SCK_DOWN = 300,
        SCK_UP = 301,
        SCK_LEFT = 302,
        SCK_RIGHT = 303,
        SCK_HOME = 304,
        SCK_END = 305,
        SCK_PRIOR = 306,
        SCK_NEXT = 307,
        SCK_DELETE = 308,
        SCK_INSERT = 309,
        SCK_ESCAPE = 7,
        SCK_BACK = 8,
        SCK_TAB = 9,
        SCK_RETURN = 13,
        SCK_ADD = 310,
        SCK_SUBTRACT = 311,
        SCK_DIVIDE = 312,
        SCK_WIN = 313,
        SCK_RWIN = 314,
        SCK_MENU = 315,
        SCMOD_NORM = 0,
        SCMOD_SHIFT = 1,
        SCMOD_CTRL = 2,
        SCMOD_ALT = 4,
        SCMOD_SUPER = 8,
        SCN_STYLENEEDED = 2000,
        SCN_CHARADDED = 2001,
        SCN_SAVEPOINTREACHED = 2002,
        SCN_SAVEPOINTLEFT = 2003,
        SCN_MODIFYATTEMPTRO = 2004,
        SCN_KEY = 2005,
        SCN_DOUBLECLICK = 2006,
        SCN_UPDATEUI = 2007,
        SCN_MODIFIED = 2008,
        SCN_MACRORECORD = 2009,
        SCN_MARGINCLICK = 2010,
        SCN_NEEDSHOWN = 2011,
        SCN_PAINTED = 2013,
        SCN_USERLISTSELECTION = 2014,
        SCN_URIDROPPED = 2015,
        SCN_DWELLSTART = 2016,
        SCN_DWELLEND = 2017,
        SCN_ZOOM = 2018,
        SCN_HOTSPOTCLICK = 2019,
        SCN_HOTSPOTDOUBLECLICK = 2020,
        SCN_CALLTIPCLICK = 2021,
        SCN_AUTOCSELECTION = 2022,
        SCN_INDICATORCLICK = 2023,
        SCN_INDICATORRELEASE = 2024,
        SCN_AUTOCCANCELLED = 2025,
        SCN_AUTOCCHARDELETED = 2026,
        SCN_SCROLLED = 2080
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_CharacterRange
    {
        public Sci_CharacterRange(int cpmin, int cpmax) { cpMin = cpmin; cpMax = cpmax; }
        public int cpMin;
        public int cpMax;
    }

    public class Sci_TextRange : IDisposable
    {
        _Sci_TextRange _sciTextRange;
        IntPtr _ptrSciTextRange;
        bool _disposed = false;
        
        public Sci_TextRange(Sci_CharacterRange chrRange, int stringCapacity)
        {
            _sciTextRange.chrg = chrRange;
            _sciTextRange.lpstrText = Marshal.AllocHGlobal(stringCapacity);
        }
        public Sci_TextRange(int cpmin, int cpmax, int stringCapacity)
        {
            _sciTextRange.chrg.cpMin = cpmin;
            _sciTextRange.chrg.cpMax = cpmax;
            _sciTextRange.lpstrText = Marshal.AllocHGlobal(stringCapacity);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct _Sci_TextRange
        {
            public Sci_CharacterRange chrg;
            public IntPtr lpstrText;
        }

        public IntPtr NativePointer { get { _initNativeStruct(); return _ptrSciTextRange; } }
        public string lpstrText { get { _readNativeStruct(); return Marshal.PtrToStringAnsi(_sciTextRange.lpstrText); } }
        public Sci_CharacterRange chrg { get { _readNativeStruct(); return _sciTextRange.chrg; } set { _sciTextRange.chrg = value; _initNativeStruct(); } }
        void _initNativeStruct()
        {
            if (_ptrSciTextRange == IntPtr.Zero)
                _ptrSciTextRange = Marshal.AllocHGlobal(Marshal.SizeOf(_sciTextRange));
            Marshal.StructureToPtr(_sciTextRange, _ptrSciTextRange, false);
        }
        void _readNativeStruct()
        {
            if (_ptrSciTextRange != IntPtr.Zero)
                _sciTextRange = (_Sci_TextRange)Marshal.PtrToStructure(_ptrSciTextRange, typeof(_Sci_TextRange));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_sciTextRange.lpstrText != IntPtr.Zero) Marshal.FreeHGlobal(_sciTextRange.lpstrText);
                if (_ptrSciTextRange != IntPtr.Zero) Marshal.FreeHGlobal(_ptrSciTextRange);
                _disposed = true;
            }
        }
        ~Sci_TextRange()
        {
            Dispose();
        }
    }

    public class Sci_TextToFind : IDisposable
    {
        _Sci_TextToFind _sciTextToFind;
        IntPtr _ptrSciTextToFind;
        bool _disposed = false;

        public Sci_TextToFind(Sci_CharacterRange chrRange, string searchText)
        {
            _sciTextToFind.chrg = chrRange;
            _sciTextToFind.lpstrText = Marshal.StringToHGlobalAnsi(searchText);
        }
        public Sci_TextToFind(int cpmin, int cpmax, string searchText)
        {
            _sciTextToFind.chrg.cpMin = cpmin;
            _sciTextToFind.chrg.cpMax = cpmax;
            _sciTextToFind.lpstrText = Marshal.StringToHGlobalAnsi(searchText);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct _Sci_TextToFind
        {
            public Sci_CharacterRange chrg;
            public IntPtr lpstrText;
            public Sci_CharacterRange chrgText;
        }

        public IntPtr NativePointer { get { _initNativeStruct(); return _ptrSciTextToFind; } }
        public string lpstrText { set { _freeNativeString(); _sciTextToFind.lpstrText = Marshal.StringToHGlobalAnsi(value); } }
        public Sci_CharacterRange chrg { get { _readNativeStruct(); return _sciTextToFind.chrg; } set { _sciTextToFind.chrg = value; _initNativeStruct(); } }
        public Sci_CharacterRange chrgText { get { _readNativeStruct(); return _sciTextToFind.chrgText; } }
        void _initNativeStruct()
        {
            if (_ptrSciTextToFind == IntPtr.Zero)
                _ptrSciTextToFind = Marshal.AllocHGlobal(Marshal.SizeOf(_sciTextToFind));
            Marshal.StructureToPtr(_sciTextToFind, _ptrSciTextToFind, false);
        }
        void _readNativeStruct()
        {
            if (_ptrSciTextToFind != IntPtr.Zero)
                _sciTextToFind = (_Sci_TextToFind)Marshal.PtrToStructure(_ptrSciTextToFind, typeof(_Sci_TextToFind));
        }
        void _freeNativeString()
        {
            if (_sciTextToFind.lpstrText != IntPtr.Zero) Marshal.FreeHGlobal(_sciTextToFind.lpstrText);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _freeNativeString();
                if (_ptrSciTextToFind != IntPtr.Zero) Marshal.FreeHGlobal(_ptrSciTextToFind);
                _disposed = true;
            }
        }
        ~Sci_TextToFind()
        {
            Dispose();
        }
    }
    #endregion

    #region " Platform "
    public class Win32
    {
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, NppMenuCmd lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, int lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, out int lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, IntPtr wParam, int lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, ref LangType lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, NppMsg Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, string lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SciMsg Msg, int wParam, int lParam);

        public const int MAX_PATH = 260;
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);
        [DllImport("kernel32")]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        public const int MF_BYCOMMAND = 0;
        public const int MF_CHECKED = 8;
        public const int MF_UNCHECKED = 0;
        [DllImport("user32")]
        public static extern IntPtr GetMenu(IntPtr hWnd);
        [DllImport("user32")]
        public static extern int CheckMenuItem(IntPtr hmenu, int uIDCheckItem, int uCheck);

        public const int WM_CREATE = 1;
    }

    public class ClikeStringArray : IDisposable
    {
        IntPtr _nativeArray;
        List<IntPtr> _nativeItems;
        bool _disposed = false;

        public ClikeStringArray(int num, int stringCapacity)
        {
            _nativeArray = Marshal.AllocHGlobal((num + 1) * IntPtr.Size);
            _nativeItems = new List<IntPtr>();
            for (int i = 0; i < num; i++)
            {
                IntPtr item = Marshal.AllocHGlobal(stringCapacity);
                Marshal.WriteIntPtr((IntPtr)((int)_nativeArray + (i * IntPtr.Size)), item);
                _nativeItems.Add(item);
            }
            Marshal.WriteIntPtr((IntPtr)((int)_nativeArray + (num * IntPtr.Size)), IntPtr.Zero);
        }

        public IntPtr NativePointer { get { return _nativeArray; } }
        public List<string> ManagedStringsAnsi { get { return _getManagedItems(false); } }
        public List<string> ManagedStringsUnicode { get { return _getManagedItems(true); } }
        List<string> _getManagedItems(bool unicode)
        {
            List<string> _managedItems = new List<string>();
            for (int i = 0; i < _nativeItems.Count; i++)
            {
                if (unicode) _managedItems.Add(Marshal.PtrToStringUni(_nativeItems[i]));
                else _managedItems.Add(Marshal.PtrToStringAnsi(_nativeItems[i]));
            }
            return _managedItems;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                for (int i = 0; i < _nativeItems.Count; i++)
                    if (_nativeItems[i] != IntPtr.Zero) Marshal.FreeHGlobal(_nativeItems[i]);
                if (_nativeArray != IntPtr.Zero) Marshal.FreeHGlobal(_nativeArray);
                _disposed = true;
            }
        }
        ~ClikeStringArray()
        {
            Dispose();
        }
    }
    #endregion
}
