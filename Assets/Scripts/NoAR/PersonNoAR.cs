using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonNoAR : MonoBehaviour
{
    [SerializeField] private GameObject moveCircle;
    [SerializeField] private GameObject attackCircle;
    public int mulCoef = 5;

    [NonSerialized] public Helpers.IntVector2 Position;
    public Game currGame;

    private List<GameObject> _moveCircles = new List<GameObject>();

    public Helpers.Teams team = Helpers.Teams.White;

    //Return to the ship
    void ReturnToShip()
    {
        Position = new Helpers.IntVector2(Ships.AllShips[team].Position);
        transform.position = currGame.PlayingField[Position.x, Position.z].OwnGO.transform.position;
    }

    private bool EnemyOnCard(Helpers.IntVector2 pos)
    {
        foreach (var figure in currGame.PlayingField[pos.x, pos.z].FiguresNoAR)
        {
            if (figure && figure.team != team)
            {
                return true;
            }
        }
        
        return false;
    }

    //Create curr circle to move
    private void CreateMovement(Helpers.IntVector2 addPos, PersonManagerScr.PossibilityToWalk func)
    {
        Debug.Log("In create movement");
        Helpers.IntVector2 newPos = Position + addPos;

        if (0 <= newPos.x && newPos.x <= 12 && 0 <= newPos.z && newPos.z <= 12 && func(newPos))
        {
            Card currCard = currGame.PlayingField[newPos.x, newPos.z];
            bool isEnemyShip = ((currCard.Type == Card.CardType.Ship) && ((currCard as WaterCard).OwnShip.team != team));
            if (isEnemyShip)
            {
                return;
            }
            
            GameObject result;
            Debug.Log("Have to instantiate movement");
            if (EnemyOnCard(newPos))
            {
                result = Instantiate(attackCircle, transform.position, Quaternion.identity);
            }
            else
            {
                result = Instantiate(moveCircle, transform.position, Quaternion.identity);
            }

            //result.transform.position += addPos.ToVector3() * (transform.localScale.x * mulCoef);
            result.transform.position += addPos.ToVector3() * currGame.sizeCardPrefab.x;
            _moveCircles.Add(result);
        }
    }

    //Create all circles to move
    public void GenerateMovements()
    {
        Card currentCard = currGame.PlayingField[Position.x, Position.z];
        PersonManagerScr.PossibilityToWalk func = PersonManagerScr.PossibilityToWalkByType[currentCard.Type];
        List<Helpers.IntVector2> directions = PersonManagerScr.DirectionsToWalkByType[currentCard.Type];

        foreach (var direction in directions)
        {
            CreateMovement(direction, func);
        }
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
        DestroyCircles();
        
        //Remove person from prev card
        for (int i = 0; i < 3; ++i)
        {
            if (currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i] == this)
            {
                currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i] = null;
                break;
            }
        }

        //Change person's pos (in game and in scene)
        Helpers.IntVector2 prevPos = new Helpers.IntVector2(Position);
        Vector3 posChanges = newPos - transform.position;
        Position.x += (int) Math.Round(posChanges.x / currGame.sizeCardPrefab.x);
        Position.z += (int) Math.Round(posChanges.z / currGame.sizeCardPrefab.z);
        transform.position = newPos;
        if (currGame.PlayingField[prevPos.x, prevPos.z].Type == Card.CardType.Ship && currGame.PlayingField[Position.x, Position.z].Type == Card.CardType.Water)
        {
            (currGame.PlayingField[prevPos.x, prevPos.z] as WaterCard).MoveShip(Position.x, Position.z, currGame);
        }

        //Look at new card
        bool findPlace = false;
        for (int i = 0; i < 3; ++i)
        {
            if (currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i])
            {
                if (currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i].team != team)
                {
                    currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i].ReturnToShip();
                    currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i] = null;
                }
            }
        
            if (!currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i] && !findPlace)
            {
                currGame.PlayingField[Position.x, Position.z].FiguresNoAR[i] = this;
                findPlace = true;
            }
        }

        if (!currGame.PlayingField[Position.x, Position.z].IsOpen)
        {
            currGame.PlayingField[Position.x, Position.z].Open();
        }
    }
}
