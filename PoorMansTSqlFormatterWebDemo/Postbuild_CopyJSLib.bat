REM Copy compiled JS libraries from another project

copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\bridge.min.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END
copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\bridge.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END
copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\bridge.meta.min.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END
copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\bridge.meta.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END
copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\PoorMansTSqlFormatterJS.min.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END
copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\PoorMansTSqlFormatterJS.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END
copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\PoorMansTSqlFormatterJS.meta.min.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END
copy %1PoorMansTSqlFormatterJSLib\bin\%3\bridge\PoorMansTSqlFormatterJS.meta.js %2JSLibReference\
IF %ERRORLEVEL% NEQ 0 GOTO END

:END