using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private static readonly int SwimAngle = Animator.StringToHash("SwimAngle");
    private static readonly int Sprinting = Animator.StringToHash("Sprinting");
    private static readonly int Swimming = Animator.StringToHash("Swimming");
    private static readonly int Dashing = Animator.StringToHash("Dashing");
    private static readonly int Idle = Animator.StringToHash("Player");

    [Header("Components")]
    public Rigidbody2D rigidBody2D;
    public GameObject characterModel;
    public Animator characterAnimator;
    public PlayerBubble playerBubble;
    
    [Header("Movement")]
    [SerializeField] public float acceleration = 1f;
    [SerializeField] public float deceleration = 1f;
    [SerializeField] public float maxSpeed = 3f;
    [SerializeField] public float rotationSpeed = 5f;
    [SerializeField] public float rotationNeutralRecoverySpeed = 1f;

    [Header("Sprint")]
    [SerializeField] public float sprintMaxSpeed = 10f;
    [SerializeField] public float sprintAccelaration = 3f;
    [SerializeField] public float sprintDeceleration = 5f;
    
    [Header("Dash")]
    [SerializeField] public float dashForce = 10f;
    [SerializeField] public float dashCooldown = 1f;
    [SerializeField] public float dashMaxSpeed = 10f;
    [SerializeField] public float dashSpeedDecay = 1f;
    private float _lastDashTime;
    private bool _isDashing;

    [Header("Values")]
    [SerializeField] public int playerHearts = 3;

    [SerializeField] public float gravityScale = 1f;
    
    private float _maxSpeedBase;
    private int _playerHeartsBase;
    private float _maxAccelerationBase;

    private bool _isVictory;
    private bool _isDead;
    private bool _isFacingRight = true;
    private bool _isSprinting;

    private InputAction _movement;
    private InputAction _sprint;
    private InputAction _dash;

    internal static event Action<float> TakeHit;
    internal static event Action IsDashing;
    internal static event Action<bool> IsSprinting;
    public static event Action PlayerHasDied;

    private void Reset()
    {
        maxSpeed = _maxSpeedBase;
        playerHearts = _playerHeartsBase;
        acceleration = _maxAccelerationBase;
        transform.rotation = Quaternion.identity;
        _isDead = false;
    }
    
    private void Start()
    {
        _movement = InputSystem.actions.FindAction("Move");
        _sprint = InputSystem.actions.FindAction("Sprint");
        _dash = InputSystem.actions.FindAction("Jump");
        _lastDashTime = -dashCooldown;
        _playerHeartsBase = playerHearts;
        _maxSpeedBase = maxSpeed;
        _maxAccelerationBase = acceleration;
        
        WorldLogic.OnGameFinished += OnGameFinished;
        WorldLogic.OnGameReset += Reset;
        characterAnimator.SetBool("Swimming", false); // Garantir Idle no início
    }

    private void Update()
    {
        if (_isDead || _isVictory) return;
        var movement = _movement.ReadValue<Vector2>().normalized;
        MoveCharacter(movement);
        ApplyLinearDrag(movement);
        ApplyGravity(movement);
        RotateCharacter(movement);
        FlipSprite(movement);
        HandleSprint();
        HandleDash(movement);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(TagHandle.GetExistingTag("Enemy"))) return;
        var enemy = other.GetComponent<IEnemy>();
        TakeDamage(enemy?.Damage() ?? 1f);
    }
    
    public void TakeDamage(float damage)
    {
        if (playerBubble.IsProtected)
        {
            TakeHit?.Invoke(damage);
        }
        else
        {
            ReducePlayerHearts();
        }
    }
    
    private void FlipSprite(Vector2 movement)
    {
        if (_isFacingRight && movement.x < 0 || !_isFacingRight && movement.x > 0)
        {
            _isFacingRight = !_isFacingRight;
            var scale = characterModel.transform.localScale;
            characterModel.transform.localScale = new Vector3(scale.x * -1, scale.y, scale.z);
            var invertRotation = Quaternion.Inverse(characterModel.transform.rotation);
            characterModel.transform.rotation = invertRotation;
        }
    }

    private void RotateCharacter(Vector2 movement)
    {
        if (movement == Vector2.zero)
        {
            var rotation = Quaternion.Euler(0.0f, 0f, 0f);
            characterModel.transform.rotation = Quaternion.RotateTowards(characterModel.transform.rotation, rotation, Time.deltaTime * rotationNeutralRecoverySpeed);
        }
        else
        {
            var rotation = Quaternion.LookRotation(Vector3.forward, movement);
            rotation = movement.x switch
            {
                0 => !_isFacingRight ? rotation * Quaternion.AngleAxis(-90, Vector3.forward) : rotation * Quaternion.AngleAxis(90, Vector3.forward),
                < 0 => rotation * Quaternion.AngleAxis(-90, Vector3.forward),
                _ => rotation * Quaternion.AngleAxis(90, Vector3.forward)
            };
            characterModel.transform.rotation = Quaternion.RotateTowards(characterModel.transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            
        }
        characterAnimator.SetFloat(SwimAngle, characterModel.transform.rotation.eulerAngles.z + 90);
    }

    private void MoveCharacter(Vector2 movement)
    {
        rigidBody2D.AddForce(acceleration * movement);
        if(Mathf.Abs(rigidBody2D.linearVelocity.x) > maxSpeed)
        {
            rigidBody2D.linearVelocity = new Vector2(Mathf.Sign(rigidBody2D.linearVelocity.x) * maxSpeed, rigidBody2D.linearVelocity.y);
        }
        if(Mathf.Abs(rigidBody2D.linearVelocity.y) > maxSpeed)
        {
            rigidBody2D.linearVelocity = new Vector2(rigidBody2D.linearVelocity.x, Mathf.Sign(rigidBody2D.linearVelocity.y) * maxSpeed);
        }
        characterAnimator.SetBool(Swimming, movement != Vector2.zero);
    }
    
    private void ApplyGravity(Vector2 movement)
    {
        if (movement == Vector2.zero)
        {
            rigidBody2D.AddForce(Vector2.down * gravityScale);
        }
    }

    private void HandleSprint()
    {
        if (_sprint.ReadValue<float>() > 0 && playerBubble.bubbleStrength > 0)
        {
            StartSprint();
        }
        else if (_isSprinting)
        {
            acceleration = _maxAccelerationBase;
            maxSpeed = Mathf.Lerp(maxSpeed, 5f, sprintDeceleration * Time.deltaTime);
            if (Mathf.Abs(maxSpeed - 5f) < 0.1f)
            {
                maxSpeed = 5f;
                _isSprinting = false;
            }
        }
        IsSprinting?.Invoke(_isSprinting);
        characterAnimator.SetBool(Sprinting, _isSprinting);
    }

    private void HandleDash(Vector2 movement)
    {
        if (_dash.triggered && Time.time >= _lastDashTime + dashCooldown && playerBubble.bubbleStrength > 0)
        {
            Dash(movement);
        }
        if (_isDashing)
        {
            maxSpeed = Mathf.Lerp(maxSpeed, 5f, dashSpeedDecay * Time.deltaTime);
            if (Mathf.Abs(maxSpeed - 5f) < 0.1f)
            {
                maxSpeed = 5f;
                _isDashing = false;
            }
        }
        characterAnimator.SetBool(Dashing, _isDashing);
    }

    private void ApplyLinearDrag(Vector2 movement)
    {
        if(Mathf.Abs(movement.x) < 0.4f || Mathf.Abs(movement.y) < 0.4f)
        {
            rigidBody2D.linearDamping = deceleration;
        }
        else
        {
            rigidBody2D.linearDamping = 0;
        }
    }
    
    private void ReducePlayerHearts(int amount = 1)
    {
        playerHearts -= amount;
        if (playerHearts > 0) return;
        Die();
    }

    private void Dash(Vector2 movement)
    {
        if (movement != Vector2.zero)
        {
            rigidBody2D.AddForce(movement * dashForce, ForceMode2D.Impulse);
            maxSpeed = dashMaxSpeed;
            _isDashing = true;
            _lastDashTime = Time.time;
            IsDashing?.Invoke();
        }
    }

    private void StartSprint()
    {
        maxSpeed = sprintMaxSpeed;
        acceleration = sprintAccelaration;
        _isSprinting = true;
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        _isDead = true;
        rigidBody2D.linearVelocity = Vector2.zero;
        rigidBody2D.angularVelocity = 0;
        PlayerHasDied?.Invoke();
    }

    private void OnGameFinished()
    {
        rigidBody2D.AddForce(Vector2.up);
        _isVictory = true;
    }
}
