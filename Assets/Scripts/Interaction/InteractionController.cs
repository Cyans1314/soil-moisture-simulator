/*
 * 文件名：InteractionController.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：交互控制器，处理所有鼠标点击交互。
 * 射线检测、操作分发、物体移动动画、盖子门的开关。
 */

using UnityEngine;
using SoilMoistureExperiment.Core;
using SoilMoistureExperiment.Objects;
using System.Collections;

namespace SoilMoistureExperiment.Interaction
{
    public class InteractionController : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask InteractableLayer;
        public float MaxRayDistance = 100f;
        public float MoveAnimationDuration = 0.5f;

        [Header("References")]
        public Camera MainCamera;

        private bool isMovingObject = false;

        private void Start()
        {
            if (MainCamera == null)
            {
                MainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (isMovingObject) return;

            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        private void HandleClick()
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, MaxRayDistance, InteractableLayer))
            {
                return;
            }

            GameObject hitObject = hit.collider.gameObject;
            Debug.Log($"点击: {hitObject.name}");

            // 处理铝盒点击 - 自动移动到目标位置
            AluminumBox box = hitObject.GetComponent<AluminumBox>();
            if (box == null) box = hitObject.GetComponentInParent<AluminumBox>();
            
            if (box == null && hitObject.name.ToLower().Contains("cap"))
            {
                // 查找所有铝盒，看哪个的 Cap 是这个物体
                AluminumBox[] allBoxes = FindObjectsOfType<AluminumBox>();
                foreach (var b in allBoxes)
                {
                    if (b.Cap != null && b.Cap.gameObject == hitObject)
                    {
                        box = b;
                        break;
                    }
                }
            }
            
            if (box != null)
            {
                if (hitObject.name.ToLower().Contains("cap"))
                {
                    HandleBoxCapClick(box);
                }
                else
                {
                    // 点击铝盒本体，处理移动
                    HandleBoxClick(box);
                }
                return;
            }

            // 处理烘箱门点击
            Oven oven = hitObject.GetComponentInParent<Oven>();
            if (oven != null && hitObject.name.ToLower().Contains("door"))
            {
                HandleOvenDoorClick(oven);
                return;
            }

            // 处理干燥器盖点击
            Desiccator desiccator = hitObject.GetComponentInParent<Desiccator>();
            if (desiccator != null && hitObject.name.ToLower().Contains("cap"))
            {
                HandleDesiccatorCapClick(desiccator);
                return;
            }

            // 处理勺子点击
            Spoon spoon = hitObject.GetComponent<Spoon>();
            if (spoon == null) spoon = hitObject.GetComponentInParent<Spoon>();
            if (spoon != null)
            {
                HandleSpoonClick(spoon);
                return;
            }

            // 处理土样点击
            Soil soil = hitObject.GetComponent<Soil>();
            if (soil == null) soil = hitObject.GetComponentInParent<Soil>();
            if (soil != null)
            {
                HandleSoilClick(soil);
                return;
            }
        }

        private void HandleBoxClick(AluminumBox box)
        {
            var stepManager = GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            Vector3? targetPos = GetBoxTargetPosition(currentStep, box);
            
            if (targetPos == null)
            {
                Debug.LogWarning("当前步骤不需要移动铝盒");
                return;
            }

            // 第一步时选择铝盒
            if (currentStep == ExperimentStep.PlaceEmptyBoxOnBalance)
            {
                if (GameManager.Instance.CurrentRound == 2)
                {
                    // 第二轮必须用另一个铝盒
                    var round1Data = GameManager.Instance.DataManager.Round1Data;
                    string usedBoxId = GetBoxIdFromRound1();
                    if (box.BoxId == usedBoxId)
                    {
                        Debug.LogWarning($"铝盒{box.BoxId}已在第一轮使用，请选择另一个");
                        return;
                    }
                }
                GameManager.Instance.SelectBox(box.BoxId);
            }
            else
            {
                // 其他步骤检查是否是正确的铝盒
                string expectedBoxId = GameManager.Instance.SelectedBoxId;
                if (expectedBoxId != null && box.BoxId != expectedBoxId)
                {
                    Debug.LogWarning($"请使用铝盒{expectedBoxId}");
                    return;
                }
            }

            // 从当前容器中取出
            TryRemoveBoxFromCurrentContainer(box);

            // 开始移动动画
            StartCoroutine(MoveBoxToTarget(box, targetPos.Value, currentStep));
        }

        /// <summary>
        /// 获取第一轮使用的铝盒ID
        /// </summary>
        private string GetBoxIdFromRound1()
        {
            // 遍历所有铝盒，找到第一轮用的那个
            AluminumBox[] boxes = FindObjectsOfType<AluminumBox>();
            foreach (var box in boxes)
            {
                if (box.HasSoil)
                {
                    return box.BoxId;
                }
            }
            return null;
        }

        private Vector3? GetBoxTargetPosition(ExperimentStep step, AluminumBox box)
        {
            switch (step)
            {
                case ExperimentStep.PlaceEmptyBoxOnBalance:
                case ExperimentStep.PlaceWetBoxOnBalance:
                case ExperimentStep.PlaceBoxOnBalanceForDry:
                    Balance balance = FindObjectOfType<Balance>();
                    if (balance != null && balance.WeighingPosition != null)
                    {
                        return balance.WeighingPosition.position;
                    }
                    break;

                case ExperimentStep.ReturnBoxToTray:
                case ExperimentStep.ReturnBoxToTrayAfterWet:
                    // 放回托盘，使用Center的初始位置
                    return box.GetInitialCenterPosition();

                case ExperimentStep.PlaceBoxInOven:
                    Oven oven = FindObjectOfType<Oven>();
                    if (oven != null && oven.InsidePosition != null)
                    {
                        return oven.InsidePosition.position;
                    }
                    break;

                case ExperimentStep.PlaceBoxInDesiccator:
                    Desiccator desiccator = FindObjectOfType<Desiccator>();
                    if (desiccator != null && desiccator.InsidePosition != null)
                    {
                        return desiccator.InsidePosition.position;
                    }
                    break;

                case ExperimentStep.RemoveBoxFromOven:
                case ExperimentStep.RemoveBoxFromDesiccator:
                    // 取出时移动到托盘，使用Center的初始位置
                    return box.GetInitialCenterPosition();

                case ExperimentStep.ReturnBoxAfterDryWeight:
                    // 称完干重后放回托盘
                    return box.GetInitialCenterPosition();
            }

            return null;
        }

        private void TryRemoveBoxFromCurrentContainer(AluminumBox box)
        {
            // 从天平取下
            Balance balance = FindObjectOfType<Balance>();
            if (balance != null && balance.CurrentBox == box)
            {
                balance.RemoveBox();
            }

            // 从烘箱取出
            Oven oven = FindObjectOfType<Oven>();
            if (oven != null && oven.CurrentBox == box)
            {
                oven.RemoveBox();
            }

            // 从干燥器取出
            Desiccator desiccator = FindObjectOfType<Desiccator>();
            if (desiccator != null && desiccator.CurrentBox == box)
            {
                desiccator.RemoveBox();
            }
        }

        private IEnumerator MoveBoxToTarget(AluminumBox box, Vector3 targetPos, ExperimentStep step)
        {
            isMovingObject = true;

            if (step == ExperimentStep.PlaceBoxInOven || step == ExperimentStep.PlaceBoxInDesiccator)
            {
                box.DetachCap();
            }

            Vector3 offset = Vector3.zero;
            if (box.Center != null)
            {
                offset = box.transform.position - box.Center.position;
            }

            if (step == ExperimentStep.PlaceBoxInDesiccator)
            {
                Desiccator desiccator = FindObjectOfType<Desiccator>();
                if (desiccator != null && desiccator.EntryPosition != null)
                {
                    Vector3 entryPos = desiccator.EntryPosition.position + offset;
                    yield return MoveBoxToPosition(box, entryPos);
                }
            }

            Vector3 finalTargetPos = targetPos + offset;

            yield return MoveBoxToPosition(box, finalTargetPos);

            Debug.Log($"移动完成: {box.transform.position}");

            if (step == ExperimentStep.RemoveBoxFromOven)
            {
                Oven oven = FindObjectOfType<Oven>();
                if (oven != null && oven.IsOpen)
                {
                    oven.ToggleDoor();
                }
            }
            else if (step == ExperimentStep.RemoveBoxFromDesiccator)
            {
                Desiccator desiccator = FindObjectOfType<Desiccator>();
                if (desiccator != null && desiccator.IsOpen)
                {
                    desiccator.ToggleCap();
                }
            }

            // 放置到目标容器
            PlaceBoxInContainer(box, step);

            isMovingObject = false;

            // 步骤前进
            GameManager.Instance.StepManager.AdvanceStep();
        }

        private void PlaceBoxInContainer(AluminumBox box, ExperimentStep step)
        {
            switch (step)
            {
                case ExperimentStep.PlaceEmptyBoxOnBalance:
                case ExperimentStep.PlaceBoxOnBalanceForDry:
                    Balance balance = FindObjectOfType<Balance>();
                    if (balance != null)
                    {
                        balance.PlaceBox(box);
                    }
                    break;

                case ExperimentStep.PlaceBoxInOven:
                    Oven oven = FindObjectOfType<Oven>();
                    if (oven != null)
                    {
                        oven.PlaceBox(box);
                    }
                    break;

                case ExperimentStep.PlaceBoxInDesiccator:
                    Desiccator desiccator = FindObjectOfType<Desiccator>();
                    if (desiccator != null)
                    {
                        desiccator.PlaceBox(box);
                    }
                    break;
            }
        }

        private IEnumerator MoveBoxToPosition(AluminumBox box, Vector3 targetPos)
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

        private void HandleBoxCapClick(AluminumBox box)
        {
            var stepManager = GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            // 验证是否是开/关盖步骤
            if (currentStep != ExperimentStep.OpenBoxCap &&
                currentStep != ExperimentStep.CloseBoxCap &&
                currentStep != ExperimentStep.OpenBoxCapForDrying &&
                currentStep != ExperimentStep.CloseBoxCapAfterDry)
            {
                ShowError("当前步骤不需要操作铝盒盖");
                return;
            }

            box.ToggleCap();
            stepManager.AdvanceStep();
        }

        private void HandleOvenDoorClick(Oven oven)
        {
            var stepManager = GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            if (currentStep != ExperimentStep.OpenOvenDoor &&
                currentStep != ExperimentStep.CloseOvenDoor &&
                currentStep != ExperimentStep.OpenOvenDoorAfterDry)
            {
                ShowError("当前步骤不需要操作烘箱门");
                return;
            }

            oven.ToggleDoor();
            stepManager.AdvanceStep();
        }

        private void HandleDesiccatorCapClick(Desiccator desiccator)
        {
            var stepManager = GameManager.Instance.StepManager;
            var currentStep = stepManager.CurrentStep;

            if (currentStep != ExperimentStep.OpenDesiccatorCap &&
                currentStep != ExperimentStep.CloseDesiccatorCap &&
                currentStep != ExperimentStep.OpenDesiccatorCapAfterCool)
            {
                ShowError("当前步骤不需要操作干燥器盖");
                return;
            }

            desiccator.ToggleCap();
            
            if (currentStep == ExperimentStep.OpenDesiccatorCapAfterCool && desiccator.CurrentBox != null)
            {
                StartCoroutine(AutoRemoveBoxFromDesiccator(desiccator));
            }
            else
            {
                stepManager.AdvanceStep();
            }
        }

        private IEnumerator AutoRemoveBoxFromDesiccator(Desiccator desiccator)
        {
            // 等待开盖动画完成
            yield return new WaitForSeconds(0.5f);

            AluminumBox box = desiccator.CurrentBox;
            if (box == null) yield break;

            // 取出铝盒
            desiccator.RemoveBox();

            Vector3 offset = box.Center != null ? box.transform.position - box.Center.position : Vector3.zero;

            // 先升高到入口位置
            if (desiccator.EntryPosition != null)
            {
                Vector3 entryPos = desiccator.EntryPosition.position + offset;
                yield return MoveBoxToPosition(box, entryPos);
            }

            // 再移动到托盘
            Vector3 targetPos = box.GetInitialCenterPosition();
            Vector3 finalTargetPos = targetPos + offset;
            yield return MoveBoxToPosition(box, finalTargetPos);

            // 自动关盖
            desiccator.ToggleCap();

            // 跳过 RemoveBoxFromDesiccator 步骤，直接到下一步
            GameManager.Instance.StepManager.AdvanceStep(); // OpenDesiccatorCapAfterCool -> RemoveBoxFromDesiccator
            GameManager.Instance.StepManager.AdvanceStep(); // RemoveBoxFromDesiccator -> CloseBoxCapAfterDry
        }

        private void HandleSpoonClick(Spoon spoon)
        {
            var stepManager = GameManager.Instance.StepManager;
            
            if (stepManager.CurrentStep != ExperimentStep.TakeSoil)
            {
                ShowError("当前步骤不需要使用勺子");
                return;
            }

            spoon.Select();
        }

        private void HandleSoilClick(Soil soil)
        {
            var stepManager = GameManager.Instance.StepManager;
            
            if (stepManager.CurrentStep != ExperimentStep.TakeSoil)
            {
                ShowError("当前步骤不需要取土");
                return;
            }

            Spoon spoon = FindObjectOfType<Spoon>();
            if (spoon == null || !spoon.IsSelected)
            {
                ShowError("请先点击勺子");
                return;
            }

            if (spoon.IsAnimating)
            {
                return;
            }

            // 找到当前铝盒
            AluminumBox targetBox = FindCurrentAluminumBox();
            if (targetBox == null)
            {
                ShowError("找不到铝盒");
                return;
            }

            if (!targetBox.IsOpen)
            {
                ShowError("请先打开铝盒盖");
                return;
            }

            // 获取取土位置和铝盒开口位置
            Transform soilPosition = soil.GetScoopPosition();
            Transform boxOpening = targetBox.OpeningPosition != null ? targetBox.OpeningPosition : targetBox.transform;

            // 记录选择的土样类型
            SoilType selectedType = soil.GetSoilType();
            GameManager.Instance.DataManager.SetCurrentSoilType(selectedType);

            // 执行取土动画
            spoon.DoScoopAnimation(soilPosition, boxOpening, () =>
            {
                // 动画完成后添加土样
                targetBox.AddSoil(selectedType);
                spoon.Deselect();
                stepManager.AdvanceStep();
            });
        }

        private AluminumBox FindCurrentAluminumBox()
        {
            string boxId = GameManager.Instance.SelectedBoxId;
            if (string.IsNullOrEmpty(boxId))
            {
                Debug.LogWarning("还未选择铝盒");
                return null;
            }

            AluminumBox[] boxes = FindObjectsOfType<AluminumBox>();
            foreach (var box in boxes)
            {
                if (box.BoxId == boxId)
                {
                    return box;
                }
            }

            return boxes.Length > 0 ? boxes[0] : null;
        }

        private void ShowError(string message)
        {
            Debug.LogWarning(message);
            var uiManager = FindObjectOfType<SoilMoistureExperiment.UI.UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowError(message);
            }
        }
    }
}
