#version 330 core
out vec4 FragColor;

in vec4 color;
in float ambientOcclusion;

uniform bool wireframe;

void main()
{
	if (wireframe)
	{
		FragColor = vec4(0.0 , 0.0, 0.0, 1.0);
	}
	else
	{
		FragColor = color;
	}
}