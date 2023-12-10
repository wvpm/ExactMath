using System;
using System.Diagnostics.CodeAnalysis;

namespace ExactMath;

public readonly struct Root {
	public static readonly Root Zero = new(Integer.One, Fraction.Zero), One = new(Integer.One, Fraction.One);

	/// <summary>
	/// Always positive
	/// </summary>
	public Integer Degree { get; }
	public Fraction Radicand { get; }

	public Root(Integer degree, Fraction radicand) {
		if (degree.IsZero()) {
			throw new DivideByZeroException("Zeroth root does not exist.");
		}

		if (degree.IsNegative()) {
			degree = -degree;
			radicand = Integer.One / radicand;
		}

		long simplifiedDegree = degree.Value, simplifiedNumerator = radicand.Numerator.Value, simplifiedDenominator = radicand.Denominator.Value;
		foreach (long prime in degree.FindPrimeDivisors()) {
			double numerator = Math.Pow(simplifiedNumerator, 1D / prime);
			if (!double.IsInteger(numerator)) { continue; }
			double denomerator = Math.Pow(simplifiedDenominator, 1D / prime);
			if (!double.IsInteger(denomerator)) { continue; }

			simplifiedDegree /= prime;
			simplifiedNumerator = (long)numerator;
			simplifiedDenominator = (long)denomerator;
		}

		if (simplifiedDegree == degree.Value) {
			Degree = degree;
			Radicand = radicand;
		}
		else {
			Degree = new(simplifiedDegree);
			Radicand = new Integer(simplifiedNumerator) / new Integer(simplifiedDenominator);
		}
	}

	public Root RaiseTo(Fraction exponent) => new(Degree * exponent.Denominator, Radicand.RaiseTo(exponent.Numerator));
	public static Root operator *(Root a, Root b) => new(a.Degree * b.Degree, a.Radicand.RaiseTo(b.Degree) * b.Radicand.RaiseTo(a.Degree));
	public static Root operator /(Root numerator, Root denominator) => numerator * new Root(-denominator.Degree, denominator.Radicand);
	public static bool operator >(Root a, Root b) => a.CompareTo(b) > 0;
	public static bool operator >=(Root a, Root b) => a.CompareTo(b) >= 0;
	public static bool operator <(Root a, Root b) => a.CompareTo(b) < 0;
	public static bool operator <=(Root a, Root b) => a.CompareTo(b) <= 0;
	public static bool operator ==(Root a, Root b) => a.CompareTo(b) == 0;
	public static bool operator !=(Root a, Root b) => !(a == b);
	public bool TryCastInteger([NotNullWhen(true)] out Integer? integer) {
		integer = TryCastFraction(out Fraction? fraction) && fraction.Value.TryCastInteger(out Integer? i) ? i.Value : null;
		return integer.HasValue;
	}
	public static explicit operator Integer(Root a) => (Integer)(Fraction)a;
	public bool TryCastFraction([NotNullWhen(true)] out Fraction? fraction) {
		if (Degree == Integer.One) {
			fraction = Radicand;
		}
		else if (Radicand == Fraction.Zero) {
			fraction = Fraction.Zero;
		}
		else if (Radicand == Fraction.One) {
			fraction = Fraction.One;
		}
		else if (Radicand == Fraction.MinusOne && !Degree.IsEven()) {
			fraction = Fraction.MinusOne;
		}
		else { fraction = null; }
		return fraction.HasValue;
	}

	public static explicit operator Fraction(Root a) =>
		a.TryCastFraction(out Fraction? fraction)
			? fraction.Value
			: throw new InvalidCastException("Cannot cast " + nameof(Root) + " to " + nameof(Fraction) + " as the " + nameof(Degree) + " is not 1 and the " + nameof(Radicand) + " is neither 0 nor 1.");

	public int CompareTo(Root other) => Radicand.RaiseTo(other.Degree).CompareTo(other.Radicand.RaiseTo(Degree));

	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Root root && Equals(root);
	public bool Equals(Root other) => other == this;

	public override int GetHashCode() => HashCode.Combine(Degree, Radicand);

	public override string ToString() => TryCastFraction(out Fraction? fraction) ? fraction.Value.ToString() : $"({Radicand})^({Integer.One / Degree})";
	/// <summary>
	/// Only returns the positive solution to even degree roots.
	/// </summary>
	/// <returns></returns>
	public Integer Truncate() => (long)Math.Truncate(ToDouble());
	/// <summary>
	/// Approximation
	/// Only returns the positive solution to even degree roots.
	/// </summary>
	/// <returns></returns>
	public decimal ToDecimal() => (decimal)ToDouble();
	/// <summary>
	/// Approximation
	/// Only returns the positive solution to even degree roots.
	/// </summary>
	/// <returns></returns>
	public double ToDouble() => Math.Pow(Radicand.ToDouble(), 1D / Degree.Value);
}