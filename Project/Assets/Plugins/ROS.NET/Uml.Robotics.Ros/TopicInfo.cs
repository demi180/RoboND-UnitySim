namespace Uml.Robotics.Ros
{
    public class TopicInfo
    {
        public TopicInfo(string name, string dataType)
        {
            this.Name = name;
            this.DataType = dataType;
        }

        public string DataType { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            return $"Name: '{this.Name}'; DataType: {this.DataType}";
        }
    }
}
