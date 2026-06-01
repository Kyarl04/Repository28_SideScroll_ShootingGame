// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FX_Dissolve_FadeImage"
{
	Properties
	{
		_Tex("Tex", 2D) = "white" {}
		_UV_XY("UV_XY", Vector) = (0,0,0,0)
		_UV_XY1("UV_XY", Vector) = (0,0,0,0)
		_Dissolve_Tex("Dissolve_Tex", 2D) = "white" {}
		_DIssolveIntensity("Intensity", Float) = 0
		_DissolveValue("Value", Float) = 0

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _Tex;
			uniform float4 _Tex_ST;
			uniform float2 _UV_XY;
			uniform sampler2D _Dissolve_Tex;
			uniform float4 _Dissolve_Tex_ST;
			uniform float2 _UV_XY1;
			uniform float _DissolveValue;
			uniform float _DIssolveIntensity;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_Tex = i.ase_texcoord1.xy * _Tex_ST.xy + _Tex_ST.zw;
				float2 break7_g1 = _UV_XY;
				float2 appendResult8_g1 = (float2(( break7_g1.x * _Time.y ) , ( break7_g1.y * _Time.y )));
				float2 temp_output_10_0_g1 = ( uv_Tex + appendResult8_g1 );
				float2 uv_Dissolve_Tex = i.ase_texcoord1.xy * _Dissolve_Tex_ST.xy + _Dissolve_Tex_ST.zw;
				float2 break7_g3 = _UV_XY1;
				float2 appendResult8_g3 = (float2(( break7_g3.x * _Time.y ) , ( break7_g3.y * _Time.y )));
				float2 temp_output_10_0_g3 = ( uv_Dissolve_Tex + appendResult8_g3 );
				float4 temp_cast_0 = (tex2D( _Dissolve_Tex, temp_output_10_0_g3 ).r).xxxx;
				float temp_output_4_0_g2 = _DissolveValue;
				float lerpResult7_g2 = lerp( -1.5 , temp_output_4_0_g2 , _DIssolveIntensity);
				float4 temp_cast_1 = (lerpResult7_g2).xxxx;
				
				
				finalColor = ( tex2D( _Tex, temp_output_10_0_g1 ) * saturate( ( ( temp_cast_0 * temp_output_4_0_g2 ) - temp_cast_1 ) ) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;192.4001,-6.239984;Float;False;True;-1;2;ASEMaterialInspector;100;5;FX_Dissolve_FadeImage;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;;10;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.FunctionNode;5;-700.4174,191.2496;Inherit;False;FX_Dissolve;-1;;2;4c078433b3f10b048ae28959655ac144;0;3;1;FLOAT4;0,0,0,0;False;4;FLOAT;0;False;10;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;1;-636.9466,-52.328;Inherit;True;Property;_Tex;Tex;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-1034.257,193.3295;Inherit;True;Property;_Dissolve_Tex;Dissolve_Tex;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;4;-1284.53,9.597626;Inherit;False;Property;_UV_XY;UV_XY;1;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-1312.532,-112.0024;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;2;-1034.132,-52.00255;Inherit;False;FX_UV_Scroll;-1;;1;12f51f5ad047e134a994f5b97784cdf2;1,17,0;5;9;FLOAT2;0,0;False;1;FLOAT2;0,0;False;18;FLOAT2;0,0;False;12;FLOAT2;0,0;False;13;FLOAT2;0,0;False;2;FLOAT2;0;FLOAT2;16
Node;AmplifyShaderEditor.FunctionNode;10;-1394.633,207.356;Inherit;False;FX_UV_Scroll;-1;;3;12f51f5ad047e134a994f5b97784cdf2;1,17,0;5;9;FLOAT2;0,0;False;1;FLOAT2;0,0;False;18;FLOAT2;0,0;False;12;FLOAT2;0,0;False;13;FLOAT2;0,0;False;2;FLOAT2;0;FLOAT2;16
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1674.073,147.3561;Inherit;False;0;7;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;8;-1645.031,268.9561;Inherit;False;Property;_UV_XY1;UV_XY;2;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;12;-927.1366,491.8092;Inherit;False;Property;_DIssolveIntensity;Intensity;4;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-927.1366,414.8493;Inherit;False;Property;_DissolveValue;Value;5;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-283.3772,61.24957;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
WireConnection;0;0;6;0
WireConnection;5;1;7;1
WireConnection;5;4;11;0
WireConnection;5;10;12;0
WireConnection;1;1;2;0
WireConnection;7;1;10;0
WireConnection;2;9;3;0
WireConnection;2;1;4;0
WireConnection;10;9;9;0
WireConnection;10;1;8;0
WireConnection;6;0;1;0
WireConnection;6;1;5;0
ASEEND*/
//CHKSM=0741D261878DCA0EABA9A111E3CE511CC12EBAD4