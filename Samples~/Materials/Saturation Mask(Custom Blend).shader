// example of custom blending shader
Shader "Hidden/LayerFx/Saturation Mask"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        ZClip false
            
        Pass
        {
            Name "Saturation Mask"
            HLSLPROGRAM
                        
            #pragma vertex vert
            #pragma fragment frag
            
            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _BlendTex;
            float _Weight;

            frag_in vert(const vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
            
            half4 frag(const frag_in i) : SV_Target
            {
                half4 main  = tex2D(_MainTex, i.uv);
                half4 blend = tex2D(_BlendTex, i.uv);
                
                half3 grayscale = dot(main.rgb, half3(0.299, 0.587, 0.114));    // rec601 https://en.wikipedia.org/wiki/Grayscale
                half3 result = lerp(main.rgb, grayscale, blend.a);
                return half4(lerp(main.rgb, result, _Weight), main.a);
            }
            ENDHLSL
        }
    }
}
