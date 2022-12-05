using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RubyController : MonoBehaviour
{
    //Level
    public static int level =1;

    // Projectile
    public GameObject projectilePrefab;
    public TextMeshProUGUI ammoText;
    private int cogCount = 4;
    
    // Audio
    AudioSource audioSource;
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip bgMuse;
    public AudioClip WinClip;
    public AudioClip LoseClip;
    public AudioClip ammoSound;
    public AudioClip boostSound;
    
    // Health
    public int maxHealth = 5;
    public int health { get { return currentHealth; }}
    int currentHealth;
    
    // Invincible
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;
    
    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;
    public float speed = 3.0f;
    public float boostTimer;
    private bool boosting;

    
    // Animator
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);

    // Fixed Robots
    public TextMeshProUGUI fixedText;
    private int fixCount;

    // Win & Lose Text and Restart
    public GameObject WinTextObject;
    public GameObject LoseTextObject;
    public GameObject contTextObject;
    private bool gameOver = false;

    // Particles
    public ParticleSystem damageEffect;
    public ParticleSystem healthEffect;

    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();

        WinTextObject.SetActive(false);
        LoseTextObject.SetActive(false);
        contTextObject.SetActive(false);

        // Robots Fixed
        fixedText.text = "Fixed Robots: 0/4";

        // Ammo
        ammoText.text = "Cogs: " + cogCount;

        //Music
        audioSource.clip = bgMuse;
        audioSource.Play();

        //Speed Boost
        boostTimer = 0;
        boosting = false;
    }

    // Sound
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Animation
        Vector2 move = new Vector2(horizontal, vertical);
        
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }
        
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        
        // Invincible Timer
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }
        
        // To launch projectile
        if(Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }
        
        // Talk to NPC
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                if(level == 2)
                {
                SceneManager.LoadScene("Level2");
                }
            
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                }
            }
        }

        // To close the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Restart
        if (Input.GetKey(KeyCode.R))

        {
            if (gameOver == true && level == 2)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // this loads the currently active scene
            }

            if (gameOver == true && level == 1)
            {
                SceneManager.LoadScene("Level1");
            }

        }

        //Speed Boost
        if (boosting)
        {
            boostTimer += Time.deltaTime;
            if (boostTimer >= 4)
            {
                speed = 3;
                boosting = false;
            }
        }
    }
    
    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        // Invincible
        if (amount < 0)
        {
            animator.SetTrigger("Hit");
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            
            ParticleSystem projectilePrefab = Instantiate(damageEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
            PlaySound(hitSound);
        }
        
        // Health Particles
        if (amount > 0)
        {
            ParticleSystem projectilePrefab = Instantiate(healthEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }

        // Player dies
        if (currentHealth <= 1)
        {
            LoseTextObject.SetActive(true);
            gameOver = true;

            speed = 0;
            transform.position = new Vector3(20f, 10f, -100f);
            Destroy(gameObject.GetComponent<SpriteRenderer>());

            audioSource.clip = bgMuse;
            audioSource.Stop();
            audioSource.clip = LoseClip;
            audioSource.Play();
        }

        if (amount < 0 && gameOver == true)
        {
            speed = 0;
        }


        // Health
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }
    
    // Projectile
    void Launch()
    {
        if(cogCount >0)
        {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");
        
         audioSource.PlayOneShot(throwSound);
         cogCount -= 1;
         SetCogText();
        }
    } 

    // Fixed robots text and teleport to level 2
    public void FixedText(int amount)
    {
        fixCount += amount;
        fixedText.text = "Fixed Robots: " + fixCount.ToString() + "/4";

        if(fixCount == 4 && level == 1)
        {
            contTextObject.SetActive(true);
            level = 2;
        }

        else if(level == 2 && fixCount == 4)
        {
            WinTextObject.SetActive(true);
            speed = 0;

            audioSource.clip = bgMuse;
            audioSource.Stop();
            audioSource.clip = WinClip;
            audioSource.Play();

            gameOver = true;
            level = 1;
        }
    }

    void SetCogText()
    {
        ammoText.text = "Cogs: " + cogCount.ToString();
    }

     private void OnTriggerEnter2D(Collider2D other)
     {
        if (other.tag == "Ammo")
        {
            cogCount += 4;
            SetCogText();

            PlaySound(ammoSound);

            Destroy(other.gameObject);
        }

        //Speed Boost
        if(other.tag == "SpeedBoost")
        {
            boosting = true;
            speed = 6;
            PlaySound(boostSound);
            Destroy(other.gameObject);
        }
     }
}
