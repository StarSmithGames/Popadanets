using System;
using System.Collections.Generic;

namespace StarSmithGames.Core.Utils
{
    public class Observer< T >
    {
        public event Action< T > onItemAdded;
        public event Action< T > onItemRemoved;
        public event Action onCollectionChanged;
        
        public readonly List< T > Observables = new();
        
        public virtual void Add( T observable )
        {
            Observables.Add( observable );
            
            onItemAdded?.Invoke( observable );
            onCollectionChanged?.Invoke();
        }

        public virtual void Remove( T observable )
        {
            Observables.Remove( observable );
            
            onItemRemoved?.Invoke( observable );
            onCollectionChanged?.Invoke();
        }

        public virtual bool Contains( T observable )
        {
            return Observables.Contains( observable );
        }
    }
}