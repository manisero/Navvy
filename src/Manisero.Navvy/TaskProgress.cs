namespace Manisero.Navvy
{
    public struct TaskProgress
    {
        public string StepName { get; }

        public byte ProgressPercentage { get; }

        public TaskProgress(
            string stepName, 
            byte progressPercentage)
        {
            StepName = stepName;
            ProgressPercentage = progressPercentage;
        }
    }
}
