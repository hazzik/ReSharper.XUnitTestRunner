@echo off

set R=%ProgramFiles%\JetBrains\ReSharper\v6.1\SDK\Bin
if not exist "%R%" set R=%ProgramFiles(x86)%\JetBrains\ReSharper\v6.1\SDK\Bin
if not exist "%R%" set R=%ProgramFiles%\JetBrains\Resharper\v6.1\bin
if not exist "%R%" set R=%ProgramFiles(x86)%\JetBrains\Resharper\v6.1\bin
if not exist "%R%" goto End


mkdir ReSharper
cd ReSharper
copy "%R%\JetBrains.Annotations.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.DocumentManager.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.IDE.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.MetaData.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.ProjectModel.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.Shell.???" > nul
copy "%R%\JetBrains.Platform.Resharper.UI.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.Util.???" > nul
copy "%R%\JetBrains.ReSharper.Daemon.???" > nul
copy "%R%\JetBrains.ReSharper.Features.Common.???" > nul
copy "%R%\JetBrains.ReSharper.Feature.Services.???" > nul
copy "%R%\JetBrains.ReSharper.Psi.???" > nul
copy "%R%\JetBrains.ReSharper.TaskRunnerFramework.???" > nul
copy "%R%\JetBrains.ReSharper.UnitTestExplorer.???" > nul
copy "%R%\JetBrains.ReSharper.UnitTestFramework.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.ComponentModel.???" > nul
copy "%R%\JetBrains.Platform.ReSharper.DocumentModel.???" > nul
copy "%R%\JetBrains.ReSharper.Resources.???" > nul

cd ..
echo Support for ReSharper 6.1 successfully copied
echo   from %R%

:End
pause