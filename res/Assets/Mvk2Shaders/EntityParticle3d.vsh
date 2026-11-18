#version 330 core

// Общий кубик частицы (одинаковый для всех экземпляров)
layout(location = 0) in vec3 v_position;
layout(location = 1) in float v_color;     // затемнение стороны

// Данные экземпляра (glVertexAttribDivisor = 1)
layout(location = 2) in vec4 i_posScale;   // xyz — позиция относительно камеры, w — масштаб
layout(location = 3) in vec4 i_color;      // цвет
layout(location = 4) in vec4 i_lightParam; // x — свет блока, y — свет неба, z — param, w — резерв

out vec4 a_color;
flat out vec2 a_light;
flat out int a_param;

uniform mat4 view;

void main()
{
    a_param = int(i_lightParam.z);
    a_light = i_lightParam.xy;
    // Затемнение только для RGB, альфа из цвета частицы
    a_color = vec4(i_color.rgb * v_color, i_color.a);

    gl_Position = view * vec4(v_position * i_posScale.w + i_posScale.xyz, 1.0);
}
