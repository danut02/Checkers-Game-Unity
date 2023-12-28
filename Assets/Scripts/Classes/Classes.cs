using System;

using System.Collections.Generic;

[Serializable]
public class Vec2
{
    public Vec2()
    {

    }

    public Vec2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int x;
    public int y;

    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(Vec2))
        {
            return (obj as Vec2).x == this.x && (obj as Vec2).y == this.y;
        }
        return false;
    }
}

[Serializable]
public class Move
{
    public Vec2 From;

    public Vec2 To;

    public List<Move> Children;

    public Move(Vec2 from, Vec2 to)
    {
        From = from;
        To = to;
    }
    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(Move))
        {
            Move other = obj as Move;
            return other.From.Equals(this.From) && other.To.Equals(this.To);
        }
        return false;
    }
}

public class CheckerInfo
{
    public int ownerId;
    public int clientID;
    public bool IsKing = false;

    public CheckerInfo(CheckerData checkerData)
    {
        ownerId = checkerData.ownerId;
        clientID = checkerData.clientID;
        IsKing = checkerData.IsKing;
    }
}

public class RecordMove
{
    //public CheckerData movedChecker;
    public CheckerInfo movedCheckerInfo;
    public CheckerInfo capturedPieceData;
    public int PlayerID;

    public Vec2 From;
    public Vec2 To;
    public Vec2 capturePos;

    public RecordMove(CheckerData checker, CheckerData captured, Vec2 from, Vec2 to, Vec2 capturePos, int playerId)
    {
        movedCheckerInfo = new CheckerInfo(checker);
        if (captured != null) capturedPieceData = new CheckerInfo(captured);
        From = from;
        To = to;
        this.capturePos = capturePos;
        PlayerID = playerId;
    }
    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(RecordMove))
        {
            RecordMove other = obj as RecordMove;
            return other.From.Equals(this.From) && other.To.Equals(this.To);
        }
        return false;
    }
}
