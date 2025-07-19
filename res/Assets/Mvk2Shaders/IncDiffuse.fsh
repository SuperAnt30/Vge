in vec3 a_normal; // Нормаль вершины
in vec3 a_lightDir; // Вектор к солнцу
in float a_brightness; // Яркость тени 1.0 чёрный, 0.1 светлый

// Калькулятор рассеянного освещения 
float DiffuseCalculation()
{
    // Скалярное произведение между нормали и луча солнца
    float scalar = dot(a_normal, a_lightDir);
    // Дифузию света стороны
    float diffuse = max(scalar, 0.0);
    if (diffuse > 1.0) diffuse = 1.0;
    // Смегчаем дифузию
    return (1.0 - diffuse) * a_brightness;
}
