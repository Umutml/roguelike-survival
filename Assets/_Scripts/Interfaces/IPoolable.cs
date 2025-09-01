using System;
using Addler.Runtime.Core.Pooling;
using UnityEngine;

namespace Interfaces
{
    public interface IPoolable
    {
        public Action OnReturnToPool { get; set; }
        public Action<Vector3> OnReturnToPoolByPosition { get; set; }
    }
}