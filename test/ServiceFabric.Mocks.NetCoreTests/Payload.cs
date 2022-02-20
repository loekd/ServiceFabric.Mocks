using Microsoft.ServiceFabric.Data;
using System;
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
    public class ModifyablePayload : IEquatable<ModifyablePayload>, IComparable<ModifyablePayload>
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

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not ModifyablePayload payload) return false;

            return Equals(payload);
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }

        public bool Equals(ModifyablePayload other)
        {
            return string.Equals(other.Content, Content, StringComparison.Ordinal);
        }

        public int CompareTo(ModifyablePayload other)
        {
            if (other == null) return 1;
            return string.CompareOrdinal(other.Content, Content);
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