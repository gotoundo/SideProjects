using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
//using System;

public class UIScreenDrag : MonoBehaviour, IDragHandler, IScrollHandler
{
    public static UIScreenDrag Main;

    void Awake()
    {
        Main = this;
    }

    public static float dragSpeed = .1f;

    public void OnDrag(PointerEventData eventData)
    {
        GameManager.Main.camera.transform.position -= new Vector3(eventData.delta.x, 0, eventData.delta.y) * dragSpeed;
    }

    public void OnScroll(PointerEventData eventData)
    {
        GameManager.Main.camera.transform.position -= new Vector3(0, eventData.scrollDelta.y, 0);
    }
}
