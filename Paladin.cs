using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Paladin : MonoBehaviour
{
    // The Paladin. Weaker attacks, but can heal himself and his allies, and even resurrect dead players 

    private int playerNr;                                           // aka player 1, player 2. Used for assigning the right controller

    private GameObject sword;                                       // The gameobject for the sword

    private Sprite defaultSprite;                                   // sprite of the character      
    
    [SerializeField]
    private GameObject analyticsGO;                                 // Used for measuring data / balancing

    [SerializeField]
    private float attackTimer;                                      // The time between attacks
    private float attackCooldownTimer = 0;                          // The actual time of the attack cooldown. Triggers both when the animation stops and when a new attack is available

    private Quaternion swordRotation = Quaternion.Euler(0, 0, 50);  // used for rotating the sword, which makes it look like the paladins swings it
    private Vector2 swordLocation = new Vector2(0, 0.3f);           // makes sure the paladin is attacking in th right direction

    [SerializeField]
    private float healTimer;                                        // The cooldown for the heal

    private GameObject healingCircle;                               // The animation
    private float visualHealTimer;                                  // The timer for the heal (cooldown)
    private Image healCooldownImage;                                // the image showing the cooldown

    [SerializeField]
    private float healRadius;                                       // the areasize of the heal

    [SerializeField]
    private LayerMask CharacterLayerMask;                           // the layermask for the characters


    // Use this for initialization
    void Start()
    {
        playerNr = GetComponent<PlayerTemp>().playerNr;

        sword = transform.GetChild(0).gameObject;
        healCooldownImage = GetComponent<PlayerTemp>().characterUI.transform.GetChild(3).GetComponent<Image>();
        healCooldownImage.gameObject.SetActive(true);

        gameObject.GetComponent<BoxCollider2D>().enabled = false;

        ControllerManager.GetControllerInput();

        healingCircle = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(ControllerManager.startButton[playerNr - 1]))
        {
            foreach (GameObject character in Camera.main.GetComponent<CameraController>().characters)
            {
                character.GetComponent<BoxCollider2D>().enabled = true;
            }

            analyticsGO.GetComponent<AnalyticsManager>().gameRunning = true;
        }

        if (gameObject.tag == "Player" && GetComponent<BoxCollider2D>().enabled)
        {
            Movement();
            FlipCharacter();
            Attack();
            SetAttackDirection();
            Heal();
            HealingEffect();
        }
    }

    // movement for the character
    void Movement()
    {
        transform.Translate(Vector3.right * 5 * Time.deltaTime * Input.GetAxis("Horizontal" + playerNr));
        transform.Translate(Vector3.up * 5 * Time.deltaTime * Input.GetAxis("Vertical" + playerNr));
    }

    // make the character look in the right direction
    // the character can maintain in what direction their looking if they wish to attack in that direction
    void FlipCharacter()
    {
        if (Input.GetAxis("Horizontal" + playerNr) > float.Epsilon && !Input.GetKey(ControllerManager.rightBumper[playerNr - 1]))
            GetComponent<SpriteRenderer>().flipX = false;
        else if (Input.GetAxis("Horizontal" + playerNr) < -float.Epsilon && !Input.GetKey(ControllerManager.rightBumper[playerNr - 1]))
            GetComponent<SpriteRenderer>().flipX = true;
    }

    // performs a melee attack when the button is pressed
    // rotate it to give it a swinging effect
    void Attack()
    {
        if (Input.GetKeyDown(ControllerManager.aButton[playerNr - 1]) && attackCooldownTimer <= 0)
        {
            sword.transform.localPosition = swordLocation;
            sword.transform.rotation = swordRotation;
            sword.SetActive(true);
            attackCooldownTimer = attackTimer;
        }

        if (sword.active)
            sword.transform.Rotate(0, 0, -500 * Time.deltaTime);

        if (attackCooldownTimer < attackTimer - 0.15f)
        {
            sword.SetActive(false);
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    // set the attack direction based on the movement. 
    // will first check if the left joystick is being moved. If it is, it will check which direction is strongest
    // If the shoulderbutton is pressed, it will keep on looking in that direction
    void SetAttackDirection()
    {
        if (!Input.GetKey(ControllerManager.rightBumper[playerNr - 1]))
        {
            if (Mathf.Abs(Input.GetAxis("Horizontal" + playerNr)) + Mathf.Abs(Input.GetAxis("Vertical" + playerNr)) > 0.3f && attackCooldownTimer <= attackTimer - 0.3f + float.Epsilon)
            {
                if (Mathf.Abs(Input.GetAxis("Horizontal" + playerNr)) > Mathf.Abs(Input.GetAxis("Vertical" + playerNr)))
                {
                    if (Input.GetAxis("Horizontal" + playerNr) > 0)
                    {
                        swordLocation = new Vector2(0.3f, 0);
                        swordRotation = Quaternion.Euler(0, 0, 320);
                    }
                    else
                    {
                        swordLocation = new Vector2(-0.3f, 0);
                        swordRotation = Quaternion.Euler(0, 0, 140);
                    }
                }
                else
                {
                    if (Input.GetAxis("Vertical" + playerNr) > 0)
                    {
                        swordLocation = new Vector2(0, 0.3f);
                        swordRotation = Quaternion.Euler(0, 0, 50);
                    }
                    else
                    {
                        swordLocation = new Vector2(0, -0.3f);
                        swordRotation = Quaternion.Euler(0, 0, 230);
                    }
                }
            }
        }
    }

    // heal yourself and nearby allies, or resurrect fallen allies
    void Heal()
    {
        if (Input.GetKeyDown(ControllerManager.bButton[playerNr - 1]) && healCooldownImage.fillAmount == 1)
        {
            visualHealTimer = 0.4f;

            Collider2D[] characters = Physics2D.OverlapCircleAll(transform.position, healRadius, CharacterLayerMask);

            foreach (Collider2D character in characters)
            {
                if (character.gameObject.tag == "Player")
                {
                    character.GetComponent<PlayerTemp>().healthSlider.value += 10;
                }
                else if (character.gameObject.tag == "DeadPlayer")
                {
                    character.GetComponent<PlayerTemp>().healthSlider.value += 100;

                    character.GetComponent<PlayerTemp>().Death(false);

                    character.gameObject.GetComponent<SpriteRenderer>().sprite = defaultSprite;
                    character.gameObject.GetComponent<Animator>().enabled = true;

                    PlayerTemp.deadPlayers -= 1;

                    character.gameObject.tag = "Player";
                }
            }
            healCooldownImage.fillAmount = 0;
        }

        if (healCooldownImage.fillAmount < 1)
        {
            healCooldownImage.fillAmount += Time.deltaTime / healTimer;
        }
    }

    // show the visual effect for the heal
    void HealingEffect()
    {
        if (visualHealTimer > 0)
        {
            visualHealTimer -= Time.deltaTime;

            healingCircle.SetActive(true);
            healingCircle.transform.Rotate(0, 0, 10 * Time.deltaTime);
        }
        if (visualHealTimer <= 0)
        {
            healingCircle.SetActive(false);
        }
    }
}
