using Vge.Util;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Сущность частички, часть блока
    /// </summary>
    public class EntityPartFX : EntityFX
    {
        private readonly int _blockId;

        public EntityPartFX(Rand rand, int blockId) : base(EnumParticleDraw.Cube, rand)
        {
            _gravity = .25f;
            _blockId = blockId;
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public override void InitRun(Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            motion.X += (_rand.Next(200) - 100) * .004f;
            motion.Y += (_rand.Next(200) - 100) * .004f;
            motion.Z += (_rand.Next(200) - 100) * .004f;
            float r = (_rand.NextFloat() + _rand.NextFloat() + 1f) * .036f;
            motion = motion / Glm.Distance(motion) * r;
            motion.Y += .1f;
            motion *= .5f;
            Physics.MotionX = motion.X;
            Physics.MotionY = motion.Y;
            Physics.MotionZ = motion.Z;

            Scale = .25f + _rand.NextFloat() * .125f;

            BlockBase block = Ce.Blocks.BlockObjects[_blockId];
            if (block.ParticleColors.Length > 0)
            {
                int i = block.ParticleColors.Length == 1
                    ? 0 : _rand.Next(block.ParticleColors.Length);
                Color = new Vector4(block.ParticleColors[i], 1);
            }
            else
            {
                // Тут мы не должны появится!
                Color = new Vector4(_rand.NextFloat(), _rand.NextFloat(), _rand.NextFloat(), 1);
            }
        }
    }
}
