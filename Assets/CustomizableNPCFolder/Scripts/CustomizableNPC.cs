using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private int _movementSpeed;

    [Header("FOV and Detection")]
    [SerializeField] public float _radius;
    [SerializeField] [Range(0, 360)] public float _angle;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstructionMask;
    [SerializeField] public GameObject _playerRef;
    [SerializeField] private Transform _player;
    public bool _canSeePlayer;


    [Header("Dialogue and SFX")]
    [SerializeField] private AudioClip[] _passingDialogueSFX;
    [SerializeField] private AudioClip[] _tookDamageSFX;
    [SerializeField] private AudioClip[] _deathSFX;

    [Header("VFX")]
    [SerializeField] private ParticleSystem _tookDamageParticles;
    [SerializeField] private ParticleSystem _deathPaticles;

    private void Start()
    {
        _playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
    }
    public enum State
    {
        PeacefulRoaming, PeacefulStagnant, PeacefulWarning, HostileRoaming, HostileGuarding, HostileSearching, Attacking
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

    public State CurrentState = State.PeacefulRoaming;
    private void Update()
    {
        switch (CurrentState)
        {
            case State.PeacefulRoaming:
                DoPeacefulRoaming();
                break;
            case State.PeacefulStagnant:
                DoPeacefulStagnant();
                break;
            case State.PeacefulWarning:
                DoPeacefulWarning();
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
    }

    private void DoPeacefulRoaming()
    {

    }

    private void DoPeacefulStagnant()
    {

    }

    private void DoPeacefulWarning()
    {

    }

    private void DoHostileRoaming()
    {

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

    private void Roam()
    {

    }

    private void MoveNavMesh()
    {

    }

    private void MovePointBased()
    {

    }

    private void Speak()
    {

    }

    private void PlayVFX(ParticleSystem _particleSystem)
    {

    }

    private void PlaySFX(AudioClip _audioClip)
    {

    }

    private void Die()
    {

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
