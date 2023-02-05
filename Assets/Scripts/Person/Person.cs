using System;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private GameObject moveCircle;
    [SerializeField] private GameObject attackCircle;
    public int mulCoef = 5;

    private Vector3 _pos;
    public Game currGame;

    private List<GameObject> _moveCircles;

    private delegate bool PossibilityToWalk(Vector3 pos);

    public enum Command
    {
        GREEN,
        RED,
        BLACK,
        YELLOW
    }
    public Command commandType = Command.GREEN;

    void Start()
    {
        _moveCircles = new List<GameObject>();
        switch (commandType)
        {
            case Command.GREEN:
                _pos = new Vector3(2, 0, 2);
                break;
            case Command.RED:
                _pos = new Vector3(5, 0, 5);
                break;
            case Command.YELLOW:
                break;
            case Command.BLACK:
                break;
        }
        transform.position += _pos * 0.1f;
    }
    
    //Return to the ship
    void ReturnToShip()
    {
        //Simple
        gameObject.SetActive(false);
    }

    //Check where person is
    private bool OnWaterCard(Vector3 pos)
    {
        return currGame.PlayingField[(int)pos.x, (int)pos.z] is not EmptyCard;
    }
    
    private bool OnEmptyCard(Vector3 pos)
    {
        return currGame.PlayingField[(int)pos.x, (int)pos.z] is not WaterCard;
    }

    private bool EnemyOnCard(Vector3 pos)
    {
        foreach (var figure in currGame.PlayingField[(int)pos.x, (int)pos.z].Figures)
        {
            if (figure && figure.commandType != commandType)
            {
                return true;
            }
        }
        return false;
    }

    //Create curr circle to move
    private void CreateMovement(Vector3 addPos, PossibilityToWalk func)
    {
        Vector3 newPos = _pos + addPos;
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
            result.transform.position += addPos * (transform.localScale.x * mulCoef);
            _moveCircles.Add(result);
        }
    }
    
    //Create all circles to move
    public void GenerateMovements()
    {
        PossibilityToWalk func = OnEmptyCard;
        if (currGame.PlayingField[(int)_pos.x, (int)_pos.z] is EmptyCard)
        {
            func = OnEmptyCard;
        } else if (currGame.PlayingField[(int)_pos.x, (int)_pos.z] is WaterCard)
        {
            func = OnWaterCard;
        }
        CreateMovement(new Vector3(-1, 0, 0), func);
        CreateMovement(new Vector3(0, 0, 1), func);
        CreateMovement(new Vector3(0, 0, -1), func);
        CreateMovement(new Vector3(-1, 0, 1), func);
        CreateMovement(new Vector3(-1, 0, -1), func);
        CreateMovement(new Vector3(1, 0, 1), func);
        CreateMovement(new Vector3(1, 0, -1), func);
        CreateMovement(new Vector3(1, 0, 0), func);
    }
    
    public void Move(Vector3 newPos)
    {
        //Remove person from prev card
        for (int i = 0; i < 3; ++i)
        {
            if (currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i] == this)
            {
                currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i] = null;
                break;
            }
        }
        
        //Change person's pos (in game and in scene)
        Vector3 posChanges = transform.position - newPos;
        if (posChanges.x < 0)
        {
            ++_pos.x;
        } else if (posChanges.x > 0)
        {
            --_pos.x;
        }
        if (posChanges.z < 0)
        {
            ++_pos.z;
        } else if (posChanges.z > 0)
        {
            --_pos.z;
        }
        transform.position = newPos;

        bool findPlace = false;
        //Look at new card
        for (int i = 0; i < 3; ++i)
        {
            if (currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i])
            {
                if (currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i].commandType != commandType)
                {
                    currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i].ReturnToShip();
                    currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i] = null;
                }
            }
            if (!currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i] && !findPlace)
            {
                currGame.PlayingField[(int)_pos.x, (int)_pos.z].Figures[i] = this;
                findPlace = true;
            }
        }
        
        if (!currGame.PlayingField[(int)_pos.x, (int)_pos.z].IsOpen)
        {
            currGame.PlayingField[(int)_pos.x, (int)_pos.z].Open();
        }
        
        //Destroy move circles
        foreach (var circle in _moveCircles)
        {
            Destroy(circle);
        }
        _moveCircles.Clear();
    }
}
