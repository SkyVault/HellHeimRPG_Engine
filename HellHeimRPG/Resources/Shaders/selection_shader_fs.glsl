#version 450 core

out vec4 Result;

uniform vec3 Key;

void main() {
    Result = vec4(Key, 1.0);
} 
