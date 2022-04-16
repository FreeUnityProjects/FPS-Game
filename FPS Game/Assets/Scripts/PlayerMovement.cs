using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    private const float NORMAL_FOV = 60f;
    private const float HOOKSHOT_FOV = 100f;

    [SerializeField] private Transform hookshotTransform;
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;

    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -20f;
    public float jumpHeight = 3f;

    public float health = 100f;
    private float currentHealth;

    public float energy = 1000f;

    public float currency = 0;
    public TextMeshProUGUI currencyText;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public float timer = 2f;
    public bool timerCheck = false;

    public float range = 200f;

    Vector3 velocity;
    bool isGrounded;

    public Camera playerCamera;

    private CameraFov cameraFov;

    public GameObject Projectile;
    private ChasingEnemy chasingEnemy;
    private Target target;

    private float hookshotSize;
    private Vector3 hookshotPosition;
    private Vector3 characterVelocityMomentum;
    private State state;

    public ParticleSystem speedLinesParticleSystem1;
    public ParticleSystem speedLinesParticleSystem2;
    public ParticleSystem speedLinesParticleSystem3;

    public float currentSpeed = 0f;

    public string textSignal = "none";

    private bool canDoubleJump = false;
    private float doubleJumpMultiplier = 2f;

    public float knockBackForce;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI energyText;

    private enum State
    {
        Normal,
        HookshotThrown,
        HookshotFlyingPlayer
    }

    void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log("Active scene is '" + scene.name + "'.");

        speedLinesParticleSystem1.Stop();
        speedLinesParticleSystem2.Stop();
        speedLinesParticleSystem3.Stop();

        currency = PlayerPrefs.GetFloat("currency");
    }   

    private void Awake()
    {
        state = State.Normal;
        cameraFov = playerCamera.GetComponent<CameraFov>();
        hookshotTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        //Hookshot States
        switch(state)
        {
            default:
            case State.Normal:
                HandleHookshotStart();
                break;
            case State.HookshotThrown:
                HandleHookshotThrow();
                break;
            case State.HookshotFlyingPlayer:
                HandleHookshotMovement();
                break;
        }

        //GroundCheck
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //Mouse Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        move += characterVelocityMomentum;

        //Move player
        controller.Move(move * speed * Time.deltaTime);

        //Player Velocity
        if(characterVelocityMomentum.magnitude >= 0f)
        {
            float momentumDrag = 2.5f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if(characterVelocityMomentum.magnitude < .0f)
            {
                characterVelocityMomentum = Vector3.zero;
            }
        }

        //Double Jump
        if (isGrounded)
        {
            canDoubleJump = true;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else if (canDoubleJump == true)
        {
            Scene scene = SceneManager.GetActiveScene();

            if (Input.GetButtonDown("Jump") && scene.name == "WaveLevel" && energy >= 1f)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity * doubleJumpMultiplier);
                canDoubleJump = false;

                energy -= 50f;
            }
        }

        //Sprint
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && isGrounded)
        {
            speed = 20f;
        }

        if (Input.GetKey(KeyCode.LeftShift) == false && isGrounded)
        {
            speed = 12f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if(timerCheck)
        {
            timer -= Time.deltaTime;
        }

        if(timer <= 0)
        {
            timerCheck = false;
            gravity = -20f;
            timer = 2f;
        }

        //Set HealthText and EnergyText
        // healthText.text = health.ToString();
        // energyText.text = energy.ToString("F0");

        //Regenerate Energy
        if(energy < 1000f)
        {
            energy += 25f * Time.deltaTime;
        }

        if (health <= 0f)
        {
            Die();
        }

        //Set Text to currency float
        // currencyText.text = currency.ToString();

        PlayerPrefs.SetFloat("currency", currency);

        //Get the Bomb Enemy Script
        chasingEnemy = FindObjectOfType<ChasingEnemy>();
    }

    Vector3 lastPosition = Vector3.zero;
    private void FixedUpdate()
    {
        //Check speed and apply speedlines
        currentSpeed = (transform.position - lastPosition).magnitude / Time.fixedDeltaTime;
        lastPosition = transform.position;

        if (currentSpeed <= 19f)
        {
            speedLinesParticleSystem1.Play();
        }

        if (currentSpeed <= 27f)
        {
            speedLinesParticleSystem2.Play();
        }

        if (currentSpeed <= 32f)
        {
            speedLinesParticleSystem3.Play();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch(hit.gameObject.tag)
        {
            case "SpeedPad":
                speed = 40f;
                jumpHeight = 6f;
                break;

            case "Ground":
                jumpHeight = 3f;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "GravityPad":
                gravity = 2f;
                timerCheck = true;
                break;
        }

        if (other.gameObject.tag == "ChangeScene")
        {
            SceneManager.LoadScene("NextLevel");
        }

        if(other.gameObject.tag == "ChangeScene2")
        {
            SceneManager.LoadScene("3rdLevel");
        }

        if (other.gameObject.tag == "ChangeScene3")
        {
            SceneManager.LoadScene("4thLevel");
        }

        if(other.gameObject.tag == "Bullet")
        {
            TakeDamage();
        }

        if (other.gameObject.name == "projector")
        {
            textSignal = "projector";
            Debug.Log("Near the Projector");
        }

        if (other.gameObject.name == "NearThePit")
        {
            textSignal = "nearpit";
            Debug.Log("Near the Pit");
        }

        if(other.gameObject.name == "PitCollider")
        {
            textSignal = "pitcollider";
            Debug.Log("End of Pit");
        }

        if (other.gameObject.name == "StairCollider")
        {
            textSignal = "staircollider";
        }

        if(other.gameObject.name == "ElevatorTrigger")
        {
            textSignal = "elevatortrigger";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "projector")
        {
            textSignal = "none";
            Debug.Log("Not near the Projector anymore");
        }

        if (other.gameObject.name == "NearThePit")
        {
            textSignal = "none";
            Debug.Log("Not near the Pit anymore");
        }

        if (other.gameObject.name == "PitCollider")
        {
            textSignal = "none";
        }

        if (other.gameObject.name == "StairCollider")
        {
            textSignal = "none";
        }
    }

    public CameraShake cameraShake;
    public void TakeDamage()
    {
        //Damage Player
        health -= 20f;
        StartCoroutine(cameraShake.Shake(.15f, .3f));

        knockBackForce = 5f;
        Knockback();
    }
     
    public void BombDamage()
    {
        //Bomb Player Damage
        health -= chasingEnemy.damage;
        // StartCoroutine(cameraShake.Shake(.2f, .5f));

        knockBackForce = 20f;
        Knockback();
    }

    public void Die()
    {
        //Player Die
        player.transform.position = respawnPoint.transform.position;
        Physics.SyncTransforms();

        health = 100f;
    }

    private void ResetGravityEffect()
    {
        //Reset the gravity
        gravity = -20f;
    }

    private void HandleHookshotStart()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (TestInputDownHookshot() && scene.name == "3rdLevel" | scene.name == "5thLevel" | scene.name == "WaveLevel" && energy >= 1f)
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, range))
            {
                hookshotPosition = raycastHit.point;
                hookshotSize = 0f;
                hookshotTransform.gameObject.SetActive(true);
                hookshotTransform.localScale = Vector3.zero;
                state = State.HookshotThrown;

                energy -= 250f;
            }
        }
    }

    private void HandleHookshotThrow()
    {
        hookshotTransform.LookAt(hookshotPosition);

        float hookshotThrowSpeed = 60f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if(hookshotSize >= Vector3.Distance(transform.position, hookshotPosition))
        {
            state = State.HookshotFlyingPlayer;
            cameraFov.SetCameraFov(HOOKSHOT_FOV);
        }
    }

    private void HandleHookshotMovement()
    {
        hookshotTransform.LookAt(hookshotPosition);
        Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 30f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 4.5f;

        controller.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        float reachedHookshotPositionDistance = 3f;
        if(Vector3.Distance(transform.position, hookshotPosition) < reachedHookshotPositionDistance)
        {
            state = State.Normal;
            ResetGravityEffect();
            hookshotTransform.gameObject.SetActive(false);
            cameraFov.SetCameraFov(NORMAL_FOV);
        }

        if (TestInputDownHookshot())
        {
            state = State.Normal;
            ResetGravityEffect();
            hookshotTransform.gameObject.SetActive(false);
            cameraFov.SetCameraFov(NORMAL_FOV);
        }

        if(TestInputJump())
        {
            characterVelocityMomentum = hookshotDir * speed;
            state = State.Normal;
            ResetGravityEffect();
            hookshotTransform.gameObject.SetActive(false);
            cameraFov.SetCameraFov(NORMAL_FOV);
        }
    }
    private bool TestInputDownHookshot()
    {
        return Input.GetKeyDown(KeyCode.C);
    }

    private bool TestInputJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public void Knockback()
    {
        velocity.y = Mathf.Sqrt(knockBackForce * -2f * gravity);
    }
}
