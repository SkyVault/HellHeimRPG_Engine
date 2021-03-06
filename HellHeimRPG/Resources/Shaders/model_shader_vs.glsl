﻿ #version 450 core

layout (location = 0) in vec3 iVertex;
layout (location = 1) in vec3 iNormal;
layout (location = 2) in vec2 iUvs;

out vec3 FragPos;
out vec3 Normal;
out vec2 Uvs;

uniform mat4 projection;
uniform mat4 model;
uniform mat4 view;

void main() {
	FragPos = vec3(model * vec4(iVertex, 1.0));
	Normal = mat3(transpose(inverse(model))) * iNormal;
	Uvs = iUvs;

	gl_Position = projection * view * vec4(FragPos, 1); 
}
