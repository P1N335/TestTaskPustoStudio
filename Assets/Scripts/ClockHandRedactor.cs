using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using TMPro;
using UniRx;
using Zenject;

public class ClockHandRedactor : MonoBehaviour
{
    public ClockHand ActiveHand => _activeHand;
    public ClockHand[] ClockHands;
    public Camera mainCamera;
    public Vector3 editCameraPosition;
    public Vector3 defaultCameraPosition;
    public float cameraZoomSpeed = 5f;
    [SerializeField] private Button _editButton;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _resultText;
    private bool _isEditing;
    private DateTime _currentTime;
    private ClockHand _activeHand;
    private ClockHandController _controller;

    public bool IsEditing => _isEditing;
    [Inject]
    public void Construct(ClockHandController controller) => _controller = controller;
    private void Start()
    {
        foreach (var hand in ClockHands)
        {
            hand.IsEditingHand
                .Subscribe(isEditing =>
                {
                    if (isEditing)
                    {
                        _activeHand = hand;
                    }
                    else if (_activeHand == hand)
                    {
                        _activeHand = null;
                    }
                })
                .AddTo(this);
        }
    }
    
    public void ToggleEditMode()
    {
        _isEditing = !_isEditing;
        _editButton.GetComponentInChildren<TMP_Text>().text = _isEditing ? "Save" : "Edit";
        Edit();
    }
    
    void Edit()
    {
        if (_isEditing)
        {
            _controller.timeText.gameObject.SetActive(false);
            _resultText.gameObject.SetActive(true);
            _inputField.gameObject.SetActive(true);
            _resultText.text = "Используйте формат ЧЧ:ММ:СС.";
            _controller.KillDots();
            StopAllCoroutines();
            StartCoroutine(SmoothMoveCamera(editCameraPosition));
        }
        else
        {
            if (_inputField.text != "")
            {
                ParseTime(_inputField.text);
                StopAllCoroutines();
                StartCoroutine(SmoothMoveCamera(defaultCameraPosition));
                _controller.MoveClockHandsAnimation();
                _controller.timeText.gameObject.SetActive(true);
                _resultText.gameObject.SetActive(false);
                _inputField.text = null;
                _inputField.gameObject.SetActive(false);
            }
            else
            {
                ParseTime(UpdateClockHandsInfo());
                UpdateClockHandsInfo();
                StopAllCoroutines();
                StartCoroutine(SmoothMoveCamera(defaultCameraPosition));
                _controller.MoveClockHandsAnimation();
                _controller.timeText.gameObject.SetActive(true);
                _resultText.gameObject.SetActive(false);
                _inputField.gameObject.SetActive(false);
            }
            
        }
    }

    private IEnumerator SmoothMoveCamera(Vector3 targetPosition)
    {
        while ((mainCamera.transform.position - targetPosition).magnitude > 0.01f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition,
                Time.deltaTime * cameraZoomSpeed);
            yield return null;
        }
    }

    public string UpdateClockHandsInfo()
    {
       
        int hour = 0;
        int minute = 0;
        int second = 0;
        foreach (ClockHand clockHand in ClockHands)
        {
            float rotationZ = clockHand.transform.rotation.eulerAngles.z;
            switch (clockHand.HandType)
            {
                case HandType.Hour:
                    hour = CalculateHourFromRotation(rotationZ);
                    break;
                case HandType.Minute:
                    minute = CalculateMinuteFromRotation(rotationZ);
                    break;
                case HandType.Second:
                    second = CalculateSecondFromRotation(rotationZ);
                    break;
            }
        }
        string timeString = $"{hour:00}:{minute:00}:{second:00}";
        return timeString;


    }

    private int CalculateHourFromRotation(float rotationZ) => Mathf.FloorToInt(rotationZ / 30f) % 12;
    
    private int CalculateMinuteFromRotation(float rotationZ) => Mathf.FloorToInt(rotationZ / 6f);
    
    private int CalculateSecondFromRotation(float rotationZ) => Mathf.FloorToInt(rotationZ / 6f) % 60;
    
    private void ParseTime(string input)
    {
             DateTime time;
             bool isParsed = DateTime.TryParseExact(input, "HH:mm:ss", null ,DateTimeStyles.None, out time);
             if (isParsed)
             {
                 _controller.UpdateClockTime(time); 
             }
             else
             {
                 _controller.UpdateClockTime(DateTime.UtcNow);
             }

             _inputField.text = "";
    }
}