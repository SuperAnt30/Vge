#version 330 core
 
in vec2 a_texCoord;
in float a_depth;

out vec4 f_color;

uniform sampler2DArray sampler_small;
uniform sampler2DArray sampler_big;
uniform sampler2D atlas;

#include IncDiffuse.fsh

void main()
{
    vec4 tex_color;
    if (a_depth < 0.0)
    {
        // Текстура атласа блоков и предметов
        tex_color = texture(atlas, a_texCoord);
    }
    else
    {
        float depth = a_depth;
        bool big = depth > 65535;
        if (big) depth -= 65536;
        vec3 uv = vec3(a_texCoord, depth);
        if (big) tex_color = texture(sampler_big, uv);
        else tex_color = texture(sampler_small, uv);
    }
    if (tex_color.a < 0.1) discard;
    
    float diffuse = DiffuseCalculation();
    vec3 col3 = vec3(tex_color) * (1.0 - diffuse);
    f_color = vec4(col3, tex_color.a);
}