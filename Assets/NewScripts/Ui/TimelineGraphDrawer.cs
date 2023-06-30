using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class TimelineGraphDrawer : MonoBehaviour
    {
        public float lineWidth;
        public Vector2 graphScale;
        public GameObject imagePrefab;
        public GameObject scrollView;
        public TextMeshProUGUI textPrefabLeft;
        public TextMeshProUGUI textPrefabRight;
        public GameObject lineParent;
        public GameObject textParent;
        public float graphHeight;
        public float stepWidth = 100;
        private float _lastX;
        private float _lastY;
        private readonly System.Random _random = new();
        private float _valueModifier = 1;
        
        public void DrawLine()
        {
            float ax = _lastX;
            float bx = _lastX + 100;
            float ay = _lastY;
            float by = _random.Next(0, 900);
            _lastX = bx;
            _lastY = by;
            MakeLine(ax, ay, bx, by, Color.red);
        }

        public void InitializeValues(float max, float stepwidth = 100)
        {
            _valueModifier = graphHeight / max;
            stepWidth = stepwidth;
            for (int i = 1; i < 10; i++)
            {
                var tickLeft = Instantiate(textPrefabLeft, textParent.transform);
                var tickRight = Instantiate(textPrefabRight, textParent.transform);
                string text = $"{max / 10 * i:0}";
                tickLeft.text = text;
                tickRight.text = text;
                tickLeft.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, i * 100);
                tickRight.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50 - stepWidth, i * 100);
            }
        }

        public void AddDatapoint(float value)
        {
            float ax = _lastX;
            float bx = _lastX + stepWidth;
            float ay = _lastY;
            float by = value * _valueModifier;
            _lastX = bx;
            _lastY = by;
            MakeLine(ax, ay, bx, by, Color.red);
        }

        private void MakeLine(float ax, float ay, float bx, float by, Color col)
        {
            lineParent.GetComponent<RectTransform>().sizeDelta = new Vector2(_lastX + stepWidth, graphHeight);
            GameObject newObj = Instantiate(imagePrefab, lineParent.transform);
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

            //StartCoroutine(ResetScrollview());
            var sv = scrollView.GetComponent<ScrollRect>();
            sv.normalizedPosition = new Vector2(1, 0);

        }

        IEnumerator ResetScrollview()
        {
            var sv = scrollView.GetComponent<ScrollRect>();
            sv.normalizedPosition = new Vector2(1, 0);
            yield return new WaitForEndOfFrame();
        }
    }
}