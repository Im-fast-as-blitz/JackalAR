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
    [SerializeField] public RpcConnector rpcConnector;
    public bool isGameAR = false;
    public bool isDebug = false;

    private ARRaycastManager _arRaycastManagerScript;
    private bool _placedMap = false;
    private Person _personScr;
    private LayerMask _layerMask;

    private Teams _currTeam = Teams.White;
    private Person _currPerson = null;

    private Vector3 midCardPosition;

    void Start()
    {
        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        _layerMask = 1 << LayerMask.NameToLayer("Person");

        CurrentGame = new Game(PhotonNetwork.IsMasterClient);

        CurrentGame.ShamanBtn = shamanBtn;
        CurrentGame.TakeCoinBtn = takeCoinBtn;
        CurrentGame.PutCoinBtn = putCoinBtn;
        
        rpcConnector.SetGameObj(CurrentGame);

        PersonManagerScr.currGame = CurrentGame;

        if (!isGameAR)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                BuildPlayingField(new Vector3(0, 0, 0));
                CreateTeam();
            }

            _placedMap = true;
            arCamera.transform.position = new Vector3(0, 2f, 0);
            //arCamera.transform.position = new Vector3(0, 1.25f, 0);
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

    void ChangeTeam()
    {
        if (_currTeam == Teams.White)
        {
            _currTeam = Teams.Red;
        }
        else
        {
            _currTeam = Teams.White;
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
                    if (currentPerson.team == _currTeam)
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
                    ChangeTeam();
                }
            }
        }
    }

    public void RevivePerson()
    {
        Person zombie = null;
        foreach (var per in CurrentGame.Persons[_currTeam])
        {
            if (!per._isAlive)
            {
                zombie = per;
                break;
            }
        }

        Person prev_pers = null;
        int teammates_count = 0;
        foreach (var per in CurrentGame.Persons[_currTeam])
        {
            if (CurrentGame.PlayingField[per.Position.x, per.Position.z].Type == Card.CardType.Shaman)
            {
                ++teammates_count;
                if (teammates_count == 1)
                {
                    prev_pers = per;
                    zombie.Position = new IntVector2(per.Position);
                    zombie.gameObject.SetActive(true);
                    zombie._isAlive = true;

                    Vector3 beautiPos;
                    if (_currTeam == Teams.White || _currTeam == Teams.Yellow)
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
        ChangeTeam();
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
                else if (ownCard is CannonCard)
                {
                    (ownCard as CannonCard).Rotation = (Rotation)Random.Range(0, 4);
                }
                else if (ownCard is ArrowCard)
                {
                    int arrowType = Random.Range(0, 7);
                    if (arrowType == 0)
                    {
                        ownCard.Type = Card.CardType.ArrowStraight;
                        ownCard.LogoPath = "Cards/Arrows/straight";
                    }
                    else if (arrowType == 1)
                    {
                        ownCard.Type = Card.CardType.ArrowDiagonal;
                        ownCard.LogoPath = "Cards/Arrows/diagonal";
                    }
                    else if (arrowType == 2)
                    {
                        ownCard.Type = Card.CardType.ArrowStraight2;
                        ownCard.LogoPath = "Cards/Arrows/straight2";
                    }
                    else if (arrowType == 3)
                    {
                        ownCard.Type = Card.CardType.ArrowDiagonal2;
                        ownCard.LogoPath = "Cards/Arrows/diagonal2";
                    }
                    else if (arrowType == 4)
                    {
                        ownCard.Type = Card.CardType.Arrow3;
                        ownCard.LogoPath = "Cards/Arrows/3";
                    }
                    else if (arrowType == 5)
                    {
                        ownCard.Type = Card.CardType.ArrowStraight4;
                        ownCard.LogoPath = "Cards/Arrows/straight4";
                    }
                    else if (arrowType == 6)
                    {
                        ownCard.Type = Card.CardType.ArrowDiagonal4;
                        ownCard.LogoPath = "Cards/Arrows/diagonal4";
                    }

                    if (ownCard.Type != Card.CardType.ArrowStraight4 && ownCard.Type != Card.CardType.ArrowDiagonal4)
                    {
                        (ownCard as ArrowCard).Rotation = (Rotation)Random.Range(0, 4);
                    }
                }
                else if (ownCard is ChestCard)
                {
                    if (ChestCard.CardsCount < 5)
                    {
                        (ownCard as ChestCard).Coins = 1;
                        CurrentGame.TotalCoins += 1;
                    }
                    else if (ChestCard.CardsCount < 10)
                    {
                        (ownCard as ChestCard).Coins = 2;
                        CurrentGame.TotalCoins += 2;
                    }
                    else if (ChestCard.CardsCount < 13)
                    {
                        (ownCard as ChestCard).Coins = 3;
                        CurrentGame.TotalCoins += 3;
                    }
                    else if (ChestCard.CardsCount < 15)
                    {
                        (ownCard as ChestCard).Coins = 4;
                        CurrentGame.TotalCoins += 4;
                    }
                    else if (ChestCard.CardsCount < 16)
                    {
                        (ownCard as ChestCard).Coins = 5;
                        CurrentGame.TotalCoins += 5;
                    }
                    else
                    {
                        int randomedCoins = Random.Range(1, 6);
                        (ownCard as ChestCard).Coins = randomedCoins;
                        CurrentGame.TotalCoins += randomedCoins;
                    }

                    ChestCard.CardsCount++;
                }
                else if (ownCard is TurntableCard)
                {
                    ++turntable_count;
                    if (turntable_count <= 5)
                    {
                        (ownCard as TurntableCard).StepCount = 2;
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(0.03f, 0, 0.03f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(-0.03f, 0, -0.03f));
                    }
                    else if (turntable_count <= 9)
                    {
                        ownCard.LogoPath = "Cards/Turntables/3-steps";
                        (ownCard as TurntableCard).StepCount = 3;
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(-0.03f, 0, 0.03f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(0.01f, 0, 0));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(-0.03f, 0, -0.03f));
                    }
                    else if (turntable_count <= 11)
                    {
                        ownCard.LogoPath = "Cards/Turntables/4-steps";
                        (ownCard as TurntableCard).StepCount = 4;
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(-0.03f, 0, 0.035f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(0.03f, 0, 0.015f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(-0.025f, 0, -0.02f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(+0.02f, 0, -0.035f));
                    }
                    else
                    {
                        ownCard.LogoPath = "Cards/Turntables/5-steps";
                        (ownCard as TurntableCard).StepCount = 5;
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(-0.035f, 0, -0.03f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(0, 0, -0.03f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(0.03f, 0, -0.01f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(0.02f, 0, 0.03f));
                        (ownCard as TurntableCard).StepPos.Add(new Vector3(-0.035f, 0, 0.03f));
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

        for (var currentTeam = CurrentGame.currentTeam; currentTeam < PhotonNetwork.PlayerList.Length; ++currentTeam)
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
                pers.team = (Teams)currentTeam;
                pers.Position = new IntVector2(shipPosition);
                // Add person to card's list of persons
                CurrentGame.PlayingField[shipPosition.x, shipPosition.z].Figures[player] = pers;

                personsInTeam[player] = pers;
            }

            CurrentGame.Persons.Add((Teams)currentTeam, personsInTeam);
        }

        CurrentGame.currentTeam = PhotonNetwork.PlayerList.Length;
    }
}