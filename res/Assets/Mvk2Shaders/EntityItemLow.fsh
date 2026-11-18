#version 330 core
 
in vec2 a_light;
in vec2 a_texCoord;

out vec4 f_color;

uniform sampler2D atlas;
uniform sampler2D light_map;

#include IncDiffuse.fsh

void main()
{
    // Текстура атласа блоков и предметов
    vec4 tex_color = texture(atlas, a_texCoord);
    if (tex_color.a < 0.1) discard;
    
    float diffuse = DiffuseCalculation();
    vec4 color = tex_color * texture(light_map, a_light);
    vec3 col3 = vec3(color) * (1.0 - diffuse);
    f_color = vec4(col3, color.a);
}