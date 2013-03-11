namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ReSharper.TaskRunnerFramework;
    using XUnitTestRunner;

    public partial class XunitTestProvider
    {
        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }
    }
}