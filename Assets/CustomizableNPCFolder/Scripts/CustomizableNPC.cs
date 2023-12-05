using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomizableNPC : MonoBehaviour
{
    [Header("NPC Type")]
    [SerializeField] private bool _enemy;
    [SerializeField] private bool _villager;
    [SerializeField] private bool _guard;

    [Header("Health")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _currentHealth;

    [Header("Movement")]
    [SerializeField] private bool _roaming;
    [SerializeField] private bool _stagnant;
    [SerializeField] private int _movementSpeed;
    [SerializeField] private bool _pointBased;
    [SerializeField] private Transform[] _movementPoints;

    [SerializeField] private bool _navMeshBased;
    [SerializeField] private NavMeshAgent _navAgent;
    [SerializeField] private Transform _navCenterPoint;
    [SerializeField] private float _navRange;

    private int _current = 0;

    [Header("FOV and Detection")]                                 
    [SerializeField] public float _radius;
    [SerializeField] [Range(0, 360)] public float _angle;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstructionMask;
    [SerializeField] public GameObject _playerRef;
    [SerializeField] private Transform _player;
    public bool _canSeePlayer;


    [Header("Dialogue and SFX")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _greetings;
    [SerializeField] private AudioClip[] _warnings;
    [SerializeField] private int _timeBetweenPassingDialogues;
    [SerializeField] private AudioClip[] _tookDamageSFX;
    [SerializeField] private AudioClip[] _deathSFX;
    private int _time;

    [Header("VFX")]
    [SerializeField] private ParticleSystem _tookDamageParticles;
    [SerializeField] private ParticleSystem _deathPaticles;

    private void Start()
    {
        _time = 0;
        _playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
    }
    public enum State
    {
        Initialize, PeacefulRoaming, PeacefulStagnant, PeacefulWarning, HostileRoaming, HostileGuarding, HostileSearching, Attacking
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    public State CurrentState = State.Initialize;



    //STATES........

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Initialize:
                DoInitialize();
                break;
            case State.PeacefulRoaming:
                DoPeacefulRoaming();
                break;
            case State.PeacefulStagnant:
                DoPeacefulStagnant();
                break;
            case State.HostileRoaming:
                DoHostileRoaming();
                break;
            case State.HostileGuarding:
                DoHostileGuarding();
                break;
            case State.HostileSearching:
                DoHostileSearching();
                break;
            case State.Attacking:
                DoAttacking();
                break;
        }

        if (_villager == true && _canSeePlayer == true && _time >= _timeBetweenPassingDialogues)
        {
            PlaySFX(_greetings);
            _time = 0;
        }
        else
        {
            _time += 1; 
        }

        if (_guard == true && _canSeePlayer == true && _time >= _timeBetweenPassingDialogues)
        {
            PlaySFX(_warnings);
            _time = 0;
        }
        else
        {
            _time += 1;
        }

        if (_enemy == true && _canSeePlayer == true && _time >= _timeBetweenPassingDialogues)
        {
            PlaySFX(_warnings);
            _time = 0;
        }
        else
        {
            _time += 1;
        }
    }

    private void DoInitialize()
    {
        if (_villager == true && _stagnant == true)
        {
            ChangeState(State.PeacefulStagnant);
        }

        if (_villager == true && _roaming == true)
        {
            ChangeState(State.PeacefulRoaming);
        }

        if (_guard == true && _stagnant == true)
        {
            ChangeState(State.HostileGuarding);
        }

        if (_guard == true && _roaming == true)
        {
            ChangeState(State.HostileRoaming);
        }

        if (_enemy == true && _stagnant == true)
        {
            ChangeState(State.HostileGuarding);
        }

        if (_enemy == true && _roaming == true)
        {
            ChangeState(State.HostileRoaming);
        }
    }

    private void DoPeacefulRoaming()
    {
        Debug.Log("roaming");
        Roam();
    }

    private void DoPeacefulStagnant()
    {
        if (_pointBased == false && _navMeshBased == false)
        {
            Debug.Log("NPC is Stagnant");
        }
    }


    private void DoHostileRoaming()
    {
        Debug.Log("roaming");
        Roam();
    }

    private void DoHostileGuarding()
    {

    }

    private void DoHostileSearching()
    {

    }

    private void DoAttacking()
    {

    }

    public void ChangeState(State newstate)
    {
        if (CurrentState == newstate)
            return;
        CurrentState = newstate;
    }
   

    //FUNCTIONS........
    private void Roam()
    {
        if (_pointBased == true)
        {
            MovePointBased();
            Debug.Log("NPC is moving using pointBased");
        }

        if (_navMeshBased == true)
        {
            MoveNavMesh();
            Debug.Log("NPC is moving using navMesh");
        }
    }

    private void MoveNavMesh()
    {
        if(_navAgent.remainingDistance <= _navAgent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(_navCenterPoint.position, _navRange, out point))
            {
                _navAgent.SetDestination(point);
            }
        }
    }
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private void MovePointBased()
    {
        if (transform.position != _movementPoints[_current].position)
        {
            transform.position = Vector3.MoveTowards(transform.position, _movementPoints[_current].position, _movementSpeed * Time.deltaTime);
            transform.LookAt(_movementPoints[_current].position);
        }
        else
            _current = (_current + 1) % _movementPoints.Length;
    }

     private void SpeakGreeting()
    {
        int randomIndex = Random.Range(0, _greetings.Length);
        _audioSource.PlayOneShot(_greetings[randomIndex]);
        
    }

    private void SpeakWarning()
    {
        int randomIndex = Random.Range(0, _warnings.Length);
        _audioSource.PlayOneShot(_warnings[randomIndex]);
    }
    private void PlayVFX(ParticleSystem _particleSystem)
    {
        if (_particleSystem != null)
        {
            // spawn a particle effect from assets
            ParticleSystem newParticle = Instantiate(_particleSystem,
                transform.position, Quaternion.identity);
            newParticle.Play();
        }
    }

    private void PlaySFX(AudioClip[] _audioClip)
    {
        if (_audioClip != null)
        {
            Debug.Log("playing audio");
            int randomIndex = Random.Range(0, _audioClip.Length);
            _audioSource.PlayOneShot(_audioClip[randomIndex]);

        }
    }

    private void Die()
    {
        PlaySFX(_deathSFX);
        PlayVFX(_deathPaticles);
        gameObject.SetActive(false);
    }

    private void Retreat()
    {

    }

    private void LookAt()
    {
        transform.LookAt(_player);
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _radius, _targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < _angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstructionMask))
                {
                    _canSeePlayer = true;
                }
                else
                    _canSeePlayer = false;
            }
            else
                _canSeePlayer = false;
        }
        else if (_canSeePlayer)
            _canSeePlayer = false;
    }

}
