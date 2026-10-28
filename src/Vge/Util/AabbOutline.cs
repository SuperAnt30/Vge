using System;
using System.Collections.Generic;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Построение контура (видимых рёбер) фигуры, полученной объединением
    /// массива AxisAlignedBB (булева операция "Или").
    /// Внутренние рёбра и стыки между рамками в результат не попадают,
    /// соосные отрезки склеиваются в одну линию.
    /// 
    /// Объект написан ИИ (Cloud Opus 5.5 от 28.10.2026)
    /// Поверхностно работает, но детально не проверял
    /// </summary>
    public static class AabbOutline
    {
        /// <summary>
        /// Погрешность сравнения координат
        /// </summary>
        private const float _eps = 0.00001f;

        /// <summary>
        /// Построить контур. Результат — пары точек (начало, конец) для GL_LINES.
        /// </summary>
        public static List<Vector3> Build(AxisAlignedBB[] boxes)
        {
            List<Vector3> lines = new List<Vector3>();
            if (boxes == null || boxes.Length == 0) return lines;

            // 1. Сжатие координат: все уникальные границы рамок по каждой оси
            float[][] coords = new float[3][];
            for (int a = 0; a < 3; a++) coords[a] = _Coords(boxes, a);

            int nx = coords[0].Length - 1;
            int ny = coords[1].Length - 1;
            int nz = coords[2].Length - 1;
            if (nx < 1 || ny < 1 || nz < 1) return lines;

            // 2. Сетка ячеек: ячейка заполнена, если входит хотя бы в одну рамку ("Или")
            bool[,,] cells = new bool[nx, ny, nz];
            for (int n = 0; n < boxes.Length; n++)
            {
                Vector3 min = boxes[n].Min;
                Vector3 max = boxes[n].Max;
                // Вырожденные (плоские) рамки пропускаем
                if (max.X - min.X < _eps || max.Y - min.Y < _eps || max.Z - min.Z < _eps) continue;

                int x0 = _Index(coords[0], min.X), x1 = _Index(coords[0], max.X);
                int y0 = _Index(coords[1], min.Y), y1 = _Index(coords[1], max.Y);
                int z0 = _Index(coords[2], min.Z), z1 = _Index(coords[2], max.Z);

                for (int x = x0; x < x1; x++)
                    for (int y = y0; y < y1; y++)
                        for (int z = z0; z < z1; z++)
                            cells[x, y, z] = true;
            }

            // 3. Поиск рёбер вдоль каждой из осей
            for (int axis = 0; axis < 3; axis++)
            {
                _ScanAxis(axis, coords, cells, lines);
            }
            return lines;
        }

        /// <summary>
        /// Проход по всем линиям сетки, параллельным оси axis.
        /// Для каждого участка линии смотрим 4 ячейки вокруг него и решаем, ребро ли это.
        /// Подряд идущие рёбра склеиваются в один отрезок.
        /// </summary>
        private static void _ScanAxis(int axis, float[][] coords, bool[,,] cells, List<Vector3> lines)
        {
            float[] ca = coords[axis];
            float[] cb = coords[(axis + 1) % 3];
            float[] cc = coords[(axis + 2) % 3];
            int na = ca.Length - 1;

            for (int j = 0; j < cb.Length; j++)
            {
                for (int k = 0; k < cc.Length; k++)
                {
                    int start = -1;
                    // i == na — служебный шаг, чтобы закрыть последний отрезок
                    for (int i = 0; i <= na; i++)
                    {
                        bool edge = i < na && _IsEdge(
                            _Cell(cells, axis, i, j - 1, k - 1),
                            _Cell(cells, axis, i, j, k - 1),
                            _Cell(cells, axis, i, j, k),
                            _Cell(cells, axis, i, j - 1, k));

                        if (edge)
                        {
                            if (start < 0) start = i;
                        }
                        else if (start >= 0)
                        {
                            lines.Add(_Point(axis, ca[start], cb[j], cc[k]));
                            lines.Add(_Point(axis, ca[i], cb[j], cc[k]));
                            start = -1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Является ли участок рёбром контура по 4 ячейкам вокруг него (по кругу a, b, c, d).
        /// 1 или 3 заполненных — выпуклый или вогнутый угол, ребро.
        /// 2 по диагонали — две рамки касаются ребром, ребро.
        /// 0, 4 или 2 соседних — пустота, монолит или ровная грань, ребра нет.
        /// </summary>
        private static bool _IsEdge(bool a, bool b, bool c, bool d)
        {
            int n = (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0) + (d ? 1 : 0);
            if (n == 1 || n == 3) return true;
            return n == 2 && a == c;
        }

        /// <summary>
        /// Получить ячейку в локальных координатах оси (i — вдоль оси, j и k — две другие оси по кругу).
        /// За пределами сетки — пусто.
        /// </summary>
        private static bool _Cell(bool[,,] cells, int axis, int i, int j, int k)
        {
            int x, y, z;
            switch (axis)
            {
                case 0: x = i; y = j; z = k; break;
                case 1: y = i; z = j; x = k; break;
                default: z = i; x = j; y = k; break;
            }
            return x >= 0 && y >= 0 && z >= 0
                && x < cells.GetLength(0) && y < cells.GetLength(1) && z < cells.GetLength(2)
                && cells[x, y, z];
        }

        /// <summary>
        /// Перевести локальные координаты оси в мировую точку
        /// </summary>
        private static Vector3 _Point(int axis, float a, float b, float c)
        {
            switch (axis)
            {
                case 0: return new Vector3(a, b, c);
                case 1: return new Vector3(c, a, b);
                default: return new Vector3(b, c, a);
            }
        }

        /// <summary>
        /// Отсортированные уникальные координаты границ рамок по оси
        /// </summary>
        private static float[] _Coords(AxisAlignedBB[] boxes, int axis)
        {
            List<float> list = new List<float>(boxes.Length * 2);
            for (int n = 0; n < boxes.Length; n++)
            {
                list.Add(_Comp(boxes[n].Min, axis));
                list.Add(_Comp(boxes[n].Max, axis));
            }
            list.Sort();

            List<float> result = new List<float>(list.Count);
            for (int n = 0; n < list.Count; n++)
            {
                if (result.Count == 0 || list[n] - result[result.Count - 1] > _eps)
                {
                    result.Add(list[n]);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Индекс ближайшей координаты в массиве
        /// </summary>
        private static int _Index(float[] arr, float value)
        {
            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < arr.Length; i++)
            {
                float d = Math.Abs(arr[i] - value);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }
            return best;
        }

        private static float _Comp(Vector3 v, int axis)
            => axis == 0 ? v.X : (axis == 1 ? v.Y : v.Z);
    }
}