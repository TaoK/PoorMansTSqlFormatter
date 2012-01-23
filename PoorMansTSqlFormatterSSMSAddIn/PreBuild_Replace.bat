
REM %1 is solution dir, %2 is project dir
%1\ExternalBuildTools\fart\fart.exe -r %2Properties\Settings.Designer.cs "global::System.Configuration.ApplicationSettingsBase {" "global::System.Configuration.ApplicationSettingsBase, PoorMansTSqlFormatterPluginShared.ISqlSettings {"

REM if errorlevel is 0 or greater, then we theoretically have a success (fart returns number of matches/replacements as return value), so reset errorlevel to 0
if ERRORLEVEL 1 CMD /C EXIT 0

REM ensure that we exit with the current errorlevel context...
exit %errorlevel%

