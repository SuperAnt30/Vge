#version 330 core
 
in vec4 a_color;
in vec2 a_texCoord;
in float fog_factor;
in vec3 fog_color;
in vec2 a_light;
in float a_sharpness;

out vec4 f_color;

uniform sampler2D atlas_blurry;
uniform sampler2D atlas_sharpness;
uniform sampler2D light_map;

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
    
    vec4 color = a_color * tex_color * light_color;
    vec3 col3 = mix(vec3(color), fog_color, fog_factor);
    f_color = vec4(col3, color.a);
}