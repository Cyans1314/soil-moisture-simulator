/*
 * 文件名：AluminumBox.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：铝盒脚本，是实验中最核心的物体。
 * 处理盖子开关、土样管理、状态追踪、位置记录、拖拽支持。
 */

using UnityEngine;
using SoilMoistureExperiment.Core;
using SoilMoistureExperiment.Interaction;
using System.Collections;

namespace SoilMoistureExperiment.Objects
{
    public class AluminumBox : MonoBehaviour, IDraggable
    {
        [Header("References")]
        public Transform Cap;
        public Transform Body;
        public Transform Center;
        public Transform CapPlacePosition;
        public Transform OpeningPosition;

        [Header("Settings")]
        public float OpenAngle = 90f;
        public float AnimationDuration = 0.3f;
        public string BoxId = "A"; // "A" 或 "B"

        // State
        public bool IsOpen { get; private set; }
        public bool HasSoil { get; private set; }
        public bool IsHot { get; private set; }
        public bool IsDried { get; private set; }
        public SoilType CurrentSoil { get; private set; } = SoilType.None;

        // IDraggable
        public bool CanDrag => true;
        public Vector3 OriginalPosition { get; private set; }

        private Vector3 capClosedPosition;
        private Quaternion capClosedRotation;
        private bool isAnimating;

        private void Awake()
        {
            OriginalPosition = transform.position;
            
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
            Debug.Log($"铝盒{BoxId}盖子{(IsOpen ? "打开" : "关闭")}");
        }

        private IEnumerator AnimateCap(bool open)
        {
            isAnimating = true;
            
            Vector3 startPos = Cap.position;
            Vector3 endPos;
            
            if (open && CapPlacePosition != null)
            {
                // 打开：移动到桌面位置
                endPos = CapPlacePosition.position;
            }
            else
            {
                // 关闭：回到铝盒上
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
            
            // 关闭时确保盖子是铝盒的子物体
            if (!open)
            {
                Cap.SetParent(transform);
                Cap.localPosition = capClosedPosition;
                Cap.localRotation = capClosedRotation;
            }
            
            isAnimating = false;
        }

        /// <summary>
        /// 把盖子从层级中移出（放入烘箱前调用）
        /// </summary>
        public void DetachCap()
        {
            if (Cap != null && Cap.parent == transform)
            {
                Cap.SetParent(null);
            }
        }

        public void AddSoil(SoilType type)
        {
            if (!IsOpen)
            {
                Debug.LogWarning("铝盒盖子未打开，无法添加土样");
                return;
            }
            
            HasSoil = true;
            CurrentSoil = type;
            IsDried = false;
            Debug.Log($"铝盒{BoxId}添加{(type == SoilType.Dry ? "干土" : "湿土")}");
        }

        public void RemoveSoil()
        {
            HasSoil = false;
            CurrentSoil = SoilType.None;
            IsDried = false;
            Debug.Log($"铝盒{BoxId}土样已清空");
        }

        public void SetHot(bool hot)
        {
            IsHot = hot;
        }

        public void SetDried(bool dried)
        {
            IsDried = dried;
            if (dried)
            {
                IsHot = true;
            }
        }

        public float GetWeight()
        {
            return GameManager.Instance.DataManager.GetBoxWeight(HasSoil, CurrentSoil, IsDried);
        }

        // IDraggable Implementation
        public void OnDragStart()
        {
            OriginalPosition = transform.position;
            Debug.Log($"[拖拽开始] 位置: {OriginalPosition}, 缩放: {transform.localScale}");
        }

        public void OnDrag(Vector3 worldPosition)
        {
            // 安全检查：如果新位置和当前位置差距太大，可能有问题
            float distance = Vector3.Distance(transform.position, worldPosition);
            if (distance > 100f)
            {
                Debug.LogWarning($"[拖拽] 位置变化过大，忽略: {distance}");
                return;
            }
            
            transform.position = worldPosition;
        }

        public void OnDragEnd(bool success)
        {
            Debug.Log($"[拖拽结束] success: {success}, 位置: {transform.position}, 缩放: {transform.localScale}");
            if (!success)
            {
                ResetPosition();
            }
        }

        public void ResetPosition()
        {
            transform.position = OriginalPosition;
            Debug.Log($"[重置位置] 位置: {OriginalPosition}");
        }

        public void SetPosition(Vector3 position)
        {
            Debug.Log($"[设置位置] 从 {transform.position} 到 {position}");
            transform.position = position;
            OriginalPosition = position;
        }

        private Vector3 initialPosition;
        private Vector3 initialCenterPosition; // Center子物体的初始位置
        private bool initialIsOpen;

        private void Start()
        {
            initialPosition = transform.position;
            initialIsOpen = IsOpen;
            
            // 记录Center的初始位置
            if (Center != null)
                initialCenterPosition = Center.position;
            else
                initialCenterPosition = transform.position;
        }

        /// <summary>
        /// 获取Center子物体的初始位置（用于放回托盘）
        /// </summary>
        public Vector3 GetInitialCenterPosition()
        {
            return initialCenterPosition;
        }

        public void ResetToInitial()
        {
            transform.position = initialPosition;
            OriginalPosition = initialPosition;
            HasSoil = false;
            CurrentSoil = SoilType.None;
            IsHot = false;
            IsDried = false;
            
            if (Cap != null)
            {
                Cap.SetParent(transform);
                Cap.localPosition = capClosedPosition;
                Cap.localRotation = capClosedRotation;
                IsOpen = false;
            }
        }
    }
}
