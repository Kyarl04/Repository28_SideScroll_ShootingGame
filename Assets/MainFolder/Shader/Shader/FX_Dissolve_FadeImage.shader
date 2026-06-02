// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FX_Dissolve_FadeImage"
{
	Properties
	{
		_Tex("Tex", 2D) = "white" {}
		_UV_XY("UV_XY", Vector) = (0,0,0,0)
		[Header(__________________________________________________________________)][Header(Dissolve Texture)]_DissolveTex("DissolveTex", 2D) = "white" {}
		DUV_Scroll2("D.UV_Scroll", Vector) = (0,0,0,0)
		[KeywordEnum(Off,Smoothness,Step,DirectionDissolve)] _Dissolve("Dissolve", Float) = 0
		D_RotateValue("D_RotateValue", Range( 0 , 1)) = 0
		[KeywordEnum(Custom2_Off,Custom2_X_ON)] _DIntensity_Custom2_X("D.Intensity_Custom2_X", Float) = 0
		_DIntensity("D.Intensity", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.3
		[Header(Dir.Dissolve)][KeywordEnum(U_Pos,U_Neg,V_Pos,V_Neg)] _UVMaskDirection("UV Mask Direction", Float) = 0
		[KeywordEnum(Custom2_Off,Custom2_X_On)] _Dissolve_Custom2_X("Dissolve_Custom2_X", Float) = 0
		_DissolveFloat("Dissolve_Intensity", Range( 0 , 1)) = 0.4526118
		_DDirection_Width("D.Direction_Width", Range( 0 , 1)) = 1
		_DirectionSmoothness("Direction Smoothness", Range( 0 , 1)) = 0.3282696
		_Opacity("Opacity", Range( 0 , 2)) = 1

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
			#pragma shader_feature_local _DISSOLVE_OFF _DISSOLVE_SMOOTHNESS _DISSOLVE_STEP _DISSOLVE_DIRECTIONDISSOLVE
			#pragma shader_feature_local _DINTENSITY_CUSTOM2_X_CUSTOM2_OFF _DINTENSITY_CUSTOM2_X_CUSTOM2_X_ON
			#pragma shader_feature_local _DISSOLVE_CUSTOM2_X_CUSTOM2_OFF _DISSOLVE_CUSTOM2_X_CUSTOM2_X_ON
			#pragma shader_feature_local _UVMASKDIRECTION_U_POS _UVMASKDIRECTION_U_NEG _UVMASKDIRECTION_V_POS _UVMASKDIRECTION_V_NEG


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord3 : TEXCOORD3;
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
			uniform float _Smoothness;
			uniform float _DIntensity;
			uniform sampler2D _DissolveTex;
			uniform float4 _DissolveTex_ST;
			uniform float D_RotateValue;
			uniform float2 DUV_Scroll2;
			uniform float _DissolveFloat;
			uniform float _DirectionSmoothness;
			uniform float _DDirection_Width;
			uniform float _Opacity;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				o.ase_texcoord1.zw = v.ase_texcoord3.xy;
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
				float2 break7_g96 = _UV_XY;
				float2 appendResult8_g96 = (float2(( break7_g96.x * _Time.y ) , ( break7_g96.y * _Time.y )));
				float2 temp_output_10_0_g96 = ( uv_Tex + appendResult8_g96 );
				float2 texCoord42 = i.ase_texcoord1.zw * float2( 1,1 ) + float2( 0,0 );
				#if defined(_DINTENSITY_CUSTOM2_X_CUSTOM2_OFF)
				float staticSwitch39 = _DIntensity;
				#elif defined(_DINTENSITY_CUSTOM2_X_CUSTOM2_X_ON)
				float staticSwitch39 = texCoord42.x;
				#else
				float staticSwitch39 = _DIntensity;
				#endif
				float2 uv_DissolveTex = i.ase_texcoord1.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				float2 temp_cast_0 = (0.5).xx;
				float cos1_g95 = cos( ( D_RotateValue * 6.28318548202515 ) );
				float sin1_g95 = sin( ( D_RotateValue * 6.28318548202515 ) );
				float2 rotator1_g95 = mul( uv_DissolveTex - temp_cast_0 , float2x2( cos1_g95 , -sin1_g95 , sin1_g95 , cos1_g95 )) + temp_cast_0;
				float2 break7_g94 = DUV_Scroll2;
				float2 appendResult8_g94 = (float2(( break7_g94.x * _Time.y ) , ( break7_g94.y * _Time.y )));
				float2 temp_output_10_0_g94 = ( rotator1_g95 + appendResult8_g94 );
				float4 tex2DNode50 = tex2D( _DissolveTex, temp_output_10_0_g94 );
				float temp_output_41_0 = (( tex2DNode50 * tex2DNode50.a )).r;
				float smoothstepResult29 = smoothstep( 0.0 , _Smoothness , ( ( (0.0 + (staticSwitch39 - 0.0) * (2.0 - 0.0) / (1.0 - 0.0)) * -1.0 ) + temp_output_41_0 + 1.0 ));
				float temp_output_44_0 = ( staticSwitch39 + 0.01 );
				float temp_output_35_0 = step( temp_output_44_0 , temp_output_41_0 );
				float2 texCoord75 = i.ase_texcoord1.zw * float2( 1,1 ) + float2( 0,0 );
				#if defined(_DISSOLVE_CUSTOM2_X_CUSTOM2_OFF)
				float staticSwitch24 = _DissolveFloat;
				#elif defined(_DISSOLVE_CUSTOM2_X_CUSTOM2_X_ON)
				float staticSwitch24 = texCoord75.x;
				#else
				float staticSwitch24 = _DissolveFloat;
				#endif
				float temp_output_18_0 = ( 1.0 + _DirectionSmoothness );
				float temp_output_20_0 = ( staticSwitch24 * temp_output_18_0 );
				float2 texCoord25 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				#if defined(_UVMASKDIRECTION_U_POS)
				float staticSwitch74 = texCoord25.x;
				#elif defined(_UVMASKDIRECTION_U_NEG)
				float staticSwitch74 = ( 1.0 - texCoord25.x );
				#elif defined(_UVMASKDIRECTION_V_POS)
				float staticSwitch74 = texCoord25.y;
				#elif defined(_UVMASKDIRECTION_V_NEG)
				float staticSwitch74 = ( 1.0 - texCoord25.y );
				#else
				float staticSwitch74 = texCoord25.x;
				#endif
				float smoothstepResult23 = smoothstep( temp_output_20_0 , ( temp_output_20_0 - _DirectionSmoothness ) , staticSwitch74);
				float temp_output_22_0 = ( ( staticSwitch24 + _DDirection_Width ) * temp_output_18_0 );
				float smoothstepResult63 = smoothstep( temp_output_22_0 , ( temp_output_22_0 - _DirectionSmoothness ) , staticSwitch74);
				#if defined(_DISSOLVE_OFF)
				float staticSwitch67 = 1.0;
				#elif defined(_DISSOLVE_SMOOTHNESS)
				float staticSwitch67 = smoothstepResult29;
				#elif defined(_DISSOLVE_STEP)
				float staticSwitch67 = temp_output_35_0;
				#elif defined(_DISSOLVE_DIRECTIONDISSOLVE)
				float staticSwitch67 = ( step( smoothstepResult23 , temp_output_41_0 ) - step( smoothstepResult63 , temp_output_41_0 ) );
				#else
				float staticSwitch67 = 1.0;
				#endif
				
				
				finalColor = ( tex2D( _Tex, temp_output_10_0_g96 ) * staticSwitch67 * _Opacity );
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
Node;AmplifyShaderEditor.CommentaryNode;13;-3459.321,696.7247;Inherit;False;4356.794;1828.188;;18;69;68;67;64;55;54;53;52;51;50;49;42;41;39;38;16;15;14;Dissolve;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;14;-987.6421,1098.818;Inherit;False;1126.691;479.1692;;11;59;58;48;47;46;45;44;43;37;36;35;Step;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;15;-1026.833,724.1298;Inherit;False;1016.249;358.4513;;7;40;32;31;30;29;28;27;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;16;-2864.075,1647.184;Inherit;False;2175.255;676.1514;;21;75;74;73;72;71;70;66;65;63;62;61;60;25;24;23;22;21;20;19;18;17;Dissolve UV Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;17;-2153.778,1697.185;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-1775.181,1996.557;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;19;-2153.174,1796.556;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1664.681,1904.257;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;21;-1528.363,2013.577;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1666.057,2172.84;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;23;-1359.395,1893.305;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;24;-2521.054,1878.742;Inherit;False;Property;_Dissolve_Custom2_X;Dissolve_Custom2_X;15;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Custom2_Off;Custom2_X_On;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-743.6329,785.3428;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-503.7989,783.7748;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;29;-199.388,777.0387;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-894.8589,967.1827;Inherit;False;Constant;_Float1;Float 1;48;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-644.0499,959.3438;Inherit;False;Constant;_Float5;Float 5;48;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;32;-976.833,774.1298;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;35;-413.4158,1148.818;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;36;-240.8219,1191.874;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;37;-411.8149,1260.818;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;39;-1575.562,1291.916;Inherit;False;Property;_DIntensity_Custom2_X;D.Intensity_Custom2_X;8;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Custom2_Off;Custom2_X_ON;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-484.2237,961.6348;Inherit;False;Property;_Smoothness;Smoothness;10;0;Create;True;0;0;0;False;0;False;0.3;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;41;-1425.417,1098.634;Inherit;False;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;42;-1833.545,1363.536;Inherit;False;3;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;43;-260.7579,1514.712;Inherit;False;Property;_DEdge_Intensity;D.Edge_Intensity;12;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-764.36,1235.136;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-960.7139,1308.924;Inherit;False;Constant;_Float10;Float 10;50;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-589.5192,1329.978;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-952.623,1478.106;Inherit;False;Constant;_Float12;Float 12;52;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-717.3439,1420.586;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1629.025,1074.92;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;50;-1953.372,1026.113;Inherit;True;Property;_DissolveTex;DissolveTex;4;1;[Header];Create;True;2;__________________________________________________________________;Dissolve Texture;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;53;-2288.426,1024.504;Inherit;False;FX_UV_Scroll;-1;;94;12f51f5ad047e134a994f5b97784cdf2;1,17,0;5;9;FLOAT2;0,0;False;1;FLOAT2;0,0;False;18;FLOAT2;0,0;False;12;FLOAT2;0,0;False;13;FLOAT2;0,0;False;2;FLOAT2;0;FLOAT2;16
Node;AmplifyShaderEditor.ColorNode;58;-288.1048,1301.588;Inherit;False;Property;_DEdge_Color;D.Edge_Color;11;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,0.9094239,0.7830188,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-5.38098,1379.582;Inherit;False;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-2124.474,2054.729;Inherit;False;Property;_DirectionSmoothness;Direction Smoothness;18;0;Create;True;0;0;0;False;0;False;0.3282696;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-1987.987,1946.335;Inherit;False;Constant;_2;1;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;62;-1530.397,2103.905;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;63;-1363.157,2066.367;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-987.8569,1401.539;Inherit;False;Property;_DEdge_Width;D.Edge_Width;13;0;Create;True;0;0;0;False;0;False;0.01;-0.01;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-2256.841,2096.84;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-2579.676,2150.062;Inherit;False;Property;_DDirection_Width;D.Direction_Width;17;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;67;408.0982,1405.717;Inherit;False;Property;_Dissolve;Dissolve;6;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;4;Off;Smoothness;Step;DirectionDissolve;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;253.9881,1210.415;Inherit;False;Constant;_Float9;Float 9;50;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;69;435.9542,1576.21;Inherit;False;Property;_Keyword0;Keyword 0;8;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;-1;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;71;-1060.198,1875.903;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;72;-1070.726,1981.326;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;73;-863.3302,1919.596;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;74;-1989.771,1699.034;Inherit;False;Property;_UVMaskDirection;UV Mask Direction;14;0;Create;True;0;0;0;False;1;Header(Dir.Dissolve);False;0;0;0;True;;KeywordEnum;4;U_Pos;U_Neg;V_Pos;V_Neg;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;75;-2814.075,1976.554;Inherit;False;3;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;487.5625,369.034;Inherit;True;Property;_Tex;Tex;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;4;-160.0207,430.9597;Inherit;False;Property;_UV_XY;UV_XY;3;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-188.0227,309.3596;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;2;90.37747,369.3595;Inherit;False;FX_UV_Scroll;-1;;96;12f51f5ad047e134a994f5b97784cdf2;1,17,0;5;9;FLOAT2;0,0;False;1;FLOAT2;0,0;False;18;FLOAT2;0,0;False;12;FLOAT2;0,0;False;13;FLOAT2;0,0;False;2;FLOAT2;0;FLOAT2;16
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;1080.634,809.6207;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1400.487,833.8785;Float;False;True;-1;2;ASEMaterialInspector;100;5;FX_Dissolve_FadeImage;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;;10;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.Vector2Node;52;-2465.756,1174.019;Inherit;False;Property;DUV_Scroll2;D.UV_Scroll;5;0;Create;False;0;0;0;False;0;False;0,0;0.1,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;38;-1879.554,1275.916;Inherit;False;Property;_DIntensity;D.Intensity;9;0;Create;False;0;0;0;False;0;False;0;0.665;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-2404.044,1699.577;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;54;-2899.724,893.225;Inherit;False;Property;D_RotateValue;D_RotateValue;7;0;Create;False;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;55;-2613.51,990.378;Inherit;False;FX_Rotator;1;;95;5b5f1b7a40b0c5a4aa99189029455ca7;0;2;7;FLOAT2;0,0;False;8;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;51;-3026.946,992.538;Inherit;False;0;50;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;77;740.2292,955.3581;Inherit;False;Property;_Opacity;Opacity;19;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-2813.383,1876.529;Inherit;False;Property;_DissolveFloat;Dissolve_Intensity;16;0;Create;False;1;UV Mask;0;0;False;0;False;0.4526118;0.6553627;0;1;0;1;FLOAT;0
WireConnection;17;0;25;1
WireConnection;18;0;61;0
WireConnection;18;1;60;0
WireConnection;19;0;25;2
WireConnection;20;0;24;0
WireConnection;20;1;18;0
WireConnection;21;0;20;0
WireConnection;21;1;60;0
WireConnection;22;0;65;0
WireConnection;22;1;18;0
WireConnection;23;0;74;0
WireConnection;23;1;20;0
WireConnection;23;2;21;0
WireConnection;24;1;70;0
WireConnection;24;0;75;1
WireConnection;27;0;32;0
WireConnection;27;1;30;0
WireConnection;28;0;27;0
WireConnection;28;1;41;0
WireConnection;28;2;31;0
WireConnection;29;0;28;0
WireConnection;29;2;40;0
WireConnection;32;0;39;0
WireConnection;35;0;44;0
WireConnection;35;1;41;0
WireConnection;36;0;35;0
WireConnection;36;1;37;0
WireConnection;37;0;46;0
WireConnection;37;1;41;0
WireConnection;39;1;38;0
WireConnection;39;0;42;1
WireConnection;41;0;49;0
WireConnection;44;0;39;0
WireConnection;44;1;45;0
WireConnection;46;0;44;0
WireConnection;46;1;48;0
WireConnection;48;0;64;0
WireConnection;48;1;47;0
WireConnection;49;0;50;0
WireConnection;49;1;50;4
WireConnection;50;1;53;0
WireConnection;53;9;55;0
WireConnection;53;1;52;0
WireConnection;59;0;36;0
WireConnection;59;1;58;0
WireConnection;59;2;43;0
WireConnection;59;3;58;4
WireConnection;62;0;22;0
WireConnection;62;1;60;0
WireConnection;63;0;74;0
WireConnection;63;1;22;0
WireConnection;63;2;62;0
WireConnection;65;0;24;0
WireConnection;65;1;66;0
WireConnection;67;1;68;0
WireConnection;67;0;29;0
WireConnection;67;2;35;0
WireConnection;67;3;73;0
WireConnection;71;0;23;0
WireConnection;71;1;41;0
WireConnection;72;0;63;0
WireConnection;72;1;41;0
WireConnection;73;0;71;0
WireConnection;73;1;72;0
WireConnection;74;1;25;1
WireConnection;74;0;17;0
WireConnection;74;2;25;2
WireConnection;74;3;19;0
WireConnection;1;1;2;0
WireConnection;2;9;3;0
WireConnection;2;1;4;0
WireConnection;76;0;1;0
WireConnection;76;1;67;0
WireConnection;76;2;77;0
WireConnection;0;0;76;0
WireConnection;55;7;51;0
WireConnection;55;8;54;0
ASEEND*/
//CHKSM=BB91C79432D43A692DE7EFA6CCB92BAD69219C44