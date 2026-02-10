using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class WeaponRanged : Weapon, PlayerActions.ICombatInputActions
{
    [Header("Enums")]
    [SerializeField] private Rarity rarity = Rarity.Normal;
    [SerializeField] private Modification modification = Modification.New;
    [SerializeField] private Effect effect = Effect.None;
    [Header("Name and description")]
    [SerializeField] private string weaponName;
    [SerializeField] private string weaponDescription;
    [Header("Characteristics")]
    [SerializeField] private int maxAmmo;
    [SerializeField] private int maxAmount;
    [SerializeField] private float mana;
    [SerializeField] private float damage;
    [SerializeField] private float splashDamage;
    [SerializeField] private float fireRate;
    [SerializeField] private float spread;
    [SerializeField] private float maxSpreadLimit;
    [SerializeField] private float multiplySpeed;
    [SerializeField] private float splashRadius;
    [SerializeField] private int currentAmmo;
    [Header("Prefabs")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [Header("Projectile")]
    [SerializeField] private float reloadTime;
    [SerializeField] private float projectileLifeTime;
    [SerializeField] private float projectileSpeed;

    private float nextFireTime;

    public override Rarity WeaponRarity => rarity;
    public override Modification WeaponModification => modification;
    public override Effect WeaponEffect => effect;
    public override int MaxAmmo => maxAmmo;
    public override float Mana => mana;
    public override float Damage => damage;
    public override float SplashDamage => splashDamage;
    public override float Spread => spread;
    public override float MaxSpreadLimit => maxSpreadLimit;
    public override int MaxAmount => maxAmount;
    public override string WeaponDescription => weaponDescription;
    public override string WeaponName => weaponName; 
    public override float ReloadTime => reloadTime;
    public override float ProjectileLifeTime => projectileLifeTime;
    public override float ProjectileSpeed => projectileSpeed;

    private PlayerActions _input; 
    private bool _isShooting;
    private bool _isReloading;
    private float _spreadBooster = 1f;

    public override float Firerate
    {
        get => fireRate;
        set => fireRate = Mathf.Max(0.01f, value);
    }

    public override float MultiplySpeed
    {
        get => multiplySpeed;
        set => multiplySpeed = Mathf.Max(0.1f, value);
    }

    public override float SplashRadius
    {
        get => splashRadius;
        set => splashRadius = Mathf.Max(0f, value);
    }

    private void OnEnable()
    {
        _input = new PlayerActions();
        _input.CombatInput.AddCallbacks(this);
        _input.CombatInput.Enable();
    }

    private void OnDisable()
    {
        _input.CombatInput.Disable();
    }

    private void OnDestroy()
    {
        _input.Dispose();
    }

    private void Update()
    {
        if (_isShooting)
        {
            ShootHandler();
        }
        Rotate();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started) 
            _isShooting = true;

        if (context.canceled)
            _isShooting = false;
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        if (context.started) 
            _spreadBooster = Spread;

        if (context.canceled)
            _spreadBooster = 1f;
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed && currentAmmo < maxAmmo && !_isReloading)
            StartCoroutine(ReloadHandler());
    }

    public override void Rotate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public override void ShootHandler()
    {
        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0 && !_isReloading)
        {
            StartCoroutine(ReloadHandler());
            return;
        }

        SpawnProjectile();

        currentAmmo--;
        nextFireTime = Time.time + Firerate / MultiplySpeed;
    }

    public override void SpawnProjectile()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        float spreadAngle = UnityEngine.Random.Range(-maxSpreadLimit, maxSpreadLimit + 1) * _spreadBooster;
        Quaternion spreadRotation = shootPoint.rotation * Quaternion.Euler(0, 0, spreadAngle);

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootPoint.position,
            spreadRotation
        );

        RangedProjectile proj = projectile.GetComponent<RangedProjectile>();
        if (proj != null)
        {
            proj.Init(Damage, SplashDamage, SplashRadius, ProjectileLifeTime, ProjectileSpeed);
        }
    }

    public override IEnumerator ReloadHandler()
    {
        _isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        _isReloading = false;
    }
}