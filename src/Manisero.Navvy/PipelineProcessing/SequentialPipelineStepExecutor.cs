using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing.Events;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.PipelineProcessing
{
    internal class SequentialPipelineStepExecutor<TItem> : ITaskStepExecutor<PipelineTaskStep<TItem>>
    {
        public async Task Execute(
            PipelineTaskStep<TItem> step,
            TaskStepExecutionContext context,
            CancellationToken cancellation)
        {
            var items = step.Input.ItemsFactory();
            var itemNumber = 0;
            var taskEvents = context.EventsBag.TryGetEvents<TaskExecutionEvents>();
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();
            var itemSw = new Stopwatch();
            var blockSw = new Stopwatch();
            var totalInputMaterializationDuration = TimeSpan.Zero;
            var totalBlockDurations = step.Blocks.Select(x => x.Name).Distinct().ToDictionary(x => x, _ => TimeSpan.Zero);

            using (var inputEnumerator = items.Items.GetEnumerator())
            {
                while (true)
                {
                    itemNumber++;

                    var itemStartTs = DateTimeOffset.UtcNow;
                    itemSw.Restart();
                    
                    bool hasNextItem;

                    try
                    {
                        hasNextItem = inputEnumerator.MoveNext();
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, step.GetInputExceptionData(itemNumber));
                    }

                    if (!hasNextItem)
                    {
                        itemSw.Stop();
                        break;
                    }
                    
                    var item = inputEnumerator.Current;
                    var materializationDuration = itemSw.Elapsed;
                    totalInputMaterializationDuration += materializationDuration;
                    events?.Raise(x => x.OnItemMaterialized(itemNumber, item, itemStartTs, materializationDuration, step, context.Task));

                    foreach (var block in step.Blocks)
                    {
                        events?.Raise(x => x.OnBlockStarted(block, itemNumber, item, step, context.Task));
                        blockSw.Restart();

                        try
                        {
                            await block.Body(item);
                        }
                        catch (Exception e)
                        {
                            throw new TaskExecutionException(e, step, block.GetExceptionData(itemNumber));
                        }

                        blockSw.Stop();
                        totalBlockDurations[block.Name] += blockSw.Elapsed;
                        events?.Raise(x => x.OnBlockEnded(block, itemNumber, item, step, context.Task, blockSw.Elapsed));
                        cancellation.ThrowIfCancellationRequested();
                    }

                    itemSw.Stop();
                    events?.Raise(x => x.OnItemEnded(itemNumber, item, step, context.Task, itemSw.Elapsed));
                    taskEvents?.Raise(x => x.OnStepProgressed(itemNumber, items.ExpectedCount, itemSw.Elapsed, step, context.Task));
                }
            }

            events?.Raise(x => x.OnPipelineEnded(totalInputMaterializationDuration, totalBlockDurations, step, context.Task));
        }
    }
}
