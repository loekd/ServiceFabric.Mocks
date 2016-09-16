using System.Runtime.Serialization;

namespace ServiceFabric.Mocks.Tests
{
    [DataContract]
    public class Payload
    {
        [DataMember]
        public readonly string Content;

        public Payload(string content)
        {
            Content = content;
        }
    }
}