using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class TimelineGraphDrawer : MonoBehaviour
    {
        public float lineWidth;
        public Vector2 graphScale;
        public GameObject imagePrefab;
        public GameObject scrollView;

        private float _lastX;
        private float _lastY;
        private readonly System.Random _random = new();
        
        public void DrawLine()
        {
            float ax = _lastX;
            float bx = _lastX + 50;
            float ay = _lastY;
            float by = _random.Next(0, 500);
            _lastX = bx;
            _lastY = by;
            MakeLine(ax, ay, bx, by, Color.red);
        }

        private void MakeLine(float ax, float ay, float bx, float by, Color col)
        {
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(_lastX, 900);
            GameObject newObj = Instantiate(imagePrefab, transform);
            newObj.name = "line from " + ax + " to " + bx;
            newObj.GetComponent<RawImage>().color = col;
            RectTransform rect = newObj.GetComponent<RectTransform>();
            //rect.SetParent(transform);
            //rect.localScale = Vector3.one;
 
            Vector3 a = new Vector3(ax*graphScale.x, ay*graphScale.y, 0);
            Vector3 b = new Vector3(bx*graphScale.x, by*graphScale.y, 0);
            Vector3 dif = a - b;

            rect.localPosition = (a + b) / 2;
            rect.sizeDelta = new Vector2(dif.magnitude, lineWidth);
            rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));

            StartCoroutine(ResetScrollview());
            //var sv = scrollView.GetComponent<ScrollRect>();
            //sv.normalizedPosition = new Vector2(1, 0);

        }

        IEnumerator ResetScrollview()
        {
            var sv = scrollView.GetComponent<ScrollRect>();
            sv.normalizedPosition = new Vector2(1, 0);
            yield return new WaitForEndOfFrame();
        }
    }
}