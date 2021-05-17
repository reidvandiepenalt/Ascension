using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEditor.Experimental.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTestScript : MonoBehaviour
{
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
    public int attackDamage = 50;
    public LayerMask enemyLayers;

    public IntValue currentHealth, maxHealth;
    public Signal playerHealthSignal;
    public float invincLength = 0.75f;
    private float invincIntervalTimer = 0.0f;
    private float invincTotalTimer = 0.0f;
    public float invincIntervalLength = 0.15f;
    private bool opaque = false;

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
    [SerializeField] bool backstepUnlock = true;
    private int backstepComboCount = 2;

    private bool dead = false;

    [SerializeField] bool slamUnlock = true;
    private int slamComboCount = 4;

    [SerializeField] bool doubleJumpUnlock = true;
    private bool doubleJumpUsed = false;
    [SerializeField] bool doubleJumpUpgrade = true;
    [SerializeField] float doubleJumpStunRange = 6.0f;

    [SerializeField] bool chargeJumpUnlock = true;
    [SerializeField] float chargeStepper = 0.05f;
    [SerializeField] float currentCharge;

    [SerializeField] bool sprayUnlock = true;
    private int sprayComboCount = 7;

    
    [SerializeField] bool shootUnlock = true;
    private int shootComboCount = 3;

    [SerializeField] bool guardUnlock = true;

    [SerializeField] bool dashUnlock = true;
    [SerializeField] bool dashUpgrade = true;
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
    public GameObject HBGribPrefab;

    GameObject pauseCanvas;
    GameObject inGameMenuCanvas;
    GameObject settingsCanvas;
    GameObject UICanvas;
    Camera mainCam;

    GameObject skillsGrid;

    public delegate void FeatherShot();
    [SerializeField] public FeatherShot fs;

    public delegate void Slam();
    [SerializeField] public Slam slam;

    public delegate void Spray();
    [SerializeField] public Spray spray;

    public delegate void Guard();
    [SerializeField] public Guard guard;

    public FeatherShotTypes featherShotsScript;
    public SlamTypes slamTypeScripts;
    public SprayTypes sprayTypeScripts;
    public GuardTypes guardTypeScripts;

    private ProgressBarScript shotUIScript;
    private ProgressBarScript slamUIScript;
    private ProgressBarScript sprayUIScript;
    private ProgressBarScript dashUIScript;
    private ProgressBarScript backstepUIScript;
    private GuardUIScript guardUIScript;
    private List<ProgressBarScript> UISkillScripts = new List<ProgressBarScript>(5);


    public VectorValue startingPosition;
    [SerializeField] Vector3 spawnPoint;
    public Vector3 SpawnPoint
    {
        set
        {
            spawnPoint = value;
        }
    }

    private Component[] spriteComponents;

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

    // Start is called before the first frame update
    void Start()
    {
        //set up
        state = PlayerState.idle;
        controller = GetComponent<Controller2D>();
        anim = GetComponent<Animator>();
        spriteComponents = GetComponentsInChildren(typeof(SpriteRenderer));

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        terminalVelocity = -300f * maxJumpVelocity;

        currentCharge = maxJumpVelocity / 2;


        //ui set up
        pauseCanvas = Instantiate(PauseCanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));
        inGameMenuCanvas = Instantiate(InGameMenuCanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));
        settingsCanvas = Instantiate(SettingsCanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));
        UICanvas = Instantiate(UICanvasPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0));

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
        skillsGrid = Instantiate(SkillGridPrefab, UICanvas.transform);

        //change?
        currentHealth.Value = maxHealth.Value;

        //set up skill ui's if unlocked
        if (slamUnlock)
        {
            slamUIScript = Instantiate(slamUI, skillsGrid.transform).GetComponent<ProgressBarScript>();
            slamTypeScripts.slamUIScript = slamUIScript;
            slamUIScript.comboToCharge = slamComboCount;
            slam = slamTypeScripts.DefaultSlam;
            UISkillScripts.Add(slamUIScript);
        }
        if (sprayUnlock)
        {
            sprayUIScript = Instantiate(sprayUI, skillsGrid.transform).GetComponent<ProgressBarScript>();
            sprayTypeScripts.sprayUIScript = sprayUIScript;
            sprayUIScript.comboToCharge = sprayComboCount;
            spray = sprayTypeScripts.DefaultSpray;
            UISkillScripts.Add(sprayUIScript);
        }
        if (shootUnlock)
        {
            shotUIScript = Instantiate(shotUI, skillsGrid.transform).GetComponent<ProgressBarScript>();
            featherShotsScript.shotUIScript = shotUIScript;
            featherShotsScript.shotUIScript.comboToCharge = shootComboCount;
            fs = featherShotsScript.DefaultShot;
            UISkillScripts.Add(shotUIScript);
        }
        if (guardUnlock)
        {
            guardUIScript = Instantiate(guardUI, skillsGrid.transform).GetComponent<GuardUIScript>();
            guardTypeScripts.guardUIScript = guardUIScript;
            guardUIScript.maximum = guardTypeScripts.hitguardCD;
            guard = guardTypeScripts.DefaultGuard;
        }
        if (backstepUnlock)
        {
            backstepUIScript = Instantiate(backstepUI, skillsGrid.transform).GetComponent<ProgressBarScript>();
            backstepUIScript.comboToCharge = backstepComboCount;
            UISkillScripts.Add(backstepUIScript);
        }
        if (dashUnlock)
        {
            dashUIScript = Instantiate(dashUI, skillsGrid.transform).GetComponent<ProgressBarScript>();
            dashUIScript.comboToCharge = dashComboCount;
            UISkillScripts.Add(dashUIScript);
        }

        //set position to starting position of room?
        transform.position = startingPosition.initialValue;
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
        //basic jump
        if (controller.collisions.below && (state == PlayerState.idle || state == PlayerState.walking || state == PlayerState.attacking))//jump
        {
            velocity.y = maxJumpVelocity;
            controller.collisions.below = false;
            doubleJumpUsed = false;
            state = PlayerState.idle;
        }
        else if (doubleJumpUnlock && !doubleJumpUsed && !controller.collisions.below && 
            (state == PlayerState.idle || state == PlayerState.gliding))//doublejump
        {
            velocity.y = maxJumpVelocity;
            doubleJumpUsed = true;
            state = PlayerState.gliding;
            anim.SetTrigger("DoubleJump");
            //if upgraded, spawn stun objects
            if (doubleJumpUpgrade)
            {
                RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, Vector2.down, doubleJumpStunRange, LayerMask.GetMask("Ground"));
                if (hit.transform != null)
                {
                    float distance = gameObject.transform.position.y - hit.transform.position.y - 0.25f; //not perfect
                    GameObject gust1 = Instantiate(gustStun, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - distance, gameObject.transform.position.z), Quaternion.identity);
                    GameObject gust2 = Instantiate(gustStun, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - distance, gameObject.transform.position.z), Quaternion.identity);
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
        if (velocity.y < 0 && (state == PlayerState.idle || state == PlayerState.gliding) && !controller.collisions.below)
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
        if (PauseMenu.IsPaused || InventoryMenu.InInventory)
        {
            return;
        }

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
        if (chargeJumpUnlock)
        {
            ChargeJump();
        }
        if (slamUnlock && (state == PlayerState.slamming || Input.GetKeyDown("k")))
        {
            slam();
        }
        if (sprayUnlock && (state == PlayerState.spraying || Input.GetKeyDown("j")))
        {
            spray();
        }
        if (shootUnlock && (state == PlayerState.shooting || Input.GetKeyDown("l")))
        {
            fs();
        }
        if (guardUnlock && (state == PlayerState.guarding || Input.GetKeyDown("y")))
        {
            guard();
        }
        if (backstepUnlock)
        {
            Backstep();
        }
        if (dashUnlock)
        {
            Dash();
        }
        
        //invincible anim effect
        if (hitInvincible)
        {
            SpriteBlinkingEffect();
        }

        //terminal velocity
        if(velocity.y < terminalVelocity * Time.deltaTime) { velocity.y = terminalVelocity * Time.deltaTime; }
        controller.Move(velocity * Time.deltaTime, directionalInput);

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
        if (controller.collisions.below && Input.GetKey(KeyCode.LeftControl) && chargeJumpUnlock)
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
        a.transform.localScale = new Vector3(6, 6, 6);
        attackPoint.localPosition = new Vector3(attackDistance * Mathf.Cos((angle + 90 + (facingLeft?0:180)) * Mathf.Deg2Rad), attackDistance * Mathf.Sin((angle + 90) * Mathf.Deg2Rad), 0);

        //see if enemies are hit and do damage
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach(Collider2D enemy in hitEnemies)
        {
            ComboInc();
            enemy.GetComponent<IEnemy>().TakeDamage(attackDamage);
        }
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
    public void TakeDamage(int damage)
    {
        //Only take damage if not invincible
        if (!hitInvincible && !skillInvincible)
        {
            //guarding
            if (state == PlayerState.guarding)
            {
                guardTypeScripts.Unguard(true);
                hitInvincible = true;
                return;
            }
            //not guarding
            currentHealth.Value -= damage;
            playerHealthSignal.RaiseSignal();
            if (currentHealth.Value <= 0 && !dead)
            {
                anim.SetTrigger("Dead");
                dead = true;
            }
            else if (!dead)
            {
                hitInvincible = true;
                ComboReset();
            }
        }
    }

    /// <summary>
    /// Unfinished function for respawning player at their last spawn point
    /// </summary>
    public void Respawn()
    {
        gameObject.transform.position = spawnPoint;
        
    }

    /// <summary>
    /// Unworking function for making player sprite blink when invincible
    /// </summary>
    private void SpriteBlinkingEffect() //not working
    {
        invincTotalTimer += Time.deltaTime;
        //no longer invinc to reset to opaque
        if (invincTotalTimer >= invincLength)
        {
            hitInvincible = false;
            opaque = false;
            invincTotalTimer = 0.0f;
            foreach (SpriteRenderer sprite in spriteComponents)
            {
                sprite.color = new Color(1f, 1f, 1f, 1f);
            }
            return;
        }

        //alternate between opaque and translucent
        invincIntervalTimer += Time.deltaTime;
        if (invincIntervalTimer >= invincIntervalLength)
        {
            invincIntervalTimer = 0.0f;
            if (opaque)
            {
                foreach (SpriteRenderer sprite in spriteComponents)
                {
                    sprite.color = new Color(1f, 1f, 1f, 1f);
                }
            }
            else
            {
                foreach (SpriteRenderer sprite in spriteComponents)
                {
                    sprite.color = new Color(1f, 1f, 1f, 0.1f);
                }
                opaque = true;
            }
        }
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
                        hit.collider.gameObject.GetComponent<EnemyAI>().Stun(backstepStunTime);
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
                        hit.collider.gameObject.GetComponent<EnemyAI>().Stun(backstepStunTime);
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
            && dashUpgrade && Input.GetKeyDown("u") && dashUIScript.charge >= 1) //upgraded dash
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
                    enemy.GetComponent<EnemyAI>().TakeDamage(attackDamage);
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
    public void ChangeFeather(FeatherTypes type)
    {
        switch (type)
        {
            case FeatherTypes.Default:
                fs = featherShotsScript.DefaultShot;
                break;
            case FeatherTypes.Fireball:
                fs = featherShotsScript.Fireball;
                break;
            case FeatherTypes.Boomerang:
                fs = featherShotsScript.BoomerangShot;
                break;
            case FeatherTypes.BallLightning:
                fs = featherShotsScript.BallLightning;
                break;
        }
    }
}