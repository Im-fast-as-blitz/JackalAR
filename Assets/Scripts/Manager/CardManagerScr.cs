using System;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


public enum CardType
{
    Undefined = -1,
    Empty = 0,
    Water = 1,
    Ship = 2,
    Horse = 3,
    Cannon = 4,
    Ogre = 5,
    ArrowStraight = 6,
    ArrowStraight2 = 7,
    ArrowDiagonal = 8,
    ArrowDiagonal2 = 9,
    Arrow3 = 10,
    ArrowStraight4 = 11,
    ArrowDiagonal4 = 12,
    Fortress = 13,
    Shaman = 14,
    Chest = 15,
    Turntable = 16,
    Turntable2 = 17,
    Turntable3 = 18,
    Turntable4 = 19,
    Turntable5 = 20,
    Arrow = 21,
    Ice = 22,
    Crocodile = 23,
    Helicopter = 24,
    Balloon = 25,
    Trap = 26,
    Rum = 27,
}

public class Card
{
    public GameObject OwnGO;
    public string LogoPath;
    public List<Person> Figures = new List<Person>() { null, null, null };
    public bool IsOpen = false;
    public CardType Type = CardType.Undefined;
    public int Coins = 0;
    public GameObject CoinGO = null;

    public void UpdateLogo()
    {
        Material gOMaterial = Resources.Load(LogoPath, typeof(Material)) as Material;
        if (gOMaterial)
        {
            OwnGO.GetComponent<Renderer>().material = gOMaterial;
        }
        else
        {
            throw new Exception("Can't find path while opening");
        }
    }

    public void Open()
    {
        IsOpen = true;
        UpdateLogo();
        OpenAction();
    }

    public virtual void OpenAction()
    {
    }

    public virtual void StepAction()
    {
    }

    public object NewObj()
    {
        return Activator.CreateInstance(GetType());
    }
}

public class EmptyCard : Card
{
    public EmptyCard()
    {
        LogoPath = "Cards/empty";
        Type = CardType.Empty;
    }
}

public class Ship
{
    public Teams team;
    public string LogoPath;
    public List<Person> Figures = new List<Person>();
    public IntVector2 Position;

    public Ship(Teams shipTeam, string logoPath, IntVector2 position)
    {
        team = shipTeam;
        LogoPath = logoPath;
        Position = position;
    }
}

public class WaterCard : Card
{
    public Ship OwnShip = null;

    public WaterCard()
    {
        LogoPath = "Cards/water";
        Type = CardType.Water;
    }

    public void LoadShipLogo()
    {
        Material gOMaterial = Resources.Load(OwnShip.LogoPath, typeof(Material)) as Material;
        if (gOMaterial)
        {
            OwnGO.GetComponent<Renderer>().material = gOMaterial;
        }
        else
        {
            throw new Exception("Can't find path while loading ship logo");
        }
    }


    public void MoveShip(int x, int y, Game currGame)
    {
        WaterCard waterCardToMove = currGame.PlayingField[x, y] as WaterCard;
        if (waterCardToMove == null)
        {
            throw new Exception("Error: attempt to move the ship to the non water card");
        }

        OwnShip.Position = new IntVector2(x, y);
        waterCardToMove.OwnShip = OwnShip;
        waterCardToMove.Type = CardType.Ship;
        OwnShip = null;
        Type = CardType.Water;
        UpdateLogo();
        waterCardToMove.LoadShipLogo();
    }
}

public class HorseCard : Card
{
    public HorseCard()
    {
        LogoPath = "Cards/horse";
        Type = CardType.Horse;
    }
}

public enum Rotation
{
    None = 4,
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public class CannonCard : Card
{
    public Rotation Rotation;

    public CannonCard()
    {
        LogoPath = "Cards/cannon";
        Type = CardType.Cannon;
    }

    public override void OpenAction()
    {
        OwnGO.transform.eulerAngles = new Vector3(0, 90 * (int)Rotation, 0);
    }
}


public class ArrowCard : Card
{
    public Rotation Rotation = Rotation.None;

    public ArrowCard()
    {
        Type = CardType.Arrow;
    }

    public override void OpenAction()
    {
        OwnGO.transform.eulerAngles = new Vector3(0, 90 * (int)Rotation, 0);
    }
}

public class ArrowStraight : ArrowCard
{
    public ArrowStraight()
    {
        LogoPath = "Cards/Arrows/straight";
        Type = CardType.ArrowStraight;
    }
}

public class ArrowStraight2 : ArrowCard
{
    public ArrowStraight2()
    {
        LogoPath = "Cards/Arrows/straight2";
        Type = CardType.ArrowStraight2;
    }
}

public class ArrowDiagonal : ArrowCard
{
    public ArrowDiagonal()
    {
        LogoPath = "Cards/Arrows/diagonal";
        Type = CardType.ArrowDiagonal;
    }
}

public class ArrowDiagonal2 : ArrowCard
{
    public ArrowDiagonal2()
    {
        Type = CardType.ArrowDiagonal2;
        LogoPath = "Cards/Arrows/diagonal2";
    }
}

public class Arrow3 : ArrowCard
{
    public Arrow3()
    {
        Type = CardType.Arrow3;
        LogoPath = "Cards/Arrows/3";
    }
}

public class ArrowStraight4 : ArrowCard
{
    public ArrowStraight4()
    {
        Type = CardType.ArrowStraight4;
        LogoPath = "Cards/Arrows/straight4";
    }
}

public class ArrowDiagonal4 : ArrowCard
{
    public ArrowDiagonal4()
    {
        Type = CardType.ArrowDiagonal4;
        LogoPath = "Cards/Arrows/diagonal4";
    }
}

public class OgreCard : Card
{
    public OgreCard()
    {
        LogoPath = "Cards/ogre";
        Type = CardType.Ogre;
    }

    public override void StepAction()
    {
        if (!Figures[0])
        {
            throw new Exception("Can't find a person");
        }

        Figures[0].Death();
        Figures[0] = null;
    }
}

public class FortressCard : Card
{
    public FortressCard()
    {
        LogoPath = "Cards/fortress";
        Type = CardType.Fortress;
    }
}

public class ShamanCard : Card
{
    public ShamanCard()
    {
        LogoPath = "Cards/shaman";
        Type = CardType.Shaman;
    }
}

public class ChestCard : Card
{
    public static int CardsCount = 0;

    public ChestCard()
    {
        LogoPath = "Cards/chest";
        Type = CardType.Chest;
    }

    public override void OpenAction()
    {
        GameObject loadedCoinGO = Resources.Load("Prefabs/coin", typeof(GameObject)) as GameObject;
        CoinGO = GameManagerScr.Instantiate(loadedCoinGO, OwnGO.transform.position, Quaternion.Euler(0, 180, 0));
        CoinGO.transform.GetChild(0).GetComponent<TextMeshPro>().text = Coins.ToString();
    }
}

public class TurntableCard : Card
{
    public static int turntableCount = 0;

    public short StepCount;
    public List<Vector3> StepPos = new List<Vector3>();

    public TurntableCard()
    {
        Type = CardType.Turntable;

        for (int i = 3; i < 12; ++i)
        {
            Figures.Add(null);
        }
    }
}

public class TurntableCard2 : TurntableCard
{
    public TurntableCard2()
    {
        LogoPath = "Cards/Turntables/2-steps";
        Type = CardType.Turntable2;

        StepCount = 2;
        StepPos.Add(new Vector3(0.03f, 0, 0.03f));
        StepPos.Add(new Vector3(-0.03f, 0, -0.03f));
    }
}

public class TurntableCard3 : TurntableCard
{
    public TurntableCard3()
    {
        LogoPath = "Cards/Turntables/3-steps";
        Type = CardType.Turntable3;

        StepCount = 3;
        StepPos.Add(new Vector3(-0.03f, 0, 0.03f));
        StepPos.Add(new Vector3(0.01f, 0, 0));
        StepPos.Add(new Vector3(-0.03f, 0, -0.03f));
    }
}

public class TurntableCard4 : TurntableCard
{
    public TurntableCard4()
    {
        LogoPath = "Cards/Turntables/4-steps";
        Type = CardType.Turntable4;

        StepCount = 4;
        StepPos.Add(new Vector3(-0.03f, 0, 0.035f));
        StepPos.Add(new Vector3(0.03f, 0, 0.015f));
        StepPos.Add(new Vector3(-0.025f, 0, -0.02f));
        StepPos.Add(new Vector3(+0.02f, 0, -0.035f));
    }
}

public class TurntableCard5 : TurntableCard
{
    public TurntableCard5()
    {
        LogoPath = "Cards/Turntables/5-steps";
        Type = CardType.Turntable5;

        StepCount = 5;
        StepPos.Add(new Vector3(-0.035f, 0, -0.03f));
        StepPos.Add(new Vector3(0, 0, -0.03f));
        StepPos.Add(new Vector3(0.03f, 0, -0.01f));
        StepPos.Add(new Vector3(0.02f, 0, 0.03f));
        StepPos.Add(new Vector3(-0.035f, 0, 0.03f));
    }
}

public class HelicopterCard : Card
{
    public int IsUsed = 0;

    public HelicopterCard()
    {
        LogoPath = "Cards/helicopter";
        Type = CardType.Helicopter;
    }
}

public class BalloonCard : Card
{
    public BalloonCard()
    {
        LogoPath = "Cards/balloon";
        Type = CardType.Balloon;
    }
}

public class TrapCard : Card
{
    public TrapCard()
    {
        LogoPath = "Cards/trap";
        Type = CardType.Trap;
    }
}

public class IceCard : Card
{
    public IceCard()
    {
        LogoPath = "Cards/ice";
        Type = CardType.Ice;
    }
}

public class CrocodileCard : Card
{
    public CrocodileCard()
    {
        LogoPath = "Cards/crocodile";
        Type = CardType.Crocodile;
    }
}

public class RumCard : Card
{
    public RumCard()
    {
        LogoPath = "Cards/rum";
        Type = CardType.Rum;
    }

    public override void StepAction()
    {
        foreach (var per in Figures)
        {
            if (per && per.drunkCount == 0)
            {
                int currMask = 1 << (int)per.team;
                if ((per.currGame.drunkTeams & currMask) == 0)
                {
                    per.currGame.drunkTeams += currMask;
                }

                per.gameObject.layer = LayerMask.NameToLayer("Drunk");
                per.drunkCount = 2;
                break;
            }
        }
    }
}


public static class Cards
{
    public static List<PairCardInt> AllCards = new List<PairCardInt>();
    public static HashSet<CardType> OnCurrentStep = new HashSet<CardType>();
    public static Dictionary<CardType, Card> CreateCardByType = new Dictionary<CardType, Card>();
}


public static class Ships
{
    public static Dictionary<Teams, Ship> AllShips = new Dictionary<Teams, Ship>();

    public static void GenerateShips()
    {
        AllShips.Add(Teams.White, new Ship(Teams.White, "Ships/white", new IntVector2(6, 0)));
        if (Game.MaxCountInRoom == 2)
        {
            AllShips.Add(Teams.Red, new Ship(Teams.Red, "Ships/red", new IntVector2(6, 12)));
            AllShips.Add(Teams.Black, new Ship(Teams.Black, "Ships/black", new IntVector2(12, 6)));
        }
        else
        {
            AllShips.Add(Teams.Red, new Ship(Teams.Red, "Ships/red", new IntVector2(12, 6)));
            AllShips.Add(Teams.Black, new Ship(Teams.Black, "Ships/black", new IntVector2(6, 12)));
        }
        AllShips.Add(Teams.Yellow,
            new Ship(Teams.Yellow, "Ships/yellow", new IntVector2(0, 6)));
    }
}

public class CardManagerScr : MonoBehaviour
{
    public void Awake()
    {
        // Total 169 cards (52 water cards + 117 other). Water must stay first
        Cards.AllCards.Add(new PairCardInt(new WaterCard(), 52));
        Cards.AllCards.Add(new PairCardInt(new EmptyCard(), 40)); // 40
        Cards.AllCards.Add(new PairCardInt(new HorseCard(), 2)); // 2
        Cards.AllCards.Add(new PairCardInt(new CannonCard(), 2)); // 2
        Cards.AllCards.Add(new PairCardInt(new OgreCard(), 1)); // 1
        Cards.AllCards.Add(new PairCardInt(new ArrowCard(), 21)); // 21
        Cards.AllCards.Add(new PairCardInt(new ShamanCard(), 1)); // 1
        Cards.AllCards.Add(new PairCardInt(new FortressCard(), 2)); // 2
        Cards.AllCards.Add(new PairCardInt(new TurntableCard(), 12)); // 12
        Cards.AllCards.Add(new PairCardInt(new ChestCard(), 16)); // 16
        Cards.AllCards.Add(new PairCardInt(new IceCard(), 6)); // 6
        Cards.AllCards.Add(new PairCardInt(new CrocodileCard(), 4)); // 4
        Cards.AllCards.Add(new PairCardInt(new HelicopterCard(), 1)); // 1
        Cards.AllCards.Add(new PairCardInt(new BalloonCard(), 2)); // 2
        Cards.AllCards.Add(new PairCardInt(new TrapCard(), 3)); // 3
        Cards.AllCards.Add(new PairCardInt(new RumCard(), 4)); // 4

        foreach (var pairCardInt in Cards.AllCards)
        {
            Cards.CreateCardByType.Add(pairCardInt.CardPair.Type, pairCardInt.CardPair);
        }

        Cards.CreateCardByType.Add(CardType.Ship, new WaterCard());
        Cards.CreateCardByType.Add(CardType.Arrow3, new Arrow3());
        Cards.CreateCardByType.Add(CardType.ArrowStraight, new ArrowStraight());
        Cards.CreateCardByType.Add(CardType.ArrowDiagonal, new ArrowDiagonal());
        Cards.CreateCardByType.Add(CardType.ArrowDiagonal2, new ArrowDiagonal2());
        Cards.CreateCardByType.Add(CardType.ArrowDiagonal4, new ArrowDiagonal4());
        Cards.CreateCardByType.Add(CardType.ArrowStraight2, new ArrowStraight2());
        Cards.CreateCardByType.Add(CardType.ArrowStraight4, new ArrowStraight4());
        Cards.CreateCardByType.Add(CardType.Turntable2, new TurntableCard2());
        Cards.CreateCardByType.Add(CardType.Turntable3, new TurntableCard3());
        Cards.CreateCardByType.Add(CardType.Turntable4, new TurntableCard4());
        Cards.CreateCardByType.Add(CardType.Turntable5, new TurntableCard5());

        Cards.OnCurrentStep.Add(CardType.Ice);
        Cards.OnCurrentStep.Add(CardType.Balloon);
        Cards.OnCurrentStep.Add(CardType.Horse);
        Cards.OnCurrentStep.Add(CardType.Cannon);
        Cards.OnCurrentStep.Add(CardType.ArrowStraight);
        Cards.OnCurrentStep.Add(CardType.ArrowDiagonal);
        Cards.OnCurrentStep.Add(CardType.ArrowDiagonal2);
        Cards.OnCurrentStep.Add(CardType.ArrowDiagonal4);
        Cards.OnCurrentStep.Add(CardType.ArrowStraight2);
        Cards.OnCurrentStep.Add(CardType.ArrowStraight4);

        //Ships.GenerateShips();
    }
}