using System;
using Manisero.StreamProcessingModel.TaskExecution.Models;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors
{
    public class TaskExecutor
    {
        private readonly ITaskStepExecutorResolver _taskStepExecutorResolver;

        public TaskExecutor(
            ITaskStepExecutorResolver taskStepExecutorResolver)
        {
            _taskStepExecutorResolver = taskStepExecutorResolver;
        }

        public void Execute(TaskDescription taskDescription)
        {
            foreach (var step in taskDescription.Steps)
            {
                throw new NotImplementedException();
            }
        }
    }
}
