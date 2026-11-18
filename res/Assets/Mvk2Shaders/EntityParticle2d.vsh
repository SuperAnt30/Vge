#version 330 core

// Общий квадрат частицы (одинаковый для всех экземпляров)
layout(location = 0) in vec2 v_position;
layout(location = 1) in vec2 v_texCoord;

// Данные экземпляра (glVertexAttribDivisor = 1), 16 float = 64 байта
layout(location = 2) in vec4 i_posScale;   // xyz — позиция относительно камеры, w — масштаб
layout(location = 3) in vec4 i_color;      // цвет
layout(location = 4) in vec4 i_uv;         // x1, y1, x2, y2 в атласе
layout(location = 5) in vec4 i_lightParam; // x — свет блока, y — свет неба, z — param, w — резерв

out vec2 a_texCoord;
flat out vec4 a_color;
flat out vec2 a_light;
flat out int a_param;

uniform mat4 view;
uniform mat4 rotateMatrix;

void main()
{
    a_param = int(i_lightParam.z);
    a_light = i_lightParam.xy;
    a_color = i_color;

    // Текстура из атласа: v_texCoord 0 -> uv.xy, 1 -> uv.zw (без ветвлений)
    if ((a_param & 1) == 1) {
        if (v_texCoord.x == 0) a_texCoord.x = i_uv.x;
        else a_texCoord.x = i_uv.z;
        if (v_texCoord.y == 0) a_texCoord.y = i_uv.y;
        else a_texCoord.y = i_uv.w;
    }
    else a_texCoord = v_texCoord;
    
    // То же, что view * model(scale, pos) * rotate * v, но без сборки матрицы на каждую вершину
    vec3 local = (rotateMatrix * vec4(v_position, 0.0, 1.0)).xyz;
    gl_Position = view * vec4(local * i_posScale.w + i_posScale.xyz, 1.0);
}
