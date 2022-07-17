using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerStats
    {
        public int health;
        public int baseMaxHealth;
        public float speed;
        public float acceleration;
        public float baseMaxSpeed;
        public float baseRateOfFire;
    }

    public PlayerStats stats;

    public bool canMove;
    public bool isDying;
    
    public GameObject bomb;

    [HideInInspector]
    public Vector2 additionalForce;
    [HideInInspector]
    public Vector2 knockbackForceFromEnemies;

    bool rechargingAttack;
    bool iFramesActive;
    Transform crosshair;
    Transform weaponHolder;
    Animator anim;
    SpriteRenderer spr;
    SpriteRenderer weaponSpr;
    Transform bulletSpawnPos;
    GameManager gm;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        spr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        weaponHolder = transform.GetChild(1);
        weaponSpr = weaponHolder.GetChild(0).GetComponent<SpriteRenderer>();
        bulletSpawnPos = weaponHolder.GetChild(1);
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
    }

    private void FixedUpdate()
    {
        if (stats.health <= 0)
            StartCoroutine(Die());

        if (canMove && !isDying && stats.health > 0)
        {
            UpdateInputAxes();
            //UpdateMovementAnimations();
        }
        else
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.15f);
    }

    IEnumerator Die()
    {
        yield return null;
    }

    void Update()
    {
        if (gm.gm_gameVars.gamePaused || !canMove || isDying || stats.health <= 0)
            return;

        UpdateInputButtons();
        RotateArm();
    }

    void UpdateInputAxes()
    {
        if (rb.velocity.magnitude > 1)
            CheckAndPlayClip("Player_Walk");
        else
            CheckAndPlayClip("Player_Idle");
        
        if (!canMove)
            return;

        // Movement
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        Vector2 speed = new Vector2(horiz, vert);

        if (isDying || stats.health <= 0)
            additionalForce = Vector2.zero;

        if (additionalForce == Vector2.zero && rb.velocity.magnitude > stats.baseMaxSpeed)
        {
            rb.velocity -= rb.velocity * 0.1f;
        }
        else
        {
            rb.velocity = (Vector3.ClampMagnitude(speed, 1) * stats.baseMaxSpeed) + (Vector3)additionalForce + (Vector3)knockbackForceFromEnemies;
            stats.speed = rb.velocity.magnitude;
        }

        additionalForce = Vector2.MoveTowards(additionalForce, Vector2.zero, 1f);
        knockbackForceFromEnemies = Vector2.MoveTowards(knockbackForceFromEnemies, Vector2.zero, 0.66f);
    }

    void RotateArm()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;

        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);

        // Flip sprite X if mouse is to left of player
        spr.flipX = mousePos.x < objectPos.x;
        weaponSpr.flipY = spr.flipX;
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        weaponHolder.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateInputButtons()
    {
        if (gm.gm_gameVars.gamePaused || isDying || stats.health <= 0)
            return;

        // Shoot gun
        if (Input.GetButtonDown("Fire1") && !rechargingAttack)
        {
            gm.PlaySFX(gm.gm_gameSfx.playerSfx[0]);
            BombProjectile b = Instantiate(bomb, bulletSpawnPos.position, Quaternion.identity).GetComponent<BombProjectile>();
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse = new Vector3(mouse.x, mouse.y, 0);
            Vector2 dir = (mouse - bulletSpawnPos.position).normalized;
            additionalForce = -dir * 10;
            gm.ScreenShake(2);
            StartCoroutine(AttackCooldown());
            b.Initialize(dir);
        }
    }

    IEnumerator AttackCooldown()
    {
        rechargingAttack = true;
        yield return new WaitForSeconds(stats.baseRateOfFire);
        rechargingAttack = false;
    }

    IEnumerator IFramesCooldown()
    {
        iFramesActive = true;
        yield return new WaitForSeconds(1.5f);
        iFramesActive = false;
    }

    public void SetVisible()
    {
        spr.color = Color.white;
        
    }

    public void ReceiveDamage(int damage)
    {
        if (stats.health > 0 && !iFramesActive)
            StartCoroutine(ReceiveDamageCoroutine(damage));
    }

    IEnumerator ReceiveDamageCoroutine(int damage)
    {
        //gm.PlaySFX(gm.gm_gameSfx.playerSfx[0]);
        spr.color = Color.red;
        float shakeAmount = 0.5f;
        stats.health -= damage;

        while (spr.color.g < 0.9f)
        {
            spr.color = Color.Lerp(spr.color, Color.white, 0.05f);
            spr.transform.localPosition = new Vector2(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
            shakeAmount /= 1.25f;
            yield return new WaitForFixedUpdate();
        }
        spr.transform.localPosition = Vector2.zero;
        StartCoroutine(IFramesCooldown());
    }

    float AngleBetweenMouse(Transform reference)
    {
        Vector3 relative = reference.transform.InverseTransformPoint(crosshair.position);
        float angle = Mathf.Atan2(-relative.x, relative.y) * Mathf.Rad2Deg;
        return -angle;
    }

    public void CheckAndPlayClip(string clipName)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(clipName))
        {
            anim.Play(clipName);
        }
    }

    public void CheckAndPlayClip(string clipName, Animator a)
    {
        if (!a.GetCurrentAnimatorStateInfo(0).IsName(clipName))
        {
            a.Play(clipName);
        }
    }
}
