using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.Particle;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Renderer.World.Entity;
using Vge.Util;
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

        public ParticlesRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            _hitbox = new HitboxEntityRender(gl);
            _ddddd = new ParticleRender(gl);
        }

        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public PlayerClientOwner Player
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _game.Player;
        }

        public void Spawn(int particleId, Vector3 pos, Vector3 motion, int parameter)
        { 
            while (_list.Count >= 12000) _list.RemoveAt(0);

            EntityParticle entity = new EntityParticle();
            entity.Init(this, _game.World.Collision);
            entity.InitRun(_game.World.Rnd, pos, motion);
            _list.Add(entity);
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            Render.ShLine.Bind();
            Render.ShLine.SetUniformMatrix4("view", Gi.MatrixView);
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
                    _list[i].Dispose();
                    _list.RemoveAt(i);
                }
                else
                {
                    _list[i].Update();
                }
            }
        }

        /// <summary>
        /// Получить объект рендера частички по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParticleRender GetParticleRender(int indexEntity) => _ddddd;

        public override void Dispose()
        {
            _hitbox.Dispose();
            _ddddd.Dispose();
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
