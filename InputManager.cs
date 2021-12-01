using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class InputManager : Controller
{
    public enum AgentID
    {
        CHARACTER1,
        CHARACTER2,
        CHARACTER3,
        FOLLOW,
        AGENTID_COUNT
    }

    public static InputManager main;

    NavMeshAgent[] agent = new NavMeshAgent[4];
    [SerializeField] Character[] characters = new Character[3];
    public AgentID mainCharacter = AgentID.CHARACTER1;
    RaycastHit mousePointedOut = new RaycastHit();
    bool isMousePointing = false;
    bool isMainMoving = false;
    [SerializeField] Portrait[] portrait = null;

    Character MainCharacter { get => characters[(int)mainCharacter]; }
    NavMeshAgent MainAgent { get => agent[(int)mainCharacter]; }

    // the speed at which the troop move
    // is set to the lowest speed less 1 in that way the troop doesn't spread to much
    float troopSpeed;
    [SerializeField] GameObject lineObject;// = new LineRenderer();
    LineRenderer line;// = new LineRenderer();

    // speed at which a unit turn around herself
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    //PlayerManager
    PlayerManager pm;

    void Start()
    {
        if (main != null)
        {
            Destroy(gameObject);
            Debug.Log("Permanent loaded twice, look-up for an error !");
            return;
        }

        main = this;
        lineObject = Instantiate(lineObject, transform);
        line = lineObject.GetComponent<LineRenderer>();
        characters[0] = PlayerManager.main.PlayerFairy;
        characters[1] = PlayerManager.main.PlayerTank;
        characters[2] = PlayerManager.main.PlayerRogue;
        // stores all the agents and set their speed and acceleration
        // should probably be done in the character constructor
        agent[(int)AgentID.CHARACTER1] = characters[(int)AgentID.CHARACTER1].GetComponent<NavMeshAgent>();
        agent[(int)AgentID.CHARACTER1].speed = characters[(int)AgentID.CHARACTER1].GetMobility * 2f;
        agent[(int)AgentID.CHARACTER1].acceleration = characters[(int)AgentID.CHARACTER1].GetMobility * 6f;

        agent[(int)AgentID.CHARACTER2] = characters[(int)AgentID.CHARACTER2].GetComponent<NavMeshAgent>();
        agent[(int)AgentID.CHARACTER2].speed = characters[(int)AgentID.CHARACTER2].GetMobility * 2f;
        agent[(int)AgentID.CHARACTER2].acceleration = characters[(int)AgentID.CHARACTER2].GetMobility * 6f;

        agent[(int)AgentID.CHARACTER3] = characters[(int)AgentID.CHARACTER3].GetComponent<NavMeshAgent>();
        agent[(int)AgentID.CHARACTER3].speed = characters[(int)AgentID.CHARACTER3].GetMobility * 2f;
        agent[(int)AgentID.CHARACTER3].acceleration = characters[(int)AgentID.CHARACTER3].GetMobility * 6f;

        // gets back the camera target's navemesh
        agent[(int)AgentID.FOLLOW] = GetComponentInChildren<NavMeshAgent>();

        troopSpeed = Mathf.Min(agent[(int)AgentID.CHARACTER1].speed, Mathf.Min(agent[(int)AgentID.CHARACTER2].speed, agent[(int)AgentID.CHARACTER3].speed)) - 1f;

        pm = PlayerManager.main;
    }

    protected override void Update()
    {
        if (SceneManager.GetActiveScene().name != "menu")
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // gets what the mouse points at
        isMousePointing = Physics.Raycast(CameraBehaviour.instance.mainCamera.ScreenPointToRay(Input.mousePosition), out mousePointedOut) 
                                            && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        // execute default action such as auto attack and, later, talking to people etc... and return if an action is done
        if (DefaultInteraction()) return;

        if (MainCharacter.SpellCasted)
        {
            CastPendingSpell();            

            //if (MainCharacter.State != CHARACTER_STATE.FREE)
            { return; }
        }        

        ChangeMainCharacter();

        switch (MainCharacter.State)
        {
            case CHARACTER_STATE.FIGHT:
                HandleFightInput();
                return;

            case CHARACTER_STATE.FREE:
                HandleFreeInput();
                break;

            case CHARACTER_STATE.LOCKED:
                HandleLockInput();
                break;
        }
        //MainCharacter.gameObject.transform.GetChild(0).GetComponent<Animator>().SetBool("isAttacking", false);
        //MainCharacter.gameObject.transform.GetChild(1).GetComponent<Animator>().SetBool("isAttacking", false);
    }

    protected bool AutoAttack(Entity _entity)
    {
        //MainCharacter.gameObject.transform.GetChild(0).GetComponent<Animator>().SetBool("isAttacking", false);
        //MainCharacter.gameObject.transform.GetChild(1).GetComponent<Animator>().SetBool("isAttacking", false);

        if (!_entity.IsDead)
        {
            Spell tmpSpell;
            if (MainCharacter.AutoSpellCasting)
            {
                tmpSpell = MainCharacter.SpellCasted;
            }
            else
            {
                tmpSpell = MainCharacter.AutoAttack?.Activate();
            }

            if (tmpSpell)
            {
                MainCharacter.AutoSpellCasting = true;
                tmpSpell.ShowTrajectoryPreview = true;
                if (Input.GetMouseButtonDown(0))
                {
                    if (tmpSpell.SetTarget(_entity, _entity.transform.position))
                    {
                        MainCharacter.AutoSpellCasting = false;                        
                    }
                    else
                    {
                        MainCharacter.AutoSpellCasting = false;
                        tmpSpell.Cancel();
                    }
                    return true;
                }
                else
                {
                    tmpSpell.ShowTrajectoryPreview = tmpSpell.ValidateTarget(_entity, _entity.transform.position);
                    return true;
                }
            }
        }

        MainCharacter.AutoSpellCasting = false;
        MainCharacter.SpellCasted?.Cancel();
        return false;
    }

    protected bool DefaultInteractionWithPickable(Entity _pointedEntity)
    {
        if (_pointedEntity as Pickable
            && Input.GetMouseButtonDown(0)
            && Vector3.Distance(_pointedEntity.transform.position, PlayerManager.main.FocusedCharacter.transform.position) <= 4.0f)
        {
            (_pointedEntity as Pickable).TakeHim(PlayerManager.main.FocusedCharacter);
            return true;
        }
        return false;
    }
   
    protected bool DefaultInteractionWithContainer(Container _pointedContainer)
    {
        if( Input.GetMouseButtonDown(0) 
            && Vector3.Distance(_pointedContainer.transform.position, PlayerManager.main.FocusedCharacter.transform.position) <= 4.0f)
        {
            ItemGenerator itGen;
            itGen = _pointedContainer.GetComponentInChildren<ItemGenerator>();
            if(itGen != null)
            {
                itGen.Open();
            }
            _pointedContainer.ShowLoot();
            return true;
        }
        return false;
    }
    protected bool DefaultInteractionWithCharacter(Entity _pointedEntity)
    {
        if (_pointedEntity as Character)
        {
            Character pointedCharacter = _pointedEntity as Character;
            if (pointedCharacter)
            {
                switch (pointedCharacter.Hostility)
                {
                    case CHARACTER_HOSTILITY.ENEMY:
                        return AutoAttack(pointedCharacter);
                    default:
                        break;
                }
            }
        }
        return false;
    }

    protected bool DefaultInteraction()
    {
        Character currentPlayer = MainCharacter;
        if (currentPlayer.State != CHARACTER_STATE.LOCKED &&
           (!currentPlayer.SpellCasted || currentPlayer.AutoSpellCasting) &&
            isMousePointing)
        {
            Entity pointedEntity = mousePointedOut.collider.GetComponent<Entity>();
            ChangeLvl changLvl = mousePointedOut.collider.GetComponent<ChangeLvl>();
            FeedBack.HighlightEntity(pointedEntity);
            if (PlayerManager.main.ItsPlayer(pointedEntity as Character)) return false;

            if (pointedEntity && changLvl == null)
            {
                if (DefaultInteractionWithCharacter(pointedEntity))      return true;
                else if (DefaultInteractionWithPickable(pointedEntity))  return true;
            
            }
            else if (currentPlayer.AutoSpellCasting)
            {
                currentPlayer.AutoSpellCasting = false;
                currentPlayer.SpellCasted?.Cancel();
            }
            else if (    changLvl  
                        && Vector3.Distance(changLvl.transform.position, PlayerManager.main.FocusedCharacter.transform.position) <= 4.0f
                        && Input.GetMouseButtonDown(0)
                         )
            {
                changLvl.LoadNextlvl();
                return true;
            }
        }
        else if (currentPlayer.AutoSpellCasting)
        {
            currentPlayer.AutoSpellCasting = false;
            currentPlayer.SpellCasted?.Cancel();
        }
        if (currentPlayer.State == CHARACTER_STATE.FREE    
                    && (!currentPlayer.SpellCasted || currentPlayer.AutoSpellCasting)
                    && isMousePointing)
        {
            Container pointedContainer = mousePointedOut.collider.GetComponent<Container>();
            if (pointedContainer != null && DefaultInteractionWithContainer(pointedContainer)) return true;
        }
        return false;
    }

    override protected void HandleFreeInput()
    {
        ////if the camera were following the FOLLOW, then it follow the main character + the Cam go to ThirdPersonView
        if (CameraBehaviour.instance.thirdPersonCamera.m_YAxis.m_InputAxisName == "Mouse ScrollWheel")
        {
            CameraBehaviour.instance.isGoingToThirdPersonView = true;
            CameraBehaviour.instance.target = MainAgent;
        }

        //// no need to draw the line in free mode
        if (line.enabled)
        {
            line.enabled = false;
        }

        ////// gets the direction given by the inputs and normalize it
        ////*****
        Vector3 speed = new Vector3(0f, 0f, 0f);
        speed.x = Input.GetAxisRaw("Horizontal");
        speed.z = Input.GetAxisRaw("Vertical");

        if (speed.x != 0f && speed.z != 0f)
        {            
            speed.Normalize();
        }
        else
        {
            //MainAgent.gameObject.transform.GetChild(0).GetComponent<Animator>().SetBool("isMoving", false);
            //MainAgent.gameObject.transform.GetChild(1).GetComponent<Animator>().SetBool("isMoving", false);
        }

        ////*****


        if (isMousePointing)
        {
            if (mousePointedOut.collider.tag == "Character")
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (mousePointedOut.collider.gameObject.GetComponent<DialogueTrigger>() != null && Vector3.Distance(PlayerManager.main.FocusedCharacter.transform.position, mousePointedOut.collider.gameObject.transform.position) < 4.0f)
                    {
                        mousePointedOut.collider.gameObject.GetComponent<DialogueTrigger>().TriggerDialogue(mousePointedOut.collider.gameObject.GetComponent<Character>());
                    }
                    else if(mousePointedOut.collider.gameObject.GetComponent<Character>().IsMerchant && Vector3.Distance(PlayerManager.main.FocusedCharacter.transform.position, mousePointedOut.collider.gameObject.transform.position) < 4.0f)
                    {
                        mousePointedOut.collider.gameObject.GetComponent<Character>().MarchandInventoryMake();
                        PlayerManager.main.FocusedCharacter.State = CHARACTER_STATE.LOCKED;
                        TradingManager.main.CurrentTrader = mousePointedOut.collider.gameObject.GetComponent<Character>();
                        TradingManager.main.OpenTrade();
                    }                    
                }
            }
        }

        ////// calculates the path of the main agent and set the target of his mates
        MoveTarget(speed, MainAgent);

        // updates camera behaviour
        CameraBehaviour.instance.TurnCameraAround();
    }

    override protected void HandleFightInput()
    {
        //Activate Cam Zoom and put Cam in "Combat Mode" 
        if (CameraBehaviour.instance.thirdPersonCamera.m_YAxis.m_InputAxisName == "")
        {
            CameraBehaviour.instance.isGoingToOrbitalView = true;
        }

        // when player comes from free mode the line is disabled
        // so need to enable it for the fight
        if (!line.enabled)
        {
            line.enabled = true;
        }

        //Activate cam free mode when cam target is null
        Vector3 speed = new Vector3(0f, 0f, 0f);
        speed.x = Input.GetAxisRaw("Horizontal");
        speed.z = Input.GetAxisRaw("Vertical");

        if (speed.x != 0f || speed.z != 0f)
        {
            CameraBehaviour.instance.target = null;
        }
        //-----------

        if (MainAgent.enabled && MainAgent.gameObject.activeSelf)
        {
            if (!isMainMoving)
            {
                // get the object the mouse click on
                if (isMousePointing)
                {
                    line.enabled = false;
                    if (mousePointedOut.collider.tag == "Terrain")
                    {
                        NavMeshPath path = new NavMeshPath();
                        // access the path from the character trough the mousePos
                        MainAgent.CalculatePath(mousePointedOut.point, path);

                        float maxDist = MainCharacter.GetMobility * (MainCharacter.CurrentPa + ((MainCharacter.MobilityMov) ? 1 : 0)) - MainCharacter.DistCovered;
                        if (maxDist > 0 && path.corners.Length > 0)
                        {
                            line.enabled = true;
                            // stores the max distance the character can browses
                            float distance = 0;

                            // the list of all point in the pass
                            List<Vector3> maxPath = new List<Vector3>();
                            maxPath.Add(path.corners[0]);

                            // calculates the length of each segment and in the way to recreate the path but limited by the max distance the character can browses
                            for (int c = 0; c < path.corners.Length - 1; ++c)
                            {
                                // calculates the distances between the current corner and the next one
                                float segmentDist = Vector3.Distance(path.corners[c], path.corners[c + 1]);

                                // if the current distance makes the character goes over its max distance
                                // it is normalised and multiplied by le last amount of distance the character can browses to get the final point of its trajectory
                                if (distance + segmentDist > maxDist)
                                {
                                    float finalSegmentDist = maxDist - distance;
                                    maxPath.Add(path.corners[c] + (path.corners[c + 1] - path.corners[c]).normalized * finalSegmentDist);
                                    distance = maxDist;

                                    // because the character has browses the max distance he can, we don't need to calculate the remaining path
                                    break;
                                }
                                // increases the total amount of browsed distance by the current segment length
                                distance += segmentDist;
                                // add the next corner to the path
                                maxPath.Add(path.corners[c + 1]- transform.position);
                            }

                            // calculates the cost of the path the player is going to move on
                            VisuActionPoints.main.MovementCost = Mathf.Max(0, (int)((distance + MainCharacter.DistCovered) / MainCharacter.GetMobility) - ((MainCharacter.MobilityMov) ? 1 : 0));

                            line.positionCount = maxPath.Count;
                            line.SetPositions(maxPath.ToArray());

                            // to click gives to the character the order to move trought the path
                            if (Input.GetButtonDown("LeftClick"))
                            {
                                isMainMoving = true;
                                //MainAgent.SetDestination(maxPath[maxPath.Count - 1]);
                                MainCharacter.SetDestination(maxPath[maxPath.Count - 1]);
                                MainCharacter.DistTotal = distance;

                                /*if (MainCharacter.MobilityMov)
                                {
                                    MainCharacter.MobilityMov = false;
                                }

                                MainCharacter.CurrentPa -= VisuActionPoints.main.MovementCost;*/
                            }
                        }
                    }
                }
                else if (line.positionCount > 0)
                {
                    line.positionCount = 0;
                    VisuActionPoints.main.MovementCost = 0;
                }
            }
            else // the current character is traveling through its pass
            {
                //Warning Distance Covered can be Infinite if the trajectory isn't straight

                // calculate the length of the remaining path, starting from the character pos to the end of the path
                //********************
                //********************
                Vector3[] corners = MainAgent.path.corners;
                float remainingDistance = Vector3.Distance(MainAgent.transform.position - new Vector3(0f, MainAgent.baseOffset, 0f), MainAgent.steeringTarget);
                int x = 0;
                while (x < corners.Length)
                {
                    if (Input.GetKey(KeyCode.Space)) break;

                    if (corners[x] == MainAgent.steeringTarget)
                    {
                        break;
                    }
                    ++x;
                }

                while (x < corners.Length - 1)
                {
                    if (Input.GetKey(KeyCode.Space)) break;

                    remainingDistance += Vector3.Distance(corners[x], corners[x + 1]);
                    ++x;
                }
                //********************
                //********************

                MainCharacter.DistCovered += MainCharacter.DistTotal - remainingDistance;
                MainCharacter.DistTotal = remainingDistance;

                if (MainCharacter.DistCovered >= MainCharacter.GetMobility - 0.0001f)
                {
                    if (MainCharacter.MobilityMov)
                    {
                        MainCharacter.MobilityMov = false;
                    }
                    else
                    {
                        --MainCharacter.CurrentPa;
                        --VisuActionPoints.main.MovementCost;
                    }
                    MainCharacter.DistCovered -= MainCharacter.GetMobility;
                }

                if (MainAgent.remainingDistance == 0f)
                {
                    isMainMoving = false;
                }
            }
        }

        CameraBehaviour.instance.TurnCameraAround();
    }

    override protected void HandleLockInput()
    {
        if (line.enabled)
        {
            line.enabled = false;
        }

        CameraBehaviour.instance.TurnCameraAround();
    }

    public void MoveTarget(Vector3 _direction, NavMeshAgent _follow)
    {
        if (!MainAgent.gameObject.activeSelf) return;
        if (_direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg + CameraBehaviour.instance.mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(_follow.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            _follow.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            _follow.Move(moveDir.normalized * troopSpeed * Time.deltaTime);            

            int time = -1;
            for (AgentID id = AgentID.CHARACTER1; id <= AgentID.CHARACTER3; ++id)
            {
                if (id != mainCharacter && agent[(int)id].gameObject.activeSelf)
                {
                    if (!agent[(int)id].gameObject.GetComponent<Character>().IsDead)
                    {                        
                        Vector3 runTo = MainAgent.transform.position + (- MainAgent.transform.forward * 2.0f + time * MainAgent.transform.right);
                        NavMeshHit hit;
                        NavMesh.SamplePosition(runTo, out hit, 10, NavMesh.AllAreas);                   

                        //agent[(int)id].SetDestination(MainAgent.transform.position
                        //   + 2.0f * (-MainAgent.transform.forward + time * MainAgent.transform.right));
                        agent[(int)id].gameObject.GetComponent<Character>().SetDestination(hit.position);
                        //agent[(int)id].SetDestination(hit.position);
                        time += 2;
                    }
                }
            }
        }
    }

    void CastPendingSpell()
    {
        if (isMousePointing)
        {
            Entity entity = mousePointedOut.collider.GetComponentInParent<Entity>();
            if (Input.GetMouseButtonDown(0))
            {
                MainCharacter.SpellCasted.SetTarget(entity, mousePointedOut.point);
            }
            else
            {
                MainCharacter.SpellCasted.ValidateTarget(entity, mousePointedOut.point);
            }

            MainCharacter.SpellCasted.ShowTrajectoryPreview = true;
        }
        else if (VisuPortraitsPermHUD.FocusedPortrait)
        {
            Entity entity = VisuPortraitsPermHUD.FocusedPortrait.GetCharacter();
            if (Input.GetMouseButtonDown(0))
            {
                MainCharacter.SpellCasted.SetTarget(entity, entity.transform.position);
            }
            else
            {
                MainCharacter.SpellCasted.ValidateTarget(entity, entity.transform.position);                
            }

            MainCharacter.SpellCasted.ShowTrajectoryPreview = true;
        }
        else
        {
            MainCharacter.SpellCasted.ShowTrajectoryPreview = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            MainCharacter.SpellCasted.Cancel();
        }

        CameraBehaviour.instance.TurnCameraAround();
    }

    // changes the main character
    // in free mode the character is changed by clicking on a mate
    // in fight mode the character is changed automatically
    void ChangeMainCharacter()
    {
        ///********************
        /// /!\ NEED TO CHECK THIS FUNCTION
        if (MainCharacter.State != CHARACTER_STATE.FIGHT)
        {
            for (AgentID id = AgentID.CHARACTER1; id < AgentID.FOLLOW; ++id)
            {
                if (pm.FocusedCharacter.Equals(characters[(int)id]))
                {
                    mainCharacter = id;
                    if (agent[(int)id].isOnNavMesh) agent[(int)id].ResetPath();

                    break;
                }
            }
        }


        if (MainCharacter.State == CHARACTER_STATE.FREE)
        {
            if (isMousePointing && Input.GetButtonDown("LeftClick") && mousePointedOut.collider.tag == "Player")
            {
                for (AgentID id = AgentID.CHARACTER1; id < AgentID.FOLLOW; ++id)
                {
                    if (characters[(int)id].gameObject.Equals(mousePointedOut.collider.gameObject))
                    {
                        pm.FocusedCharacter = characters[(int)id];

                        mainCharacter = id;
                        agent[(int)id].ResetPath();
                        CameraBehaviour.instance.target = MainAgent;
                        break;
                    }
                }
            }
        }
        else if (MainCharacter.State == CHARACTER_STATE.LOCKED)
        {
            for (AgentID id = AgentID.CHARACTER1; id < AgentID.FOLLOW; ++id)
            {
                if (characters[(int)id].State == CHARACTER_STATE.FIGHT)
                {
                    pm.FocusedCharacter = characters[(int)id];

                    mainCharacter = id;
                    if (agent[(int)id].enabled) agent[(int)id].ResetPath();
                    break;
                }
            }
        }
    }
}