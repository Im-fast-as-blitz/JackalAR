using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Photon.Pun;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;


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
    [SerializeField] public GameObject endGameTitle;
    [SerializeField] public GameObject currTeamTitle;
    public bool isGameAR = false;

    public bool isDebug = false;

    // Change only in single playet mode
    public int numTeams = 1;

    private ARRaycastManager _arRaycastManagerScript;
    private bool _placedMap = false;
    public Person _personScr;
    public LayerMask _layerMask;

    // private Person _currPerson = null;
    private Person _currPerson = null;

    // private Teams _currTeam = Teams.White;
    private Teams _currTeam = Teams.White;

    private Vector3 midCardPosition;

    private Teams _userTeam;

    void Start()
    {
        _userTeam = (Teams)(PhotonNetwork.PlayerList.Length - 1);
        isGameAR = SceneManager.GetActiveScene().name == "GameAR";
        
        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        _layerMask = 1 << LayerMask.NameToLayer("Person");
        
        CurrentGame = new Game(PhotonNetwork.IsMasterClient);

        if (isDebug)
        {
            numTeams = PhotonNetwork.CurrentRoom.MaxPlayers;
        }

        CurrentGame.ShamanBtn = shamanBtn;
        CurrentGame.TakeCoinBtn = takeCoinBtn;
        CurrentGame.PutCoinBtn = putCoinBtn;
        CurrentGame.SuicideBtn = suicideBtn;
        CurrentGame.EndGameTitle = endGameTitle;
        CurrentGame.CurrTeamTitle = currTeamTitle;

        rpcConnector.SetGameObj(CurrentGame);

        PersonManagerScr.currGame = CurrentGame;

        if (!isGameAR)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                BuildPlayingField(new Vector3(0, 0, 0));
                CreateTeam();
                rpcConnector.SyncCardsRpc();
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
            CurrentGame.addPositionInGame = hits[0].pose.position + new Vector3(0, 0.03f, 0);


            planeMarkerPrefab.SetActive(false);
            _placedMap = true;
            BuildPlayingField(CurrentGame.addPositionInGame);
            CreateTeam();
            if (PhotonNetwork.IsMasterClient)
            {
                rpcConnector.SyncCardsRpc();
            }
        }
    }

    
    public void EndRound(int currTeamRound)
    {
        if (_personScr)
        {
            if (LayerMask.LayerToName(_personScr.gameObject.layer) == "Circles")
            {
                _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
            }

            _layerMask = 1 << LayerMask.NameToLayer("Person");
        }
        if (!CurrentGame.ShouldMove)
        {
            // find drunk persons
            int teamMask = 1 << currTeamRound;
            if ((CurrentGame.drunkTeams & teamMask) != 0)
            {
                bool flag = true;
                foreach (var per in CurrentGame.Persons[(Teams)currTeamRound])
                {
                    if (per.drunkCount > 0)
                    {
                        if (--per.drunkCount == 0)
                        {
                            per.gameObject.layer = LayerMask.NameToLayer("Person");
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                }

                if (flag)
                {
                    CurrentGame.drunkTeams -= teamMask;
                }
            }
            _personScr = null;
        
            CurrentGame.ChangeTeam();
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
                    if (currentPerson.team == CurrentGame.curTeam && (isDebug || _userTeam == CurrentGame.curTeam))
                    {
                        if (CurrentGame.ShouldMove && currentPerson != CurrentGame.ShouldMove)
                        {
                            return;
                        }
                        
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
                                IncCoins(_personScr.Position.x, _personScr.Position.z);
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
                    rpcConnector.MovePersonRpc(hitObject.collider.gameObject.transform.position - CurrentGame.addPositionInGame, 
                        _personScr.team, _personScr.personNumber);
                }
            }
        }
    }

    public void CalledRevivePerson()
    {
        rpcConnector.RevivePersonRpc();
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
                    if (CurrentGame.curTeam == Teams.White || CurrentGame.curTeam == Teams.Black || 
                        (Game.MaxCountInRoom == 2 && CurrentGame.curTeam == Teams.Red))
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
        EndRound((int)_personScr.team);
    }

    public void TakeCoin()
    {
        rpcConnector.TakeCoinPersonRpc(_personScr);
        CurrentGame.PutCoinBtn.gameObject.SetActive(true);
        CurrentGame.TakeCoinBtn.gameObject.SetActive(false);
        
        // _personScr.isWithCoin = true;
        // DecCoins();
        // CurrentGame.TakeCoinBtn.gameObject.SetActive(false);
        // CurrentGame.PutCoinBtn.gameObject.SetActive(true);
        // _personScr.DestroyCircles(false);
        // _personScr.GenerateMovements(false);
    }

    public void PutCoin()
    {
        rpcConnector.PutCoinPersonRpc(_personScr);
        CurrentGame.PutCoinBtn.gameObject.SetActive(false);
        CurrentGame.TakeCoinBtn.gameObject.SetActive(true);
        
        // _personScr.isWithCoin = false;
        // Card currCard = CurrentGame.PlayingField[_personScr.Position.x, _personScr.Position.z];
        // IncCoins();
        // CurrentGame.PutCoinBtn.gameObject.SetActive(false);
        // CurrentGame.TakeCoinBtn.gameObject.SetActive(true);
        // _personScr.DestroyCircles(false);
        // _personScr.GenerateMovements(false);
    }

    public void Suicide()
    {
        rpcConnector.SuicidePersonRpc(_personScr);
    }

    public void BuildPlayingField(Vector3 middleCardPosition)
    {
        if (isGameAR && !_placedMap)
        {
            return;
        }
        
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


                GameObject cardGO = Instantiate(cardPrefab, newPosition, Quaternion.Euler(0, 180, 0));

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
        if (isGameAR && !_placedMap)
        {
            return;
        }
        
        float firstCardX = midCardPosition.x - 6 * CurrentGame.sizeCardPrefab.x;
        float firstCardY = midCardPosition.y;
        float firstCardZ = midCardPosition.z - 6 * CurrentGame.sizeCardPrefab.z;

        
        Debug.Log(PhotonNetwork.PlayerList.Length);
        for (var currentTeam = CurrentGame.currentNumTeam;
             currentTeam < PhotonNetwork.PlayerList.Length + numTeams - 1;
             ++currentTeam)
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

                string currMaterial = "Persons/";
                switch (currentTeam)
                {
                    case 0:
                        currMaterial += "white";
                        break;
                    case 1:
                        currMaterial += "red";
                        break;
                    case 2:
                        currMaterial += "black";
                        break;
                    case 3:
                        currMaterial += "yellow";
                        break;

                }
                personGO.transform.GetChild(1).GetComponent<Renderer>().material =
                    Resources.Load(currMaterial, typeof(Material)) as Material;
                personGO.transform.GetChild(2).GetComponent<Renderer>().material =
                    Resources.Load(currMaterial, typeof(Material)) as Material;
                
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

    public void DecCoins(int x, int z)
    {
        Card currCard = CurrentGame.PlayingField[x, z];
        currCard.Coins--;
        if (currCard.Coins == 0)
        {
            Destroy(currCard.CoinGO);
            currCard.CoinGO = null;
        }
        else
        {
            currCard.CoinGO.transform.GetChild(0).GetComponent<TextMeshPro>().text = currCard.Coins.ToString();
        }
    }

    public void IncCoins(int x, int z)
    {
        Card currCard = CurrentGame.PlayingField[x, z];
        currCard.Coins++;
        if (currCard.Coins == 1)
        {
            GameObject coinGO = Resources.Load("Prefabs/coin", typeof(GameObject)) as GameObject;
            currCard.CoinGO = Instantiate(coinGO, currCard.OwnGO.transform.position, Quaternion.Euler(0, 180, 0));
            currCard.CoinGO.transform.GetChild(0).GetComponent<TextMeshPro>().text = "1";
        }
        else
        {
            currCard.CoinGO.transform.GetChild(0).GetComponent<TextMeshPro>().text = currCard.Coins.ToString();
        }
    }
}
