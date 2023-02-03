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

    public void UpdateLogo()
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
            throw new Exception("Can't find path while opening or updating");
        }
    }

    public void LoadShipLogo()
    {
        CardGOInfo gOInfo = GetComponent<CardGOInfo>();
        WaterCard waterCard = gOInfo.OwnCard as WaterCard;
        string shipLogoPath = waterCard.OwnShip.LogoPath;
        Material gOMaterial = Resources.Load(shipLogoPath, typeof(Material)) as Material;
        if (gOMaterial)
        {
            GetComponent<Renderer>().material = gOMaterial;
        }
        else
        {
            throw new Exception("Can't find path while loading ship");
        }
    }
    
    public void Open()
    { 
        UpdateLogo();
        Card ownCard = GetComponent<CardGOInfo>().OwnCard;
        ownCard.OpenAction();
        ownCard.StepAction();
    }
}
