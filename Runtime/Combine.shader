Shader "Hidden/LayerFx/Combine"
{
    CGINCLUDE
    
    #include "UnityCG.cginc"
    
    struct vert_in
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct frag_in
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };
    
    frag_in vert(vert_in v)
    {
        frag_in o;
        o.vertex = v.vertex;
        o.uv = v.uv;
        return o;
    }

    fixed lum(fixed3 rgb)
    {
        return dot(rgb, fixed3(0.299, 0.587, 0.114));
    }

    fixed3 ColorToLuma(fixed3 rgb, fixed luma)
    {
        fixed lumSrc = lum(rgb) - 0.001;
        fixed scaleDesire = luma / lumSrc;
        
        fixed peak = max(max(max(rgb.r, rgb.g), rgb.b), 0.001);
        fixed scaleLimit = 1 / peak;

        fixed3 res = rgb * min(scaleDesire, scaleLimit);
        fixed lumRes = lum(res);
        if (lumRes < luma)
        {
            fixed3 rem = (fixed3(1, 1, 1) - rgb);
            res += ((luma - lumRes) / lum(rem)) * rem;
        }

        return res;
    }
    
    fixed3 rgb2hsv(fixed3 rgb)
    {
        fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
        fixed4 p = lerp(fixed4(rgb.bg, K.wz), fixed4(rgb.gb, K.xy), step(rgb.b, rgb.g));
        fixed4 q = lerp(fixed4(p.xyw, rgb.r), fixed4(rgb.r, p.yzx), step(p.x, rgb.r));
        
        float d = q.x - min(q.w, q.y);
        float e = 1.0e-10;
        return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
    }
 
    fixed3 hsv2rgb(fixed3 hsv)
    {
        fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
        fixed3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
        return hsv.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), hsv.y);
    }
    
    ENDCG

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        ZClip false
            
        Pass    // 0
        {
            name "Blit with Alpha"
            
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            uniform sampler2D _MainTex;
            uniform float _Weight;
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a *= _Weight;
                
                return col;
            }
            ENDCG
        }
        
        Pass    // 1
        {
            Name "Normal"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                return lerp(main, blend, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 2
        {
            Name "Multiply"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                return lerp(main, main * blend, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 3
        {
            Name "Screen"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 screen  = fixed4(main.rgb + blend.rgb - main.rgb * blend.rgb, main.a);
                
                return lerp(main, screen, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 4
        {
            Name "Overlay"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;

            
            fixed over(fixed src, fixed dst)
            {
                return (dst < 0.5) ? 2.0 * src * dst : 1.0 - 2.0 * (1.0 - src) * (1.0 - dst);
            }
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 overlay  = fixed4(over(main.r, blend.r), over(main.g, blend.g), over(main.b, blend.b), main.a);
                
                return lerp(main, overlay, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 5
        {
            Name "SoftLight"
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
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
                
                return half4(lerp(main.rgb, SoftLight(main.rgb, blend.rgb), _Weight), main.a);
            }
            ENDHLSL
        }
        
        Pass    // 6
        {
            Name "Difference"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 dif  = fixed4(abs(blend.rgb - main.rgb), main.a);
                return lerp(main, dif, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 7
        {
            Name "Add"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 add  = fixed4(blend.rgb + main.rgb, main.a);
                return lerp(main, add, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 8
        {
            Name "Substract"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 sub  = fixed4(main.rgb - blend.rgb, main.a);
                return lerp(main, sub, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 9
        {
            Name "Hue"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
            
            fixed3 hue(fixed3 src, fixed3 dst)
            {
                dst = rgb2hsv(dst);
                dst.x = rgb2hsv(src).x;
                return hsv2rgb(dst);
            }
                        
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 dest  = fixed4(hue(main.rgb, blend.rgb), main.a);
                return lerp(main, dest, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 10
        {
            Name "Saturation"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;

            fixed3 sat(fixed3 src, fixed3 dst)
            {
                fixed3 res = rgb2hsv(src);
                res.y = rgb2hsv(dst).y;
                return hsv2rgb(res);
            }
                        
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 dest  = fixed4(sat(main.rgb, blend.rgb), main.a);
                return lerp(main, dest, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 11
        {
            Name "Color"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
                                    
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 dest  = fixed4(ColorToLuma(blend.rgb, lum(main.rgb)), main.a);
                return lerp(main, dest, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 12
        {
            Name "Luminosity"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
                                    
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                fixed4 dest  = fixed4(ColorToLuma(main.rgb, lum(blend.rgb)), main.a);
                return lerp(main, dest, blend.a * _Weight);
            }
            ENDCG
        }
        
        Pass    // 13
        {
            Name "Alpha"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BlendTex;
            uniform float _Weight;
                                    
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 main  = tex2D(_MainTex, i.uv);
                fixed4 blend = tex2D(_BlendTex, i.uv);
                return fixed4(main.rgb, lerp(main.a, lum(blend.rgb),  blend.a * _Weight));
            }
            ENDCG
        }
    }
}