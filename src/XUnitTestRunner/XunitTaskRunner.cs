namespace ReSharper.XUnitTestRunner
{
using System;
using System.IO;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

    public partial class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit_hazzik";

        public XunitTaskRunner(IRemoteTaskServer server) 
            : base(server)
        {
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            var priorCurrentDirectory = Environment.CurrentDirectory;
            try
            {
                var assemblyTask = (XunitTestAssemblyTask) node.RemoteTask;
                var assemblyLocation = assemblyTask.AssemblyLocation;
                var assemblyFolder = Path.GetDirectoryName(assemblyLocation);
                var assemblyPath = Path.Combine(assemblyFolder, Path.GetFileName(assemblyLocation));
                var config = assemblyPath + ".config";
                Environment.CurrentDirectory = assemblyFolder;

                var shadowCopy = TaskExecutor.Configuration != null && TaskExecutor.Configuration.ShadowCopy;
                using (var executorWrapper = new ExecutorWrapper(assemblyLocation, config, shadowCopy))
                {
                    foreach (var childNode in node.Children)
                    {
                        var classTask = (XunitTestClassTask) childNode.RemoteTask;

                        var runnerLogger = new ReSharperRunnerLogger(Server, classTask);
                        runnerLogger.ClassStart();

                        var tasks = childNode.Children
                                             .Select(methodNode => (XunitTestMethodTask) methodNode.RemoteTask)
                                             .ToList();

                        runnerLogger.SetMethodTasks(tasks);

                        var methodNames = tasks
                            .Select(methodTask => methodTask.ShortName)
                            .ToList();

                        new TestRunner(executorWrapper, runnerLogger).RunTests(classTask.TypeName, methodNames);

                        runnerLogger.ClassFinished();
                    }
                }
            }
            finally
            {
                Environment.CurrentDirectory = priorCurrentDirectory;
            }
        }
    }
}
