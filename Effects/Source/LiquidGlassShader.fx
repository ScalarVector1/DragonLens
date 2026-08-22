float4 u_color;
float3 u_fade;
float2 u_resolution;
float u_time;
float u_power;
float2 u_boxsize;
float2 u_stretch;
float2 u_mouse;
float4x4 u_scale;

texture normal_t;
sampler2D normal = sampler_state { texture = <normal_t>; AddressU = clamp; AddressV = clamp; };

texture back_t;
sampler2D background = sampler_state { texture = <back_t>; AddressU = clamp; AddressV = clamp; };

float4 PixelShaderFunction(float2 uv : TEXCOORD0, float4 pos : VPOS) : COLOR0
{
    // Proper background sampling
    float2 screenUV = mul(float4(pos.xy / u_resolution, 1.0, 1.0), u_scale).xy;

    float3 norm = tex2D(normal, uv).xyz * 2.0 - 1.0;
    norm.g = -norm.g;
    float alpha = tex2D(normal, uv).a;

    float2 distort = screenUV - (norm.rg) * u_power;

    // light effect
    float3 lightPos = float3(u_mouse, 80.0); // Z controls light height
    float3 fragPos = float3(pos.xy, 0.0);

    float3 L = normalize(lightPos - fragPos);
    
    float diffuse = dot(norm, L) * 0.2;
    
    float3 R = reflect(-L, norm);
    float3 V = float3(0, 0, 1);
    float spec = pow(saturate(dot(R, V)), 10.0);
    float fresnel = pow(1.0 - saturate(dot(norm, V)), 3.0);
    
    float3 R2 = reflect(L, -norm);
    float specBack = pow(saturate(dot(R2, V)), 20.0);
    
    // final
    float4 bg = tex2D(background, distort);

    float4 light = float4(0.9, 1.0, 1.0, 1.0) * diffuse * 0.5;
    light += float4(1.0, 1.0, 1.0, 1.0) * spec * 0.2;
    light += float4(0.5, 0.8, 1.0, 1.0) * specBack * 0.5;
    light += float4(0.8, 1.0, 1.0, 1.0) * fresnel;
    
    return (u_color * (0.5) + bg * 0.5 + light) * alpha;
}

technique Technique1
{
    pass VitricReplicaItemPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}