using System;
using System.Text;

namespace AuthenticodeLint.Core.Asn
{
    public sealed class AsnIA5String : AsnElement, IDirectoryString
    {
        public string Value { get; }

        public AsnIA5String(AsnTag tag, ArraySegment<byte> contentData) : base(tag, contentData)
        {
            Value = Encoding.ASCII.GetString(contentData.Array, contentData.Offset, contentData.Count);
        }

        public override string ToString() => Value;
    }
}