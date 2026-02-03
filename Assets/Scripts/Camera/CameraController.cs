/*
 * 文件名：CameraController.cs
 * 作者：Cyans
 * 开发日期：2025年12月12日
 * 来自：长安大学
 * 
 * 描述：相机控制器，提供场景浏览功能。
 * 支持移动、旋转、缩放，可禁用缩放和重置位置。
 */

using UnityEngine;

namespace SoilMoistureExperiment.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float MoveSpeed = 5f;
        public float FastMoveMultiplier = 2f;

        [Header("Look Settings")]
        public float MouseSensitivity = 2f;
        public float MinPitch = -80f;
        public float MaxPitch = 80f;

        [Header("Zoom Settings")]
        public float ZoomSpeed = 5f;
        public bool EnableZoom = true;

        private float yaw;   // 水平旋转
        private float pitch; // 垂直旋转

        private Vector3 initialPosition;
        private float initialYaw;
        private float initialPitch;

        private void Start()
        {
            // 初始化角度为当前相机朝向
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
            
            if (pitch > 180f) pitch -= 360f;

            // 保存初始位置和角度
            initialPosition = transform.position;
            initialYaw = yaw;
            initialPitch = pitch;
        }

        public void ResetToInitial()
        {
            transform.position = initialPosition;
            yaw = initialYaw;
            pitch = initialPitch;
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        private void Update()
        {
            HandleMovement();
            HandleLook();
            HandleZoom();
        }

        private void HandleMovement()
        {
            // WASD移动
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;

            if (horizontal == 0f && vertical == 0f) return;

            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            Vector3 moveDir = forward * vertical + right * horizontal;
            moveDir.Normalize();

            // Shift加速
            float speed = MoveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= FastMoveMultiplier;
            }

            transform.position += moveDir * speed * Time.deltaTime;
        }

        private void HandleLook()
        {
            // 按住空格 + 鼠标左键才能旋转视角
            if (!Input.GetKey(KeyCode.Space) || !Input.GetMouseButton(0)) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * MouseSensitivity;
            pitch -= mouseY * MouseSensitivity;

            // 限制垂直角度
            pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        private void HandleZoom()
        {
            if (!EnableZoom) return;
            
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // 向前/后移动相机
                transform.position += transform.forward * scroll * ZoomSpeed;
            }
        }
    }
}
