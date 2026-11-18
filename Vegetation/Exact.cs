using System.Globalization;
using System.Numerics;

namespace Vegetation;

// The sine, cosine, power, exponential, arc cosine and arc tangent the module works a tree and its
// pictures out with, the same to the bit on every processor and system. The runtime's own come
// from each system's library, which rounds them apart in the last place, and a last place apart in
// the growth buds a twig more, so a seed grew another tree on another machine. Each function here
// is worked in doubles by sums, products and quotients alone, which IEEE 754 rounds alike
// everywhere and the JIT does not fuse into one rounding, and its result rounded once to a float,
// within two units in the last place of the runtime's, which ExactTests holds.
internal static class Exact
{
    // π/2 in three parts of 33 bits and a last part, so k times each of the first three is exact
    // for a k below 2^20, Cody and Waite's reduction of an argument below 1.6e6.
    private const double HalfPi1 = 1.57079632673412561417e+00;
    private const double HalfPi2 = 6.07710050630396597660e-11;
    private const double HalfPi3 = 2.02226624871116645580e-21;
    private const double HalfPi3Tail = 8.47842766036889956997e-32;
    private const double TwoOverPiDouble = 6.36619772367581382433e-01;
    private const double SmallReduction = 1.6e6;

    // 2/π to 448 bits, floor(2/π * 2^448), for the reduction of a larger argument by Payne and
    // Hanek's method, worked out from Machin's formula for π in integers.
    private const int TwoOverPiBits = 448;
    private static readonly BigInteger TwoOverPi = BigInteger.Parse(
        "0A2F9836E4E441529FC2757D1F534DDC0DB6295993C439041FE5163ABDEBBC561B7246E3A424DD2E006492EEA09D1921CFE1DEB1CB129A73E",
        NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    // ln 2 in a part of 32 bits and the rest, so k times the first is exact for the k of a double.
    private const double Ln2High = 6.93147180369123816490e-01;
    private const double Ln2Low = 1.90821492927058770002e-10;
    private const double Ln2 = 6.93147180559945286227e-01;

    private const double Sqrt3 = 1.73205080756887719318e+00;
    private const double TanPiOver12 = 2.67949192431122695761e-01;

    /// <summary>The sine of an angle in radians.</summary>
    public static float Sin(float angle) => (float)SinCos((double)angle).Sin;

    /// <summary>The cosine of an angle in radians.</summary>
    public static float Cos(float angle) => (float)SinCos((double)angle).Cos;

    /// <summary>The sine and the cosine of an angle in radians.</summary>
    public static (float Sin, float Cos) SinCos(float angle)
    {
        var (sin, cos) = SinCos((double)angle);
        return ((float)sin, (float)cos);
    }

    /// <summary>
    /// A number raised to a power, by IEEE 754's special cases where one applies.
    /// </summary>
    public static float Pow(float value, float power) => (float)Pow((double)value, power);

    /// <summary>e raised to a power.</summary>
    public static float Exp(float power) => (float)Exp((double)power);

    /// <summary>
    /// The angle in radians from 0 to π whose cosine a number is, NaN outside -1 to 1.
    /// </summary>
    public static float Acos(float value)
    {
        if (!(value is >= -1f and <= 1f)) return float.NaN;
        var x = (double)value;
        return (float)Atan2(Math.Sqrt((1.0 - x) * (1.0 + x)), x);
    }

    /// <summary>
    /// The angle in radians from -π to π of the point at x and y, as Math.Atan2 gives it.
    /// </summary>
    public static float Atan2(float y, float x) => (float)Atan2((double)y, (double)x);

    // -- In doubles

    private static (double Sin, double Cos) SinCos(double angle)
    {
        if (!double.IsFinite(angle)) return (double.NaN, double.NaN);

        // A zero's sign kept, which the reduction's subtraction would turn positive.
        if (angle == 0.0) return (angle, 1.0);
        var r = Reduce(angle, out var quadrant);
        var (s, c) = (SinKernel(r), CosKernel(r));
        return quadrant switch
        {
            0 => (s, c),
            1 => (c, -s),
            2 => (-s, -c),
            _ => (-c, s),
        };
    }

    // The angle less the nearest multiple of π/2, from -π/4 to π/4, and that multiple's quadrant.
    private static double Reduce(double angle, out int quadrant)
    {
        if (Math.Abs(angle) < SmallReduction)
        {
            var k = Math.Round(angle * TwoOverPiDouble);
            quadrant = (int)((long)k & 3);
            return angle - k * HalfPi1 - k * HalfPi2 - k * HalfPi3 - k * HalfPi3Tail;
        }

        // The angle a whole number of bits times a power of two, times 2/π in integers, its
        // whole part the quadrant and its fraction the share of π/2 left.
        var bits = BitConverter.DoubleToInt64Bits(Math.Abs(angle));
        var exponent = (int)((bits >> 52) & 0x7FF) - 1075;
        var mantissa = (bits & ((1L << 52) - 1)) | (1L << 52);
        var product = mantissa * TwoOverPi;
        var shift = TwoOverPiBits - exponent;
        var whole = product >> shift;
        var fraction = product - (whole << shift);
        if (fraction >= BigInteger.One << (shift - 1))
        {
            whole += 1;
            fraction -= BigInteger.One << shift;
        }

        var r = Math.ScaleB((double)fraction, -shift) * (Math.PI / 2.0);
        quadrant = (int)(whole & 3);
        if (angle < 0)
        {
            r = -r;
            quadrant = (4 - quadrant) & 3;
        }

        return r;
    }

    // Taylor's series to the 19th power, below 1e-19 off for an angle within π/4.
    private static double SinKernel(double r)
    {
        var r2 = r * r;
        var sum = 1.0 / 121645100408832000.0;
        sum = sum * -r2 + 1.0 / 355687428096000.0;
        sum = sum * -r2 + 1.0 / 1307674368000.0;
        sum = sum * -r2 + 1.0 / 6227020800.0;
        sum = sum * -r2 + 1.0 / 39916800.0;
        sum = sum * -r2 + 1.0 / 362880.0;
        sum = sum * -r2 + 1.0 / 5040.0;
        sum = sum * -r2 + 1.0 / 120.0;
        sum = sum * -r2 + 1.0 / 6.0;
        return r - r * r2 * sum;
    }

    // Taylor's series to the 20th power, below 1e-19 off for an angle within π/4.
    private static double CosKernel(double r)
    {
        var r2 = r * r;
        var sum = 1.0 / 2432902008176640000.0;
        sum = sum * -r2 + 1.0 / 6402373705728000.0;
        sum = sum * -r2 + 1.0 / 20922789888000.0;
        sum = sum * -r2 + 1.0 / 87178291200.0;
        sum = sum * -r2 + 1.0 / 479001600.0;
        sum = sum * -r2 + 1.0 / 3628800.0;
        sum = sum * -r2 + 1.0 / 40320.0;
        sum = sum * -r2 + 1.0 / 720.0;
        sum = sum * -r2 + 1.0 / 24.0;
        sum = sum * -r2 + 1.0 / 2.0;
        return 1.0 - r2 * sum;
    }

    private static double Exp(double power)
    {
        if (double.IsNaN(power)) return double.NaN;
        if (power > 709.78) return double.PositiveInfinity;
        if (power < -745.2) return 0.0;

        // e^x = 2^k e^r, r within ln 2 / 2, by Taylor's series to the 14th power, below 1e-18 off.
        var k = Math.Round(power / Ln2);
        var r = power - k * Ln2High - k * Ln2Low;
        var sum = 1.0 / 87178291200.0;
        sum = sum * r + 1.0 / 6227020800.0;
        sum = sum * r + 1.0 / 479001600.0;
        sum = sum * r + 1.0 / 39916800.0;
        sum = sum * r + 1.0 / 3628800.0;
        sum = sum * r + 1.0 / 362880.0;
        sum = sum * r + 1.0 / 40320.0;
        sum = sum * r + 1.0 / 5040.0;
        sum = sum * r + 1.0 / 720.0;
        sum = sum * r + 1.0 / 120.0;
        sum = sum * r + 1.0 / 24.0;
        sum = sum * r + 1.0 / 6.0;
        sum = sum * r + 1.0 / 2.0;
        sum = sum * r + 1.0;
        return Math.ScaleB(sum * r + 1.0, (int)k);
    }

    // The natural logarithm of a positive finite number.
    private static double Log(double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value);
        var exponent = (int)((bits >> 52) & 0x7FF);
        if (exponent == 0)
        {
            bits = BitConverter.DoubleToInt64Bits(value * 18014398509481984.0);
            exponent = (int)((bits >> 52) & 0x7FF) - 54;
        }

        // value = m 2^e with m from √½ to √2, and ln m = 2 atanh s for s = (m - 1) / (m + 1), by
        // its series to the 25th power of s, below 1e-19 off where s is within 0.172.
        var e = exponent - 1023;
        var m = BitConverter.Int64BitsToDouble((bits & ((1L << 52) - 1)) | (1023L << 52));
        if (m > 1.4142135623730951)
        {
            m *= 0.5;
            e += 1;
        }

        var s = (m - 1.0) / (m + 1.0);
        var s2 = s * s;
        var sum = 1.0 / 25.0;
        for (var n = 23; n >= 1; n -= 2) sum = sum * s2 + 1.0 / n;
        return e * Ln2High + (2.0 * s * sum + e * Ln2Low);
    }

    private static double Pow(double value, double power)
    {
        if (power == 0.0 || value == 1.0) return 1.0;
        if (double.IsNaN(value) || double.IsNaN(power)) return double.NaN;

        var odd = IsOddInteger(power);
        if (double.IsInfinity(power))
        {
            var magnitude = Math.Abs(value);
            if (magnitude == 1.0) return 1.0;
            return (magnitude > 1.0) == (power > 0.0) ? double.PositiveInfinity : 0.0;
        }

        if (value == 0.0)
        {
            var zero = power > 0.0 ? 0.0 : double.PositiveInfinity;
            return odd && double.IsNegative(value) ? -zero : zero;
        }

        var sign = 1.0;
        if (value < 0.0)
        {
            if (Math.Floor(power) != power) return double.NaN;
            sign = odd ? -1.0 : 1.0;
            value = -value;
        }

        if (double.IsPositiveInfinity(value)) return sign * (power > 0.0 ? double.PositiveInfinity : 0.0);
        return sign * Exp(power * Log(value));
    }

    private static bool IsOddInteger(double value) =>
        double.IsFinite(value) && Math.Floor(value) == value && Math.Abs(value) < 9007199254740992.0 && Math.Abs(value % 2.0) == 1.0;

    private static double Atan2(double y, double x)
    {
        if (double.IsNaN(y) || double.IsNaN(x)) return double.NaN;
        if (y == 0.0)
        {
            if (x > 0.0 || (x == 0.0 && !double.IsNegative(x))) return y;
            return double.CopySign(Math.PI, y);
        }

        if (double.IsInfinity(x))
        {
            if (double.IsInfinity(y)) return double.CopySign(x > 0.0 ? Math.PI / 4.0 : 3.0 * Math.PI / 4.0, y);
            return x > 0.0 ? double.CopySign(0.0, y) : double.CopySign(Math.PI, y);
        }

        if (double.IsInfinity(y) || x == 0.0) return double.CopySign(Math.PI / 2.0, y);
        var angle = Atan(Math.Abs(y / x));
        return double.CopySign(x > 0.0 ? angle : Math.PI - angle, y);
    }

    // The arc tangent of a number at or above zero, brought within tan(π/12) by its reciprocal and
    // by the turn of π/6, and summed by its series to the 31st power, below 1e-19 off there.
    private static double Atan(double t)
    {
        if (double.IsPositiveInfinity(t)) return Math.PI / 2.0;
        var inverted = t > 1.0;
        if (inverted) t = 1.0 / t;
        var turned = t > TanPiOver12;
        if (turned) t = (t * Sqrt3 - 1.0) / (t + Sqrt3);

        // t - t³/3 + t⁵/5 and on, as t times a sum in t² from its last term.
        var t2 = t * t;
        var sum = -1.0 / 31.0;
        for (var term = 14; term >= 0; term--) sum = sum * t2 + (term % 2 == 0 ? 1.0 : -1.0) / (2 * term + 1);
        var angle = t * sum;
        if (turned) angle += Math.PI / 6.0;
        return inverted ? Math.PI / 2.0 - angle : angle;
    }
}
