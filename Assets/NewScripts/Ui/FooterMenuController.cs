using System;
using NewScripts.Common;
using NewScripts.Enums;
using NewScripts.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class FooterMenuController : MonoBehaviour
    {
        public TextMeshProUGUI selectedCompanyText;
        public TextMeshProUGUI currentRoundText;
        public GameObject decisionPanel;
        public GameObject cameraGo;
        public Button decisionButton;
        public Button musicToggle;
        public Button cameraToggle;
        public Button pauseToggle;
        private Settings _settings;

        public void RegisterEvents()
        {
            _settings = ServiceLocator.Instance.Settings;
            decisionButton.interactable = false;
            ServiceLocator.Instance.UiUpdateManager.companySelectedEvent.AddListener(company =>
            {
                selectedCompanyText.text = $"Company: {company.Name} ({company.PlayerType})";
                decisionButton.interactable = company.PlayerType == PlayerType.Human;
                decisionPanel.SetActive(company.PlayerType == PlayerType.Human);
            });
            ServiceLocator.Instance.UiUpdateManager.newPeriodStartedEvent.AddListener((month, year) =>
            {
                currentRoundText.text = $"Round: {month}/{year}";
            });
            musicToggle.onClick.AddListener(() =>
            {
                _settings.IsMusicOn = _settings.IsMusicOn == false;
                musicToggle.GetComponentInChildren<RawImage>().color = _settings.IsMusicOn ? Colors.Indigo : Color.gray;
            });
            pauseToggle.onClick.AddListener(() =>
            {
                _settings.IsPaused = _settings.IsPaused == false;
                foreach (Transform t in pauseToggle.transform)
                {
                    if (t.name == "Pause")
                    {
                        t.gameObject.SetActive(_settings.IsPaused == false);
                    }
                    else if (t.name == "Play")
                    {
                        t.gameObject.SetActive(_settings.IsPaused);
                    }
                }
            });
            cameraToggle.onClick.AddListener(() =>
            {
                _settings.IsIsometricCameraActive = _settings.IsIsometricCameraActive == false;
                if (_settings.IsIsometricCameraActive)
                {
                    cameraGo.GetComponent<Camera>().orthographic = true;
                    cameraGo.GetComponent<SimpleCameraController>().enabled = false;
                    cameraGo.GetComponent<IsometricCameraController>().enabled = true;
                    cameraToggle.GetComponentInChildren<RawImage>().color = Colors.Indigo;
                }
                else
                {
                    cameraGo.GetComponent<Camera>().orthographic = false;
                    cameraGo.GetComponent<SimpleCameraController>().enabled = true;
                    cameraGo.GetComponent<IsometricCameraController>().enabled = false;
                    cameraToggle.GetComponentInChildren<RawImage>().color = Color.gray;
                }
            });
        }
    }
}