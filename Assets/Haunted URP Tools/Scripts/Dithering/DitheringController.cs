using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PSX
{
    [ExecuteInEditMode]
    public class DitheringController : MonoBehaviour
    {
        [SerializeField] protected VolumeProfile volumeProfile;
        [SerializeField] protected bool isEnabled = true;

        protected Dithering dithering;

        [SerializeField] protected int patternIndex = 0;
        [SerializeField] protected float ditherThreshold = 1;
        [SerializeField] protected float ditherStrength = 1;
        [SerializeField] protected float ditherScale = 2;

        // color depth and bayer size options.
        [Range(1, 8)]
        [SerializeField] protected int rBitDepth = 8;
        [Range(1, 8)]
        [SerializeField] protected int gBitDepth = 8;
        [Range(1, 8)]
        [SerializeField] protected int bBitDepth = 8;

        [Range(1, 4096)]
        protected int ditherSeparation = 256;

        protected float intensity = 0;

        protected int downscaleFactor = 4;


        protected void Update()
        {
            this.SetParams();
        }

        protected void SetParams()
        {
            if (!this.isEnabled) return;
            if (this.volumeProfile == null) return;
            if (this.dithering == null) volumeProfile.TryGet<Dithering>(out this.dithering);
            if (this.dithering == null) return;

            this.dithering.patternIndex.value = this.patternIndex;
            this.dithering.ditherThreshold.value = this.ditherThreshold;
            this.dithering.ditherStrength.value = this.ditherStrength;
            this.dithering.ditherScale.value = this.ditherScale;

            this.dithering.rBitDepth.value = this.rBitDepth;
            this.dithering.gBitDepth.value = this.gBitDepth;
            this.dithering.bBitDepth.value = this.bBitDepth;

            this.dithering.intensity.value = this.intensity;
            this.dithering.downscaleFactor.value = this.downscaleFactor;

        }
    }
}