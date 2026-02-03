/*
 * 文件名：TrashBin.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：垃圾桶脚本，用于清理废土。
 * 提供倒土位置和清理功能。
 */

using UnityEngine;

namespace SoilMoistureExperiment.Objects
{
    public class TrashBin : MonoBehaviour
    {
        [Header("References")]
        public Transform DisposePosition;

        public bool DisposeSoil(AluminumBox box)
        {
            if (box == null)
            {
                Debug.LogWarning("没有铝盒");
                return false;
            }
            
            if (!box.HasSoil)
            {
                Debug.LogWarning("铝盒内没有土样");
                return false;
            }
            
            box.RemoveSoil();
            Debug.Log("土样已倒入垃圾桶");
            return true;
        }
    }
}
