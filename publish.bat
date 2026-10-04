@echo off
echo ==============================================
echo  Building MyAutoSuperFastClicker Standalone EXE
echo ==============================================

dotnet publish MyAutoSuperFastClicker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\dist\standalone

echo.
echo ==============================================
echo  Done! Standalone EXE is ready at:
echo  .\dist\standalone\MyAutoSuperFastClicker.exe
echo ==============================================
pause
