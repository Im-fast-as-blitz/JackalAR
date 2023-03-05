using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonManagerScr : MonoBehaviour
{
    public delegate bool PossibilityToWalk(IntVector2 pos);
    
    public static Dictionary<Card.CardType, PossibilityToWalk> PossibilityToWalkByType;
    public static Dictionary<Tuple<int, Card.CardType>, PossibilityToWalk> PossibilityToWalkByRotation;
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
    
    public static bool OnArrowCard(IntVector2 pos)
    {
        return true;
    }
    
    
    public static bool RotationDefault(IntVector2 pos)
    {
        return true;
    }
    
    public static bool RotationDown(IntVector2 pos)
    {
        return pos.z < 0;
    }
    public static bool RotationRight(IntVector2 pos)
    {
        return pos.x > 0;
    }
    public static bool RotationUp(IntVector2 pos)
    {
        return pos.z > 0;
    }
    public static bool RotationLeft(IntVector2 pos)
    {
        return pos.x < 0;
    }
    
    public static bool RotationLU(IntVector2 pos)
    {
        return pos.x < 0 && pos.z > 0;
    }
    public static bool RotationUR(IntVector2 pos)
    {
        return pos.z > 0 && pos.x > 0;
    }
    public static bool RotationRD(IntVector2 pos)
    {
        return pos.x > 0 && pos.z < 0;
    }
    public static bool RotationDL(IntVector2 pos)
    {
        return pos.z < 0 && pos.x < 0;
    }
    
    public static bool RotationUOrD(IntVector2 pos)
    {
        return RotationUp(pos) || RotationDown(pos);
    }
    public static bool RotationLOrR(IntVector2 pos)
    {
        return RotationLeft(pos) || RotationRight(pos);
    }
    
    public static bool RotationLUOrRD(IntVector2 pos)
    {
        return RotationLU(pos) || RotationRD(pos);
    }
    public static bool RotationUROrDL(IntVector2 pos)
    {
        return RotationUR(pos) || RotationDL(pos);
    }
    
    public static bool RotationLU3(IntVector2 pos)
    {
        return (pos.x == -1 && pos.z == 1) || (pos.x == 1 && pos.z == 0) || (pos.x == 0 && pos.z == -1);
    }
    public static bool RotationUR3(IntVector2 pos)
    {
        return (pos.x == 1 && pos.z == 1) || (pos.x == -1 && pos.z == 0) || (pos.x == 0 && pos.z == -1);
    }
    public static bool RotationRD3(IntVector2 pos)
    {
        return (pos.x == 1 && pos.z == -1) || (pos.x == -1 && pos.z == 0) || (pos.x == 0 && pos.z == 1);
    }
    public static bool RotationDL3(IntVector2 pos)
    {
        return (pos.x == -1 && pos.z == -1) || (pos.x == 1 && pos.z == 0) || (pos.x == 0 && pos.z == 1);
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
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Up, Card.CardType.Cannon), RotationUp);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Right, Card.CardType.Cannon), RotationRight);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Down, Card.CardType.Cannon), RotationDown);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Left, Card.CardType.Cannon), RotationLeft);

        // Arrows
        for (int i = 6; i <= 12; ++i)
        {
            PossibilityToWalkByType.Add((Card.CardType) i , OnArrowCard);
        }
        // Straight
        DirectionsToWalkByType.Add(Card.CardType.ArrowStraight, CrossDirections);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Up, Card.CardType.ArrowStraight), RotationUp);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Right, Card.CardType.ArrowStraight), RotationRight);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Down, Card.CardType.ArrowStraight), RotationDown);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Left, Card.CardType.ArrowStraight), RotationLeft);
        // Diagonal
        DirectionsToWalkByType.Add(Card.CardType.ArrowDiagonal, DiagonalDirections);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Up, Card.CardType.ArrowDiagonal), RotationLU);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Right, Card.CardType.ArrowDiagonal), RotationUR);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Down, Card.CardType.ArrowDiagonal), RotationRD);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Left, Card.CardType.ArrowDiagonal), RotationDL);
        // Straight 2
        DirectionsToWalkByType.Add(Card.CardType.ArrowStraight2, CrossDirections);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Up, Card.CardType.ArrowStraight2), RotationUOrD);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Right, Card.CardType.ArrowStraight2), RotationLOrR);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Down, Card.CardType.ArrowStraight2), RotationUOrD);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Left, Card.CardType.ArrowStraight2), RotationLOrR);
        // Diagonal 2
        DirectionsToWalkByType.Add(Card.CardType.ArrowDiagonal2, DiagonalDirections);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Up, Card.CardType.ArrowDiagonal2), RotationLUOrRD);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Right, Card.CardType.ArrowDiagonal2), RotationUROrDL);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Down, Card.CardType.ArrowDiagonal2), RotationLUOrRD);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Left, Card.CardType.ArrowDiagonal2), RotationUROrDL);
        // 3
        DirectionsToWalkByType.Add(Card.CardType.Arrow3, DefaultDirections);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Up, Card.CardType.Arrow3), RotationLU3);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Right, Card.CardType.Arrow3), RotationUR3);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Down, Card.CardType.Arrow3), RotationRD3);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>((int)Rotation.Left, Card.CardType.Arrow3), RotationDL3);
        // Straight 4
        DirectionsToWalkByType.Add(Card.CardType.ArrowStraight4, CrossDirections);
        PossibilityToWalkByRotation.Add(new Tuple<int, Card.CardType>(-1, Card.CardType.ArrowStraight4), RotationDefault);
    }

    public static List<IntVector2> DefaultDirections = new List<IntVector2>();
    public static List<IntVector2> CrossDirections = new List<IntVector2>();
    public static List<IntVector2> DiagonalDirections = new List<IntVector2>();
    public static List<IntVector2> HorseDirections = new List<IntVector2>();
    public static List<IntVector2> CannonDirections = new List<IntVector2>();
}
