# Navvy
A long-running tasks execution framework.

## How to use

1. Install [Manisero.Navvy](https://www.nuget.org/packages/Manisero.Navvy/) NuGet package.

- Consider adding [Manisero.Navvy.Dataflow](https://www.nuget.org/packages/Manisero.Navvy.Dataflow/) package to use [TPL Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) for pipelines execution.

2. Define task to execute:

```C#
var input = Enumerable.Repeat(1, 10);
var sum = 0;

var summingTask = new TaskDefinition(
    new BasicTaskStep(
        name: "Initialize",
        body: () => Console.WriteLine("Summing task started.")),
    new PipelineTaskStep<int>(
        name: "Sum",
        input: input,
        expectedItemsCount: 10,
        blocks: new List<PipelineBlock<int>>
        {
            new PipelineBlock<int>(
                name: "Sum",
                body: x => sum += x),
            new PipelineBlock<int>(
                name: "Log",
                body: x => Console.WriteLine($"Added {x}."))
        }),
    new BasicTaskStep(
        name: "Complete",
        body: () => Console.WriteLine("Summing task finished.")));
```

3. Create task execution engine:

```C#
var executor = new TaskExecutorBuilder()
    .RegisterDataflowExecution() // Only if Manisero.Navvy.Dataflow package referenced
    .Build();
```

4. Execute the task:

```C#
executor.Execute(task);

Console.WriteLine($"Result: {sum}.");
```
