using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PSX
{
    public class Dithering : VolumeComponent, IPostProcessComponent
    {
        //PIXELATION
        //public TextureParameter ditherTexture;
        public IntParameter patternIndex = new IntParameter(0);
        public FloatParameter ditherThreshold = new FloatParameter(512);
        public FloatParameter ditherStrength = new FloatParameter(1);
        public FloatParameter ditherScale = new FloatParameter(2);

        public ClampedIntParameter rBitDepth = new ClampedIntParameter(8, 1, 8);
        public ClampedIntParameter gBitDepth = new ClampedIntParameter(8, 1, 8);
        public ClampedIntParameter bBitDepth = new ClampedIntParameter(8, 1, 8);

        public ClampedIntParameter ditherSeparation = new ClampedIntParameter(256, 1, 4096);

        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        public ClampedIntParameter downscaleFactor = new ClampedIntParameter(4, 1, 8);

        //INTERFACE REQUIREMENT 
        public bool IsActive() => true;
        public bool IsTileCompatible() => false;
    }
}