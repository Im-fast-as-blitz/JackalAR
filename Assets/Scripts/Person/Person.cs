using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private GameObject moveCircle;
    [SerializeField] private GameObject attackCircle;
    private List<GameObject> _moveCircles = new List<GameObject>();

    [NonSerialized] public IntVector2 Position;
    [NonSerialized] public RpcConnector rpcConnector;

    public short CellDepth = 1;
    public Game currGame;

    public Teams team = Teams.White;
    public int personNumber;

    public bool _isAlive = true;
    public bool isWithCoin = false;
    public Card prevNotIceCard = null;
    public IntVector2 previousPosition;

    public bool isInTrap = false;

    public HashSet<CardType> turnTables = new HashSet<CardType>()
    {
        CardType.Turntable, CardType.Turntable2, CardType.Turntable3,
        CardType.Turntable4, CardType.Turntable5
    };

    // Return to the ship
    void ReturnToShip()
    {
        Position = new IntVector2(Ships.AllShips[team].Position);
        Card curCard = currGame.PlayingField[Position.x, Position.z];
        transform.position = curCard.OwnGO.transform.position;
        bool findPlace = false;
        for (short i = 0, teammates_count = 0, prev_pers = 0; i < 3; ++i)
        {
            if (curCard.Figures[i])
            {
                ++teammates_count;
                if (teammates_count == 1)
                {
                    prev_pers = i;
                    Vector3 beautiPos;
                    if (team == Teams.White || team == Teams.Yellow)
                    {
                        beautiPos = new Vector3(0.025f, 0, 0);
                    }
                    else
                    {
                        beautiPos = new Vector3(0, 0, 0.025f);
                    }

                    curCard.Figures[i].gameObject.transform.position =
                        curCard.OwnGO.transform.position + new Vector3(
                            currGame.TeemRotation[(int)team, 1].x * beautiPos.x, 0,
                            beautiPos.z * currGame.TeemRotation[(int)team, 1].z);
                    transform.position += new Vector3(currGame.TeemRotation[(int)team, 2].x * beautiPos.x, 0,
                        beautiPos.z * currGame.TeemRotation[(int)team, 2].z);
                }
                else
                {
                    curCard.Figures[i].gameObject.transform.position = curCard.OwnGO.transform.position +
                                                                       new Vector3(
                                                                           currGame.TeemRotation[(int)team, 1].x *
                                                                           0.025f, 0,
                                                                           0.025f * currGame.TeemRotation[(int)team, 1]
                                                                               .z);
                    curCard.Figures[prev_pers].gameObject.transform.position = curCard.OwnGO.transform.position +
                                                                               new Vector3(
                                                                                   currGame.TeemRotation[(int)team, 2]
                                                                                       .x * 0.025f, 0,
                                                                                   0.025f * currGame
                                                                                       .TeemRotation[(int)team, 2].z);
                    transform.position = curCard.OwnGO.transform.position + new Vector3(
                        currGame.TeemRotation[(int)team, 0].x * 0.025f, 0,
                        0.025f * currGame.TeemRotation[(int)team, 0].z);
                }
            }
            else if (!findPlace)
            {
                curCard.Figures[i] = this;
                findPlace = true;
            }
        }
    }

    public void Death()
    {
        _isAlive = false;
        transform.gameObject.SetActive(false);
    }

    private bool EnemyOnCard(IntVector2 pos, int depth)
    {
        foreach (var figure in currGame.PlayingField[pos.x, pos.z].Figures)
        {
            if (figure && figure.team != team && figure.CellDepth == depth)
            {
                return true;
            }
        }

        return false;
    }

    // Create curr circle to move
    private bool CreateMovement(IntVector2 addPos, PersonManagerScr.PossibilityToWalk possByType,
        PersonManagerScr.PossibilityToWalk possByRotation, PersonManagerScr.PossibilityToWalk possByCoin)
    {
        IntVector2 newPos = Position + addPos;

        if ((newPos.x is >= 0 and <= 12) && (newPos.z is >= 0 and <= 12) && possByType(newPos) &&
            possByRotation(addPos) && possByCoin(newPos))
        {
            Card currCard = currGame.PlayingField[newPos.x, newPos.z];
            Card prevCard = currGame.PlayingField[Position.x, Position.z];

            GameObject result = null;
            int depth = CellDepth;
            if (turnTables.Contains(prevCard.Type))
            {
                if (CellDepth < (prevCard as TurntableCard).StepCount)
                {
                    ++depth;
                    newPos = Position;
                }
                else
                {
                    depth = 1;
                }
            }

            if (EnemyOnCard(newPos, depth))
            {
                if (currCard.Type != CardType.Fortress && currCard.Type != CardType.Shaman)
                {
                    result = Instantiate(attackCircle, currCard.OwnGO.transform.position, Quaternion.identity);
                }
            }
            else
            {
                result = Instantiate(moveCircle, currCard.OwnGO.transform.position, new Quaternion(0, 0, 0, 0));
            }

            if (result)
            {
                if (turnTables.Contains(prevCard.Type) && CellDepth < (prevCard as TurntableCard).StepCount)
                {
                    result.transform.GetChild(0).gameObject.SetActive(true);
                    result.transform.GetChild(0).GetComponent<TextMeshPro>().text = (CellDepth + 1).ToString();
                }

                _moveCircles.Add(result);
            }

            return true;
        }

        return false;
    }

    //Create all circles to move
    public void GenerateMovements(bool IsSetActiveTakeCoin = true)
    {
        Card currentCard = currGame.PlayingField[Position.x, Position.z];
        PersonManagerScr.PossibilityToWalk possByType = PersonManagerScr.PossibilityToWalkByType[currentCard.Type];
        List<IntVector2> directions = PersonManagerScr.DirectionsToWalkByType[currentCard.Type];
        PersonManagerScr.PossibilityToWalk possByRotation = PersonManagerScr.RotationDefault;

        if (currentCard.Type != CardType.Trap) // na vsyakiy sluchay))
        {
            isInTrap = false;
        }

        if (currentCard.Type == CardType.Cannon)
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
        else if (currentCard.Type == CardType.Ice)
        {
            if (prevNotIceCard.Type == CardType.Horse || (prevNotIceCard.Type == CardType.Helicopter &&
                                                          (prevNotIceCard as HelicopterCard).IsUsed == 1))
            {
                possByType = PersonManagerScr.PossibilityToWalkByType[prevNotIceCard.Type];
                directions = PersonManagerScr.DirectionsToWalkByType[prevNotIceCard.Type];
            }
            else
            {
                directions = new List<IntVector2>();
                directions.Add(new IntVector2(Position.x - previousPosition.x, Position.z - previousPosition.z));
            }
        }
        else if (currentCard.Type == CardType.Crocodile)
        {
            directions = new List<IntVector2>();
            directions.Add(new IntVector2(previousPosition.x - Position.x, previousPosition.z - Position.z));
        }
        else if (currentCard.Type == CardType.Helicopter && (currentCard as HelicopterCard).IsUsed > 0)
        {
            directions = PersonManagerScr.DefaultDirections;
        }
        else if (currentCard.Type == CardType.Balloon)
        {
            directions.Clear();
            directions.Add(new IntVector2(Ships.AllShips[team].Position.x - Position.x,
                Ships.AllShips[team].Position.z - Position.z));
        }
        else if (isInTrap)
        {
            directions = new List<IntVector2>();
        }

        PersonManagerScr.PossibilityToWalk possByCoin = PersonManagerScr.WithoutCoin;
        if (isWithCoin)
        {
            possByCoin = PersonManagerScr.WithCoin;
        }

        int cellsCreated = 0;
        foreach (var direction in directions)
        {
            if (CreateMovement(direction, possByType, possByRotation, possByCoin))
            {
                cellsCreated++;
            }
        }

        if (cellsCreated == 0)
        {
            currGame.SuicideBtn.gameObject.SetActive(true);
        }
        else if (currentCard.Type == CardType.Shaman)
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
        else if (currentCard.Coins > 0 && IsSetActiveTakeCoin)
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

        if (currGame.PlayingField[Position.x, Position.z].Type == CardType.Shaman)
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

    public void Move(Vector3 newPos, bool isMainMove = true)
    {
        if (isMainMove)
        {
            rpcConnector.MovePersonRpc(newPos, team, personNumber);
        }

        Debug.Log("Moved");
        DestroyCircles();

        //Remove person from prev card

        Card prevCard = currGame.PlayingField[Position.x, Position.z];

        if (prevCard.Type == CardType.Helicopter)
        {
            (prevCard as HelicopterCard).IsUsed++;
        }

        if (prevCard.Type != CardType.Ice)
        {
            prevNotIceCard = prevCard;
        }

        previousPosition = new IntVector2(Position.x, Position.z);

        for (short i = 0, teammates_count = 0, prev_pers = 0; i < prevCard.Figures.Count; ++i)
        {
            if (prevCard.Figures[i] == this)
            {
                prevCard.Figures[i] = null;
            }
            else if (prevCard.Figures[i])
            {
                if (turnTables.Contains(prevCard.Type))
                {
                    if (CellDepth == prevCard.Figures[i].CellDepth)
                    {
                        ++teammates_count;
                        if (teammates_count == 1)
                        {
                            prev_pers = i;
                            prevCard.Figures[i].transform.GetChild(0).gameObject.SetActive(false);
                            transform.GetChild(0).gameObject.SetActive(false);
                        }
                        else
                        {
                            prevCard.Figures[prev_pers].transform.GetChild(0).GetComponent<TextMeshPro>().text =
                                (teammates_count).ToString();
                            prevCard.Figures[i].transform.GetChild(0).GetComponent<TextMeshPro>().text =
                                (teammates_count).ToString();
                            prevCard.Figures[prev_pers].transform.GetChild(0).gameObject.SetActive(true);
                        }
                    }

                    continue;
                }

                ++teammates_count;
                if (teammates_count == 1)
                {
                    prev_pers = i;
                    prevCard.Figures[i].gameObject.transform.position = prevCard.OwnGO.transform.position;
                }
                else
                {
                    Vector3 beautiPos;
                    if (team == Teams.White || team == Teams.Yellow)
                    {
                        beautiPos = new Vector3(0.025f, 0, 0);
                    }
                    else
                    {
                        beautiPos = new Vector3(0, 0, 0.025f);
                    }

                    prevCard.Figures[i].gameObject.transform.position = prevCard.OwnGO.transform.position +
                                                                        new Vector3(
                                                                            currGame.TeemRotation[(int)team, 1].x *
                                                                            beautiPos.x, 0,
                                                                            beautiPos.z *
                                                                            currGame.TeemRotation[(int)team, 1].z);
                    prevCard.Figures[prev_pers].gameObject.transform.position = prevCard.OwnGO.transform.position +
                        new Vector3(currGame.TeemRotation[(int)team, 2].x * beautiPos.x, 0,
                            beautiPos.z * currGame.TeemRotation[(int)team, 2].z);
                }
            }
        }

        //Change person's pos (in game and in scene)
        if (turnTables.Contains(prevCard.Type) && CellDepth < (prevCard as TurntableCard).StepCount)
        {
            ++CellDepth;
            newPos = prevCard.OwnGO.transform.position;
        }
        else
        {
            if (turnTables.Contains(prevCard.Type))
            {
                CellDepth = 1;
            }

            Vector3 posChanges = newPos - transform.position;
            Position.x += (int)Math.Round(posChanges.x / currGame.sizeCardPrefab.x);
            Position.z += (int)Math.Round(posChanges.z / currGame.sizeCardPrefab.z);
            transform.position = newPos;
        }

        Card curCard = currGame.PlayingField[Position.x, Position.z];

        if (curCard.Type != CardType.Ice)
        {
            currGame.ChangeTeam();
        }

        if (prevCard.Type == CardType.Ship && curCard.Type == CardType.Water)
        {
            (prevCard as WaterCard).MoveShip(Position.x, Position.z, currGame);
            for (int i = 0; i < curCard.Figures.Count; ++i)
            {
                if (prevCard.Figures[i] && prevCard.Figures[i] != this)
                {
                    prevCard.Figures[i].Move(newPos);
                }
            }
        }

        if (curCard.Type == CardType.Trap)
        {
            isInTrap = true;
            foreach (var pers in curCard.Figures)
            {
                if (pers != null)
                {
                    pers.isInTrap = false;
                    if (pers.team == team)
                    {
                        isInTrap = false;
                    }
                }
            }
        }

        if (isWithCoin)
        {
            isWithCoin = false;
            if (curCard is WaterCard || curCard.Type == CardType.Ogre)
            {
                currGame.TotalCoins--;
            }
            else if (curCard.Type == CardType.Ship)
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
        if (curCard.Type == CardType.Ship && (curCard as WaterCard).OwnShip.team != team)
        {
            Death();
            return;
        }
        else if (turnTables.Contains(curCard.Type))
        {
            transform.position = newPos + (curCard as TurntableCard).StepPos[CellDepth - 1];
        }

        bool findPlace = false;
        for (short i = 0, teammates_count = 0, prev_pers = 0; i < curCard.Figures.Count; ++i)
        {
            if (curCard.Figures[i])
            {
                if (curCard.Figures[i].team != team)
                {
                    if ((curCard.Type == CardType.Ship && (curCard as WaterCard).OwnShip.team == team) ||
                        curCard.Type == CardType.Water)
                    {
                        curCard.Figures[i].Death();
                        curCard.Figures[i] = null;
                    }
                    else if (!turnTables.Contains(curCard.Type) || (turnTables.Contains(curCard.Type) &&
                                                                    CellDepth == curCard.Figures[i].CellDepth))
                    {
                        if (turnTables.Contains(curCard.Type))
                        {
                            curCard.Figures[i].transform.GetChild(0).gameObject.SetActive(false);
                        }

                        curCard.Figures[i].CellDepth = 1;
                        curCard.Figures[i].ReturnToShip();
                        curCard.Figures[i] = null;
                    }
                }
                else
                {
                    if (turnTables.Contains(curCard.Type))
                    {
                        if (CellDepth == curCard.Figures[i].CellDepth)
                        {
                            ++teammates_count;
                            if (teammates_count == 1)
                            {
                                prev_pers = i;
                                curCard.Figures[i].transform.GetChild(0).GetComponent<TextMeshPro>().text =
                                    (teammates_count + 1).ToString();
                                curCard.Figures[i].transform.GetChild(0).gameObject.SetActive(true);
                                transform.GetChild(0).gameObject.SetActive(true);
                            }
                            else
                            {
                                curCard.Figures[prev_pers].transform.GetChild(0).GetComponent<TextMeshPro>().text =
                                    (teammates_count + 1).ToString();
                                curCard.Figures[i].transform.GetChild(0).GetComponent<TextMeshPro>().text =
                                    (teammates_count + 1).ToString();
                                curCard.Figures[i].transform.GetChild(0).gameObject.SetActive(true);
                            }

                            transform.GetChild(0).GetComponent<TextMeshPro>().text = (teammates_count + 1).ToString();
                        }

                        continue;
                    }

                    ++teammates_count;
                    if (teammates_count == 1)
                    {
                        prev_pers = i;
                        Vector3 beautiPos;
                        if (team == Teams.White || team == Teams.Yellow)
                        {
                            beautiPos = new Vector3(0.025f, 0, 0);
                        }
                        else
                        {
                            beautiPos = new Vector3(0, 0, 0.025f);
                        }

                        curCard.Figures[i].gameObject.transform.position =
                            curCard.OwnGO.transform.position + new Vector3(
                                currGame.TeemRotation[(int)team, 1].x * beautiPos.x, 0,
                                beautiPos.z * currGame.TeemRotation[(int)team, 1].z);
                        transform.position += new Vector3(currGame.TeemRotation[(int)team, 2].x * beautiPos.x, 0,
                            beautiPos.z * currGame.TeemRotation[(int)team, 2].z);
                    }
                    else
                    {
                        curCard.Figures[i].gameObject.transform.position = curCard.OwnGO.transform.position +
                                                                           new Vector3(
                                                                               currGame.TeemRotation[(int)team, 1].x *
                                                                               0.025f, 0,
                                                                               0.025f * currGame
                                                                                   .TeemRotation[(int)team, 1].z);
                        curCard.Figures[prev_pers].gameObject.transform.position = curCard.OwnGO.transform.position +
                            new Vector3(currGame.TeemRotation[(int)team, 2].x * 0.025f, 0,
                                0.025f * currGame.TeemRotation[(int)team, 2].z);
                        transform.position = curCard.OwnGO.transform.position +
                                             new Vector3(currGame.TeemRotation[(int)team, 0].x * 0.025f, 0,
                                                 0.025f * currGame.TeemRotation[(int)team, 0].z);
                    }
                }
            }

            if (!curCard.Figures[i] && !findPlace)
            {
                curCard.Figures[i] = this;
                findPlace = true;
            }
        }

        if (!curCard.IsOpen)
        {
            // rpcConnector.OpenCardRpc(Position);
            curCard.Open();
        }
        else
        {
            curCard.StepAction();
        }

        curCard.StepAction();
    }
}