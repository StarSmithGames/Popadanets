using DG.Tweening;
using System;
using UnityEngine;
using Zenject;

namespace Game.UI
{
    public sealed class RootCanvas : MonoBehaviour
    {
        public HUDCanvas HUDCanvas;
        public CharacterCanvas CharacterCanvas;
        
        private void Start()
        {
            HUDCanvas.Enable( true );
            
            DOTween.Sequence()
                .AppendInterval( 1f )
                .AppendCallback( () => HUDCanvas.DoLocation( "Mine" ) );
        }

        private void Update()
        {
            if ( Input.GetKeyDown( KeyCode.Tab ) )
            {
                if ( CharacterCanvas.IsShowing )
                {
                    CharacterCanvas.Hide();
                }
                else
                {
                    CharacterCanvas.Show();
                }
            }
        }
    }
}