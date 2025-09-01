using UnityEngine;

namespace GameCore.DynamicGridObstacle
{
    public class DynamicObstacle : MonoBehaviour
    {
        [SerializeField] private Pathfinding.DynamicGridObstacle dynamicObstacle;

        public void ToggleObstacle(bool isActive)
        {
            dynamicObstacle.gameObject.SetActive(isActive);
        }

        public void ToggleTrigger(bool isTrigger)
        {
            if (!dynamicObstacle.TryGetComponent(out BoxCollider boxCollider))
            {
                return;
            }

            boxCollider.isTrigger = isTrigger;
        }
    }
}