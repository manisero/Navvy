using FluentAssertions;
using Manisero.StreamProcessingModel.PipelineExecutors.Synchronous;
using Xunit;
using Xunit.Abstractions;

namespace Manisero.StreamProcessingModel.Samples
{
    public class Synchronous
    {
        private readonly ITestOutputHelper _output;

        public Synchronous(
            ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void sample()
        {
            var sum = 0;

            var pipeline = new Pipeline<string>("Test", new[] { "1", "2", "3" });
            pipeline.AddBlock(Block.CreateActionBlock<string>("Report", _output.WriteLine));
            pipeline.AddBlock(Block.CreateFunctionBlock<string, int>("Parse", int.Parse));
            pipeline.AddBlock(Block.CreateActionBlock<int>("UpdateSum", x => sum += x));
            
            var executor = new SynchronousPipelineExecutor();

            executor.Execute(pipeline);

            sum.Should().Be(6);
        }
    }
}
