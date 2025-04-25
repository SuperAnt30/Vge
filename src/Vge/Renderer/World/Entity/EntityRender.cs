using System;
using System.Collections.Generic;
using Vge.Entity;
using Vge.Management;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рисует сущности
    /// </summary>
    public class EntityRender : IDisposable
    {
        /// <summary>
        /// Сетка сущности
        /// </summary>
        private readonly MeshEntity _mesh;

        public readonly uint Texture;

        /// <summary>
        /// Объект отвечающий за прорисовку
        /// </summary>
        public readonly RenderMain Render;
        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public readonly PlayerClientOwner Player;

        public EntityRender(GL gl, RenderMain render, PlayerClientOwner player)
        {
            Render = render;
            Player = player;

            Texture = Render.SetTexture(Ce.ModelEntities.ModelEntitiesObjects[0].Textures[1]);

            _mesh = new MeshEntity(gl);
            // TODO::SceletAnim #1 Тут ставим кубы в ноль, вращения
            _mesh.Reload(Ce.ModelEntities.ModelEntitiesObjects[0].Buffer);
        }


        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void Draw(float timeIndex, EntityBase entity)
        {
            Render.BindTexture(Texture);
            // Матрица анимации первой кости
            Mat4 m1 = Mat4.Identity();

            //m1 = new Mat4(0, 0, -.5f);

            // Матрица анимации второй кости
            // TODO::SceletAnim #2 Тут мы смещаем матрицы на костях на их требуемое положение
            Mat4 m2 = Mat4.Identity();
            Mat4 m3 = Mat4.Identity();
            // Mat4 m2 = new Mat4(0, 1.53f, 0);

            if (entity is EntityLiving entityLiving)
            {
                float yaw = entityLiving.GetRotationFrameYaw(timeIndex);
                float pitch = entityLiving.GetRotationFramePitch(timeIndex);

                // TODO::SceletAnim #3 Тут перемещение по анимации
                //m2 = Glm.Translate(m2, 0, entityLiving.Eye, 0);

                // TODO::SceletAnim #4 Тут вращение по анимации
                if (yaw != 0)
                {
                    // Это два действия типа анимация
                    m1 = Glm.Rotate(m1, -yaw, new Vector3(0, 1, 0));
                    // m1 = Glm.Translate(m1, 0, 0, -0.5f);

                    // Последующая кость, наследуюет всё с учётом прошлой анимации
                    m2 = new Mat4(m1);

                    // m2 = Glm.Rotate(m2, -yaw, new Vector3(0, 1, 0));
                    //m2 = Glm.Translate(m2, 0, 1.53f, 0);

                    // Добавляет смещение стартовое (Local space)
                    //m2 = Glm.Translate(m2, 0, 1f, -.3f);
                }
                if (pitch != 0) m2 = Glm.Rotate(m2, pitch, new Vector3(1, 0, 0));
            }

            List<float> list = new List<float>(m1.ToArray4x3());
            list.AddRange(m2.ToArray4x3());
            for (int k = 2; k < 24; k++) list.AddRange(m1.ToArray4x3());

            Render.ShaderBindEntity(Player.View,
                entity.GetPosFrameX(timeIndex) - Player.PosFrameX,
                entity.GetPosFrameY(timeIndex) - Player.PosFrameY,
                entity.GetPosFrameZ(timeIndex) - Player.PosFrameZ,
                list.ToArray()
                );

            _mesh.Draw();
        }

        public void Dispose() => _mesh.Dispose();
    }
}
