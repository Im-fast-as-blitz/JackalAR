using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Photon.Pun;
using TMPro;


public class Game
{
    public const int PlayingFieldFirstDim = 13, PlayingFieldSecondDim = 13; // size of playing field
    public Card[,] PlayingField = new Card[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public GameObject[,] GOCards = new GameObject[PlayingFieldFirstDim, PlayingFieldSecondDim];
    public Dictionary<Teams, Person[]> Persons = new Dictionary<Teams, Person[]>();
    public int currentNumTeam = 0;
    public Teams curTeam = Teams.White;
    public int NumTeams = 2;
    public static int MaxCountInRoom = 0;
    public Vector3 sizeCardPrefab = new Vector3(0, 0, 0);
    public Button ShamanBtn;
    public int rotMassSize = 0;

    public Vector3[,] TeemRotation = new Vector3[4, 3];

    public GameObject EndGameTitle;
    public GameObject CurrTeamTitle;
    public GameObject CurrCoinTitle;
    public Button TakeCoinBtn;
    public Button PutCoinBtn;
    public int TotalCoins = 0;
    public int[] CoinsInTeam = new int[4];
    public int drunkTeams = 0;
    public bool IsGameEnded = false;

    public Teams playerTeam;

    public Button SuicideBtn;
    public Person ShouldMove = null;

    public List<String> teamNames = Enum.GetNames(typeof(Teams)).ToList();

    public Vector3 addPositionInGame = new Vector3(0, 0, 0);
    
    // special cards
    List<int> TurntablesSizes = new List<int>();
    List<int> ChestsCoins = new List<int>();

    public Game(bool isMaster)
    {
        playerTeam = (Teams)(PhotonNetwork.PlayerList.Length - 1);
        MaxCountInRoom = PhotonNetwork.CurrentRoom.MaxPlayers;
        SelectTeemRotation();
        if (isMaster)
        {
            FillAndShufflePlayingField();
            PlaceShips();
        }
    }

    public void ChangeTeam()
    {
        if (MaxCountInRoom == 2)
        {
            curTeam = curTeam == Teams.White ? Teams.Red : Teams.White;
        }
        else
        {
            curTeam = (Teams)(((int)curTeam + 1) % MaxCountInRoom);
        }

        CurrTeamTitle.GetComponent<Text>().text = "Now Turn: " + teamNames[(int)curTeam];
    }

    private void RandomCard(ref Card ownCard)
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

        if (arrowCard.Type != CardType.ArrowStraight4 && arrowCard.Type != CardType.ArrowDiagonal4)
        {
            arrowCard.Rotation = (Rotation)Random.Range(0, 4);
            ++rotMassSize;
        }

        return arrowCard;
    }

    private void RandomChestCard(ref Card ownCard)
    {
        int coins = ChestsCoins.Last();
        ChestsCoins.RemoveAt(ChestsCoins.Count - 1);
        ownCard.Coins = coins;
        TotalCoins += coins;

        ++rotMassSize;
    }

    private TurntableCard RandomTurntableCard()
    {
        Debug.Log(TurntablesSizes.Count);
        int turntableSize = TurntablesSizes.Last();
        TurntablesSizes.RemoveAt(TurntablesSizes.Count - 1);
        if (turntableSize == 2)
        {
            return new TurntableCard2();
        }
        else if (turntableSize == 3)
        {
            return new TurntableCard3();
        }
        else if (turntableSize == 4)
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
        ++rotMassSize;
    }

    private void SelectTeemRotation()
    {
        TeemRotation[(int)Teams.White, 0] = new Vector3(0, 0, 1);
        TeemRotation[(int)Teams.White, 1] = new Vector3(-1, 0, -1);
        TeemRotation[(int)Teams.White, 2] = new Vector3(1, 0, -1);

        if (MaxCountInRoom == 2)
        {
            TeemRotation[(int)Teams.Red, 0] = new Vector3(0, 0, -1);
            TeemRotation[(int)Teams.Red, 1] = new Vector3(1, 0, 1);
            TeemRotation[(int)Teams.Red, 2] = new Vector3(-1, 0, 1);
        }
        else
        {
            TeemRotation[(int)Teams.Red, 0] = new Vector3(-1, 0, 0);
            TeemRotation[(int)Teams.Red, 1] = new Vector3(1, 0, -1);
            TeemRotation[(int)Teams.Red, 2] = new Vector3(1, 0, 1);
        }

        TeemRotation[(int)Teams.Black, 0] = new Vector3(0, 0, -1);
        TeemRotation[(int)Teams.Black, 1] = new Vector3(1, 0, 1);
        TeemRotation[(int)Teams.Black, 2] = new Vector3(-1, 0, 1);

        TeemRotation[(int)Teams.Yellow, 0] = new Vector3(1, 0, 0);
        TeemRotation[(int)Teams.Yellow, 1] = new Vector3(-1, 0, 1);
        TeemRotation[(int)Teams.Yellow, 2] = new Vector3(-1, 0, -1);
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

            if (Cards.AllCards[i].CardPair is ChestCard)
            {
                ChestCard.CardsCount = Cards.AllCards[i].Amount;
            }
            else if (Cards.AllCards[i].CardPair is TurntableCard)
            {
                TurntableCard.turntableCount = Cards.AllCards[i].Amount;
            }
        }

        Shuffle(ref cardsWithoutWater);
        RandomSpecialCards();

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

                RandomCard(ref PlayingField[i, j]);
            }
        }

        ++rotMassSize;
    }

    public void PlaceShips()
    {
        Ships.GenerateShips();
        int i = 0;
        foreach (var pair in Ships.AllShips)
        {
            if (i == MaxCountInRoom)
            {
                break;
            }

            WaterCard waterCard = PlayingField[pair.Value.Position.x, pair.Value.Position.z] as WaterCard;
            if (waterCard == null)
            {
                throw new Exception("Wrong ship or water card position");
            }

            waterCard.OwnShip = pair.Value;
            waterCard.Type = CardType.Ship;

            ++i;
        }
    }

    private void Shuffle<T>(ref List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void RandomSpecialCards()
    {
        for (int i = 1; i <= TurntableCard.turntableCount; i++)
        {
            if (i <= 5)
            {
                TurntablesSizes.Add(2);
            }
            else if (i <= 9)
            {
                TurntablesSizes.Add(3);
            }
            else if (i <= 11)
            {
                TurntablesSizes.Add(4);
            }
            else
            {
                TurntablesSizes.Add(5);
            }
        }
        Shuffle(ref TurntablesSizes);

        for (int i = 0; i < ChestCard.CardsCount; i++)
        {
            if (i < 5)
            {
                ChestsCoins.Add(1);
            }
            else if (i < 10)
            {
                ChestsCoins.Add(2);
            }
            else if (i < 13)
            {
                ChestsCoins.Add(3);
            }
            else if (i < 15)
            {
                ChestsCoins.Add(4);
            }
            else if (i < 16)
            {
                ChestsCoins.Add(5);
            }
            else
            {
                ChestsCoins.Add(Random.Range(1, 6));
            }
        }
        
        Shuffle(ref ChestsCoins);
    }
}