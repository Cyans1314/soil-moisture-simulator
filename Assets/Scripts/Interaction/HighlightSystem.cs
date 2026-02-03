/*
 * 文件名：HighlightSystem.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：高亮系统（预留功能）。
 * 用于显示当前步骤需要点击的物体轮廓。
 */

using UnityEngine;
using SoilMoistureExperiment.Core;
using SoilMoistureExperiment.Objects;

namespace SoilMoistureExperiment.Interaction
{
    // 高亮系统 - 暂时禁用，只保留引用
    public class HighlightSystem : MonoBehaviour
    {
        [Header("引用")]
        public AluminumBox AluminumBoxA;
        public AluminumBox AluminumBoxB;
        public Oven Oven;
        public Desiccator Desiccator;
        public Spoon Spoon;
        public Camera MainCamera;
        public LayerMask InteractableLayer;

        // 不做任何事情
    }
}
