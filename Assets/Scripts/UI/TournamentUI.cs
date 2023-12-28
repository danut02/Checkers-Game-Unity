using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentUI : MonoBehaviour
{
    public GameObject ui,text,ui_topPlayers;

    public float time1;
    [SerializeField] private NetworkManager networkManager;

    public bool click_on=false;
    // Start is called before the first frame update
    public void Start()
    {

        ui.SetActive(false);

    }

    void Update()
    {
        if(click_on)
        time1 += Time.deltaTime;
        if (time1 > 5.0f)
        {
            text.SetActive(false);
            ui_topPlayers.SetActive(true);
            // networkManager.findMatch();
        }
        
    }
    // Update is called once per frame
    public void PlayButton()
    {
        networkManager.findMatch();
        
    }
   public void ClickButton()
   {
       click_on = true;
        ui.SetActive(true);
        
    }
}
