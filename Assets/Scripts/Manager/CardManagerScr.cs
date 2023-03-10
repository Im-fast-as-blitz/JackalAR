using System;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public enum CardType
    {
        Undefined,
        Empty,
        Water,
        Ship,
        Horse,
        Cannon,
        Ogre
    }

    public GameObject OwnGO;
    public string LogoPath;
    public List<Person> Figures = new List<Person>() {null, null, null};
    public bool IsOpen = false;
    public CardType Type = CardType.Undefined;

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

public enum CannonRotation{
    Up = 0, Right = 1, Down = 2, Left = 3
}

public class CannonCard : Card
{
    public CannonRotation Rotation;

    public CannonCard()
    {
        LogoPath = "Cards/cannon";
        Type = CardType.Cannon;
    }
    public override void OpenAction()
    {
        OwnGO.transform.eulerAngles = new Vector3(0, 90 * (int) Rotation, 0);
    }

    public override void StepAction()
    {
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

public static class Cards
{
    public static List<PairCardInt> AllCards = new List<PairCardInt>();
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
        Cards.AllCards.Add(new PairCardInt(new EmptyCard(), 30));
        Cards.AllCards.Add(new PairCardInt(new HorseCard(), 30));
        Cards.AllCards.Add(new PairCardInt(new CannonCard(), 27));
        Cards.AllCards.Add(new PairCardInt(new OgreCard(), 30));
        Ships.GenerateShips();
    }
}
