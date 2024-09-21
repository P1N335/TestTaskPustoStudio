using UniRx;
using UnityEngine;
using Zenject;
public class ClockHand : MonoBehaviour
{
    public HandType HandType; 
    public IReadOnlyReactiveProperty<bool> IsEditingHand => _isEditingHand;
    [Inject]
    public void Construct(ClockHandRedactor redactor) =>  _redactor = redactor;
    private bool _isDragging;
    private ClockHandRedactor _redactor;
    private readonly ReactiveProperty<bool> _isEditingHand = new ReactiveProperty<bool>(false);

     private void OnMouseDown()
     {
         if (_redactor.IsEditing)
         {
             _isEditingHand.Value = true;
         }
     }

     private void OnMouseDrag()
    {
        if ( _redactor.IsEditing)
        {
            RotateHandWithMouse();
        }
    }
     private void OnMouseUp()
     {
         if (_redactor.IsEditing)
         {
             _isEditingHand.Value = false;
         }
     }
     
     private void RotateHandWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xy = new Plane(Vector3.forward, transform.position);

        if (xy.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            Vector3 direction = worldPosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        _isEditingHand.Value = true;
    }
}

public enum HandType
{
    Second,
    Minute,
    Hour
}

