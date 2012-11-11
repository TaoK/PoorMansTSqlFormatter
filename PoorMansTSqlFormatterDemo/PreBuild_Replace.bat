REM Preemptively replace class names that are auto-set by Visual Studio designer, where we want to use a custom version
REM - in this case the Resource Manager that will need to find our translations in the merged output assembly

REM %1 is solution dir, %2 is project dir
%1\ExternalBuildTools\fart\fart.exe -r %2*.cs "System.ComponentModel.ComponentResourceManager" "FrameworkClassReplacements.SingleAssemblyComponentResourceManager"

REM standard "fart.exe" error-handling block; 9009 (missing program) is bad, anything else above 0 is OK and should be reset to 0 for standard handling
if ERRORLEVEL 9009 (
	REM do nothing
) else (
	if ERRORLEVEL 1 CMD /C EXIT 0
)

REM ensure that we exit with the current errorlevel context...
exit %errorlevel%

