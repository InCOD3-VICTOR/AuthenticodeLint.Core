using System;
using System.Numerics;

namespace AuthenticodeLint.Core.Asn
{

	/// <summary>
	/// A signed, big endian, asn1 integer.
	/// </summary>
	public sealed class AsnInteger : AsnElement
	{
		/// <summary>
		/// The value of the integer.
		/// </summary>
		public BigInteger Value { get; }

		public AsnInteger(AsnTagType tag, ArraySegment<byte> data) : base(tag, data)
		{
			var buffer = new byte[data.Count];
			//BigInteger expects the number in little endian.
			for (int i = data.Count - 1, j = 0; i >= 0; i--, j++)
			{
				buffer[j] = data.Array[data.Offset + i];
			}
			Value = new BigInteger(buffer);
		}

		public override string ToString() => Value.ToString();
	}

}