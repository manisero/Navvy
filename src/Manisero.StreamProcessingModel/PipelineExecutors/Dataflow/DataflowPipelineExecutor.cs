using System.Threading.Tasks.Dataflow;

namespace Manisero.StreamProcessingModel.PipelineExecutors.Dataflow
{
    public class DataflowPipelineExecutor : IPipelineExecutor
    {
        private readonly DataflowPipelineBuilder _dataflowPipelineBuilder = new DataflowPipelineBuilder();

        public void Execute<TInput>(Pipeline<TInput> pipeline)
        {
            var dataflowPipeline = _dataflowPipelineBuilder.Build(pipeline);

            ExecutePipeline(dataflowPipeline);
        }

        private void ExecutePipeline<TInput>(DataflowPipeline<TInput> pipeline)
        {
            foreach (var item in pipeline.Input)
            {
                pipeline.StartBlock.SendAsync(item).Wait();
            }

            pipeline.StartBlock.Complete();
            pipeline.Completion.Wait();
        }
    }
}
