using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RpcConnector : MonoBehaviourPun
{
    private Game currGame;
    public GameManagerScr gameManagerScr;

    public void SetGameObj(Game game)
    {
        Debug.Log("SetGameObjCalled");
        currGame = game;
    }

    [PunRPC]
    void DebugRpc(int x, int y)
    {
        Debug.Log("DebugRpcCalled");
        currGame.PlayingField[x, y].Open();
    }

    public void DebugRpc()
    {
        Debug.Log("DebugFromPrcCalled");
        photonView.RPC("DebugRpc", RpcTarget.AllBuffered, 2, 4);
    }

    [PunRPC]
    void SyncCards(IReadOnlyList<int[]> cardTypes, IReadOnlyList<int> rotMass)
    {
        Debug.Log("SyncCardsCalled");
        int rotInd = 0;
        for (var i = 0; i < currGame.PlayingField.GetLength(0); ++i)
        {
            for (var j = 0; j < currGame.PlayingField.GetLength(1); ++j)
            {
                currGame.PlayingField[i, j] = (Card)Cards.CreateCardByType[(CardType)cardTypes[i][j]].NewObj();
                var curCard = currGame.PlayingField[i, j];
                var curType = curCard.Type;
                if (curCard is ArrowCard arrowCard && curType != CardType.ArrowStraight4 && curType != CardType.ArrowDiagonal4)
                {
                    arrowCard.Rotation = (Rotation)rotMass[rotInd++];
                } else if (curCard is CannonCard card)
                {
                    card.Rotation = (Rotation)rotMass[rotInd++];
                }
            }
        }

        currGame.PlaceShips();
        gameManagerScr.BuildPlayingField(new Vector3(0, 0, 0));
        CreateNewTeamRpc();
    }

    public void SyncCardsRpc(int massSize)
    {
        Debug.Log(string.Format("SyncCardsFromRpcCalled"));
        var cardTypes = new int[currGame.PlayingField.GetLength(0)][];
        var rotMass = new int[massSize];
        var rotIndex = 0;
        for (var i = 0; i < currGame.PlayingField.GetLength(0); ++i)
        {
            cardTypes[i] = new int[massSize + currGame.PlayingField.GetLength(1)];
            for (var j = 0; j < currGame.PlayingField.GetLength(1); ++j)
            {
                var curCard = currGame.PlayingField[i, j];
                var curType = curCard.Type;
                Debug.Log(curType);
                cardTypes[i][j] = (int)currGame.PlayingField[i, j].Type;
                if (curCard is ArrowCard card && curType != CardType.ArrowStraight4 && curType != CardType.ArrowDiagonal4)
                {
                    rotMass[rotIndex++] = (int)card.Rotation;
                } else if (curCard is CannonCard cannonCard)
                {
                    rotMass[rotIndex++] = (int)cannonCard.Rotation;
                }
            }
        }
        photonView.RPC("SyncCards", RpcTarget.OthersBuffered, cardTypes, rotMass);
    }

    [PunRPC]
    void CreateNewTeam()
    {
        gameManagerScr.CreateTeam();
    }

    public void CreateNewTeamRpc()
    {
        photonView.RPC("CreateNewTeam", RpcTarget.AllBuffered);
    }
    
    [PunRPC]
    void MovePerson(float x, float y, float z, int team, int personNum)
    {
        Debug.Log(string.Format("MovePersonCalled"));
        currGame.Persons[(Teams)team][personNum].Move(new Vector3(x, y, z));
        gameManagerScr.EndRound(team);
    }
    
    public void MovePersonRpc(Vector3 pos, Teams team, int personNum)
    {
        Debug.Log(string.Format("RpcMovePersonCalled"));
        photonView.RPC("MovePerson", RpcTarget.AllBuffered, pos.x, pos.y, pos.z, (int)team, personNum);
    }
    
    [PunRPC]
    void SuicedPerson(int team, int personNum)
    {
        Debug.Log(string.Format("SuicedPersonCalled"));
        currGame.Persons[(Teams)team][personNum].SuicidePerson();
        gameManagerScr._layerMask = 1 << LayerMask.NameToLayer("Person");
        gameManagerScr._personScr = null;
        currGame.SuicideBtn.gameObject.SetActive(false);
    }
    
    public void SuicidePersonRpc(Person person)
    {
        Debug.Log(string.Format("RpcSuicedPersonCalled"));
        photonView.RPC("SuicedPerson", RpcTarget.AllBuffered, (int)person.team, person.personNumber);
    }
    
    
    [PunRPC]
    void TakeCoinPerson(int team, int personNum)
    {
        Debug.Log(string.Format("TakeCoinPersonCalled"));
        currGame.Persons[(Teams)team][personNum].TakeCoinByPerson();
    }
    
    public void TakeCoinPersonRpc(Person person)
    {
        Debug.Log(string.Format("RpcTakeCoinPersonCalled"));
        photonView.RPC("TakeCoinPerson", RpcTarget.AllBuffered, (int)person.team, person.personNumber);
    }
    
    [PunRPC]
    void PutCoinPerson(int team, int personNum)
    {
        Debug.Log(string.Format("PutCoinPersonCalled"));
        currGame.Persons[(Teams)team][personNum].PutCoinPersonByPerson();
    }
    
    public void PutCoinPersonRpc(Person person)
    {
        Debug.Log(string.Format("RpcTakeCoinPersonCalled"));
        photonView.RPC("PutCoinPerson", RpcTarget.AllBuffered, (int)person.team, person.personNumber);
    }
    
    [PunRPC]
    void ReviveRpcPerson()
    {
        Debug.Log(string.Format("ReviveRpcPersonCalled"));
        gameManagerScr.RevivePerson();
    }
    
    public void RevivePersonRpc()
    {
        Debug.Log(string.Format("RpcRevivePersonCalled"));
        photonView.RPC("ReviveRpcPerson", RpcTarget.AllBuffered);
    }
}