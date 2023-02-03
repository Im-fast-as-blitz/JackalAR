using UnityEngine;
using System.Collections.Generic;

public class Ship
{
    public string LogoPath;
    public List<Person> Figures = new List<Person>();
    public Helpers.IntVector2 Position;

    public Ship(string logoPath, Helpers.IntVector2 position)
    {
        LogoPath = logoPath;
        Position = position;
    }
}

public static class Ships
{
    public static Dictionary<string, Ship> AllShips;
}

public class ShipManagerScr : MonoBehaviour
{
    public void Awake()
    {
        Ships.AllShips.Add("white", new Ship("Ships/white", new Helpers.IntVector2(7, 0)));
        Ships.AllShips.Add("black", new Ship("Ships/black", new Helpers.IntVector2(0, 7)));
        Ships.AllShips.Add("red", new Ship("Ships/red", new Helpers.IntVector2(13, 7)));
        Ships.AllShips.Add("yellow", new Ship("Ships/yellow", new Helpers.IntVector2(7, 13)));
    }
}
