#version 330 core

layout(location = 0) in vec3 aPosition;
uniform mat4 transform;
uniform vec3 colorgradual;
uniform float highest;
out vec3 colordiff;

void main(void)
{
    float ratio= aPosition.z / highest;
    colordiff= colorgradual * ratio;
    gl_Position = vec4(aPosition, 1.0) * transform;
}
