using System;
using System.Runtime.CompilerServices;

namespace WinGL.Util
{
    /// <summary>
    /// Represents a 4x4 matrix.
    /// </summary>
    public struct Mat4
    {
        private static Mat4 _matCache = Identity();

        /// <summary>
        /// The columms of the matrix.
        /// </summary>
        private Vector4[] _cols;

        #region Construction

        public Mat4(Mat4 mat)
        {
            _cols = new[]
            {
                new Vector4(mat[0][0], mat[0][1], mat[0][2], mat[0][3]),
                new Vector4(mat[1][0], mat[1][1], mat[1][2], mat[1][3]),
                new Vector4(mat[2][0], mat[2][1], mat[2][2], mat[2][3]),
                new Vector4(mat[3][0], mat[3][1], mat[3][2], mat[3][3])
            };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="mat4"/> struct.
        /// This matrix is the identity matrix scaled by <paramref name="scale"/>.
        /// </summary>
        /// <param name="scale">The scale.</param>
        public Mat4(float scale)
        {
            _cols = new[]
            {
                new Vector4(scale, 0, 0, 0),
                new Vector4(0, scale, 0, 0),
                new Vector4(0, 0, scale, 0),
                new Vector4(0, 0, 0, scale),
            };
        }

        public Mat4(float m00, float m11, float m22, float m33)
        {
            _cols = new[]
            {
                new Vector4(m00, 0, 0, 0),
                new Vector4(0, m11, 0, 0),
                new Vector4(0, 0, m22, 0),
                new Vector4(0, 0, 0, m33),
            };
        }

        /// <summary>
        /// Создать матрицу сразу со смещением
        /// </summary>
        public Mat4(float x, float y, float z)
        {
            _cols = new[]
            {
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(x, y, z, 1),
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="mat4"/> struct.
        /// The matrix is initialised with the <paramref name="cols"/>.
        /// </summary>
        /// <param name="cols">The colums of the matrix.</param>
        public Mat4(Vector4[] cols)
        {
            _cols = new[]
            {
                cols[0],
                cols[1],
                cols[2],
                cols[3]
            };
        }

        public Mat4(Vector4 a, Vector4 b, Vector4 c, Vector4 d)
        {
            _cols = new[]
            {
                a, b, c, d
            };
        }

        /// <summary>
        /// Creates an identity matrix.
        /// </summary>
        /// <returns>A new identity matrix.</returns>
        public static Mat4 Identity()
        {
            return new Mat4
            {
                _cols = new[]
                {
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1)
                }
            };
        }

        public void Clear()
        {
            _cols[0].X = _cols[1].Y = _cols[2].Z = _cols[3].W = 1;
            _cols[0].Y = _cols[0].Z = _cols[0].W =
            _cols[1].X  = _cols[1].Z = _cols[1].W =
            _cols[2].X = _cols[2].Y = _cols[2].W =
            _cols[3].X = _cols[3].Y = _cols[3].Z = 0;
        }

        /// <summary>
        /// Копировать текущую матрицу в матрицу m
        /// </summary>
        public void Copy(Mat4 m)
        {
            m._cols[0] = _cols[0];
            m._cols[1] = _cols[1];
            m._cols[2] = _cols[2];
            m._cols[3] = _cols[3];
        }

        #endregion

        #region Index Access

        /// <summary>
        /// Gets or sets the <see cref="vec4"/> column at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="vec4"/> column.
        /// </value>
        /// <param name="column">The column index.</param>
        /// <returns>The column at index <paramref name="column"/>.</returns>
        public Vector4 this[int column]
        {
            get { return _cols[column]; }
            set { _cols[column] = value; }
        }

        /// <summary>
        /// Gets or sets the element at <paramref name="column"/> and <paramref name="row"/>.
        /// </summary>
        /// <value>
        /// The element at <paramref name="column"/> and <paramref name="row"/>.
        /// </value>
        /// <param name="column">The column index.</param>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The element at <paramref name="column"/> and <paramref name="row"/>.
        /// </returns>
        public float this[int column, int row]
        {
            get { return _cols[column][row]; }
            set { _cols[column][row] = value; }
        }

        #endregion

        #region Transform

        /// <summary>
        /// Вращение матрицы по кватерниону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateQuat(float x, float y, float z, float w)
        {
            float wx, wy, wz, xx, yy, yz, xy, xz, zz, x2, y2, z2;

            x2 = x + x;
            y2 = y + y;
            z2 = z + z;
            xx = x * x2;
            xy = x * y2;
            xz = x * z2;
            yy = y * y2;
            yz = y * z2;
            zz = z * z2;
            wx = w * x2;
            wy = w * y2;
            wz = w * z2;

            _matCache.Clear();
            _matCache[0, 0] = 1f - (yy + zz);
            _matCache[0, 1] = xy + wz;
            _matCache[0, 2] = xz - wy;
            _matCache[1, 0] = xy - wz;
            _matCache[1, 1] = 1f - (xx + zz);
            _matCache[1, 2] = yz + wx;
            _matCache[2, 0] = xz + wy;
            _matCache[2, 1] = yz - wx;
            _matCache[2, 2] = 1f - (xx + yy);
            this *= _matCache;
        }
        /// <summary>
        /// Вращение матрицы по кватерниону
        /// </summary>
        public void RotateQuat(Vector4 quaternion)
            => RotateQuat(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

        /// <summary>
        /// Вращение текущей матрицы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rotate(float angle, float x, float y, float z)
        {
            float c = Glm.Cos(angle);
            float cm = 1 - c;
            float sz = Glm.Sin(angle);

            float tx = cm * x;
            float ty = cm * y;
            float tz = cm * z;

            float sx = sz * x;
            float sy = sz * y;
            sz *= z;

            Vector4 v0 = _cols[0] * (c + tx * x) + _cols[1] * (tx * y + sz) + _cols[2] * (tx * z - sy);
            Vector4 v1 = _cols[0] * (ty * x - sz) + _cols[1] * (c + ty * y) + _cols[2] * (ty * z + sx);
            _cols[2] = _cols[0] * (tz * x + sy) + _cols[1] * (tz * y - sx) + _cols[2] * (c + tz * z);
            _cols[0] = v0;
            _cols[1] = v1;
        }

        /// <summary>
        /// Вращение текущей матрицы по оси X
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateX(float pitch)
        {
            float c = Glm.Cos(pitch);
            float s = Glm.Sin(pitch);

            Vector4 v = _cols[1] * c + _cols[2] * s;
            _cols[2] = _cols[1] * -s + _cols[2] * c;
            _cols[1] = v;
        }

        /// <summary>
        /// Вращение текущей матрицы по оси Y
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateY(float yaw)
        {
            float c = Glm.Cos(yaw);
            float s = Glm.Sin(yaw);

            Vector4 v = _cols[0] * c + _cols[2] * -s;
            _cols[2] = _cols[0] * s + _cols[2] * c;
            _cols[0] = v;
        }

        /// <summary>
        /// Вращение текущей матрицы по оси Z
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateZ(float roll)
        {
            float c = Glm.Cos(roll);
            float s = Glm.Sin(roll);

            Vector4 v = _cols[0] * c + _cols[1] * s;
            _cols[1] = _cols[0] * -s + _cols[1] * c;
            _cols[0] = v;
        }

        /// <summary>
        /// Вращение текущей матрицы по очерёдности XYZ
        /// </summary>
        /// <param name="yaw">По оси Y</param>
        /// <param name="pitch">По оси X</param>
        /// <param name="roll">По оси Z</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateXYZ(float yaw, float pitch, float roll)
        {
            if (pitch != 0) RotateX(pitch);
            if (yaw != 0) RotateY(yaw);
            if (roll != 0) RotateZ(roll);
        }

        /// <summary>
        /// Вращение текущей матрицы по очерёдности ZYX
        /// </summary>
        /// <param name="yaw">По оси Y</param>
        /// <param name="pitch">По оси X</param>
        /// <param name="roll">По оси Z</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateZYX(float yaw, float pitch, float roll)
        {
            if (roll != 0) RotateZ(roll); 
            if (yaw != 0) RotateY(yaw);
            if (pitch != 0) RotateX(pitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Translate(float x, float y, float z)
            => _cols[3] += _cols[0] * x + _cols[1] * y + _cols[2] * z;

        #endregion

        #region Conversion

        /// <summary>
        /// Returns the matrix as a flat array of elements, column major.
        /// </summary>
        public float[] ToArray() => new float[]
        {
            _cols[0].X,
            _cols[0].Y,
            _cols[0].Z,
            _cols[0].W,
            _cols[1].X,
            _cols[1].Y,
            _cols[1].Z,
            _cols[1].W,
            _cols[2].X,
            _cols[2].Y,
            _cols[2].Z,
            _cols[2].W,
            _cols[3].X,
            _cols[3].Y,
            _cols[3].Z,
            _cols[3].W
        };
            //=> _cols.SelectMany(v => v.ToArray()).ToArray();

        /// <summary>
        /// Вернуть массив элементов матрицы, 4*3
        /// </summary>
        public float[] ToArray4x3() => new float[]
        {
            _cols[0].X,
            _cols[0].Y,
            _cols[0].Z,
            _cols[1].X,
            _cols[1].Y,
            _cols[1].Z,
            _cols[2].X,
            _cols[2].Y,
            _cols[2].Z,
            _cols[3].X,
            _cols[3].Y,
            _cols[3].Z
        };

        /// <summary>
        /// Передать все данные в входящий массив не создавая ни структур ни массивов
        /// </summary>
        public void ConvArray4x3(float[] array, int offset)
        {
            //Buffer.BlockCopy(_cols[0].ToArray3(), 0, array, offset * 4, 12);

            array[offset] = _cols[0].X;
            array[offset + 1] = _cols[0].Y;
            array[offset + 2] = _cols[0].Z;
            array[offset + 3] = _cols[1].X;
            array[offset + 4] = _cols[1].Y;
            array[offset + 5] = _cols[1].Z;
            array[offset + 6] = _cols[2].X;
            array[offset + 7] = _cols[2].Y;
            array[offset + 8] = _cols[2].Z;
            array[offset + 9] = _cols[3].X;
            array[offset + 10] = _cols[3].Y;
            array[offset + 11] = _cols[3].Z;
        }

        /// <summary>
        /// Передать все данные в входящий массив не создавая ни структур ни массивов
        /// </summary>
        public void ConvArray(float[] array)
        {
            array[0] = _cols[0].X;
            array[1] = _cols[0].Y;
            array[2] = _cols[0].Z;
            array[3] = _cols[0].W;
            array[4] = _cols[1].X;
            array[5] = _cols[1].Y;
            array[6] = _cols[1].Z;
            array[7] = _cols[1].W;
            array[8] = _cols[2].X;
            array[9] = _cols[2].Y;
            array[10] = _cols[2].Z;
            array[11] = _cols[2].W;
            array[12] = _cols[3].X;
            array[13] = _cols[3].Y;
            array[14] = _cols[3].Z;
            array[15] = _cols[3].W;
        }

        #endregion

        #region Multiplication

        /// <summary>
        /// Произведение текущей матрица на заданную, и результат идёт в текущую
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(Mat4 rhs)
        {
            _matCache[0] = rhs[0][0] * _cols[0] + rhs[0][1] * _cols[1] + rhs[0][2] * _cols[2] + rhs[0][3] * _cols[3];
            _matCache[1] = rhs[1][0] * _cols[0] + rhs[1][1] * _cols[1] + rhs[1][2] * _cols[2] + rhs[1][3] * _cols[3];
            _matCache[2] = rhs[2][0] * _cols[0] + rhs[2][1] * _cols[1] + rhs[2][2] * _cols[2] + rhs[2][3] * _cols[3];
            _cols[3] = rhs[3][0] * _cols[0] + rhs[3][1] * _cols[1] + rhs[3][2] * _cols[2] + rhs[3][3] * _cols[3];
            _cols[0] = _matCache[0];
            _cols[1] = _matCache[1];
            _cols[2] = _matCache[2];
        }

        /// <summary>
        /// Multiplies the <paramref name="lhs"/> matrix by the <paramref name="rhs"/> vector.
        /// </summary>
        /// <param name="lhs">The LHS matrix.</param>
        /// <param name="rhs">The RHS vector.</param>
        /// <returns>The product of <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Mat4 lhs, Vector4 rhs)
        {
            return new Vector4(
                lhs[0, 0] * rhs[0] + lhs[1, 0] * rhs[1] + lhs[2, 0] * rhs[2] + lhs[3, 0] * rhs[3],
                lhs[0, 1] * rhs[0] + lhs[1, 1] * rhs[1] + lhs[2, 1] * rhs[2] + lhs[3, 1] * rhs[3],
                lhs[0, 2] * rhs[0] + lhs[1, 2] * rhs[1] + lhs[2, 2] * rhs[2] + lhs[3, 2] * rhs[3],
                lhs[0, 3] * rhs[0] + lhs[1, 3] * rhs[1] + lhs[2, 3] * rhs[2] + lhs[3, 3] * rhs[3]
            );
        }

        static Mat4 _mm = Mat4.Identity();
        /// <summary>
        /// Multiplies the <paramref name="lhs"/> matrix by the <paramref name="rhs"/> matrix.
        /// </summary>
        /// <param name="lhs">The LHS matrix.</param>
        /// <param name="rhs">The RHS matrix.</param>
        /// <returns>The product of <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mat4 operator *(Mat4 lhs, Mat4 rhs)
        {
            return new Mat4(new[]
            {
                rhs[0][0] * lhs[0] + rhs[0][1] * lhs[1] + rhs[0][2] * lhs[2] + rhs[0][3] * lhs[3],
                rhs[1][0] * lhs[0] + rhs[1][1] * lhs[1] + rhs[1][2] * lhs[2] + rhs[1][3] * lhs[3],
                rhs[2][0] * lhs[0] + rhs[2][1] * lhs[1] + rhs[2][2] * lhs[2] + rhs[2][3] * lhs[3],
                rhs[3][0] * lhs[0] + rhs[3][1] * lhs[1] + rhs[3][2] * lhs[2] + rhs[3][3] * lhs[3]
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mat4 operator *(Mat4 lhs, float s)
        {
            return new Mat4(new[]
            {
                lhs[0]*s,
                lhs[1]*s,
                lhs[2]*s,
                lhs[3]*s
            });
        }

        #endregion

        #region ToString support

        public override string ToString()
        {
            return string.Format(
                "[{0}, {1}, {2}, {3}; {4}, {5}, {6}, {7}; {8}, {9}, {10}, {11}; {12}, {13}, {14}, {15}]",
                this[0, 0], this[1, 0], this[2, 0], this[3, 0],
                this[0, 1], this[1, 1], this[2, 1], this[3, 1],
                this[0, 2], this[1, 2], this[2, 2], this[3, 2],
                this[0, 3], this[1, 3], this[2, 3], this[3, 3]
            );
        }
        #endregion

        #region Comparision
        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// The Difference is detected by the different values
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Mat4))
            {
                var mat = (Mat4)obj;
                if (mat[0] == this[0] && mat[1] == this[1] && mat[2] == this[2] && mat[3] == this[3])
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="m1">The first Matrix.</param>
        /// <param name="m2">The second Matrix.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Mat4 m1, Mat4 m2)
        {
            return m1.Equals(m2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="m1">The first Matrix.</param>
        /// <param name="m2">The second Matrix.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Mat4 m1, Mat4 m2)
        {
            return !m1.Equals(m2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this[0].GetHashCode() ^ this[1].GetHashCode() ^ this[2].GetHashCode() ^ this[3].GetHashCode();
        }
        #endregion
    }
}
