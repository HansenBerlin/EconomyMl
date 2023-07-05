using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts.Ui.Controller
{
    public class TimelineDataPanelController : MonoBehaviour
    {
        public float lineWidth;
        public Vector2 graphScale;
        public GameObject imagePrefab;
        public GameObject scrollView;
        public TextMeshProUGUI textPrefabLeft;
        public TextMeshProUGUI textPrefabRight;
        public GameObject lineParent;
        public GameObject textParent;
        public Color color = Colors.Amber;
        public float graphHeight;
        public float stepWidth = 50;
        [FormerlySerializedAs("drawTicks")] public bool drawRightTicks = true;
        public bool drawLeftTicks = true;
        
        private float _lastX;
        private float _lastY;
        private float _valueModifier = 1;
        private readonly List<GameObject> _lines = new();
        private readonly List<GameObject> _ticks = new();
        private readonly List<float> _values = new();
        private float _alltimeMax = 1;
        
        public void DrawGraph<T>(List<T> values = null, float maxOverwrite = -1)
        {
            if (values != null)
            {
                var floats = (from object obj in values select Convert.ToSingle(obj)).ToList();
                _values.AddRange(floats);
            }
            _alltimeMax = maxOverwrite > 0 ? maxOverwrite : _values.Max() < 1 ? 1 : _values.Max();
            _valueModifier = graphHeight / _alltimeMax * 0.9F;
            _lastX = _values[0];
            _lastY = _values[0] * _valueModifier;
            
            AddTicks(_alltimeMax);

            foreach (var val in _values.GetRange(1, _values.Count - 1))
            {
                if (val < 0)
                {
                    //continue;
                }
                DefineLineValues(val);
            }
            SetScrollview();
        }

        public void RemoveGraph()
        {
            _values.Clear();
            foreach (var tick in _ticks)
            {
                Destroy(tick);
            }
            foreach (var line in _lines)
            {
                Destroy(line);
            }
        }

        public void AddDatapoint(float value)
        {
            _values.Add(value);
            if (value > _alltimeMax)
            {
                var values = new List<float>(_values);
                RemoveGraph();
                _values.AddRange(values);
                DrawGraph<float>();
            }
            else
            {
                DefineLineValues(value);
            }
            SetScrollview();
        }

        private void DefineLineValues(float value)
        {
            float ax = _lastX;
            float bx = _lastX + stepWidth;
            float ay = _lastY;
            float by = value * _valueModifier;
            _lastX = bx;
            _lastY = by < 0 ? _lastY : by;
            DrawLine(ax, ay, bx, by, color);
        }

        private void AddTicks(float max)
        {
            float range = graphHeight / 11;
            
            for (int i = 1; i < 11; i++)
            {

                string text = max > 10 ? $"{max / 10 * i:0}" : $"{max / 10 * i:0.##}";
                if (drawRightTicks)
                {
                    var tickRight = Instantiate(textPrefabRight, textParent.transform);
                    tickRight.text = text;
                    tickRight.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50 - stepWidth, i * range);
                    _ticks.Add(tickRight.gameObject);
                }

                if (drawLeftTicks)
                {
                    var tickLeft = Instantiate(textPrefabLeft, textParent.transform);
                    tickLeft.text = text;
                    tickLeft.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, i * range);
                    _ticks.Add(tickLeft.gameObject);
                }
            }
        }

        private void DrawLine(float ax, float ay, float bx, float by, Color col)
        {
            lineParent.GetComponent<RectTransform>().sizeDelta = new Vector2(bx, graphHeight);
            float byadapted = by;
            if (by < 0)
            {
                col = Color.clear;
                byadapted = _lastY;
            }
            GameObject line = Instantiate(imagePrefab, lineParent.transform);
            line.GetComponent<RawImage>().color = col;
            RectTransform rect = line.GetComponent<RectTransform>();
 
            Vector3 a = new Vector3(ax*graphScale.x, ay*graphScale.y, 0);
            Vector3 b = new Vector3(bx*graphScale.x, byadapted*graphScale.y, 0);
            Vector3 dif = a - b;
            rect.localPosition = (a + b) / 2;
            rect.sizeDelta = new Vector2(dif.magnitude, lineWidth);
            rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
                
            _lines.Add(line);
        }

        private void SetScrollview()
        {
            if (scrollView != null)
            {
                var sv = scrollView.GetComponent<ScrollRect>();
                sv.normalizedPosition = new Vector2(1, 0);
            }
        }
    }
}