using Vge.Realms;

namespace Mvk2.Realms
{
    /// <summary>
    /// Объект который запускает текстуры, звук, и формирует всё для игры
    /// </summary>
    public class LoadingMvk : Loading
    {
        /// <summary>
        /// Объект окна малювек
        /// </summary>
        private readonly WindowMvk window;

        public LoadingMvk(WindowMvk window) => this.window = window;

        /// <summary>
        /// Максимальное количество шагов
        /// </summary>
        public override int GetMaxCountSteps()
        {
            return 100;
        }
        /// <summary>
        /// Этот метод как раз и реализует список загрузок
        /// </summary>
        protected override void Steps()
        {
            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread.Sleep(5);
                OnStep();
            }
        }

    }
}
