namespace Manisero.Navvy
{
    /// <summary>Use it to build <see cref="TaskDefinition"/> steps in a convenient way. Start with <see cref="Build"/> static property, then continue with extension methods (see <see cref="BasicProcessing.TaskStepBuilderUtils"/> or <see cref="PipelineProcessing.TaskStepBuilderUtils"/>).</summary>
    public class TaskStepBuilder
    {
        public static TaskStepBuilder Build { get; } = new TaskStepBuilder();

        private TaskStepBuilder()
        {
        }
    }
}
