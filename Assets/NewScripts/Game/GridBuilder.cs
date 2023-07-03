using System;
using UnityEngine;

namespace NewScripts.Game
{
    public class GridBuilder : MonoBehaviour
    {
        public int gridWidth = 10;
        public int gridHeight = 10;
        public int gridSize = 40;
        
        public GameObject horizontalStreetPrefab;
        public GameObject horizontalStreetPrefabGreen;
        public GameObject verticalStreetPrefab;
        public GameObject verticalStreetPrefabGreen;
        public GameObject crossingStreetPrefab;
        
        private readonly System.Random _random = new();

        private void Awake()
        {
            BuildGrid();
        }

        private void BuildGrid()
        {
            int xBlock = 0;
            int zBlock = 0;
            while (xBlock < gridWidth && zBlock < gridHeight)
            {
                if (zBlock < gridWidth - 1)
                {
                    Instantiate(GetRandomHorizontalStreetPrefab(), new Vector3(xBlock * gridSize, 0, zBlock * gridSize * -1), Quaternion.identity);
                }
                if (xBlock < gridHeight - 1)
                {
                    Instantiate(GetRandomVerticalStreetPrefab(), new Vector3(xBlock * gridSize, 0, zBlock * gridSize * -1), Quaternion.identity);
                }
                if(xBlock < gridWidth - 1 && zBlock < gridHeight - 1)
                {
                    Instantiate(crossingStreetPrefab, new Vector3(xBlock * gridSize, 0, zBlock * gridSize * -1), Quaternion.identity);
                }
                xBlock++;
                if (xBlock == gridWidth && zBlock < gridHeight)
                {
                    xBlock = 0;
                    zBlock++;
                }
            }
        }
        
        private GameObject GetRandomHorizontalStreetPrefab()
        {
            int random = _random.Next(0, 5);
            return random == 0 ? horizontalStreetPrefabGreen : horizontalStreetPrefab;
        }
        
        private GameObject GetRandomVerticalStreetPrefab()
        {
            int random = _random.Next(0, 5);
            return random == 0 ? verticalStreetPrefabGreen : verticalStreetPrefab;
        }
    }
}