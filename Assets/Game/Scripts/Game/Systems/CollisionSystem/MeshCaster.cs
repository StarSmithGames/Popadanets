using Sirenix.OdinInspector;
using StarSmithGames.Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.CollisionSystem
{
    public sealed class MeshCaster : MonoBehaviour
    {
        public LayerMask Mask = -1;
        [Space]
        public MeshFilter MeshFilter;
        public MeshDrawSettings FacesSettings;
        public MeshDrawSettings TrianglesSettings;
        public MeshDrawSettings VertexsSettings;
        
        public readonly Observer< Collider > Observer = new();

        private void OnTriggerEnter( Collider other )
        {
            if ( !Layers.Contains( Mask.value, other.gameObject.layer ) ) return;
            
            if ( !Observer.Contains( other ) )
            {
                Observer.Add( other );
            }
        }

        private void OnTriggerExit( Collider other )
        {
            if ( !Layers.Contains( Mask.value, other.gameObject.layer ) ) return;
            
            if ( Observer.Contains( other ) )
            {
                Observer.Remove( other );
            }
        }

        private void OnDrawGizmos()
        {
            if ( MeshFilter == null ) return;

            Mesh mesh = MeshFilter.sharedMesh;

            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            List< Vector3 > centres = new();
            
            //Draw Normals
            for ( int i = 0; i < triangles.Length; i += 3 )
            {
                Vector3 v0 = transform.TransformPoint( vertices[ triangles[ i ] ] );
                Vector3 v1 = transform.TransformPoint( vertices[ triangles[ i + 1 ] ] );
                Vector3 v2 = transform.TransformPoint( vertices[ triangles[ i + 2 ] ] );
                Vector3 center = ( v0 + v1 + v2 ) / 3;

                Vector3 dir = Vector3.Cross( v1 - v0, v2 - v0 );
                dir /= dir.magnitude;

                centres.Add( center );
            }

            for ( int i = 0; i < centres.Count; i += 2 )
            {
                var center = (centres[ i ] + centres[ i + 1 ]) * 0.5f;
                
                Draw( FacesSettings, center, center + normals[ i ]);
            }
            
            
            //Draw Triangle Normals
            for ( int i = 0; i < triangles.Length; i += 3 )
            {
                Vector3 v0 = transform.TransformPoint( vertices[ triangles[ i ] ] );
                Vector3 v1 = transform.TransformPoint( vertices[ triangles[ i + 1 ] ] );
                Vector3 v2 = transform.TransformPoint( vertices[ triangles[ i + 2 ] ] );
                Vector3 center = ( v0 + v1 + v2 ) / 3;

                Vector3 dir = Vector3.Cross( v1 - v0, v2 - v0 );
                dir /= dir.magnitude;

                Draw( TrianglesSettings, center, dir );
            }

            //Draw Vertex Normals
            for ( int i = 0; i < vertices.Length; i++ )
            {
                Draw( VertexsSettings, transform.TransformPoint( vertices[ i ] ), transform.TransformVector( normals[ i ] ) );
            }
        }
        
        public void Draw( MeshDrawSettings settings, Vector3 from, Vector3 direction )
        {
            if ( !settings.Enable ) return;
            
            if ( Camera.current.transform.InverseTransformDirection( direction ).z < 0f )
            {
                Gizmos.color = settings.BaseColor;
                Gizmos.DrawWireSphere( from, settings.BaseSize );

                Gizmos.color = settings.NormalColor;
                Gizmos.DrawRay( from, direction * settings.Length );
            }
        }
    }

    [ System.Serializable ]
    public sealed class MeshDrawSettings
    {
        public bool Enable = false;
        [ ReadOnly ]
        public float BaseSize = 0.0125f;
        [ Space ]
        public float Length = 0.3f;
        public Color NormalColor = new Color32(34, 221, 221, 155);
        public Color BaseColor = new Color32( 255, 133, 0, 255 );
    }
}