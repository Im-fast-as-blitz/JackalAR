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
    public int currentTeam = 0;
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

    private void RandmomCard(ref Card ownCard)
    {
        if (ownCard is ArrowCard)
        {
            ownCard = RandomArrowCard();
        }
        else if (ownCard is ChestCard)
        {
            RandomChestCard(ref ownCard);
        }
        else if (ownCard is CannonCard)
        {
            RandomCannonCard(ref ownCard);
        }
        else if (ownCard is TurntableCard)
        {
            ownCard = RandomTurntableCard();
        }
    }

    private ArrowCard RandomArrowCard()
    {
        ArrowCard arrowCard = new ArrowCard();
        int arrowType = Random.Range(0, 7);
        if (arrowType == 0)
        {
            arrowCard = new ArrowStraight();
        }
        else if (arrowType == 1)
        {
            arrowCard = new ArrowDiagonal();
        }
        else if (arrowType == 2)
        {
            arrowCard = new ArrowStraight2();
        }
        else if (arrowType == 3)
        {
            arrowCard = new ArrowDiagonal2();
        }
        else if (arrowType == 4)
        {
            arrowCard = new Arrow3();
        }
        else if (arrowType == 5)
        {
            arrowCard = new ArrowStraight4();
        }
        else if (arrowType == 6)
        {
            arrowCard = new ArrowDiagonal4();
        }

        if (arrowCard.Type != Card.CardType.ArrowStraight4 && arrowCard.Type != Card.CardType.ArrowDiagonal4)
        {
            arrowCard.Rotation = (Rotation)Random.Range(0, 4);
        }

        return arrowCard;
    }

    private void RandomChestCard(ref Card ownCard)
    {
        if (ChestCard.CardsCount < 5)
        {
            ownCard.Coins = 1;
            TotalCoins += 1;
        }
        else if (ChestCard.CardsCount < 10)
        {
            ownCard.Coins = 2;
            TotalCoins += 2;
        }
        else if (ChestCard.CardsCount < 13)
        {
            ownCard.Coins = 3;
            TotalCoins += 3;
        }
        else if (ChestCard.CardsCount < 15)
        {
            ownCard.Coins = 4;
            TotalCoins += 4;
        }
        else if (ChestCard.CardsCount < 16)
        {
            ownCard.Coins = 5;
            TotalCoins += 5;
        }
        else
        {
            int randomedCoins = Random.Range(1, 6);
            ownCard.Coins = randomedCoins;
            TotalCoins += randomedCoins;
        }

        ChestCard.CardsCount++;
    }

    private TurntableCard RandomTurntableCard()
    {
        ++TurntableCard.turntableCount;
        var turntableCount = TurntableCard.turntableCount;
        if (turntableCount <= 5)
        {
            return new TurntableCard2();
        }
        else if (turntableCount <= 9)
        {
            return new TurntableCard3();
        }
        else if (turntableCount <= 11)
        {
            return new TurntableCard4();
        }
        else
        {
            return new TurntableCard5();
        }
    }

    private void RandomCannonCard(ref Card cannonCard)
    {
        (cannonCard as CannonCard).Rotation = (Rotation)Random.Range(0, 4);
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
        int turntableCount = 0;
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

                RandmomCard(ref PlayingField[i, j]);
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