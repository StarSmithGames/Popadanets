using UnityEngine;

namespace Game
{
    public static class Layers
    {
        public static readonly int DEFAULT = LayerMask.GetMask( "Default" );
        public static readonly int ENVIRONMENT = LayerMask.GetMask( "Environment" );

        public static bool Contains( int mask, int layer)
        {
            return ( mask & 1 << layer ) > 0;
        }
        
        public static int Combine( int layerX, int layerY )
        {
            return (1 << layerX) | (1 << layerY);
        }
        
        public static int Flip( int layer )
        {
            return ~layer;
        }
    }
}