using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        private readonly List<EntityParticle> _list = new List<EntityParticle>(4000);

        /// <summary>
        /// Объект для отладки хитбокса сущности
        /// </summary>
        private readonly HitboxEntityRender _hitbox;

        // TODO::!!!!!!!!
        private ParticleRender _ddddd;
        private ParticleRender _ddddd2;

        public ParticlesRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);

            _ddddd = new ParticleRender(gl);
            float width = 0.125f;
            //width = 0.0625f;
            _ddddd.Reload(_Cube(-width, 0, -width, width, width + width, width, 1, 1, 1, 1));

            _ddddd2 = new ParticleRender(gl);
            //width = 0.03125f;
            width = 0.125f;
            _ddddd2.Reload(_Rectangle(-width, -width, width, width, 1, 0, 0, .5f));
        }

        private float[] _Rectangle(float x1, float y1, float x2, float y2, 
            float r, float g, float b, float a)
        {
            return new float[] 
            {
                x1, y1, 0, r, g, b, a,
                x2, y1, 0, r, g, b, a,
                x2, y2, 0, r, g, b, a,
                x1, y2, 0, r, g, b, a,
            };
        }

        private float[] _Cube(float x1, float y1, float z1, float x2, float y2, float z2,
           float r, float g, float b, float a)
        {
            float r1 = r * Gi.DarkeningSideB;
            float g1 = g * Gi.DarkeningSideB;
            float b1 = b * Gi.DarkeningSideB;

            float r2 = r * Gi.DarkeningSideA;
            float g2 = g * Gi.DarkeningSideA;
            float b2 = b * Gi.DarkeningSideA;

            float r3 = r * Gi.DarkeningSideDown;
            float g3 = g * Gi.DarkeningSideDown;
            float b3 = b * Gi.DarkeningSideDown;

            return new float[]
            {
                //
                x1, y2, z2, r, g, b, a,
                x2, y2, z2, r, g, b, a,
                x2, y2, z1, r, g, b, a,
                x1, y2, z1, r, g, b, a,
                //
                x1, y1, z1, r3, g3, b3, a,
                x2, y1, z1, r3, g3, b3, a,
                x2, y1, z2, r3, g3, b3, a,
                x1, y1, z2, r3, g3, b3, a,

                //
                x1, y1, z2, r1, g1, b1, a,
                x2, y1, z2, r1, g1, b1, a,
                x2, y2, z2, r1, g1, b1, a,
                x1, y2, z2, r1, g1, b1, a,
                //
                x2, y1, z1, r1, g1, b1, a,
                x1, y1, z1, r1, g1, b1, a,
                x1, y2, z1, r1, g1, b1, a,
                x2, y2, z1, r1, g1, b1, a,
                
                //
                x2, y1, z2, r2, g2, b2, a,
                x2, y1, z1, r2, g2, b2, a,
                x2, y2, z1, r2, g2, b2, a,
                x2, y2, z2, r2, g2, b2, a,
                //
                x1, y1, z1, r2, g2, b2, a,
                x1, y1, z2, r2, g2, b2, a,
                x1, y2, z2, r2, g2, b2, a,
                x1, y2, z1, r2, g2, b2, a
            };
        }

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
            while (_list.Count >= 12000) _list.RemoveAt(0);

            EntityParticle entity = new EntityParticle();
            entity.Init(particleId, this, _game.World.Collision);
            entity.InitRun(_game.World.Rnd, pos, motion);
            _list.Add(entity);
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            Render.ShsEntity.BindUniformParticle(
                _game.Player.RotationFrameYaw,
                _game.Player.RotationFramePitch);
            int count = _list.Count;
            for (int i = 0; i < count; i++)
            {
                _list[i].Render.Draw(timeIndex, _game.DeltaTimeFrame);
            }
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            // Нужно для эффектов (частичек)
            int count = _list.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (_list[i].IsDead)
                {
                    _list.RemoveAt(i);
                }
                else
                {
                    _list[i].UpdateClient(_game.World, deltaTime);
                    _list[i].Update();
                }
            }
        }

        /// <summary>
        /// Получить объект рендера частички по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntityRender GetParticleRender(int indexEntity)
        {
            if (indexEntity == 0) return _ddddd;
            return _ddddd2;
        }

        public override void Dispose()
        {
            _hitbox.Dispose();
            _ddddd.Dispose();
            _ddddd2.Dispose();
            int count = _list.Count;
            for (int i = 0; i < count; i++)
            {
                _list[i].Dispose();
            }
            _list.Clear();
        }

        public override string ToString() => "Fx:" + _list.Count;
    }
}
