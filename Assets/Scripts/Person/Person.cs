using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private GameObject moveCircle;
    [SerializeField] private GameObject attackCircle;
    public int mulCoef = 5;

    [NonSerialized] public Helpers.IntVector2 Position;
    public Game currGame;

    private List<GameObject> _moveCircles = new List<GameObject>();

    private delegate bool PossibilityToWalk(Helpers.IntVector2 pos);

    // public enum Team
    // {
    //     GREEN,
    //     RED,
    //     BLACK,
    //     YELLOW
    // }

    public Helpers.Teams team = Helpers.Teams.White;

    // void Start()
    // {
    //     switch (team)
    //     {
    //         case Helpers.Teams.White:
    //             _pos = new Vector3(2, 0, 2);
    //             break;
    //         case Helpers.Teams.Red:
    //             _pos = new Vector3(5, 0, 5);
    //             break;
    //         case Helpers.Teams.Yellow:
    //             break;
    //         case Helpers.Teams.Black:
    //             break;
    //     }
    //
    //     transform.position += _pos * 0.1f;
    // }

    //Return to the ship
    void ReturnToShip()
    {
        //Simple
        gameObject.SetActive(false);
    }

    //Check where person is
    private bool OnWaterCard(Helpers.IntVector2 pos)
    {
        return currGame.PlayingField[pos.x, pos.z] is not EmptyCard;
    }

    private bool OnEmptyCard(Helpers.IntVector2 pos)
    {
        return currGame.PlayingField[pos.x, pos.z] is not WaterCard;
    }

    private bool EnemyOnCard(Helpers.IntVector2 pos)
    {
        foreach (var figure in currGame.PlayingField[pos.x, pos.z].Figures)
        {
            if (figure && figure.team != team)
            {
                return true;
            }
        }

        return false;
    }

    //Create curr circle to move
    private void CreateMovement(Helpers.IntVector2 addPos, PossibilityToWalk func)
    {
        Helpers.IntVector2 newPos = Position + addPos;
        if (0 <= newPos.x && newPos.x <= 12 && 0 <= newPos.z && newPos.z <= 12 && func(newPos))
        {
            GameObject result;
            if (EnemyOnCard(newPos))
            {
                result = Instantiate(attackCircle, transform.position, Quaternion.identity);
            }
            else
            {
                result = Instantiate(moveCircle, transform.position, Quaternion.identity);
            }
            
            result.transform.position += addPos.ToVector3() * (transform.localScale.x * mulCoef);
            _moveCircles.Add(result);
        }
    }

    //Create all circles to move
    public void GenerateMovements()
    {
        PossibilityToWalk func = OnEmptyCard;
        if (currGame.PlayingField[Position.x, Position.z] is EmptyCard)
        {
            func = OnEmptyCard;
        }
        else if (currGame.PlayingField[Position.x, Position.z] is WaterCard)
        {
            func = OnWaterCard;
        }

        CreateMovement(new Helpers.IntVector2(-1, 0), func);
        CreateMovement(new Helpers.IntVector2(0, 1), func);
        CreateMovement(new Helpers.IntVector2(0, -1), func);
        CreateMovement(new Helpers.IntVector2(-1, 1), func);
        CreateMovement(new Helpers.IntVector2(-1, -1), func);
        CreateMovement(new Helpers.IntVector2(1, 1), func);
        CreateMovement(new Helpers.IntVector2(1, -1), func);
        CreateMovement(new Helpers.IntVector2(1, 0), func);
    }

    public void DestroyCircles()
    {
        foreach (var circle in _moveCircles)
        {
            Destroy(circle);
        }

        _moveCircles.Clear();
    }

    public void Move(Vector3 newPos)
    {
        //Remove person from prev card
        for (int i = 0; i < 3; ++i)
        {
            if (currGame.PlayingField[Position.x, Position.z].Figures[i] == this)
            {
                currGame.PlayingField[Position.x, Position.z].Figures[i] = null;
                break;
            }
        }

        //Change person's pos (in game and in scene)
        Vector3 posChanges = transform.position - newPos;
        if (posChanges.x < 0)
        {
            Position.x++;
        }
        else if (posChanges.x > 0)
        {
            Position.x--;
        }

        if (posChanges.z < 0)
        {
            Position.z++;
        }
        else if (posChanges.z > 0)
        {
            Position.z--;
        }

        transform.position = newPos;

        //Look at new card
        bool findPlace = false;
        for (int i = 0; i < 3; ++i)
        {
            if (currGame.PlayingField[Position.x, Position.z].Figures[i])
            {
                if (currGame.PlayingField[Position.x, Position.z].Figures[i].team != team)
                {
                    currGame.PlayingField[Position.x, Position.z].Figures[i].ReturnToShip();
                    currGame.PlayingField[Position.x, Position.z].Figures[i] = null;
                }
            }

            if (!currGame.PlayingField[Position.x, Position.z].Figures[i] && !findPlace)
            {
                currGame.PlayingField[Position.x, Position.z].Figures[i] = this;
                findPlace = true;
            }
        }

        if (!currGame.PlayingField[Position.x, Position.z].IsOpen)
        {
            currGame.PlayingField[Position.x, Position.z].Open();
        }

        DestroyCircles();
    }
}
