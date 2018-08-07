using System;

namespace Manisero.StreamProcessingModel.Models
{
    public class TaskExecutionException : Exception
    {
        public TaskExecutionException(Exception innerException) 
            : base("Error while executing task. See inner exception.", innerException)
        {
        }
    }
}
