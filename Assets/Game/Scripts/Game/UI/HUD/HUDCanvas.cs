using DG.Tweening;
using StarSmithGames.Go;
using System;

namespace Game.UI
{
    public sealed class HUDCanvas : ViewBase
    {
        public HUDLocation HUDLocation;

        private void Awake()
        {
            HUDLocation.Enable( false );
        }

        public void DoLocation( string title )
        {
            HUDLocation.Title.text = title;
            HUDLocation.Enable( false );
            
            HUDLocation.Show( () =>
            {
                DOTween.Sequence()
                    .AppendInterval( 0.99f )
                    .AppendCallback( () => HUDLocation.Hide() );
            });
        }
    }
}