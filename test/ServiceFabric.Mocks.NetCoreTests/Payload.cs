using System.Runtime.Serialization;

namespace ServiceFabric.Mocks.NetCoreTests
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