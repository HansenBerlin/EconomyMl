using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace NewScripts.Http
{
    public static class HttpService
    {
        public static IEnumerator Insert<T>(string url, T requestModel)
        {
            string body = JsonUtility.ToJson(requestModel);
            UnityWebRequest request = UnityWebRequest.Post(url, body, "application/json");
            request.SetRequestHeader("content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
 
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
            }
        }
    }
}