#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

float near = 0.5; 
float far  = 100.0; 

//float LinearizeDepth(float depth)
//{
//    float ndc = depth * 2 - 1; // back to NDC
//    return (2 * near * far) / (far + near - ndc * (far - near));
//}

void main()
{    
    //float depth = LinearizeDepth(gl_FragCoord.z) / far; // divide by far for to linearly transform to range of [0, 1]
    //FragColor = vec4(vec3(depth), 1.0);

    FragColor = vec4(vec3(texture(texture1, TexCoords)), 1.0f);
}