using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonManagerScr : MonoBehaviour
{
    public delegate bool PossibilityToWalk(IntVector2 pos);

    public static Dictionary<Card.CardType, PossibilityToWalk> PossibilityToWalkByType;

    public static PossibilityToWalk[,] PossibilityToWalkByRotation =
        new PossibilityToWalk[Enum.GetNames(typeof(Card.CardType)).Length - 1, 5];

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

    public static bool WithoutCoin(IntVector2 pos)
    {
        return true;
    }

    public static bool WithCoin(IntVector2 pos)
    {
        Card currCard = currGame.PlayingField[pos.x, pos.z];
        return currCard.IsOpen && currCard.Type != Card.CardType.Fortress && currCard.Type != Card.CardType.Shaman;
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
        PossibilityToWalkByRotation[(int)Card.CardType.Cannon, (int)Rotation.Up] = RotationUp;
        PossibilityToWalkByRotation[(int)Card.CardType.Cannon, (int)Rotation.Right] = RotationRight;
        PossibilityToWalkByRotation[(int)Card.CardType.Cannon, (int)Rotation.Down] = RotationDown;
        PossibilityToWalkByRotation[(int)Card.CardType.Cannon, (int)Rotation.Left] = RotationLeft;
        // Arrows
        for (int i = 6; i <= 12; ++i)
        {
            PossibilityToWalkByType.Add((Card.CardType)i, OnArrowCard);
        }

        // Straight
        DirectionsToWalkByType.Add(Card.CardType.ArrowStraight, CrossDirections);
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight, (int)Rotation.Up] = RotationUp;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight, (int)Rotation.Right] = RotationRight;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight, (int)Rotation.Down] = RotationDown;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight, (int)Rotation.Left] = RotationLeft;
        // Diagonal
        DirectionsToWalkByType.Add(Card.CardType.ArrowDiagonal, DiagonalDirections);
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal, (int)Rotation.Up] = RotationLU;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal, (int)Rotation.Right] = RotationUR;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal, (int)Rotation.Down] = RotationRD;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal, (int)Rotation.Left] = RotationDL;
        // Straight 2
        DirectionsToWalkByType.Add(Card.CardType.ArrowStraight2, CrossDirections);
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight2, (int)Rotation.Up] = RotationUOrD;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight2, (int)Rotation.Right] = RotationLOrR;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight2, (int)Rotation.Down] = RotationUOrD;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight2, (int)Rotation.Left] = RotationLOrR;
        // Diagonal 2
        DirectionsToWalkByType.Add(Card.CardType.ArrowDiagonal2, DiagonalDirections);
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal2, (int)Rotation.Up] = RotationLUOrRD;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal2, (int)Rotation.Right] = RotationUROrDL;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal2, (int)Rotation.Down] = RotationLUOrRD;
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal2, (int)Rotation.Left] = RotationUROrDL;
        // 3
        DirectionsToWalkByType.Add(Card.CardType.Arrow3, DefaultDirections);
        PossibilityToWalkByRotation[(int)Card.CardType.Arrow3, (int)Rotation.Up] = RotationLU3;
        PossibilityToWalkByRotation[(int)Card.CardType.Arrow3, (int)Rotation.Right] = RotationUR3;
        PossibilityToWalkByRotation[(int)Card.CardType.Arrow3, (int)Rotation.Down] = RotationRD3;
        PossibilityToWalkByRotation[(int)Card.CardType.Arrow3, (int)Rotation.Left] = RotationDL3;
        // Straight 4
        DirectionsToWalkByType.Add(Card.CardType.ArrowStraight4, CrossDirections);
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowStraight4, (int)Rotation.None] = RotationDefault;
        // Diagonal 4
        DirectionsToWalkByType.Add(Card.CardType.ArrowDiagonal4, DiagonalDirections);
        PossibilityToWalkByRotation[(int)Card.CardType.ArrowDiagonal4, (int)Rotation.None] = RotationDefault;
        // Fortress
        PossibilityToWalkByType.Add(Card.CardType.Fortress, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Fortress, DefaultDirections);
        // Shaman
        PossibilityToWalkByType.Add(Card.CardType.Shaman, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Shaman, DefaultDirections);
        // Chest
        PossibilityToWalkByType.Add(Card.CardType.Chest, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Chest, DefaultDirections);
        // Turntable
        PossibilityToWalkByType.Add(Card.CardType.Turntable, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Turntable, DefaultDirections);
        
        PossibilityToWalkByType.Add(Card.CardType.Turntable2, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Turntable2, DefaultDirections);
        
        PossibilityToWalkByType.Add(Card.CardType.Turntable3, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Turntable3, DefaultDirections);
        
        PossibilityToWalkByType.Add(Card.CardType.Turntable4, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Turntable4, DefaultDirections);
        
        PossibilityToWalkByType.Add(Card.CardType.Turntable5, OnEmptyCard);
        DirectionsToWalkByType.Add(Card.CardType.Turntable5, DefaultDirections);
    }

    public static List<IntVector2> DefaultDirections = new List<IntVector2>();
    public static List<IntVector2> CrossDirections = new List<IntVector2>();
    public static List<IntVector2> DiagonalDirections = new List<IntVector2>();
    public static List<IntVector2> HorseDirections = new List<IntVector2>();
    public static List<IntVector2> CannonDirections = new List<IntVector2>();
}