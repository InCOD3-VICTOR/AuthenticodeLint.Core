using System;
using System.Globalization;
using System.Text;

namespace AuthenticodeLint.Core.Asn
{
    public class AsnUtcTime : AsnElement, IAsnDateTime
    {
        public DateTimeOffset Value { get; }

        public AsnUtcTime(AsnTag tag, ArraySegment<byte> contentData) : base(tag, contentData)
        {
            var strData = Encoding.ASCII.GetString(contentData.Array, contentData.Offset, contentData.Count);
            var formats = new string[] {
                "yyMMddHHmmsszzz",
                "yyMMddHHmmzzz",
                "yyMMddHHmmZ",
                "yyMMddHHmmssZ",
            };
            DateTimeOffset val;
            if (!DateTimeOffset.TryParseExact(strData, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out val))
            {
                throw new InvalidOperationException("Encoded UTCDate is not valid.");
            }
            Value = val;
        }

        public override string ToString() => Value.ToString();
    }
}