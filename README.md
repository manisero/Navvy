# Navvy
A .NET long-running tasks execution framework.

## How to use

1. Install [Manisero.Navvy](https://www.nuget.org/packages/Manisero.Navvy/) NuGet package.

- Consider adding [Manisero.Navvy.Dataflow](https://www.nuget.org/packages/Manisero.Navvy.Dataflow/) package in order to use [TPL Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) for pipelines execution.

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
executor.Execute(summingTask);

Console.WriteLine($"Result: {sum}.");
```

For a more detailed sample, see [sample application](https://github.com/manisero/Navvy.SampleApp) demonstrating usage of Navvy.

## Features

- Built-in progress reporting.
- Cancellation support.
- Detailed execution reporting:
  - Task-, Step- and Block- -Started / -Ended events,
  - -Ended events contain duration of the task / step / block.
- Parallel processing support (Dataflow only).
- Task execution errors handling.
- Conditional steps execution.
- Extensibility - new types of steps and new ways to execute them can be defined and registered.
- Built on .NET Standard 2.0.

## More

You are encouraged to play around with Navvy. If you do so, please give feedback! Write an email (you'll find the address [here](https://github.com/manisero)) or create an [issue](https://github.com/manisero/Navvy/issues).
