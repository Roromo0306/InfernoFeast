Shader "Custom/FadeOccluderStandard"
{
    Properties
    {
        _BaseMap ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _EmissionMap ("Emission", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)

        // Fade parameters
        _FadeColor ("Fade Color", Color) = (1,1,1,0.35)
        _Radius ("Radius (meters)", Float) = 0.5
        _Thickness ("Thickness (m)", Float) = 0.6
        _Softness ("Softness", Float) = 0.2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        CGPROGRAM
        // Cambiado: usar "alpha" válido para surface shader
        #pragma surface surf Standard fullforwardshadows alpha
        #pragma target 3.0

        sampler2D _BaseMap;
        float4 _Color;
        sampler2D _BumpMap;
        float _Metallic;
        float _Glossiness;
        sampler2D _EmissionMap;
        float4 _EmissionColor;

        float4 _FadeColor;
        float _Radius;
        float _Thickness;
        float _Softness;

        float3 _CharacterPos; // set from script

        struct Input
        {
            float2 uv_BaseMap;
            float2 uv_BumpMap;
            float2 uv_EmissionMap;
            float3 worldPos;
        };

        static float smoothstepf(float edge0, float edge1, float x)
        {
            float t = saturate((x - edge0) / (edge1 - edge0));
            return t * t * (3.0 - 2.0 * t);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap) * _Color;
            o.Albedo = c.rgb;

            // Normal map (si la textura es blanco, UnpackNormal devolverá vector neutro)
            fixed4 n = tex2D(_BumpMap, IN.uv_BumpMap);
            o.Normal = UnpackNormal(n);

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // Emission
            fixed4 em = tex2D(_EmissionMap, IN.uv_EmissionMap) * _EmissionColor;
            o.Emission = em.rgb;

            // compute fade per worldPos (misma lógica que antes)
            float3 camPos = _WorldSpaceCameraPos;
            float3 charPos = _CharacterPos;
            float3 camToChar = charPos - camPos;
            float camToCharLen = length(camToChar);
            float fade = 0.0;
            if (camToCharLen > 0.0001)
            {
                float3 dir = camToChar / camToCharLen;
                float3 fragVec = IN.worldPos - camPos;
                float tFrag = dot(fragVec, dir);
                float tChar = dot(charPos - camPos, dir);
                if (tFrag > 0.0 && tFrag < tChar)
                {
                    float3 alongRay = dir * tFrag;
                    float3 perp = fragVec - alongRay;
                    float perpDist = length(perp);
                    float alongDistToChar = tChar - tFrag;
                    float longFactor = saturate((_Radius - alongDistToChar) / _Radius);
                    float perpFactor = 1.0 - smoothstepf(_Thickness - _Softness, _Thickness + _Softness, perpDist);
                    fade = longFactor * perpFactor;
                }
            }

            // set output alpha usando fade
            float baseAlpha = c.a;
            float targetAlpha = lerp(baseAlpha, _FadeColor.a, fade);
            o.Alpha = targetAlpha;

            // tint RGB ligeramente hacia el fade color
            o.Albedo = lerp(o.Albedo, o.Albedo * _FadeColor.rgb, fade);
        }
        ENDCG
    }

    FallBack "Standard"
}

