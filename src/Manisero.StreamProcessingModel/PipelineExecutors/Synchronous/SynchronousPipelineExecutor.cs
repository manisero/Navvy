namespace Manisero.StreamProcessingModel.PipelineExecutors.Synchronous
{
    public class SynchronousPipelineExecutor : IPipelineExecutor
    {
        public void Execute<TInput>(Pipeline<TInput> pipeline)
        {
            foreach (var item in pipeline.Input)
            {
                object currentOutput = item;

                foreach (var block in pipeline.Blocks)
                {
                    currentOutput = block.Body(currentOutput);
                }
            }
        }
    }
}
