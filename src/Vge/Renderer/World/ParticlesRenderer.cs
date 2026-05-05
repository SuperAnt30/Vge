using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Particle;
using Vge.Entity.Player;
using Vge.Entity.Render;
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
        private const int _MaxAmount3d = 1000;
        /// <summary>
        /// Максимальное количество частичек для 2д
        /// </summary>
        private const int _MaxAmount2d = 4000;

        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        /// <summary>
        /// Объёмные частички
        /// </summary>
        private readonly List<EntityFX> _list3d = new List<EntityFX>(_MaxAmount3d);
        /// <summary>
        /// Двухмерные частички
        /// </summary>
        private readonly List<EntityFX> _list2d = new List<EntityFX>(_MaxAmount2d);

        /// <summary>
        /// Объект для отладки хитбокса сущности
        /// </summary>
        private readonly HitboxEntityRender _hitbox;

        /// <summary>
        /// Рендер частички 2д квадрат просто цвет
        /// </summary>
        private readonly ParticleRender _particleRenderQuad;
        /// <summary>
        /// Рендер частички 2д с текстурой
        /// </summary>
        private readonly ParticleRender _particleRenderSprite;
        /// <summary>
        /// Рендер частики 3д, куб
        /// </summary>
        private readonly ParticleRender _particleRenderCube;

        public ParticlesRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);

            _particleRenderQuad = new ParticleRender(gl, false, _Rectangle(-.125f, .125f));
            _particleRenderSprite = new ParticleRender(gl, true, _Rectangle(-.125f, .125f));
            _particleRenderCube = new ParticleRender(gl, false, _Cube(-.125f, 0, .125f, .25f));
        }

        #region Figure

        private float[] _Rectangle(float f1, float f2) => new float[] {
                f1, f1, 0, 0,
                f2, f1, 1, 0,
                f2, f2, 1, 1,
                f1, f2, 0, 1
            };

        private float[] _Cube(float w1, float h1, float w2, float h2)
        {
            float s1 = Gi.DarkeningSideB;
            float s2 = Gi.DarkeningSideA;
            float s3 = Gi.DarkeningSideDown;

            return new float[]
            {
                w1, h2, w2, 1,
                w2, h2, w2, 1,
                w2, h2, w1, 1,
                w1, h2, w1, 1,

                w1, h1, w1, s3,
                w2, h1, w1, s3,
                w2, h1, w2, s3,
                w1, h1, w2, s3,

                w1, h1, w2, s1,
                w2, h1, w2, s1,
                w2, h2, w2, s1,
                w1, h2, w2, s1,

                w2, h1, w1, s1,
                w1, h1, w1, s1,
                w1, h2, w1, s1,
                w2, h2, w1, s1,

                w2, h1, w2, s2,
                w2, h1, w1, s2,
                w2, h2, w1, s2,
                w2, h2, w2, s2,

                w1, h1, w1, s2,
                w1, h1, w2, s2,
                w1, h2, w2, s2,
                w1, h2, w1, s2
            };
        }

        #endregion

        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public PlayerClientOwner Player
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _game.Player;
        }

        public void Spawn(ushort particleId, Vector3 pos, Vector3 motion, int parameter)
        {
            EntityFX entity = Ce.EntitiesFX.CreateEntityClient(particleId, _game.World, this, parameter);
            entity.InitRun(pos, motion);

            if (entity.IsCube)
            {
                while (_list3d.Count >= _MaxAmount3d) _list3d.RemoveAt(0);
                _list3d.Add(entity);
            }
            else
            {
                while (_list2d.Count >= _MaxAmount2d) _list2d.RemoveAt(0);
                _list2d.Add(entity);
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            Render.ShsEntity.BindUniformParticle3d();
            
            int count = _list3d.Count;
            for (int i = 0; i < count; i++)
            {
                _list3d[i].Render.Draw(timeIndex, _game.DeltaTimeFrame);
            }

            Render.BindTextureParticles();
            float pitch = _game.Player.RotationFramePitch;
            if (_game.Player.ViewCamera == EnumViewCamera.Front) pitch += Glm.Pi;

            Render.ShsEntity.BindUniformParticle2d(_game.Player.RotationFrameYaw, pitch);

            count = _list2d.Count;
            for (int i = 0; i < count; i++)
            {
                _list2d[i].Render.Draw(timeIndex, _game.DeltaTimeFrame);
            }
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
        /// Получить объект рендера частички по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntityRender GetParticleRender(EnumParticleDraw enumParticle)
        {
            if (enumParticle == EnumParticleDraw.Cube) return _particleRenderCube;
            if (enumParticle == EnumParticleDraw.Sprite) return _particleRenderSprite;
            return _particleRenderQuad;
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
            _particleRenderQuad.Dispose();
            _particleRenderSprite.Dispose();
            _particleRenderCube.Dispose();
            _list3d.Clear();
            _list2d.Clear();
        }

        public override string ToString() => "Fx2d:" + _list2d.Count + " Fx3d:" + _list3d.Count;
    }
}
