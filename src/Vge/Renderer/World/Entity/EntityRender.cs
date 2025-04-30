using System;
using System.Collections.Generic;
using Vge.Entity;
using Vge.Entity.Model;
using Vge.Management;
using Vge.Util;
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

            Texture = Render.SetTexture(Ce.ModelEntities.ModelEntitiesObjects[0].Textures[0]);

            _mesh = new MeshEntity(gl);
            // TODO::SceletAnim #1 Тут ставим кубы в ноль, вращения
            _mesh.Reload(Ce.ModelEntities.ModelEntitiesObjects[0].BufferMesh);
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void Draw(float timeIndex, EntityBase entity)
        {
            Render.BindTexture(Texture);
            // Матрица анимации первой кости


            // Матрица анимации второй кости
            // TODO::SceletAnim #2 Тут мы смещаем матрицы на костях на их требуемое положение
            float yaw = 0;
            float pitch = 0;

            if (entity is EntityLiving entityLiving)
            {
                yaw = entityLiving.GetRotationFrameYaw(timeIndex);
                pitch = entityLiving.GetRotationFramePitch(timeIndex);
           /* }
            else
            {
                //yaw = Player.FFF;
                //Player.
                float x = entity.PosX - Player.PosFrameX;
                float y = entity.PosY - Player.PosFrameY;
                float z = entity.PosZ - Player.PosFrameZ;
                yaw = Glm.Atan2(z, x) - Glm.Pi90;
                pitch = -Glm.Atan2(y, Mth.Sqrt(x * x + z * z));
                //yaw = Player.GetRotationFrameYaw(timeIndex);
            }*/
                Render.ShaderBindEntity(Player.View,
                    entity.GetPosFrameX(timeIndex) - Player.PosFrameX,
                    entity.GetPosFrameY(timeIndex) - Player.PosFrameY,
                    entity.GetPosFrameZ(timeIndex) - Player.PosFrameZ,
                    Ce.ModelEntities.ModelEntitiesObjects[0].GenMatrix(yaw, pitch)
                );

                // TODO::SceletAnim #3 Тут перемещение по анимации
                //m2 = Glm.Translate(m2, 0, entityLiving.Eye, 0);

                // TODO::SceletAnim #4 Тут вращение по анимации
                //if (yaw != 0)
                //{
                //    // Это два действия типа анимация
                //    m1 = Glm.Rotate(m1, -yaw, new Vector3(0, 1, 0));
                //    // m1 = Glm.Translate(m1, 0, 0, -0.5f);
                //    //m1 = Glm.Translate(m1, bone.OriginX, bone.OriginY, bone.OriginZ);


                //    // Последующая кость, наследуюет всё с учётом прошлой анимации
                //    m2 = new Mat4(m1);
                //    m3 = new Mat4(m1);
                //    m4 = new Mat4(m1);

                //    // m2 = Glm.Rotate(m2, -yaw, new Vector3(0, 1, 0));
                //    //m2 = Glm.Translate(m2, 0, 1.53f, 0);

                //    // Добавляет смещение стартовое (Local space)
                //    // m2 = Glm.Translate(m2, 0, 1f, -.3f);

                //}

                //if (pitch != 0)
                //{
                //    Bone bone0 = Ce.ModelEntities.ModelEntitiesObjects[0].Bones[0].Children[0];
                //    Bone bone4 = Ce.ModelEntities.ModelEntitiesObjects[0].Bones[0].Children[1].Children[1];
                //    m2 = Glm.Translate(m2, bone0.OriginX, bone0.OriginY, bone0.OriginZ);
                //    m2 = Glm.Rotate(m2, pitch, new Vector3(1, 0, 0));
                //    m2 = Glm.Translate(m2, -bone0.OriginX, -bone0.OriginY, -bone0.OriginZ);

                //    m4 = Glm.Translate(m4, bone4.OriginX, bone4.OriginY, bone4.OriginZ);
                //    m4 = Glm.Rotate(m4, pitch, new Vector3(0, 0, 1));
                //    m4 = Glm.Translate(m4, -bone4.OriginX, -bone4.OriginY, -bone4.OriginZ);
                //}
            }
            else
            {
                //Rand rand = new Rand();
                //Mat4 m = new Mat4(
                //        entity.GetPosFrameX(timeIndex) - Player.PosFrameX,
                //        entity.GetPosFrameY(timeIndex) - Player.PosFrameY,
                //        entity.GetPosFrameZ(timeIndex) - Player.PosFrameZ);
                //m = Glm.Rotate(m, rand.Next(360), new Vector3(0, 1, 0));


                //Render.ShaderBindEntityPrimitive(Player.View, m.ToArray());
                Render.ShaderBindEntityPrimitive(Player.View,
                    entity.GetPosFrameX(timeIndex) - Player.PosFrameX,
                    entity.GetPosFrameY(timeIndex) - Player.PosFrameY,
                    entity.GetPosFrameZ(timeIndex) - Player.PosFrameZ);
            }

            _mesh.Draw();
        }

        public void Dispose() => _mesh.Dispose();
    }
}
