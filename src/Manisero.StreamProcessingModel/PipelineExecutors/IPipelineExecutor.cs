namespace Manisero.StreamProcessingModel.PipelineExecutors
{
    public interface IPipelineExecutor
    {
        void Execute<TInput>(Pipeline<TInput> pipeline);
    }
}
