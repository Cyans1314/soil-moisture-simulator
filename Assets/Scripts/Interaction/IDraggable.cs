using UnityEngine;

namespace SoilMoistureExperiment.Interaction
{
    public interface IDraggable
    {
        bool CanDrag { get; }
        Vector3 OriginalPosition { get; }
        
        void OnDragStart();
        void OnDrag(Vector3 worldPosition);
        void OnDragEnd(bool success);
        void ResetPosition();
    }
}
