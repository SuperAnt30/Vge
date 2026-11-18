using System.Collections.Generic;
using Vge.Entity.Particle;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Renderer.World.Entity;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера всех частичек
    /// </summary>
    public class ParticlesRenderer : WarpRenderer
    {
        /// <summary>
        /// Максимальное количество частичек для 3д
        /// </summary>
        public static int MaxAmount3d = 10000;
        /// <summary>
        /// Максимальное количество частичек для 2д
        /// </summary>
        public static int MaxAmount2d = 16000;
        /// <summary>
        /// Максимальное количество частичек для 3д FrustumCulling
        /// </summary>
        public static int MaxAmount3dFC = 10000;
        /// <summary>
        /// Максимальное количество частичек для 2д FrustumCulling
        /// </summary>
        public static int MaxAmount2dFC = 16000;

        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public readonly PlayerClientOwner Player;

        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        /// <summary>
        /// Объёмные частички
        /// </summary>
        private readonly List<EntityFX> _list3d = new List<EntityFX>(MaxAmount3d);
        /// <summary>
        /// Двухмерные частички
        /// </summary>
        private readonly List<EntityFX> _list2d = new List<EntityFX>(MaxAmount2d);

        /// <summary>
        /// Рендер частички 2д квадрат просто цвет
        /// </summary>
        private readonly ParticleInstanced2d _particleInstanced2D;
        /// <summary>
        /// Рендер частики 3д, куб
        /// </summary>
        private readonly ParticleInstanced3d _particleInstanced3D;

        public ParticlesRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            Player = game.Player;

            _particleInstanced2D = new ParticleInstanced2d(gl, MaxAmount2dFC);
            _particleInstanced3D = new ParticleInstanced3d(gl, MaxAmount3dFC);
        }

        public void Spawn(ushort particleId, Vector3 pos, Vector3 motion, int parameter)
        {
            EntityFX entity = Ce.EntitiesFX.CreateEntityClient(particleId, _game.World, this, parameter);
            entity.InitRun(pos, motion);
            if (entity.IsCube)
            {
                while (_list3d.Count >= MaxAmount3d) _list3d.RemoveAt(0);
                _list3d.Add(entity);
            }
            else
            {
                while (_list2d.Count >= MaxAmount2d) _list2d.RemoveAt(0);
                _list2d.Add(entity);
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            EntityFX entityFX;
            int i;

            // Рендер 3д частичек
            Render.ShsEntity.BindUniformParticle3d();
            _particleInstanced3D.Begin();
            int count = _list3d.Count - 1;
            for (i = count; i >= 0; i--)
            {
                entityFX = _list3d[i];
                if (Player.IsPointInFrustum(entityFX.PosX, entityFX.PosY, entityFX.PosZ))
                {
                    entityFX.RenderParticle.WriteInstance(_particleInstanced3D, timeIndex);
                    if (_particleInstanced3D.Count >= MaxAmount3dFC) break;
                }
            }
            _particleInstanced3D.Draw();

            // Рендер 2д частичек
            Render.BindTextureParticles();
            float pitch = _game.Player.RotationFramePitch;
            if (Player.ViewCamera == EnumViewCamera.Front) pitch += Glm.Pi;

            Render.ShsEntity.BindUniformParticle2d(Player.RotationFrameYaw, pitch);

            _particleInstanced2D.Begin();
            count = _list2d.Count - 1;
            for (i = count; i >= 0; i--)
            {
                entityFX = _list2d[i];
                if (Player.IsPointInFrustum(entityFX.PosX, entityFX.PosY, entityFX.PosZ))
                {
                    entityFX.RenderParticle.WriteInstance(_particleInstanced2D, timeIndex);
                    if (_particleInstanced2D.Count >= MaxAmount2dFC) break;
                }
            }
            _particleInstanced2D.Draw();
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            _Update(_list3d, deltaTime);
            _Update(_list2d, deltaTime);
        }

        private void _Update(List<EntityFX> list, float deltaTime)
        {
            int count = list.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (list[i].IsDead)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    list[i].UpdateClient(_game.World, deltaTime);
                    list[i].Update();
                }
            }
        }

        /// <summary>
        /// Удалить все частички, нужен для перехода миров
        /// </summary>
        public void Clear()
        {
            _list3d.Clear();
            _list2d.Clear();
        }

        public override void Dispose()
        {
            Clear();
            _particleInstanced2D.Dispose();
            _particleInstanced3D.Dispose();
        }

        public override string ToString()
            => "Fx2d: " + _particleInstanced2D.Count + "|" + _list2d.Count
                + " Fx3d: " + _particleInstanced3D.Count + "|" + _list3d.Count;
    }
}
