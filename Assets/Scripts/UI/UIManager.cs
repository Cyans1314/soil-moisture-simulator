/*
 * 文件名：UIManager.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：UI管理器，处理所有界面交互。
 * 管理按钮、弹窗、记录板、步骤提示、清理动画。
 */

using UnityEngine;
using UnityEngine.UI;

namespace SoilMoistureExperiment.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Buttons")]
        public Button ManualButton;
        public Button StartButton;
        public Button ExitButton;
        public Button WeighButton;
        public Button HeatButton;
        public Button CoolButton;
        public Button CleanButton;

        [Header("Panels")]
        public GameObject ManualWindow;

        [Header("Texts")]
        public Text RecordText;
        public Text StepHintText;

        private bool isExperimentMode = false;
        private Color originalButtonColor;

        private void Start()
        {
            // 绑定按钮事件
            if (ManualButton != null)
                ManualButton.onClick.AddListener(OpenManual);

            if (ExitButton != null)
                ExitButton.onClick.AddListener(CloseManual);

            if (StartButton != null)
            {
                StartButton.onClick.AddListener(ToggleExperiment);
                var img = StartButton.GetComponent<Image>();
                if (img != null)
                    originalButtonColor = img.color;
            }

            // 默认隐藏手册弹窗
            if (ManualWindow != null)
                ManualWindow.SetActive(false);

            // 默认隐藏步骤提示
            if (StepHintText != null)
                StepHintText.gameObject.SetActive(false);

            // 订阅步骤变化事件
            var stepManager = Core.GameManager.Instance?.StepManager;
            if (stepManager != null)
            {
                stepManager.OnInstructionChanged += UpdateStepHint;
            }

            // 绑定称重和烘烤按钮
            if (WeighButton != null)
                WeighButton.onClick.AddListener(OnWeighClick);
            if (HeatButton != null)
                HeatButton.onClick.AddListener(OnHeatClick);
            if (CoolButton != null)
                CoolButton.onClick.AddListener(OnCoolClick);
            if (CleanButton != null)
                CleanButton.onClick.AddListener(OnCleanClick);
        }

        private void OnDestroy()
        {
            var stepManager = Core.GameManager.Instance?.StepManager;
            if (stepManager != null)
            {
                stepManager.OnInstructionChanged -= UpdateStepHint;
            }
        }

        public void OpenManual()
        {
            if (ManualWindow != null)
                ManualWindow.SetActive(true);
            
            // 禁用相机缩放
            var camera = FindObjectOfType<CameraSystem.CameraController>();
            if (camera != null)
                camera.EnableZoom = false;
            
            // 按钮点击反馈
            StartCoroutine(ButtonClickEffect(ManualButton));
        }

        public void CloseManual()
        {
            if (ManualWindow != null)
                ManualWindow.SetActive(false);
            
            // 恢复相机缩放
            var camera = FindObjectOfType<CameraSystem.CameraController>();
            if (camera != null)
                camera.EnableZoom = true;
            
            StartCoroutine(ButtonClickEffect(ExitButton));
        }

        public void ToggleExperiment()
        {
            isExperimentMode = !isExperimentMode;

            var img = StartButton.GetComponent<Image>();

            if (isExperimentMode)
            {
                // 进入实验模式 - 绿色
                if (img != null)
                    img.color = new Color(0.6f, 1f, 0.6f);

                if (StepHintText != null)
                    StepHintText.gameObject.SetActive(true);

                // 相机回到起始位置
                var camera = FindObjectOfType<CameraSystem.CameraController>();
                if (camera != null)
                    camera.ResetToInitial();

                Core.GameManager.Instance.StartExperiment();
            }
            else
            {
                // 退出实验模式 - 恢复原色
                if (img != null)
                    img.color = originalButtonColor;

                if (StepHintText != null)
                    StepHintText.gameObject.SetActive(false);

                // 重置一切
                Core.GameManager.Instance.ResetExperiment();
                
                // 清空记录板内容
                if (RecordText != null)
                    RecordText.text = "";
            }
        }

        public void UpdateStepHint(string hint)
        {
            if (StepHintText != null)
                StepHintText.text = hint;
        }

        public void UpdateRecord(string record)
        {
            if (RecordText != null)
                RecordText.text = record;
        }

        public void ShowError(string message)
        {
            Debug.LogWarning(message);
        }

        public void OnWeighClick()
        {
            var stepManager = Core.GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            if (currentStep != Core.ExperimentStep.RecordEmptyWeight &&
                currentStep != Core.ExperimentStep.RecordWetWeight &&
                currentStep != Core.ExperimentStep.RecordDryWeight)
            {
                ShowError("当前步骤不需要称重");
                return;
            }

            // 按钮点击反馈
            StartCoroutine(ButtonClickEffect(WeighButton));

            // 获取重量并记录
            var dataManager = Core.GameManager.Instance.DataManager;
            float weight = dataManager.RecordCurrentWeight(currentStep);

            // 更新记录板
            UpdateRecord(dataManager.GetRecordText());

            // 显示称重结果，延时后进入下一步
            StartCoroutine(ShowWeightAndAdvance(weight, currentStep, stepManager));
        }

        private System.Collections.IEnumerator ShowWeightAndAdvance(float weight, Core.ExperimentStep step, Core.StepManager stepManager)
        {
            string weightName = "";
            var data = Core.GameManager.Instance.DataManager.GetCurrentRoundData();
            string soilName = data.SoilType == Core.SoilType.Wet ? "湿土" : "干土";
            
            switch (step)
            {
                case Core.ExperimentStep.RecordEmptyWeight:
                    weightName = "空盒重量";
                    break;
                case Core.ExperimentStep.RecordWetWeight:
                    weightName = $"{soilName}重量";
                    break;
                case Core.ExperimentStep.RecordDryWeight:
                    weightName = $"{soilName}烘干后重量";
                    break;
            }

            // 显示称重结果
            UpdateStepHint($"{weightName}：{weight:F2}g");

            if (step == Core.ExperimentStep.RecordDryWeight)
            {
                yield return new WaitForSeconds(2f);
                UpdateStepHint($"含水率：{data.MoistureContent:F2}%");
                // 再次更新记录板，确保含水率显示
                UpdateRecord(Core.GameManager.Instance.DataManager.GetRecordText());
            }

            yield return new WaitForSeconds(2f);

            // 步骤前进
            stepManager.AdvanceStep();
        }

        public void OnHeatClick()
        {
            var stepManager = Core.GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            // 只处理烘干步骤
            if (currentStep != Core.ExperimentStep.WaitForDrying)
            {
                ShowError("当前步骤不需要烘烤");
                return;
            }

            // 按钮点击反馈
            StartCoroutine(ButtonClickEffect(HeatButton));

            var oven = FindObjectOfType<Objects.Oven>();
            if (oven != null)
                oven.CompleteDrying();

            // 步骤前进
            stepManager.AdvanceStep();
        }

        public void OnCoolClick()
        {
            var stepManager = Core.GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            // 只处理冷却步骤
            if (currentStep != Core.ExperimentStep.WaitForCooling)
            {
                ShowError("当前步骤不需要冷却");
                return;
            }

            // 按钮点击反馈
            StartCoroutine(ButtonClickEffect(CoolButton));

            var desiccator = FindObjectOfType<Objects.Desiccator>();
            if (desiccator != null)
                desiccator.CompleteCooling();

            // 步骤前进
            stepManager.AdvanceStep();
        }

        public void OnCleanClick()
        {
            var stepManager = Core.GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            // 只处理清理废土步骤
            if (currentStep != Core.ExperimentStep.DisposeToTrash)
            {
                ShowError("当前步骤不需要清理");
                return;
            }

            // 按钮点击反馈
            StartCoroutine(ButtonClickEffect(CleanButton));

            // 找到当前铝盒
            string boxId = Core.GameManager.Instance.SelectedBoxId;
            if (string.IsNullOrEmpty(boxId))
            {
                ShowError("找不到当前使用的铝盒");
                return;
            }

            Objects.AluminumBox[] boxes = FindObjectsOfType<Objects.AluminumBox>();
            Objects.AluminumBox targetBox = null;
            foreach (var box in boxes)
            {
                if (box.BoxId == boxId)
                {
                    targetBox = box;
                    break;
                }
            }

            if (targetBox == null)
            {
                ShowError("找不到铝盒");
                return;
            }

            // 执行清理动画
            StartCoroutine(CleanTrashAnimation(targetBox));
        }

        private System.Collections.IEnumerator CleanTrashAnimation(Objects.AluminumBox box)
        {
            var trashBin = FindObjectOfType<Objects.TrashBin>();
            if (trashBin == null)
            {
                Debug.LogError("找不到垃圾桶");
                yield break;
            }

            Vector3 originalPos = box.transform.position;
            Vector3 disposeTarget = trashBin.DisposePosition != null 
                ? trashBin.DisposePosition.position 
                : trashBin.transform.position + Vector3.up * 0.3f;

            Vector3 offset = box.Center != null ? box.transform.position - box.Center.position : Vector3.zero;
            Vector3 trashPos = disposeTarget + offset;

            // 1. 开盖
            if (!box.IsOpen)
            {
                box.ToggleCap();
                yield return new WaitForSeconds(0.4f);
            }

            // 把盖子移出层级
            box.DetachCap();

            // 2. 飞到垃圾桶上方
            yield return MoveBoxToPosition(box, trashPos);

            // 3. 倒土动作（旋转）
            yield return RotateBoxToPour(box);

            // 清空土样
            box.RemoveSoil();

            // 4. 恢复旋转
            yield return RotateBoxBack(box);

            // 5. 飞回原位
            yield return MoveBoxToPosition(box, originalPos);

            // 6. 盖盖
            box.ToggleCap();
            yield return new WaitForSeconds(0.4f);

            // 步骤前进到 RoundComplete
            Core.GameManager.Instance.StepManager.AdvanceStep();

            // 显示实验完成，延时2秒后结束实验
            UpdateStepHint("实验完成！");
            yield return new WaitForSeconds(2f);

            // 结束实验模式
            EndExperiment();
        }

        private System.Collections.IEnumerator MoveBoxToPosition(Objects.AluminumBox box, Vector3 targetPos)
        {
            Vector3 startPos = box.transform.position;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);
                box.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            box.transform.position = targetPos;
        }

        private System.Collections.IEnumerator RotateBoxToPour(Objects.AluminumBox box)
        {
            Quaternion startRot = box.transform.rotation;
            Quaternion endRot = box.transform.rotation * Quaternion.Euler(120f, 0f, 0f);
            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                box.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            box.transform.rotation = endRot;
            yield return new WaitForSeconds(0.3f);
        }

        private System.Collections.IEnumerator RotateBoxBack(Objects.AluminumBox box)
        {
            Quaternion startRot = box.transform.rotation;
            Quaternion endRot = Quaternion.identity;
            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                box.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            box.transform.rotation = endRot;
        }

        private void EndExperiment()
        {
            isExperimentMode = false;

            var img = StartButton.GetComponent<Image>();
            if (img != null)
                img.color = originalButtonColor;

            if (StepHintText != null)
                StepHintText.gameObject.SetActive(false);

            // 重置实验数据但不重置物品位置
            Core.GameManager.Instance.DataManager.ResetData();
            
            // 清空记录板
            if (RecordText != null)
                RecordText.text = "";
        }

        private System.Collections.IEnumerator ButtonClickEffect(Button button)
        {
            if (button == null) yield break;

            var img = button.GetComponent<Image>();
            if (img == null) yield break;

            Color originalColor = img.color;
            
            // 变亮
            img.color = new Color(
                Mathf.Min(originalColor.r + 0.3f, 1f),
                Mathf.Min(originalColor.g + 0.3f, 1f),
                Mathf.Min(originalColor.b + 0.3f, 1f),
                originalColor.a
            );

            yield return new WaitForSeconds(0.1f);

            // 恢复
            img.color = originalColor;
        }
    }
}
