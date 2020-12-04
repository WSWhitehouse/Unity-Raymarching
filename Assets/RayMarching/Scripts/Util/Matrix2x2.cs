namespace WSWhitehouse.Util
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Matrix2X2
    {
        public float m00, m01, m10, m11;

        public Matrix2X2()
        {
            SetValue(1, 0, 0, 1);
        }

        public Matrix2X2(float m00, float m01, float m10, float m11)
        {
            SetValue(m00, m01, m10, m11);
        }

        public Matrix2X2(Matrix2X2 m)
        {
            SetValue(m[0, 0], m[0, 1], m[1, 0], m[1, 1]);
        }

        public Matrix2X2(float m00, float m11)
        {
            // Diagonal
            SetValue(m00, 0, 0, m11);
        }

        public void LoadIdentity()
        {
            SetValue(1, 0, 0, 1);
        }

        public void SetValue(float m00, float m01, float m10, float m11)
        {
            this[0, 0] = m00;
            this[0, 1] = m01;
            this[1, 0] = m10;
            this[1, 1] = m11;
        }

        public void SetValue(float m00, float m11)
        {
            SetValue(m00, 0, 0, m11);
        }

        public void SetValue(Matrix2X2 m)
        {
            SetValue(m[0, 0], m[0, 1], m[1, 0], m[1, 1]);
        }

        public void SetValue(float value)
        {
            SetValue(value, value, value, value);
        }

        public void Normalize()
        {
            for (int row = 0; row < 2; row++)
            {
                float l = 0;
                for (int column = 0; column < 2; column++)
                {
                    l += this[row, column] * this[row, column];
                }

                l = Mathf.Sqrt(l);

                for (int column = 0; column < 2; column++)
                {
                    this[row, column] /= l;
                }
            }
        }

        public float Determinant()
        {
            return this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];
        }

        public Matrix2X2 Transpose()
        {
            return new Matrix2X2(this[0, 0], this[1, 0], this[0, 1], this[1, 1]);
        }

        public Matrix2X2 Inverse()
        {
            float det = Determinant();
            return new Matrix2X2(this[1, 1] / det, -this[0, 1] / det, -this[1, 0] / det, this[0, 0] / det);
        }

        public Matrix2X2 Cofactor()
        {
            return new Matrix2X2(this[1, 1], -this[1, 0], -this[0, 1], this[0, 0]);
        }

        public float FrobeniusInnerProduct(Matrix2X2 m)
        {
            float prod = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    prod += this[i, j] * m[i, j];
                }
            }

            return prod;
        }

        /// <summary>
        /// Singular Value Decomposition
        /// </summary>
        /// <param name="w">Returns rotation matrix</param>
        /// <param name="e">Returns sigma matrix</param>
        /// <param name="v">Returns (not transposed)</param>
        public void Svd(ref Matrix2X2 w, ref Matrix2X2 e, ref Matrix2X2 v)
        {
            // If it is diagonal, SVD is trivial
            if (Mathf.Abs(this[1, 0] - this[0, 1]) < Mathf.Epsilon && Mathf.Abs(this[1, 0]) < Mathf.Epsilon)
            {
                w.SetValue(this[0, 0] < 0 ? -1 : 1, 0, 0, this[1, 1] < 0 ? -1 : 1);
                e.SetValue(Mathf.Abs(this[0, 0]), Mathf.Abs(this[1, 1]));
                v.LoadIdentity();
            }

            // Otherwise, we need to compute A^T*A
            else
            {
                float j = this[0, 0] * this[0, 0] + this[1, 0] * this[1, 0],
                    k = this[0, 1] * this[0, 1] + this[1, 1] * this[1, 1],
                    vC = this[0, 0] * this[0, 1] + this[1, 0] * this[1, 1];
                // Check to see if A^T*A is diagonal
                if (Mathf.Abs(vC) < Mathf.Epsilon)
                {
                    float s1 = Mathf.Sqrt(j), s2 = Mathf.Abs(j - k) < Mathf.Epsilon ? s1 : Mathf.Sqrt(k);
                    e.SetValue(s1, s2);
                    v.LoadIdentity();
                    w.SetValue(this[0, 0] / s1, this[0, 1] / s2, this[1, 0] / s1, this[1, 1] / s2);
                }
                // Otherwise, solve quadratic for eigenvalues
                else
                {
                    float jmk = j - k,
                        jpk = j + k,
                        root = Mathf.Sqrt(jmk * jmk + 4 * vC * vC),
                        eig = (jpk + root) / 2,
                        s1 = Mathf.Sqrt(eig),
                        s2 = Mathf.Abs(root) < Mathf.Epsilon ? s1 : Mathf.Sqrt((jpk - root) / 2);

                    e.SetValue(s1, s2);

                    // Use eigenvectors of A^T*A as V
                    float vS = eig - j, len = Mathf.Sqrt(vS * vS + vC * vC);
                    vC /= len;
                    vS /= len;
                    v.SetValue(vC, -vS, vS, vC);
                    // Compute w matrix as Av/s
                    w.SetValue(
                        (this[0, 0] * vC + this[0, 1] * vS) / s1,
                        (this[0, 1] * vC - this[0, 0] * vS) / s2,
                        (this[1, 0] * vC + this[1, 1] * vS) / s1,
                        (this[1, 1] * vC - this[1, 0] * vS) / s2
                    );
                }
            }
        }

        //DIAGONAL MATRIX OPERATIONS
        //Matrix * Matrix
        public void DiagProduct(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                    this[i, j] *= v[i];
            }
        }

        //Matrix * Matrix^-1
        public void DiagProductInv(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                    this[i, j] /= v[i];
            }
        }

        //Matrix - Matrix
        public void DiagDifference(float c)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] -= c;
        }

        public void DiagDifference(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] -= v[i];
        }

        //Matrix + Matrix
        public void DiagSum(float c)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] += c;
        }

        public void DiagSum(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] += v[i];
        }

        private static Matrix2X2 Identity()
        {
            return new Matrix2X2(1, 0, 0, 1);
        }

        // Array subscripts
        public float this[int row, int column]
        {
            get
            {
                switch (row)
                {
                    case 0 when column == 0:
                        return m00;
                    case 0 when column == 1:
                        return m01;
                    case 1 when column == 0:
                        return m10;
                    case 1 when column == 1:
                        return m11;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (row)
                {
                    case 0 when column == 0:
                        m00 = value;
                        break;
                    case 0 when column == 1:
                        m01 = value;
                        break;
                    case 1 when column == 0:
                        m10 = value;
                        break;
                    case 1 when column == 1:
                        m11 = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return m00;
                    case 1:
                        return m01;
                    case 2:
                        return m10;
                    case 3:
                        return m11;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        m00 = value;
                        break;
                    case 1:
                        m01 = value;
                        break;
                    case 2:
                        m10 = value;
                        break;
                    case 3:
                        m11 = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // Matrix - Scalar overloads
        public static Matrix2X2 operator +(Matrix2X2 l, float r)
        {
            Matrix2X2 result = new Matrix2X2(l);
            for (int index = 0; index < 4; index++)
            {
                result[index] += r;
            }

            return result;
        }

        public static Matrix2X2 operator +(float l, Matrix2X2 r)
        {
            Matrix2X2 result = new Matrix2X2(r);
            for (int index = 0; index < 4; index++)
            {
                result[index] += l;
            }

            return result;
        }

        public static Matrix2X2 operator -(Matrix2X2 l, float r)
        {
            Matrix2X2 result = new Matrix2X2(l);
            for (int index = 0; index < 4; index++)
            {
                result[index] -= r;
            }

            return result;
        }

        public static Matrix2X2 operator *(Matrix2X2 l, float r)
        {
            Matrix2X2 result = new Matrix2X2(l);
            for (int index = 0; index < 4; index++)
            {
                result[index] *= r;
            }

            return result;
        }

        public static Matrix2X2 operator *(float l, Matrix2X2 r)
        {
            Matrix2X2 result = new Matrix2X2(r);
            for (int index = 0; index < 4; index++)
            {
                result[index] *= l;
            }

            return result;
        }

        public static Matrix2X2 operator /(Matrix2X2 l, float r)
        {
            Matrix2X2 result = new Matrix2X2(l);
            for (int index = 0; index < 4; index++)
            {
                result[index] /= r;
            }

            return result;
        }

        // Matrix - Matrix overloads
        public static Matrix2X2 operator +(Matrix2X2 l, Matrix2X2 r)
        {
            Matrix2X2 result = new Matrix2X2(l);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {
                    result[row, column] += r[row, column];
                }
            }

            return result;
        }

        public static Matrix2X2 operator -(Matrix2X2 l, Matrix2X2 r)
        {
            Matrix2X2 result = new Matrix2X2(l);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {
                    result[row, column] -= r[row, column];
                }
            }

            return result;
        }

        public static Matrix2X2 operator *(Matrix2X2 l, Matrix2X2 r)
        {
            Matrix2X2 result = new Matrix2X2(l);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {
                    result[row, column] = l[row, 0] * r[0, column];
                    for (int i = 1; i < 2; i++)
                    {
                        result[row, column] += l[row, i] * r[i, column];
                    }
                }
            }

            return result;
        }

        // Matrix - Vector Overloads
        public static Vector2 operator *(Matrix2X2 l, Vector2 r)
        {
            return new Vector2(
                l[0, 0] * r[0] + l[0, 1] * r[1],
                l[1, 0] * r[0] + l[1, 1] * r[1]
            );
        }

        public override string ToString()
        {
            string str = "[\n";
            for (int row = 0; row < 2; row++)
            {
                str += "[";
                for (int column = 0; column < 2; column++)
                {
                    str += this[row, column] + ", ";
                }

                str += "]\n";
            }

            str += "]";

            return str;
        }
    }
}