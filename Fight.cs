using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct FighterData
{
    public FighterData(Character _character, SphereCollider _area)
    {
        character = _character;
        area = _area;
    }

    public Character character;
    public SphereCollider area;
}

public class Fight : MonoBehaviour, IComparer<FighterData>
{
    public List<FighterData> fighters = new List<FighterData>();

    int ennemyNb = 0;
    int allyNb = 0;
    public int current = 0;
    public static float turnTime = 6f;
    public bool sortTimeLine = false;
    public int updateTimeLine = 0;
    bool isNewTurn = true;
    public int Current { get => current; set => current = value; }
    public bool IsNewTurn { get => isNewTurn; set => isNewTurn = value; }

    static bool FightsMerging = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public int Compare(FighterData _fighter_1, FighterData _fighter_2)
    {
        return (_fighter_1.character.GetMobility < _fighter_2.character.GetMobility) ? 1 : (_fighter_1.character.GetMobility > _fighter_2.character.GetMobility) ? -1 : Random.Range(-1, 2);
    }

    // Update is called once per frame
    void Update()
    {
        FightsMerging = false;
        if (((fighters[current].character.CurrentPa <= 0 && fighters[current].character.Hostility == CHARACTER_HOSTILITY.ALLY) 
            || (/*fighters[current].character.NavAgent.enabled && */fighters[current].character.State == CHARACTER_STATE.LOCKED))
            && !PlayerManager.main.ItsPlayer(fighters[current].character))
        {
                EndTurn();
        }

        checkForDeath();

        for (int i = 0; i < fighters.Count; i++)
        {
            FighterData fData = fighters[i];
            if (!fData.area)
            {
                fData.area = gameObject.AddComponent<SphereCollider>();
                fData.area.radius = 7f;
                fData.area.center = fData.character.transform.position;
                fData.area.isTrigger = true;
            }
            fData.area.center = fData.character.transform.position;
        }
    }

    void CallNextFighter()
    {
        current++;
        if (current >= fighters.Count)
        {
            isNewTurn = true;
            current = 0;
            fighters.Sort(this);
        }
        sortTimeLine = true;
        CameraBehaviour.instance.target = fighters[current].character.GetComponent<NavMeshAgent>();
        fighters[current].character.StartTurn();
    }

    public void EndTurn()
    {
        fighters[current].character.EndTurn();
        CallNextFighter();
    }

    public void SetFighter(List<Character> _characters)
    {
        for (int x = 0; x < _characters.Count; ++x)
        {
            if (!_characters[x].IsDead && _characters[x].enabled)
            {
                _characters[x].State = CHARACTER_STATE.LOCKED;
                BoxCollider bxColl;
                _characters[x].TryGetComponent<BoxCollider>(out bxColl);               
                if(bxColl != null) bxColl.enabled = false;

                if (_characters[x].GetComponent<NavMeshAgent>().remainingDistance > 0)
                {
                    _characters[x].GetComponent<NavMeshAgent>().ResetPath();
                }

                SphereCollider area = gameObject.AddComponent<SphereCollider>();
                area.radius = 7f;
                area.center = _characters[x].transform.position;
                area.isTrigger = true;
                FeedBack.HighlightEntity(_characters[x]);
                _characters[x].CurrentFight = this;
                fighters.Add(new FighterData(_characters[x], area));

                if (_characters[x].Hostility == CHARACTER_HOSTILITY.ALLY)
                {
                    ++allyNb;
                }
                else
                {
                    ++ennemyNb;
                }
            }
        }

        fighters.Sort(this);
        fighters[0].character.State = CHARACTER_STATE.FIGHT;
    }

    public void checkForDeath()
    {
        // browses the fighters to check if someone has died
        // updates the number of fighters and the current player in consequence
        for (int x = 0; x < fighters.Count;)
        {
            //Debug.Log(fighters[x].character.CurrentFight);
            if (fighters[x].character.IsDead)
            {   
                FightHUD.main.DestroyPortrait(x);
                Destroy(fighters[x].area);

                if (PlayerManager.main.ItsPlayer(fighters[x].character)) DontDestroyOnLoad(fighters[x].character);

                fighters[x].character.CurrentFight = null;
                fighters.RemoveAt(x);
                if (current > x)
                {
                    --current;
                }
                else if (current == x)
                {
                    --current;
                    CallNextFighter();
                }
            }
            else
            {
                ++x;
            }
        }

        allyNb = 0;
        ennemyNb = 0;
        for(int x = 0; x < fighters.Count; x++)
        {
            if(fighters[x].character.Hostility == CHARACTER_HOSTILITY.ALLY)
            {
                ++allyNb;
            }
            else
            {
                ++ennemyNb;
            }
        }

        // if there is no ally or ennemy anymore then the fight is ended
        if (allyNb == 0 || ennemyNb == 0)
        {
            BeforeDestroy();
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        handleCollision(other);
    }

    public void OnTriggerStay(Collider other)
    {
        handleCollision(other);
    }

    private void handleCollision(Collider other)
    {
        //if (other.tag == "Fight" && !FightsMerging)
        //{
        //    FightsMerging = true;
        //    // merges the fight and place each fighter at the right place
        //    // depending on if he has already played and his velocity
        //    Fight fight = other.GetComponent<Fight>();
        //    int lastChecked = fight.current;
        //    while (fighters.Count > 0)
        //    {
        //        FighterData fD = fighters[0];
        //        fD.area = null;
        //        fighters[0] = fD;
        //        //current > 0 means the character has already play and so it's not importante to sort it
        //        if (current > 0)
        //        {
        //            fighters[0].character.CurrentFight = fight;
        //            fight.fighters.Insert(0, fighters[0]);
        //        }
        //        // otherwise it need to be inserted at the right place
        //        else
        //        {
        //            bool inserted = false;
        //            for (int y = lastChecked; y < fight.fighters.Count; ++y)
        //            {
        //                int comparison = Compare(fighters[0], fight.fighters[y]);
        //                if (comparison == -1)
        //                {
        //                    if (y == fight.current)
        //                    {
        //                        fight.fighters[y].character.State = CHARACTER_STATE.LOCKED;
        //                    }
        //                    fighters[0].character.CurrentFight = fight;
        //                    fight.fighters.Insert(y, fighters[0]);
        //                    lastChecked = y + 1;
        //                    inserted = true;

        //                    break;
        //                }
        //                else if (comparison == 0)
        //                {
        //                    if (Random.value > .5)
        //                    {
        //                        if (current == 0)
        //                        {
        //                            fighters[0].character.State = CHARACTER_STATE.LOCKED;
        //                        }
        //                        fighters[0].character.CurrentFight = fight;
        //                        fight.fighters.Insert(y + 1, fighters[0]);
        //                        lastChecked = y + 1;
        //                    }
        //                    else
        //                    {
        //                        if (y == fight.current)
        //                        {
        //                            fight.fighters[y].character.State = CHARACTER_STATE.LOCKED;
        //                        }
        //                        fighters[0].character.CurrentFight = fight;
        //                        fight.fighters.Insert(y, fighters[0]);
        //                        lastChecked = y + 1;
        //                    }
        //                    inserted = true;
        //                    break;
        //                }
        //            }
        //            if (!inserted)
        //            {
        //                fighters[0].character.CurrentFight = fight;
        //                fight.fighters.Add(fighters[0]);
        //            }
        //        }
        //        //fighters[0].area.center = fighters[0].character.transform.position;
        //        fighters.RemoveAt(0);
        //        --current;
        //    }

        //    fight.ennemyNb += ennemyNb;
        //    fight.allyNb += allyNb;
        //    FightHUD.main.InitializeTimeline(fight);
        //    Destroy(gameObject);
        //}
        //else
        if ((other.tag == "Character" || other.tag == "Player") && other.GetComponent<Character>().CurrentFight == null)
        {
            Character character = other.GetComponent<Character>();

            if (character && character.enabled && !character.IsDead && character.Hostility != CHARACTER_HOSTILITY.NEUTRAL && character.State == CHARACTER_STATE.FREE)
            {
                character.State = CHARACTER_STATE.LOCKED;
                BoxCollider bxColl;
                character.TryGetComponent<BoxCollider>(out bxColl);
                if (bxColl != null) bxColl.enabled = false;

                SphereCollider area = gameObject.AddComponent<SphereCollider>();
                area.radius = 7f;
                area.center = character.transform.position - transform.position;
                area.isTrigger = true;
                FeedBack.HighlightEntity(character);
                character.CurrentFight = this;
                fighters.Add(new FighterData(character, area));

                ++updateTimeLine;
            }
        }        
    }

    private void BeforeDestroy()
    {
        FightHUD.main.EndCombat();

        // frees all the fighters and set them to free
        for (int x = 0; x < fighters.Count; ++x)
        {
            if (fighters[x].character.CurrentFight == this) fighters[x].character.CurrentFight = null;
            fighters[x].character.State = CHARACTER_STATE.FREE;            
            BoxCollider bxColl;
            fighters[x].character.TryGetComponent<BoxCollider>(out bxColl);
            if (bxColl != null) bxColl.enabled = !fighters[x].character.IsDead;
            if (PlayerManager.main.ItsPlayer(fighters[x].character)) DontDestroyOnLoad(fighters[x].character);


        }        
        PlayerManager.main.PlayerFairy.State = CHARACTER_STATE.FREE;
        if (PlayerManager.main.PlayerFairy.CurrentFight == this) PlayerManager.main.PlayerFairy.CurrentFight = null;
        PlayerManager.main.PlayerRogue.State = CHARACTER_STATE.FREE;
        if (PlayerManager.main.PlayerRogue.CurrentFight == this) PlayerManager.main.PlayerRogue.CurrentFight = null;
        PlayerManager.main.PlayerTank.State = CHARACTER_STATE.FREE;
        if (PlayerManager.main.PlayerTank.CurrentFight == this) PlayerManager.main.PlayerTank.CurrentFight = null;

        //End Fight Music
        MusicsManager.instance.musicState.Remove(MusicsManager.MusicKind.Going_To_War);
        //
    }
}
