Shader "Custom/UIBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 20)) = 2.0
        
        // Required for UI Canvas
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma shader_feature_local _ _USEUIALPHACLIP_ON

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Box blur with texture coordinates offset
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;
                
                fixed4 col = fixed4(0,0,0,0);
                
                // Sample 9 points in a grid around the pixel
                col += tex2D(_MainTex, i.texcoord + float2(-1.0, -1.0) * texelSize);
                col += tex2D(_MainTex, i.texcoord + float2(0.0, -1.0) * texelSize);
                col += tex2D(_MainTex, i.texcoord + float2(1.0, -1.0) * texelSize);
                
                col += tex2D(_MainTex, i.texcoord + float2(-1.0, 0.0) * texelSize);
                col += tex2D(_MainTex, i.texcoord + float2(0.0, 0.0) * texelSize);
                col += tex2D(_MainTex, i.texcoord + float2(1.0, 0.0) * texelSize);
                
                col += tex2D(_MainTex, i.texcoord + float2(-1.0, 1.0) * texelSize);
                col += tex2D(_MainTex, i.texcoord + float2(0.0, 1.0) * texelSize);
                col += tex2D(_MainTex, i.texcoord + float2(1.0, 1.0) * texelSize);
                
                col /= 9.0;
                col *= i.color;
                col += _TextureSampleAdd;

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
        ENDCG
        }
    }
}
