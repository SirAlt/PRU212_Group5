using System;
using System.Collections;
using UnityEngine;

public class FlyingEye : Enemy, IDamageable, IHealable
{
    private const string FlyAnim = "Fly";
    private const string BiteAnim = "Bite";
    private const string ShootAnim = "Shoot";
    private const string HitAnim = "Hit";
    private const string DyingAnim = "Dying";
    private const string DeathAnim = "Death";

    private const string FlapSpeedParam = "flapSpeed";

    [SerializeField] private DetectionZone detector;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask visualObstructionLayers;
    [SerializeField] private bool relentless;

    [Header("Attacks")]
    [SerializeField] private float globalAttackCooldown;
    [SerializeField] private float movementResumeDelay;
    [SerializeField] private int aggroHyperArmorHitCount;
    [SerializeField] private Attack biteAttack;
    [SerializeField] private int biteHyperArmorHitCount;
    [SerializeField] private float biteCooldown;
    [SerializeField] private int shootHyperArmorHitCount;
    [SerializeField] private float shootCooldown;
    [SerializeField] private int barrageSize;
    [SerializeField] private float barrageDelay;
    [SerializeField] private int spreadAngle;

    [Header("Flinch")]
    [SerializeField] private float flinchDistance;
    [SerializeField] private bool flinchHyperArmor;

    private Animator _anim;
    private Rigidbody2D _rb;
    private EnemyFacing _facing;
    private WaypointMover _patrolMover;
    private TetherMover _combatMover;
    private SimpleMover _followMover;
    private ChaseMover _biteChaseMover;
    private IHitReceptor _hurtbox;

    private Transform _target;
    private DarkBall[] _darkballs;
    private Vector2 _lastHitForce;

    #region IDamageable

    [field: SerializeField, Header("IDamageable")] public float MaxHealth { get; set; }

    [SerializeField] private float _currentHealth;
    public float CurrentHealth
    {
        get => _currentHealth;
        private set
        {
            if (_currentHealth == value) return;
            _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
        }
    }

    private BodyContacts _bodyContacts;

    public void TakeDamage(float damage) => TakeDamage(damage, Vector2.zero);

    public void TakeDamage(float damage, Vector2 direction)
    {
        if (_stateMachine.CurrentState == _biteState)
        {
            damage = 1f;
        }
        else if (_stateMachine.CurrentState == _biteChaseState)
        {
            damage = Mathf.Floor(damage / 2f);
        }
        CurrentHealth -= damage;
        CharacterEvents.characterDamaged?.Invoke(gameObject, damage);

        if (CurrentHealth > 0)
        {
            _lastHitForce = direction;
            if (flinchHyperArmor && _stateMachine.CurrentState == _hitState)
                return;
            if (_stateMachine.CurrentState == _aggroState
                || _stateMachine.CurrentState == _biteChaseState
                || _stateMachine.CurrentState == _biteState
                || _stateMachine.CurrentState == _shootState)
            {
                if (_hyperArmor-- > 0) return;
            }
            _stateMachine.ChangeState(_hitState);
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        _stateMachine.ChangeState(_dyingState);
    }

    #endregion

    #region IHealable

    public bool Heal(float amount)
    {
        if (CurrentHealth <= 0f || CurrentHealth >= MaxHealth) return false;
        CurrentHealth += amount;
        CharacterEvents.characterHealed?.Invoke(gameObject, amount);
        return true;
    }

    #endregion

    #region States

    private State _patrolState;
    private State _aggroState;
    private State _followState;
    private State _biteChaseState;
    private State _biteState;
    private State _shootState;
    private State _diveState;
    private State _hitState;
    private State _dyingState;
    private State _deadState;

    private StateMachine _stateMachine;

    private enum AnimationEvent
    {
        AttackAimFinished,
        AttackWindupFinished,
        AttackRecoveryFinished,
        FlinchFinished,
        BodyLanded,
    }

    #region Commands

    private void NotifyAnimEventTriggered(AnimationEvent animEvent)
    {
        _stateMachine.CurrentState.HandleAnimEvent(animEvent);
    }

    private void ReturnToFlight()
    {
        if (detector.detected.Count > 0)
            _stateMachine.ChangeState(_aggroState);
        else if (relentless)
            _stateMachine.ChangeState(_followState);
        else
            _stateMachine.ChangeState(_patrolState);
    }

    #region Patrol

    private void Patrol_Enter()
    {
        _aggroTimer = 0f;
        _patrolMover.enabled = true;

        _anim.Play(FlyAnim, -1);
        _anim.SetFloat(FlapSpeedParam, 1f);

        _combatMover.UnsetTarget();
        _biteChaseMover.UnsetTarget();
    }

    private void Patrol_Exit()
    {
        _patrolMover.enabled = false;
    }

    private State Patrol_CheckTransition()
    {
        if (detector.detected.Count > 0) return _aggroState;
        return null;
    }

    private void Patrol_Update()
    {
        if (_patrolMover.MoveVector.x < 0)
            _facing.FaceLeft();
        else
            _facing.FaceRight();
    }

    #endregion

    #region Aggro

    private float _aggroTimer;

    private float _globalAtkCdTimer;

    private float _biteCdTimer;
    private float _shootCdTimer;
    private float _diveCdTimer;

    private float _biteChance;
    private float _shootChance;
    private float _diveChance;

    private bool HasLineOfSight()
    {
        if (_target == null) return false;

        var targetVector = -transform.position;
        var hit = Physics2D.Linecast(transform.position, _target.position, visualObstructionLayers);

#if UNITY_EDITOR
        if (hit)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            return false;
        }
        else
        {
            Debug.DrawLine(transform.position, _target.position, Color.green);
            return true;
        }
#else
return !hit;
#endif
    }

    private void Aggro_Enter()
    {
        _hyperArmor = aggroHyperArmorHitCount;

        _target = detector.detected[0].transform;
        _combatMover.enabled = true;
        _combatMover.SetTarget(_target);

        _anim.Play(FlyAnim, -1);
        _anim.SetFloat(FlapSpeedParam, 1.5f);
    }

    private void Aggro_Exit()
    {
        _combatMover.enabled = false;
    }

    private State Aggro_CheckTransition()
    {
        if (detector.detected.Count == 0)
            return relentless ? _followState : _patrolState;

        var hasLineOfSight = HasLineOfSight();

        if (_globalAtkCdTimer > 0) return null;

        var distanceToTarget = Vector2.Distance(_target.position, transform.position);
        _biteChance = GetBiteChance();
        _shootChance = GetShootChance();
        _diveChance = GetDiveChance();

        var attackChoice = RollAttackChoice();
        if (attackChoice != null)
        {
            var varianceAvg = Mathf.Lerp(0.5f, -1.5f, _aggroTimer / 20f);
            _globalAtkCdTimer = globalAttackCooldown + UnityEngine.Random.Range(varianceAvg - 0.75f, varianceAvg + 0.75f);
        }
        return attackChoice;

        float GetBiteChance()
        {
            if (_biteCdTimer > 0) return 0;

            var baseChance = 15f;

            var distanceFactor = Mathf.Lerp(8f, -3f, distanceToTarget / 6f);

            float healthFactor;
            var healthRatioCutoff = 0.4f;
            if (CurrentHealth / MaxHealth < healthRatioCutoff)
                healthFactor = 15f;
            else
                healthFactor = Mathf.Lerp(-6f, 3f, (CurrentHealth / MaxHealth - healthRatioCutoff) / (1 - healthRatioCutoff));

            var lineOfSightFactor = 0f;
            if (!hasLineOfSight)
            {
                var distanceCutoff = 8.0f;
                if (distanceToTarget > distanceCutoff)
                    lineOfSightFactor = 30f;
            }

            var result = baseChance + distanceFactor + healthFactor + lineOfSightFactor;
            return result >= 0 ? result : 0;
        }

        float GetShootChance()
        {
            if (_shootCdTimer > 0) return 0;

            var baseChance = 25f;

            float distanceFactor;
            var safetyDistance = 6f;
            if (distanceToTarget < safetyDistance)
                distanceFactor = -12f;
            else
                distanceFactor = Mathf.Lerp(0f, 15f, (distanceToTarget - safetyDistance) / 2f);

            var healthFactor = Mathf.Lerp(12f, -6f, CurrentHealth / MaxHealth);

            var lineOfSightFactor = 0f;
            if (!hasLineOfSight) lineOfSightFactor = -30f;

            var result = baseChance + distanceFactor + healthFactor + lineOfSightFactor;
            return result >= 0 ? result : 0;
        }

        float GetDiveChance()
        {
            return 0f;
        }

        State RollAttackChoice()
        {
            var attackChance = _biteChance + _shootChance + _diveChance;
            var _combatStartFactor = _aggroTimer * 5;

            var choice = UnityEngine.Random.Range(0f, Mathf.Max(175f - _combatStartFactor, 100f));

            if (choice >= attackChance) return null;
            if (choice < _biteChance) return _biteChaseState;
            if (choice < _biteChance + _shootChance) return _shootState;
            return _diveState;
        }
    }

    private void FaceTarget()
    {
        if ((_target.position - transform.position).x < 0)
            _facing.FaceLeft();
        else
            _facing.FaceRight();
    }

    #endregion

    #region Follow

    private float _followTimer;

    private void StartFollow()
    {
        _followMover.enabled = true;
        _followMover.SetTarget(_target);
        _anim.Play(FlyAnim, -1);
        _anim.SetFloat(FlapSpeedParam, 1.75f);
    }

    private void EndFollow()
    {
        _followTimer = 0f;
        _followMover.enabled = false;
    }

    private State Follow_CheckTransition()
    {
        if (detector.detected.Count > 0) return _aggroState;
        if (_followTimer > 60.0f) return _patrolState;
        return null;
    }

    #endregion

    #region Bite Chase

    private float _chaseTimer;
    private int _hyperArmor;

    private void StartChase()
    {
        _chaseTimer = 5.0f;
        _biteChaseMover.enabled = true;
        _biteChaseMover.SetTarget(_target);
        _anim.Play(FlyAnim, -1);
        _anim.SetFloat(FlapSpeedParam, 2.25f);
    }

    private void EndChase()
    {
        _biteChaseMover.enabled = false;
    }

    private State BiteChase_CheckTransition()
    {
        _chaseTimer -= Time.deltaTime;  // Use delta time for higher precision.
        var distanceToTarget = Vector2.Distance(_target.position, transform.position);
        if (distanceToTarget < 2.4f)
        {
            return _biteState;
        }
        if (_chaseTimer < 0f && distanceToTarget > 3.0f)
        {
            StartCoroutine(nameof(AndFlight));
            return null;
        }
        return null;
    }

    #endregion

    #region Bite

    private void Bite_Enter()
    {
        _anim.Play(BiteAnim, -1, 0f);
    }

    private void Bite_Exit()
    {
        EndChase();
        biteAttack.SetActive(false);
    }

    private void Bite_AnimEvent(AnimationEvent animEvent)
    {
        switch (animEvent)
        {
            case AnimationEvent.AttackAimFinished:
                EndChase();
                break;
            case AnimationEvent.AttackWindupFinished:
                _biteCdTimer = biteCooldown;
                biteAttack.SetActive(true);
                break;
            case AnimationEvent.AttackRecoveryFinished:
                biteAttack.SetActive(false);
                StartCoroutine(nameof(AndFlight));
                break;
        }
    }

    #endregion

    #region Shoot

    private void Shoot_Enter()
    {
        _hyperArmor = shootHyperArmorHitCount;
        _anim.Play(ShootAnim, -1, 0f);
    }

    private void Shoot_Exit()
    {
        StopCoroutine(nameof(Fire));
        StopCoroutine(nameof(Blast));
        StopCoroutine(nameof(AndFlight));
    }

    private void Shoot_AnimEvent(AnimationEvent animEvent)
    {
        switch (animEvent)
        {
            case AnimationEvent.AttackWindupFinished:
                _shootCdTimer = shootCooldown;

                var roll = 0f;
                if (spreadAngle > 0f) roll = UnityEngine.Random.Range(0f, 1f);
                if (roll > 0.5f) StartCoroutine(nameof(Blast), barrageSize);
                else StartCoroutine(nameof(Fire), barrageSize);
                break;
            case AnimationEvent.AttackRecoveryFinished:
                StartCoroutine(nameof(AndFlight));
                break;
        }
    }

    private IEnumerator Blast(int count)
    {
        var sin = Mathf.Sin(spreadAngle * Mathf.Deg2Rad);
        var cos = Mathf.Cos(spreadAngle * Mathf.Deg2Rad);

        var straight = (_target.position - firePoint.position).normalized;
        var angledUp = new Vector3(straight.x * cos - straight.y * sin, straight.x * sin + straight.y * cos);
        var angledDown = new Vector3(straight.x * cos + straight.y * sin, -straight.x * sin + straight.y * cos);

        var nozzleLength = Mathf.Max(firePoint.localPosition.x, 0f);
        var nozzleBase = firePoint.position - nozzleLength * straight;

#if UNITY_EDITOR
        Debug.DrawLine(nozzleBase, nozzleBase + nozzleLength * straight, Color.magenta);
        Debug.DrawLine(nozzleBase, nozzleBase + nozzleLength * angledUp, Color.magenta);
        Debug.DrawLine(nozzleBase, nozzleBase + nozzleLength * angledDown, Color.magenta);
#endif

        for (var i = 0; i < count; i++)
        {
            var idx = FindDarkball();

            Vector2 launchDir = straight;
            Vector3 launchPosition = firePoint.position;
            switch (i % 3)
            {
                case 1:
                    launchDir = angledUp;
                    launchPosition = nozzleBase + nozzleLength * angledUp;
                    break;
                case 2:
                    launchDir = angledDown;
                    launchPosition = nozzleBase + nozzleLength * angledDown;
                    break;
            }

            _darkballs[idx].transform.position = launchPosition;
            _darkballs[idx].GetComponent<DarkBall>().Launch(_target, launchDir);
            yield return new WaitForSeconds(barrageDelay);
        }
    }

    private IEnumerator Fire(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var idx = FindDarkball();
            _darkballs[idx].transform.position = firePoint.position;
            _darkballs[idx].GetComponent<DarkBall>().Launch(_target, Vector2.zero);
            yield return new WaitForSeconds(barrageDelay);
        }
    }

    private IEnumerator AndFlight()
    {
        yield return new WaitForSeconds(movementResumeDelay);
        ReturnToFlight();
    }

    private int FindDarkball()
    {
        for (int i = 0; i < _darkballs.Length; i++)
        {
            if (!_darkballs[i].gameObject.activeInHierarchy)
            {
                return i;
            }
        }
        return 0;
    }

    #endregion

    #region Dive

    // TODO

    #endregion

    #region Hit

    private Vector2 _flinchVector;

    private void Hit_Enter()
    {
        _anim.Play(HitAnim, -1, 0f);
        _flinchVector = flinchDistance * Time.fixedDeltaTime * _lastHitForce;
    }

    private void Hit_Update()
    {
        _rb.position += _flinchVector;
        transform.position += (Vector3)_flinchVector;
    }

    private void Hit_AnimEvent(AnimationEvent animEvent)
    {
        if (animEvent == AnimationEvent.FlinchFinished)
        {
            ReturnToFlight();
        }
    }

    #endregion

    #region Dying

    private void Dying_Enter()
    {
        _anim.Play(DyingAnim, -1, 0f);
        _rb.gravityScale = 1.2f;
        _hurtbox.SetActive(false);
    }

    private State Dying_CheckTransition()
    {
        if (_bodyContacts.Ground)
        {
            return _deadState;
        }
        return null;
    }

    #endregion

    #region Dead

    private void Dead_Start()
    {
        _anim.Play(DeathAnim);
        var mod = transform.localScale.x * transform.localScale.y;
        ScreenShake.Instance.Shake(2f * mod, 0.35f * mod);
    }

    private void Dead_Update()
    {
        _rb.velocity = Vector2.Lerp(_rb.velocity, Vector2.zero, 0.02f);
    }

    private void Dead_AnimEvent(AnimationEvent animEvent)
    {
        if (animEvent == AnimationEvent.BodyLanded)
        {
            Invoke(nameof(DestroyBody), 3f);
        }
    }

    private void DestroyBody() => Destroy(transform.parent.gameObject);

    #endregion

    #endregion

    #region Classes

    private class State
    {
        private readonly Action _enter;
        private readonly Action _exit;
        private readonly Func<State> _checkTransition;
        private readonly Action _process;
        private readonly Action<AnimationEvent> _handleAnimEvent;

        public State(Action enter, Action exit, Func<State> checkTransition, Action process, Action<AnimationEvent> handleAnimEvent)
        {
            _enter = enter;
            _exit = exit;
            _checkTransition = checkTransition;
            _process = process;
            _handleAnimEvent = handleAnimEvent;
        }

        public void Enter() => _enter?.Invoke();
        public void Exit() => _exit?.Invoke();
        public State CheckTransition() => _checkTransition?.Invoke();
        public void Update() => _process?.Invoke();
        public void HandleAnimEvent(AnimationEvent animEvent) => _handleAnimEvent?.Invoke(animEvent);
    }

    private class StateMachine
    {
        private State _startingState;

        public State PrevState { get; private set; }
        public State CurrentState { get; private set; }
        public State NextState { get; private set; }

        public void Initialize(State state)
        {
            _startingState = state;
            PrevState = null;
            CurrentState = state;
            NextState = null;
            CurrentState.Enter();
        }

        public void ChangeState(State newState)
        {
            NextState = newState;
            CurrentState.Exit();
            PrevState = CurrentState;
            CurrentState = newState;
            NextState = null;
            CurrentState.Enter();
        }

        public void Update()
        {
            var newState = CurrentState.CheckTransition();
            if (newState != null) ChangeState(newState);
            CurrentState.Update();
        }

        public void Reset()
        {
            CurrentState?.Exit();
            if (_startingState != null)
                Initialize(_startingState);
        }
    }

    #endregion

    #endregion

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _facing = GetComponent<EnemyFacing>();
        _bodyContacts = GetComponent<BodyContacts>();
        _patrolMover = GetComponent<WaypointMover>();
        _combatMover = GetComponent<TetherMover>();
        _followMover = GetComponent<SimpleMover>();
        _biteChaseMover = GetComponent<ChaseMover>();
        _hurtbox = GetComponentInChildren<IHitReceptor>();

        _darkballs = transform.parent.GetComponentsInChildren<DarkBall>(includeInactive: true);

        _stateMachine = new StateMachine();

        InitStates();
    }

    private void InitStates()
    {
        _patrolState = new State(
            Patrol_Enter,
            Patrol_Exit,
            Patrol_CheckTransition,
            Patrol_Update,
            (_) => { });
        _aggroState = new State(
            Aggro_Enter,
            Aggro_Exit,
            Aggro_CheckTransition,
            FaceTarget,
            (_) => { });
        _followState = new State(
            StartFollow,
            EndFollow,
            Follow_CheckTransition,
            () => { },
            (_) => { });
        _biteChaseState = new State(
            () => { StartChase(); _hyperArmor = biteHyperArmorHitCount; },
            () => { if (_stateMachine.NextState != _biteState) EndChase(); },
            BiteChase_CheckTransition,
            FaceTarget,
            (_) => { });
        _biteState = new State(
            Bite_Enter,
            Bite_Exit,
            () => null,
            FaceTarget,
            Bite_AnimEvent);
        _shootState = new State(
            Shoot_Enter,
            Shoot_Exit,
            () => null,
            () => { },
            Shoot_AnimEvent);
        _diveState = new State(
            () => { },
            () => { },
            () => null,
            () => { },
            (_) => { });
        _hitState = new State(
            Hit_Enter,
            () => { },
            () => null,
            Hit_Update,
            Hit_AnimEvent);
        _dyingState = new State(
            Dying_Enter,
            () => { },
            Dying_CheckTransition,
            () => { },
            (_) => { });
        _deadState = new State(
            Dead_Start,
            () => { },
            () => null,
            Dead_Update,
            Dead_AnimEvent);
    }

    private void OnEnable()
    {
        _patrolMover.enabled = false;
        _combatMover.enabled = false;
        _biteChaseMover.enabled = false;
        if (_followMover != null) _followMover.enabled = false;

        CurrentHealth = MaxHealth;

        // Really ugly. We should've just used destroy and reload instead of trying to be re-entrant.
        if (_stateMachine.CurrentState == _dyingState) return;
        if (_stateMachine.CurrentState == _deadState)
        {
            DestroyBody();
        }
        else
        {
            _stateMachine.Reset();
        }
    }

    private void Start()
    {
        _stateMachine.Initialize(_patrolState);
    }

    private void Update()
    {
        _globalAtkCdTimer -= Time.deltaTime;
        _biteCdTimer -= Time.deltaTime;
        _shootCdTimer -= Time.deltaTime;
        _diveCdTimer -= Time.deltaTime;

        if (_stateMachine.CurrentState != _patrolState)
            _aggroTimer += Time.deltaTime;
        if (_stateMachine.CurrentState == _followState)
            _followTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        _stateMachine.Update();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (detector == null) Debug.LogWarning($"No Detector assigned for [ {gameObject.name} ].", gameObject);
        if (biteAttack == null) Debug.LogWarning($"No Bite Attack assigned for [ {gameObject.name} ].", gameObject);
    }
#endif
}
