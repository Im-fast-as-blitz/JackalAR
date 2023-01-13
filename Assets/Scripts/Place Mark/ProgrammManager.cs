using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ProgrammManager : MonoBehaviour
{
    public bool placedMap = false;
    
    [SerializeField] private GameObject PlaneMarkerPrefab;
    [SerializeField] private GameObject PlacedObjectPrefab;

    private ARRaycastManager ARRaycastManagerScript;
    void Start()
    {
        ARRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        PlaneMarkerPrefab.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!placedMap)
        {
            ShowMarker();
        }
    }

    void ShowMarker()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        ARRaycastManagerScript.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);
        if (hits.Count > 0)
        {
            PlaneMarkerPrefab.transform.position = hits[0].pose.position;
            PlaneMarkerPrefab.SetActive(true);
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Instantiate(PlacedObjectPrefab, hits[0].pose.position, PlacedObjectPrefab.transform.rotation);
            PlaneMarkerPrefab.SetActive(true);
            placedMap = true;
        }
    }
}
