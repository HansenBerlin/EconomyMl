using NewScripts.Game.Flow;
using NewScripts.Game.Services;
using NewScripts.Game.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Controller
{
    public class GameSetupPanelController : MonoBehaviour
    {
        public GameObject servicesGo;
        public GameObject setupGo;
        public GameObject bottomBar;
        public Slider playerCountSlider;
        public Slider aiCountSlider;
        public Slider aiCountSliderTwo;
        public Toggle isTrainingToggle;
        public Toggle writeToDatabaseToggle;
        public Toggle isAutomode;
        public Toggle isGovermnentTraining;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI aiCountText;
        public TextMeshProUGUI aiCountTwoText;
        public Button startButton;

        public void Awake()
        {
            startButton.onClick.AddListener(StartGame);
        }
        public void Update()
        {
            playerCountText.text = $"Human players: {playerCountSlider.value}/4";
            aiCountText.text = $"AI Players PPO: {aiCountSlider.value}/40";
            aiCountTwoText.text = $"AI Players SAC: {aiCountSliderTwo.value}/40";
        }
        
        private void StartGame()
        {
            if (playerCountSlider.value == 0 && aiCountSlider.value == 0 && aiCountSliderTwo.value == 0)
            {
                return;
            }
            startButton.interactable = false;
            bottomBar.SetActive(true);
            servicesGo.SetActive(true);
            servicesGo.GetComponent<ServiceLocator>().Settings.IsAutoPlay = isAutomode.isOn;
            var setup = setupGo.GetComponent<EnvironmentSetup>();
            int humanPlayers = isTrainingToggle.isOn ? 0 : (int)playerCountSlider.value;
            setup.playerCompaniesPerType = humanPlayers;
            setup.aiPpoCompaniesPerType = (int)aiCountSlider.value;
            setup.writeToDatabase = writeToDatabaseToggle.isOn;
            setup.isTraining = isTrainingToggle.isOn;
            setup.aiSacCompaniesPerType = (int)aiCountSliderTwo.value;
            setup.isGovermnentTraining = isGovermnentTraining.isOn;
            setupGo.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}