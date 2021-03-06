using System;
using System.Text;

namespace AuthenticodeLint.Core.Asn
{

    public sealed class AsnBitString : AsnElement
    {
        public ArraySegment<byte> Value { get; }
        public override ArraySegment<byte> ContentData { get; }
        public override ArraySegment<byte> ElementData { get; }
        public int UnusedBits { get; }

        public AsnBitString(AsnTag tag, ArraySegment<byte> elementData, long? contentLength, int headerSize)
            : base(tag, headerSize)
        {
            if (tag.Constructed)
            {
                throw new AsnException("Constructed forms of BitString are not valid.");
            }
            if (contentLength == null)
            {
                throw new AsnException("Undefined lengths for BitString are not supported.");
            }
            if (contentLength == 0)
            {
                throw new AsnException("asn.1 BitString does not have enough data.");
            }
            ElementData = elementData.Constrain(contentLength.Value + headerSize);
            ContentData = elementData.Window(headerSize, contentLength.Value);
            UnusedBits = ContentData.At(0);
            Value = ContentData.Advance(1);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            const int BITS_IN_OCTET = 8;
            var wholeBytes = UnusedBits / BITS_IN_OCTET;
            var remainingBits = UnusedBits % BITS_IN_OCTET;
            for (var i = 0; i < Value.Count - wholeBytes; i++)
            {
                var b = Value.Array[Value.Offset + i];
                if (i <  Value.Count - wholeBytes - 1)
                {
                    builder.Append((b & 0x80) > 0 ? "1" : "0");
                    builder.Append((b & 0x40) > 0 ? "1" : "0");
                    builder.Append((b & 0x20) > 0 ? "1" : "0");
                    builder.Append((b & 0x10) > 0 ? "1" : "0");
                    builder.Append((b & 0x08) > 0 ? "1" : "0");
                    builder.Append((b & 0x04) > 0 ? "1" : "0");
                    builder.Append((b & 0x02) > 0 ? "1" : "0");
                    builder.Append((b & 0x01) > 0 ? "1" : "0");
                }
                else
                {
                    for (var j = 0; j < BITS_IN_OCTET - remainingBits; j++)
                    {
                        var mask = 0x80 >> j;
                        builder.Append((b & mask) > 0 ? "1" : "0");
                    }
                }
            }
            return builder.ToString();
        }
    }

}