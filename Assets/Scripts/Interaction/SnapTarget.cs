using UnityEngine;

namespace SoilMoistureExperiment.Interaction
{
    public enum SnapTargetType
    {
        Balance,
        Oven,
        Desiccator,
        TrashBin
    }

    public class SnapTarget : MonoBehaviour
    {
        [Header("Settings")]
        public SnapTargetType TargetType;
        public Transform SnapPosition;
        public float SnapRadius = 1f;

        // State
        public bool IsOccupied { get; private set; }

        private void OnDrawGizmosSelected()
        {
            // 在编辑器中显示吸附范围
            Gizmos.color = Color.green;
            Vector3 center = SnapPosition != null ? SnapPosition.position : transform.position;
            Gizmos.DrawWireSphere(center, SnapRadius);
        }

        public bool IsInRange(Vector3 position)
        {
            Vector3 center = SnapPosition != null ? SnapPosition.position : transform.position;
            return Vector3.Distance(position, center) <= SnapRadius;
        }

        public Vector3 GetSnapPosition()
        {
            return SnapPosition != null ? SnapPosition.position : transform.position;
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }
    }
}
