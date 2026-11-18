using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The module's own arithmetic, <see cref="Exact"/>, within two units in the last place of the
/// runtime's over a sweep and at the edges, its results the same to the bit on every system, and
/// the build rounding a product and a sum apart rather than fusing them.
/// </summary>
public sealed class ExactTests
{
    private const int Ulps = 2;

    private static readonly float[] Unit = MakeUnit();

    private static readonly float[] Angles =
    [
        .. Unit.Select(value => (value * 2f - 1f) * 40f),
        0f, -0f, 1e-30f, -1e-30f, MathF.PI / 4f, -MathF.PI / 4f, MathF.PI / 2f, MathF.PI, 3f, -3f,
        1e5f, -1e5f, 1.5e6f, 1.6e6f, -1.6e6f, 2e6f, 1e10f, -1e10f, 1e20f, 1e30f, -1e30f, float.MaxValue, float.MinValue,
    ];

    /// <summary>
    /// Sine and cosine stay within two units in the last place, huge angles among them, and give
    /// NaN for no angle.
    /// </summary>
    [Fact]
    public void SineAndCosineAreTheRuntimes()
    {
        foreach (var angle in Angles)
        {
            Near(MathF.Sin(angle), Exact.Sin(angle), $"sin {angle:R}");
            Near(MathF.Cos(angle), Exact.Cos(angle), $"cos {angle:R}");
            var (sin, cos) = Exact.SinCos(angle);
            Assert.Equal((Exact.Sin(angle), Exact.Cos(angle)), (sin, cos));
        }

        foreach (var none in (float[])[float.NaN, float.PositiveInfinity, float.NegativeInfinity])
            Assert.Equal((float.NaN, float.NaN), Exact.SinCos(none));
        Assert.True(float.IsNegative(Exact.Sin(-0f)));
    }

    /// <summary>
    /// The exponential stays within two units in the last place from a tiny result to past the
    /// largest float, which it gives as infinity.
    /// </summary>
    [Fact]
    public void TheExponentialIsTheRuntimes()
    {
        foreach (var power in Unit.Select(value => value * 200f - 110f).Concat([0f, -0f, 1f, -1f, 88.7f, 88.8f, -103f, -110f, 100f]))
            Near(MathF.Exp(power), Exact.Exp(power), $"exp {power:R}");
        Assert.Equal(float.PositiveInfinity, Exact.Exp(float.PositiveInfinity));
        Assert.Equal(0f, Exact.Exp(float.NegativeInfinity));
        Assert.True(float.IsNaN(Exact.Exp(float.NaN)));
    }

    /// <summary>
    /// A power stays within two units in the last place over the bases and powers the growth, the
    /// leaves and the pictures take, and keeps IEEE 754's special cases.
    /// </summary>
    [Fact]
    public void APowerIsTheRuntimes()
    {
        float[] powers = [0.75f, 0.8f, 0.9f, 2.4f, 3f, 0.1f, 0.5f, 1.5f, 4f, 10f];
        foreach (var value in Unit.Take(4_000).Select(value => value * 3f))
        foreach (var power in powers)
            Near(MathF.Pow(value, power), Exact.Pow(value, power), $"pow {value:R} {power:R}");
        foreach (var value in Unit.Take(4_000))
        {
            Near(MathF.Pow(100f, value), Exact.Pow(100f, value), $"pow 100 {value:R}");
            Near(MathF.Pow(0.5f, value * 30f), Exact.Pow(0.5f, value * 30f), $"pow 0.5 {value * 30f:R}");
        }

        (float Value, float Power, float Result)[] special =
        [
            (0f, 2f, 0f), (0f, -1f, float.PositiveInfinity), (-0f, 3f, -0f), (-0f, -3f, float.NegativeInfinity),
            (-2f, 3f, -8f), (-2f, 2f, 4f), (-2f, 0.5f, float.NaN), (1f, float.NaN, 1f), (float.NaN, 0f, 1f),
            (2f, float.PositiveInfinity, float.PositiveInfinity), (0.5f, float.PositiveInfinity, 0f),
            (-1f, float.PositiveInfinity, 1f), (float.PositiveInfinity, 2f, float.PositiveInfinity),
            (float.PositiveInfinity, -1f, 0f), (float.NaN, 1f, float.NaN), (2f, 0f, 1f),
        ];
        foreach (var (value, power, result) in special)
        {
            Assert.Equal(result, Exact.Pow(value, power));
            Assert.Equal(float.IsNegative(result), float.IsNegative(Exact.Pow(value, power)));
        }
    }

    /// <summary>
    /// The arc cosine stays within two units in the last place from -1 to 1 and gives NaN outside.
    /// </summary>
    [Fact]
    public void TheArcCosineIsTheRuntimes()
    {
        foreach (var value in Unit.Select(value => value * 2f - 1f).Concat([1f, -1f, 0f, -0f, 0.999999f, -0.999999f]))
            Near(MathF.Acos(value), Exact.Acos(value), $"acos {value:R}");
        Assert.True(float.IsNaN(Exact.Acos(1.0001f)));
        Assert.True(float.IsNaN(Exact.Acos(float.NaN)));
    }

    /// <summary>
    /// The arc tangent of a point stays within two units in the last place over the plane and keeps
    /// the runtime's angles at zeros and infinities.
    /// </summary>
    [Fact]
    public void TheArcTangentIsTheRuntimes()
    {
        for (var i = 0; i + 1 < Unit.Length; i += 2)
        {
            var (y, x) = ((Unit[i] * 2f - 1f) * 10f, (Unit[i + 1] * 2f - 1f) * 10f);
            Near(MathF.Atan2(y, x), Exact.Atan2(y, x), $"atan2 {y:R} {x:R}");
        }

        float[] edges = [0f, -0f, 1f, -1f, 1e-30f, -1e30f, float.PositiveInfinity, float.NegativeInfinity];
        foreach (var y in edges)
        foreach (var x in edges)
        {
            Near(MathF.Atan2(y, x), Exact.Atan2(y, x), $"atan2 {y:R} {x:R}");
            Assert.Equal(float.IsNegative(MathF.Atan2(y, x)), float.IsNegative(Exact.Atan2(y, x)));
        }
    }

    /// <summary>
    /// The functions give the same bits on every system, hashed over the sweeps, the hashes taken
    /// on the machine that wrote this test.
    /// </summary>
    [Fact]
    public void TheFunctionsAreTheSameOnEverySystem()
    {
        var made = new Dictionary<string, string>
        {
            ["sin"] = Hash(Angles.Select(Exact.Sin)),
            ["cos"] = Hash(Angles.Select(Exact.Cos)),
            ["exp"] = Hash(Unit.Select(value => Exact.Exp(value * 200f - 110f))),
            ["pow"] = Hash(Unit.Select(value => Exact.Pow(value * 3f, 0.75f))),
            ["acos"] = Hash(Unit.Select(value => Exact.Acos(value * 2f - 1f))),
            ["atan2"] = Hash(Unit.Zip(Unit.Skip(1), (y, x) => Exact.Atan2(y * 2f - 1f, x * 2f - 1f))),
        };
        var kept = new Dictionary<string, string>
        {
            ["sin"] = "530173CA16AD072A", ["cos"] = "54C541B8E96ED64A", ["exp"] = "5B0342711E36258C",
            ["pow"] = "914CB7B61E2059FC", ["acos"] = "9E12CBE5CE66F52B", ["atan2"] = "54E406E496F5E299",
        };
        Assert.True(made.All(pair => kept[pair.Key] == pair.Value), string.Join(", ", made.Select(pair => $"{pair.Key} {pair.Value}")));
    }

    /// <summary>
    /// A product and a sum round apart, as IEEE 754 rounds each, where fusing them into one
    /// rounding would give another number. (1 + 2⁻¹²)² rounds to 1 + 2⁻¹¹, so less 1 + 2⁻¹¹ it
    /// is zero, where fused it is 2⁻²⁴, and the same in doubles and in a vector's lanes.
    /// </summary>
    [Fact]
    public void TheBuildDoesNotFuseAProductAndASum()
    {
        // Read from a string, so neither the compiler nor the JIT folds the sum as a constant.
        var a = float.Parse(Environment.GetEnvironmentVariable("VEGETATION_NONE") ?? "1.000244140625", System.Globalization.CultureInfo.InvariantCulture);
        var c = -(1f + 2f * (a - 1f));
        Assert.Equal(0f, a * a + c);

        // (1 + 2⁻²⁷)² rounds to 1 + 2⁻²⁶ in a double, where fused less it the sum is 2⁻⁵⁴.
        var ad = double.Parse(Environment.GetEnvironmentVariable("VEGETATION_NONE") ?? "1.000000007450580596923828125", System.Globalization.CultureInfo.InvariantCulture);
        var cd = -(1.0 + 2.0 * (ad - 1.0));
        Assert.Equal(0.0, ad * ad + cd);

        var lanes = new System.Numerics.Vector3(a) * new System.Numerics.Vector3(a) + new System.Numerics.Vector3(c);
        Assert.Equal(System.Numerics.Vector3.Zero, lanes);
    }

    private static void Near(float expected, float actual, string what)
    {
        if (float.IsNaN(expected) || float.IsNaN(actual))
        {
            Assert.True(float.IsNaN(expected) && float.IsNaN(actual), $"{what} is {actual:R} where the runtime gives {expected:R}");
            return;
        }

        var apart = Math.Abs(Ordered(expected) - Ordered(actual));
        Assert.True(apart <= Ulps, $"{what} is {actual:R}, {apart} units in the last place from the runtime's {expected:R}");
    }

    // A float's bits as a whole number that counts up with the float, so two floats' difference
    // is how many floats lie between them.
    private static long Ordered(float value)
    {
        var bits = BitConverter.SingleToInt32Bits(value);
        return bits < 0 ? -(long)(bits & int.MaxValue) : bits;
    }

    private static float[] MakeUnit()
    {
        var random = new Pcg(0xE7AC7);
        return [.. Enumerable.Range(0, 20_000).Select(_ => random.Unit())];
    }

    private static string Hash(IEnumerable<float> values) => Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(values.ToArray().AsSpan())))[..16];
}
