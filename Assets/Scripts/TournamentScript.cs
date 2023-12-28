using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentScript : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    public int nr_rounds = 0;
    void Start()
    {
  
        players.Add(new Player());
        players.Add(new Player());
        players.Add(new Player());

       
        StartTournament();
    }

    void StartTournament()
    {
        while (nr_rounds <= Mathf.CeilToInt(Mathf.Log(players.Count, 2)))
        {
            StartRound();
            nr_rounds++;
        }

    }
     public Player GetRandomPlayers(List<Player> playerList)
    {
        int randomIndex = Random.Range(0, playerList.Count);
        return playerList[randomIndex];
    }
    void StartRound()
    {
       

        List<Player> roundPlayers = new List<Player>(players);

        while (roundPlayers.Count > 1)
        {
            Player player1 = GetRandomPlayers(roundPlayers);
            roundPlayers.Remove(player1);

            Player player2 = GetRandomPlayers(roundPlayers);
            roundPlayers.Remove(player2);

         
            SimulateGame(player1, player2);

          
            RecordResult(player1, player2);
        }

      

        foreach (Player player in players)
        {
            Debug.Log(player.myId);
        }
    }

    

    void SimulateGame(Player player1, Player player2)
    {
        
        if (Random.Range(0f, 1f) > 0.5f)
        {
            player1.points++;
        }
        else
        {
            player2.points++;
        }
    }

    void RecordResult(Player player1, Player player2)
    {
        for(int i = 0; i < 7; i++)
        {
           // player1[i, 1] = Instantiate(player1, new Vector3(i * 5.11f, 1 * 5.11f, -1), Quaternion.identity);
            //player2[i, 6] = Instantiate(player2, new Vector3(i * 5.11f, 6 * 5.11f, -1), Quaternion.identity);
        }
    }
}


