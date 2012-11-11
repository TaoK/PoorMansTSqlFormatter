REM Merge third-party and satellite (translation) assemblies into a single new executable assembly for easier distribution

IF EXIST %1SqlFormatterWinforms.exe DEL %1SqlFormatterWinforms.exe

"..\..\..\ExternalBuildTools\ILRepack\ILRepack.exe" /t:winexe /out:%1SqlFormatterWinformsTemp.exe %1PoorMansTSqlFormatterDemo.exe %1PoorMansTSqlFormatterLib.dll %1LinqBridge.dll %1es\PoorMansTSqlFormatterDemo.resources.dll
IF %ERRORLEVEL% NEQ 0 GOTO END

"..\..\..\ExternalBuildTools\ILRepack\ILRepack.exe" /t:winexe /out:%1SqlFormatterWinforms.exe %1SqlFormatterWinformsTemp.exe %1fr\PoorMansTSqlFormatterDemo.resources.dll
IF %ERRORLEVEL% NEQ 0 GOTO END

del %1LinqBridge.dll
del %1PoorMansTSqlFormatterLib.dll
del %1PoorMansTSqlFormatterLib.pdb
del %1PoorMansTSqlFormatterDemo.exe
del %1PoorMansTSqlFormatterDemo.pdb
del %1SqlFormatterWinformsTemp.exe
del %1SqlFormatterWinformsTemp.pdb
del %1es\*.* /Q
del %1fr\*.* /Q
:END