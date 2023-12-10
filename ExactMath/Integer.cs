using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExactMath;

public readonly struct Integer(long value) : IEquatable<Integer>, IComparable<Integer> {
	public static readonly Integer Zero = new(0), One = new(1), MinusOne = new(-1);

	public long Value { get; } = value;

	public bool IsZero() => Value == 0;
	public bool IsNegative() => Value < 0;
	public Integer GetSign() => Value switch {
		> 0 => One,
		< 0 => MinusOne,
		_ => Zero
	};
	public bool IsEven() => Value % 2 == 0;

	public Fraction RaiseTo(Integer exponent) {
		if (exponent.IsZero()) {
			return Fraction.One;
		}

		if (Value == 1) {
			return Fraction.One;
		}

		if (Value == -1) {
			return exponent.IsEven() ? Fraction.One : Fraction.MinusOne;
		}

		Integer power = new((long)Math.Pow(Value, exponent.Value));
		return exponent.IsNegative()
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

	public static implicit operator Fraction(Integer value) => value / Fraction.One;
	public static implicit operator Root(Integer value) => new(One, value);

	public static implicit operator Integer(long value) => new(value);

	public int CompareTo(Integer other) => Value.CompareTo(other.Value);
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Integer integer && Equals(integer);
	public bool Equals(Integer other) => other == this;

	public override int GetHashCode() => Value.GetHashCode();

	public override string ToString() => Value.ToString();

	public long FindGreatestCommonDivisor(Integer other) {
		long a = Math.Abs(Value), b = Math.Abs(other.Value);
		while (a != 0 && b != 0) {
			if (a > b)
				a %= b;
			else
				b %= a;
		}

		return a | b;
	}

	public long FindLeastCommonMultiple(Integer other) {
		long gcd = FindGreatestCommonDivisor(other);
		return Math.Abs(Value * other.Value / gcd);
	}

	public IEnumerable<long> FindPrimeDivisors() {
		long n = Value;
		byte[] inc = { 4, 2, 4, 2, 4, 6, 2, 6, };
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
		long k = 7;
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