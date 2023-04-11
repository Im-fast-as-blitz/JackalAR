using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;


public class Game
{
    public const int PlayingFieldFirstDim = 13, PlayingFieldSecondDim = 13; // size of playing field
    public Card[,] PlayingField = new Card[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public GameObject[,] GOCards = new GameObject[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public Dictionary<Teams, Person[]> Persons = new Dictionary<Teams, Person[]>();
    public int NumTeams = 2;
    public Vector3 sizeCardPrefab = new Vector3(0, 0, 0);
    public Button ShamanBtn;

    public Vector3[,] TeemRotation = new Vector3[4, 3];

    public Button TakeCoinBtn;
    public Button PutCoinBtn;
    public int TotalCoins = 0;
    public int[] CoinsInTeam = new int[4];


    public Teams CurrTeam;

    public Game()
    {
        SelectTeemRotation();
        FillAndShufflePlayingField();
        PlaceShips();
    }

    private void SelectTeemRotation()
    {
        TeemRotation[(int)Teams.White, 0] = new Vector3(0, 0, 1);
        TeemRotation[(int)Teams.White, 1] = new Vector3(-1, 0, -1);
        TeemRotation[(int)Teams.White, 2] = new Vector3(1, 0, -1);
        
        TeemRotation[(int)Teams.Red, 0] = new Vector3(-1, 0, 0);
        TeemRotation[(int)Teams.Red, 1] = new Vector3(1, 0, -1);
        TeemRotation[(int)Teams.Red, 2] = new Vector3(1, 0, 1);
        
        TeemRotation[(int)Teams.Yellow, 0] = new Vector3(0, 0, -1);
        TeemRotation[(int)Teams.Yellow, 1] = new Vector3(1, 0, 1);
        TeemRotation[(int)Teams.Yellow, 2] = new Vector3(-1, 0, 1);
        
        TeemRotation[(int)Teams.Black, 0] = new Vector3(1, 0, 0);
        TeemRotation[(int)Teams.Black, 1] = new Vector3(-1, 0, 1);
        TeemRotation[(int)Teams.Black, 2] = new Vector3(-1, 0, -1);
    }

    private void FillAndShufflePlayingField()
    {
        // Fill by water cards
        int firstDim = PlayingFieldFirstDim;
        int secondDim = PlayingFieldSecondDim;
        for (int j = 0; j < secondDim; j += secondDim - 1)
        {
            for (int i = 0; i < firstDim; i++)
            {
                PlayingField[i, j] = new WaterCard();
            }
        }

        for (int j = 1; j < secondDim - 1; j++)
        {
            for (int i = 0; i < firstDim; i += firstDim - 1)
            {
                PlayingField[i, j] = new WaterCard();
            }
        }

        // Fill by other cards temporary array
        List<Card> cardsWithoutWater = new List<Card>();
        for (int i = 1; i < Cards.AllCards.Count; i++)
        {
            for (int remain = Cards.AllCards[i].Amount; remain > 0; --remain)
            {
                cardsWithoutWater.Add((Card)Cards.AllCards[i].CardPair.NewObj());
            }
        }

        // Shuffle temporary array
        for (int i = 0; i < cardsWithoutWater.Count; i++)
        {
            Card temp = cardsWithoutWater[i];
            int randomIndex = Random.Range(i, cardsWithoutWater.Count);
            cardsWithoutWater[i] = cardsWithoutWater[randomIndex];
            cardsWithoutWater[randomIndex] = temp;
        }

        // Fill Playing Field by temporary array
        int tempArrayInd = 0;
        for (int j = 1; j < secondDim - 1; j++)
        {
            for (int i = 1; i < firstDim - 1; i++)
            {
                if ((i == 1 && j == 1) || (i == firstDim - 2 && j == 1) || (i == 1 && j == secondDim - 2) ||
                    (i == firstDim - 2 && j == secondDim - 2))
                {
                    PlayingField[i, j] = new WaterCard();
                    continue;
                }

                PlayingField[i, j] = cardsWithoutWater[tempArrayInd];
                tempArrayInd++;
            }
        }
    }

    private void PlaceShips()
    {
        foreach (var pair in Ships.AllShips)
        {
            WaterCard waterCard = PlayingField[pair.Value.Position.x, pair.Value.Position.z] as WaterCard;
            if (waterCard == null)
            {
                throw new Exception("Wrong ship or water card position");
            }

            waterCard.OwnShip = pair.Value;
            waterCard.Type = CardType.Ship;
        }
    }
}

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
    public bool isGameAR = false;
    public bool isDebug = false;

    private ARRaycastManager _arRaycastManagerScript;
    private bool _placedMap = false;
    private Person _personScr;
    private LayerMask _layerMask;

    private Teams _currTeam = Teams.White;
    // private Person _currPerson = null;

    void Start()
    {
        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        _layerMask = 1 << LayerMask.NameToLayer("Person");

        CurrentGame = new Game();

        CurrentGame.ShamanBtn = shamanBtn;
        CurrentGame.TakeCoinBtn = takeCoinBtn;
        CurrentGame.PutCoinBtn = putCoinBtn;

        PersonManagerScr.currGame = CurrentGame;

        if (!isGameAR)
        {
            BuildPlayingField(new Vector3(0, 0, 0));
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
                    if (_currTeam == Teams.White || _currTeam == Teams.Yellow)
                    {
                        beautiPos = new Vector3(0.025f, 0, 0);
                    }
                    else
                    {
                        beautiPos = new Vector3(0, 0, 0.025f);
                    }
                    zombie.transform.position = per.gameObject.transform.position + 
                                                           new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 1].x * beautiPos.x, 0, beautiPos.z * CurrentGame.TeemRotation[(int)_currTeam, 1].z);;
                    per.transform.position += new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 2].x * beautiPos.x, 0, beautiPos.z * CurrentGame.TeemRotation[(int)_currTeam, 2].z);
                }
                else
                {
                    prev_pers.transform.position = per.transform.position + new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 2].x * 0.025f, 0, 0.025f * CurrentGame.TeemRotation[(int)_currTeam, 2].z);
                    transform.position = per.transform.position + new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 0].x * 0.025f, 0, 0.025f * CurrentGame.TeemRotation[(int)_currTeam, 0].z);
                    per.transform.position += new Vector3(CurrentGame.TeemRotation[(int)_currTeam, 1].x * 0.025f, 0, 0.025f * CurrentGame.TeemRotation[(int)_currTeam, 1].z);
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

    void BuildPlayingField(Vector3 middleCardPosition)
    {
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
                        ownCard.Type = CardType.ArrowStraight;
                        ownCard.LogoPath = "Cards/Arrows/straight";
                    }
                    else if (arrowType == 1)
                    {
                        ownCard.Type = CardType.ArrowDiagonal;
                        ownCard.LogoPath = "Cards/Arrows/diagonal";
                    }
                    else if (arrowType == 2)
                    {
                        ownCard.Type = CardType.ArrowStraight2;
                        ownCard.LogoPath = "Cards/Arrows/straight2";
                    }
                    else if (arrowType == 3)
                    {
                        ownCard.Type = CardType.ArrowDiagonal2;
                        ownCard.LogoPath = "Cards/Arrows/diagonal2";
                    }
                    else if (arrowType == 4)
                    {
                        ownCard.Type = CardType.Arrow3;
                        ownCard.LogoPath = "Cards/Arrows/3";
                    }
                    else if (arrowType == 5)
                    {
                        ownCard.Type = CardType.ArrowStraight4;
                        ownCard.LogoPath = "Cards/Arrows/straight4";
                    }
                    else if (arrowType == 6)
                    {
                        ownCard.Type = CardType.ArrowDiagonal4;
                        ownCard.LogoPath = "Cards/Arrows/diagonal4";
                    }

                    if (ownCard.Type != CardType.ArrowStraight4 && ownCard.Type != CardType.ArrowDiagonal4)
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

        // Generate persons on ships
        for (int team = 0; team < CurrentGame.NumTeams; team++)
        {
            const int numPersonsInTeam = 3;
            Person[] personsInTeam = new Person[numPersonsInTeam];

            for (int player = 0; player < numPersonsInTeam; player++)
            {
                IntVector2 shipPosition = Ships.AllShips[(Teams)team].Position;
                float persX = firstCardX + shipPosition.x * CurrentGame.sizeCardPrefab.x;
                float persY = firstCardY;
                float persZ = firstCardZ + shipPosition.z * CurrentGame.sizeCardPrefab.z;
                Vector3 beautiPos = CurrentGame.TeemRotation[team, player];
                Vector3 persPosition = new Vector3(persX + beautiPos.x * 0.025f, persY, persZ + beautiPos.z * 0.025f);
                
                GameObject personGO = Instantiate(placedObjectPrefab, persPosition, Quaternion.identity);
                personGO.SetActive(true);
                Person pers = personGO.GetComponent<Person>();
                pers.currGame = CurrentGame;
                pers.team = (Teams)team;
                pers.Position = new IntVector2(shipPosition);
                // Add person to card's list of persons
                CurrentGame.PlayingField[shipPosition.x, shipPosition.z].Figures[player] = pers;

                personsInTeam[player] = pers;
            }

            CurrentGame.Persons.Add((Teams)team, personsInTeam);
        }
    }
}