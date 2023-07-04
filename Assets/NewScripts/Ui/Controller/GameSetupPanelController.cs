using NewScripts.Game.Flow;
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
        public Toggle isTrainingToggle;
        public Toggle writeToDatabaseToggle;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI aiCountText;
        public Button startButton;

        public void Awake()
        {
            startButton.onClick.AddListener(StartGame);
        }
        public void Update()
        {
            playerCountText.text = $"Human players: {playerCountSlider.value}/4";
            aiCountText.text = $"AI Players: {aiCountSlider.value}/30";
        }
        
        private void StartGame()
        {
            if (playerCountSlider.value == 0 && aiCountSlider.value == 0)
            {
                return;
            }
            startButton.interactable = false;
            bottomBar.SetActive(true);
            servicesGo.SetActive(true);
            var setup = setupGo.GetComponent<SetupEnvironment>();
            int humanPlayers = isTrainingToggle.isOn ? 0 : (int)playerCountSlider.value;
            setup.playerCompaniesPerType = humanPlayers;
            setup.aiCompaniesPerType = (int)aiCountSlider.value;
            setup.writeToDatabase = writeToDatabaseToggle.isOn;
            setup.isTraining = isTrainingToggle.isOn;
            setupGo.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}