@echo off
del *.nupkg
nuget pack Plugin.HttpTransferTasks.nuspec
pause