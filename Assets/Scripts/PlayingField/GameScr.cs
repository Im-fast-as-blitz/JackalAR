using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Photon.Pun;


public class Game
{
    public const int PlayingFieldFirstDim = 13, PlayingFieldSecondDim = 13; // size of playing field
    public Card[,] PlayingField = new Card[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public GameObject[,] GOCards = new GameObject[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public Dictionary<Teams, Person[]> Persons = new Dictionary<Teams, Person[]>();
    public int currentNumTeam = 0;
    public int NumTeams = 2;
    public Vector3 sizeCardPrefab = new Vector3(0, 0, 0);
    public Button ShamanBtn;

    public Vector3[,] TeemRotation = new Vector3[4, 3];

    public Button TakeCoinBtn;
    public Button PutCoinBtn;
    public int TotalCoins = 0;
    public int[] CoinsInTeam = new int[4];


    public Teams CurrTeam;

    public Game(bool isMaster)
    {
        SelectTeemRotation();
        if (isMaster)
        {
            FillAndShufflePlayingField();
            PlaceShips();
        }
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

    public void PlaceShips()
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