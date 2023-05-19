using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulesManager : MonoBehaviour
{
    public GameObject prevSection;
    public GameObject nextSection;
    public GameObject rules;
    public GameObject cards;
    public GameObject rulesInfo;
    public GameObject cardsInfo;

    private void Open(bool flag)
    {
        prevSection.SetActive(!flag);
        nextSection.SetActive(flag);
        rules.SetActive(flag);
        cards.SetActive(!flag);
        rulesInfo.SetActive(flag);
        cardsInfo.SetActive(!flag);
    }
    
    public void OpenRules()
    {
        Open(true);
    }

    public void OpenCardsInfo()
    {
        Open(false);
    }
}
