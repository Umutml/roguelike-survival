using _Scripts.Utilities;
using Pathfinding;
using UnityEngine;
using Utilities;

namespace _Utilities
{
    public static class AstarPathHelper
    {
        public static Vector3? FindNearestWalkablePosition(Vector3 desiredPosition)
        {
            var node = AstarPath.active.GetNearest(desiredPosition, NNConstraint.Walkable).node;
            if (node is {Walkable: true})
            {
                if (Vector3.Distance((Vector3)node.position, desiredPosition) > 3)
                    return null;
                return (Vector3) node.position;
            }

            LoggerNS.LogWarning("No walkable position found near the desired position.");
            return null;
        }
        public static Vector3? FindNearestWalkablePosition(Vector3 desiredPosition, Camera camera)
        {
            var node = AstarPath.active?.GetNearest(desiredPosition, NNConstraint.Walkable).node;

            if (node is {Walkable: true})
            {
                var closestWalkablePosition = (Vector3) node.position;

                if (camera.IsInViewport(closestWalkablePosition))
                    return null;

                return closestWalkablePosition;
            }

            LoggerNS.LogWarning("No walkable position found near the desired position.");
            return null;
        }


        public static Vector3? GetRandomWalkablePosition(Vector3 desiredPosition, float radius, Camera camera)
        {
            var randomAngle = Random.Range(0f, Mathf.PI * 2f);

            var randomRadius = Mathf.Sqrt(Random.Range(0f, 1f)) * radius;

            var xOffset = Mathf.Cos(randomAngle) * randomRadius;
            var zOffset = Mathf.Sin(randomAngle) * randomRadius;

            var randomPosition = new Vector3(desiredPosition.x + xOffset, desiredPosition.y, desiredPosition.z + zOffset);

            return FindNearestWalkablePosition(randomPosition, camera);
        }
    }
}
