namespace PluginHost.App.Tasks
{
    public interface ITaskMetadata
    {
        string TaskName { get; }
    }

    public class TaskMetadata : ITaskMetadata
    {
        public const string MetadataKey = "TaskName";

        public string TaskName { get; set; }

        public TaskMetadata() {}
        public TaskMetadata(string name)
        {
            TaskName = name;
        }
    }
}
