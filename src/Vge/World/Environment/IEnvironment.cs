using Vge.Entity.Player;
using Vge.NBT;
using Vge.Network.Packets.Server;
using WinGL.Util;

namespace Vge.World.Environment
{
    /// <summary>
    /// Время, погода, освещение
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Увеличивается каждый игровой тик
        /// </summary>
        uint TickCounter { get; }

        /// <summary>
        /// Обновление раз в тик на клиенте
        /// </summary>
        void UpdateClient();

        /// <summary>
        /// Обновление раз в тик на сервере
        /// </summary>
        void UpdateServer(WorldServer worldServer);

        /// <summary>
        /// Обновление во фрейме, и возвращает было ли изменение
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        void UpdateFrame(float timeIndex);

        /// <summary>
        /// Задать погоду клиенту
        /// </summary>
        void SetEnvironmentClient(byte value);
        /// <summary>
        /// Задать погоду серверу
        /// </summary>
        void SetEnvironmentServer(byte value);

        /// <summary>
        /// Внести изменение по мировому времени
        /// </summary>
        void SetTickCounter(uint tickCounter);

        /// <summary>
        /// Сколько игровых тактов длится день
        /// </summary>
        int GetSpeedDay();

        /// <summary>
        /// Получить нормализованный вектор источника света
        /// </summary>
        Vector3 GetVectorLight();

        /// <summary>
        /// Получить яркость солнца, 0.0 - 1.0
        /// </summary>
        float GetSunLight();

        /// <summary>
        /// Получить яркость луны, 0.0 - 0.5
        /// </summary>
        float GetMoonLight();

        /// <summary>
        /// Получить небесный угол
        /// </summary>
        float GetCelestialAngle();

        /// <summary>
        /// Получить небесныц свет ночь 0..3 в зависимости от фазы луны, день 15
        /// </summary>
        int GetSkylightSubtracted();

        /// <summary>
        /// Получить цвет неба
        /// </summary>
        Vector3 GetColorSky();

        /// <summary>
        /// Получить цвет тумана
        /// </summary>
        Vector3 GetColorFog();

        /// <summary>
        /// Пара года
        /// </summary>
        EnumTimeYear TimeYear { get; }

        /// <summary>
        /// Проверяет, является ли сейчас дневное время, определяя по яркости неба
        /// </summary>
        bool IsDayTime();
        /// <summary>
        /// Параметр ветра
        /// </summary>
        float GetWind();
        /// <summary>
        /// Присоединён игрок, передаём данные пакета
        /// </summary>
        void JoinWorld(PlayerServer player);

        #region NBT

        /// <summary>
        /// Сохранить данные
        /// </summary>
        void WriteToNBT(TagCompound nbt);
        /// <summary>
        /// Прочесть данные
        /// </summary>
        void ReadFromNBT(TagCompound nbt);

        #endregion
    }
}
