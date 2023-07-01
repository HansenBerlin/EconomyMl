using UnityEngine;

namespace NewScripts
{
    public class DummyTileActivator : MonoBehaviour
    {
        public GameObject houseTile;
        public GameObject parkTile;
        public GameObject parkingLotTile;
        
        public void Awake()
        {
            var random = new System.Random().Next(0, 3);
            switch (random)
            {
                case 0:
                    houseTile.SetActive(true);
                    break;
                case 1:
                    parkTile.SetActive(true);
                    break;
                default:
                    parkingLotTile.SetActive(true);
                    break;
            }
        }
        
    }
}