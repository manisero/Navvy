using System.Collections.Generic;
using FluentAssertions;
using Manisero.StreamProcessingModel.TaskExecution.Executors;
using Manisero.StreamProcessingModel.TaskExecution.Executors.StepExecutorResolvers;
using Manisero.StreamProcessingModel.TaskExecution.Models;
using Manisero.StreamProcessingModel.TaskExecution.Models.TaskSteps;
using Xunit;
using Xunit.Abstractions;

namespace Manisero.StreamProcessingModel.Samples.TaskExecution
{
    public class Streaming
    {
        private readonly ITestOutputHelper _output;

        public Streaming(
            ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void sequential()
        {
            test(new SequentialTaskExecutorResolver());
        }

        [Fact]
        public void streaming()
        {
            test(new StreamingTaskExecutorResolver());
        }
        
        private void test(ITaskStepExecutorResolver taskStepExecutorResolver)
        {
            var initialized = false;
            var sum = 0;
            var completed = false;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep
                    {
                        Name = "Initialize",
                        Body = () => { initialized = true; }
                    },
                    new PipelineTaskStep<int>(
                        "Pipeline",
                        new[]
                        {
                            new[] { 1, 2, 3 },
                            new[] { 4, 5, 6 }
                        },
                        new List<PipelineBlock<int>>
                        {
                            new PipelineBlock<int>(
                                "Sum",
                                x => sum += x),
                            new PipelineBlock<int>(
                                "Log",
                                x => _output.WriteLine(x.ToString()))
                        }),
                    new BasicTaskStep
                    {
                        Name = "Complete",
                        Body = () => { completed = true; }
                    }
                }
            };

            var executor = new TaskExecutor(taskStepExecutorResolver);

            executor.Execute(taskDescription);

            initialized.Should().Be(true);
            sum.Should().Be(21);
            completed.Should().Be(true);
        }
    }
}
