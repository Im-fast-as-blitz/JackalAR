using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private GameObject moveCircle;
    [SerializeField] private GameObject attackCircle;
    public int mulCoef = 5;
    private List<GameObject> _moveCircles = new List<GameObject>();

    [NonSerialized] public IntVector2 Position;
    public Game currGame;

    public Teams team = Teams.White;
    public bool _isAlive = true;
    public bool isWithCoin = false;

    //Return to the ship
    void ReturnToShip()
    {
        Position = new IntVector2(Ships.AllShips[team].Position);
        //Vector3 newPos = currGame.PlayingField[Position.x, Position.z].OwnGO.transform.position;
        //transform.position = new Vector3(newPos.x, newPos.y, newPos.z);
        transform.position = currGame.PlayingField[Position.x, Position.z].OwnGO.transform.position;
    }

    //Death
    public void Death()
    {
        ReturnToShip();
        _isAlive = false;
        transform.gameObject.SetActive(false);
    }

    private bool EnemyOnCard(IntVector2 pos)
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
    private void CreateMovement(IntVector2 addPos, PersonManagerScr.PossibilityToWalk possByType,
        PersonManagerScr.PossibilityToWalk possByRotation, PersonManagerScr.PossibilityToWalk possByCoin)
    {
        IntVector2 newPos = Position + addPos;

        if (possByCoin == PersonManagerScr.WithCoin)
        {
            Debug.Log("with coin");
        }

        if ((newPos.x is >= 0 and <= 12) && (newPos.z is >= 0 and <= 12) && possByType(newPos) &&
            possByRotation(addPos) && possByCoin(newPos))
        {
            Card currCard = currGame.PlayingField[newPos.x, newPos.z];
            bool isEnemyShip = currCard.Type == Card.CardType.Ship && (currCard as WaterCard).OwnShip.team != team;
            if (isEnemyShip)
            {
                return;
            }

            GameObject result = null;
            if (EnemyOnCard(newPos))
            {
                if (currGame.PlayingField[newPos.x, newPos.z].Type != Card.CardType.Fortress &&
                    currGame.PlayingField[newPos.x, newPos.z].Type != Card.CardType.Shaman)
                {
                    result = Instantiate(attackCircle, transform.position, Quaternion.identity);
                }
            }
            else
            {
                result = Instantiate(moveCircle, transform.position, new Quaternion(0, 0, 0, 0));
            }

            if (result)
            {
                result.transform.position += addPos.ToVector3() * (transform.localScale.x * mulCoef);
                _moveCircles.Add(result);
            }
        }
    }

    //Create all circles to move
    public void GenerateMovements(bool IsSetActiveTakeCoin = true)
    {
        Card currentCard = currGame.PlayingField[Position.x, Position.z];
        PersonManagerScr.PossibilityToWalk possByType = PersonManagerScr.PossibilityToWalkByType[currentCard.Type];
        List<IntVector2> directions = PersonManagerScr.DirectionsToWalkByType[currentCard.Type];
        PersonManagerScr.PossibilityToWalk possByRotation = PersonManagerScr.RotationDefault;
        if (currentCard is CannonCard)
        {
            possByRotation =
                PersonManagerScr.PossibilityToWalkByRotation[(int)currentCard.Type,
                    (int)(currentCard as CannonCard).Rotation];
        }
        else if (currentCard is ArrowCard)
        {
            possByRotation =
                PersonManagerScr.PossibilityToWalkByRotation[(int)currentCard.Type,
                    (int)(currentCard as ArrowCard).Rotation];
        }

        PersonManagerScr.PossibilityToWalk possByCoin = PersonManagerScr.WithoutCoin;
        if (isWithCoin)
        {
            Debug.Log("With coin");
            possByCoin = PersonManagerScr.WithCoin;
        }


        foreach (var direction in directions)
        {
            CreateMovement(direction, possByType, possByRotation, possByCoin);
        }

        if (currentCard.Type == Card.CardType.Shaman)
        {
            foreach (var per in currGame.Persons[team])
            {
                if (!per._isAlive)
                {
                    currGame.ShamanBtn.gameObject.SetActive(true);
                    break;
                }
            }
        }

        if (currentCard.Coins > 0 && IsSetActiveTakeCoin)
        {
            currGame.TakeCoinBtn.gameObject.SetActive(true);
        }

        Debug.Log("Coins on this card: " + currentCard.Coins);
    }

    public void DestroyCircles(bool IsDeleteTakePut = true)
    {
        foreach (var circle in _moveCircles)
        {
            Destroy(circle);
        }

        _moveCircles.Clear();

        if (currGame.PlayingField[Position.x, Position.z].Type == Card.CardType.Shaman)
        {
            currGame.ShamanBtn.gameObject.SetActive(false);
        }

        if (IsDeleteTakePut)
        {
            if (currGame.PlayingField[Position.x, Position.z].Coins > 0)
            {
                currGame.TakeCoinBtn.gameObject.SetActive(false);
            }

            if (isWithCoin || currGame.PlayingField[Position.x, Position.z].Coins > 0)
            {
                currGame.PutCoinBtn.gameObject.SetActive(false);
            }
        }
    }

    public void Move(Vector3 newPos)
    {
        DestroyCircles();

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
        IntVector2 prevPos = new IntVector2(Position);
        Vector3 posChanges = newPos - transform.position;
        Position.x += (int)Math.Round(posChanges.x / currGame.sizeCardPrefab.x);
        Position.z += (int)Math.Round(posChanges.z / currGame.sizeCardPrefab.z);
        transform.position = newPos;
        Card curCard = currGame.PlayingField[Position.x, Position.z];
        if (currGame.PlayingField[prevPos.x, prevPos.z].Type == Card.CardType.Ship &&
            curCard.Type == Card.CardType.Water)
        {
            (currGame.PlayingField[prevPos.x, prevPos.z] as WaterCard).MoveShip(Position.x, Position.z, currGame);
            for (int i = 0; i < 3; ++i)
            {
                if (currGame.PlayingField[prevPos.x, prevPos.z].Figures[i] &&
                    currGame.PlayingField[prevPos.x, prevPos.z].Figures[i] != this)
                {
                    currGame.PlayingField[prevPos.x, prevPos.z].Figures[i].Move(newPos);
                }
            }
        }

        if (isWithCoin)
        {
            isWithCoin = false;
            if (curCard is WaterCard || curCard.Type == Card.CardType.Ogre)
            {
                currGame.TotalCoins--;
            }
            else if (curCard.Type == Card.CardType.Ship)
            {
                currGame.TotalCoins--;
                if ((curCard as WaterCard).OwnShip.team == team)
                {
                    currGame.CoinsInTeam[(int)team]++;
                }
            }
            else
            {
                curCard.Coins++;
            }
        }

        //Look at new card
        bool findPlace = false;
        for (int i = 0; i < 3; ++i)
        {
            if (curCard.Figures[i] && curCard.Figures[i].team != team)
            {
                if (currGame.PlayingField[Position.x, Position.z].Type == Card.CardType.Ship)
                {
                    curCard.Figures[i].Death();
                }
                else
                {
                    curCard.Figures[i].ReturnToShip();
                }

                curCard.Figures[i] = null;
            }

            if (!curCard.Figures[i] && !findPlace)
            {
                curCard.Figures[i] = this;
                findPlace = true;
            }
        }

        if (!curCard.IsOpen)
        {
            curCard.Open();
        }
        else
        {
            curCard.StepAction();
        }

        currGame.PlayingField[Position.x, Position.z].StepAction();
    }
}