namespace ReSharper.XUnitTestRunner
{
    using JetBrains.ReSharper.TaskRunnerFramework;

    public partial class XunitTaskRunner
    {
#pragma warning disable 672
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
#pragma warning restore 672
    }
}