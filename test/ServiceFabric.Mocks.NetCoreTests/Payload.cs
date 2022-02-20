using Microsoft.ServiceFabric.Data;
using System.IO;
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


    [DataContract]
    public class ModifyablePayload
    {
        [DataMember]
        public string Content { get; set; }

        public ModifyablePayload(string content)
        {
            Content = content;
        }
        public ModifyablePayload()
        {
        }
    }

    public class ModifyablePayloadSerializer : IStateSerializer<ModifyablePayload>
    {
        public ModifyablePayload Read(BinaryReader binaryReader)
        {
            return Read(new ModifyablePayload(), binaryReader);
        }

        public ModifyablePayload Read(ModifyablePayload baseValue, BinaryReader binaryReader)
        {
            baseValue.Content = binaryReader.ReadString();
            return baseValue;
        }

        public void Write(ModifyablePayload value, BinaryWriter binaryWriter)
        {
            Write(null, value, binaryWriter);
        }

        public void Write(ModifyablePayload baseValue, ModifyablePayload targetValue, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(targetValue.Content);
        }
    }
}