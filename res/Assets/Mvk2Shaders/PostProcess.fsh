#version 330 core

in vec2 a_uv;
out vec4 f_color;

uniform sampler2D u_color;       // цвет сцены
uniform sampler2D u_depth;       // глубина сцены
uniform vec2 u_near_far;         // ближняя и дальняя плоскость проекции
uniform float u_time;            // время в тиках, для анимации
uniform int u_underwater;        // 1 — камера под водой
uniform vec3 u_underwater_color; // цвет воды
uniform float u_vignette;        // сила виньетки, 0 — выключена

// Глубина из буфера (0..1) в расстояние до камеры в блоках
float linearDepth(float d)
{
    float n = u_near_far.x;
    float f = u_near_far.y;
    float z = d * 2.0 - 1.0;
    return 2.0 * n * f / (f + n - z * (f - n));
}

void main()
{
    vec2 uv = a_uv;

    if (u_underwater == 1)
    {
        // Лёгкое колыхание картинки
        uv += vec2(sin(uv.y * 30.0 + u_time * 0.15), cos(uv.x * 30.0 + u_time * 0.15)) * 0.0025;
    }

    vec3 color = texture(u_color, uv).rgb;

    if (u_underwater == 1)
    {
        // Под водой видимость падает с расстоянием: смешиваем с цветом воды по глубине
        float dist = linearDepth(texture(u_depth, uv).r);
        float fog = clamp(dist / 24.0, 0.0, 1.0);
        color = mix(color * u_underwater_color * 1.6, u_underwater_color, fog);
    }

    if (u_vignette > 0.0)
    {
        // Затемнение к краям экрана
        vec2 c = a_uv - 0.5;
        float v = 1.0 - dot(c, c) * u_vignette * 2.0;
        color *= clamp(v, 0.0, 1.0);
    }

    f_color = vec4(color, 1.0);
}
