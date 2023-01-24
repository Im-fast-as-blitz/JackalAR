using System.Collections.Generic;
using UnityEngine;
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
    public Vector3 firstCardPosition = new Vector3(0f, 0f, 0f);
    
    void Start()
    {
        CurrentGame = new Game();
        BuildPlayingField();
    }

    void OnApplicationQuit()
    {
        foreach (GameObject gO in CurrentGame.GOCards)
        {
            Destroy(gO);
        }
    }
    
    void BuildPlayingField()
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
