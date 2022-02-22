using Microsoft.ServiceFabric.Data;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;

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
    public record class ModifyablePayload : IComparable<ModifyablePayload>
    {
        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string OtherContent { get; set; }

        public ModifyablePayload(string content)
        {
            Content = content;
        }
        public ModifyablePayload()
        {
        }

        public int CompareTo(ModifyablePayload other)
        {
            if (other == null) return 1;
            return string.Compare(Content + OtherContent, other.Content + other.OtherContent);
        }
    }

    public class ModifyablePayloadSerializer : IStateSerializer<ModifyablePayload>
    {
        private const string _nullValue = "<<null>>";

        public ModifyablePayload Read(BinaryReader binaryReader)
        {
            return Read(new ModifyablePayload(), binaryReader);
        }

        public ModifyablePayload Read(ModifyablePayload baseValue, BinaryReader binaryReader)
        {
            baseValue.Content = binaryReader.ReadString();
            baseValue.OtherContent = binaryReader.ReadString();
            if (string.Equals(baseValue.Content, _nullValue, StringComparison.Ordinal))
            {
                baseValue.Content = null;
            }
            if (string.Equals(baseValue.OtherContent, _nullValue, StringComparison.Ordinal))
            {
                baseValue.OtherContent = null;
            }
            return baseValue;
        }

        public void Write(ModifyablePayload value, BinaryWriter binaryWriter)
        {
            Write(null, value, binaryWriter);
        }

        public void Write(ModifyablePayload baseValue, ModifyablePayload targetValue, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(targetValue.Content ?? _nullValue);
            binaryWriter.Write(targetValue.OtherContent ?? _nullValue);
        }
    }
}