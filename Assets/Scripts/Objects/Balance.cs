/*
 * 文件名：Balance.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：天平脚本，用于称重。
 * 管理放置的铝盒、获取重量、触发重量变化事件。
 */

using UnityEngine;
using System;

namespace SoilMoistureExperiment.Objects
{
    public class Balance : MonoBehaviour
    {
        [Header("References")]
        public Transform WeighingPosition;

        // State
        public AluminumBox CurrentBox { get; private set; }

        public event Action<float> OnWeightChanged;

        public bool PlaceBox(AluminumBox box)
        {
            if (CurrentBox != null)
            {
                Debug.LogWarning("天平上已有铝盒");
                return false;
            }
            
            CurrentBox = box;
            // 位置已经在移动动画中设置好了，这里不再重复设置
            
            float weight = GetWeight();
            OnWeightChanged?.Invoke(weight);
            Debug.Log($"铝盒放到天平上，重量: {weight:F2}g");
            return true;
        }

        public AluminumBox RemoveBox()
        {
            var box = CurrentBox;
            CurrentBox = null;
            
            OnWeightChanged?.Invoke(0);
            
            if (box != null)
            {
                Debug.Log("铝盒从天平取下");
            }
            
            return box;
        }

        public float GetWeight()
        {
            if (CurrentBox == null)
            {
                return 0f;
            }
            
            return CurrentBox.GetWeight();
        }

        public bool HasBox()
        {
            return CurrentBox != null;
        }

        public void Reset()
        {
            CurrentBox = null;
            OnWeightChanged?.Invoke(0);
        }
    }
}
