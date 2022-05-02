#version 410

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;
layout(location = 2) in vec2 inTextureCoordinates;

uniform mat4 camera;

out vec4 color;
out vec2 textureCoordinates;

void main() {
    gl_Position = camera * vec4(inPosition.x, inPosition.y, inPosition.z, 1.0);
    color = inColor;
    textureCoordinates = inTextureCoordinates;
}