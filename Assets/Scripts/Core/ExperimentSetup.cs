using UnityEngine;
using SoilMoistureExperiment.Objects;
using SoilMoistureExperiment.Interaction;
using SoilMoistureExperiment.UI;
using SoilMoistureExperiment.CameraSystem;

namespace SoilMoistureExperiment.Core
{
    /// <summary>
    /// 实验场景初始化脚本
    /// 挂载到一个空物体上，用于管理所有引用
    /// </summary>
    public class ExperimentSetup : MonoBehaviour
    {
        [Header("=== 场景物体引用 ===")]
        
        [Header("铝盒")]
        public AluminumBox AluminumBoxA;
        public AluminumBox AluminumBoxB;

        [Header("设备")]
        public Oven Oven;
        public Desiccator Desiccator;
        public Balance Balance;
        public TrashBin TrashBin;

        [Header("工具")]
        public Spoon Spoon;

        [Header("土样")]
        public Soil DrySoil;
        public Soil WetSoil;

        [Header("=== 系统组件 ===")]
        public GameManager GameManager;
        public InteractionController InteractionController;
        public HighlightSystem HighlightSystem;
        public UIManager UIManager;
        public CameraController CameraController;

        private void Awake()
        {
            ValidateReferences();
            SetupComponents();
        }

        private void ValidateReferences()
        {
            if (AluminumBoxA == null) Debug.LogError("缺少引用: AluminumBoxA");
            if (AluminumBoxB == null) Debug.LogError("缺少引用: AluminumBoxB");
            if (Oven == null) Debug.LogError("缺少引用: Oven");
            if (Desiccator == null) Debug.LogError("缺少引用: Desiccator");
            if (Balance == null) Debug.LogError("缺少引用: Balance");
            if (Spoon == null) Debug.LogError("缺少引用: Spoon");
            if (DrySoil == null) Debug.LogError("缺少引用: DrySoil");
            if (WetSoil == null) Debug.LogError("缺少引用: WetSoil");
        }

        private void SetupComponents()
        {
            // 设置铝盒ID
            if (AluminumBoxA != null) AluminumBoxA.BoxId = "A";
            if (AluminumBoxB != null) AluminumBoxB.BoxId = "B";

            // 设置土样类型
            if (DrySoil != null) DrySoil.Type = SoilType.Dry;
            if (WetSoil != null) WetSoil.Type = SoilType.Wet;

            // 设置高亮系统引用
            if (HighlightSystem != null)
            {
                HighlightSystem.AluminumBoxA = AluminumBoxA;
                HighlightSystem.AluminumBoxB = AluminumBoxB;
                HighlightSystem.Oven = Oven;
                HighlightSystem.Desiccator = Desiccator;
                HighlightSystem.Spoon = Spoon;
            }
        }

        [ContextMenu("自动查找场景引用")]
        public void AutoFindReferences()
        {
            // 查找铝盒
            AluminumBox[] boxes = FindObjectsOfType<AluminumBox>();
            foreach (var box in boxes)
            {
                if (box.BoxId == "A" || box.name.Contains("A"))
                    AluminumBoxA = box;
                else if (box.BoxId == "B" || box.name.Contains("B"))
                    AluminumBoxB = box;
            }

            // 查找其他组件
            Oven = FindObjectOfType<Oven>();
            Desiccator = FindObjectOfType<Desiccator>();
            Balance = FindObjectOfType<Balance>();
            TrashBin = FindObjectOfType<TrashBin>();
            Spoon = FindObjectOfType<Spoon>();

            // 查找土样
            Soil[] soils = FindObjectsOfType<Soil>();
            foreach (var soil in soils)
            {
                if (soil.Type == SoilType.Dry || soil.name.Contains("干"))
                    DrySoil = soil;
                else if (soil.Type == SoilType.Wet || soil.name.Contains("湿"))
                    WetSoil = soil;
            }

            // 查找系统组件
            GameManager = FindObjectOfType<GameManager>();
            InteractionController = FindObjectOfType<InteractionController>();
            HighlightSystem = FindObjectOfType<HighlightSystem>();
            UIManager = FindObjectOfType<UIManager>();
            CameraController = FindObjectOfType<CameraController>();

            Debug.Log("自动查找完成，请检查引用是否正确");
        }
    }
}
