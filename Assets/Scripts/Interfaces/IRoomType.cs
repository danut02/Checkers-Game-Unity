public interface IRoomType
{
    public void OnJoined_Room();
    public void OnLeft_Room();

    public void OnPlayerEntered_Room(Photon.Realtime.Player NewPlayer);
    public void OnPlayerLeft_Room(Photon.Realtime.Player OtherPlayer);
}
