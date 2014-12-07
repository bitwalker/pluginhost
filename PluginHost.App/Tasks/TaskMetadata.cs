namespace PluginHost.App.Tasks
{
    public class TaskMetadata
    {
        public string Name { get; set; }

        public TaskMetadata() {}
        public TaskMetadata(string name)
        {
            Name = name;
        }
    }
}
