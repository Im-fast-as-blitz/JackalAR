using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;


public class Game
{
    public const int PlayingFieldFirstDim = 13, PlayingFieldSecondDim = 13; // size of playing field
    public Card[,] PlayingField = new Card[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public GameObject[,] GOCards = new GameObject[PlayingFieldFirstDim, PlayingFieldSecondDim];

    public Game()
    {
        FillAndShufflePlayingField();
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
        for (int i = 0; i < cardsWithoutWater.Count; i++) {
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
}

public class GameManagerScr : MonoBehaviour
{
    public Game CurrentGame;
    public GameObject cardPrefab;
    
    [SerializeField] private GameObject planeMarkerPrefab;
    [SerializeField] private GameObject placedObjectPrefab;
    [SerializeField] private Camera arCamera;
    [SerializeField] private GameObject startText;

    private ARRaycastManager _arRaycastManagerScript;
    private bool _placedMap = false;
    private Person _personScr;
    private LayerMask _layerMask;

    private Person.Command _currCommand = Person.Command.GREEN;

    void Start()
    {
        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        _layerMask = LayerMask.NameToLayer("Person");
        
        CurrentGame = new Game();

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
            planeMarkerPrefab.transform.position = hits[0].pose.position + new Vector3(0, 0.01f, 0);
            planeMarkerPrefab.SetActive(true);
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            startText.SetActive(false);
            Vector3 gamePos = hits[0].pose.position + new Vector3(0, 0.01f, 0);
            GameObject person = Instantiate(placedObjectPrefab, gamePos, Quaternion.identity);
            person.GetComponent<Person>().currGame = CurrentGame;
            person.SetActive(true);
            
            //Enemy
            person = Instantiate(placedObjectPrefab, gamePos, Quaternion.identity);
            //person.GetComponent<Renderer>().material = Resources.Load("Person/Red", typeof(Material)) as Material;
            person.GetComponent<Person>().currGame = CurrentGame;
            person.GetComponent<Person>().commandType = Person.Command.RED;
            person.SetActive(true);
            //
            
            planeMarkerPrefab.SetActive(false);
            BuildPlayingField(gamePos);
            _placedMap = true;
        }
    }

    void DetachedMovePerson()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            Ray ray = arCamera.ScreenPointToRay(touch.position);
            RaycastHit hitObject;

            if (Physics.Raycast(ray, out hitObject, _layerMask))
            {
                if (hitObject.collider.CompareTag("Person"))
                {
                    Person currPerson = hitObject.collider.gameObject.GetComponent<Person>();
                    if (currPerson.commandType == _currCommand)
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
                } else if (hitObject.collider.CompareTag("Movement"))
                {
                    _personScr.Move(hitObject.collider.gameObject.transform.position);
                    _personScr.gameObject.layer = LayerMask.NameToLayer("Person");
                    _layerMask = LayerMask.NameToLayer("Person");
                    _personScr = null;
                    if (_currCommand == Person.Command.GREEN)
                    {
                        _currCommand = Person.Command.RED;
                    }
                    else
                    {
                        _currCommand = Person.Command.GREEN;
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
    }
    
    void BuildPlayingField(Vector3 firstCardPosition)
    {
        MeshRenderer rendererCardPrefab = cardPrefab.GetComponent<MeshRenderer>();
        Vector3 sizeCardPrefab = rendererCardPrefab.bounds.size;

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
                
                CardGOInfo gOInfo = cardGO.GetComponent<CardGOInfo>();
                gOInfo.FieldPosition = new IntVector2(i, j);

                Card ownCard = CurrentGame.PlayingField[i, j];
                gOInfo.OwnCard = ownCard;
                ownCard.OwnGO = cardGO;
                if (ownCard is WaterCard)
                {
                    ownCard.Open();
                }
            }
        }
    }
}
