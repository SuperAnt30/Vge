#version 330 core
 
in vec2 a_light;
in vec2 a_texCoord;
in float a_depth;

out vec4 f_color;

uniform sampler2DArray sampler_small;
uniform sampler2DArray sampler_big;
uniform sampler2D light_map;

void main()
{
    float depth = a_depth;
    bool big = depth > 65535;
    if (big) depth -= 65536;
    vec3 uv = vec3(a_texCoord, depth);
    vec4 tex_color;
    if (big) tex_color = texture(sampler_big, uv);
    else tex_color = texture(sampler_small, uv);
    if (tex_color.a < 0.1) discard;
    f_color = tex_color * texture(light_map, a_light);
}