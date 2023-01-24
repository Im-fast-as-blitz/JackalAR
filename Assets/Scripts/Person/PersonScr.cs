using System.Collections.Generic;
using UnityEngine;

public class PersonScr : MonoBehaviour
{
    [SerializeField] private GameObject moveCircle;
    [SerializeField] private GameObject attackCircle;

    private int _pos_x, _pos_y;

    private List<GameObject> _moveCircles;

    /*
    1 - green
    2 - red
    3 - black
    4 - yellow 
    */
    public int command_type = 0;
    void Start()
    {
        _moveCircles = new List<GameObject>();
        _pos_y = 0;
        _pos_x = 0;
    }

    void Update()
    {
        
    }

    private GameObject CreateMovement(Vector3 addPos)
    {
        GameObject result = Instantiate(moveCircle, transform.position, Quaternion.identity);
        result.transform.position += addPos * transform.localScale.x;
        return result;
    }

    public void GenerateMovements()
    {
        moveCircle.SetActive(true);
        _moveCircles.Add(CreateMovement(new Vector3(-2, 0, 0)));
        _moveCircles.Add(CreateMovement(new Vector3(0, 0, 2)));
        _moveCircles.Add(CreateMovement(new Vector3(0, 0, -2)));
        _moveCircles.Add(CreateMovement(new Vector3(-2, 0, 2)));
        _moveCircles.Add(CreateMovement(new Vector3(-2, 0, -2)));
        _moveCircles.Add(CreateMovement(new Vector3(2, 0, 2)));
        _moveCircles.Add(CreateMovement(new Vector3(2, 0, -2)));
        moveCircle.transform.position = transform.position + (new Vector3(2, 0, 0) * transform.localScale.x);
    }

    public void CleanUpMovements(Vector3 newPos)
    {
        transform.position = newPos;
        foreach (var circle in _moveCircles)
        {
            Destroy(circle);
        }
        _moveCircles.Clear();
        moveCircle.transform.localPosition = new Vector3(0, 0, 0);
        moveCircle.SetActive(false);
    }
}
