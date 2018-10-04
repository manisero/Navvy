namespace Manisero.Navvy.Logging
{
    public static class TaskExecutionLoggingUtils
    {
        public const string TaskExecutionLogExtraKey = "ExecutionLog";

        internal static void SetExecutionLog(
            this TaskDefinition task,
            TaskExecutionLog log)
            => task.Extras.Set(TaskExecutionLogExtraKey, log);

        public static TaskExecutionLog GetExecutionLog(
            this TaskDefinition task)
            => task.Extras.Get<TaskExecutionLog>(TaskExecutionLogExtraKey);

        public static ITaskExecutorBuilder RegisterTaskExecutionLogger(
            this ITaskExecutorBuilder builder)
            => builder.RegisterEvents(TaskExecutionLogger.CreateEvents());
    }
}
