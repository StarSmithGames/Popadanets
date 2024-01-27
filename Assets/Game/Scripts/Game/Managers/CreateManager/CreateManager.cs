using System;
using Zenject;

namespace Game.Managers.CreateManager
{
    public sealed class CreateManager
    {
        public readonly DiContainer Container;
        
        public CreateManager( DiContainer container )
        {
            Container = container ?? throw new ArgumentNullException( nameof(container) );
        }
    }
}