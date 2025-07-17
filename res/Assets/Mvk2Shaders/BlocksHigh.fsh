#version 330 core
 
in vec4 a_color;
in vec2 a_texCoord;
in float fog_factor;
in vec3 fog_color;
in vec2 a_light;
in float a_sharpness;
in vec4 a_fragToLight;

out vec4 f_color;

uniform sampler2D atlas_blurry;
uniform sampler2D atlas_sharpness;
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
    shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
    
    // Проверяем, находится ли текущий фрагмент в тени
    //shadow = currentDepth > closestDepth  ? 1.0 : 0.0;
    vec2 poissonDisk[4] = vec2[](
        vec2( -0.74201624, -0.49906216 ),
        vec2( 0.74558609, -0.66890725 ),
        vec2( -0.394184101, -0.72938870 ),
        vec2( 0.44495938, 0.39387760 )
    );
    shadow = 1;
    if ( texture( depth_map, projCoords.xy).r  <  projCoords.z - bias)
    {
        shadow = 0.5;
    }
/*
    for (int i=0; i<4; i++)
    {
        if ( texture( depth_map, projCoords.xy + poissonDisk[i] / 900).r  <  projCoords.z-bias )
        {
            shadow-=0.2;
        }
    }
    
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(depth_map, projCoords.xy + vec2(x, y) * texelSize).r; 
            if (currentDepth - bias > pcfDepth)
            {
                shadow -= 0.025;
            }
            //shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
        }    
    }
    shadow = 1.0 - (1.0 - shadow)/3.0;
*/
    // Оставляем значение тени на уровне 0.0 за границей дальней плоскости пирамиды видимости глазами источника света
    if(projCoords.z > 1.0) shadow = 0.0;
 
    return shadow;
    //return 1.0 - shadow;
}

void main()
{
    vec4 tex_color;
    if (a_sharpness == 0)
    {
        tex_color = texture(atlas_blurry, a_texCoord);
    }
    else
    {
        tex_color = texture(atlas_sharpness, a_texCoord);
    }
    
    if (tex_color.a < 0.1) discard;
    vec4 light_color = texture(light_map, a_light);
    
    float shadow = ShadowCalculation(a_fragToLight);
    
    vec4 color = a_color * tex_color * light_color;
    vec3 col3 = vec3(color) * shadow;
    col3 = mix(col3, fog_color, fog_factor);
    f_color = vec4(col3, color.a);
}