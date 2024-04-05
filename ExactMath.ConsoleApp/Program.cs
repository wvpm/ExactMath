using System;
using System.Collections.Generic;
using System.Linq;
using ExactMath;

int limit = 7;
HashSet<Fraction> f0 = new(limit * (limit + 1) / 2);
for (int i = 1; i <= limit; i++) {
	for (int j = 2; j <= i; j++) {
		Fraction f = new(i, j);
		if (f <= 2) {
			f0.Add(f);
		}
	}
}

HashSet<FractionChain> f1 = [];
foreach (Fraction fA in f0) {
	foreach (Fraction fB in f0) {
		FractionChain fractionChain = new([fA, fB]);
		f1.Add(fractionChain);
	}
}

HashSet<FractionChain> f2 = [];
foreach (FractionChain fA in f1) {
	foreach (Fraction fB in f0) {
		f2.Add(fA * fB);
	}
}

foreach (FractionChain fractionChain in f2.Where(x => x.Combined <= 4).OrderBy(x => x.Combined)) {
	if (fractionChain.Combined.TryCastDecimal(out var decimalValue)) {
		Console.Write($"{fractionChain.LeastCommonMultiple}\t{decimalValue:N3}\t{fractionChain.Combined}\t");
		Console.WriteLine(string.Join('\t', fractionChain.Factors));
	}
}

Console.ReadLine();

internal class FractionChain {
	public IReadOnlyList<Fraction> Factors { get; }
	public Fraction Combined { get; }
	public Integer LeastCommonMultiple { get; }
	public FractionChain(IReadOnlyList<Fraction> factors) {
		Factors = factors;
		Fraction combined = Fraction.One;
		Integer leastCommonMultiple = 1;
		foreach (Fraction f in Factors) {
			combined *= f;
			if (combined.Denominator > leastCommonMultiple) {
				leastCommonMultiple = combined.Denominator;
			}
		}
		Combined = combined;
		LeastCommonMultiple = leastCommonMultiple;
	}

	public override bool Equals(object? obj) {
		return obj is FractionChain other && Factors.SequenceEqual(other.Factors);
	}

	public override int GetHashCode() {
		return Combined.GetHashCode();
	}

	public static FractionChain operator *(FractionChain lhs, Fraction rhs) {
		return new FractionChain(new List<Fraction>(lhs.Factors) { rhs });
	}
}