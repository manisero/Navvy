using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Executors.StepExecutorResolvers;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Models.TaskSteps;
using Xunit;
using Xunit.Abstractions;

namespace Manisero.StreamProcessingModel.Samples
{
    public class task_execution
    {
        private readonly ITestOutputHelper _output;

        public task_execution(
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
            // Arrange
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
                                x =>
                                {
                                    sum += x;
                                    Task.Delay(100).Wait();
                                }),
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

            var progress = new Progress<TaskProgress>(x => _output.WriteLine($"{x.StepName}: {x.ProgressPercentage}%"));

            var executor = new TaskExecutor(taskStepExecutorResolver);

            // Act
            executor.Execute(taskDescription, progress);

            // Assert
            initialized.Should().Be(true);
            sum.Should().Be(21);
            completed.Should().Be(true);
        }
    }
}
