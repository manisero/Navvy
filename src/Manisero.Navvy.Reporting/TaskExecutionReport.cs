namespace Manisero.Navvy.Reporting
{
    public class TaskExecutionReport
    {
        public string Name { get; }
        public string Content { get; }

        public TaskExecutionReport(
            string name,
            string content)
        {
            Name = name;
            Content = content;
        }
    }
}
