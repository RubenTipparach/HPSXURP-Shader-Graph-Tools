Shader "PostEffect/Dithering"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    
    CGINCLUDE
        #include "UnityCG.cginc"
    
        sampler2D _MainTex;  
        float2 _MainTex_TexelSize;

        //for dither     
        uint _PatternIndex; 
        float _DitherThreshold;
        float _DitherStrength;
        float _DitherScale;

        // set via script!
        uniform float iMatrix4x4[16];
        uniform float _Intensity;
        uniform uint _DownscaleFactor;
        uniform int levels;
        uniform float4 pn_bitsPerChannel;

        //for Pixelation      
        float _WidthPixelation;
        float _HeightPixelation;
        
        //for color precision
        float _ColorPrecision;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 position : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 screenPosition : TEXCOORD1;
        };
        
        
        
        float PixelBrightness(float3 col)
        {
            return col.r + col.g + col.b / 3.0; //can use averaging or the dot product to evaluate brightness
            //return dot(col, float3(0.34543, 0.65456, 0.287));
        }
        
        float4 GetTexelSize(float width, float height)
        {
            return float4(1/width, 1/height, width, height);
        }
        
        float Get4x4TexValue(float2 uv, float brightness, float4x4 pattern)
        {        
            uint x = uv.x % 4;
            uint y = uv.y % 4;
            
            if((brightness * _DitherThreshold) < pattern[x][y])
                return 0;
            else
                return 1;
            //return brightness * pattern[x][y];
        }      
        
        v2f Vert(appdata v)
        {
            v2f o;
            o.position = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            o.screenPosition = ComputeScreenPos(o.position);
            return o;
        }

        float roundToNearest(float x, float n) {
            return round(x / n) * n;
        }
        float getScore(fixed4 c0, fixed4 c1) {
            fixed r = c0.r - c1.r;
            fixed g = c0.g - c1.g;
            fixed b = c0.b - c1.b;
            return r * r + g * g + b * b;
        }

        float floorToNearest(float x, float n) {
            return floor(x / n) * n;
        }

        fixed getLuminosity(fixed4 c) {
            return sqrt(0.299 * c.r * c.r + 0.587 * c.g * c.g + 0.114 * c.b * c.b);
        }

        fixed dither_sample(uint x, uint y) {
            float4x4 tab = float4x4 (
                -4.0, 0.0, -3.0, 1.0,
                2.0, -2.0, 3.0, -1.0,
                -3.0, 1.0, -4.0, 0.0,
                3.0, -1.0, 2.0, -2.0
                );

            return tab[x % 4][y % 4] / 4;
        }

        float4 Dither(float3 color, float levels, float limit)
        {
            limit = limit * 2.0 - 1.0;
            float4 colora = float4(color, 1.0);
            return float4(colora.rgb + limit / (levels - 1.0), colora.a);
        }

        float4 Frag (v2f i) : SV_Target
        {
            //base texture
            // todo: downscale this texture...
            // pixelation 
            float2 uv = i.uv;
            //uv.x = floor(uv.x * _WidthPixelation) / _WidthPixelation;
            //uv.y = floor(uv.y * _HeightPixelation) / _HeightPixelation;
            uv.x = floor(uv.x * _WidthPixelation) / _WidthPixelation;
            uv.y = floor(uv.y * _HeightPixelation) / _HeightPixelation;

            float4 c = tex2D(_MainTex, uv);

            //c = floor(c * _ColorPrecision)/_ColorPrecision;


            //dithering  
            float4 texelSize = GetTexelSize(1,1);
            float2 screenPos = i.screenPosition.xy / i.screenPosition.w;
            //uint2 ditherCoordinate = screenPos * _ScreenParams.xy * texelSize.xy;
            float3 offset = (1.0f / pn_bitsPerChannel) * 255.0f;
            offset = ceil(offset) / floor(offset);
            int2 puv = i.uv * _ScreenParams.xy / _DitherScale;

            // Resolution scale
            if (_DitherScale > 1) {
                float2 units = 1 / _ScreenParams.xy;

                i.uv.x = floorToNearest(i.uv.x, units.x * _DitherScale);
                i.uv.y = floorToNearest(i.uv.y, units.y * _DitherScale);
            }

            i.uv.x *= (_ScreenParams.x / _DownscaleFactor);
            i.uv.y *= (_ScreenParams.y / _DownscaleFactor);

            //ditherCoordinate /= _DitherScale;
            
            float brightness = PixelBrightness(c.rgb);
            uint width  = 4;
            uint height = 4;
            uint size = width * height;

            uint x = i.uv.x % width;
            uint y = i.uv.y % height;
            int index = width * y + x;

            float limit = 0;
            limit = iMatrix4x4[index] / size;

  /*          float4x4 ditherPattern = GetDitherPattern(_PatternIndex);
            float ditherPixel = Get4x4TexValue(ditherCoordinate.xy, brightness, ditherPattern) * _DitherStrength / 3;;
            */
/*
            fixed ditherAmount = dither_sample(puv.x, puv.y) * _DitherStrength / 3;

            fixed luminosity = getLuminosity(c);
            ditherAmount *= 4 * luminosity - 4 * luminosity * luminosity;
            c += ditherAmount;
            c = saturate(c);
*/
            c = Dither(c / pn_bitsPerChannel, levels, limit);

            //c -= 0.01;
            //c = saturate(c);


            c.a = 1.0; // Set alpha to 1.

            // I have a hard time this is a different dev, looks like similar code ;)
            //c.rgb = (floor(c * 255.0f) / 255.0f) * offset;
            //c.rgb *= pn_bitsPerChannel;
            c.rgb = (floor(c * 255.0f) / 255.0f) * offset;
            c.rgb *= pn_bitsPerChannel;

            return c;
        }
    ENDCG
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            CGPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDCG
        }
    }
}
