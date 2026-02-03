/*
 * 文件名：StepManager.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：步骤管理器，控制实验流程的推进。
 * 定义所有实验步骤、管理当前步骤状态、验证玩家操作。
 */

using UnityEngine;
using System;

namespace SoilMoistureExperiment.Core
{
    public enum ExperimentStep
    {
        NotStarted,
        PlaceEmptyBoxOnBalance,    // 放空铝盒到天平
        RecordEmptyWeight,         // 记录空盒重量 m₀
        ReturnBoxToTray,           // 称完空盒重量后放回托盘
        OpenBoxCap,                // 打开铝盒盖
        TakeSoil,                  // 取土样
        CloseBoxCap,               // 盖上铝盒盖
        PlaceWetBoxOnBalance,      // 放湿土铝盒到天平
        RecordWetWeight,           // 记录湿重 m₁
        ReturnBoxToTrayAfterWet,   // 称完湿土重量后放回托盘
        OpenBoxCapForDrying,       // 打开盖子准备烘干
        OpenOvenDoor,              // 打开烘箱门
        PlaceBoxInOven,            // 放铝盒入烘箱
        CloseOvenDoor,             // 关烘箱门
        WaitForDrying,             // 等待烘干（可加速）
        OpenOvenDoorAfterDry,      // 打开烘箱门取出
        RemoveBoxFromOven,         // 取出铝盒
        OpenDesiccatorCap,         // 打开干燥器盖
        PlaceBoxInDesiccator,      // 放入干燥器
        CloseDesiccatorCap,        // 关干燥器盖
        WaitForCooling,            // 等待冷却（可加速）
        OpenDesiccatorCapAfterCool,// 打开干燥器取出
        RemoveBoxFromDesiccator,   // 取出铝盒
        CloseBoxCapAfterDry,       // 盖上盖子
        PlaceBoxOnBalanceForDry,   // 放到天平称干重
        RecordDryWeight,           // 记录干重 m₂
        ReturnBoxAfterDryWeight,   // 称完干重后放回托盘
        DisposeToTrash,            // 点击【清理】按钮清理废土
        RoundComplete              // 本轮完成
    }

    public enum ActionType
    {
        Click,
        Drag,
        Drop
    }

    public class StepManager : MonoBehaviour
    {
        public ExperimentStep CurrentStep { get; private set; } = ExperimentStep.NotStarted;
        
        public event Action<ExperimentStep> OnStepChanged;
        public event Action<string> OnInstructionChanged;

        private static readonly string[] StepInstructions = new string[]
        {
            "点击开始实验",                           // NotStarted
            "点击铝盒，放到天平上",                   // PlaceEmptyBoxOnBalance
            "点击【称重】按钮记录空盒重量",           // RecordEmptyWeight
            "点击铝盒，放回托盘",                     // ReturnBoxToTray
            "点击铝盒盖打开",                         // OpenBoxCap
            "点击勺子，然后点击土样取土",             // TakeSoil
            "点击铝盒盖关闭",                         // CloseBoxCap
            "点击铝盒，放到天平上",                   // PlaceWetBoxOnBalance
            "点击【称重】按钮记录土样重量",           // RecordWetWeight
            "点击铝盒，放回托盘",                     // ReturnBoxToTrayAfterWet
            "点击铝盒盖打开（烘干需开盖）",           // OpenBoxCapForDrying
            "点击烘箱门打开",                         // OpenOvenDoor
            "点击铝盒，放入烘箱",                     // PlaceBoxInOven
            "点击烘箱门关闭",                         // CloseOvenDoor
            "点击【烘烤】按钮跳过烘干等待",           // WaitForDrying
            "点击烘箱门打开",                         // OpenOvenDoorAfterDry
            "点击铝盒，从烘箱中取出",                 // RemoveBoxFromOven
            "点击干燥器盖打开",                       // OpenDesiccatorCap
            "点击铝盒，放入干燥器",                   // PlaceBoxInDesiccator
            "点击干燥器盖关闭",                       // CloseDesiccatorCap
            "点击【冷却】按钮跳过冷却等待",           // WaitForCooling
            "点击干燥器盖打开",                       // OpenDesiccatorCapAfterCool
            "点击铝盒，从干燥器中取出",               // RemoveBoxFromDesiccator
            "点击铝盒盖关闭",                         // CloseBoxCapAfterDry
            "点击铝盒，放到天平上",                   // PlaceBoxOnBalanceForDry
            "点击【称重】按钮记录干土重量",           // RecordDryWeight
            "点击铝盒，放回托盘",                     // ReturnBoxAfterDryWeight
            "点击【清理】按钮清理废土",               // DisposeToTrash
            "本轮实验完成"                            // RoundComplete
        };

        public void StartExperiment()
        {
            SetStep(ExperimentStep.PlaceEmptyBoxOnBalance);
        }

        public void AdvanceStep()
        {
            if (CurrentStep == ExperimentStep.RoundComplete)
            {
                GameManager.Instance.NextRound();
                return;
            }

            ExperimentStep nextStep = CurrentStep + 1;
            SetStep(nextStep);
        }

        public void SetStep(ExperimentStep step)
        {
            CurrentStep = step;
            string instruction = GetStepInstruction();
            
            Debug.Log($"步骤变更: {step} - {instruction}");
            
            OnStepChanged?.Invoke(step);
            OnInstructionChanged?.Invoke(instruction);
        }

        public string GetStepInstruction()
        {
            int index = (int)CurrentStep;
            if (index >= 0 && index < StepInstructions.Length)
            {
                return StepInstructions[index];
            }
            return "";
        }

        /// <summary>
        /// 验证当前操作是否符合当前步骤
        /// </summary>
        public bool ValidateAction(string objectTag, ActionType action)
        {
            switch (CurrentStep)
            {
                case ExperimentStep.PlaceEmptyBoxOnBalance:
                case ExperimentStep.PlaceWetBoxOnBalance:
                    return objectTag == "AluminumBox" && action == ActionType.Drag;
                
                case ExperimentStep.RecordEmptyWeight:
                    return objectTag == "RecordButton" && action == ActionType.Click;
                
                case ExperimentStep.ReturnBoxToTray:
                case ExperimentStep.ReturnBoxToTrayAfterWet:
                    return objectTag == "AluminumBox" && action == ActionType.Drag;
                
                case ExperimentStep.OpenBoxCap:
                case ExperimentStep.OpenBoxCapForDrying:
                    return objectTag == "AluminumBoxCap" && action == ActionType.Click;
                
                case ExperimentStep.TakeSoil:
                    return (objectTag == "Spoon" || objectTag == "Soil") && action == ActionType.Click;
                
                case ExperimentStep.CloseBoxCap:
                case ExperimentStep.CloseBoxCapAfterDry:
                    return objectTag == "AluminumBoxCap" && action == ActionType.Click;
                
                case ExperimentStep.RecordWetWeight:
                case ExperimentStep.RecordDryWeight:
                    return objectTag == "RecordButton" && action == ActionType.Click;
                
                case ExperimentStep.OpenOvenDoor:
                case ExperimentStep.OpenOvenDoorAfterDry:
                    return objectTag == "OvenDoor" && action == ActionType.Click;
                
                case ExperimentStep.PlaceBoxInOven:
                    return objectTag == "AluminumBox" && action == ActionType.Drag;
                
                case ExperimentStep.CloseOvenDoor:
                    return objectTag == "OvenDoor" && action == ActionType.Click;
                
                case ExperimentStep.WaitForDrying:
                case ExperimentStep.WaitForCooling:
                    return objectTag == "SkipButton" && action == ActionType.Click;
                
                case ExperimentStep.RemoveBoxFromOven:
                case ExperimentStep.RemoveBoxFromDesiccator:
                    return objectTag == "AluminumBox" && action == ActionType.Drag;
                
                case ExperimentStep.OpenDesiccatorCap:
                case ExperimentStep.OpenDesiccatorCapAfterCool:
                    return objectTag == "DesiccatorCap" && action == ActionType.Click;
                
                case ExperimentStep.PlaceBoxInDesiccator:
                    return objectTag == "AluminumBox" && action == ActionType.Drag;
                
                case ExperimentStep.CloseDesiccatorCap:
                    return objectTag == "DesiccatorCap" && action == ActionType.Click;
                
                case ExperimentStep.PlaceBoxOnBalanceForDry:
                    return objectTag == "AluminumBox" && action == ActionType.Drag;
                
                case ExperimentStep.ReturnBoxAfterDryWeight:
                    return objectTag == "AluminumBox" && action == ActionType.Drag;
                
                case ExperimentStep.DisposeToTrash:
                    return objectTag == "CleanButton" && action == ActionType.Click;
                
                case ExperimentStep.RoundComplete:
                    return objectTag == "NextButton" && action == ActionType.Click;
                
                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取当前步骤需要高亮的物体标签
        /// </summary>
        public string GetHighlightTarget()
        {
            switch (CurrentStep)
            {
                case ExperimentStep.PlaceEmptyBoxOnBalance:
                case ExperimentStep.PlaceWetBoxOnBalance:
                case ExperimentStep.ReturnBoxToTray:
                case ExperimentStep.ReturnBoxToTrayAfterWet:
                case ExperimentStep.PlaceBoxInOven:
                case ExperimentStep.RemoveBoxFromOven:
                case ExperimentStep.PlaceBoxInDesiccator:
                case ExperimentStep.RemoveBoxFromDesiccator:
                case ExperimentStep.PlaceBoxOnBalanceForDry:
                case ExperimentStep.ReturnBoxAfterDryWeight:
                    return "AluminumBox";
                
                case ExperimentStep.OpenBoxCap:
                case ExperimentStep.CloseBoxCap:
                case ExperimentStep.OpenBoxCapForDrying:
                case ExperimentStep.CloseBoxCapAfterDry:
                    return "AluminumBoxCap";
                
                case ExperimentStep.TakeSoil:
                    return "Spoon";
                
                case ExperimentStep.OpenOvenDoor:
                case ExperimentStep.CloseOvenDoor:
                case ExperimentStep.OpenOvenDoorAfterDry:
                    return "OvenDoor";
                
                case ExperimentStep.OpenDesiccatorCap:
                case ExperimentStep.CloseDesiccatorCap:
                case ExperimentStep.OpenDesiccatorCapAfterCool:
                    return "DesiccatorCap";
                
                default:
                    return "";
            }
        }

        /// <summary>
        /// 检查当前步骤是否需要显示记录按钮
        /// </summary>
        public bool ShouldShowRecordButton()
        {
            return CurrentStep == ExperimentStep.RecordEmptyWeight ||
                   CurrentStep == ExperimentStep.RecordWetWeight ||
                   CurrentStep == ExperimentStep.RecordDryWeight;
        }

        /// <summary>
        /// 检查当前步骤是否需要显示加速按钮
        /// </summary>
        public bool ShouldShowSkipButton()
        {
            return CurrentStep == ExperimentStep.WaitForDrying ||
                   CurrentStep == ExperimentStep.WaitForCooling;
        }
    }
}
