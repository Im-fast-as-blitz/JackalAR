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

    private ARRaycastManager _arRaycastManagerScript;
    private bool _placedMap = false;
    private Person _personScr;
    
    void Start()
    {
        CurrentGame = new Game();
        _arRaycastManagerScript = FindObjectOfType<ARRaycastManager>();
        
        planeMarkerPrefab.SetActive(false);
        //placedObjectPrefab.SetActive(false);
        //BuildPlayingField(new Vector3(0, 0, 0));
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
            planeMarkerPrefab.transform.position = hits[0].pose.position;
            planeMarkerPrefab.SetActive(true);
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Vector3 gamePos = hits[0].pose.position + new Vector3(0, 0.1f, 0);
            GameObject person = Instantiate(placedObjectPrefab, gamePos, Quaternion.identity);
            person.GetComponent<Person>().currGame = CurrentGame;
            person.SetActive(true);
            planeMarkerPrefab.SetActive(false);
            BuildPlayingField(gamePos);
            _placedMap = true;
        }
    }

    void DetachedMovePerson()
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
                    _personScr = hitObject.collider.gameObject.GetComponent<Person>();
                    _personScr.GenerateMovements();
                } else if (hitObject.collider.CompareTag("Movement"))
                {
                    _personScr.CleanUpMovements(hitObject.collider.gameObject.transform.position);
                    _personScr = null;
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
                    cardGO.GetComponent<CardGOBehaviourScr>().Open();
                }
            }
        }
    }
}
