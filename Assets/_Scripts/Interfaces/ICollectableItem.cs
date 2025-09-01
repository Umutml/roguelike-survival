using UnityEngine;
using VContainer;

namespace Interfaces
{
    public interface ICollectableItem
    {
        public bool IsCollected { get; }
        public float Distance { get; }
        public float Progress { get; set; }
        public float ProgressSpeed { get; }
        public Transform Transform { get; }
        public void Initialize();
        public void Collect(IObjectResolver resolver);
        public void Reset();
    }
}
