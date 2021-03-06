using System;
using System.Text;

namespace AuthenticodeLint.Core.Asn
{

    public sealed class AsnBmpString : AsnElement, IDirectoryString
    {
        public string Value { get; }
        public override ArraySegment<byte> ContentData { get; }
        public override ArraySegment<byte> ElementData { get; }

        public AsnBmpString(AsnTag tag, ArraySegment<byte> elementData, long? contentLength, int headerSize)
            : base(tag, headerSize)
        {
            if (tag.Constructed)
            {
                throw new AsnException("Constructed forms of BmpString are not valid.");
            }
            if (contentLength == null)
            {
                throw new AsnException("Undefined lengths for BmpString are not supported.");
            }
            try
            {
                ElementData = elementData.Constrain(contentLength.Value + headerSize);
                ContentData = elementData.Window(headerSize, contentLength.Value);
                Value = AsnTextEncoding.BigEndianUnicode.GetString(ContentData.Array, ContentData.Offset, ContentData.Count);
            }
            catch (Exception e) when (e is ArgumentException || e is DecoderFallbackException)
            {
                throw new AsnException("asn.1 BmpString failed to decode.", e);
            }
        }

        public override string ToString() => Value;
    }

}