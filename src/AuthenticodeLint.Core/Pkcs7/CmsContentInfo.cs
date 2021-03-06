using System;
using AuthenticodeLint.Core.Asn;

namespace AuthenticodeLint.Core.Pkcs7
{
    public sealed class CmsContentInfo
    {
        public Oid ContentType { get; }
        public AsnElement Content { get; }
        public ArraySegment<byte> CmsContentInfoData { get; }

        public CmsContentInfo(AsnSequence sequence)
        {
            CmsContentInfoData = sequence.ContentData;
            var reader = new AsnConstructedReader(sequence);
            if (!reader.MoveNext(out AsnObjectIdentifier contentType))
            {
                throw new Pkcs7Exception("Unable to read ContentType from ContentInfo.");
            }
            if (!reader.MoveNext(out AsnConstructed content))
            {
                content = null;
            }
            ContentType = contentType.Value;
            Content = AsnReader.Read<AsnElement>(content);
        }
    }
}