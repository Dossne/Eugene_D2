using UnityEngine;

namespace Features.PhysicsLogic
{
    public static class LayerConstants
    {
        public static int DefaultLayer;
        public static int NoCollabPhysLayer;


        public static void Initialize()
        {
            DefaultLayer = LayerMask.NameToLayer("Default");
            NoCollabPhysLayer = LayerMask.NameToLayer("NoCollabPhys");
        }
    }
}