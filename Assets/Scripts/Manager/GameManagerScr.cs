using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Photon.Pun;
using UnityEditor;


public class GameManagerScr : MonoBehaviour
{
    public Game CurrentGame;
    public GameObject cardPrefab;

    [SerializeField] private GameObject planeMarkerPrefab;
    [SerializeField] private GameObject placedObjectPrefab;
    [SerializeField] private Camera arCamera;
    [SerializeField] private GameObject startText;
    [SerializeField] private Button shamanBtn;
    [SerializeField] private Button takeCoinBtn;
    [SerializeField] private Button putCoinBtn;
    [SerializeField] private Button suicideBtn;
    [SerializeField] public RpcConnector rpcConnector;
    public bool isGameAR = false;
    public bool isDebug = false;
    // Change only in single playet mode
    public int numTeams = 1; 

    private ARRaycastManager _arRaycastManagerScript;
    private bool _placedMap = false;
    private Person _personScr;
    private LayerMask _layerMask;

    // private Person _currPerson = null;
    private Person _currPerson = null;
    // private Teams _currTeam = Teams.White;
    private Teams _currTeam = Teams.White;

    private Vector3 midCardPosition;

    void Start()
    {
        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        _layerMask = 1 << LayerMask.NameToLayer("Person");

        CurrentGame = new Game(PhotonNetwork.IsMasterClient);

        CurrentGame.ShamanBtn = shamanBtn;
        CurrentGame.TakeCoinBtn = takeCoinBtn;
        CurrentGame.PutCoinBtn = putCoinBtn;
        CurrentGame.SuicideBtn = suicideBtn;

        rpcConnector.SetGameObj(CurrentGame);

        PersonManagerScr.currGame = CurrentGame;

        if (!isGameAR)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                BuildPlayingField(new Vector3(0, 0, 0));
                CreateTeam();
                rpcConnector.SyncCardsRpc(CurrentGame.rotMassSize);
            }

            _placedMap = true;
            arCamera.transform.position = new Vector3(0, 2f, 0);
        }

        planeMarkerPrefab.SetActive(false);
    }

    void Update()
    {
        if (!_placedMap)
        {
            ShowMarker();
        }
        else
        {
            DetachedMovePerson();
        }
    }

    void ShowMarker()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        _arRaycastManagerScript.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);
        if (hits.Count > 0)
        {
            planeMarkerPrefab.transform.position = hits[0].pose.position + new Vector3(0, 0.03f, 0);
            planeMarkerPrefab.SetActive(true);
        }

        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            startText.SetActive(false);
            Vector3 gamePos = hits[0].pose.position + new Vector3(0, 0.03f, 0);


            planeMarkerPrefab.SetActive(false);
            BuildPlayingField(gamePos);
            CreateTeam();
            _placedMap = true;
        }
    }

    

    void DetachedMovePerson()
    {
        if ((Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) ||
            (isDebug && Input.GetMouseButtonDown(0)))
        {
            Ray ray;
            if (!isDebug)
            {
                Touch touch = Input.GetTouch(0);
                ray = arCamera.ScreenPointToRay(touch.position);
            }
            else
            {
                ray = arCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            }

            RaycastHit hitObject;

            if (Physics.Raycast(ray, out hitObject, Mathf.Infinity, _layerMask))
            {
                if (hitObject.collider.CompareTag("Person"))
                {
                    Person currentPerson = hitObject.collider.gameObject.GetComponent<Person>();
                    if (currentPerson.team == CurrentGame.curTeam)
                    {
                        if (!_personScr)
                        {
                            _personScr = currentPerson;
                            _personScr.gameObject.layer = LayerMask.NameToLayer("Circles");
                            _layerMask = 1 << LayerMask.NameToLayer("Circles");
                            _personScr.GenerateMovements();
                        }
                        else
                        {
                            // put coin back from prev person
                            if (_personScr.isWithCoin)
                            {
                                _personScr.isWithCoin = false;
                                CurrentGame.PlayingField[_personScr.Position.x, _personScr.Position.z].Coins++;
                            }
                            
                            suicideBtn.gameObject.SetActive(false);

                            _personScr.DestroyCircles();
                            _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
                            _layerMask = 1 << LayerMask.NameToLayer("Person");
                            _personScr = null;
                        }
                    }
                }
                else if (hitObject.collider.CompareTag("Movement"))
                {
                    _personScr.Move(hitObject.collider.gameObject.transform.position);
                    _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
                    _layerMask = 1 << LayerMask.NameToLayer("Person");
                    _personScr = null;
                }
            }
        }
    }

    public void RevivePerson()
    {
        Person zombie = null;
        foreach (var per in CurrentGame.Persons[CurrentGame.curTeam])
        {
            if (!per._isAlive)
            {
                zombie = per;
                break;
            }
        }

        Person prev_pers = null;
        int teammates_count = 0;
        foreach (var per in CurrentGame.Persons[CurrentGame.curTeam])
        {
            if (CurrentGame.PlayingField[per.Position.x, per.Position.z].Type == CardType.Shaman)
            {
                ++teammates_count;
                if (teammates_count == 1)
                {
                    prev_pers = per;
                    zombie.Position = new IntVector2(per.Position);
                    zombie.gameObject.SetActive(true);
                    zombie._isAlive = true;

                    Vector3 beautiPos;
                    if (CurrentGame.curTeam == Teams.White || CurrentGame.curTeam == Teams.Yellow)
                    {
                        beautiPos = new Vector3(0.025f, 0, 0);
                    }
                    else
                    {
                        beautiPos = new Vector3(0, 0, 0.025f);
                    }

                    zombie.transform.position = per.gameObject.transform.position +
                                                new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 1].x * beautiPos.x,
                                                    0, beautiPos.z * CurrentGame.TeemRotation[(int)_currTeam, 1].z);
                    ;
                    per.transform.position += new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 2].x * beautiPos.x,
                        0, beautiPos.z * CurrentGame.TeemRotation[(int)_currTeam, 2].z);
                }
                else
                {
                    prev_pers.transform.position = per.transform.position +
                                                   new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 2].x * 0.025f,
                                                       0, 0.025f * CurrentGame.TeemRotation[(int)_currTeam, 2].z);
                    transform.position = per.transform.position +
                                         new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 0].x * 0.025f, 0,
                                             0.025f * CurrentGame.TeemRotation[(int)_currTeam, 0].z);
                    per.transform.position += new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 1].x * 0.025f, 0,
                        0.025f * CurrentGame.TeemRotation[(int)_currTeam, 1].z);
                }
            }
        }

        _personScr.DestroyCircles();
        _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
        _layerMask = 1 << LayerMask.NameToLayer("Person");
        _personScr = null;
        CurrentGame.ChangeTeam();
    }

    public void TakeCoin()
    {
        _personScr.isWithCoin = true;
        CurrentGame.PlayingField[_personScr.Position.x, _personScr.Position.z].Coins--;
        CurrentGame.TakeCoinBtn.gameObject.SetActive(false);
        CurrentGame.PutCoinBtn.gameObject.SetActive(true);
        _personScr.DestroyCircles(false);
        _personScr.GenerateMovements(false);
    }

    public void PutCoin()
    {
        _personScr.isWithCoin = false;
        CurrentGame.PlayingField[_personScr.Position.x, _personScr.Position.z].Coins++;
        CurrentGame.PutCoinBtn.gameObject.SetActive(false);
        CurrentGame.TakeCoinBtn.gameObject.SetActive(true);
        _personScr.DestroyCircles(false);
        _personScr.GenerateMovements(false);
    }

    public void Suicide()
    {
        _personScr.Death();
        for (int i = 0; i < 3; i++)
        {
            if (CurrentGame.PlayingField[_personScr.Position.x, _personScr.Position.z].Figures[i] == _personScr)
            {
                CurrentGame.PlayingField[_personScr.Position.x, _personScr.Position.z].Figures[i] = null;
            }
        }
        _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
        _layerMask = 1 << LayerMask.NameToLayer("Person");
        _personScr = null;
        CurrentGame.SuicideBtn.gameObject.SetActive(false);
    }

    public void BuildPlayingField(Vector3 middleCardPosition)
    {
        midCardPosition = middleCardPosition;
        MeshRenderer rendererCardPrefab = cardPrefab.GetComponent<MeshRenderer>();
        CurrentGame.sizeCardPrefab = rendererCardPrefab.bounds.size;

        float firstCardX = middleCardPosition.x - 6 * CurrentGame.sizeCardPrefab.x;
        float firstCardY = middleCardPosition.y;
        float firstCardZ = middleCardPosition.z - 6 * CurrentGame.sizeCardPrefab.z;
        Vector3 firstCardPosition = new Vector3(firstCardX, firstCardY, firstCardZ);

        for (int j = 0, turntable_count = 0; j < CurrentGame.PlayingField.GetLength(1); j++)
        {
            for (int i = 0; i < CurrentGame.PlayingField.GetLength(0); i++)
            {
                Card ownCard = CurrentGame.PlayingField[i, j];
                float newX = firstCardPosition.x + i * CurrentGame.sizeCardPrefab.x;
                float newY = firstCardPosition.y;
                float newZ = firstCardPosition.z + j * CurrentGame.sizeCardPrefab.z;
                Vector3 newPosition = new Vector3(newX, newY, newZ);

                GameObject cardGO = Instantiate(cardPrefab, newPosition, Quaternion.identity);

                CurrentGame.GOCards[i, j] = cardGO;
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
    }

    // Generate persons on ships
    public void CreateTeam()
    {
        float firstCardX = midCardPosition.x - 6 * CurrentGame.sizeCardPrefab.x;
        float firstCardY = midCardPosition.y;
        float firstCardZ = midCardPosition.z - 6 * CurrentGame.sizeCardPrefab.z;

        Debug.Log(PhotonNetwork.PlayerList.Length);
        for (var currentTeam = CurrentGame.currentNumTeam; currentTeam < PhotonNetwork.PlayerList.Length + numTeams - 1; ++currentTeam)
        {
            const int numPersonsInTeam = 3;
            Person[] personsInTeam = new Person[numPersonsInTeam];

            for (int player = 0; player < numPersonsInTeam; player++)
            {
                IntVector2 shipPosition = Ships.AllShips[(Teams)currentTeam].Position;
                float persX = firstCardX + shipPosition.x * CurrentGame.sizeCardPrefab.x;
                float persY = firstCardY;
                float persZ = firstCardZ + shipPosition.z * CurrentGame.sizeCardPrefab.z;
                Vector3 beautiPos = CurrentGame.TeemRotation[currentTeam, player];
                Vector3 persPosition = new Vector3(persX + beautiPos.x * 0.025f, persY, persZ + beautiPos.z * 0.025f);

                GameObject personGO = Instantiate(placedObjectPrefab, persPosition, Quaternion.identity);
                personGO.SetActive(true);
                Person pers = personGO.GetComponent<Person>();
                
                pers.currGame = CurrentGame;
                pers.rpcConnector = rpcConnector;
                pers.team = (Teams)currentTeam;
                pers.Position = new IntVector2(shipPosition);
                pers.personNumber = player;
                
                // Add person to card's list of persons
                CurrentGame.PlayingField[shipPosition.x, shipPosition.z].Figures[player] = pers;

                personsInTeam[player] = pers;
            }

            CurrentGame.Persons.Add((Teams)currentTeam, personsInTeam);
        }

        CurrentGame.currentNumTeam = PhotonNetwork.PlayerList.Length;
    }
}