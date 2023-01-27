Shader "Custom/Paint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PaintColor ("Paint Color", Color) = (0,0,0,1)
        _LineSize ("Line Size", Range(0.001,0.05)) = 0.001
        _NoiseSize ("Noise Size", Range(0.0,0.05)) = 0.0
        _NoiseScale ("Noise Scale", Range(1.,50.)) = 1.
        _NoiseColorSize ("Noise Color Size", Range(0.0,0.3)) = 0.0
        _Cutoff  ("Cutoff", Float) = 0.8

        [HideInInspector]
		_StartPos ("Start POS", VECTOR) = (0,0,0,0)
        [HideInInspector]
		_EndPos ("End POS", VECTOR) = (0,0,0,0)
        [HideInInspector]
        _IsDrawing ("IsDrawing", int) = 0
        [HideInInspector]
        _Clear ("Clear", int) = 0
    }
    SubShader
    {
        Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define Rot(a) float2x2(cos(a),-sin(a),sin(a),cos(a))
            #define antialiasing(n) n/min(_ScreenParams.y,_ScreenParams.x)
            #define S(d,b) smoothstep(antialiasing(1.0),b,d)

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _StartPos;
            float4 _EndPos;
            float4 _PaintColor;
            float _LineSize;
            float _NoiseSize;
            float _NoiseScale;
            float _NoiseColorSize;
            float _Cutoff;

            int _IsDrawing;
            int _Clear;

            float sdSegment( float2 p, float2 a, float2 b )
            {
                float2 pa = p-a, ba = b-a;
                float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
                return length( pa - ba*h );
            }

            float2 hash( float2 p )
            {
                p = float2( dot(p,float2(127.1,311.7)),
                         dot(p,float2(269.5,183.3)) );
                return -1.0 + 2.0*frac(sin(p)*43758.5453123);
            }

            float noise2d( float2 p )
            {
                const float K1 = 0.366025404; // (sqrt(3)-1)/2;
                const float K2 = 0.211324865; // (3-sqrt(3))/6;
    
                float2 i = floor( p + (p.x+p.y)*K1 );
    
                float2 a = p - i + (i.x+i.y)*K2;
                float2 o = (a.x>a.y) ? float2(1.0,0.0) : float2(0.0,1.0);
                float2 b = a - o + K2;
                float2 c = a - 1.0 + 2.0*K2;
    
                float3 h = max( 0.5-float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
    
                float3 n = h*h*h*h*float3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
    
                return dot( n, float3(70.0,70.0,70.0) );
            }

            float B(float2 p, float2 s){
                return max(abs(p.x)-s.x,abs(p.y)-s.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 resolution = _ScreenParams;
                float2 p = i.uv;
                float2 prevP = p;
                
                fixed4 col = tex2D(_MainTex, p);

                if(_IsDrawing == 1){
                    p+=noise2d(p*_NoiseScale)*_NoiseSize;
                    float d = sdSegment(p,_StartPos.xy,_EndPos.xy)-_LineSize;
                    _PaintColor+=noise2d(p*_NoiseScale)*_NoiseColorSize;

                    col = lerp(col,_PaintColor,S(d,0.0));
                }

                if(_Clear == 1) {
                    col = fixed4(1,1,1,0);
                }
                clip(col.a - _Cutoff);
                return col;
            }
            ENDCG
        }
    }
}
