::set fdir=%WINDIR%\Microsoft.NET\Framework64

::if not exist %fdir% (
::	set fdir=%WINDIR%\Microsoft.NET\Framework
::)

set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"

%msbuild% ../src/Raven.MongoDB.Repository/Raven.MongoDB.Repository/Raven.MongoDB.Repository.csproj /t:Clean;Rebuild /p:Configuration=Release
xcopy ..\src\Raven.MongoDB.Repository\Raven.MongoDB.Repository\bin\Release ..\output\Raven.MongoDB.Repository /i /e /y

pause