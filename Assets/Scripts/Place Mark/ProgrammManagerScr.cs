using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ProgrammManagerScr : MonoBehaviour
{
    [SerializeField] private GameObject planeMarkerPrefab;
    [SerializeField] private GameObject placedObjectPrefab;
    [SerializeField] private Camera arCamera;

    private ARRaycastManager _arRaycastManagerScript;
    private bool _placedMap = false;
    private PersonScr _personScr;

    void Start()
    {
        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        planeMarkerPrefab.SetActive(false);
        placedObjectPrefab.SetActive(false);
    }

    void Update()
    {
        if (!_placedMap)
        {
            ShowMarker();
        }
        else
        {
            DetechedMovePerson();
        }
    }

    void ShowMarker()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        _arRaycastManagerScript.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);
        if (hits.Count > 0)
        {
            planeMarkerPrefab.transform.position = hits[0].pose.position;
            planeMarkerPrefab.SetActive(true);
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            GameObject person = Instantiate(placedObjectPrefab, hits[0].pose.position, Quaternion.identity);
            person.SetActive(true);
            planeMarkerPrefab.SetActive(false);
            _placedMap = true;
        }
    }

    void DetechedMovePerson()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            Ray ray = arCamera.ScreenPointToRay(touch.position);
            RaycastHit hitObject;

            if (Physics.Raycast(ray, out hitObject))
            {
                if (hitObject.collider.CompareTag("Person"))
                {
                    _personScr = hitObject.collider.gameObject.GetComponent<PersonScr>();
                    _personScr.GenerateMovements();
                } else if (hitObject.collider.CompareTag("Movement"))
                {
                    _personScr.CleanUpMovements(hitObject.collider.gameObject.transform.position);
                    _personScr = null;
                }
            }
        }
    }
}
