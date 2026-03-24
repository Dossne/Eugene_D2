using System;
using System.Collections;
using UnityEngine ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using UnityEngine.UI ;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

[RequireComponent(typeof(Button))]
public class UISayKitLongPressListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    [Range(0.3f, 5f)] public float holdDuration = 0.5f;
    public UnityEvent onLongPress;
    
    private bool _isPointerDown;
    private bool _isLongPressed;
    private DateTime pressTime;

    private Button button;
    private WaitForSeconds delay;
    
    private void Awake() {
        button = GetComponent<Button>();
        delay = new WaitForSeconds(0.1f);
    }

    public void OnPointerDown(PointerEventData eventData) {
        _isPointerDown = true;
        pressTime = DateTime.Now;
        StartCoroutine(Timer());
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        _isPointerDown = false;
        _isLongPressed = false;
    }

    private IEnumerator Timer() {
        while (_isPointerDown && !_isLongPressed) {
            var elapsedSeconds = (DateTime.Now - pressTime).TotalSeconds;
            if (elapsedSeconds >= holdDuration) {
                _isLongPressed = true;
                if (button.interactable)
                    onLongPress?.Invoke();

                yield break;
            }

            yield return delay;
        }
    }
}
