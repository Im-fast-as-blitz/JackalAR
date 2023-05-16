using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    private Vector3 touch;

    [SerializeField] private float zoomMin = 0.1f;
    [SerializeField] private float zoomMax = 2f;

    private Camera _currCamera;

    private void Start()
    {
        _currCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touch = _currCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, Camera.main.nearClipPlane));
        }

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroLastPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOneLastPos = touchOne.position - touchOne.deltaPosition;

            float distTouch = (touchZeroLastPos - touchOneLastPos).magnitude;
            float currDistTouch = (touchZero.position - touchOne.position).magnitude;
            Zoom((currDistTouch - distTouch) * 0.01f);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 direction = touch - _currCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, _currCamera.nearClipPlane));
            _currCamera.transform.position = new Vector3(Mathf.Clamp(_currCamera.transform.position.x + direction.x, -1.0f, 1.0f), _currCamera.transform.position.y,
                Mathf.Clamp(_currCamera.transform.position.z + direction.z, -1.0f, 1.0f));
        }
    }

    void Zoom(float increment)
    {
        _currCamera.orthographicSize = Mathf.Clamp(_currCamera.orthographicSize - increment, zoomMin, zoomMax);
    }
}
