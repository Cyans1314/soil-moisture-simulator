/*
 * 文件名：Oven.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：烘箱脚本，用于烘干土样。
 * 处理门的开关动画、放入取出铝盒、烘干状态管理。
 */

using UnityEngine;
using System.Collections;

namespace SoilMoistureExperiment.Objects
{
    public class Oven : MonoBehaviour
    {
        [Header("References")]
        public Transform Door;
        public Transform DoorPivot; // 门的旋转轴心点
        public Transform InsidePosition;

        [Header("Settings")]
        public float OpenAngle = 100f;
        public float AnimationDuration = 0.5f;

        // State
        public bool IsOpen { get; private set; }
        public bool IsDrying { get; private set; }
        public AluminumBox CurrentBox { get; private set; }

        private Vector3 doorClosedPosition;
        private Quaternion doorClosedRotation;
        private bool isAnimating;

        private void Awake()
        {
            if (Door != null)
            {
                doorClosedPosition = Door.position;
                doorClosedRotation = Door.rotation;
            }
        }

        public void ToggleDoor()
        {
            if (isAnimating || Door == null) return;
            
            IsOpen = !IsOpen;
            StartCoroutine(AnimateDoor(IsOpen));
            Debug.Log($"烘箱门{(IsOpen ? "打开" : "关闭")}");

            // 关门且有铝盒时自动开始烘干
            if (!IsOpen && CurrentBox != null && !IsDrying)
            {
                StartDrying();
            }
        }

        private IEnumerator AnimateDoor(bool open)
        {
            isAnimating = true;
            
            float targetAngle = open ? OpenAngle : -OpenAngle;
            float currentAngle = 0f;
            float anglePerFrame;
            float duration = AnimationDuration;
            float elapsed = 0f;

            if (!open)
            {
                // 直接设置回初始位置
                float step = OpenAngle / (duration / Time.deltaTime);
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    t = t * t * (3f - 2f * t); // Smoothstep
                    
                    // 绕 DoorPivot 的 X 轴旋转（往下开）
                    anglePerFrame = -OpenAngle * Time.deltaTime / duration;
                    if (DoorPivot != null)
                    {
                        Door.RotateAround(DoorPivot.position, DoorPivot.right, anglePerFrame);
                    }
                    yield return null;
                }
                Door.position = doorClosedPosition;
                Door.rotation = doorClosedRotation;
            }
            else
            {
                // 开门
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    anglePerFrame = OpenAngle * Time.deltaTime / duration;
                    if (DoorPivot != null)
                    {
                        Door.RotateAround(DoorPivot.position, DoorPivot.right, anglePerFrame);
                    }
                    yield return null;
                }
            }
            
            isAnimating = false;
        }

        public bool PlaceBox(AluminumBox box)
        {
            if (!IsOpen)
            {
                Debug.LogWarning("烘箱门未打开，无法放入铝盒");
                return false;
            }
            
            if (CurrentBox != null)
            {
                Debug.LogWarning("烘箱内已有铝盒");
                return false;
            }
            
            CurrentBox = box;
            if (InsidePosition != null)
            {
                box.SetPosition(InsidePosition.position);
            }
            Debug.Log("铝盒放入烘箱");
            return true;
        }

        public AluminumBox RemoveBox()
        {
            if (!IsOpen)
            {
                Debug.LogWarning("烘箱门未打开，无法取出铝盒");
                return null;
            }
            
            var box = CurrentBox;
            CurrentBox = null;
            
            if (box != null)
            {
                box.SetHot(true);
                Debug.Log("铝盒从烘箱取出（高温）");
            }
            
            return box;
        }

        public void StartDrying()
        {
            if (CurrentBox == null)
            {
                Debug.LogWarning("烘箱内没有铝盒");
                return;
            }
            
            if (IsOpen)
            {
                Debug.LogWarning("请先关闭烘箱门");
                return;
            }
            
            IsDrying = true;
            Debug.Log("开始烘干...");
        }

        public void CompleteDrying()
        {
            IsDrying = false;
            
            if (CurrentBox != null)
            {
                CurrentBox.SetDried(true);
                Debug.Log("烘干完成");
            }
        }

        public void Reset()
        {
            // 关闭门，恢复初始位置
            if (Door != null)
            {
                IsOpen = false;
                Door.position = doorClosedPosition;
                Door.rotation = doorClosedRotation;
            }
            
            IsDrying = false;
            CurrentBox = null;
        }
    }
}
