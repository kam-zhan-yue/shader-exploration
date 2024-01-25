Shader "Graph/PointSurfaceGPU"
{
	Properties {
		_Smoothness ("Smoothness", Range(0,1)) = 0.5
	}
    SubShader
    {
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        // For procedural rendering on the GPU
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        // Level 4.5 indicates that we need at least the capabilities of OpenGL ES 3.1
        #pragma target 4.5
        #include "PointGPU.hlsl"
        struct Input
        {
            float3  worldPos;
        };
        
        float _Smoothness;
        
        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
        {
            surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
            surface.Smoothness = _Smoothness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}