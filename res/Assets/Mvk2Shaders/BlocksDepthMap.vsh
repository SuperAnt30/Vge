#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;
layout(location = 2) in int v_rgbl;
layout(location = 3) in int v_anim;
layout(location = 4) in int v_normal;

out vec4 a_color;
out vec2 a_texCoord;
out float a_sharpness;

uniform mat4 view;
uniform int takt;
uniform float animOffset;
uniform float wind;
uniform vec3 player;

uniform vec2 chunk;

void main()
{
	vec3 pos = vec3(chunk.x - player.x, -player.y, chunk.y - player.z);

    a_texCoord = v_texCoord;
    a_sharpness = (v_anim >> 18) & 1;
    
    int frame = (v_anim & 0xFF);
    if (frame > 0)
    {
        int pause = ((v_anim >> 8) & 0xFF);
        int t;
        if (pause > 1) {
            int maxframe = frame * pause;
            t = (takt - takt / maxframe * maxframe) / pause;
        } else {
            t = takt - takt / frame * frame;
        }
        a_texCoord.y += t * animOffset;
    }
    
    if (((v_anim >> 16) & 1) == 1)
    {
        vec3 posanim = v_position;
        posanim.x += wind;
        posanim.z += wind;
        gl_Position = view * vec4(pos + posanim, 1.0);
    }
    else if (((v_anim >> 17) & 1) == 1)
    {
        vec3 posanim = v_position;
        posanim.y += wind * 0.5;
        gl_Position = view * vec4(pos + posanim, 1.0);
    }
    else
    {
        gl_Position = view * vec4(pos + v_position, 1.0);
    }
    a_color = vec4(1.0);
}