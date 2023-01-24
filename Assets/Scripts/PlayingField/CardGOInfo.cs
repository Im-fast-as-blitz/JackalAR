using System;
using UnityEngine;

public class CardGOInfo : MonoBehaviour
{
    [NonSerialized] public IntVector2 FieldPosition;
    [NonSerialized] public Card OwnCard;
}

public class IntVector2
{
    public int x;
    public int y;

    public IntVector2(int first, int second)
    {
        x = first;
        y = second;
    }
};