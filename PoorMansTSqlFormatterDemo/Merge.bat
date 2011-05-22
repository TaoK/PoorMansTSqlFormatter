"c:\Program Files\Microsoft\ILMerge\ILMerge.exe" /t:winexe /out:%1SqlFormatterWinforms.exe %1PoorMansTSqlFormatterDemo.exe %1PoorMansTSqlFormatterLib.dll 
del %1PoorMansTSqlFormatterLib.dll
del %1PoorMansTSqlFormatterLib.pdb
del %1PoorMansTSqlFormatterDemo.exe
del %1PoorMansTSqlFormatterDemo.pdb
