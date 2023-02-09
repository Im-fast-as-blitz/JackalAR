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
        Ship
    }

    public GameObject OwnGO;
    public string LogoPath;
    public List<Person> Figures = new List<Person>() { null, null, null };
    public bool IsOpen = false;
    public CardType Type = CardType.Undefined;

    // public Card(string logoPath)
    // {
    //     LogoPath = logoPath;
    // }

    // public Card(Card other)
    // {
    //     LogoPath = other.LogoPath;
    //     Figures = other.Figures;
    //     IsOpen = other.IsOpen;
    //     Type = other.Type;
    // }

    // public Card()
    // {
    // }

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
        StepAction();
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
        if (OwnShip == null)
        {
            throw new Exception("Error: water card haven't ship");
        }

        WaterCard waterCardToMove = currGame.PlayingField[x, y] as WaterCard;
        if (waterCardToMove == null)
        {
            throw new Exception("Error: attempt to move the ship to the non water card");
        }

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

public static class Cards
{
    public static List<Helpers.PairCardInt> AllCards = new List<Helpers.PairCardInt>();
}

public class Ship
{
    public Helpers.Teams team;
    public string LogoPath;
    public List<Person> Figures = new List<Person>();
    public Helpers.IntVector2 Position;

    public Ship(string logoPath, Helpers.IntVector2 position)
    {
        LogoPath = logoPath;
        Position = position;
    }
}

public static class Ships
{
    public static Dictionary<Helpers.Teams, Ship> AllShips = new Dictionary<Helpers.Teams, Ship>();

    public static void GenerateShips()
    {
        AllShips.Add(Helpers.Teams.White, new Ship("Ships/white", new Helpers.IntVector2(6, 0)));
        AllShips.Add(Helpers.Teams.Red, new Ship("Ships/red", new Helpers.IntVector2(12, 6)));
        AllShips.Add(Helpers.Teams.Black, new Ship("Ships/black", new Helpers.IntVector2(0, 6)));
        AllShips.Add(Helpers.Teams.Yellow, new Ship("Ships/yellow", new Helpers.IntVector2(6, 12)));
    }
}

public class CardManagerScr : MonoBehaviour
{
    public void Awake()
    {
        // Total 169 cards (52 water cards + 117 other). Water must stay first
        Cards.AllCards.Add(new Helpers.PairCardInt(new WaterCard(), 52));
        Cards.AllCards.Add(new Helpers.PairCardInt(new EmptyCard(), 117));
        Ships.GenerateShips();
    }
}
