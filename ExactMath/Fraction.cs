using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace ExactMath;

public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction> {
	public static readonly Fraction Zero = new(Integer.Zero, Integer.One), One = new(Integer.One, Integer.One), MinusOne = new(Integer.MinusOne, Integer.One);
	public Integer Numerator { get; }
	/// <summary>
	/// Always positive
	/// </summary>
	public Integer Denominator { get; }

	public Fraction(Integer numerator, Integer denominator) : this(numerator, denominator, true) { }

	private Fraction(Integer numerator, Integer denominator, bool shouldSimplify) {
		if (denominator.IsZero) {
			throw new DivideByZeroException();
		}

		if (shouldSimplify) {
			if (denominator.IsNegative) {
				numerator = -numerator;
				denominator = -denominator;
			}

			BigInteger gcd = numerator.FindGreatestCommonDivisor(denominator);
			if (gcd > 1) {
				numerator = new(numerator.Value / gcd);
				denominator = new(denominator.Value / gcd);
			}
		}

		Numerator = numerator;
		Denominator = denominator;
	}

	public bool IsZero { get => Numerator.IsZero; }
	public Integer GetSign { get => Numerator.GetSign * Denominator.GetSign; }
	public bool IsNegative { get => GetSign.IsNegative; }

	public Fraction RaiseTo(Integer exponent) {
		if (exponent.IsZero) {
			return One;
		}

		if (this == One) {
			return One;
		}

		if (this == MinusOne) {
			return exponent.IsEven ? One : MinusOne;
		}

		Integer numeratorPower = (Integer)Numerator.RaiseTo(exponent);
		Integer denominatorPower = (Integer)Denominator.RaiseTo(exponent);
		return exponent.IsNegative
			? new(denominatorPower, numeratorPower)
			: new(numeratorPower, denominatorPower);
	}

	public Root RaiseTo(Fraction exponent) => exponent.TryCastInteger(out Integer? integer)
		? RaiseTo(integer.Value)
		: new Root(exponent.Denominator, RaiseTo(exponent.Numerator));

	public static Fraction operator -(Fraction a) => a * MinusOne;
	public static Fraction operator +(Fraction a, Fraction b) => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
	public static Fraction operator ++(Fraction value) => value + One;
	public static Fraction operator -(Fraction a, Fraction b) => a + -b;
	public static Fraction operator --(Fraction value) => value - One;
	public static Fraction operator *(Fraction a, Fraction b) => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
	public static Fraction operator /(Integer numerator, Fraction denominator) => new(numerator * denominator.Denominator, denominator.Numerator);
	public static Fraction operator /(Fraction numerator, Integer denominator) => new(numerator.Numerator, numerator.Denominator * denominator);
	public static Fraction operator /(Fraction numerator, Fraction denominator) => numerator * new Fraction(denominator.Denominator, denominator.Numerator, false);
	public static bool operator >(Fraction a, Fraction b) => a.CompareTo(b) > 0;
	public static bool operator >=(Fraction a, Fraction b) => a.CompareTo(b) >= 0;
	public static bool operator <(Fraction a, Fraction b) => a.CompareTo(b) < 0;
	public static bool operator <=(Fraction a, Fraction b) => a.CompareTo(b) <= 0;
	public static bool operator ==(Fraction a, Fraction b) => a.CompareTo(b) == 0;
	public static bool operator !=(Fraction a, Fraction b) => !(a == b);

	public bool TryCastInteger([NotNullWhen(true)] out Integer? integer) {
		integer = Denominator == Integer.One ? Numerator : null;
		return integer.HasValue;
	}
	public static explicit operator Integer(Fraction value) => value.TryCastInteger(out Integer? integer) ? integer.Value : throw new InvalidCastException("Cannot cast " + nameof(Fraction) + " to " + nameof(Integer) + " as the " + nameof(Denominator) + " is not 1.");
	public static implicit operator Root(Fraction value) => new(Integer.One, value);

	public static implicit operator Fraction(decimal value) {
		long denominator = (long)Math.Pow(10D, value.Scale);
		long numerator = (long)(value * denominator);
		return new(new(numerator), new(denominator));
	}

	public int CompareTo(Fraction other) => (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);

	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Fraction fraction && Equals(fraction);
	public bool Equals(Fraction other) => other == this;

	public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

	public override string ToString() => TryCastInteger(out Integer? integer) ? integer.Value.ToString() : Numerator.ToString() + '/' + Denominator.ToString();
	public Integer Truncate() => Numerator.Value / Denominator.Value;
	/// <summary>
	/// Approximation
	/// </summary>
	/// <returns></returns>
	public bool TryCastDecimal([NotNullWhen(true)] out decimal? decimalValue) {
		Integer quotientBig = DivideRemainder(out Fraction remainder);
		if (!quotientBig.TryCastDecimal(out var quotient)
			|| !remainder.Numerator.TryCastDecimal(out var numerator)
			|| !remainder.Denominator.TryCastDecimal(out var denominator)) {
			decimalValue = null;
			return false;
		}
		decimalValue = quotient + numerator / denominator;
		return true;
	}

	/// <summary>
	/// Approximation
	/// </summary>
	/// <returns></returns>
	public bool TryCastDouble([NotNullWhen(true)] out double? doubleValue) {
		Integer quotientBig = DivideRemainder(out Fraction remainder);
		if (!quotientBig.TryCastDouble(out var quotient)
			|| !remainder.Numerator.TryCastDouble(out var numerator)
			|| !remainder.Denominator.TryCastDouble(out var denominator)) {
			doubleValue = null;
			return false;
		}
		doubleValue = quotient + numerator / denominator;
		return true;
	}
	public Integer DivideRemainder(out Fraction remainder) {
		if (Denominator == Integer.One) {
			remainder = Zero;
			return Numerator;
		}

		if (Numerator.IsZero) {
			remainder = Zero;
			return Integer.Zero;
		}

		if (Numerator.IsNegative) {
			Integer whole = (-this).DivideRemainder(out remainder);
			remainder = -remainder;
			return -whole;
		}

		Integer modulo = Numerator % Denominator;
		remainder = modulo / Denominator;
		return Truncate();
	}
}