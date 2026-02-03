/*
 * 文件名：Spoon.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：勺子脚本，用于取土。
 * 处理选中状态、取土动画（移动、旋转、倒土）。
 */

using UnityEngine;
using System.Collections;
using SoilMoistureExperiment.Core;

namespace SoilMoistureExperiment.Objects
{
    public class Spoon : MonoBehaviour
    {
        [Header("References")]
        public Transform SpoonTip; // 勺子尖端

        [Header("Settings")]
        public float MoveSpeed = 2f;
        public float ScoopAngle = 30f; // 铲土时的旋转角度
        public int ScoopCount = 3; // 铲土次数

        // State
        public bool IsSelected { get; private set; }
        public bool IsAnimating { get; private set; }

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        public void Select()
        {
            IsSelected = true;
            Debug.Log("勺子已选中，请点击土样取土");
        }

        public void Deselect()
        {
            IsSelected = false;
        }

        /// <summary>
        /// 执行取土动画
        /// </summary>
        public void DoScoopAnimation(Transform soilPosition, Transform boxOpeningPosition, System.Action onComplete)
        {
            if (IsAnimating) return;
            StartCoroutine(ScoopAnimationCoroutine(soilPosition, boxOpeningPosition, onComplete));
        }

        private IEnumerator ScoopAnimationCoroutine(Transform soilPosition, Transform boxOpeningPosition, System.Action onComplete)
        {
            IsAnimating = true;

            Vector3 tipOffset = Vector3.zero;
            if (SpoonTip != null)
            {
                tipOffset = transform.position - SpoonTip.position;
            }

            // 重复铲土动作
            for (int i = 0; i < ScoopCount; i++)
            {
                // 1. 移动到土样位置
                yield return MoveToPosition(soilPosition.position + tipOffset);

                // 2. 铲土动作（向下旋转）
                yield return DoScoopRotation();

                // 3. 移动到铝盒口
                yield return MoveToPosition(boxOpeningPosition.position + tipOffset);

                // 4. 倒土动作（向下旋转倒出）
                yield return DoPourRotation();
            }

            // 5. 回到原位
            yield return MoveToPosition(initialPosition);
            transform.rotation = initialRotation;

            IsAnimating = false;
            onComplete?.Invoke();
        }

        private IEnumerator MoveToPosition(Vector3 targetPos)
        {
            float duration = 0.4f;
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t); // Smoothstep
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
        }

        private IEnumerator DoScoopRotation()
        {
            // 向下旋转铲土
            float duration = 0.2f;
            Quaternion startRot = transform.rotation;
            Quaternion endRot = transform.rotation * Quaternion.Euler(ScoopAngle, 0, 0);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            // 抬起
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.rotation = Quaternion.Slerp(endRot, startRot, t);
                yield return null;
            }

            transform.rotation = startRot;
        }

        private IEnumerator DoPourRotation()
        {
            // 向下旋转倒土
            float duration = 0.2f;
            Quaternion startRot = transform.rotation;
            Quaternion endRot = transform.rotation * Quaternion.Euler(ScoopAngle, 0, 0);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            // 恢复
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.rotation = Quaternion.Slerp(endRot, startRot, t);
                yield return null;
            }

            transform.rotation = startRot;
        }

        public void ResetToInitial()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            IsSelected = false;
            IsAnimating = false;
        }
    }
}
