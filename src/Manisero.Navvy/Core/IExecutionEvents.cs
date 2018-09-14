namespace Manisero.Navvy.Core
{
    public interface IExecutionEvents
    {
    }

    public delegate void ExecutionEventHandler<TEventArgs>(TEventArgs e);
}
