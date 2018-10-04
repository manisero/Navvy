namespace Manisero.Navvy.Logging
{
    public static class TaskExecutionLoggingUtils
    {
        public const string TaskExecutionLogExtraKey = "ExecutionLog";

        internal static void SetExecutionLog(
            this TaskDefinition task,
            TaskExecutionLog log)
            => task.Extras[TaskExecutionLogExtraKey] = log;

        public static TaskExecutionLog GetExecutionLog(
            this TaskDefinition task)
            => (TaskExecutionLog)task.Extras[TaskExecutionLogExtraKey];

        public static ITaskExecutorBuilder RegisterTaskExecutionLogger(
            this ITaskExecutorBuilder builder)
            => builder.RegisterEvents(TaskExecutionLogger.CreateEvents());
    }
}
