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

    [System.Serializable]
    public class StatBuffs
    {
        public float speedMultiplier = 1;
        public float healthMultiplier = 1;
        public float firerateMultiplier = 1;
        public float damageMultiplier = 1;
    }

    public enum BulletType
    {
        bigshot,
        splitshot,
        spreadshot,
        stun,
        crit,
        homing,
        homerun,
        bigBoom,
        lightning
    }

    public PlayerStats stats;
    public StatBuffs buffs;

    public bool canMove;
    public bool isDead;

    bool hurting;

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
    Animator gunAnim;
    SpriteRenderer spr;
    SpriteRenderer weaponSpr;
    Transform bulletSpawnPos;
    GameManager gm;
    Rigidbody2D rb;
    StatManager sm;
    WaveManager wm;

    Vector2 lastFirePosition;

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        spr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        weaponHolder = transform.GetChild(1);
        weaponSpr = weaponHolder.GetChild(0).GetComponent<SpriteRenderer>();
        bulletSpawnPos = weaponHolder.GetChild(1);
        gunAnim = weaponSpr.GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
        sm = FindObjectOfType<StatManager>();
        wm = FindObjectOfType<WaveManager>();
    }

    private void FixedUpdate()
    {
        if (canMove && !isDead && stats.health > 0)
        {
            UpdateInputAxes();
            UpdateMovementAnimations();
        }
        else
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.15f);
    }

    void Update()
    {
        if (gm.gm_gameVars.gamePaused || !canMove || isDead || stats.health <= 0)
            return;

        UpdateInputButtons();
        RotateArm();

        if(wm.inBattle)
        {
            if (sm.currentUltraStat.info.id.Contains("goodstuff_firetrail"))
            {
                if(Vector2.Distance(transform.position, lastFirePosition) > 1)
                {
                    Instantiate(gm.gm_gameRefs.fire, transform.position, Quaternion.identity);
                    lastFirePosition = transform.position;
                }
            }
        }
    }

    void UpdateMovementAnimations()
    {
        if (!isDead && !wm.inBattle)
            CheckAndPlayClip("Player_Idle");

        if (!hurting)
        {
            if (rb.velocity.magnitude > 1)
                CheckAndPlayClip("Player_Walk");
            else
                CheckAndPlayClip("Player_Idle");
        }
    }

    void UpdateInputAxes()
    {
        if (!canMove)
            return;

        // Movement
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        Vector2 speed = new Vector2(horiz, vert);

        if (isDead)
            additionalForce = Vector2.zero;

        if (additionalForce == Vector2.zero && rb.velocity.magnitude > stats.baseMaxSpeed * buffs.speedMultiplier)
        {
            rb.velocity -= rb.velocity * 0.1f;
        }
        else
        {
            rb.velocity = (Vector3.ClampMagnitude(speed, 1) * stats.baseMaxSpeed * buffs.speedMultiplier) + (Vector3)additionalForce + (Vector3)knockbackForceFromEnemies;
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
        if (gm.gm_gameVars.gamePaused || isDead || stats.health <= 0)
            return;

        // Shoot gun
        if (Input.GetButton("Fire1") && !rechargingAttack)
        {
            gunAnim.Play("Gun_Shoot", 0, 0);
            List<BulletType> types = DetermineModsForBullet();
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse = new Vector3(mouse.x, mouse.y, 0);
            Vector2 dir = (mouse - bulletSpawnPos.position).normalized;

            // Spreadshot
            if (types.Contains(BulletType.spreadshot))
            {
                float spread = 0.25f;
                Vector2 dirLeft = (dir + (Vector2)Vector3.Cross(dir, -Vector3.forward) * spread).normalized;
                Vector2 dirRight = (dir - (Vector2)Vector3.Cross(dir, -Vector3.forward) * spread).normalized;

                Vector2[] dirs = new Vector2[3] { dir, dirLeft, dirRight };
                for (int i = 0; i < 3; i++)
                {
                    BombProjectile b = Instantiate(bomb, bulletSpawnPos.position, Quaternion.identity).GetComponent<BombProjectile>();
                    b.Initialize(dirs[i], types, null);
                }
            }
            else
            {
                BombProjectile b = Instantiate(bomb, bulletSpawnPos.position, Quaternion.identity).GetComponent<BombProjectile>();
                b.Initialize(dir, types, null);
            }


            int soundToPlay = 0;
            if (types.Count > 0)
                soundToPlay = 1;

            gm.PlaySFX(gm.gm_gameSfx.playerSfx[soundToPlay]);

            //additionalForce = -dir * 10;
            gm.ScreenShake(2);
            rechargingAttack = true;
            StartCoroutine(AttackCooldown());
        }
    }

    List<BulletType> DetermineModsForBullet()
    {
        List<BulletType> bTypes = new List<BulletType>();

        //bTypes.Add(BulletType.bigshot);
        //bTypes.Add(BulletType.spreadshot);
        //bTypes.Add(BulletType.crit);
        //bTypes.Add(BulletType.homerun);
        //bTypes.Add(BulletType.splitshot);
        //bTypes.Add(BulletType.bigBoom);
        //bTypes.Add(BulletType.lightning);
        //bTypes.Add(BulletType.homing);

        for (int i = 0; i < sm.unlockedWeaponMods.Count; i++)
        {
            float rand = Random.Range(0, 1f);
            if (rand <= sm.unlockedWeaponMods[i].numericalValue / 18f)
            {
                BulletType type = BulletType.crit;
                switch (sm.unlockedWeaponMods[i].info.id)
                {
                    case "mods_bigshot":
                        type = BulletType.bigshot;
                        break;
                    case "mods_splitshot":
                        type = BulletType.splitshot;
                        break;
                    case "mods_spreadshot":
                        type = BulletType.spreadshot;
                        break;
                    case "mods_crit":
                        type = BulletType.crit;
                        break;
                    case "mods_homing":
                        type = BulletType.homing;
                        break;
                    case "mods_homerun":
                        type = BulletType.homerun;
                        break;
                    case "mods_stun":
                        type = BulletType.stun;
                        break;
                }
                bTypes.Add(type);
            }
        }
        print(sm.currentUltraStat.info.id);
        if (sm.currentUltraStat.info.id.Contains("goodstuff_bigboom"))
            bTypes.Add(BulletType.bigBoom);
        if (sm.currentUltraStat.info.id.Contains("goodstuff_lightning"))
            bTypes.Add(BulletType.lightning);

        return bTypes;
    }

    IEnumerator AttackCooldown()
    {
        rechargingAttack = true;
        yield return new WaitForSeconds(stats.baseRateOfFire / buffs.firerateMultiplier);
        rechargingAttack = false;
    }

    IEnumerator IFramesCooldown()
    {
        iFramesActive = true;
        yield return new WaitForSeconds(0.5f);
        hurting = false;
        yield return new WaitForSeconds(0.25f);
        iFramesActive = false;
    }

    public void SetVisible()
    {
        spr.color = Color.white;

    }

    public void ReceiveDamage(int damage)
    {
        if (!isDead && !iFramesActive && wm.inBattle)
            StartCoroutine(ReceiveDamageCoroutine(damage));
    }

    IEnumerator ReceiveDamageCoroutine(int damage)
    {
        //gm.PlaySFX(gm.gm_gameSfx.playerSfx[0]);
        spr.color = Color.red;
        float shakeAmount = 0.5f;
        stats.health = Mathf.Clamp(stats.health - damage, 0, 100);

        if (stats.health <= 0)
        {
            Die();
            yield break;
        }
            

        hurting = true;
        iFramesActive = true;
        StartCoroutine(IFramesCooldown());
        CheckAndPlayClip("Player_Hurt");

        while (spr.color.g < 0.9f)
        {
            spr.color = Color.Lerp(spr.color, Color.white, 0.05f);
            spr.transform.localPosition = new Vector2(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
            shakeAmount /= 1.25f;
            yield return new WaitForFixedUpdate();
        }
        spr.transform.localPosition = Vector2.zero;
    }

    void Die()
    {
        rb.isKinematic = true;
        gm.StopMusic();
        gm.PlaySFX(gm.gm_gameSfx.generalSfx[10]);
        CheckAndPlayClip("Player_Die");
        isDead = true;
        wm.inBattle = false;
        gm.StartCoroutine(gm.EndLevel());
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
