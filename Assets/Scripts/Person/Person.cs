using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private GameObject moveCircle;
    [SerializeField] private GameObject attackCircle;
    public int mulCoef = 5;

    private Vector3 _pos;
    public Game currGame;

    private List<GameObject> _moveCircles;

    private delegate bool PossibilityToWalk(Vector3 pos);

    public enum Command
    {
        GREEN,
        RED,
        BLACK,
        YELLOW
    }
    public Command command_type = Command.GREEN;

    void Start()
    {
        _moveCircles = new List<GameObject>();
        _pos = new Vector3(1, 0, 1);
        transform.position += _pos * 0.1f;
    }

    private bool OnWaterCard(Vector3 pos)
    {
        return !(currGame.PlayingField[(int)pos.x, (int)pos.z] is EmptyCard);
    }
    
    private bool OnEmptyCard(Vector3 pos)
    {
        return !(currGame.PlayingField[(int)pos.x, (int)pos.z] is WaterCard);
    }

    private void CreateMovement(Vector3 addPos, PossibilityToWalk func)
    {
        Vector3 newPos = _pos + addPos;
        if (0 <= newPos.x && newPos.x <= 12 && 0 <= newPos.z && newPos.z <= 12 && func(newPos))
        {
            GameObject result = Instantiate(moveCircle, transform.position, Quaternion.identity);
            result.transform.position += addPos * (transform.localScale.x * mulCoef);
            _moveCircles.Add(result);
        }
    }

    public void GenerateMovements()
    {
        PossibilityToWalk func = OnEmptyCard;
        if (currGame.PlayingField[(int)_pos.x, (int)_pos.y] is EmptyCard)
        {
            func = OnEmptyCard;
        } else if (currGame.PlayingField[(int)_pos.x, (int)_pos.y] is WaterCard)
        {
            func = OnWaterCard;
        }
        CreateMovement(new Vector3(-1, 0, 0), func);
        CreateMovement(new Vector3(0, 0, 1), func);
        CreateMovement(new Vector3(0, 0, -1), func);
        CreateMovement(new Vector3(-1, 0, 1), func);
        CreateMovement(new Vector3(-1, 0, -1), func);
        CreateMovement(new Vector3(1, 0, 1), func);
        CreateMovement(new Vector3(1, 0, -1), func);
        CreateMovement(new Vector3(1, 0, 0), func);
    }

    public void CleanUpMovements(Vector3 newPos)
    {
        Vector3 posChanges = transform.position - newPos;
        if (posChanges.x < 0)
        {
            ++_pos.x;
        } else if (posChanges.x > 0)
        {
            --_pos.x;
        }
        if (posChanges.z < 0)
        {
            ++_pos.z;
        } else if (posChanges.z > 0)
        {
            --_pos.z;
        }
        
        transform.position = newPos;
        foreach (var circle in _moveCircles)
        {
            Destroy(circle);
        }
        _moveCircles.Clear();
    }
}
