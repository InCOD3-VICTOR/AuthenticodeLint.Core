using System;
using AuthenticodeLint.Core.Asn;

namespace AuthenticodeLint.Core.Pkcs7
{
    public struct TstAccuracy : IEquatable<TstAccuracy>
    {
        private long _seconds;
        private short _millis;
        private short _micros;
        private long _ticks;

        public TstAccuracy(AsnSequence sequence)
        {
            var reader = new AsnConstructedReader(sequence);
            if (reader.MoveNext(out AsnInteger seconds))
            {
                _seconds = (long)seconds.Value;
                _millis = 0;
                _micros = 0;
            }
            else
            {
                throw new Pkcs7Exception("Unable to read seconds for accuracy.");
            }
            while (reader.MoveNext(out AsnElement next))
            {
                if (next.Tag.IsExImTag(0))
                {
                    var millis = next.Reinterpret<AsnInteger>();
                    _millis = (short)millis.Value;
                }
                else if (next.Tag.IsExImTag(1))
                {
                    var micros = next.Reinterpret<AsnInteger>();
                    _micros = (short)micros.Value;
                }
            }
            const long ticksPerSecond = 10000000;
            const long ticksPerMilli = ticksPerSecond / 1000;
            const long ticksPerMicro = ticksPerMilli / 1000;
            _ticks = _seconds * ticksPerSecond + _millis * ticksPerMilli + _micros * ticksPerMicro;
        }

        public static explicit operator TimeSpan(TstAccuracy accuracy) => new TimeSpan(accuracy._ticks);

        public static bool operator < (TstAccuracy left, TstAccuracy right) => left._ticks < right._ticks;
        public static bool operator > (TstAccuracy left, TstAccuracy right) => left._ticks > right._ticks;
        public static bool operator == (TstAccuracy left, TstAccuracy right) => left._ticks == right._ticks;
        public static bool operator != (TstAccuracy left, TstAccuracy right) => left._ticks != right._ticks;

        public override int GetHashCode() => _ticks.GetHashCode();

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case TstAccuracy tst:
                    return Equals(tst);
                default:
                    return false;
            }
        }

        public bool Equals(TstAccuracy other) => this == other;

        public long Seconds => _seconds;
        public short Milliseconds => _millis;
        public short Microseconds => _micros;
    }
}