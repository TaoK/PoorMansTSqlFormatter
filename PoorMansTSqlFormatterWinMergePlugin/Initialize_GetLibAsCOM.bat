if exist "bin" goto copydlls
mkdir bin
:copydlls
cd bin
copy ..\..\PoorMansTSqlFormatterLib\bin\Release\*.dll .
c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe PoorMansTSqlFormatterLib.dll /codebase /tlb:PoorMansTSqlFormatterLib.tlb
pause