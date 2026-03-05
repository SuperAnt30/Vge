using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Json;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Item
{
    /// <summary>
    /// Базовый объект Предмета
    /// </summary>
    public class ItemBase
    {
        /// <summary>
        /// Индекс предмета из таблицы
        /// </summary>
        public ushort IndexItem { get; private set; }
        /// <summary>
        /// Псевдоним предмета из таблицы
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Название предмета на текущем языке
        /// </summary>
        public string CurentName { get; protected set; } = "";
        /// <summary>
        /// Описание предмета на текущем языке
        /// </summary>
        public string Description { get; protected set; } = "";

        /// <summary>
        /// Максимальное количество однотипный вещей в одной ячейке
        /// </summary>
        public byte MaxStackSize { get; protected set; }
        /// <summary>
        /// Максимальное количество урона, при 0, нет учёта урона
        /// </summary>
        public int MaxDamage { get; protected set; }

        /// <summary>
        /// Пол ширина предмета
        /// </summary>
        public float Width { get; protected set; } = .4375f;
        /// <summary>
        /// Высота предмета
        /// </summary>
        public float Height { get; protected set; } = .125f;
        /// <summary>
        /// Вес предмета. В килограммах.
        /// </summary>
        public int Weight { get; protected set; } = 2;
        /// <summary>
        /// Коэффициент рикошета, 0 нет отскока, 1 максимальный
        /// </summary>
        public float Rebound { get; protected set; } = .5f;

        /// <summary>
        /// Объект буферов для рендера
        /// </summary>
        public ItemRenderBuffer Buffer { get; protected set; }

        /// <summary>
        /// Название анимации держать, если не указана, то будет по умолчанию 
        /// </summary>
        public string Hold { get; protected set; } = "";

        /// <summary>
        /// Массив ключей ячеек одежды инвентаря, куда можно устанавливать этот предмет.
        /// Если null, то можно установить только в ячейку с ключём 0
        /// </summary>
        protected byte[] _slotClothIndex;

        #region Init

        /// <summary>
        /// Инициализация предметов данные с json, 
        /// </summary>
        public virtual void Init(JsonCompound state)
        {
            _ReadStateFromJson(state);
            // Создание буфера для рендера
            Buffer = new ItemRenderBuffer();
        }
        /// <summary>
        /// Удалить объект данных буфера, он уже должен быть в буфере GPU
        /// </summary>
        public void BufferClear()
        {
            Buffer.Clear();
            //Buffer = null;
        }

        /// <summary>
        /// Задать индекс, из таблицы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndex(ushort id) => IndexItem = id;

        /// <summary>
        /// Задать псевдоним
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAlias(string alias) => Alias = alias;

        #endregion

        /// <summary>
        /// Проверить может ли предмет быть в текущем слоте одежды с данным ключём
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CheckSlotClothKey(byte key)
        {
            if (_slotClothIndex == null) return key == 0;
            foreach(byte slot in _slotClothIndex)
            {
                if (slot == key) return true;
            }
            return false;
        }

        /// <summary>
        /// Имеется ли замена предмета при выборе его в StackAir
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CheckToAir() => false;

        /// <summary>
        /// Заменить предмет из-за воздуха, при выборе его в StackAir
        /// TODO::2025-09-22 это надо горящий факел, чтоб зутухал, для горяжего факела, сделать доп индекс хранения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ushort IndexReplaceItemDueAir() => 0;

        #region Дейстыия рук, ЛКМ и ПКМ

        /// <summary>
        /// Проверка, может ли блок устанавливаться в этом месте
        /// </summary>
        /// <param name="blockPos">Координата блока, по которому щелкают правой кнопкой мыши</param>
        /// <param name="block">Объект блока который хотим установить</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        protected virtual bool _CanPlaceBlockOnSide(ItemStack stack, PlayerBase player, WorldBase world, 
            BlockPos blockPos, BlockBase block, Pole side, Vector3 facing)
        {
            //if (world.IsRemote) return true;

            BlockState blockState = world.GetBlockState(blockPos);
            BlockBase blockOld = blockState.GetBlock();
            // Проверка ставить блоки, на те которые можно, к примеру на траву
            if (!blockOld.IsReplaceable) return false;
            // Если устанавливаемый блок такой же как стоит
            if (blockOld == block) return false;
            // Если стак пуст
            if (stack == null || stack.Amount == 0) return false;

            //if (world.IsRemote) return true;
            bool isCheckCollision = !block.IsCollidable;
            if (!isCheckCollision)
            {
                AxisAlignedBB axisBlock = block.GetCollisionOne(blockPos, blockState.Met);
                // Проверка коллизии сущностей и блока
                isCheckCollision = !player.Size.IntersectsWith(axisBlock);
                    //world.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axisBlock, player.Id).Count == 0;
            }

            return isCheckCollision;
        }

        /// <summary>
        /// Действие предмета ЛКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        public virtual ResultHandAction OnAction(bool begin, ItemStack stack, PlayerClientOwner player)
        {
            MovingObjectPosition moving = player.MovingObject;
            if (moving.IsEntity())
            {
                // Урон для сущности
                return new ResultHandAction(ResultHandAction.ActionType.AttackEntity);
            }
            if (moving.IsBlock() && player.CanDestroyedBlock(moving.Block.GetBlock()))
            {
                // Можно разрушить блок без предмета
                return new ResultHandAction(8, 1);
            }
            return new ResultHandAction(ResultHandAction.ActionType.None);
        }

        /// <summary>
        /// Действие текущего предмета.
        /// Для сервера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnUseAction(ItemStack stack, PlayerServer player) { }

        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public virtual ResultHandSecond OnSecond(bool begin, ItemStack stack, 
            PlayerClientOwner player, int counter)
        {
            MovingObjectPosition moving = player.MovingObject;
            if (moving.IsEntity())
            {
                return new ResultHandSecond(ResultHandSecond.ActionType.InteractEntity);
            }
            return new ResultHandSecond(ResultHandSecond.ActionType.None);
        }

        /// <summary>
        /// Окончание вспомогательного действия предмета ПКМ, вызывается после отпущения клавиши или смены предмета.
        /// Для клиента.
        /// Возвращает Дополнительный цифровой параметр, если -1 нет действий
        /// </summary>
        /// <param name="abort">Бало ли астановка из-за отмены</param>
        /// <param name="counter">счётчик тактов от нажатия</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual int OnSecondEnd(ItemStack stack, PlayerClientOwner player, 
            bool abort, int counter) => -1;

        /// <summary>
        /// Вспомогательное действие текущего предмета.
        /// Для сервера
        /// </summary>
        /// <param name="number">Дополнительный цифровой параметр</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnUseSecond(ItemStack stack, PlayerServer player, int number) { }

        /// <summary>
        /// Вызывается, когда текущий предмет пробуюет установить на блок,
        /// возвращает true если действие состоялось
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают ПКМ</param>
        /// <param name="side">Сторона, по которой щелкнули ПКМ</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="flagReplaceable">Надо ли проверять смещение на установку блока параметра IsReplaceable</param>
        public virtual bool OnItemOnBlockPlacement(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing, bool flagReplaceable) => false;

        #endregion

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        protected virtual void _ReadStateFromJson(JsonCompound state)
        {
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey(Cti.MaxStackSize))
                        {
                            int size = json.GetInt();
                            if (size > 255) size = 255;
                            if (size < 1) size = 1;
                            MaxStackSize = (byte)size;
                        }
                        else if (json.IsKey(Cti.MaxDamage)) MaxDamage = json.GetInt();
                        else if (json.IsKey(Cti.SlotClothIndex))
                        {
                            if (json.IsValue()) // byte
                            {
                                _slotClothIndex = new byte[] { (byte)json.GetInt() };
                            }
                            else if (json.IsArray()) // Array
                            {
                                _slotClothIndex = json.GetArray().ToArrayByte();
                            }
                        }
                        else if (json.IsKey(Cti.Width)) Width = json.GetFloat();
                        else if (json.IsKey(Cti.Height)) Height = json.GetFloat();
                        else if (json.IsKey(Cti.Weight)) Weight = json.GetInt();
                        else if (json.IsKey(Cti.Rebound)) Rebound = json.GetFloat();
                        else if (json.IsKey(Cti.Hold)) Hold = json.GetString();
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonItemStat, Alias));
                }
            }
        }

        #endregion



        public override string ToString() => IndexItem.ToString() + " " + Alias;
    }
}
