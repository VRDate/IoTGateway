﻿using System;
using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Curve448, as defined in RFC 7748:
    /// https://tools.ietf.org/html/rfc7748
    /// </summary>
    public class Curve448 : MontgomeryCurve
    {
        private static readonly BigInteger p0 = BigInteger.Pow(2, 448) - BigInteger.Pow(2, 224) - 1;
        private static readonly BigInteger A = 156326;
        private static readonly BigInteger A24 = (A - 2) / 4;
        private static readonly BigInteger n = BigInteger.Pow(2, 446) - BigInteger.Parse("8335dc163bb124b65129c96fde933d8d723a70aadc873d6d54a7bb0d", NumberStyles.HexNumber);
        private const int cofactor = 4;
        private static readonly BigInteger BasePointU = 5;
        private static readonly BigInteger BasePointV = BigInteger.Parse("355293926785568175264127502063783334808976399387714271831880898435169088786967410002932673765864550910142774147268105838985595290606362");

        /// <summary>
        /// Curve448, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        public Curve448()
            : base(p0, A, new PointOnCurve(BasePointU, BasePointV), n, cofactor)
        {
        }

        /// <summary>
        /// Curve448, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        /// <param name="D">Private key.</param>
        public Curve448(BigInteger D)
            : base(p0, A, new PointOnCurve(BasePointU, BasePointV), n, cofactor, D)
        {
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Curve448";

        /// <summary>
        /// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
        /// in the birational Edwards curve.
        /// </summary>
        /// <param name="UV">(U,V) coordinates.</param>
        /// <returns>(X,Y) coordinates.</returns>
        public override PointOnCurve ToXY(PointOnCurve UV)
        {
            BigInteger U2 = this.Multiply(UV.X, UV.X);
            BigInteger U3 = this.Multiply(U2, UV.X);
            BigInteger U4 = this.Multiply(U2, U2);
            BigInteger U5 = this.Multiply(U3, U2);
            BigInteger V2 = this.Multiply(UV.Y, UV.Y);
            BigInteger X = this.Divide(this.Multiply(4 * UV.Y, U2 - 1),
                (U4 - 2 * U2 + 4 * V2 + BigInteger.One));
            BigInteger Y = this.Divide(-(U5 - 2 * U3 - 4 * UV.X * V2 + UV.X),
               (U5 - 2 * U2 * V2 - 2 * U3 - 2 * V2 + UV.X));

            if (X.Sign < 0)
                X += this.p;

            if (Y.Sign < 0)
                Y += this.p;

            return new PointOnCurve(X, Y);
        }

        /// <summary>
        /// Converts a pair of (X,Y) coordinates for the birational Edwards curve
        /// to a pair of (U,V) coordinates.
        /// </summary>
        /// <param name="XY">(X,Y) coordinates.</param>
        /// <returns>(U,V) coordinates.</returns>
        public override PointOnCurve ToUV(PointOnCurve XY)
        {
            BigInteger X2 = this.Multiply(XY.X, XY.X);
            BigInteger Y2 = this.Multiply(XY.Y, XY.Y);
            BigInteger U = this.Divide(Y2, X2);
            BigInteger X3 = this.Multiply(XY.X, X2);
            BigInteger V = this.Divide(this.Multiply(Two - X2 - Y2, XY.Y), X3);

            if (U.Sign < 0)
                U += this.p;

            if (V.Sign < 0)
                V += this.p;

            return new PointOnCurve(U, V);
        }

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="U">U-coordinate of point</param>
        /// <returns><paramref name="N"/>*<paramref name="U"/></returns>
        public override BigInteger ScalarMultiplication(BigInteger N, BigInteger U)
        {
            return XFunction(N, U, A24, this.p, 448, 0xfc, 0x7f, 0x80);
        }

        /// <summary>
        /// Creates the Edwards Curve pair.
        /// </summary>
        /// <returns>Edwards curve.</returns>
        public override EdwardsCurve CreatePair()
        {
            PointOnCurve PublicKeyUV = this.PublicKey;
            PointOnCurve PublicKeyXY = this.ToXY(PublicKeyUV);

            byte[] Bin = this.privateKey.ToByteArray();
            if (Bin.Length != 57)
                Array.Resize<byte>(ref Bin, 57);

            Bin[0] &= 0xfc;
            Bin[55] |= 0x80;
            Bin[56] = 0;

            BigInteger PrivateKey = new BigInteger(Bin);
            BigInteger PrivateKey2 = BigInteger.Remainder(this.Order - PrivateKey, this.Order);
            if (PrivateKey2.Sign < 0)
                PrivateKey2 += this.Order;

            Edwards448 Candidate = new Edwards448(PrivateKey2);
            PointOnCurve PublicKeyXY2 = Candidate.PublicKey;

            if (PublicKeyXY.Y.Equals(PublicKeyXY2.Y))
                return Candidate;

            Candidate = new Edwards448(PrivateKey);
            PublicKeyXY2 = Candidate.PublicKey;

            if (PublicKeyXY.Y.Equals(PublicKeyXY2.Y))
                return Candidate;

            throw new InvalidOperationException("Unable to create pair curve.");
        }

    }
}
