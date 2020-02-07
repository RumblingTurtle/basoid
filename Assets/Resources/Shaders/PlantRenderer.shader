Shader "Custom/PlantRenderer"
{
	Properties
	{
		_DataTex("Data texture", 2D) = "white" {}
		_AtlasTex("Atlas", 2D) = "white" {}
		_DataTexRes("DataTexRes", Range(0,255)) = 30
		_TileTypeNum("Number of tile types",int) = 4
		_TileRes("Tile resolution",int) = 32
		_BiomeNum("Number of biome types",int) = 3
		_PosBaseOffset("Position offset",Range(0.0,0.5)) = 0.4
		_ScaleBaseOffset("Scale offset",Range(0.0,1.0)) = 0.5
		_WindSpeed("Oscillation speed",Range(0,150)) = 21
		_MovementAmplitude("Movement amplitude",Range(0.0,3.0)) = 2.0
		_Baked("Baked", Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _DataTex;
				float4 _DataTex_ST;
				sampler2D _AtlasTex;
				half _DataTexRes;
				int _TileTypeNum;
				int _TileRes;
				int _BiomeNum;
				fixed _ScaleBaseOffset;
				fixed _PosBaseOffset;
				int _WindSpeed;
				int _MovementAmplitude;
				float _Baked;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _DataTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				float random(float2 p)
				{
					float2 K1 = float2(
						23.14069263277926, // e^pi (Gelfond's constant)
						2.665144142690225 // 2^sqrt(2) (Gelfondâ€“Schneider constant)
					);
					return frac(cos(dot(p, K1)) * 12345.6789);
				}


				float4 ThreeColorLerp(float4 c1, float4 c2, float4 c3, float lerpVal)
				{
					float lrp = (lerpVal + 2) / 2;

					if (lrp >= 0.75)
					{
						return saturate(lerp(c2, c1, (lrp - 0.75) / 0.25));
					}
					if (lrp >= 0.5 &&  lrp < 0.75)
					{
						return saturate(lerp(c3, c2, (lrp - 0.5) / 0.25));
					}
					if (lrp >= 0.25 &&  lrp < 0.5)
					{
						return saturate(lerp(c2, c3, (lrp - 0.25) / 0.25));
					}
					return saturate(lerp(c1, c2, lrp / 0.25));
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = fixed4(0, 0, 0, 0); //Result variable
					float2 scaledUV = IN.uv*_DataTexRes; //Scale UV in order to sample correct pixel from data texture
					
					int2 cellCoords = floor(scaledUV); //Find current cell coordinates
					float2 offset; //calculate uv offset within the cell

					fixed4 cellData = tex2D(_DataTex, cellCoords / _DataTexRes);
					int currentTileType = cellData.b * 255.0f;

					int i = -1, j = -1;

					for (i = -1; i <= 1; i++)
					{
						for (j = -1; j <= 1; j++)
						{

							float2 cellPos = cellCoords + float2(i, j);
							if (cellPos.x >= _DataTexRes || cellPos.y >= _DataTexRes || cellPos.x < 0 || cellPos.y < 0)
								continue;

							cellData = tex2D(_DataTex, cellPos / _DataTexRes);
							offset = scaledUV - cellPos;

							float xTileOffset = cellData.r;
							float yTileOffset = cellData.g;

							float tileScale = cellData.a;

							int tileType = fmod(cellData.b * 255.0, _TileTypeNum);
							int biomeOffset = (cellData.b * 255.0 - tileType) / _TileTypeNum;


							offset.x -= (xTileOffset*2.0f - 1.0f)*_PosBaseOffset;
							offset.y -= (yTileOffset*2.0f - 1.0f)*_PosBaseOffset;


							float2 jiggleOffset = offset;

							offset.x = saturate(offset.x);
							offset.y = saturate(offset.y);

							float distToRoot = offset.y;

							offset.x *= _TileRes;
							offset.y *= _TileRes;

							offset.x += biomeOffset * _TileRes;
							offset.y += tileType * _TileRes;

							offset.x /= _BiomeNum * _TileRes;
							offset.y /= _TileTypeNum * _TileRes;

							if (_Baked != 0)
								offset.x += _MovementAmplitude*sin(random(cellCoords + float2(i, j))*10+_Time * _WindSpeed)*pow(jiggleOffset.y/7.0,2);

							float4 spriteSample = tex2D(_AtlasTex, offset);

							if (spriteSample.a > 0.0) {
								float blendLeft = (1 - c.a);

								c += spriteSample * spriteSample.a * blendLeft;

								if (_Baked == 0 && tileType > 0)
								{
									c.a = distToRoot;
								}
							}
						}
					}

					if (_Baked == 0)
						return c;
					else
						return c *ThreeColorLerp(float4(1.0, 1.0, 1.0, 1.0), float4(1.0, 0.5, 0.0, 1.0), float4(0.0, 0.2, 0.4, 1.0), _SinTime.y);
				}
				ENDCG
			}
		}
}
