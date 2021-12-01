using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    static public FightManager main;
    [SerializeField] GameObject figthPrefab = null;
    Animator fightLogo;

    // Start is called before the first frame update
    void Awake()
    {
        if (main != null)
        {
            Destroy(gameObject);
            return;
        }
        main = this;

        fightLogo = GameObject.Find("FightAlert").GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Fight CreateFight()
    {
        DialogueManager.Main.EndDialogue();
		        
		//Start Fight Music
        MusicsManager.instance.musicState.Add(MusicsManager.MusicKind.Going_To_War);
        //
		
        if (TradingManager.main.GetComponent<CanvasGroup>().alpha == 1)
        {
            TradingManager.main.CancelTrade();
        }
        Fight newFight = Instantiate(figthPrefab).GetComponent<Fight>();
        newFight.tag = "Fight";
        fightLogo.SetBool("FightStarted", true);
        return newFight;
    }
}
