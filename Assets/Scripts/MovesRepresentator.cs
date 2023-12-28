using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovesRepresentator : MonoBehaviour
{
    //All moves marks
    [SerializeField] private GameObject redMark;
    [SerializeField] private GameObject greenMark;
    [SerializeField] private GameObject yellowMark;
    [SerializeField] private GameObject FieldPrefab;

    private List<GameObject> spawnedMarks = new List<GameObject>();

    public Dictionary<string, Queue<GameObject>> markPool = new Dictionary<string, Queue<GameObject>>();

    private int crntPlayerId = 0;

    public Toggle previousMoveToggle;

    private void Start()
    {
        previousMoveToggle.isOn = PlayerPrefs.GetInt("PrevMoves", 1) == 1 ? true : false;
    }

    public void PieceMoved(int playerID, Vec2 from, Vec2 to, bool isLastMove)
    {
        if(playerID != crntPlayerId) //Firt move (Init Pos)
        {
            //Clearing previous marks
            crntPlayerId = playerID;
            EnqueAllMarks();
            SpawnMoveMark(from, redMark);
        }

        //Landing Position
        if(!isLastMove)
            SpawnMoveMark(to, yellowMark);
        else
            SpawnMoveMark(to, greenMark);
    }

    private void SpawnMoveMark(Vec2 pos, GameObject mark)
    {
        GameObject _mark = GetMark(mark);
        _mark.SetActive(previousMoveToggle.isOn);
        _mark.transform.position = GetFieldPosition(pos, false);
        spawnedMarks.Add(_mark);
    }

    public Vector3 GetFieldPosition(Vec2 pos, bool isChecker)
    {
        SpriteRenderer renderer = FieldPrefab.GetComponent<SpriteRenderer>();
        float fieldSize = renderer.sprite.bounds.size.x * FieldPrefab.transform.localScale.x;
        float startFieldPosition = -(fieldSize * 8 / 2);
        return new Vector3(startFieldPosition + pos.x * fieldSize, startFieldPosition + pos.y * fieldSize, (isChecker ? -1.0f : 0.0f));
    }

    private GameObject GetMark(GameObject mark)
    {
        if (markPool.TryGetValue(mark.name, out Queue<GameObject> objectList))
        {
            if (objectList.Count == 0)
                return CreateNewObject(mark);
            else
            {
                GameObject _mark = objectList.Dequeue();
                _mark.SetActive(true);
                return _mark;
            }
        }
        else
            return CreateNewObject(mark);
    }

    private GameObject CreateNewObject(GameObject mark)
    {
        GameObject _mark = Instantiate(mark);
        _mark.SetActive(previousMoveToggle.isOn);
        _mark.name = mark.name;
        return _mark;
    }
    public void EnqueAllMarks()
    {
        foreach (var item in spawnedMarks)
        {
            EnqueMark(item);
        }
        spawnedMarks.Clear();
    }
    private void EnqueMark(GameObject mark)
    {
        if (markPool.TryGetValue(mark.name, out Queue<GameObject> objectList))
        {
            objectList.Enqueue(mark);
        }
        else
        {
            Queue<GameObject> newPool = new Queue<GameObject>();
            newPool.Enqueue(mark);
            markPool.Add(mark.name, newPool);
        }
        mark.SetActive(false);
    }

    public void OnTogglePrevMoves()
    {
        PlayerPrefs.SetInt("PrevMoves", previousMoveToggle.isOn ? 1 : 0);
        foreach (var item in spawnedMarks)
        {
            item.SetActive(previousMoveToggle.isOn);
        }
    }
}
