using UnityEngine;
using UniRx;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;
using Zenject;

public class ClockHandController : MonoBehaviour
{
    private TimeManager _timeManager;
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Transform secondHand; 
    public TMP_Text timeText;
    private DateTime _startTime;
    private float _elapsedTime;
    private DateTime _currentTime;
    [Inject]
    public void Construct(TimeManager timeManager)
    {
        _timeManager = timeManager;
    }
    private void Start()
    {
        if (_timeManager != null)
        {
            _timeManager.OnTimeReceived 
                .Subscribe(OnNext, OnError)
                .AddTo(this);
        }
        else
        {
            Debug.LogError("TimeHandler is not assigned.");
        }
    }
    
    private void OnNext(DateTime dateTime)
    {
         Debug.Log("Current Time: " + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
         UpdateClockTime(dateTime);
    }

     private void OnError(Exception ex)
    {
        Debug.LogError("Error receiving time: " + ex.Message);
        
    }
     private void Update()
     {
             _elapsedTime += Time.deltaTime;
             _currentTime = _startTime.AddSeconds(_elapsedTime);
             timeText.text = _currentTime.ToString("HH:mm:ss");
     }
     public void UpdateClockTime(DateTime dateTime)
     {
         float hourAngle = (dateTime.Hour % 12 + dateTime.Minute / 60.0f) * 30.0f; 
         float minuteAngle = dateTime.Minute * 6.0f;                          
         float secondAngle = dateTime.Second * 6.0f;
         hourHand.localRotation = Quaternion.Euler(0, 0, hourAngle);     
         minuteHand.localRotation = Quaternion.Euler(0, 0, minuteAngle);  
         secondHand.localRotation = Quaternion.Euler(0, 0, secondAngle);
         KillDots();
         MoveClockHandsAnimation();
         RestartTime(dateTime);
     }
     public void MoveClockHandsAnimation()
     {
         hourHand.DORotate(new Vector3(hourHand.rotation.x, hourHand.rotation.y, hourHand.rotation.z + 360), 43200f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear);
         minuteHand.DORotate(new Vector3(minuteHand.rotation.x, minuteHand.rotation.y, minuteHand.rotation.z + 360), 3600f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
         secondHand.DORotate(new Vector3(secondHand.rotation.x, secondHand.rotation.y, secondHand.rotation.z + 360), 60f, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
     }
     public void KillDots() => DOTween.KillAll();
     

     public void RestartTime(DateTime userTime)
     {
         _startTime = userTime;    
         _elapsedTime = 0;
         _currentTime = _startTime;
     }
}
