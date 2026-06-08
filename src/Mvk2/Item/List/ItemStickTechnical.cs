using Mvk2.World.Block;
using Mvk2.World.Block.List;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Mvk2.Item.List
{
    /// <summary>
    /// Предмет техническая палочка, для отладки
    /// </summary>
    public class ItemStickTechnical : ItemBase
    {
        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool begin, ItemStack stack,
            PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = base.OnSecond(begin, stack, player, counter);
            if (result.Action != ResultHandSecond.ActionType.None)
            {
                return result;
            }

            MovingObjectPosition moving = player.MovingObject;
            if (moving.IsBlock())
            {
                return new ResultHandSecond(ResultHandSecond.ActionType.BlockPlacement);
            }
            return new ResultHandSecond(ResultHandSecond.ActionType.None);
        }

        /// <summary>
        /// Действие предмета ЛКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        //public override ResultHandAction OnAction(bool begin, ItemStack stack, PlayerClientOwner player)
        //    => new ResultHandAction(player.MovingObject.IsBlock() 
        //        ? ResultHandAction.ActionType.ItemOnBlock
        //        : ResultHandAction.ActionType.None);

        /// <summary>
        /// Вызывается, когда текущий предмет пробуюет установить на блок,
        /// возвращает true если действие состоялось
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают ПКМ</param>
        /// <param name="side">Сторона, по которой щелкнули ПКМ</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="flagReplaceable">Надо ли проверять смещение на установку блока параметра IsReplaceable</param>
        /// <param name="playerYaw">Угол Yaw игрока в момент установки блока</param>
        public override bool OnItemOnBlockPlacement(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing, bool flagReplaceable, float playerYaw)
        {
            WorldBase world = player.GetWorld();

            if (!world.IsRemote && world is WorldServer worldServer)
            {
                ChunkServer chunk = worldServer.GetChunkServer(blockPos);
                BlockState blockState = chunk.GetBlockState(blockPos);
                BlockBase block = blockState.GetBlock();

                if (block is BlockLeaves blockLeaves)
                {
                    // Клик по листве, вешаем снизу плод
                    blockLeaves.HangTheFetus(chunk, blockPos);
                }
                else if (block is BlockTree blockTree)
                {
                    if (blockTree.Type == BlockTree.TypeTree.Sapling)
                    {
                        // Клик по саженцу дерева
                        blockTree.UpdateTick(worldServer, chunk, blockPos, blockState, world.Rnd);
                    }
                    else if (blockTree.Type == BlockTree.TypeTree.Branch
                        || blockTree.Type == BlockTree.TypeTree.Log)
                    {
                        // Клик по саженцу дерева
                        blockTree.Find(worldServer, chunk, blockPos);
                    }
                }
                else if (block is BlockTallGrass tallGrass)
                {
                    // Клик по высокой траве, чтоб выросла
                    tallGrass.GrassGrowth(worldServer, chunk, blockState, blockPos);
                }
            }

            return false;
        }
    }
}
