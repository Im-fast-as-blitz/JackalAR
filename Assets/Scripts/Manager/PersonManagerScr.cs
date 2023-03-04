using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonManagerScr : MonoBehaviour
{
    public delegate bool PossibilityToWalk(IntVector2 pos);
    
    public static Dictionary<Card.CardType, PossibilityToWalk> PossibilityToWalkByType;
    public static Dictionary<int, PossibilityToWalk> PossibilityToWalkByRotation;
    public static Dictionary<Card.CardType, List<IntVector2>> DirectionsToWalkByType;
    
    public static Game currGame;

    public static bool OnEmptyCard(IntVector2 pos)
    {
        return currGame.PlayingField[pos.x, pos.z].Type != Card.CardType.Water;
    }

    public static bool OnWaterCard(IntVector2 pos)
    {
        return currGame.PlayingField[pos.x, pos.z].Type == Card.CardType.Water ||
               currGame.PlayingField[pos.x, pos.z].Type == Card.CardType.Ship;
    }

    public static bool OnShipCard(IntVector2 pos)
    {
        return !(((pos.x is 1 or 11) && (pos.z is 0 or 12)) || ((pos.x is 0 or 12) && (pos.z is 1 or 11)));
    }
    
    public static bool OnCannonCard(IntVector2 pos)
    {
        return pos.x == 0 || pos.x == 12 || pos.z == 0 || pos.z == 12;
    }
    
    
    public static bool RotDefault(IntVector2 pos)
    {
        return true;
    }
    
    public static bool RotCannonDown(IntVector2 pos)
    {
        return pos.z < 0;
    }
    public static bool RotCannonRight(IntVector2 pos)
    {
        return pos.x > 0;
    }
    public static bool RotCannonUp(IntVector2 pos)
    {
        return pos.z > 0;
    }
    public static bool RotCannonLeft(IntVector2 pos)
    {
        return pos.x < 0;
    }
    
    

    private void Awake()
    {
        // Generate directions
        CrossDirections.Add(new IntVector2(1, 0));
        CrossDirections.Add(new IntVector2(0, 1));
        CrossDirections.Add(new IntVector2(-1, 0));
        CrossDirections.Add(new IntVector2(0, -1));

        DiagonalDirections.Add(new IntVector2(1, 1));
        DiagonalDirections.Add(new IntVector2(-1, 1));
        DiagonalDirections.Add(new IntVector2(1, -1));
        DiagonalDirections.Add(new IntVector2(-1, -1));
        
        HorseDirections.Add(new IntVector2(1, 2));
        HorseDirections.Add(new IntVector2(-1, 2));
        HorseDirections.Add(new IntVector2(2, 1));
        HorseDirections.Add(new IntVector2(2, -1));
        HorseDirections.Add(new IntVector2(1, -2));
        HorseDirections.Add(new IntVector2(-1, -2));
        HorseDirections.Add(new IntVector2(-2, -1));
        HorseDirections.Add(new IntVector2(-2, 1));
        
        for (int i = 1; i < 13; ++i)
        {
            CannonDirections.Add(new IntVector2(0, i));
            CannonDirections.Add(new IntVector2(i, 0));
        }
        for (int i = -12; i < 0; ++i)
        {
            CannonDirections.Add(new IntVector2(0, i));
            CannonDirections.Add(new IntVector2(i, 0));
        }

        DefaultDirections.AddRange(CrossDirections);
        DefaultDirections.AddRange(DiagonalDirections);

        // Fill Dictionaries
        PossibilityToWalkByType = new Dictionary<Card.CardType, PossibilityToWalk>();
        PossibilityToWalkByRotation = new Dictionary<int, PossibilityToWalk>{{-1, RotDefault}};
        DirectionsToWalkByType = new Dictionary<Card.CardType, List<IntVector2>>();
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
        // Cannon
        PossibilityToWalkByType.Add(Card.CardType.Cannon, OnCannonCard);
        DirectionsToWalkByType.Add(Card.CardType.Cannon, CannonDirections);
        PossibilityToWalkByRotation.Add((int)CannonRotation.Left, RotCannonLeft);
        PossibilityToWalkByRotation.Add((int)CannonRotation.Down, RotCannonDown);
        PossibilityToWalkByRotation.Add((int)CannonRotation.Right, RotCannonRight);
        PossibilityToWalkByRotation.Add((int)CannonRotation.Up, RotCannonUp);
    }

    public static List<IntVector2> DefaultDirections = new List<IntVector2>();
    public static List<IntVector2> CrossDirections = new List<IntVector2>();
    public static List<IntVector2> DiagonalDirections = new List<IntVector2>();
    public static List<IntVector2> HorseDirections = new List<IntVector2>();
    public static List<IntVector2> CannonDirections = new List<IntVector2>();
}
