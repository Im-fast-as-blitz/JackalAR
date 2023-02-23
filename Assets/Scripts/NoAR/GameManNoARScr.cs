using UnityEngine;

public class GameManNoARScr : MonoBehaviour
{
    public Game CurrentGame;
    public GameObject cardPrefab;

//    [SerializeField] private GameObject planeMarkerPrefab;
    [SerializeField] private GameObject placedObjectPrefab;
    [SerializeField] private Camera mainCamera;
//    [SerializeField] private GameObject startText;

//    private ARRaycastManager _arRaycastManagerScript;
//    private bool _placedMap = false;
    private PersonNoAR _personScr;
    private LayerMask _layerMask;

    private Helpers.Teams _currTeam = Helpers.Teams.White;

    void Start()
    {
//        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        _layerMask = LayerMask.NameToLayer("Person");

        CurrentGame = new Game();
        
        PersonManagerScr.currGame = CurrentGame;

        BuildPlayingField(new Vector3(0, 0, 0));

//        planeMarkerPrefab.SetActive(false);
    }

    void Update()
    {
//        if (!_placedMap)
//        {
//            ShowMarker();
//        }
//        else
//        {
            DetachedMovePerson();
//        }
    }

    // void ShowMarker()
    // {
    //     List<ARRaycastHit> hits = new List<ARRaycastHit>();
    //     _arRaycastManagerScript.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);
    //     if (hits.Count > 0)
    //     {
    //         planeMarkerPrefab.transform.position = hits[0].pose.position + new Vector3(0, 0.03f, 0);
    //         planeMarkerPrefab.SetActive(true);
    //     }
    //
    //     if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
    //     {
    //         startText.SetActive(false);
    //         Vector3 gamePos = hits[0].pose.position + new Vector3(0, 0.03f, 0);
    //
    //
    //         planeMarkerPrefab.SetActive(false);
    //         BuildPlayingField(gamePos);
    //         _placedMap = true;
    //     }
    // }

    void DetachedMovePerson()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            Ray ray = mainCamera.ScreenPointToRay(touch.position);
            RaycastHit hitObject;

            if (Physics.Raycast(ray, out hitObject, _layerMask))
            {
                if (hitObject.collider.CompareTag("Person"))
                {
                    PersonNoAR currPerson = hitObject.collider.gameObject.GetComponent<PersonNoAR>();
                    if (currPerson.team == _currTeam)
                    {
                        if (!_personScr)
                        {
                            _personScr = currPerson;
                            _personScr.gameObject.layer = LayerMask.NameToLayer("Circles");
                            _layerMask = LayerMask.NameToLayer("Circles");
                            _personScr.GenerateMovements();
                        }
                        else
                        {
                            _personScr.DestroyCircles();
                            _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
                            _layerMask = LayerMask.NameToLayer("Person");
                            _personScr = null;
                        }
                    }
                }
                else if (hitObject.collider.CompareTag("Movement"))
                {
                    _personScr.Move(hitObject.collider.gameObject.transform.position);
                    _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
                    _layerMask = LayerMask.NameToLayer("Person");
                    _personScr = null;
                    if (_currTeam == Helpers.Teams.White)
                    {
                        _currTeam = Helpers.Teams.White;
                    }
                    else
                    {
                        _currTeam = Helpers.Teams.White;
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        foreach (GameObject gO in CurrentGame.GOCards)
        {
            Destroy(gO);
        }

        foreach (GameObject gO in CurrentGame.GOPersons)
        {
            Destroy(gO);
        }
    }

    void BuildPlayingField(Vector3 middleCardPosition)
    {
        MeshRenderer rendererCardPrefab = cardPrefab.GetComponent<MeshRenderer>();
        Vector3 sizeCardPrefab = rendererCardPrefab.bounds.size;

        float firstCardX = middleCardPosition.x - 6 * sizeCardPrefab.x;
        float firstCardY = middleCardPosition.y;
        float firstCardZ = middleCardPosition.z - 6 * sizeCardPrefab.z;
        Vector3 firstCardPosition = new Vector3(firstCardX, firstCardY, firstCardZ);

        for (int j = 0; j < CurrentGame.PlayingField.GetLength(1); j++)
        {
            for (int i = 0; i < CurrentGame.PlayingField.GetLength(0); i++)
            {
                float newX = firstCardPosition.x + i * sizeCardPrefab.x;
                float newY = firstCardPosition.y;
                float newZ = firstCardPosition.z + j * sizeCardPrefab.z;
                Vector3 newPosition = new Vector3(newX, newY, newZ);
                GameObject cardGO = Instantiate(cardPrefab, newPosition, Quaternion.identity);

                CurrentGame.GOCards[i, j] = cardGO;

                Card ownCard = CurrentGame.PlayingField[i, j];
                ownCard.OwnGO = cardGO;
                if (ownCard is WaterCard)
                {
                    ownCard.Open();

                    WaterCard ownWaterCard = ownCard as WaterCard;
                    if (ownWaterCard.OwnShip != null)
                    {
                        ownWaterCard.LoadShipLogo();
                    }
                }
            }
        }

        // Generate persons on ships
        for (int team = 0; team < CurrentGame.NumTeams; team++)
        {
            const int numPersonsInTeam = 1;
            PersonNoAR[] personsInTeam = new PersonNoAR[numPersonsInTeam];

            for (int player = 0; player < numPersonsInTeam; player++)
            {
                Helpers.IntVector2 shipPosition = Ships.AllShips[(Helpers.Teams)team].Position;
                float persX = firstCardX + shipPosition.x * sizeCardPrefab.x;
                float persY = firstCardY;
                float persZ = firstCardZ + shipPosition.z * sizeCardPrefab.z;
                Vector3 persPosition = new Vector3(persX, persY, persZ);

                GameObject personGO = Instantiate(placedObjectPrefab, persPosition, Quaternion.identity);
                personGO.SetActive(true);
                CurrentGame.GOPersons.Add(personGO);
                PersonNoAR pers = personGO.GetComponent<PersonNoAR>();
                pers.currGame = CurrentGame;
                pers.team = (Helpers.Teams)team;
                pers.Position = shipPosition;
                // Add person to card's list of persons
                for (int i = 0; i < 3; ++i)
                {
                    if (!CurrentGame.PlayingField[shipPosition.x, shipPosition.z].FiguresNoAR[i])
                    {
                        CurrentGame.PlayingField[shipPosition.x, shipPosition.z].FiguresNoAR[i] = pers;
                        break;
                    }
                }

                personsInTeam[player] = pers;
            }
            CurrentGame.PersonsNoAR.Add((Helpers.Teams)team, personsInTeam);
        }
    }
}
