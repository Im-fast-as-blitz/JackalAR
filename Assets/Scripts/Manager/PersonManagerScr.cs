using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonManagerScr : MonoBehaviour
{
    public delegate bool PossibilityToWalk(Helpers.IntVector2 pos);
    
    public static Dictionary<Card.CardType, PossibilityToWalk> PossibilityToWalkByType;
    public static Dictionary<Card.CardType, List<Helpers.IntVector2>> DirectionsToWalkByType;
    public static Game currGame;

    public static bool OnEmptyCard(Helpers.IntVector2 pos)
    {
        return currGame.PlayingField[pos.x, pos.z].Type != Card.CardType.Water;
    }

    public static bool OnWaterCard(Helpers.IntVector2 pos)
    {
        return currGame.PlayingField[pos.x, pos.z].Type == Card.CardType.Water ||
               currGame.PlayingField[pos.x, pos.z].Type == Card.CardType.Ship;
    }

    public static bool OnShipCard(Helpers.IntVector2 pos)
    {
        return !(((pos.x is 1 or 11) && (pos.z is 0 or 12)) || ((pos.x is 0 or 12) && (pos.z is 1 or 11)));
    }

    private void Awake()
    {
        // Generate directions
        CrossDirections.Add(new Helpers.IntVector2(1, 0));
        CrossDirections.Add(new Helpers.IntVector2(0, 1));
        CrossDirections.Add(new Helpers.IntVector2(-1, 0));
        CrossDirections.Add(new Helpers.IntVector2(0, -1));

        DiagonalDirections.Add(new Helpers.IntVector2(1, 1));
        DiagonalDirections.Add(new Helpers.IntVector2(-1, 1));
        DiagonalDirections.Add(new Helpers.IntVector2(1, -1));
        DiagonalDirections.Add(new Helpers.IntVector2(-1, -1));
        
        HorseDirections.Add(new Helpers.IntVector2(1, 2));
        HorseDirections.Add(new Helpers.IntVector2(-1, 2));
        HorseDirections.Add(new Helpers.IntVector2(2, 1));
        HorseDirections.Add(new Helpers.IntVector2(2, -1));
        HorseDirections.Add(new Helpers.IntVector2(1, -2));
        HorseDirections.Add(new Helpers.IntVector2(-1, -2));
        HorseDirections.Add(new Helpers.IntVector2(-2, -1));
        HorseDirections.Add(new Helpers.IntVector2(-2, 1));

        DefaultDirections.AddRange(CrossDirections);
        DefaultDirections.AddRange(DiagonalDirections);

        // Fill Dictionaries
        PossibilityToWalkByType = new Dictionary<Card.CardType, PossibilityToWalk>();
        DirectionsToWalkByType = new Dictionary<Card.CardType, List<Helpers.IntVector2>>();
        // Empty
        PossibilityToWalkByType.Add(Card.CardType.Empty, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Empty, DefaultDirections);
        // Water
        PossibilityToWalkByType.Add(Card.CardType.Water, OnWaterCard);
        DirectionsToWalkByType.Add(Card.CardType.Water, DefaultDirections);
        // Ship
        PossibilityToWalkByType.Add(Card.CardType.Ship, OnShipCard);
        DirectionsToWalkByType.Add(Card.CardType.Ship, CrossDirections);
        // Horse
        PossibilityToWalkByType.Add(Card.CardType.Horse, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Horse, HorseDirections);
    }

    public static List<Helpers.IntVector2> DefaultDirections = new List<Helpers.IntVector2>();
    public static List<Helpers.IntVector2> CrossDirections = new List<Helpers.IntVector2>();
    public static List<Helpers.IntVector2> DiagonalDirections = new List<Helpers.IntVector2>();
    public static List<Helpers.IntVector2> HorseDirections = new List<Helpers.IntVector2>();
}
