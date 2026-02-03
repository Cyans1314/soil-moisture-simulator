/*
 * 文件名：DataManager.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：数据管理器，处理实验数据的记录、计算和展示。
 * 管理两轮实验数据、计算含水率、生成记录板文本。
 */

using UnityEngine;
using System;

namespace SoilMoistureExperiment.Core
{
    [Serializable]
    public class ExperimentData
    {
        public float EmptyBoxWeight;  // m₀
        public float WetWeight;       // m₁
        public float DryWeight;       // m₂
        public float MoistureContent; // 含水率
        public bool IsComplete;
        public SoilType SoilType;     // 选择的土样类型
        public string BoxId;          // 使用的铝盒ID

        public void Reset()
        {
            EmptyBoxWeight = 0;
            WetWeight = 0;
            DryWeight = 0;
            MoistureContent = 0;
            IsComplete = false;
            SoilType = SoilType.None;
            BoxId = null;
        }
    }

    public enum WeightType
    {
        EmptyBox,  // m₀
        WetSoil,   // m₁
        DrySoil    // m₂
    }

    public class DataManager : MonoBehaviour
    {
        // 重量常量
        public const float EMPTY_BOX_WEIGHT = 25.00f;
        public const float DRY_SOIL_WEIGHT = 50.00f;
        public const float WET_SOIL_WEIGHT = 60.00f;
        public const float DRY_SOIL_MOISTURE_LOSS = 1.50f;  // 干土烘干失水
        public const float WET_SOIL_MOISTURE_LOSS = 10.00f; // 湿土烘干失水

        public ExperimentData Round1Data { get; private set; } = new ExperimentData();
        public ExperimentData Round2Data { get; private set; } = new ExperimentData();

        public event Action<ExperimentData> OnDataUpdated;

        public void ResetData()
        {
            Round1Data.Reset();
            Round2Data.Reset();
        }

        public ExperimentData GetCurrentRoundData()
        {
            return GameManager.Instance.CurrentRound == 1 ? Round1Data : Round2Data;
        }

        public void RecordWeight(WeightType type, float value)
        {
            var data = GetCurrentRoundData();

            switch (type)
            {
                case WeightType.EmptyBox:
                    data.EmptyBoxWeight = value;
                    Debug.Log($"记录空盒重量 m₀ = {value:F2}g");
                    break;
                case WeightType.WetSoil:
                    data.WetWeight = value;
                    Debug.Log($"记录湿重 m₁ = {value:F2}g");
                    break;
                case WeightType.DrySoil:
                    data.DryWeight = value;
                    Debug.Log($"记录干重 m₂ = {value:F2}g");
                    CalculateMoistureContent(data);
                    break;
            }

            OnDataUpdated?.Invoke(data);
        }

        public float CalculateMoistureContent(ExperimentData data)
        {
            if (data.DryWeight <= data.EmptyBoxWeight)
            {
                Debug.LogWarning("无效数据：干重小于等于空盒重量");
                return 0;
            }

            // w = (m₁ - m₂) / (m₂ - m₀) × 100%
            float moistureContent = (data.WetWeight - data.DryWeight) / 
                                   (data.DryWeight - data.EmptyBoxWeight) * 100f;
            
            data.MoistureContent = moistureContent;
            data.IsComplete = true;
            
            Debug.Log($"含水率 = ({data.WetWeight:F2} - {data.DryWeight:F2}) / ({data.DryWeight:F2} - {data.EmptyBoxWeight:F2}) × 100% = {moistureContent:F2}%");
            
            return moistureContent;
        }

        /// <summary>
        /// 计算铝盒当前重量（根据状态）
        /// </summary>
        public float GetBoxWeight(bool hasSoil, SoilType soilType, bool isDried)
        {
            float weight = EMPTY_BOX_WEIGHT;

            if (hasSoil)
            {
                if (soilType == SoilType.Dry)
                {
                    weight += isDried ? (DRY_SOIL_WEIGHT - DRY_SOIL_MOISTURE_LOSS) : DRY_SOIL_WEIGHT;
                }
                else if (soilType == SoilType.Wet)
                {
                    weight += isDried ? (WET_SOIL_WEIGHT - WET_SOIL_MOISTURE_LOSS) : WET_SOIL_WEIGHT;
                }
            }

            return weight;
        }

        /// <summary>
        /// 设置当前轮次的土样类型
        /// </summary>
        public void SetCurrentSoilType(SoilType type)
        {
            var data = GetCurrentRoundData();
            data.SoilType = type;
            Debug.Log($"选择土样类型：{(type == SoilType.Dry ? "干土" : "湿土")}");
        }

        /// <summary>
        /// 称重并记录（带随机波动）
        /// </summary>
        public float RecordCurrentWeight(ExperimentStep step)
        {
            var data = GetCurrentRoundData();
            float baseWeight = 0f;
            float randomOffset = UnityEngine.Random.Range(-0.5f, 0.5f);

            switch (step)
            {
                case ExperimentStep.RecordEmptyWeight:
                    baseWeight = EMPTY_BOX_WEIGHT + randomOffset;
                    data.EmptyBoxWeight = baseWeight;
                    data.BoxId = GameManager.Instance.SelectedBoxId; // 记录使用的铝盒
                    Debug.Log($"记录空盒重量 m₀ = {baseWeight:F2}g (铝盒{data.BoxId})");
                    break;

                case ExperimentStep.RecordWetWeight:
                    // 根据实际选择的土样类型计算重量
                    float soilWeight = data.SoilType == SoilType.Wet ? WET_SOIL_WEIGHT : DRY_SOIL_WEIGHT;
                    baseWeight = data.EmptyBoxWeight + soilWeight + UnityEngine.Random.Range(0f, 2f);
                    data.WetWeight = baseWeight;
                    Debug.Log($"记录湿重 m₁ = {baseWeight:F2}g");
                    break;

                case ExperimentStep.RecordDryWeight:
                    // 根据实际选择的土样类型计算失水量
                    float moistureLoss = data.SoilType == SoilType.Wet ? WET_SOIL_MOISTURE_LOSS : DRY_SOIL_MOISTURE_LOSS;
                    baseWeight = data.WetWeight - moistureLoss - UnityEngine.Random.Range(0f, 1f);
                    data.DryWeight = baseWeight;
                    Debug.Log($"记录干重 m₂ = {baseWeight:F2}g");
                    CalculateMoistureContent(data);
                    break;
            }

            OnDataUpdated?.Invoke(data);
            return baseWeight;
        }

        /// <summary>
        /// 获取记录板显示文本（按行显示）
        /// </summary>
        public string GetRecordText()
        {
            var round1 = Round1Data;
            var round2 = Round2Data;
            string text = "";

            // 第一轮数据
            string box1Id = string.IsNullOrEmpty(round1.BoxId) ? "A" : round1.BoxId;
            string soil1Name = round1.SoilType == SoilType.Wet ? "湿土" : "干土";
            if (round1.EmptyBoxWeight > 0)
                text += $"空铝罐{box1Id}：{round1.EmptyBoxWeight:F2}g\n";
            if (round1.WetWeight > 0)
                text += $"{soil1Name}罐子：{round1.WetWeight:F2}g\n";
            if (round1.DryWeight > 0)
                text += $"{soil1Name}烘干后：{round1.DryWeight:F2}g\n";
            if (round1.IsComplete)
                text += $"{soil1Name}含水率：{round1.MoistureContent:F2}%\n";

            // 第二轮数据
            string box2Id = string.IsNullOrEmpty(round2.BoxId) ? "B" : round2.BoxId;
            string soil2Name = round2.SoilType == SoilType.Wet ? "湿土" : "干土";
            if (round2.EmptyBoxWeight > 0)
                text += $"\n空铝罐{box2Id}：{round2.EmptyBoxWeight:F2}g\n";
            if (round2.WetWeight > 0)
                text += $"{soil2Name}罐子：{round2.WetWeight:F2}g\n";
            if (round2.DryWeight > 0)
                text += $"{soil2Name}烘干后：{round2.DryWeight:F2}g\n";
            if (round2.IsComplete)
                text += $"{soil2Name}含水率：{round2.MoistureContent:F2}%";

            return text;
        }
    }
}
