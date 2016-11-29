using System;

namespace AuthenticodeLint.Core.Asn
{
    public sealed class AsnNull : AsnElement
    {
        public AsnNull(AsnTag tag, ArraySegment<byte> contentData, ArraySegment<byte> elementData)
            : base(tag, contentData, elementData)
        {
            if (tag.Constructed)
            {
                throw new AsnException("Constructed forms of NULL are not valid.");
            }
            if (contentData.Count > 0)
            {
                throw new AsnException("Null data cannot have a length.");
            }
        }

        public override string ToString() => "Null";
    }
}