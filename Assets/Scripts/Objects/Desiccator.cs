/*
 * 文件名：Desiccator.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：干燥器脚本，用于冷却烘干后的土样。
 * 处理盖子开关、放入取出铝盒、冷却状态管理、穿墙处理。
 */

using UnityEngine;
using System.Collections;

namespace SoilMoistureExperiment.Objects
{
    public class Desiccator : MonoBehaviour
    {
        [Header("References")]
        public Transform Cap;
        public Transform InsidePosition;
        public Transform EntryPosition;
        public Transform CapPlacePosition;

        [Header("Settings")]
        public float AnimationDuration = 0.4f;

        // State
        public bool IsOpen { get; private set; }
        public bool IsCooling { get; private set; }
        public AluminumBox CurrentBox { get; private set; }

        private Vector3 capClosedPosition;
        private Quaternion capClosedRotation;
        private bool isAnimating;

        private void Awake()
        {
            if (Cap != null)
            {
                capClosedPosition = Cap.localPosition;
                capClosedRotation = Cap.localRotation;
            }
        }

        public void ToggleCap()
        {
            if (isAnimating || Cap == null) return;
            
            IsOpen = !IsOpen;
            StartCoroutine(AnimateCap(IsOpen));
            Debug.Log($"干燥器盖{(IsOpen ? "打开" : "关闭")}");

            // 关盖且有铝盒时自动开始冷却
            if (!IsOpen && CurrentBox != null && !IsCooling)
            {
                StartCooling();
            }
        }

        private IEnumerator AnimateCap(bool open)
        {
            isAnimating = true;
            
            Vector3 startPos = Cap.position;
            Vector3 endPos;
            
            if (open && CapPlacePosition != null)
            {
                // 打开：移动到桌面位置，保持原旋转
                endPos = CapPlacePosition.position;
            }
            else
            {
                // 关闭：回到干燥器上
                endPos = transform.TransformPoint(capClosedPosition);
            }
            
            float elapsed = 0f;
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                t = t * t * (3f - 2f * t); // Smoothstep
                Cap.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            Cap.position = endPos;
            isAnimating = false;
        }

        public bool PlaceBox(AluminumBox box)
        {
            if (!IsOpen)
            {
                Debug.LogWarning("干燥器盖未打开，无法放入铝盒");
                return false;
            }
            
            if (CurrentBox != null)
            {
                Debug.LogWarning("干燥器内已有铝盒");
                return false;
            }
            
            CurrentBox = box;
            if (InsidePosition != null)
            {
                box.SetPosition(InsidePosition.position);
            }
            Debug.Log("铝盒放入干燥器");
            return true;
        }

        public AluminumBox RemoveBox()
        {
            if (!IsOpen)
            {
                Debug.LogWarning("干燥器盖未打开，无法取出铝盒");
                return null;
            }
            
            var box = CurrentBox;
            CurrentBox = null;
            
            if (box != null)
            {
                box.SetHot(false);
                Debug.Log("铝盒从干燥器取出（已冷却）");
            }
            
            return box;
        }

        public void StartCooling()
        {
            if (CurrentBox == null)
            {
                Debug.LogWarning("干燥器内没有铝盒");
                return;
            }
            
            if (IsOpen)
            {
                Debug.LogWarning("请先关闭干燥器盖");
                return;
            }
            
            IsCooling = true;
            Debug.Log("开始冷却...");
        }

        public void CompleteCooling()
        {
            IsCooling = false;
            
            if (CurrentBox != null)
            {
                CurrentBox.SetHot(false);
                Debug.Log("冷却完成");
            }
        }

        public void Reset()
        {
            if (IsOpen && Cap != null)
            {
                IsOpen = false;
                Cap.localPosition = capClosedPosition;
                Cap.localRotation = capClosedRotation;
            }
            
            IsCooling = false;
            CurrentBox = null;
        }
    }
}
