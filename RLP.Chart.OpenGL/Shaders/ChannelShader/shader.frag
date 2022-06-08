#version 330
in vec3 colordiff;
uniform vec3 basecolor;
out vec4 outputColor;


void main()
{
    vec3 final= basecolor+colordiff;
    outputColor = vec4(final,1.0);
}