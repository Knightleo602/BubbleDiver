using UnityEngine;

public class PiranhaAI : MonoBehaviour, IEnemy
{
    
    [Header("Components")]
    [SerializeField] public GameObject player;
    [SerializeField] public GameObject patrolPointA;
    [SerializeField] public GameObject patrolPointB;
    
    [Header("Movement")]
    [SerializeField] public float maxSpeed;
    [SerializeField] public float maxSpeedOnChase;
    [SerializeField] public float acceleration;
    [SerializeField] public float deceleration;
    [SerializeField] public float rotationSpeed;
    
    [Header("Attack")]
    [SerializeField] public float attackRange;
    [SerializeField] public float attackCooldown;
    [SerializeField] public float attackDamage;

    [Header("Detection")] 
    [SerializeField] public float fovAngle;
    [SerializeField] public float detectionRange;
    
    private float _baseMaxSpeed;
    private bool _isAttacking;
    private bool _isFacingRight;

    private bool _isAttackInCooldown;
    private bool _isPlayerInSight;
    private bool _isPlayerInAttackRange;
    private bool _isPlayerDead;
    
    private Collider2D _collider2D;
    private Rigidbody2D _rigidBody2D;
    private Transform _currentPatrolPoint;
    
    private void Start()
    {
        _collider2D = GetComponent<Collider2D>();
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _currentPatrolPoint = patrolPointA.transform;
        _baseMaxSpeed = maxSpeed;
        Player.PlayerHasDied += OnPlayerDeath;
        WorldLogic.OnGameReset += OnGameReset;
    }

    private void Update()
    {
        if(_isAttackInCooldown) return;
        CheckIfPlayerInRange();
        Move();
        Attack();
    }

    private void CheckIfPlayerInRange()
    {
        var directionToPlayer = (player.transform.position - transform.position).normalized;
        var angleToPlayer = Vector2.Angle(transform.rotation.eulerAngles, directionToPlayer);
        if(angleToPlayer < fovAngle / 2)
        {
            var distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
            if(distanceToPlayer < detectionRange)
            {
                _isPlayerInSight = true;
                _isPlayerInAttackRange = distanceToPlayer < attackRange;
                return;
            }
        }
        _isPlayerInSight = false;
        _isPlayerInAttackRange = false;
        _isAttacking = false;
    }

    private void Move()
    {
        if (!_isPlayerInSight || _isPlayerDead)
        {
            Patrol();
            return;
        }
        var playerDirection = (player.transform.position - transform.position).normalized;
        MoveRigidBody(playerDirection, maxSpeedOnChase);
    }
    
    private void MoveRigidBody(Vector2 movement, float maximumSpeed)
    {
        _rigidBody2D.AddForce(acceleration * movement);
        if(Mathf.Abs(_rigidBody2D.linearVelocity.x) > maximumSpeed)
        {
            _rigidBody2D.linearVelocity = new Vector2(Mathf.Sign(_rigidBody2D.linearVelocity.x) * maximumSpeed, _rigidBody2D.linearVelocity.y);
        }
        if(Mathf.Abs(_rigidBody2D.linearVelocity.y) > maximumSpeed)
        {
            _rigidBody2D.linearVelocity = new Vector2(_rigidBody2D.linearVelocity.x, Mathf.Sign(_rigidBody2D.linearVelocity.y) * maximumSpeed);
        }
        ApplyLinearDrag(movement);
        FlipSprite(movement);
        Rotate(movement);
    }

    private void Rotate(Vector2 movement)
    {
        if (movement == Vector2.zero)
        {
            var rotation = Quaternion.Euler(0.0f, 0f, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * 50f);
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
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
        }
    }
    
    private void ApplyLinearDrag(Vector2 movement)
    {
        if(Mathf.Abs(movement.x) < 0.4f || Mathf.Abs(movement.y) < 0.4f)
        {
            _rigidBody2D.linearDamping = deceleration;
        }
        else
        {
            _rigidBody2D.linearDamping = 0;
        }
    }
    
    private void FlipSprite(Vector2 movement)
    {
        if (_isFacingRight && movement.x < 0 || !_isFacingRight && movement.x > 0)
        {
            _isFacingRight = !_isFacingRight;
            var scale = transform.localScale;
            transform.localScale = new Vector3(scale.x * -1, scale.y, scale.z);
            var invertRotation = Quaternion.Inverse(transform.rotation);
            transform.rotation = invertRotation;
        }
    }

    private void ResumeAttack()
    {
        _isAttackInCooldown = false;
    }

    private void Patrol()
    {
        if(Vector2.Distance(transform.position, _currentPatrolPoint.position) < 0.5f)
        {
            _currentPatrolPoint = _currentPatrolPoint == patrolPointA.transform ? patrolPointB.transform : patrolPointA.transform;
        }
        Vector2 direction = (_currentPatrolPoint.position - transform.position).normalized;
        MoveRigidBody(direction, maxSpeed);
    }
    
    private void Attack()
    {
        if(!_isPlayerInAttackRange || _isAttacking || _isPlayerDead)
        {
            maxSpeed = _baseMaxSpeed;
            return;
        }
        _collider2D.enabled = true;
        _isAttacking = true;
        maxSpeed = maxSpeedOnChase;
    }
    
    private void OnPlayerDeath()
    {
        _isPlayerDead = true;
    }
    
    private void OnGameReset()
    {
        Destroy(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(_isAttackInCooldown || !_isAttacking) return;
        var playerComponent = other.GetComponent<Player>();
        if (playerComponent == null) return;
        playerComponent.TakeDamage(attackDamage);
        _isAttackInCooldown = true;
        _isAttacking = false;
        Invoke(nameof(ResumeAttack), attackCooldown);
    }

    public float Damage()
    {
        return attackDamage;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(patrolPointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(patrolPointB.transform.position, 0.5f);
        Gizmos.DrawLine(patrolPointA.transform.position, patrolPointB.transform.position);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, fovAngle / 2) * transform.right) * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, fovAngle) * transform.right) * detectionRange);
    }
}
