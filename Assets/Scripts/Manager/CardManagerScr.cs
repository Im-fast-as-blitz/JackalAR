using System;
using System.Collections.Generic;
using UnityEngine;


public class Card
{
    public GameObject OwnGO;
    public string LogoPath;
    public List<Person> Figures = new List<Person>();
    public bool IsOpen = false;

    public Card(string logoPath)
    {
        LogoPath = logoPath;
    }

    public Card(Card other)
    {
        LogoPath = other.LogoPath;
        Figures = other.Figures;
        IsOpen = other.IsOpen;
    }

    public Card() { }

    public virtual void OpenAction() { }
    public virtual void StepAction() { }

    public void UpdateLogo()
    {
        OwnGO.GetComponent<CardGOBehaviourScr>().UpdateLogo();
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
    }
    public override void OpenAction() { }
    public override void StepAction() { }
}

public class WaterCard : Card
{
    public Ship OwnShip = null;
    
    public WaterCard()
    {
        LogoPath = "Cards/water";
    }
    
    public void LoadShipLogo()
    {
        OwnGO.GetComponent<CardGOBehaviourScr>().LoadShipLogo();
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
        OwnShip = null;
        UpdateLogo();
        waterCardToMove.LoadShipLogo();
    }
    
    public override void OpenAction() { }
    public override void StepAction() { }
}

public static class Cards
{
    public static List<Helpers.PairCardInt> AllCards = new List<Helpers.PairCardInt>();
}

public class CardManagerScr : MonoBehaviour
{
    public void Awake()
    {   // Total 169 cards (52 water cards + 117 other). Water must stay first
        Cards.AllCards.Add(new Helpers.PairCardInt(new WaterCard(), 52));
        Cards.AllCards.Add(new Helpers.PairCardInt(new EmptyCard(), 117));
    }
}
