﻿#version 450 core

in vec3 Normal;
in vec3 FragPos;
in vec2 Uvs;

out vec4 Result;

uniform vec3 lightPos = vec3(-2, 1, 2);
uniform vec3 viewPos = vec3(0, 0, 0);
uniform vec3 lightColor = vec3(1, 1, 1);
uniform vec3 diffuse = vec3(1, 1, 1);

uniform float hasTexture = 0.0;

uniform float specularStrength = 0.5;

uniform sampler2D sampler1;

void main() {
	float ambientStrength = 0.1;
	vec3 ambient = ambientStrength * diffuse;
	
	// diffuse
	vec3 norm = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);
	float diff = max(dot(norm, lightDir), 0.0);
	vec3 rdiffuse = diff * lightColor;

	// specular
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
	vec3 specular = specularStrength * spec * lightColor;

	vec3 result = (ambient + rdiffuse + specular) * diffuse;

	vec4 textureColor = texture(sampler1, Uvs);

	Result = vec4(result, 1.0f);

	if (hasTexture > 0.0) Result *= textureColor;
} 
