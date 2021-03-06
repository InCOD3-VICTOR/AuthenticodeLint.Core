using System;
using System.Globalization;
using System.Text;

namespace AuthenticodeLint.Core.Asn
{
    public sealed class AsnGeneralizedTime : AsnElement, IAsnDateTime
    {
        public DateTimeOffset Value { get; }
        public override ArraySegment<byte> ContentData { get; }
        public override ArraySegment<byte> ElementData { get; }

        public AsnGeneralizedTime(AsnTag tag, ArraySegment<byte> elementData, long? contentLength, int headerSize)
            : base(tag, headerSize)
        {
            if (tag.Constructed)
            {
                throw new AsnException("Constructed forms of GeneralizeTime are not valid.");
            }
            if (contentLength == null)
            {
                throw new AsnException("Undefined lengths for BitString are not supported.");
            }
            string strData;
            try
            {
                ElementData = elementData.Constrain(contentLength.Value + headerSize);
                ContentData = elementData.Window(headerSize, contentLength.Value);
                strData = Encoding.ASCII.GetString(ContentData.Array, ContentData.Offset, ContentData.Count);
            }
            catch (Exception e) when (e is DecoderFallbackException || e is ArgumentException)
            {
                throw new AsnException("asn.1 encoded GeneralizedTime could not be decoded into a string.", e);
            }
            var formats = new string[] {
                //Valid local times
                "yyyyMMddHH",
                "yyyyMMddHHmm",
                "yyyyMMddHHmmss",
                "yyyyMMddHHmmss.fff",

                //Valid UTC times
                "yyyyMMddHHZ",
                "yyyyMMddHHmmZ",
                "yyyyMMddHHmmssZ",
                "yyyyMMddHHmmss.fffZ",

                //Valid offset times
                "yyyyMMddHHzzz",
                "yyyyMMddHHmmzzz",
                "yyyyMMddHHmmsszzz",
                "yyyyMMddHHmmss.fffzzz",
            };
            if (!DateTimeOffset.TryParseExact(strData, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var val))
            {
                throw new AsnException("Encoded GeneralizedTime is not valid.");
            }
            Value = val;
        }

        public override string ToString() => Value.ToString();
    }
}
