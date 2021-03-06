using System;
using AuthenticodeLint.Core.Asn;

namespace AuthenticodeLint.Core.x509
{

    public sealed class AlgorithmIdentifier : IEquatable<AlgorithmIdentifier>
    {
        public AlgorithmIdentifier(AsnSequence sequence)
        {
            RawData = sequence.ElementData;
            var reader = new AsnConstructedReader(sequence);
            if (!reader.MoveNext(out AsnObjectIdentifier algorithm))
            {
                throw new x509Exception("Unable to read algorithm from sequence.");
            }
            Algorithm = algorithm.Value;
            if (reader.MoveNext(out AsnElement parameters) && !(parameters is AsnNull))
            {
                Parameters = parameters.ElementData;
            }
            else
            {
                Parameters = null;
            }
        }

        public ArraySegment<byte> RawData { get; }
        public Oid Algorithm { get; }
        public ArraySegment<byte>? Parameters { get; }

        public override string ToString() => Algorithm.Value;

        public bool Equals(AlgorithmIdentifier other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.RawData.Compare(other.RawData) == 0;
        }

        public override bool Equals(object obj) => Equals(obj as AlgorithmIdentifier);

        public static bool operator ==(AlgorithmIdentifier left, AlgorithmIdentifier right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
            if (ReferenceEquals(left, null) && !ReferenceEquals(right, null)) return false;
            if (!ReferenceEquals(left, null) && ReferenceEquals(right, null)) return false;
            return left.Equals(right);
        }

        public override int GetHashCode() => RawData.GetHashCode();

        public static bool operator !=(AlgorithmIdentifier left, AlgorithmIdentifier right) => !(left == right);
    }
}