namespace ReSharper.XUnitTestRunner
{
    using System;
    using System.IO;
    using System.Linq;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using Xunit;

    public class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit_hazzik";

        public XunitTaskRunner(IRemoteTaskServer server) : base(server)
        {
        }

        public override TaskResult Start(TaskExecutionNode node)
        {
            return TaskResult.Success;
        }

        public override TaskResult Execute(TaskExecutionNode node)
        {
            return TaskResult.Success;
        }

        public override TaskResult Finish(TaskExecutionNode node)
        {
            return TaskResult.Success;
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            var priorCurrentDirectory = Environment.CurrentDirectory;
            try
            {
                var assemblyTask = (XunitTestAssemblyTask) node.RemoteTask;

                var assemblyLocation = assemblyTask.AssemblyLocation;

                Environment.CurrentDirectory = Path.GetDirectoryName(assemblyLocation);

                var shadowCopy = Server.GetConfiguration().ShadowCopy;
                using (var executorWrapper = new ExecutorWrapper(assemblyLocation, null, shadowCopy))
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