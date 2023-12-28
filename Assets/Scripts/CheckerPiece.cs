using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckerPiece : CheckerData
{
    private void Start()
    {
        InGameUI.FlipPiece += OnFlipPiece;

        OnFlipPiece(InGameUI.IsViewFlipped); //Flip new pieces if view is already flipped
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.5f);
            if (Vector3.Distance(transform.position, targetPosition) < .1f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
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
        InGameUI.FlipPiece -= OnFlipPiece;
    }
}
