using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using VContainer;

namespace Infrastructure.QualityGraphicsControl
{
    public class ShadowsController
    {
        private readonly GraphicsQualityHandler graphicsQualityHandler;


        public ShadowsController(GraphicsQualityHandler graphicsQualityHandler)
        {
            this.graphicsQualityHandler = graphicsQualityHandler;
        }


        public void SetShadowsMaxDistance(float shadowsMaxDistance)
        {
            graphicsQualityHandler.CurrentRenderPipeline.shadowDistance = shadowsMaxDistance;
        }
    }
}


