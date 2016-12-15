using System;
using AuthenticodeLint.Core.Asn;

namespace AuthenticodeLint.Core.Pkcs7
{
    public sealed class CmsOpusAttribute : CmsGenericAttribute
    {
        public CmsOpusAttribute(string attributeId, AsnSet content) : base(attributeId, content)
        {
            var items = AsnReader.Read<AsnSequence>(content);
            AsnConstructed programName = null, moreInfo = null;
            var reader = new AsnConstructedReader(items.Item1);
            AsnConstructed next;
            while (reader.MoveNext(out next))
            {
                if (next.Tag.IsExImTag(0))
                {
                    programName = next;
                }
                else if (next.Tag.IsExImTag(1))
                {
                    moreInfo = next;
                }
            }
            if (programName != null)
            {
                var program = AsnReader.Read<AsnElement>(programName);
                ProgramName = DecodeSpcString(program.Item1);
            }

            if (moreInfo != null)
            {
                var more = AsnReader.Read<AsnElement>(moreInfo);
                if (more.Item1.Tag.IsExImTag(0))
                {
                    var moreString = more.Item1.Reinterpret<AsnIA5String>();
                    MoreInfo = new SpcMoreInfoString(moreString.Value, InfoType.Url);
                }
                else if (more.Item1.Tag.IsExImTag(1))
                {
                    var serializedSeq = more.Item1.Reinterpret<AsnSequence>();
                    var data = AsnReader.Read<AsnOctetString, AsnOctetString>(serializedSeq);
                    MoreInfo = new SpcMoreInfoBinary(new Guid(data.Item1.Value.AsArray()), data.Item2.Value);
                }
                else if (more.Item1.Tag.IsExImTag(2))
                {
                    var spcString = AsnReader.Read<AsnElement>((AsnConstructed)more.Item1);
                    var moreString = DecodeSpcString(spcString.Item1);
                    MoreInfo = new SpcMoreInfoString(moreString, InfoType.File);
                }
                else
                {
                    throw new Pkcs7Exception("Unable to decode MoreInfo data.");
                }
            }
        }

        private static string DecodeSpcString(AsnElement element)
        {
            if (element.Tag.IsExImTag(0)) //0 is an implicit tag for a BmpString
            {
                var bmpProgramString = element.Reinterpret<AsnBmpString>();
                return bmpProgramString.Value;
            }
            else if (element.Tag.IsExImTag(1)) //1 is an implicit tag for a IA5String
            {
                var ia5ProgramString = element.Reinterpret<AsnIA5String>();
                return ia5ProgramString.Value;
            }
            else
            {
                throw new Pkcs7Exception("Unable to decode SpcString data.");
            }
        }

        public string ProgramName { get; }
        public SpcMoreInfo MoreInfo { get; }
    }

    public abstract class SpcMoreInfo
    {
        public InfoType MoreInfoType { get; }

        protected SpcMoreInfo(InfoType moreInfoType)
        {
            MoreInfoType = moreInfoType;
        }
    }

    public sealed class SpcMoreInfoString : SpcMoreInfo
    {
        public string Value { get; }

        public SpcMoreInfoString(string value, InfoType moreInfoType) : base(moreInfoType)
        {
            Value = value;
        }
    }

    public sealed class SpcMoreInfoBinary : SpcMoreInfo
    {
        public Guid Uuid { get; }
        public ArraySegment<byte> SerializedData { get; }

        public SpcMoreInfoBinary(Guid uuid, ArraySegment<byte> serializedData) : base(InfoType.Moniker)
        {
            Uuid = uuid;
            SerializedData = serializedData;
        }
    }

        public enum InfoType
        {
            Url,
            File,
            Moniker,
        }
}