in vec4 a_fragToLight; // 
in vec3 a_normal; // Нормаль вершины
in vec3 a_lightDir; // Вектор к солнцу
in float a_brightness; // Яркость тени 1.0 чёрный, 0.1 светлый

// Калькулятор тени
float ShadowCalculation()
{
    // Выполняем деление перспективы
    vec3 projCoords = a_fragToLight.xyz / a_fragToLight.w;
    
    // Преобразуем в диапазон [0,1]
    projCoords = projCoords * 0.5 + 0.5;
    
    // Получаем наиболее близкое значение глубины, исходя из перспективы с точки зрения источника света (используя диапазон [0,1] fragPosLight в качестве координат)
    float closestDepth = texture(depth_map, projCoords.xy).r; 
 
    // Получаем глубину текущего фрагмента, исходя из перспективы с точки зрения источника света
    float currentDepth = projCoords.z;
 
    // Скалярное произведение между нормали и луча солнца
    float scalar = dot(a_normal, a_lightDir);
    // Смещение для тени
    float bias = 0.00025 * scalar;
    // Дифузию света стороны
    float diffuse = max(scalar, 0.0);
    if (diffuse > 1.0) diffuse = 1.0;
    // Смегчаем дифузию
    diffuse = (1.0 - diffuse) * a_brightness;
    
    // Проверка нахождения текущего фрагмента в тени
    float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
    
    if (shadow == 1.0)
    {
        // PCF для отсечения дефектных из-за Shadow acne, оставляем если по периметру тень
        shadow = 0.0;
        vec2 texelSize = 1.0 / textureSize(depth_map, 0);
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                float pcfDepth = texture(depth_map, projCoords.xy + vec2(x, y) * texelSize).r;
                if (currentDepth - bias > pcfDepth) shadow++;
            }    
        }
        if (shadow == 9) shadow = a_brightness; // Тень
        else shadow = 0.0; // Нет тени
    
        // Poisson Sampling (https://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/)
        vec2 poissonDisk[4] = vec2[](
            vec2( -0.74201624, -0.49906216 ),
            vec2( 0.74558609, -0.66890725 ),
            vec2( -0.394184101, -0.72938870 ),
            vec2( 0.44495938, 0.39387760 )
        );
        
        float ps = a_brightness / 5.0;
        for (int i = 0; i < 4; i++)
        {
            if (texture( depth_map, projCoords.xy + poissonDisk[i] / 900).r  >  projCoords.z-bias)
            {
                //if (shadow > 0.8) 
                shadow -= ps;
            }
        }
    }
    // Если тень от дифузии больше, то указываем её как основу
    if (diffuse > shadow) shadow = diffuse;
    return shadow;
}
