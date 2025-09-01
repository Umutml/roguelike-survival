using System;

namespace Interfaces
{
    public interface INPCBehavior
    {
        public event Action<bool> OnStateChanged;

        public void Execute(bool isActive);
    }
}
