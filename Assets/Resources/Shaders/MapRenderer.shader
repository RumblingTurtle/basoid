Shader "Custom/MapRenderer"
{
	Properties
	{
		_DataTex("Data texture", 2D) = "white" {}
		_AtlasTex("Atlas", 2D) = "white" {}
		_BakedTerrain("Baked terrain", 2D) = "white" {}
		_BakedPlants("Baked plants", 2D) = "white" {}

		_DataTexRes("DataTexRes", Range(0,255)) = 30
		_TileTypeNum("Number of tile types",int) = 6
		_BiomeNum("Number of biome types",int) = 3
		_TileRes("Tile resolution",int) = 32
		_PosBaseOffset("Position offset",Range(0.0,0.5)) = 0.6
		_ScaleBaseOffset("Scale offset",Range(0.001,1.0)) = 0.01
		_SunZ("Z sun angle",Range(1,90)) = 7.7
		_Baked("Baked", Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100
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
				sampler2D _BakedTerrain;
				sampler2D _BakedPlants;
				float _Baked;
				float4 _DataTex_ST;
				float _SunZ;
				sampler2D _AtlasTex;
				fixed _DataTexRes;
				int _TileTypeNum;
				int _BiomeNum;
				int _TileRes;
				fixed _ScaleBaseOffset;
				fixed _PosBaseOffset;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _DataTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				float3 height2normal_sobel(float3x3 c)
				{
					float3x3 x = float3x3(1.0, 0.0, -1.0,
										  2.0, 0.0, -2.0,
						                  1.0, 0.0, -1.0);

					float3x3 y = float3x3(1.0, 2.0, 1.0,
						                  0.0, 0.0, 0.0,
						                -1.0, -2.0, -1.0);

					x = x * c;
					y = y * c;

					float cx = x[0][0] + x[0][2]
						+ x[1][0] + x[1][2]
						+ x[2][0] + x[2][2];

					float cy = y[0][0] + y[0][1] + y[0][2]
						+ y[2][0] + y[2][1] + y[2][2];

					float cz = sqrt(1 - (cx*cx + cy*cy));

					return float3(cx, cy, cz);
				}

				float3x3 img3x3(sampler2D color_map, float2 tc, float ts, int ch)
				{
					float   d = 1.0 / ts; // ts, texture sampling size
					float3x3 c;
					c[0][0] = tex2D(color_map, tc + float2(-d, -d))[ch];
					c[0][1] = tex2D(color_map, tc + float2(0, -d))[ch];
					c[0][2] = tex2D(color_map, tc + float2(d, -d))[ch];

					c[1][0] = tex2D(color_map, tc + float2(-d, 0))[ch];
					c[1][1] = tex2D(color_map, tc)[ch];
					c[1][2] = tex2D(color_map, tc + float2(d, 0))[ch];

					c[2][0] = tex2D(color_map, tc + float2(-d, d))[ch];
					c[2][1] = tex2D(color_map, tc + float2(0, d))[ch];
					c[2][2] = tex2D(color_map, tc + float2(d, d))[ch];

					return c;
				}


				float2 PointOnLine(float2 start, float angle, float length) {
					float x = length * cos(angle);
					float y = length * sin(angle);

					return float2(start.x + x, start.y + y);
				}

				float PixelHeightAtPoint(float2 texCoord, float LightAngleXY, float distance)
				{
					float2 newTexCoord = PointOnLine(texCoord, LightAngleXY, distance);
					float plantHeight = tex2D(_BakedPlants,newTexCoord).a;
					float height = tex2D(_BakedTerrain, newTexCoord).a + plantHeight/_TileTypeNum;
					return height;
				}

				float GetRayHeightAtPoint(float height, float LightAngleZ, float distance) {
					return distance * tan(LightAngleZ) + height;
				}

				float TraceLight(float LightAngleXY, float LightAngleZ, float2 texCoord, float step) {

					float distance; // current distance along the line from current heightmap pixel towards the light
					float currentHeight; // value of currently tested heightmap pixel
					float newHeight; // values of heightmap pixels lying somewhere on the line towards the light from current position
					float rayHeight; // height of a ray drawn from currentHeight along the light Z angle, sampled at a certain position

					currentHeight = tex2D(_BakedTerrain, texCoord).a;

					for (int i = 0; i < 100; i++) {
						distance = step * i;
						float2 nextPoint = PointOnLine(texCoord, LightAngleXY, distance);

						newHeight = PixelHeightAtPoint(texCoord, LightAngleXY, distance);
						if (length(nextPoint - saturate(nextPoint)) != 0 && newHeight > currentHeight)
							break;

						if (newHeight > currentHeight) { // there's a higher point on the line from current pixel to light
							rayHeight = GetRayHeightAtPoint(currentHeight, LightAngleZ, distance);
							if (rayHeight <= newHeight) { // the higher point also blocks the direct visibility from light to current pixel,  current pixel is in shadow
								return distance;
							}
						}
					}

					return 1.0; // pixel is not occluded
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

					if (_Baked == 0)
					{
						float2 scaledUV = IN.uv*_DataTexRes; //Scale UV in order to sample correct pixel from data texture

						int2 cellCoords = floor(scaledUV); //Find current cell coordinates
						float2 offset; //calculate uv offset within the cell

						fixed4 cellData = tex2D(_DataTex, cellCoords / _DataTexRes);

						int currentTileType = fmod(cellData.b * 255.0, _TileTypeNum);
						int currentBiomeOffset = (cellData.b * 255.0 - currentTileType) / _TileTypeNum;


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
								int biomeOffset = ((cellData.b * 255.0 - tileType) / _TileTypeNum);

								offset.x -= (xTileOffset*2.0f - 1.0f)*_PosBaseOffset;
								offset.y -= (yTileOffset*2.0f - 1.0f)*_PosBaseOffset;


								offset.x = saturate(offset.x);
								offset.y = saturate(offset.y);



								if (offset.x == 0.0 || offset.x == 1.0 || offset.y == 0.0 || offset.y == 1.0)
									continue;

								offset.x *= _TileRes;
								offset.y *= _TileRes;

								offset.x += biomeOffset * _TileRes;
								offset.y += tileType * _TileRes;

								offset.x /= _BiomeNum * _TileRes;
								offset.y /= _TileTypeNum * _TileRes;

								float4 spriteSample = tex2D(_AtlasTex, offset);

								if (currentTileType < tileType)
								{
									c = spriteSample;
									if (tileType == 0)
										tileType += 1;
									if (tileType == 4)
										tileType -= 1;

									c.a = float(tileType + 1) / _TileTypeNum;
								}
							}
						}

						if (c.a == 0.0)
						{
							float2 offset = scaledUV - cellCoords;

							offset.x *= _TileRes;
							offset.y *= _TileRes;

							offset.x += currentBiomeOffset * _TileRes;
							offset.y += currentTileType * _TileRes;

							offset.x /= _BiomeNum * _TileRes;
							offset.y /= _TileTypeNum * _TileRes;

							float4 spriteSample = tex2D(_AtlasTex,offset);
							c = spriteSample;

							if (currentTileType == 0)
								currentTileType += 1;
							if (currentTileType == 4)
								currentTileType -= 1;

							c.a = float(currentTileType + 1) / _TileTypeNum;
						}
					}
					else
					{
						c = fixed4(0,0,0,0);
						fixed4 bakedSample = tex2D(_BakedTerrain, IN.uv);
						c = fixed4(bakedSample.rgb, 1.0);
						float3x3 neighbours = img3x3(_BakedTerrain, IN.uv, 512, 3);
						float3 normal = height2normal_sobel(neighbours);

						normal = normalize(float3(normal.xy, normal.z));
						c.xyz *= normal.z;// *abs(_SinTime.w);

						const float LightAngleXY = lerp(0,-90,_SinTime.x/100);
						const float LightAngleZ = lerp(70, 85, _SinTime.w / 1000);
						const float TextureStep = 0.001;

						float MovingLight = LightAngleXY + abs(_SinTime.x);
						float lightLevel = TraceLight(MovingLight, LightAngleZ, IN.uv, TextureStep);

						c *= saturate(0.5 + lightLevel) * ThreeColorLerp(float4(1.0, 1.0, 1.0, 1.0), float4(1.0, 0.5, 0.0, 1.0),float4(0.0,0.2,0.4,1.0),_SinTime.y);

					}

					return c;
				}
				ENDCG

			}
		}
}
