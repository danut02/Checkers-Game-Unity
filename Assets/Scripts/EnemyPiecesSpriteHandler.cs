using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyPiecesSpriteHandler : MonoBehaviour, IEnemyPiece
{
    public int ownerPlayerId;
    public SpriteRenderer SR;
    [HideInInspector] public bool isEnemyPiece;

    private InGameUI inGameUI;

    private void Awake()
    {
        inGameUI = InGameUI.instance;
    }

    private void Start()
    {
        if (GetComponent<CheckerData>()) ownerPlayerId = GetComponent<CheckerData>().ownerId;

        //if (isEnemyPiece)
        //{
        //    if (ownerPlayerId == 1)
        //        inGameUI._UpdateWhiteEnemyPieces_SP += updateSprite;
        //    else
        //        inGameUI._UpdateBlackEnemyPieces_SP += updateSprite;
        //}

        InGameUI.FlipPiece += OnFlipPiece;

        if (InGameUI.IsViewFlipped) OnFlipPiece(true); //Flip new pieces if view is already flipped
    }

    public void IsEnemyPiece()
    {
        isEnemyPiece = true;
        if (ownerPlayerId == 1)
            InGameUI.instance._UpdateWhiteEnemyPieces_SP += updateSprite;
        else
            InGameUI.instance._UpdateBlackEnemyPieces_SP += updateSprite;
    }

    public void updateSprite(Sprite newSprite)
    {
        SR.sprite = newSprite;
    }

    //Flip View
    private void OnFlipPiece(bool IsFlipped)
    {
        Vector3 rot = transform.eulerAngles;
        rot.y = IsFlipped ? 180 : 0;

        transform.eulerAngles = rot;
    }

    private void OnDestroy()
    {
        if(isEnemyPiece)
        {
            if (ownerPlayerId == 1)
                InGameUI.instance._UpdateWhiteEnemyPieces_SP -= updateSprite;
            else
                InGameUI.instance._UpdateBlackEnemyPieces_SP -= updateSprite;
        }

        InGameUI.FlipPiece -= OnFlipPiece;
    }
}
