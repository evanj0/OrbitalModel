#version 410

in vec4 color;
in vec2 textureCoordinates;

out vec4 fragColor;

void main() {
    fragColor = vec4(color.r, color.g, color.b, color.a);
}