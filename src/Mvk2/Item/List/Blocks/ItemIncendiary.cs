using Mvk2.World.Block.List;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Item.List;
using Vge.Realms;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.Item.List.Blocks
{
    /// <summary>
    /// Объект поджигательного предмета
    /// </summary>
    public class ItemIncendiary : ItemBlock
    {
        /// <summary>
        /// уровень инструмента, Качество дропа, 0-минимум, 1-левел инструмента, 2-левел, 3-левел
        /// </summary>
        public int Level { get; private set; } = 2;
        /// <summary>
        /// Какая фартуна поджёга, 0-1.0, где 0 не загорится, 1 загорится
        /// </summary>
        public float Fartune { get; private set; } = .3f;
        /// <summary>
        /// Пауза между ударами в тактах
        /// </summary>
        public int Pause { get; protected set; } = 8;
        /// <summary>
        /// Задержка до установки блока
        /// </summary>
        public int DelayPlacement { get; protected set; } = 4;

        private readonly BlockFire _blockFire;

        public ItemIncendiary(BlockFire blockFire) : base(blockFire)
            => _blockFire = blockFire;

        /// <summary>
        /// Не удачная установка блока
        /// </summary>
        protected override ResultHandSecond _ResultHandSecondNone()
        {
            return new ResultHandSecond(ResultHandSecond.ActionType.None, Pause)
            {
                Animation = true
            };
        }

        /// <summary>
        /// Прошла проверку возможности установить блок в превью.
        /// Только клиент, так как превью у него только.
        /// </summary>
        protected override bool _CanPlaceBlockPreview(PlayerBase player)
        {
            if (player.CreativeMode) return true;

            // Не важно зажат или нет, ламаем предмет
            if (player is PlayerClientOwner playerClient)
            {
                playerClient.DamageItemTool(1);
            }
            return Fartune == 1 || player.GetWorld().Rnd.NextFloat() < Fartune;
        }

        /// <summary>
        /// Уменьшить количество предметом в текущей руке
        /// </summary>
        protected override void _DecrCurrentItem(PlayerBase player, byte amount) { }

        /// <summary>
        /// Пауза между установками блоков
        /// </summary>
        /// <param name="begin">Первый блок</param>
        protected override int _Pause(bool begin) => Pause;
        /// <summary>
        /// Задержка до установки блока.
        /// По умолчанию HandManager.DelayPlacement = 2
        /// </summary>
        protected override int _DelayPlacement() => DelayPlacement;

        /// <summary>
        /// Проверка, может ли блок устанавливаться в этом месте
        /// </summary>
        /// <param name="blockPos">Координата блока, по которому щелкают правой кнопкой мыши</param>
        /// <param name="block">Объект блока который хотим установить</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        protected override bool _CanPlaceBlockOnSide(ItemStack stack, PlayerBase player, WorldBase world,
            BlockPos blockPos, BlockBase block, Pole side, Vector3 facing, float playerYaw)
        {
            if (!base._CanPlaceBlockOnSide(stack, player, world,
                blockPos, _blockFire, side, facing, playerYaw))
            {
                return false;
            }

            return _blockFire.CanBlockStay(world, null, blockPos,
                _blockFire.OnBlockPlaced(world, blockPos, new BlockState(), side, facing, playerYaw).Met);
        }


        /// <summary>
        /// Задать подсказку
        /// </summary>
        public override void SetToolTipLang(string toolTip)
        {
            base.SetToolTipLang(toolTip);
            _toolTip = _toolTip + ChatStyle.Br + "-" + ChatStyle.Br
                + "Level {0}" + ChatStyle.Br
                + "Fartune {1}" + ChatStyle.Br
                + "{2} [{3}]";
        }

        /// <summary>
        /// Текст в подсказке для GUI
        /// </summary>
        public override string GetToolTip(ItemStack stack)
            => string.Format(_toolTip,
                Level, (int)(Fartune * 100),
                stack.ToStringDamaged(),
                Pause);
    }
}
