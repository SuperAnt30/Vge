#version 330 core
 
in vec4 a_color;
in vec2 a_texCoord;
in float a_sharpness;

out vec4 f_color;

uniform sampler2D atlas_blurry;
uniform sampler2D atlas_sharpness;

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
    vec4 color = a_color * tex_color;
    f_color = vec4(vec3(color), color.a);
}