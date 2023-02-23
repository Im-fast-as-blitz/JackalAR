using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;


public class Game
{
    public const int PlayingFieldFirstDim = 13, PlayingFieldSecondDim = 13; // size of playing field
    public Card[,] PlayingField = new Card[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public GameObject[,] GOCards = new GameObject[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public Dictionary<Helpers.Teams, Person[]> Persons = new Dictionary<Helpers.Teams, Person[]>();
    public Dictionary<Helpers.Teams, PersonNoAR[]> PersonsNoAR = new Dictionary<Helpers.Teams, PersonNoAR[]>();
    public List<GameObject> GOPersons = new List<GameObject>();
    public int NumTeams = 2;
    public Vector3 sizeCardPrefab = new Vector3(0, 0, 0);

    public Game()
    {
        FillAndShufflePlayingField();
        PlaceShips();
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
            waterCard.Type = Card.CardType.Ship;
        }
    }
}