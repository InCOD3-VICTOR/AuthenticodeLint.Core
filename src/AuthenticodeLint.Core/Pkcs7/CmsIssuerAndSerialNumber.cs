using System;
using AuthenticodeLint.Core.Asn;
using AuthenticodeLint.Core.x509;

namespace AuthenticodeLint.Core.Pkcs7
{
    public sealed class CmsIssuerAndSerialNumber
    {
        public x500DistinguishedName Name { get; }
        public ArraySegment<byte> SerialNumber { get; }

        public CmsIssuerAndSerialNumber(AsnSequence sequence)
        {
            var (asnName, asnSerial) = AsnReader.Read<AsnSequence, AsnInteger>(sequence);
            Name = new x500DistinguishedName(asnName);
            SerialNumber = asnSerial.ContentData;
        }

        public CmsIssuerAndSerialNumber(x500DistinguishedName name, ArraySegment<byte> serialNumber)
        {
            Name = name;
            SerialNumber = serialNumber;
        }

        public override string ToString() => $"Name: {{{Name}}}; SerialNumber: {{{SerialNumber.Join()}}};";
    }
}