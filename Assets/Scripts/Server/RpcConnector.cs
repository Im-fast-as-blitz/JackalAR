using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RpcConnector : MonoBehaviourPun
{
    public Game currGame;
    public GameManagerScr gameManagerScr;
    
    public void SetGameObj(Game game, GameManagerScr gameManagerScript)
    {
        Debug.Log(string.Format("SetGameObjCalled"));
        currGame = game;
        gameManagerScr = gameManagerScript;
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
    void SyncCards(int[][] cardTypes)
    {
        Debug.Log(string.Format("SyncCardsCalled"));
        for (int i = 0; i < currGame.PlayingField.GetLength(0); ++i)
        {
            for (int j = 0; j < currGame.PlayingField.GetLength(1); ++j)
            {
                currGame.PlayingField[i, j] = (Card)(Cards.createCardByType[(Card.CardType)cardTypes[i][j]].NewObj());
            }
        }
        currGame.PlaceShips();
        gameManagerScr.BuildPlayingField(new Vector3(0, 0, 0));
    }
    
    public void SyncCardsRpc()
    {
        Debug.Log(string.Format("SyncCardsFromRpcCalled"));
        int[][] cardTypes = new int[currGame.PlayingField.GetLength(0)][];
        for (int i = 0; i < currGame.PlayingField.GetLength(0); ++i)
        {
            cardTypes[i] = new int[currGame.PlayingField.GetLength(1)];
            for (int j = 0; j < currGame.PlayingField.GetLength(1); ++j)
            {
                cardTypes[i][j] = (int)currGame.PlayingField[i, j].Type;
            }
        }

        this.photonView.RPC("SyncCards", RpcTarget.AllBuffered, cardTypes);
    }
    
    [PunRPC]
    void OpenCard(int[] pos)
    {
        Debug.Log(string.Format("OpenCardCalled {0} {1}", pos[0], pos[1]));
        currGame.PlayingField[pos[0], pos[1]].Open();
    }
   
    public void OpenCardRpc(IntVector2 Position)
    {
        int[] pos = new int[2];
        pos[0] = Position.x;
        pos[1] = Position.z;
        Debug.Log(string.Format("OpenCardFromRpcCalled {0} {1}", pos[0], pos[1]));
        this.photonView.RPC("OpenCard", RpcTarget.AllBuffered, pos);
    }

    [PunRPC]
    void ChangeCurTeam(int curTeam)
    {
        currGame.currentNumTeam = curTeam;
    }

    public void ChangeCurTeamRpc(int currentTeam)
    {
        photonView.RPC("ChangeCurTeam", RpcTarget.OthersBuffered, currentTeam);
    }
}
