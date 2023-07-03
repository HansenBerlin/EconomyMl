using UnityEngine;

namespace NewScripts.Game
{
    public class IsometricCameraController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float zoomSpeed = 0.5f;
        public float maxHeight = 0.5f;
        public float defaultXAngle = 35f;
        public float currentAngle = 45;

        private Camera _cameraComponent;

        private void Start()
        {
            _cameraComponent = GetComponent<Camera>();
        }

        private void Update()
        {
            var transform1 = transform;
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f) * moveSpeed * Time.deltaTime;
            transform1.Translate(moveDirection);

            float zoomInput = Input.GetAxis("ZoomCamera");
            float newSize = _cameraComponent.orthographicSize - zoomInput * zoomSpeed * Time.deltaTime;
            _cameraComponent.orthographicSize = Mathf.Clamp(newSize, 1f, maxHeight);

            float rotation = Input.GetAxis("RotateCamera");
            currentAngle = rotation > 0 ? currentAngle + 90 : rotation < 0 ? currentAngle - 90 : currentAngle;
            transform1.eulerAngles = new Vector3(defaultXAngle, currentAngle, 0);
        }
    }
}