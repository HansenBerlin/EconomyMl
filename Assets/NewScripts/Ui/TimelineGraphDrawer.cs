using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Enums;
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
        public Color color = new(0.271F, 0.153F, 0.627F);
        public float graphHeight;
        public float stepWidth = 50;
        
        private float _lastX;
        private float _lastY;
        private float _valueModifier = 1;
        private readonly List<GameObject> _lines = new();
        private readonly List<GameObject> _ticks = new();
        private readonly List<float> _values = new();
        private float _alltimeMax;
        
       //public void InitializeValues(float max, float stepwidth = 50)
       //{
       //    _alltimeMax = max * 1.1F;
       //    _valueModifier = graphHeight / _alltimeMax;
       //    stepWidth = stepwidth;
       //    AddTicks(max);
       //}
        
        public void DrawGraph<T>(List<T> values = null)
        {
            if (values == null)
            {
                var floats = ConvertAndAddFloats(values);
                _values.AddRange(floats);
            }
            _lastX = 0;
            _lastY = 0;
            _alltimeMax = _values.Max() * 1.1F;
            _valueModifier = graphHeight / _alltimeMax;
            
            AddTicks(_alltimeMax);
            foreach (var val in _values)
            {
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
        
        public List<float> ConvertAndAddFloats<T>(List<T> objectList)
        {
            List<float> floats = new();
            foreach (object obj in objectList)
            {
                if (obj is float f)
                {
                    floats.Add(f);
                }
                else
                {
                    throw new InvalidCastException("List contains non-float values");
                }
            }

            return floats;
        }

        public void AddDatapoint(float value)
        {
            _values.Add(value);
            if (value > _alltimeMax)
            {
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
            _lastY = by;
            MakeLine(ax, ay, bx, by, color);
        }

        private void AddTicks(float max)
        {
            float range = graphHeight / 11;
            for (int i = 1; i < 11; i++)
            {
                var tickLeft = Instantiate(textPrefabLeft, textParent.transform);
                var tickRight = Instantiate(textPrefabRight, textParent.transform);
                string text = max > 10 ? $"{max / 10 * i:0}" : $"{max / 10 * i:0.##}";
                tickLeft.text = text;
                tickRight.text = text;
                tickLeft.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, i * range);
                tickRight.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50 - stepWidth, i * range);
                _ticks.Add(tickLeft.gameObject);
                _ticks.Add(tickRight.gameObject);
            }
        }

        private void MakeLine(float ax, float ay, float bx, float by, Color col)
        {
            lineParent.GetComponent<RectTransform>().sizeDelta = new Vector2(_lastX + stepWidth, graphHeight);
            GameObject line = Instantiate(imagePrefab, lineParent.transform);
            line.name = "line from " + ax + " to " + bx;
            line.GetComponent<RawImage>().color = col;
            RectTransform rect = line.GetComponent<RectTransform>();
 
            Vector3 a = new Vector3(ax*graphScale.x, ay*graphScale.y, 0);
            Vector3 b = new Vector3(bx*graphScale.x, by*graphScale.y, 0);
            Vector3 dif = a - b;

            rect.localPosition = (a + b) / 2;
            rect.sizeDelta = new Vector2(dif.magnitude, lineWidth);
            rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
            
            _lines.Add(line);
        }

        public void SetScrollview()
        {
            var sv = scrollView.GetComponent<ScrollRect>();
            sv.normalizedPosition = new Vector2(1, 0);
        }
    }
}