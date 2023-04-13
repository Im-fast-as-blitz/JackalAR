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
        Debug.Log(string.Format("SetGameObjCalled"));
        currGame = game;
    }

    [PunRPC]
    void DebugRpc(int x, int y)
    {
        Debug.Log(string.Format("DebugRpcCalled"));
        currGame.PlayingField[x, y].Open();
    }

    public void DebugRpc()
    {
        Debug.Log(string.Format("DebugFromPrcCalled"));
        photonView.RPC("DebugRpc", RpcTarget.AllBuffered, 2, 4);
    }

    [PunRPC]
    void SyncCards(IReadOnlyList<int[]> cardTypes, IReadOnlyList<int> rotMass)
    {
        Debug.Log(string.Format("SyncCardsCalled"));
        int rotInd = 0;
        for (var i = 0; i < currGame.PlayingField.GetLength(0); ++i)
        {
            for (var j = 0; j < currGame.PlayingField.GetLength(1); ++j)
            {
                currGame.PlayingField[i, j] = (Card)Cards.createCardByType[(Card.CardType)cardTypes[i][j]].NewObj();
                var curCard = currGame.PlayingField[i, j];
                var curType = curCard.Type;
                if (curCard is ArrowCard arrowCard && curType != Card.CardType.ArrowStraight4 && curType != Card.CardType.ArrowDiagonal4)
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
            cardTypes[i] = new int[massSize];
            for (var j = 0; j < currGame.PlayingField.GetLength(1); ++j)
            {
                var curCard = currGame.PlayingField[i, j];
                var curType = curCard.Type;
                cardTypes[i][j] = (int)currGame.PlayingField[i, j].Type;
                if (curCard is ArrowCard card && curType != Card.CardType.ArrowStraight4 && curType != Card.CardType.ArrowDiagonal4)
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

    // [PunRPC]
    // void OpenCard(IReadOnlyList<int> pos)
    // {
    //     Debug.Log($"OpenCardCalled {pos[0]} {pos[1]}");
    //     currGame.PlayingField[pos[0], pos[1]].Open();
    // }
    //
    // public void OpenCardRpc(IntVector2 position)
    // {
    //     int[] pos = new int[2];
    //     pos[0] = position.x;
    //     pos[1] = position.z;
    //     Debug.Log($"OpenCardFromRpcCalled {pos[0]} {pos[1]}");
    //     photonView.RPC("OpenCard", RpcTarget.AllBuffered, pos);
    // }

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
        currGame.Persons[(Teams)team][personNum].Move(new Vector3(x, y, z), false);
    }
    
    public void MovePersonRpc(Vector3 pos, Teams team, int personNum)
    {
        Debug.Log(string.Format("RpcMovePersonCalled"));
        photonView.RPC("MovePerson", RpcTarget.OthersBuffered, pos.x, pos.y, pos.z, (int)team, personNum);
    }
}