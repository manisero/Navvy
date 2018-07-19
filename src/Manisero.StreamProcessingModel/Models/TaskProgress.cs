namespace Manisero.StreamProcessingModel.Models
{
    public struct TaskProgress
    {
        public string StepName { get; set; }
        public byte ProgressPercentage { get; set; }
    }
}
