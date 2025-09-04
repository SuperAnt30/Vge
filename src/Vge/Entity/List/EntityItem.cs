using System.Runtime.CompilerServices;
using Vge.Entity.MetaData;
using Vge.Entity.Physics;
using Vge.Entity.Render;
using Vge.Entity.Sizes;
using Vge.Item;
using Vge.Renderer.World.Entity;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.List
{
    /// <summary>
    /// Сущность предмета
    /// </summary>
    public class EntityItem : EntityBase
    {
        /// <summary>
        /// Сколько тиков жизни
        /// </summary>
        protected int _age = 0;

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public virtual void InitRun(EntityLiving entityThrower, int i)
            => _InitRun(entityThrower, i, .3f);

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        protected void _InitRun(EntityLiving entityThrower, int i, float speedThrower = .49f)
        {
            // ВРЕМЕННО, бросок сущности!
            // спереди
            float f3 = (int)(i / 100) * 1.54f;
            i = i % 100; 
            float f = (i & 15) * 1.54f + 1;
            float f2 = (i >> 4) * 1.3f;
            PosX = entityThrower.PosX + Glm.Sin(entityThrower.RotationYaw) * f
                - Glm.Cos(entityThrower.RotationYaw) * f3;
            PosZ = entityThrower.PosZ - Glm.Cos(entityThrower.RotationYaw) * f
                + Glm.Sin(entityThrower.RotationYaw) * f3;
            PosY = entityThrower.PosY + entityThrower.SizeLiving.GetEye() - .2f + f2;
            
            float pitchxz = Glm.Cos(entityThrower.RotationPitch);
            Physics.MotionX = Glm.Sin(entityThrower.RotationYaw) * pitchxz * speedThrower;
            Physics.MotionY = Glm.Sin(entityThrower.RotationPitch) * speedThrower;
            Physics.MotionZ = -Glm.Cos(entityThrower.RotationYaw) * pitchxz * speedThrower;
        }

        /// <summary>
        /// Инициализация размеров сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitSize()
            => Size = new SizeEntityBox(this, .4375f, .125f, 2);

        /// <summary>
        /// Инициализация физики
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitPhysics(CollisionBase collision)
            => Physics = new PhysicsGround(collision, this, .5f);

        /// <summary>
        /// Инициализация для клиента
        /// </summary>
        public override void InitRender(ushort index, EntitiesRenderer entitiesRenderer)
        {
            IndexEntity = index;
            Render = new EntityRenderItem(this, entitiesRenderer);
            _InitMetaData();
            _InitSize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitMetaData()
        {
            MetaData = new DataWatcher(1);
            // Стак предмета
            MetaData.SetByDataType(0, EnumTypeWatcher.ItemStack);
        }

        /// <summary>
        /// Вызывается в момент спавна на клиенте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SpawnClient()
        {
            ItemStack itemStack = GetEntityItemStack();
            if (itemStack != null)
            {
                Size = new SizeEntityBox(this, itemStack.Item.Width,
                    itemStack.Item.Height, itemStack.Item.Weight);
            }
        }

        /// <summary>
        /// Вызывается в момент спавна на сервере
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SpawnServer()
        {
            ItemStack itemStack = GetEntityItemStack();
            if (itemStack != null)
            {
                Size = new SizeEntityBox(this, itemStack.Item.Width,
                    itemStack.Item.Height, itemStack.Item.Weight);
                Physics.SetRebound(itemStack.Item.Rebound);
            }
        }

        /// <summary>
        /// Возвращает true, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanBeCollidedWith() => true;

        /// <summary>
        /// Игровой такт на сервере
        /// </summary>
        public override void Update()
        {
            if (_age > 9000)
            {
                SetDead();
                return;
            }
            _age++;

            if (!Physics.IsPhysicSleep())
            {
                //Console.Write(PosX);
                //Console.Write(" ");
                //Console.Write(PosY);
                //Console.Write(" ");
                //Console.WriteLine(PosZ);

                // Расчитать перемещение в объекте физика
                Physics.LivingUpdate();

                //if (Physics.CaughtInBlock > 2)
                //{
                //    if (_jumpTime > 0) _jumpTime--;
                //    if (_jumpTime == 0 && OnGround)
                //    {
                //        Physics.MotionY = .5f;
                //        _jumpTime = 20;
                //        Physics.AwakenPhysics();
                //    }
                //}

                if (Physics.CaughtInBlock > 2 || Physics.CaughtInEntity > 30)
                {
                    SetDead();
                    return;
                }


                if (IsPositionChange())
                {
                    //float x = -Physics.MotionX;
                    //float z = -Physics.MotionZ;

                    //RotationYaw = Glm.Atan2(z, x) - Glm.Pi90;
                    //RotationPitch = -Glm.Atan2(-Physics.MotionY, Mth.Sqrt(x * x + z * z));
                    //RotationPrevYaw = RotationYaw;
                    //RotationPrevPitch = RotationPitch;

                    UpdatePositionPrev();
                        
                    LevelMotionChange = 2;
                }
                else if (!Physics.IsPhysicSleep())
                {
                    // Сущность не спит!
                    // Бодрый, только помечен на пробуждение, но не было перемещения [3 или 4]
                    // Надо ещё 2 такта как минимум передать, чтоб клиент, зафиксировал сон [1]
                    LevelMotionChange = 2;
                }
            }
            else
            {
                //if (_jumpTime++ > 150)
                //{
                //    Physics.MotionY = .5f;
                //    _jumpTime = 0;
                //    Physics.AwakenPhysics();
                //}
            }
           
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            UpdatePositionServer();
            Render.UpdateClient(world, deltaTime);
        }

        /// <summary>
        /// Возвращает ItemStack, соответствующий Entity 
        /// (Примечание: если предмет не существует, регистрируется ошибка, 
        /// но все равно возвращается ItemStack, содержащий Block.stone)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack GetEntityItemStack()
        {
            ItemStack itemStack = MetaData.GetItemStack(0);
            if (itemStack == null)
            {
                // TODO::2025-08-26 Ошибка отсутствия стака
                return null;
                //if (World != null && World is WorldServer worldServer)
                //{
                //    worldServer.Log.Log("Item entity " + Id + " has no item?!");
                //}
                //return new ItemStack(Blocks.GetBlockCache(EnumBlock.Stone));
            }
            return itemStack;
        }

        /// <summary>
        /// Заменить данные по стаку в сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEntityItemStack(ItemStack itemStack)
        {
            if (!MetaData.UpdateObject(0, itemStack))
            {
                MetaData.SetObjectWatched(0);
            }
        }
    }
}
