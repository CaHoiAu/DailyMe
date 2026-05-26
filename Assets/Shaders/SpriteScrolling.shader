Shader "Sprites/Scrolling"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        BlendOp Add

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnityCG.cginc"

            #ifdef UNITY_INSTANCING_ENABLED
                UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
                    // sampler2D instances are not being instanced, but they are coming from a different namespace
                    // (the same namespace in which the properties are) So we are handle them in a similar way.
                    UNITY_DEFINE_INSTANCED_PROP(float4, _RendererColor) // Use this to get the instanced property
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)
                #define _RendererColorValue UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, _RendererColor)
            #else
                CBUFFER_START(UnityPerDrawSprite)
                    fixed4 _RendererColor;
                CBUFFER_END
                #define _RendererColorValue _RendererColor
            #endif

            cbuffer UnityPerMaterial
            {
                float4 _MainTex_ST;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _Flip;
            sampler2D _AlphaTex;
            float _EnableExternalAlpha;

            // vertex shader
            void SpriteVert(inout appdata_full v, out half fogCoord : TEXCOORD5)
            {
            #ifdef PIXELSNAP_ON
                v.vertex = UnityPixelSnap(v.vertex);
            #endif

                v.vertex = UnityObjectToClipPos(v.vertex);
                v.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                fogCoord = 0;
            }

            // pixel shader
            fixed4 SpriteFrag(half2 texCoord : TEXCOORD0, fixed4 color : COLOR) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, texCoord) * color;

            #ifdef ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D(_AlphaTex, texCoord);
                c.a = lerp(c.a, alpha.r, _EnableExternalAlpha);
            #endif

                c.rgb *= c.a;
                c *= _Color;
                c *= _RendererColorValue;
                return c;
            }
        ENDCG
        }
    }
}
