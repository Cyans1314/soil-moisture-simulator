/*
 * 文件名：GameManager.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：游戏管理器，采用单例模式，是整个系统的中枢。
 * 负责管理实验状态、协调各个管理器、控制实验流程。
 */

using UnityEngine;

namespace SoilMoistureExperiment.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        public StepManager StepManager;
        public DataManager DataManager;

        // Game State
        public int CurrentRound { get; private set; } = 1;
        public bool IsExperimentRunning { get; private set; }
        public string SelectedBoxId { get; private set; } = null; // 当前选择的铝盒ID

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            if (StepManager == null)
                StepManager = GetComponent<StepManager>();
            if (DataManager == null)
                DataManager = GetComponent<DataManager>();
        }

        public void StartExperiment()
        {
            CurrentRound = 1;
            IsExperimentRunning = true;
            SelectedBoxId = null; // 重置选择
            DataManager.ResetData();
            StepManager.StartExperiment();
            Debug.Log("实验开始 - 请选择铝盒");
        }

        /// <summary>
        /// 选择铝盒（第一步时调用）
        /// </summary>
        public void SelectBox(string boxId)
        {
            SelectedBoxId = boxId;
            Debug.Log($"选择铝盒：{boxId}");
        }

        public void CompleteCurrentStep()
        {
            StepManager.AdvanceStep();
        }

        public void NextRound()
        {
            if (CurrentRound == 1)
            {
                CurrentRound = 2;
                SelectedBoxId = null; // 重置选择，让玩家选另一个铝盒
                StepManager.StartExperiment();
                Debug.Log("开始第二轮 - 请选择铝盒");
            }
            else
            {
                EndExperiment();
            }
        }

        public void EndExperiment()
        {
            IsExperimentRunning = false;
            Debug.Log("实验结束");
            // UIManager will show results
        }

        public void ResetExperiment()
        {
            IsExperimentRunning = false;
            CurrentRound = 1;
            SelectedBoxId = null;
            
            // 重置步骤
            StepManager.SetStep(ExperimentStep.NotStarted);
            
            // 重置数据
            DataManager.ResetData();
            
            // 重置所有物品位置和状态
            ResetAllObjects();
            
            Debug.Log("实验重置");
        }

        private void ResetAllObjects()
        {
            // 重置铝盒
            var boxes = FindObjectsOfType<Objects.AluminumBox>();
            foreach (var box in boxes)
            {
                box.ResetToInitial();
            }
            
            // 重置烘箱
            var oven = FindObjectOfType<Objects.Oven>();
            if (oven != null)
                oven.Reset();
            
            // 重置干燥器
            var desiccator = FindObjectOfType<Objects.Desiccator>();
            if (desiccator != null)
                desiccator.Reset();
            
            // 重置天平
            var balance = FindObjectOfType<Objects.Balance>();
            if (balance != null)
                balance.Reset();
        }

        public SoilType GetCurrentSoilType()
        {
            return CurrentRound == 1 ? SoilType.Dry : SoilType.Wet;
        }
    }

    public enum SoilType
    {
        None,
        Dry,
        Wet
    }
}
