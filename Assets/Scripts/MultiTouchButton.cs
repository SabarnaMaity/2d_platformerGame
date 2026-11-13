using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MultiTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Action onPointerDown;
    public Action onPointerUp;
    public Action<bool> onStateChanged;

    private int activePointerId = -1;
    public bool isPressed { get; private set; } = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPressed)
        {
            activePointerId = eventData.pointerId;
            isPressed = true;
            onPointerDown?.Invoke();
            onStateChanged?.Invoke(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressed && eventData.pointerId == activePointerId)
        {
            Release();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPressed && eventData.pointerId == activePointerId)
        {
            Release();
        }
    }

    public void Release()
    {
        if (isPressed)
        {
            isPressed = false;
            activePointerId = -1;
            onPointerUp?.Invoke();
            onStateChanged?.Invoke(false);
        }
    }

    private void OnDisable()
    {
        Release();
    }
}

