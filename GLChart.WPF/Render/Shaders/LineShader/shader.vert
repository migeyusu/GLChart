#version 430 core

 layout (std430, binding = 0) buffer YAxisScale {
    int Array[];
};

layout (std430, binding = 1) buffer TVertex
{
    vec2 vertex[];
};

layout (std430, binding = 2) buffer TestBuffer
{
    float test[];
};

uniform mat4 transform;
uniform vec2 u_resolution;
uniform float u_thickness;
uniform int startIndex;
uniform int maxLineIndex;

void main(void)
{
    int line_i = gl_VertexID / 6;
    line_i += startIndex;
    int tri_i = gl_VertexID % 6;
    vec4 va[4];
    for (int i = 0; i < 4; ++i)
    {
        int k = line_i + i;
        if (k == -1)
        {
            va[i] = vec4(vertex[0], 1.0, 1.0) * transform;
        }
        else if (k > maxLineIndex)
        {
            va[i] = vec4(vertex[maxLineIndex], 1.0, 1.0) * transform;
        }
        else
        {
            va[i] = vec4(vertex[k], 1.0, 1.0) * transform;
        }
        va[i].xy = (va[i].xy + 1.0) * 0.5 * u_resolution;
    }

    vec2 v_line = normalize(va[2].xy - va[1].xy);
    vec2 nv_line = vec2(-v_line.y, v_line.x);

    vec4 pos;
    if (tri_i == 0 || tri_i == 1 || tri_i == 3)
    {
        vec2 v_pred = normalize(va[1].xy - va[0].xy);
        vec2 v_miter = normalize(nv_line + vec2(-v_pred.y, v_pred.x));

        pos = va[1];
        pos.xy += v_miter * u_thickness * (tri_i == 1 ? -0.5 : 0.5) / dot(v_miter, nv_line);
    }
    else
    {
        vec2 v_succ = normalize(va[3].xy - va[2].xy);
        vec2 v_miter = normalize(nv_line + vec2(-v_succ.y, v_succ.x));

        pos = va[2];
        pos.xy += v_miter * u_thickness * (tri_i == 5 ? 0.5 : -0.5) / dot(v_miter, nv_line);
    }
    pos.xy = pos.xy / u_resolution * 2.0 - 1.0;
    pos.xyz *= pos.w;
    test[gl_VertexID] = pos.x;//int(vertex[gl_VertexID].x);
    gl_Position = pos;

    if ((gl_Position.x > -1) && (gl_Position.x < 1))
    {
        int index = int((gl_Position.y + 3) * 100);
        if (index > 599) {
            index = 599;
        }
        if (index < 0) {
            index = 0;
        }
        Array[index] = 1;
    }

}