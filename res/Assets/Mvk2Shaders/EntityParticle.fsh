#version 330 core

in vec4 a_color;
in vec2 a_light;

out vec4 f_color;

uniform sampler2D atlas;
uniform sampler2D light_map;

void main()
{
	//f_color = a_color;
    
    //vec4 tex_color;
    // Текстура атласа блоков и предметов
    //tex_color = texture(atlas, a_texCoord);
        
    //vec4 color = tex_color * texture(light_map, a_light);
    //vec4 color = tex_color * texture(light_map, a_light);
    //vec3 col3 = vec3(color) * (1.0 - shadow);
    //f_color = vec4(col3, color.a);
    //f_color = a_color * texture(light_map, a_light);;
    
    vec4 light_color = texture(light_map, a_light);
    
    f_color = a_color * light_color;
}