using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap.Utilities
{
    public static class RectangleUtility
    {
        public static RectangleF ToRectangleF(this Envelope e)
        {
            //new RectangleF((float)envelope.MinX, (float)envelope.MinY, (float)envelope.Width, (float)envelope.Height);
            return RectangleF.FromLTRB(ToLower(e.MinX), ToLower(e.MinY), ToUpper(e.MaxX), ToUpper(e.MaxY));
        }

        private static float ToLower(double d)
        {
            float f = (float) d;
            if (f <= d) return f;
            return nextafter(f, float.MinValue);
        }
        private static float ToUpper(double d)
        {
            float f = (float)d;
            if (f >= d) return f;
            return nextafter(f, float.MaxValue);
        }

        /// <summary>
        /// Gets the floating-point number that is next after <paramref name="fromNumber"/> in the direction of <paramref name="towardNumber"/>.
        /// </summary>
        /// <param name="fromNumber">A floating-point number.</param>
        /// <param name="towardNumber">A floating-point number.</param>
        /// <returns>The floating-point number that is next after <paramref name="fromNumber"/> in the direction of <paramref name="towardNumber"/>.</returns>
        /// <remarks>
        /// <para>
        /// IEC 60559 recommends that <paramref name="fromNumber"/> be returned whenever <c><paramref name="fromNumber"/> == <paramref name="towardNumber"/></c>.
        /// These functions return <paramref name="towardNumber"/> instead, which makes the behavior around zero consistent: <c><see cref="math.nextafter(float, float)">nextafter</see>(-0.0, +0.0)</c>
        /// returns <c>+0.0</c> and <c><see cref="math.nextafter(float, float)">nextafter</see>(+0.0, -0.0)</c> returns <c>–0.0</c>.
        /// </para>
        /// <para>
        /// See <a href="http://en.cppreference.com/w/c/numeric/math/nextafter">nextafter</a> in the C standard documentation.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code language="C#">
        /// Assert.IsTrue(math.nextafter(0F, 0F) == 0F);
        /// Assert.IsTrue(math.nextafter(-0F, 0F) == 0F;
        /// Assert.IsTrue(math.nextafter(0F, -0F) == -0F);
        /// 
        /// Assert.IsTrue(math.nextafter(math.FLT_MIN, 0D) == math.FLT_DENORM_MAX);
        /// Assert.IsTrue(math.nextafter(math.FLT_DENORM_MIN, 0F) == 0F);
        /// Assert.IsTrue(math.nextafter(math.FLT_MIN, -0F) == math.FLT_DENORM_MAX);
        /// Assert.IsTrue(math.nextafter(math.FLT_DENORM_MIN, -0F) == 0F);
        /// 
        /// Assert.IsTrue(math.nextafter(0F, System.Single.PositiveInfinity) == math.FLT_DENORM_MIN);
        /// Assert.IsTrue(math.nextafter(-0F, System.Single.NegativeInfinity) == -math.FLT_DENORM_MIN);
        /// </code> 
        /// <code language="VB.NET">
        /// Assert.IsTrue(math.nextafter(0F, 0F) = 0F);
        /// Assert.IsTrue(math.nextafter(-0F, 0F) = 0F);
        /// Assert.IsTrue(math.nextafter(0F, -0F) = -0F);
        /// 
        /// Assert.IsTrue(math.nextafter(math.FLT_MIN, 0F) = math.FLT_DENORM_MAX);
        /// Assert.IsTrue(math.nextafter(math.FLT_DENORM_MIN, 0F) = 0F);
        /// Assert.IsTrue(math.nextafter(math.FLT_MIN, -0F) = math.FLT_DENORM_MAX);
        /// Assert.IsTrue(math.nextafter(math.FLT_DENORM_MIN, -0F) = 0F);
        /// 
        /// Assert.IsTrue(math.nextafter(0F, System.Single.PositiveInfinity) = math.FLT_DENORM_MIN);
        /// Assert.IsTrue(math.nextafter(-0F, System.Single.NegativeInfinity) = -math.FLT_DENORM_MIN);
        /// </code> 
        /// </example>
        private static float nextafter(float fromNumber, float towardNumber)
        {
            // If either fromNumber or towardNumber is NaN, return NaN.
            if (System.Single.IsNaN(towardNumber) || System.Single.IsNaN(fromNumber))
            {
                return System.Single.NaN;
            }
            // If no direction or if fromNumber is infinity or is not a number, return fromNumber.
            if (fromNumber == towardNumber)
            {
                return towardNumber;
            }
            // If fromNumber is zero, return smallest subnormal.
            if (fromNumber == 0)
            {
                return (towardNumber > 0) ? System.Single.Epsilon : -System.Single.Epsilon;
            }
            // All other cases are handled by incrementing or decrementing the bits value.
            // Transitions to infinity, to subnormal, and to zero are all taken care of this way.
            int bits = SingleToInt32Bits(fromNumber);
            // A xor here avoids nesting conditionals. We have to increment if fromValue lies between 0 and toValue.
            if ((fromNumber > 0) ^ (fromNumber > towardNumber))
            {
                bits += 1;
            }
            else
            {
                bits -= 1;
            }
            return Int32BitsToSingle(bits);
        }

        /// <summary>
        /// Converts the specified single-precision floating point number to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 32-bit signed integer whose value is equivalent to <paramref name="value"/>.</returns>
        private static unsafe int SingleToInt32Bits(float value)
        {
            return *((int*)&value);
        }

        /// <summary>
        /// Converts the specified 32-bit signed integer to a single-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A double-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
        private static unsafe float Int32BitsToSingle(int value)
        {
            return *((float*)&value);
        }

    }
}
