using System.Collections.Generic;
using Vge.Entity;
using Vge.Games;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рендера всех сущностей
    /// </summary>
    public class EntitiesRenderer : WarpRenderer
    {
        /// <summary>
        /// Количество сущностей которые попали в FrustumCulling чанка, т.е. видимых для прорисовки
        /// </summary>
        public int CountEntitiesFC { get; private set; }
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;
        /// <summary>
        /// Метод чанков для прорисовки
        /// </summary>
        private readonly ArrayFast<ChunkRender> _arrayChunkRender;

        private HitboxEntityRender _hitbox;

        private EntityRender _entityRender;

        public EntitiesRenderer(GameBase game, ArrayFast<ChunkRender> arrayChunkRender) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);
            _entityRender = new EntityRender(gl);
            _arrayChunkRender = arrayChunkRender;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            int count = _arrayChunkRender.Count;
            int countEntity;
            ChunkRender chunkRender;
            EntityBase entity;
            int playerId = _game.Player.Id;
            float x = _game.Player.PosFrameX;
            float y = _game.Player.PosFrameY;
            float z = _game.Player.PosFrameZ;

            CountEntitiesFC = 0;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                countEntity = chunkRender.ListEntities.Count;
                if (countEntity > 0)
                {
                    for (int j = 0; j < countEntity; j++)
                    {
                        entity = chunkRender.ListEntities.GetAt(j);
                        //if (entity.Id != playerId)// && _game.Player.IsBoxInFrustum(entity.GetBoundingBoxOffset(-x, -y, -z)))
                        {
                            _game.WorldRender.Render.ShaderBindLine(_game.Player.View,
                                entity.GetPosFrameX(timeIndex) - x,
                                entity.GetPosFrameY(timeIndex) - y,
                                entity.GetPosFrameZ(timeIndex) - z);
                            _hitbox.Draw(timeIndex, entity);


                            // Матрица анимации первой кости
                            Mat4 m1 = Mat4.Identity();

                            //m1 = new Mat4(0, 0, -.5f);

                            // Матрица анимации второй кости
                            // TODO::SceletAnim #2 Тут мы смещаем матрицы на костях на их требуемое положение
                            Mat4 m2 = Mat4.Identity();
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
                                    m1 = Glm.Rotate(m1, -yaw, new Vector3(0, 1, 0));
                                    m1 = Glm.Translate(m1, 0, 0, -0.5f);
                                    m2 = Glm.Rotate(m2, -yaw, new Vector3(0, 1, 0));
                                    m2 = Glm.Translate(m2, 0, 1.53f, 0);
                                }
                                if (pitch != 0) m2 = Glm.Rotate(m2, pitch, new Vector3(1, 0, 0));
                            }

                            List<float> list = new List<float>(m1.ToArray4x3());
                            list.AddRange(m2.ToArray4x3());

                            _game.WorldRender.Render.ShaderBindEntity(_game.Player.View,
                                entity.GetPosFrameX(timeIndex) - x,
                                entity.GetPosFrameY(timeIndex) - y,
                                entity.GetPosFrameZ(timeIndex) - z,
                                list.ToArray()
                                );

                            _entityRender.Draw(timeIndex, entity);
                            CountEntitiesFC++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки основного игрока
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void DrawOwner(float timeIndex)
        {
            _game.WorldRender.Render.ShaderBindLine(_game.Player.View, 0, 0, 0);
            _hitbox.Draw(timeIndex, _game.Player);
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
        }

        //private void _RenderEntity(EntityBase entity, float timeIndex)
        //{
        //    _game.WorldRender.Render.ShaderBindLine(_game.Player.View,
        //        _game.Player.PosFrameX,
        //        _game.Player.PosFrameY,
        //        _game.Player.PosFrameZ);
        //   // _owner.Draw();
        //}

    }
}
