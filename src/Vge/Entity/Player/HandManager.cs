using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vge.Entity.Player
{
    /// <summary>
    /// Объект менеджер действий рук игрока. Левой и правой клавишей мыши.
    /// В прошлом было объект ItemInWorldManager, что-то подобное, но там именно очередь ударов
    /// </summary>
    public class HandManager
    {
        /// <summary>
        /// Активно ли действие ЛКМ
        /// </summary>
        private bool _action;
        /// <summary>
        /// Было ли активно действие ЛКМ в прошлом такте
        /// </summary>
        private bool _actionPrev;
        /// <summary>
        /// Активно ли вспомогательное действие ПКМ
        /// </summary>
        private bool _second;
        /// <summary>
        /// Было ли вспомогательное действие ПКМ в прошлом такте
        /// </summary>
        private bool _secondPrev;

        /// <summary>
        /// Пауза для активного действия (ЛКМ)
        /// </summary>
        protected int _pauseAction;
        /// <summary>
        /// Пауза для вспомогательное действие ПКМ
        /// </summary>
        protected int _pauseSecond;
        /// <summary>
        /// Была ли остановка с отменой, флаг для вспомогательного действия
        /// </summary>
        protected bool _flagAbort;

        /// <summary>
        /// Счётчик тактов от нажатия вспомогательное действия ПКМ
        /// </summary>
        protected int _counterSecond { get; private set; }

        public void SetStop()
        {
            _action = false;
            _second = false;
            _flagAbort = true;
        }

        /// <summary>
        /// Изменить состояние активного действия ЛКМ
        /// </summary>
        public void SetAction(bool action) => _action = action;

        /// <summary>
        /// Изменить состояние вспомогательного действия ПКМ
        /// </summary>
        public void SetSecond(bool action) => _second = action;
        
        /// <summary>
        /// В такте
        /// </summary>
        public void Update()
        {
            if (_action)
            {
                _Action(!_actionPrev);
            }
            else if (_actionPrev)
            {
                _ActionEnd();
            }
            if (_second)
            {
                if (!_secondPrev)
                {
                    _counterSecond = 0;
                    _flagAbort = false;
                }
                _Second(!_secondPrev);
            }
            else if (_secondPrev)
            {
                _SecondEnd();
            }
            _actionPrev = _action;
            _secondPrev = _second;
            _counterSecond++;
        }

        /// <summary>
        /// Такт действия
        /// </summary>
        protected virtual void _Action(bool begin) { }

        /// <summary>
        /// Конец действия
        /// </summary>
        protected virtual void _ActionEnd() { }

        /// <summary>
        /// Такт вспомогательного действия 
        /// </summary>
        protected virtual void _Second(bool begin) { }

        /// <summary>
        /// Конец вспомогательного действия
        /// </summary>
        protected virtual void _SecondEnd() { }
    }
}
