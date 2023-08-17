#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec4 vertexColor;

out vec4 color;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = vec4(position, 1.0) * model * view * projection;
	color = vertexColor;
}