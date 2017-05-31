using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isZooming;

    public void OnPointerDown(PointerEventData eventData)
    {
        isZooming = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isZooming = false;
    }
}
