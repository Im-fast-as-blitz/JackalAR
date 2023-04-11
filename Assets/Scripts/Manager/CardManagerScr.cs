using System;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
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
        
    }

    public GameObject OwnGO;
    public string LogoPath;
    public List<Person> Figures = new List<Person>() { null, null, null };
    public bool IsOpen = false;
    public CardType Type = CardType.Undefined;
    public int Coins = 0;

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

    public override void OpenAction()
    {
    }

    public override void StepAction()
    {
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

    public override void OpenAction()
    {
    }

    public override void StepAction()
    {
    }
}

public class HorseCard : Card
{
    public HorseCard()
    {
        LogoPath = "Cards/horse";
        Type = CardType.Horse;
    }

    public override void OpenAction()
    {
    }

    public override void StepAction()
    {
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

    public override void StepAction()
    {
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

    public override void StepAction()
    {
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
        LogoPath = "Cards/Arrows/straight";
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

    public override void OpenAction()
    {
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

    public override void OpenAction()
    {
    }

    public override void StepAction()
    {
    }
}

public class ShamanCard : Card
{
    public ShamanCard()
    {
        LogoPath = "Cards/shaman";
        Type = CardType.Shaman;
    }

    public override void OpenAction()
    {
    }

    public override void StepAction()
    {
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
        // Generate coins
    }

    public override void StepAction()
    {
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


public static class Cards
{
    public static List<PairCardInt> AllCards = new List<PairCardInt>();
    public static Dictionary<Card.CardType, Card> createCardByType = new Dictionary<Card.CardType, Card>();
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

public static class Ships
{
    public static Dictionary<Teams, Ship> AllShips = new Dictionary<Teams, Ship>();

    public static void GenerateShips()
    {
        AllShips.Add(Teams.White, new Ship(Teams.White, "Ships/white", new IntVector2(6, 0)));
        AllShips.Add(Teams.Red, new Ship(Teams.Red, "Ships/red", new IntVector2(12, 6)));
        AllShips.Add(Teams.Black, new Ship(Teams.Black, "Ships/black", new IntVector2(0, 6)));
        AllShips.Add(Teams.Yellow,
            new Ship(Teams.Yellow, "Ships/yellow", new IntVector2(6, 12)));
    }
}

public class CardManagerScr : MonoBehaviour
{
    public void Awake()
    {
        // Total 169 cards (52 water cards + 117 other). Water must stay first
        Cards.AllCards.Add(new PairCardInt(new WaterCard(), 52));
        Cards.AllCards.Add(new PairCardInt(new EmptyCard(), 20));
        Cards.AllCards.Add(new PairCardInt(new HorseCard(), 10));
        Cards.AllCards.Add(new PairCardInt(new CannonCard(), 10));
        Cards.AllCards.Add(new PairCardInt(new OgreCard(), 1));
        Cards.AllCards.Add(new PairCardInt(new ArrowCard(), 15));
        Cards.AllCards.Add(new PairCardInt(new ShamanCard(), 10));
        Cards.AllCards.Add(new PairCardInt(new FortressCard(), 10));
        Cards.AllCards.Add(new PairCardInt(new TurntableCard(), 16));
        Cards.AllCards.Add(new PairCardInt(new ChestCard(), 35));

        foreach (var pairCardInt in Cards.AllCards)
        {
            Cards.createCardByType.Add(pairCardInt.CardPair.Type, pairCardInt.CardPair);
        }

        Cards.createCardByType.Add(Card.CardType.Ship, new WaterCard());
        Cards.createCardByType.Add(Card.CardType.Arrow3, new Arrow3());
        Cards.createCardByType.Add(Card.CardType.ArrowStraight, new ArrowStraight());
        Cards.createCardByType.Add(Card.CardType.ArrowDiagonal, new ArrowDiagonal());
        Cards.createCardByType.Add(Card.CardType.ArrowDiagonal2, new ArrowDiagonal2());
        Cards.createCardByType.Add(Card.CardType.ArrowDiagonal4, new ArrowDiagonal4());
        Cards.createCardByType.Add(Card.CardType.ArrowStraight2, new ArrowStraight2());
        Cards.createCardByType.Add(Card.CardType.ArrowStraight4, new ArrowStraight4());
        Cards.createCardByType.Add(Card.CardType.Turntable2, new TurntableCard2());
        Cards.createCardByType.Add(Card.CardType.Turntable3, new TurntableCard3());
        Cards.createCardByType.Add(Card.CardType.Turntable4, new TurntableCard4());
        Cards.createCardByType.Add(Card.CardType.Turntable5, new TurntableCard5());

        Ships.GenerateShips();
    }
}