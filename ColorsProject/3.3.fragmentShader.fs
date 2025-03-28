#version 330 core
struct Material
{
    sampler2D diffuse; // this replaces: vec3 diffuse;
    sampler2D specular;// this replaces: vec3 specular;
    sampler2D emission;
    float shininess;
};

struct DirLight
{
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight
{
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

struct SpotLight
{
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Material material;
uniform DirLight dirLight;
uniform SpotLight spotLight;
#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[NR_POINT_LIGHTS];

uniform float time;

out vec4 FragColor;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

uniform vec3 viewPos;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir);

void main()
{
    vec3 result;

    // properties
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    // phase 1: Directional lighting
    result += CalcDirLight(dirLight, norm, viewDir);
    // phase 2: Point lights
    for(int i = 0; i < NR_POINT_LIGHTS; i++)
        result += CalcPointLight(pointLights[i], norm, viewDir);
    // phase 3: Spot light
    result += CalcSpotLight(spotLight, norm, viewDir);
    
    FragColor = vec4(result, 1.0);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0f);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0f), material.shininess);
    // ambient shading and then combine results
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoord));
    vec3 diffuse = light.diffuse * vec3(texture(material.diffuse, TexCoord) * diff);
    vec3 specular = light.specular * vec3(texture(material.specular, TexCoord) * spec);

    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - FragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    // attenuation
    float distance = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    // combine results
    vec3 ambient = light.ambient  * vec3(texture(material.diffuse, TexCoord));
    vec3 diffuse = light.diffuse  * diff * vec3(texture(material.diffuse, TexCoord));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoord));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - FragPos);

    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0f);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0f), material.shininess);
    // ambient shading and then combine results
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoord));
    vec3 diffuse = light.diffuse * vec3(texture(material.diffuse, TexCoord) * diff);
    vec3 specular = light.specular * vec3(texture(material.specular, TexCoord) * spec);

    ambient  *= intensity;
    diffuse  *= intensity;
    specular *= intensity;

    return (ambient + diffuse + specular);
}