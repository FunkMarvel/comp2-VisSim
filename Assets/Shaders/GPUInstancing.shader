Shader "GPU/PointCloudShader"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            uniform float4x4 _ObjectToWorld;
            uniform StructuredBuffer<float3> _positions;

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint cmdID = GetCommandID(0);
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                float4 wpos = mul(_ObjectToWorld, v.vertex + float4(_positions[instanceID][0], _positions[instanceID][1], _positions[instanceID][2], 0));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = float4(0.f, instanceID / float(GetIndirectInstanceCount()), cmdID & 1 ? 1.0f : 0.0f, 0.3f);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}