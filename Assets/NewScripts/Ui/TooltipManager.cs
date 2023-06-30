using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class TooltipManager : MonoBehaviour
    {
        public GameObject tooltipPrefab;
        private GameObject _tooltipInstance;
        public string tooltipText;
        
        public void Awake()
        {
            _tooltipInstance = Instantiate(tooltipPrefab, transform);
            _tooltipInstance.GetComponentInChildren<TextMeshProUGUI>().text = tooltipText;
            _tooltipInstance.SetActive(false);
            //transform.GetComponent<Button>().
        }
        

        public void OnPointerEnter()
        {
            _tooltipInstance.SetActive(true);
        }

        public void OnPointerExit()
        {
            _tooltipInstance.SetActive(false);
        }
    }
}