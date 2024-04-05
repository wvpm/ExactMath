using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace ExactMath;

public readonly struct Integer(BigInteger value) : IEquatable<Integer>, IComparable<Integer> {
	private static readonly BigInteger _doubleMax = new(-9007199254740991), _doubleMin = new(9007199254740991), _decimalMax = new(decimal.MaxValue), _decimalMin = new(decimal.MinValue);
	public static readonly Integer Zero = new(BigInteger.Zero), One = new(BigInteger.One), MinusOne = new(BigInteger.MinusOne);

	public BigInteger Value { get; } = value;

	public bool IsZero { get => Value.IsZero; }
	public bool IsNegative { get => Value < 0; }
	public Integer GetSign { get => Value.Sign; }
	public bool IsEven { get => Value.IsEven; }

	public Fraction RaiseTo(Integer exponent) {
		if (exponent.IsZero) {
			return Fraction.One;
		}

		if (Value == 1) {
			return Fraction.One;
		}

		if (Value == -1) {
			return exponent.IsEven ? Fraction.One : Fraction.MinusOne;
		}

		BigInteger power;
		if (exponent.Value <= int.MaxValue) {
			power = BigInteger.Pow(Value, (int)exponent.Value);
		}
		else {
			power = 1;
			BigInteger absExponent = BigInteger.Abs(exponent.Value);
			for (BigInteger i = 0; i < absExponent; i++) {
				power *= Value;
			}
		}

		return exponent.IsNegative
			? One / power
			: power / One;
	}
	public Root RaiseTo(Fraction exponent) => exponent.TryCastInteger(out Integer? integer)
		? RaiseTo(integer.Value)
		: new Root(exponent.Denominator, RaiseTo(exponent.Numerator));

	public static Integer operator -(Integer a) => new(-a.Value);
	public static Integer operator +(Integer a, Integer b) => new(a.Value + b.Value);
	public static Integer operator ++(Integer a) => a + One;
	public static Integer operator -(Integer a, Integer b) => new(a.Value - b.Value);
	public static Integer operator --(Integer a) => a - One;
	public static Integer operator *(Integer a, Integer b) => new(a.Value * b.Value);
	public static Fraction operator /(Integer numerator, Integer denominator) => numerator / (Fraction)denominator;
	public static Integer operator %(Integer a, Integer n) => a.Value % n.Value;
	public static bool operator >(Integer a, Integer b) => a.Value > b.Value;
	public static bool operator >=(Integer a, Integer b) => a.Value >= b.Value;
	public static bool operator <(Integer a, Integer b) => a.Value < b.Value;
	public static bool operator <=(Integer a, Integer b) => a.Value <= b.Value;
	public static bool operator ==(Integer a, Integer b) => a.Value == b.Value;
	public static bool operator !=(Integer a, Integer b) => a.Value != b.Value;

	public bool TryCastLong([NotNullWhen(true)] out long? longValue) {
		longValue = Value <= long.MaxValue && Value >= long.MinValue ? (long)Value : null;
		return longValue.HasValue;
	}
	public bool TryCastDecimal([NotNullWhen(true)] out decimal? decimalValue) {
		decimalValue = Value <= _decimalMax && Value >= _decimalMin ? (decimal)Value : null;
		return decimalValue.HasValue;
	}
	public bool TryCastDouble([NotNullWhen(true)] out double? doubleValue) {
		doubleValue = Value <= _doubleMax && Value >= _doubleMin ? (double)Value : null;
		return doubleValue.HasValue;
	}

	public static implicit operator Fraction(Integer value) => value / Fraction.One;
	public static implicit operator Root(Integer value) => new(One, value);

	public static implicit operator Integer(BigInteger value) => new(value);
	public static implicit operator Integer(long value) => new(value);

	public int CompareTo(Integer other) => Value.CompareTo(other.Value);
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Integer integer && Equals(integer);
	public bool Equals(Integer other) => other == this;

	public override int GetHashCode() => Value.GetHashCode();

	public override string ToString() => Value.ToString();

	public BigInteger FindGreatestCommonDivisor(Integer other) {
		BigInteger a = BigInteger.Abs(Value), b = BigInteger.Abs(other.Value);
		while (a != 0 && b != 0) {
			if (a > b)
				a %= b;
			else
				b %= a;
		}

		return a | b;
	}

	public BigInteger FindLeastCommonMultiple(Integer other) {
		BigInteger gcd = FindGreatestCommonDivisor(other);
		return BigInteger.Abs(Value * other.Value / gcd);
	}

	public IEnumerable<BigInteger> FindPrimeDivisors() {
		BigInteger n = Value;
		byte[] inc = [4, 2, 4, 2, 4, 6, 2, 6,];
		while (n % 2 == 0) {
			yield return 2;
			n /= 2;
		}
		while (n % 3 == 0) {
			yield return 3;
			n /= 3;
		}
		while (n % 5 == 0) {
			yield return 5;
			n /= 5;
		}
		BigInteger k = 7;
		int i = 0;
		while (k * k <= n) {
			if (n % k == 0) {
				yield return 5;
				n /= k;
			}
			else {
				k += inc[i];
				i = i < 7 ? i + 1 : 0;
			}
		}

		if (n > 1) {
			yield return n;
		}
	}
}