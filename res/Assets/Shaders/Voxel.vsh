#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;
layout(location = 2) in int v_rgbl;
layout(location = 3) in int v_anim;

out vec4 a_color;
out vec2 a_texCoord;
out float fog_factor;
out vec3 fog_color;
out vec2 a_light;
out float a_sharpness;

uniform mat4 view;
uniform float takt;
uniform float wind;
uniform float overview;
uniform vec3 colorfog;
uniform vec3 pos;
uniform vec3 camera;
uniform float torch;

void main()
{
    fog_color = colorfog;
    float camera_distance = distance(camera, vec3(v_position));
    fog_factor = pow(clamp(camera_distance / overview, 0.0, 1.0), 4.0);

    float r = (v_rgbl & 0xFF) / 255.0;
    float g = ((v_rgbl >> 8) & 0xFF) / 255.0;
    float b = ((v_rgbl >> 16) & 0xFF) / 255.0;
    
    float lightSky = ((v_rgbl >> 24) & 0xF) / 16.0 + 0.03125;
    float lightBlock = ((v_rgbl >> 28) & 0xF) / 16.0 + 0.03125;
    
    if (torch > 0 && camera_distance < torch)
    {
        float t2 = torch / 1.4;
        if (camera_distance < t2)
        {
            float lb = (t2 - camera_distance) / t2 * torch / 16.0;
            if (lb > lightBlock) lightBlock = lb;
        }
    }

    a_light = vec2(lightBlock, lightSky);
    a_texCoord = v_texCoord;
	a_sharpness = (v_anim >> 18) & 1;
	
    int frame = (v_anim & 0xFF);
    if (frame > 0)
    {
        int pause = ((v_anim >> 8) & 0xFF);
        int t;
        if (pause > 1) {
            int maxframe = frame * pause;
            int tt = maxframe - 1;
            t = ((int(takt) & tt) / pause);
        } else {
            int tt = frame - 1;
            t = (int(takt) & tt);
        }
        a_texCoord.y += t * 0.015625;
    }
    
	if (((v_anim >> 16) & 1) == 1)
    {
        vec3 posanim = v_position;
        posanim.x += wind;
        posanim.z += wind;
        gl_Position = view * vec4(pos + posanim, 1.0);
    }
    else
    {
        gl_Position = view * vec4(pos + v_position, 1.0);
    }
    a_color = vec4(r, g, b, 1.0);
}