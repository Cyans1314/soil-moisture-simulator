/*
 * 文件名：Soil.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：土样脚本，标识土样类型。
 * 提供土样类型和取土位置信息。
 */

using UnityEngine;
using SoilMoistureExperiment.Core;

namespace SoilMoistureExperiment.Objects
{
    public class Soil : MonoBehaviour
    {
        [Header("Settings")]
        public SoilType Type = SoilType.Dry;

        [Header("References")]
        public Transform ScoopPosition;

        public SoilType GetSoilType()
        {
            return Type;
        }

        public Transform GetScoopPosition()
        {
            return ScoopPosition != null ? ScoopPosition : transform;
        }
    }
}
