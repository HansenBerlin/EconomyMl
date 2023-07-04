using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Ui.Models
{
    public class WorkerRow : MonoBehaviour
    {
        public TextMeshProUGUI periodText;
        public TextMeshProUGUI startText;
        public TextMeshProUGUI hiredText;
        [FormerlySerializedAs("firedText")] public TextMeshProUGUI firedDecisionText;
        public TextMeshProUGUI firedForcedText;
        public TextMeshProUGUI quitText;
        public TextMeshProUGUI endText;
        public TextMeshProUGUI paidText;
        public TextMeshProUGUI offeredWageText;
        public TextMeshProUGUI avgWageText;
        public TextMeshProUGUI openPositionsText;
    }
}