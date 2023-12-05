using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomizableNPC : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField][Tooltip("Sets NPC to move around based on selected movement options")] private bool _roaming;
    [SerializeField] [Tooltip("Sets NPC to stay in one place without moving")] private bool _stagnant;
    [SerializeField] private bool _pointBased;
    [SerializeField] [Tooltip("Speed of NPC while point based movement is being used")] private int _movementSpeed;
    [SerializeField] [Tooltip("points that NPC will move between while point based movement is being used")] private Transform[] _movementPoints;

    [SerializeField] [Tooltip("Sets NPC to utilized the NavMesh Components that are assigned while active")] private bool _navMeshBased;
    [SerializeField] private NavMeshAgent _navAgent;
    [SerializeField] [Tooltip("Central location from which NavMesh points will be shot out")] private Transform _navCenterPoint;
    [SerializeField] [Tooltip("distance from the center point that navMesh points will be shot out")] private float _navRange;

    private int _current = 0;

    [Header("FOV and Detection")]                                 
    [SerializeField] public float _radius;
    [SerializeField] [Range(0, 360)] public float _angle;
    [SerializeField] [Tooltip("layer mask that NPC will search for")] private LayerMask _targetMask;
    [SerializeField] [Tooltip("layer mask that will obstuct the NPC field of view")] private LayerMask _obstructionMask;
    [SerializeField] public GameObject _playerRef;
    [SerializeField] private Transform _player;
    public bool _canSeePlayer;


    [Header("Dialogue and SFX")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _dialogue;
    [SerializeField] private int _timeBetweenDialogues;

    private float _time;


    private void Start()
    {
        _time = 0;
        _playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());

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



    private void FixedUpdate()
    {
        if (_roaming == true)
        {
            Roam();
        }

        if (_canSeePlayer == true && _time >= _timeBetweenDialogues)
        {
            LookAt();
            PlaySFX(_dialogue);
            _time = _time - _timeBetweenDialogues;
        }
        else
        {
            _time += Time.deltaTime;
        }
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

    private void PlaySFX(AudioClip[] _audioClip)
    {
        if (_audioClip != null)
        {
            Debug.Log("playing audio");
            int randomIndex = Random.Range(0, _audioClip.Length);
            _audioSource.PlayOneShot(_audioClip[randomIndex]);

        }
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
