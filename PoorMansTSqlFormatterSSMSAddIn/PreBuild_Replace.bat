REM Preemptively inject interface name to settings class definition so that these settings can be referenced in other projects

REM %1 is solution dir, %2 is project dir
%1\ExternalBuildTools\fart\fart.exe -r %2Properties\Settings.Designer.cs "global::System.Configuration.ApplicationSettingsBase {" "global::System.Configuration.ApplicationSettingsBase, PoorMansTSqlFormatterPluginShared.ISqlSettings {"

REM standard "fart.exe" error-handling block; 9009 (missing program) is bad, anything else above 0 is OK and should be reset to 0 for standard handling
if ERRORLEVEL 9009 (
	REM do nothing
) else (
	if ERRORLEVEL 1 CMD /C EXIT 0
)

REM ensure that we exit with the current errorlevel context...
exit %errorlevel%

