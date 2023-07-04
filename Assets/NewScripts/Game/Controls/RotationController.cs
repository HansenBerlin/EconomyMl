using System.Collections;
using UnityEngine;

namespace NewScripts.Game.Controls
{
    public class RotationController : MonoBehaviour
    {
        public Transform objectToRotate; 
        public float rotationDuration = 2f;

        private bool _isRotating; 

        private void Awake()
        {
            Vector3 centerOffset = objectToRotate.GetComponent<Renderer>().bounds.center - objectToRotate.position;
            objectToRotate.SetParent(transform);
            objectToRotate.localPosition = -centerOffset;
            StartCoroutine(RotateObject());
        }

        public void ActivateAnimation()
        {
            StartCoroutine(RotateObject());
        }

        private IEnumerator RotateObject()
        {
            while (true)
            {
                _isRotating = true;
                float elapsedTime = 0f;
                Quaternion startRotation = transform.rotation;
                Quaternion targetRotation = transform.rotation * Quaternion.Euler(0f, 0f, 180f);

                while (elapsedTime < rotationDuration)
                {
                    transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                transform.rotation = targetRotation;
                _isRotating = false;

                yield return new WaitForSeconds(2f);
            }
        }
    }
}