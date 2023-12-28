using UnityEngine;
using UnityEngine.UI;

public class CheckerDataClone
{
    public int ownerId;
    public int clientID;
    public bool IsKing = false;
    public CheckerDataClone(CheckerData data)
    {
        ownerId = data.ownerId;
        clientID = data.clientID;
        IsKing = data.IsKing;
    }
}


public class CheckerData : MonoBehaviour 
{
    //public int id;
    public int ownerId;
    public int clientID;
    public bool IsKing = false;

    public Vector2 position = new Vector2();
    public Vector3 targetPosition;
    public int direction
    {
        get
        {
            return (ownerId == 1) ? 1 : -1;
        }
    }
    public bool isMoving;

    public GameObject kingSprite;
    public RuntimeAnimatorController[] kingEffectAnims;

    public Sprite whiteKingSprite, blackKingSprite;

    public void updateCheckerInfo(CheckerInfo checkerInfo)
    {
        ownerId = checkerInfo.ownerId;
        clientID = checkerInfo.clientID;
        IsKing = checkerInfo.IsKing;
    }

    public void updatePosition(Vec2 pos, bool isInstantPos)
    {
        position = new Vector2(pos.x, pos.y);
        if(isInstantPos) transform.position = targetPosition;
    }

    public void updateKingSp()
    {
        if (IsKing)
        {
            kingSprite.GetComponent<Animator>().runtimeAnimatorController = kingEffectAnims[PlayerPrefs.GetInt("KingAnimID", 0)];
            kingSprite.SetActive(true);
        }
    }

    public void shakeChecker()
    {
    }
}
