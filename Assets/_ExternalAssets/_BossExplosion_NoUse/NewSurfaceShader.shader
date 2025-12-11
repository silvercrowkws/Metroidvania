Shader "Custom/ChromaKey"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        
        // 제거할 크로마키 색상 (R:0, G:1, B:0)
        _ChromaKeyColor ("Chroma Key Color", Color) = (0, 1, 0, 1) 
        
        // 색상 허용 범위. 1.0까지 확장하여 테두리 픽셀 제거를 용이하게 함.
        _Tolerance ("Tolerance", Range(0.001, 1.0)) = 0.5 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // 투명도 설정을 위한 블렌딩 (Blend) 활성화
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _ChromaKeyColor;
            float _Tolerance; // 인스펙터에서 설정한 허용 범위
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 텍스처에서 픽셀 색상 가져오기
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 2. 현재 픽셀 색상과 제거하려는 크로마키 색상 간의 거리(차이) 계산
                // float3(col.rgb)는 현재 픽셀 색상, _ChromaKeyColor.rgb는 순수한 초록색(0, 1, 0)
                float keyDistance = distance(col.rgb, _ChromaKeyColor.rgb);

                // 3. 클리핑 로직 적용
                // 만약 색상 거리가 허용 범위(_Tolerance)보다 작다면 (즉, 크로마키 색상과 유사하다면),
                // (keyDistance - _Tolerance)는 0보다 작은 음수가 됩니다.
                // clip() 함수는 인수가 0보다 작거나 같으면 해당 픽셀을 렌더링하지 않고 버립니다 (discard).
                clip(keyDistance - _Tolerance);
                
                // 4. 최종 색상 반환 (이미 clip()으로 투명도가 처리되었으므로 알파 조작 불필요)
                return col;
            }
            ENDCG
        }
    }
}