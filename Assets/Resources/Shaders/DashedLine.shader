Shader "Custom/DashedLine"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DashSize ("Dash Size", Float) = 0.5
        _GapSize ("Gap Size", Float) = 0.5
        _Tiling ("Tiling", Float) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            fixed4 _Color;
            float _DashSize;
            float _GapSize;
            float _Tiling;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Tiling;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float pattern = frac(i.uv.x / (_DashSize + _GapSize));
                if (pattern > (_DashSize / (_DashSize + _GapSize)))
                    discard;
                
                return _Color;
            }
            ENDCG
        }
    }
}