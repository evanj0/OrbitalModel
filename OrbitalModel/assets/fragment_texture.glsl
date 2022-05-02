#version 410

in vec4 color;
in vec2 textureCoordinates;

uniform sampler2D texture;

out vec4 fragColor;

void main() {
    fragColor = texture2D(texture, textureCoordinates);
}