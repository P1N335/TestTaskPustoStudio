using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using UniRx;


public class TimeManager : MonoBehaviour
{
#if UNITY_EDITOR
    private readonly string _timeApiUrl = "https://yandex.com/time/sync.json";
#else
    private readonly string _timeApiUrl = "https://api.allorigins.win/raw?url=https://yandex.com/time/sync.json";
#endif
    
    
   
    

    private Subject<DateTime> _timeSubject = new Subject<DateTime>();

    public IObservable<DateTime> OnTimeReceived => _timeSubject;

    private void Start()
    {
        StartCoroutine(FetchTimeCoroutine());
    }
    
    public IEnumerator FetchTimeCoroutine()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get(_timeApiUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching time: " + request.error);
                _timeSubject.OnError(new Exception(request.error));
            }
            else
            {
                bool success = false;

                try
                {
                    string responseJson = request.downloadHandler.text;
                    TimeResponse timeResponse = JsonUtility.FromJson<TimeResponse>(responseJson);

                    DateTime currentTime = DateTimeOffset.FromUnixTimeMilliseconds(timeResponse.time).DateTime;
                    _timeSubject.OnNext(currentTime);

                    success = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error processing response: " + ex.Message);
                    _timeSubject.OnError(ex);
                }

                if (success)
                {
                    yield return new WaitForSeconds(3600);
                    continue;
                }
            }
            yield return new WaitForSeconds(5);
        }
    }
    [Serializable]
    private class TimeResponse
    {
        public long time;
    }
}