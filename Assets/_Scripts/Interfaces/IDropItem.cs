using UnityEngine;
using VContainer;

namespace Interfaces
{
    public interface IDropItem
    {
        public IObjectResolver Resolver { get; set; }
        public Transform Transform { get; }
        public float? OptionalDistance { get; }
        public bool IsPickedUp { get; }
        public bool IsPickable { get; }
        public void Initialize(int value, bool isHidden = false);
        public void Use();
        public void Reset();
    }
}