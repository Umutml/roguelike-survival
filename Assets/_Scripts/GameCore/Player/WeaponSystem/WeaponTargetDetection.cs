using System;
using System.Collections;
using System.Linq;
using GameCore.Spawner;
using UnityEngine;

namespace GameCore.Player.WeaponSystem
{
    public class WeaponTargetDetection : MonoBehaviour
    {
        #region Serialized Fields

        [Range(10, 170)]
        [SerializeField] private float innerAngle = 45f;
        [SerializeField] private float edgeLength = 1f;

        #endregion

        #region Private Fields

        private readonly Vector3[] _triangleVertices = new Vector3[3];
        private readonly WaitForSeconds _targetWait = new(0.2f);
        private MobManager _mobManager;
        private Coroutine _detectionCoroutine;

        #endregion

        #region Properties

        public bool IsTarget { get; private set; }

        #endregion


        #region Public Methods

        public void Initialize(MobManager mobManager)
        {
            _mobManager = mobManager;
            _detectionCoroutine = StartCoroutine(nameof(CheckDetection));
        }

        public void Dispose()
        {
            if (_detectionCoroutine != null)
            {
                StopCoroutine(_detectionCoroutine);
                _detectionCoroutine = null;
            }
            IsTarget = false;
        }

        #endregion

        #region Private Methods

        private IEnumerator CheckDetection()
        {
            while (true)
            {
                yield return _targetWait;
                UpdateTriangleVertices();
                CheckForTargets();
            }
        }

        private void UpdateTriangleVertices()
        {
            var objectPosition = transform.position;
            var direction = transform.forward;

            _triangleVertices[0] = objectPosition;
            _triangleVertices[1] = objectPosition + Quaternion.Euler(0, -innerAngle / 2, 0) * direction * edgeLength;
            _triangleVertices[2] = objectPosition + Quaternion.Euler(0, innerAngle / 2, 0) * direction * edgeLength;
        }

        private void CheckForTargets()
        {
            foreach (var enemyPosition in _mobManager.ActiveMobs.Select(t => t.Transform.position))
            {
                if (!IsPointInTriangle(enemyPosition,
                    _triangleVertices[0],
                    _triangleVertices[1],
                    _triangleVertices[2])) continue;
                IsTarget = true;
                return;
            }

            IsTarget = false;
        }

        private bool IsPointInTriangle(Vector3 point, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var totalArea = TriangleArea(v1, v2, v3);
            var area1 = TriangleArea(point, v2, v3);
            var area2 = TriangleArea(v1, point, v3);
            var area3 = TriangleArea(v1, v2, point);

            return Mathf.Abs(totalArea - (area1 + area2 + area3)) < 0.001f;
        }

        private float TriangleArea(Vector3 a, Vector3 b, Vector3 c) =>
            Mathf.Abs((a.x * (b.z - c.z) + b.x * (c.z - a.z) + c.x * (a.z - b.z)) / 2f);

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            if (_triangleVertices is not {Length: 3}) return;
            Gizmos.DrawLine(_triangleVertices[0], _triangleVertices[1]);
            Gizmos.DrawLine(_triangleVertices[1], _triangleVertices[2]);
            Gizmos.DrawLine(_triangleVertices[2], _triangleVertices[0]);
        }

        #endregion
    }
}
