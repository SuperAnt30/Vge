namespace WinGL.Util
{
    public static partial class Glm
    {
        public static Mat4 Inverse(Mat4 m)
        {
            float coef00 = m[2][2] * m[3][3] - m[3][2] * m[2][3];
            float coef02 = m[1][2] * m[3][3] - m[3][2] * m[1][3];
            float coef03 = m[1][2] * m[2][3] - m[2][2] * m[1][3];

            float coef04 = m[2][1] * m[3][3] - m[3][1] * m[2][3];
            float coef06 = m[1][1] * m[3][3] - m[3][1] * m[1][3];
            float coef07 = m[1][1] * m[2][3] - m[2][1] * m[1][3];

            float coef08 = m[2][1] * m[3][2] - m[3][1] * m[2][2];
            float coef10 = m[1][1] * m[3][2] - m[3][1] * m[1][2];
            float coef11 = m[1][1] * m[2][2] - m[2][1] * m[1][2];

            float coef12 = m[2][0] * m[3][3] - m[3][0] * m[2][3];
            float coef14 = m[1][0] * m[3][3] - m[3][0] * m[1][3];
            float coef15 = m[1][0] * m[2][3] - m[2][0] * m[1][3];

            float coef16 = m[2][0] * m[3][2] - m[3][0] * m[2][2];
            float coef18 = m[1][0] * m[3][2] - m[3][0] * m[1][2];
            float coef19 = m[1][0] * m[2][2] - m[2][0] * m[1][2];

            float coef20 = m[2][0] * m[3][1] - m[3][0] * m[2][1];
            float coef22 = m[1][0] * m[3][1] - m[3][0] * m[1][1];
            float coef23 = m[1][0] * m[2][1] - m[2][0] * m[1][1];

            Vector4 fac0 = new Vector4(coef00, coef00, coef02, coef03);
            Vector4 fac1 = new Vector4(coef04, coef04, coef06, coef07);
            Vector4 fac2 = new Vector4(coef08, coef08, coef10, coef11);
            Vector4 fac3 = new Vector4(coef12, coef12, coef14, coef15);
            Vector4 fac4 = new Vector4(coef16, coef16, coef18, coef19);
            Vector4 fac5 = new Vector4(coef20, coef20, coef22, coef23);

            Vector4 vec0 = new Vector4(m[1][0], m[0][0], m[0][0], m[0][0]);
            Vector4 vec1 = new Vector4(m[1][1], m[0][1], m[0][1], m[0][1]);
            Vector4 vec2 = new Vector4(m[1][2], m[0][2], m[0][2], m[0][2]);
            Vector4 vec3 = new Vector4(m[1][3], m[0][3], m[0][3], m[0][3]);

            Vector4 inv0 = new Vector4(vec1 * fac0 - vec2 * fac1 + vec3 * fac2);
            Vector4 inv1 = new Vector4(vec0 * fac0 - vec2 * fac3 + vec3 * fac4);
            Vector4 inv2 = new Vector4(vec0 * fac1 - vec1 * fac3 + vec3 * fac5);
            Vector4 inv3 = new Vector4(vec0 * fac2 - vec1 * fac4 + vec2 * fac5);

            Vector4 signA = new Vector4(+1, -1, +1, -1);
            Vector4 signB = new Vector4(-1, +1, -1, +1);
            Mat4 inverse = new Mat4(inv0 * signA, inv1 * signB, inv2 * signA, inv3 * signB);

            Vector4 row0 = new Vector4(inverse[0][0], inverse[1][0], inverse[2][0], inverse[3][0]);

            Vector4 dot0 = new Vector4(m[0] * row0);
            float dot1 = dot0.X + dot0.Y + dot0.Z + dot0.W;

            float oneOverDeterminant = 1f / dot1;

            return inverse * oneOverDeterminant;
        }
    }
}
