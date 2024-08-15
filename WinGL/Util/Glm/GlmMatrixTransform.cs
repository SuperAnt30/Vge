using System;

namespace WinGL.Util
{
    public static partial class Glm
    {
        /// <summary>
        /// Creates a frustrum projection matrix.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        /// <param name="top">The top.</param>
        /// <param name="nearVal">The near val.</param>
        /// <param name="farVal">The far val.</param>
        /// <returns></returns>
        public static Mat4 Frustum(float left, float right, float bottom, float top, float nearVal, float farVal)
        {
            var result = Mat4.Identity();
            result[0, 0] = (2.0f * nearVal) / (right - left);
            result[1, 1] = (2.0f * nearVal) / (top - bottom);
            result[2, 0] = (right + left) / (right - left);
            result[2, 1] = (top + bottom) / (top - bottom);
            result[2, 2] = -(farVal + nearVal) / (farVal - nearVal);
            result[2, 3] = -1.0f;
            result[3, 2] = -(2.0f * farVal * nearVal) / (farVal - nearVal);
            return result;
        }

        /// <summary>
        /// Creates a matrix for a symmetric perspective-view frustum with far plane at infinite.
        /// </summary>
        /// <param name="fovy">The fovy.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="zNear">The z near.</param>
        /// <returns></returns>
        public static Mat4 InfinitePerspective(float fovy, float aspect, float zNear)
        {
            float range = Tan(fovy / 2f) * zNear;

            float left = -range * aspect;
            float right = range * aspect;
            float bottom = -range;
            float top = range;

            var result = new Mat4(0);
            result[0, 0] = 2f * zNear / (right - left);
            result[1, 1] = 2f * zNear / (top - bottom);
            result[2, 2] = -1f;
            result[2, 3] = -1f;
            result[3, 2] = -2f * zNear;
            return result;
        }

        /// <summary>
        /// Build a look at view matrix.
        /// </summary>
        /// <param name="eye">The eye.</param>
        /// <param name="center">The center.</param>
        /// <param name="up">Up.</param>
        /// <returns></returns>
        public static Mat4 LookAt(Vec3 eye, Vec3 center, Vec3 up)
        {
            Vec3 f = new Vec3(Normalize(center - eye));
            Vec3 s = new Vec3(Normalize(Cross(f, up)));
            Vec3 u = new Vec3(Cross(s, f));

            Mat4 Result = new Mat4(1);
            Result[0, 0] = s.x;
            Result[1, 0] = s.y;
            Result[2, 0] = s.z;
            Result[0, 1] = u.x;
            Result[1, 1] = u.y;
            Result[2, 1] = u.z;
            Result[0, 2] = -f.x;
            Result[1, 2] = -f.y;
            Result[2, 2] = -f.z;
            Result[3, 0] = -Dot(s, eye);
            Result[3, 1] = -Dot(u, eye);
            Result[3, 2] = Dot(f, eye);
            return Result;
        }

        /// <summary>
        /// Creates a matrix for an orthographic parallel viewing volume.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        /// <param name="top">The top.</param>
        /// <param name="zNear">The z near.</param>
        /// <param name="zFar">The z far.</param>
        /// <returns></returns>
        public static Mat4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            var result = Mat4.Identity();
            result[0, 0] = 2f / (right - left);
            result[1, 1] = 2f / (top - bottom);
            result[2, 2] = -2f / (zFar - zNear);
            result[3, 0] = -(right + left) / (right - left);
            result[3, 1] = -(top + bottom) / (top - bottom);
            result[3, 2] = -(zFar + zNear) / (zFar - zNear);
            return result;
        }

        /// <summary>
        /// Creates a matrix for projecting two-dimensional coordinates onto the screen.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        /// <param name="top">The top.</param>
        /// <returns></returns>
        public static Mat4 Ortho(float left, float right, float bottom, float top)
        {
            var result = Mat4.Identity();
            result[0, 0] = 2f / (right - left);
            result[1, 1] = 2f / (top - bottom);
            result[2, 2] = -1f;
            result[3, 0] = -(right + left) / (right - left);
            result[3, 1] = -(top + bottom) / (top - bottom);
            return result;
        }

        /// <summary>
        /// Creates a perspective transformation matrix.
        /// </summary>
        /// <param name="fovy">The field of view angle, in radians.</param>
        /// <param name="aspect">The aspect ratio.</param>
        /// <param name="zNear">The near depth clipping plane.</param>
        /// <param name="zFar">The far depth clipping plane.</param>
        /// <returns>A <see cref="mat4"/> that contains the projection matrix for the perspective transformation.</returns>
        public static Mat4 Perspective(float fovy, float aspect, float zNear, float zFar)
        {
            var tanHalfFovy = Tan(fovy / 2.0f);

            var result = Mat4.Identity();
            result[0, 0] = 1.0f / (aspect * tanHalfFovy);
            result[1, 1] = 1.0f / (tanHalfFovy);
            result[2, 2] = -(zFar + zNear) / (zFar - zNear);
            result[2, 3] = -1.0f;
            result[3, 2] = -(2.0f * zFar * zNear) / (zFar - zNear);
            result[3, 3] = 0.0f;
            return result;
        }

        /// <summary>
        /// Builds a perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">The fov (in radians).</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="zNear">The z near.</param>
        /// <param name="zFar">The z far.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static Mat4 PerspectiveFov(float fov, float width, float height, float zNear, float zFar)
        {
            if (width <= 0 || height <= 0 || fov <= 0)
                throw new ArgumentOutOfRangeException();

            var rad = fov;

            var h = Cos(.5f * rad) / Sin(.5f * rad);
            var w = h * height / width;

            var result = new Mat4(0);
            result[0, 0] = w;
            result[1, 1] = h;
            result[2, 2] = -(zFar + zNear) / (zFar - zNear);
            result[2, 3] = -1f;
            result[3, 2] = -(2f * zFar * zNear) / (zFar - zNear);
            return result;
        }

        /// <summary>
        /// Define a picking region.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="delta">The delta.</param>
        /// <param name="viewport">The viewport.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static Mat4 PickMatrix(Vec2 center, Vec2 delta, Vec4 viewport)
        {
            if (delta.x <= 0 || delta.y <= 0)
                throw new ArgumentOutOfRangeException();
            var result = new Mat4(1.0f);

            if (!(delta.x > 0 && delta.y > 0))
                return result; // Error

            Vec3 temp = new Vec3(
                (viewport[2] - 2f * (center.x - viewport[0])) / delta.x,
                (viewport[3] - 2f * (center.y - viewport[1])) / delta.y,
                (0f));

            // Translate and scale the picked region to the entire window
            result = Translate(result, temp);
            return Scale(result, new Vec3(viewport[2] / delta.x, viewport[3] / delta.y, 1));
        }

        /// <summary>
        /// Map the specified object coordinates (obj.x, obj.y, obj.z) into window coordinates.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="model">The model.</param>
        /// <param name="proj">The proj.</param>
        /// <param name="viewport">The viewport.</param>
        /// <returns></returns>
        public static Vec3 Project(Vec3 obj, Mat4 model, Mat4 proj, Vec4 viewport)
        {
            Vec4 tmp = new Vec4(obj, 1f);
            tmp = model * tmp;
            tmp = proj * tmp;

            tmp /= tmp.w;
            tmp = tmp * 0.5f + 0.5f;
            tmp[0] = tmp[0] * viewport[2] + viewport[0];
            tmp[1] = tmp[1] * viewport[3] + viewport[1];

            return new Vec3(tmp.x, tmp.y, tmp.z);
        }

        /// <summary>
        /// Builds a rotation 4 * 4 matrix created from an axis vector and an angle.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public static Mat4 Rotate(Mat4 m, float angle, Vec3 v)
        {
            float c = Cos(angle);
            float s = Sin(angle);

            Vec3 axis = Normalize(v);
            Vec3 temp = (1.0f - c) * axis;

            Mat4 rotate = Mat4.Identity();
            rotate[0, 0] = c + temp[0] * axis[0];
            rotate[0, 1] = 0 + temp[0] * axis[1] + s * axis[2];
            rotate[0, 2] = 0 + temp[0] * axis[2] - s * axis[1];

            rotate[1, 0] = 0 + temp[1] * axis[0] - s * axis[2];
            rotate[1, 1] = c + temp[1] * axis[1];
            rotate[1, 2] = 0 + temp[1] * axis[2] + s * axis[0];

            rotate[2, 0] = 0 + temp[2] * axis[0] + s * axis[1];
            rotate[2, 1] = 0 + temp[2] * axis[1] - s * axis[0];
            rotate[2, 2] = c + temp[2] * axis[2];

            Mat4 result = Mat4.Identity();
            result[0] = m[0] * rotate[0][0] + m[1] * rotate[0][1] + m[2] * rotate[0][2];
            result[1] = m[0] * rotate[1][0] + m[1] * rotate[1][1] + m[2] * rotate[1][2];
            result[2] = m[0] * rotate[2][0] + m[1] * rotate[2][1] + m[2] * rotate[2][2];
            result[3] = m[3];
            return result;
        }

        public static Mat4 Rotate(Mat4 rotat, float yaw, float pitch, float roll)
        {
            if (yaw != 0) rotat = Rotate(rotat, yaw, new Vec3(0, 0, 1));
            if (pitch != 0) rotat = Rotate(rotat, pitch, new Vec3(1, 0, 0));
            if (roll != 0) rotat = Rotate(rotat, roll, new Vec3(0, 1, 0));

            return rotat;
        }

        /// <summary>
        /// Applies a scale transformation to matrix <paramref name="m"/> by vector <paramref name="v"/>.
        /// </summary>
        /// <param name="m">The matrix to transform.</param>
        /// <param name="v">The vector to scale by.</param>
        /// <returns><paramref name="m"/> scaled by <paramref name="v"/>.</returns>
        public static Mat4 Scale(Mat4 m, Vec3 v)
        {
            Mat4 result = m;
            result[0] = m[0] * v[0];
            result[1] = m[1] * v[1];
            result[2] = m[2] * v[2];
            result[3] = m[3];
            return result;
        }

        /// <summary>
        /// Applies a translation transformation to matrix <paramref name="m"/> by vector <paramref name="v"/>.
        /// </summary>
        /// <param name="m">The matrix to transform.</param>
        /// <param name="v">The vector to translate by.</param>
        /// <returns><paramref name="m"/> translated by <paramref name="v"/>.</returns>
        public static Mat4 Translate(Mat4 m, Vec3 v)
        {
            Mat4 result = m;
            result[3] = m[0] * v[0] + m[1] * v[1] + m[2] * v[2] + m[3];
            return result;
        }

        /// <summary>
        /// Creates a matrix for a symmetric perspective-view frustum with far plane 
        /// at infinite for graphics hardware that doesn't support depth clamping.
        /// </summary>
        /// <param name="fovy">The fovy.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="zNear">The z near.</param>
        /// <returns></returns>
        public static Mat4 TweakedInfinitePerspective(float fovy, float aspect, float zNear)
        {
            float range = Tan(fovy / (2)) * zNear;
            float left = -range * aspect;
            float right = range * aspect;
            float bottom = -range;
            float top = range;

            Mat4 Result = new Mat4(0f);
            Result[0, 0] = 2 * zNear / (right - left);
            Result[1, 1] = 2 * zNear / (top - bottom);
            Result[2, 2] = .0001f - 1f;
            Result[2, 3] = -1;
            Result[3, 2] = -(0.0001f - 2) * zNear;
            return Result;
        }

        /// <summary>
        /// Map the specified window coordinates (win.x, win.y, win.z) into object coordinates.
        /// </summary>
        /// <param name="win">The win.</param>
        /// <param name="model">The model.</param>
        /// <param name="proj">The proj.</param>
        /// <param name="viewport">The viewport.</param>
        /// <returns></returns>
        public static Vec3 UnProject(Vec3 win, Mat4 model, Mat4 proj, Vec4 viewport)
        {
            Mat4 inverse = Inverse(proj * model);

            Vec4 tmp = new Vec4(win, 1f);
            tmp.x = (tmp.x - viewport[0]) / viewport[2];
            tmp.y = (tmp.y - viewport[1]) / viewport[3];
            tmp = tmp * 2f - 1f;

            Vec4 obj = inverse * tmp;
            obj /= obj.w;

            return new Vec3(obj);
        }
    }
}
