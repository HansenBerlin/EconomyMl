using System;
using UnityEngine;

namespace NewScripts.Ui
{
    public class CompanyActionActivator : MonoBehaviour
    {
        private GameObject _playerControls;
        private bool _initDone;
        

        public void Update()
        {
            if (_initDone == false)
            {
                _playerControls = GameObject.Find("PlayerControls");
                if (_playerControls == null)
                {
                    return;
                }
                _playerControls.SetActive(false);
                _initDone = true;
            }
        }

        public void ActivatePanel()
        {
            if (_initDone)
            {
                _playerControls.SetActive(true);
            }
        }
    }
}