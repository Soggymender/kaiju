// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ElectricShock/ElectricShock_General"
{
 
	Properties{
	     _IsShock("IsShock",Range(0,1))=1//Is being electrocuted

		 _MainColor ("Color", Color) = (1,1,1,1)//Main color of the model
		 _MainTex("MainTexture", 2D) = "white"{}//Main texture of the model
		 _BumpMap ("NormalTexture", 2D) = "bump" {}//Bumpmap texture of the model

	     _RimColor ("Rim Color", Color) = (1,0.99,0.392,1)//The color of the surface that shines
		 _RimPower ("Rim Power",Range(-10,3)) = -1.5//Intensity of surface flash
		 _ShockLightSpeed("ShockLightSpeed",Float)=30//The speed of the surface flash

		 _OutlineCol("OutlineColor", Color) = (0.99,0.99,0.46,1)//The color of the edge of the shock
		 _OutlineFactor("OutlineFactor", Range(0,1)) = 0.029//Thickness of the shock edge
		 _WaveSpeed("WaveSpeed",Float)=178//The speed at which the edge of the shock wriggles
		
		_ShakeSpeedX("ShakeSpeedX",Float)=0//The velocity of the model's vibration in the X direction
		_ShakeSpeedY("ShakeSpeedY",Float)=60//The velocity of the model's vibration in the Y direction
		_ShakePowerX("ShakePowerX",Float)=40//Stability of the X direction vibration of the model
		_ShakePowerY("ShakePowerY",Float)=40//Stability of the Y direction vibration of the model

		

	}
 

	SubShader
	{		
		Tags{"Queue" = "Transparent" }
		//Outline
		Pass
		{			
			Cull Front			
			ZWrite Off			
			CGPROGRAM
			#include "UnityCG.cginc"
			fixed4 _OutlineCol;
			float _OutlineFactor;
			float _WaveSpeed;
			float _IsShock;
			float _ShakeSpeedX;
			float _ShakeSpeedY;
			float _ShakePowerX;
			float _ShakePowerY;
			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				
				o.pos = UnityObjectToClipPos(v.vertex);
				
				float3 vnormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				
				float2 offset = TransformViewToProjection(vnormal.xy);
				
				float movev=sin(3.1416 * _Time.y*_WaveSpeed * (v.texcoord.y+20))*0.01+(sin(v.vertex.x))/216*v.vertex.xy;

	            if(_ShakePowerX<=0){_ShakePowerX=1;}
				if(_ShakePowerY<=0){_ShakePowerY=1;}
				if(_IsShock<0){_IsShock=0;}
				if(_IsShock>1){_IsShock=1;}
				o.pos.xy += offset * _OutlineFactor*_IsShock+movev*_IsShock+float2(sin(_Time.y*_ShakeSpeedX)/_ShakePowerX, sin(_Time.y*_ShakeSpeedY)/_ShakePowerY)*_IsShock;
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				if(_IsShock<=0){clip(-1);}
				return _OutlineCol;
			}
			
			
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
		  CGPROGRAM
        #pragma surface surf Lambert 	vertex:vert

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;
        };
        sampler2D _MainTex;
        sampler2D _BumpMap;
        float4 _RimColor;
        float _RimPower;
		float _ShockLightSpeed;
		float4 _MainColor;
		float _IsShock;
		float _ShakeSpeedX;
		float _ShakeSpeedY;
		float _ShakePowerX;
		float _ShakePowerY;


	    void vert (inout appdata_full v) {
		    if(_ShakePowerX<=0){_ShakePowerX=1;}
			if(_ShakePowerY<=0){_ShakePowerY=1;}
			if(_IsShock<0){_IsShock=0;}
			if(_IsShock>1){_IsShock=1;}
            v.vertex.xy += float2(sin(_Time.y*_ShakeSpeedX)/_ShakePowerX, sin(_Time.y*_ShakeSpeedY)/_ShakePowerY)*_IsShock;
        }
        void surf (Input IN, inout SurfaceOutput o) {
		    if(_IsShock<0){_IsShock=0;}
			if(_IsShock>1){_IsShock=1;}
            o.Albedo = (tex2D (_MainTex, IN.uv_MainTex)*_MainColor).rgb;
            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgb * pow (rim, _RimPower)*sin(3.1416 * _Time.y*_ShockLightSpeed)*(sin(IN.uv_MainTex.x))/10*_IsShock;
        }
        ENDCG
	}

	FallBack "Diffuse"
}
