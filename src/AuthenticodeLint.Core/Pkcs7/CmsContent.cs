using System;
using System.Collections.Generic;
using AuthenticodeLint.Core.Asn;
using AuthenticodeLint.Core.x509;

namespace AuthenticodeLint.Core.Pkcs7
{
    public abstract class CmsContent
    {
        private readonly AsnElement _content;

        protected CmsContent(AsnElement content)
        {
            _content = content;
        }

        protected AsnElement Content => _content;
    }

    public sealed class CmsData : CmsContent
    {
        private ArraySegment<byte> _data;

        public CmsData(AsnElement content) : base(content)
        {
            _data = ((AsnOctetString)content).Value;
        }

        public ArraySegment<byte> Data => _data;
    }

    public sealed class CmsSignedData : CmsContent
    {
        private readonly AsnSequence _signedData;

        public int Version { get; }

        public IReadOnlyList<x509Certificate> Certificates { get; }

        public IReadOnlyList<CmsSignerInfo> SignerInfos { get; }

        public CmsContentInfo ContentInfo { get; }
        public IReadOnlyList<AlgorithmIdentifier> DigestAlgorithms { get; }

        public CmsSignedData(AsnElement content) : base(content)
        {
            _signedData = AsnReader.Read<AsnSequence>((AsnConstructed)content);
            var reader = new AsnConstructedReader(_signedData);
            var certs = new List<x509Certificate>();
            var signerInfos = new List<CmsSignerInfo>();
            if (!reader.MoveNext(out AsnInteger version) || version.Value > int.MaxValue)
            {
                throw new Pkcs7Exception("Invalid SignedData version.");
            }
            if (!reader.MoveNext(out AsnSet digestAlgorithms))
            {
                throw new Pkcs7Exception("Invalid digestAlgorithms.");
            }
            if (!reader.MoveNext(out AsnSequence contentInfo))
            {
                throw new Pkcs7Exception("Invalid contentInfo.");
            }

            while (reader.MoveNext(out AsnConstructed next))
            {
                if (next.Tag.IsExImTag(0)) //certificates
                {
                    var certReader = new AsnConstructedReader(next);
                    while (certReader.MoveNext(out AsnSequence cert))
                    {
                        certs.Add(new x509Certificate(cert));
                    }
                }
                else if (next.Tag.IsExImTag(1)) //crls
                {
                    continue;
                }
                else if (next.Tag.IsUniTag(AsnTagValue.SetSetOf)) //signerInfos
                {
                    var signerInfoReader = new AsnConstructedReader(next);
                    while (signerInfoReader.MoveNext(out AsnSequence signerInfo))
                    {
                        signerInfos.Add(new CmsSignerInfo(signerInfo));
                    }
                    break;
                }
                else
                {
                    throw new Pkcs7Exception("Unable to parse CmsContent.");
                }
            }
            SignerInfos = signerInfos;
            Certificates = certs;
            ContentInfo = new CmsContentInfo(contentInfo);
            var digestAlgsList = new List<AlgorithmIdentifier>();
            foreach (var alg in digestAlgorithms)
            {
                digestAlgsList.Add(new AlgorithmIdentifier((AsnSequence)alg));
            }
            DigestAlgorithms = digestAlgsList.AsReadOnly();
        }
    }
}