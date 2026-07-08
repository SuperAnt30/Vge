using System.Collections.Generic;
using Vge.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Объект списка возможных задач искусственного интеллекта моба
    /// </summary>
    public class EntityAITasks
    {
        /// <summary>
        /// Список всех задача
        /// </summary>
        private readonly List<EntityAITaskEntry> _taskEntries = new List<EntityAITaskEntry>();
        /// <summary>
        /// Список выполняемых задач
        /// </summary>
        private readonly List<EntityAITaskEntry> _executingTaskEntries = new List<EntityAITaskEntry>();
        /// <summary>
        /// Счётчик тиков
        /// </summary>
        private int _tickCount;

        public EntityAITasks() { }
        

        /// <summary>
        /// Добавить задачу
        /// </summary>
        /// <param name="priority">Приоритет задачи</param>
        /// <param name="action">Объект задачи</param>
        public void AddTask(int priority, EntityAIBase task) => _taskEntries.Add(new EntityAITaskEntry(priority, task));
        /// <summary>
        /// Удалить задачу, проверка по ссылке, задача должна быть одним и тем же объектом
        /// </summary>
        public void RemoveTask(EntityAIBase task)
        {
            int index = -1;
            for (int i = 0; i < _taskEntries.Count; i++)
            {
                if (_taskEntries[i].Task == task)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                EntityAITaskEntry taskEntry = _taskEntries[index];

                if (_executingTaskEntries.Contains(taskEntry))
                {
                    taskEntry.Task.ResetTask();
                    _executingTaskEntries.Remove(taskEntry);
                }
                _taskEntries.RemoveAt(index);
            }
        }

        /// <summary>
        /// Обновить задачи
        /// </summary>
        public void OnUpdateTasks()
        {
            int i;
            // Выборка запуска задачи
            EntityAITaskEntry taskEntry;

            if (_tickCount++ % 3 == 0)
            {
                // Каждый третий такт мы тут
                for (i = 0; i < _taskEntries.Count; i++)
                {
                    taskEntry = _taskEntries[i];
                    if (_executingTaskEntries.Contains(taskEntry))
                    {
                        // эта задача выполняется
                        if (CanUse(taskEntry) && taskEntry.Task.ContinueExecuting()) continue;
                        // Задачу надо прервать
                        taskEntry.Task.ResetTask();
                        _executingTaskEntries.Remove(taskEntry);
                    }
                    if (CanUse(taskEntry) && taskEntry.Task.ShouldExecute())
                    {
                        // Начинаем выполнять задачу
                        taskEntry.Task.StartExecuting();
                        _executingTaskEntries.Add(taskEntry);
                    }
                }
            }
            else if (_executingTaskEntries.Count > 0)
            {
                // Тут проверка на окночание задачи
                List<int> indexs = new List<int>();
                for (i = 0; i < _executingTaskEntries.Count; i++)
                {
                    taskEntry = _executingTaskEntries[i];
                    if (!taskEntry.Task.ContinueExecuting())
                    {
                        taskEntry.Task.ResetTask();
                        indexs.Add(i);
                    }
                }
                // Удалить в списке задача
                for (i = indexs.Count - 1; i >= 0; i--)
                {
                    _executingTaskEntries.RemoveAt(indexs[i]);
                }
            }

            // Выполнение активной задачи
            for (i = 0; i < _executingTaskEntries.Count; i++)
            {
                _executingTaskEntries[i].Task.UpdateTask();
            }
        }

        /// <summary>
        /// Определите, может ли конкретная задача быть выполнена, что означает, 
        /// что все запущенные задачи с более высоким приоритетом совместимы с ней 
        /// или все задачи с более низким приоритетом могут быть прерваны.
        /// </summary>
        private bool CanUse(EntityAITaskEntry inTaskEntry)
        {
            EntityAITaskEntry taskEntry;
            for (int i = 0; i < _taskEntries.Count; i++)
            {
                taskEntry = _taskEntries[i];
                if (taskEntry != inTaskEntry)
                {
                    if (inTaskEntry.Priority >= taskEntry.Priority)
                    {
                        if (!((inTaskEntry.Task.MutexBits & taskEntry.Task.MutexBits) == 0)
                            && _executingTaskEntries.Contains(taskEntry)) return false;
                    }
                    else if (!taskEntry.Task.IsInterruptible()
                        && _executingTaskEntries.Contains(taskEntry)) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Статус задачи с приоритетом
        /// </summary>
        class EntityAITaskEntry
        {
            public readonly EntityAIBase Task;
            public readonly int Priority;

            public EntityAITaskEntry(int priority, EntityAIBase task)
            {
                Priority = priority;
                Task = task;
            }
        }
    }
}
