using System;
using UnityEngine;

public class CardGOBehaviourScr : MonoBehaviour {
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Person"))
        {
            Open();
        }
    }
    
    public void Open()
    {
        CardGOInfo gOInfo = GetComponent<CardGOInfo>();
        string logoPath = gOInfo.OwnCard.LogoPath;
        Material gOMaterial = Resources.Load(logoPath, typeof(Material)) as Material;
        if (gOMaterial)
        {
            GetComponent<Renderer>().material = gOMaterial;
        }
        else
        {
            throw new Exception("Can't find path while opening");
        }
        Card ownCard = GetComponent<CardGOInfo>().OwnCard;
        ownCard.OpenAction();
        ownCard.StepAction();
    }
}
