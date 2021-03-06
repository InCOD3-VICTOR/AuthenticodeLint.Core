using AuthenticodeLint.Core.Asn;
using AuthenticodeLint.Core.x509;

namespace AuthenticodeLint.Core.Pkcs7
{
    public sealed class CmsSignerInfo
    {
        private readonly AsnSequence _sequence;

        public CmsSignerInfo(AsnSequence sequence)
        {
            _sequence = sequence;
            var reader = new AsnConstructedReader(sequence);
            AsnSequence encryptionAlgorithm = null;
            AsnOctetString encryptedDigest = null;
            AsnConstructed authAttributes = null, unauthAttributes = null;
            var hasEncryptionAlgorithm = false;
            if (!reader.MoveNext(out AsnInteger version))
            {
                throw new Pkcs7Exception("Unable to read SignerInfo version.");
            }
            if (!reader.MoveNext(out AsnSequence issuerAndSerial))
            {
                throw new Pkcs7Exception("Unable to read SignerInfo issuerAndSerialNumber.");
            }
            if (!reader.MoveNext(out AsnSequence digestAlgorithm))
            {
                throw new Pkcs7Exception("Unable to read digest algorithm identifier.");
            }
            while (reader.MoveNext(out AsnElement next))
            {
                if (next.Tag.IsExImTag(0)) //authenticatedAttributes
                {
                    authAttributes = (AsnConstructed)next;
                }
                else if (next.Tag.IsExImTag(1)) //unauthenticatedAttributes
                {
                    unauthAttributes = (AsnConstructed)next;
                }
                else if (!hasEncryptionAlgorithm)
                {
                    encryptionAlgorithm = (AsnSequence)next;
                    hasEncryptionAlgorithm = true;
                }
                else
                {
                    encryptedDigest = (AsnOctetString)next;
                }
            }
            if (encryptionAlgorithm == null)
            {
                throw new Pkcs7Exception("Unable to read encryption algorithm identifier.");
            }
            Version = (int)version.Value;
            IssuerAndSerialNumber = new CmsIssuerAndSerialNumber(issuerAndSerial);
            DigestAlgorithm = new AlgorithmIdentifier(digestAlgorithm);
            AuthenticatedAttributes = authAttributes == null ? new CmsAttributes() : new CmsAttributes(authAttributes);
            UnauthenticatedAttributes = unauthAttributes == null ? new CmsAttributes() : new CmsAttributes(unauthAttributes);
            EncryptionAlgorithm = new AlgorithmIdentifier(encryptionAlgorithm);
            EncryptedDigest = encryptedDigest;
        }

        public AlgorithmIdentifier DigestAlgorithm { get; }
        public AlgorithmIdentifier EncryptionAlgorithm { get; }
        public int Version { get; }
        public CmsIssuerAndSerialNumber IssuerAndSerialNumber { get; }
        public CmsAttributes AuthenticatedAttributes { get; }
        public CmsAttributes UnauthenticatedAttributes { get; }
        public AsnOctetString EncryptedDigest { get; }
    }
}