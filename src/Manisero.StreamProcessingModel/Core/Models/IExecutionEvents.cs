namespace Manisero.StreamProcessingModel.Core.Models
{
    public interface IExecutionEvents
    {
    }

    public delegate void ExecutionEventHandler<TEventArgs>(TEventArgs e);
}
