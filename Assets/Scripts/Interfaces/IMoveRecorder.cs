public interface IMoveRecorder
{
    public void IRecordMove(CheckerData checker, CheckerData killedData, Vec2 from, Vec2 to, Vec2 killedPos, int playerID);
}
