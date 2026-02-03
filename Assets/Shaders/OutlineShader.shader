/*
 * 文件名：OutlineShader.shader
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：轮廓Shader，用于显示物体的高亮轮廓。
 * 通过顶点沿法线外扩和纯色渲染实现轮廓效果。
 */

Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 0.3, 0.3, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        
        // 轮廓Pass - 只渲染背面，放大一点
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite On
            ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            float4 _OutlineColor;
            float _OutlineWidth;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                // 沿法线方向扩展顶点
                float3 norm = normalize(v.normal);
                v.vertex.xyz += norm * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
    FallBack Off
}
