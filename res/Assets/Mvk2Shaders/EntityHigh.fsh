#version 330 core
 
in vec2 a_light;
in vec2 a_texCoord;
in float a_depth;
in float a_eyeLips;
in vec4 a_fragToLight;

out vec4 f_color;

uniform sampler2DArray sampler_small;
uniform sampler2DArray sampler_big;
uniform sampler2D light_map;
uniform sampler2D depth_map;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    // Выполняем деление перспективы
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    
    // Преобразуем в диапазон [0,1]
    projCoords = projCoords * 0.5 + 0.5;
 
    
    // Получаем наиболее близкое значение глубины, исходя из перспективы с точки зрения источника света (используя диапазон [0,1] fragPosLight в качестве координат)
    float closestDepth = texture(depth_map, projCoords.xy).r; 
 
    // Получаем глубину текущего фрагмента, исходя из перспективы с точки зрения источника света
    float currentDepth = projCoords.z;
 
    // Проверяем, находится ли текущий фрагмент в тени
    //float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;
    
    float bias = 0.0005;
    
    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(depth_map, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(depth_map, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
        }    
    }
    shadow /= 9.0;
    //shadow /= 3.0;
    
    // Проверка нахождения текущего фрагмента в тени
    //shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
    
    
    // Оставляем значение тени на уровне 0.0 за границей дальней плоскости пирамиды видимости глазами источника света
    if(projCoords.z > 1.0) shadow = 0.0;
 
    return 1.0 - shadow;
}

void main()
{
    if (a_eyeLips > 0) discard;
    float depth = a_depth;
    bool big = depth > 65535;
    if (big) depth -= 65536;
    vec3 uv = vec3(a_texCoord, depth);
    vec4 tex_color;
    if (big) tex_color = texture(sampler_big, uv);
    else tex_color = texture(sampler_small, uv);
    if (tex_color.a < 0.1) discard;
    
    float shadow = ShadowCalculation(a_fragToLight);
    vec4 color = tex_color * texture(light_map, a_light);
    vec3 col3 = vec3(color) * shadow;
    f_color = vec4(col3, color.a);
}