using UnityEngine;

namespace NewScripts.Game.World
{
    public class Spawner : MonoBehaviour
    {
        public GameObject prefabBlue;
        public GameObject prefabRed;
        public GameObject prefabHuman;
        public int count;
        public int sideLength;
        private readonly System.Random _random = new();

        void Awake()
        {
            TwoDimensionalSpawner();
        }
        
        void TwoDimensionalSpawner()
        {
            float xPos = 0;
            float zPos = 0;
            int i = 0;
            
            while (i < count)
            {
                GameObject instance = Instantiate(prefabHuman);
                float randomPositionModifier = (float) _random.Next(-1, 1) / 10;
                instance.transform.position = new Vector3(xPos + randomPositionModifier, 0, zPos + randomPositionModifier);
                float randomSize = (float) _random.Next(8, 1) / 10;
                instance.transform.localScale = new Vector3(randomSize, randomSize, randomSize);
                float randomRotation = _random.Next(-60, 60);
                instance.transform.rotation = new Quaternion(0, randomRotation, 0, 0);
                
                i++;
                if (i % sideLength == 0 && i != 0)
                {
                    zPos++;
                    xPos = 0;
                }
                else
                {
                    xPos++;
                }
            }
        }

        void ThreeDimensionalSpawner()
        {
            float xPos = 0;
            float zPos = 0;
            float yPos = 0;
            int i = 0;
            
            while (i < count)
            {
                GameObject instance = (yPos + zPos + xPos) % 2 == 0 ? Instantiate(prefabBlue) : Instantiate(prefabRed);
                instance.transform.position = new Vector3(xPos, yPos, zPos);
                
                Debug.Log($"xPos: {xPos}, yPos: {yPos}, zPos: {zPos}");
                
                i++;
                if (i % (sideLength * sideLength) == 0 && i != 0)
                {
                    yPos++;
                    zPos = 0;
                    xPos = 0;
                }
                else if (i % sideLength == 0 && i != 0)
                {
                    zPos++;
                    xPos = 0;
                }
                else
                {
                    xPos++;
                }
            }
        }
        
    }
}