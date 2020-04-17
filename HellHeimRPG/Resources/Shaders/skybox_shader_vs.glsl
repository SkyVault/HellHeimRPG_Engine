#version 450 core

layout (location = 0) in vec3 iVertex;

out vec3 Uvs;

uniform mat4 projection;
uniform mat4 view;

void main() {
	Uvs = iVertex;
	gl_Position = projection * view * vec4(iVertex, 1.0);
}