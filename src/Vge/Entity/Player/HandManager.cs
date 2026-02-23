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


        public void SetStop()
        {
            _action = false;
            _second = false;
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
                _Second(!_secondPrev);
            }
            else if (_secondPrev)
            {
                _SecondEnd();
            }
            _actionPrev = _action;
            _secondPrev = _second;
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
