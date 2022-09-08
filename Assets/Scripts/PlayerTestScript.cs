using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerTestScript : MonoBehaviour
{
    private bool debugInvinc = false;
    public bool debugSkills = false;

    public bool inDialogue;

    public Controller2D controller;
    public float speed;
    public float maxJumpHeight;
    public float minJumpHeight;
    public float timeToJumpApex;
    public float glideSpeed;

    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity;
    private float terminalVelocity;

    Vector2 directionalInput;

    public Vector3 velocity;
    float velocityXSmoothing;
    private float accelerationTimeAirborne = 0f;
    private float accelerationTimeGrounded = 0f;
    
    float nextAttackTime = 0f;
    [SerializeField] float attackRate = .66f;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackDistance = 10f;
    public int baseAttackDamage = 10;
    public int AttackDamage { get => (int)(baseAttackDamage * (rtsrEnabled ? rtsrDamageMod : 1) * (polarityEnabled ? polarityDamageMod : 1)); }
    public float rtsrDamageMod = 1.5f;
    public bool rtsrEnabled = false;
    public float polarityDamageMod = 1.2f;
    public bool polarityEnabled = false;
    public LayerMask enemyLayers;

    public IntValue currentHealth, maxHealth;
    public Signal playerHealthSignal;
    public float invincLength = 0.75f;

    private bool hitInvincible = false;
    private bool skillInvincible = false;

    public bool facingLeft = false;

    [SerializeField] float backstepCD = 0.40f;
    private float backstepTimer = 0.00f;
    [SerializeField] float backstepJump = 1.5f;
    [SerializeField] float backstepMoveTimer = 0.05f;
    [SerializeField] float backstepSpeed = 0.50f;
    [SerializeField] float backstepStunRange = 3.50f;
    [SerializeField] float backstepStunTime;
    private int backstepComboCount = 2;

    private bool dead = false;

    private int slamComboCount = 4;

    private bool doubleJumpUsed = false;
    [SerializeField] float doubleJumpStunRange = 6.0f;

    [SerializeField] float chargeStepper = 0.05f;
    [SerializeField] float currentCharge;

    private int sprayComboCount = 7;

    private int shootComboCount = 3;

    [SerializeField] float dashSpeed = 15.0f;
    float dashAngle = 0f;
    [SerializeField] float dashTime = 0.5f;
    private float dashTimer = 0.0f;
    private int dashComboCount = 5;
    public Transform dashHitbox;
    public float dashHBRange = 1f;
    public float dashHBDistance = 1f;
    private bool dashDidHit = false;
    public float dashBounceVelocity = 10f;

    public static Vector3 lastGround;
    public LayerMask groundMask;

    Animator anim;
    public GameObject attack;
    public GameObject gustStun;
    public GameObject sprayFeather; 
    public GameObject shotUI;
    public GameObject slamUI;
    public GameObject sprayUI;
    public GameObject dashUI;
    public GameObject backstepUI;
    public GameObject guardUI;
    public GameObject PauseCanvasPrefab;
    public GameObject UICanvasPrefab;
    public GameObject InGameMenuCanvasPrefab;
    public GameObject SettingsCanvasPrefab;
    public GameObject SkillGridPrefab;
    public GameObject eventSystemPrefab;
    public GameObject HBGribPrefab;
    public GameObject MainCamPrefab;
    public GameObject vmCamPrefab;

    [SerializeField] Transform camFollowTarget;
    public static Camera mainCam;
    public static GameObject vmCam;

    SkillsGridManager skillsGrid;

    public delegate void FeatherShot();
    [SerializeField] public FeatherShot fs;

    public delegate void Slam();
    [SerializeField] public Slam slam;

    public delegate void Spray();
    [SerializeField] public Spray spray;

    public delegate void Guard();
    [SerializeField] public Guard guard;

    public FeatherShotTypes featherShotsScripts;
    public SlamTypes slamTypeScripts;
    public SprayTypes sprayTypeScripts;
    public GuardTypes guardTypeScripts;

    public ProgressBarScript shotUIScript;
    public ProgressBarScript slamUIScript;
    public ProgressBarScript sprayUIScript;
    public ProgressBarScript dashUIScript;
    public ProgressBarScript backstepUIScript;
    public GuardUIScript guardUIScript;
    private List<ProgressBarScript> UISkillScripts = new List<ProgressBarScript>(5);

    [SerializeField] Sprite defaultShotUIIcon;

    public BoolValue loadFromTransition;
    [SerializeField] Signal fadeSceneOut;

    public static int blessingTotal = 7;
    public PlayerState state;

    public enum PlayerState
    {
        idle,
        walking,
        gliding,
        backstepping,
        dashing,
        guarding,
        shooting,
        spraying,
        chargingJump,
        slamming,
        attacking
    }

    public static GameObject playerInstance;
    public static GameObject eventSystem;
    public static GameObject pauseCanvas;
    public static GameObject inGameMenuCanvas;
    public static GameObject settingsCanvas;
    public static GameObject UICanvas;

    // Start is called before the first frame update
    void Start()
    {
        if(playerInstance == null)
        {
            DontDestroyOnLoad(gameObject);
            playerInstance = gameObject;

            //set up
            state = PlayerState.idle;
            controller = GetComponent<Controller2D>();
            anim = GetComponent<Animator>();

            gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
            minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
            terminalVelocity = -20f * maxJumpVelocity;

            currentCharge = maxJumpVelocity / 2;

            //camera set up
            mainCam = Instantiate(MainCamPrefab, Vector3.zero, Quaternion.identity).GetComponent<Camera>();
            CinemachineVirtualCamera vmCam = Instantiate(vmCamPrefab, Vector3.zero, Quaternion.identity).GetComponent<CinemachineVirtualCamera>();
            vmCam.Follow = camFollowTarget;

            //ui set up
            eventSystem = Instantiate(eventSystemPrefab, Vector3.zero, Quaternion.identity);
            pauseCanvas = Instantiate(PauseCanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));
            inGameMenuCanvas = Instantiate(InGameMenuCanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));
            settingsCanvas = Instantiate(SettingsCanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));
            UICanvas = Instantiate(UICanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));

            DontDestroyOnLoad(eventSystem);
            DontDestroyOnLoad(pauseCanvas);
            DontDestroyOnLoad(inGameMenuCanvas);
            DontDestroyOnLoad(settingsCanvas);
            DontDestroyOnLoad(UICanvas);


            //set cameras
            pauseCanvas.GetComponent<Canvas>().worldCamera = mainCam;
            inGameMenuCanvas.GetComponent<Canvas>().worldCamera = mainCam;
            settingsCanvas.GetComponent<Canvas>().worldCamera = mainCam;
            UICanvas.GetComponent<Canvas>().worldCamera = mainCam;

            PauseMenu pauseUIScript = pauseCanvas.GetComponent<PauseMenu>();
            pauseUIScript.SkillsUI = UICanvas;
            pauseUIScript.settingMenuUI = settingsCanvas;
            inGameMenuCanvas.GetComponent<InventoryMenu>().SkillsUICanvas = UICanvas;
            inGameMenuCanvas.GetComponent<InventoryMenu>().player = this;
            skillsGrid = UICanvas.GetComponentInChildren<SkillsGridManager>();

            //change?
            currentHealth.Value = maxHealth.Value;

            //set up skill ui's if unlocked
            if (PlayerInfo.Instance.slamUnlock)
            {
                slamUIScript = skillsGrid.AddIcon(slamUI).GetComponent<ProgressBarScript>();
                slamTypeScripts.slamUIScript = slamUIScript;
                slamUIScript.comboToCharge = slamComboCount;
                slam = slamTypeScripts.DefaultSlam;
                UISkillScripts.Add(slamUIScript);
            }
            if (PlayerInfo.Instance.sprayUnlock)
            {
                sprayUIScript = skillsGrid.AddIcon(sprayUI).GetComponent<ProgressBarScript>();
                sprayTypeScripts.sprayUIScript = sprayUIScript;
                sprayUIScript.comboToCharge = sprayComboCount;
                spray = sprayTypeScripts.DefaultSpray;
                UISkillScripts.Add(sprayUIScript);
            }
            if (PlayerInfo.Instance.shootUnlock)
            {
                shotUIScript = skillsGrid.AddIcon(shotUI).GetComponent<ProgressBarScript>();
                featherShotsScripts.shotUIScript = shotUIScript;
                featherShotsScripts.shotUIScript.comboToCharge = shootComboCount;
                fs = featherShotsScripts.DefaultShot;
                UISkillScripts.Add(shotUIScript);
            }
            if (PlayerInfo.Instance.guardUnlock)
            {
                guardUIScript = skillsGrid.AddIcon(guardUI).GetComponent<GuardUIScript>();
                guardTypeScripts.guardUIScript = guardUIScript;
                guardUIScript.maximum = guardTypeScripts.hitguardCD;
                guard = guardTypeScripts.DefaultGuard;
            }
            if (PlayerInfo.Instance.backstepUnlock)
            {
                backstepUIScript = skillsGrid.AddIcon(backstepUI).GetComponent<ProgressBarScript>();
                backstepUIScript.comboToCharge = backstepComboCount;
                UISkillScripts.Add(backstepUIScript);
            }
            if (PlayerInfo.Instance.dashUnlock)
            {
                dashUIScript = skillsGrid.AddIcon(dashUI).GetComponent<ProgressBarScript>();
                dashUIScript.comboToCharge = dashComboCount;
                UISkillScripts.Add(dashUIScript);
            }

            InvokeRepeating("UpdateLastGround", 0f, 0.33f);
        }
        else
        {
            Debug.Log("destroy player");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (loadFromTransition.Value)
        {
            transform.position = new Vector3(PlayerInfo.Instance.loadPos.x, PlayerInfo.Instance.loadPos.y, -40);
            loadFromTransition.Value = false;
        }
    }

    /// <summary>
    /// Sets the directional input to given input
    /// </summary>
    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    /// <summary>
    /// What to do when jump button is down
    /// </summary>
    public void OnJumpDown()
    {
        //don't do anything if in a pause menu
        if (Pause.isPaused)
        {
            return;
        }
        //basic jump
        if (controller.collisions.below && (state == PlayerState.idle || state == PlayerState.walking || state == PlayerState.attacking))//jump
        {
            velocity.y = maxJumpVelocity;
            controller.collisions.below = false;
            state = PlayerState.idle;
        }
        else if (PlayerInfo.Instance.doubleJumpUnlock && !doubleJumpUsed && !controller.collisions.below && 
            (state == PlayerState.idle || state == PlayerState.gliding || state == PlayerState.walking))//doublejump
        {
            velocity.y = maxJumpVelocity;
            doubleJumpUsed = true;
            state = PlayerState.gliding;
            anim.SetTrigger("DoubleJump");
            //if upgraded, spawn stun objects
            if (PlayerInfo.Instance.doubleJumpUpgrade)
            {
                RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, Vector2.down, doubleJumpStunRange, LayerMask.GetMask("Ground"));
                if (hit.transform != null)
                {
                    //not perfect
                    GameObject gust1 = Instantiate(gustStun, hit.point, Quaternion.identity);
                    Instantiate(gustStun, hit.point, Quaternion.identity);
                    gust1.GetComponent<GustScript>().left = true;
                }
            }
        }
    }

    /// <summary>
    /// Glides on jump held
    /// </summary>
    public void OnJumpHeld()
    {
        //don't do anything if in a pause menu
        if (Pause.isPaused)
        {
            return;
        }
        if (velocity.y < 0 && (state == PlayerState.idle || state == PlayerState.gliding || state == PlayerState.walking) && !controller.collisions.below)
        {
            state = PlayerState.gliding;
            velocity.y = glideSpeed;
            anim.SetBool("Gliding", true);
        }
    }

    /// <summary>
    /// Stop gliding on jump up
    /// </summary>
    public void OnJumpUp()
    {
        //don't do anything if in a pause menu
        if (Pause.isPaused)
        {
            return;
        }
        state = PlayerState.idle;
        anim.SetBool("Gliding", false);
        if (velocity.y > minJumpVelocity) //going up
        {
            velocity.y = minJumpVelocity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //don't do anything if in a pause menu
        if (Pause.isPaused)
        {
            return;
        }

        //toggle debugs
        if (Input.GetKeyDown(KeyCode.F1)) { debugInvinc = !debugInvinc; }
        if(Input.GetKeyDown(KeyCode.F2)) { debugSkills = !debugSkills; }

        //If moving left, set direction to left
        if (directionalInput.x < 0 && !facingLeft && (state != PlayerState.backstepping && state != PlayerState.dashing && state != PlayerState.slamming))
        {
            facingLeft = true;
            transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        //same but for right
        else if (directionalInput.x > 0 && facingLeft && (state != PlayerState.backstepping && state != PlayerState.dashing && state != PlayerState.slamming))
        {
            facingLeft = false;
            transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        //Set to walking/not walking
        if (directionalInput.x != 0 && controller.collisions.below)
        {
            anim.SetBool("Walking", true);
            if (state == PlayerState.idle) { state = PlayerState.walking; }
        } else if (directionalInput.x == 0 && controller.collisions.below)
        {
            anim.SetBool("Walking", false);
            if (state == PlayerState.walking) { state = PlayerState.idle; }
        }

        //Calc velocity
        CalculateVelocity();

        //Attack and skills
        if (Time.time >= nextAttackTime)
        {
            if(state == PlayerState.attacking) { state = PlayerState.idle; }
            if (Input.GetKeyDown("h") && (state == PlayerState.idle || state == PlayerState.gliding || state == PlayerState.walking))
            {
                Attack();
                nextAttackTime = Time.time + attackRate;
            }
        }
        if (PlayerInfo.Instance.chargeJumpUnlock)
        {
            ChargeJump();
        }
        if (PlayerInfo.Instance.slamUnlock && (state == PlayerState.slamming || Input.GetKeyDown("k")))
        {
            slam();
        }
        if (PlayerInfo.Instance.sprayUnlock && (state == PlayerState.spraying || Input.GetKeyDown("j")))
        {
            spray();
        }
        if (PlayerInfo.Instance.shootUnlock && (state == PlayerState.shooting || Input.GetKeyDown("l")))
        {
            fs();
        }
        if (PlayerInfo.Instance.guardUnlock && (state == PlayerState.guarding || Input.GetKeyDown("y")))
        {
            guard();
        }
        if (PlayerInfo.Instance.backstepUnlock)
        {
            Backstep();
        }
        if (PlayerInfo.Instance.dashUnlock)
        {
            Dash();
        }

        //terminal velocity
        if(velocity.y < terminalVelocity) { velocity.y = terminalVelocity; }
        controller.Move(velocity * Time.deltaTime, directionalInput);

        if(controller.collisions.below && doubleJumpUsed)
        {
            doubleJumpUsed = false;
        }
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
        if (!controller.collisions.below && !anim.GetBool("InAir"))
        {
            anim.SetBool("InAir", true);
        }
        else if(controller.collisions.below)
        {
            anim.SetBool("InAir", false);
            UpdateLastGround();
        }
    }

    /// <summary>
    /// Updates the last ground collider in storage
    /// </summary>
    void UpdateLastGround()
    {
        //update last ground
        if (controller.collisions.below && !controller.collisions.onSpikes)
        {
            lastGround = transform.position;
        }
    }

    /// <summary>
    /// Calculates the players velocity based on state and input
    /// </summary>
    void CalculateVelocity()
    {
        if (state == PlayerState.gliding || state == PlayerState.idle || 
            state == PlayerState.walking || state == PlayerState.attacking)
        {
            float targetVelocityX = directionalInput.x * speed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        velocity.y += gravity * Time.deltaTime;
    }

    /// <summary>
    /// Charge jump skill
    /// </summary>
    void ChargeJump()
    {
        if (controller.collisions.below && Input.GetKey(KeyCode.LeftControl) && PlayerInfo.Instance.chargeJumpUnlock)
        {
            if (state == PlayerState.walking || state == PlayerState.idle)//begin jump
            {
                anim.SetBool("Charging", true);
                state = PlayerState.chargingJump;
                velocity.x = 0;
            } else if (PlayerState.chargingJump == state)
            {
                currentCharge += chargeStepper;
            }
        } else if (state == PlayerState.chargingJump)//release jump
        {
            if (currentCharge > 4.0f * maxJumpVelocity)
            {
                currentCharge = 4.0f * maxJumpVelocity;
            }
            anim.SetBool("Charging", false);
            velocity = new Vector2(velocity.x, currentCharge);
            controller.collisions.below = false;
            currentCharge = maxJumpVelocity / 1.5f;
            state = PlayerState.idle;
        }
    }

    /// <summary>
    /// Basic attack
    /// </summary>
    void Attack()
    {
        state = PlayerState.attacking;
        float angle;
        //determine attack direction
        if (Input.GetAxisRaw("Horizontal") == 0) {
            if (Input.GetAxisRaw("Vertical") == 0)
            {
                angle = (Mathf.Atan2(0, 1 * (facingLeft ? -1 : 1)) * Mathf.Rad2Deg - 90);
            } 
            else { angle = (Mathf.Atan2(Input.GetAxisRaw("Vertical"), 0) * Mathf.Rad2Deg - 90); }
        } 
        else { angle = Mathf.Atan2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal")) * Mathf.Rad2Deg - 90; }

        //set anim
        anim.SetTrigger("Attack");

        //instantiate attack object
        GameObject a = Instantiate(attack, transform.position, Quaternion.AngleAxis(angle, Vector3.forward), gameObject.transform);
        a.transform.localScale = new Vector3(8, 8, 8);
        attackPoint.localPosition = new Vector3(attackDistance * Mathf.Cos((angle + 90 + (facingLeft?0:180)) * Mathf.Deg2Rad), attackDistance * Mathf.Sin((angle + 90) * Mathf.Deg2Rad), 0);

        //see if enemies are hit and do damage
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        //find closest collider and damage
        if (hitEnemies.Length == 0) { return; }
        Collider2D closestEnemy = hitEnemies[0];
        float dist = Vector2.Distance(hitEnemies[0].transform.position, transform.position);
        foreach(Collider2D enemy in hitEnemies)
        {
            float tempDist = Vector2.Distance(enemy.transform.position, transform.position);
            if(tempDist < dist) { closestEnemy = enemy; dist = tempDist; }
        }
        ComboInc();
        closestEnemy.GetComponent<EnemyCompositeHB>().TakeDamage(AttackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        //debug gizmos
        if (attackPoint == null) { return; }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        if(dashHitbox == null) { return; }
        Gizmos.DrawWireSphere(dashHitbox.position, dashHBRange);
    }

    /// <summary>
    /// Combo increased
    /// </summary>
    public void ComboInc()
    {
        foreach (ProgressBarScript progressBarScript in UISkillScripts)
        {
            progressBarScript.IncreaseCombo();
        }
    }

    /// <summary>
    /// Combo reset
    /// </summary>
    public void ComboReset()
    {
        foreach (ProgressBarScript script in UISkillScripts)
        {
            script.ResetCombo();
        }
    }

    /// <summary>
    /// Player takes damage
    /// </summary>
    /// <param name="damage">Amount of damage taken</param>
    public void TakeDamage(int damage, bool setToGround)
    {
        if (debugInvinc) { return; }

        //Only take damage if not invincible
        if (!hitInvincible && !skillInvincible)
        {
            //guarding
            if (state == PlayerState.guarding)
            {
                guardTypeScripts.Unguard(true);
                StartInvinc();
                return;
            }
            //not guarding
            currentHealth.Value -= damage;
            playerHealthSignal.RaiseSignal();
            if (currentHealth.Value <= 0 && !dead)
            {
                anim.SetTrigger("Dead");
                dead = true;
                Invoke("Respawn", 0.5f);
            }
            else if (!dead)
            {
                ComboReset();
                if (setToGround)
                {
                    //add screen fade and animations
                    SetToLastGround();
                    return;
                }
                StartInvinc();
            }
        }
    }

    /// <summary>
    /// Sets the player to their last ground position
    /// </summary>
    public void SetToLastGround()
    {
        transform.position = lastGround;
    }

    /// <summary>
    /// Unfinished function for respawning player at their last spawn point
    /// </summary>
    public void Respawn()
    {
        fadeSceneOut.RaiseSignal();
        SceneManager.LoadScene(PlayerInfo.Instance.sceneName);
        gameObject.transform.position = PlayerInfo.Instance.loadPos;
    }

    /// <summary>
    /// Starts the invincible animation and sets invincible to true
    /// </summary>
    private void StartInvinc()
    {
        hitInvincible = true;
        Invoke("EndInvinc", invincLength);
        anim.SetBool("Invinc", true);
    }

    /// <summary>
    /// Ends the invinc animation and sets invincible to false
    /// </summary>
    private void EndInvinc()
    {
        hitInvincible = false;
        anim.SetBool("Invinc", false);
    }

    /// <summary>
    /// Function for player backstep skill
    /// </summary>
    private void Backstep()
    {
        //start backstep
        if ((state == PlayerState.idle || state == PlayerState.walking) && Input.GetButtonDown("Shift") 
            && backstepUIScript.charge >= 1 && controller.collisions.below)
        {
            backstepUIScript.ResetCombo();
            state = PlayerState.backstepping;
            anim.SetTrigger("Backstep");
            velocity = new Vector2(velocity.x, backstepJump);
            //stun enemy in front of player
            if (facingLeft)
            {
                RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, Vector2.left, backstepStunRange, LayerMask.GetMask("Enemies"));
                if (hit.collider != null)
                {
                    if (hit.collider.tag == "Enemy")
                    {
                        hit.collider.gameObject.GetComponent<EnemyHealth>().Stun(backstepStunTime);
                    }
                }
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, Vector2.right, backstepStunRange, LayerMask.GetMask("Enemies"));
                if (hit.transform != null)
                {
                    if (hit.transform.gameObject.tag == "Enemy")
                    {
                        hit.collider.gameObject.GetComponent<EnemyHealth>().Stun(backstepStunTime);
                    }
                }
            }
        }
        //if backstepping, move back quickly
        if (backstepTimer < backstepCD && state == PlayerState.backstepping)
        {
            if (backstepTimer > backstepMoveTimer && state == PlayerState.backstepping)
            {
                velocity = new Vector2(backstepSpeed * (facingLeft ? 1 : -1), velocity.y);
            }
            backstepTimer += Time.deltaTime;
        }
        //backstep finished
        else if (backstepTimer >= backstepCD)
        {
            backstepTimer = 0;
            state = PlayerState.idle;
        }
    }

    /// <summary>
    /// Function for player dash skill
    /// </summary>
    private void Dash()
    {
        Vector2 dashDirection;
        //start dashing
        if ((state == PlayerState.gliding || state == PlayerState.idle || state == PlayerState.walking)
            && PlayerInfo.Instance.dashUpgrade && Input.GetKeyDown("u") && dashUIScript.charge >= 1) //upgraded dash
        {
            dashDidHit = false;
            dashUIScript.ResetCombo();
            anim.SetBool("Dashing", true);
            state = PlayerState.dashing;
            //determine direction
            if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            {
                dashDirection = new Vector2((facingLeft) ? -1 : 1, 0);
            } else { dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); }
            dashAngle = Mathf.Atan2(dashDirection.y, dashDirection.x);
            velocity = new Vector3(dashSpeed * Mathf.Cos(dashAngle), dashSpeed * Mathf.Sin(dashAngle));

        } else if ((state == PlayerState.gliding || state == PlayerState.idle || state == PlayerState.walking)
            && Input.GetKeyDown("u") && dashUIScript.charge >= 1) // no upgrade
        {
            dashDidHit = false;
            dashUIScript.ResetCombo();
            anim.SetBool("Dashing", true);
            state = PlayerState.dashing;
            dashDirection = new Vector2((facingLeft) ? -dashSpeed : dashSpeed, 0);
            dashAngle = (facingLeft) ? math.PI : 0;
            velocity = dashDirection;
        } //if dashing, move at dash speed
        else if (state == PlayerState.dashing)
        {
            dashTimer += Time.deltaTime;
            //if hit an enemy, bounce upward
            if (dashDidHit)
            {
                dashTimer = 0;
                anim.SetBool("Dashing", false);
                anim.SetTrigger("DoubleJump");
                gameObject.transform.rotation = quaternion.identity;
                state = PlayerState.idle;
                velocity.y = dashBounceVelocity;

                skillInvincible = true;
                Invoke("StopSkillInvinc", 0.25f);

                return;
            }

            //stop dashing
            if (dashTimer >= dashTime)
            {
                dashTimer = 0;
                anim.SetBool("Dashing", false);
                gameObject.transform.rotation = quaternion.identity;
                state = PlayerState.idle;
                return;
            }

            //determine if dash hit enemies
            if (!dashDidHit && !controller.collisions.below)
            {
                dashHitbox.localPosition = new Vector3(dashHBDistance * Mathf.Cos((float)(dashAngle + (facingLeft ? 0 : Math.PI))), dashHBDistance * Mathf.Sin(dashAngle), 0);

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(dashHitbox.position, dashHBRange, enemyLayers);

                foreach (Collider2D enemy in hitEnemies)
                {
                    dashDidHit = true;
                    ComboInc();
                    enemy.GetComponent<EnemyHealth>().TakeDamage(AttackDamage);
                    return;
                }
            }

            //set velocity
            velocity = new Vector3(dashSpeed * Mathf.Cos(dashAngle), dashSpeed * Mathf.Sin(dashAngle));
        }
    }

    /// <summary>
    /// Disables skill invincibility
    /// </summary>
    public void StopSkillInvinc()
    {
        skillInvincible = false;
    }

    /// <summary>
    /// Function for switching the feather shot skill from blessings
    /// </summary>
    /// <param name="type">Type to switch to</param>
    public void ChangeFeather(FeatherTypes type, Sprite icon)
    {
        shotUIScript.UpdateImage(icon);

        switch (type)
        {
            case FeatherTypes.Default:
                fs = featherShotsScripts.DefaultShot;
                break;
            case FeatherTypes.Fireball:
                fs = featherShotsScripts.Fireball;
                break;
            case FeatherTypes.Boomerang:
                fs = featherShotsScripts.BoomerangShot;
                break;
            case FeatherTypes.BallLightning:
                fs = featherShotsScripts.BallLightning;
                break;
        }
    }

    public void ChangeFeather(FeatherTypes type)
    {
        ChangeFeather(type, defaultShotUIIcon);
    }

    public void ChangeGuard(GuardSwapTypes type)
    {
        switch (type)
        {
            case GuardSwapTypes.Default:
                guard = guardTypeScripts.DefaultGuard;
                break;
            case GuardSwapTypes.QuickCharge:
                guard = guardTypeScripts.QuickCharge;
                break;
            case GuardSwapTypes.Retaliation:
                guard = guardTypeScripts.Retaliation;
                break;
        }
    }

    public void ChangeSlam(SlamSwapTypes type)
    {
        switch (type)
        {
            case SlamSwapTypes.Default:
                slam = slamTypeScripts.DefaultSlam;
                break;
            case SlamSwapTypes.Wave:
                slam = slamTypeScripts.Wave;
                break;
            case SlamSwapTypes.Poison:
                slam = slamTypeScripts.Poison;
                break;
        }
    }

    public void ChangeSpray(SpraySwapTypes type)
    {
        switch (type)
        {
            case SpraySwapTypes.Default:
                spray = sprayTypeScripts.DefaultSpray;
                break;
            case SpraySwapTypes.Lightning:
                spray = sprayTypeScripts.LightningStrike;
                break;
            case SpraySwapTypes.MoreFeathers:
                spray = sprayTypeScripts.MoreFeathers;
                break;
        }
    }
}