#version 450 core

in vec3 Uvs;

out vec4 Result;

uniform samplerCube skybox;

void main() {
	Result = texture(skybox, Uvs);	
}