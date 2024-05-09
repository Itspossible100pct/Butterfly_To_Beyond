using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AntEnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;
    private float timer = 0f;
    private bool runAway = false;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _antSound;
    

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //gameObject.SetActive(false);
    }

    public void ActivateAnt(Transform caterpillarTransform)
    {
        target = caterpillarTransform;
        Vector3 spawnPosition = target.position + Random.insideUnitSphere * 3;
        spawnPosition.y = target.position.y;
        transform.position = spawnPosition;
        
        //transform.rotation = Quaternion.Euler(0, 180, 0);
        
        agent.speed = 2.0f;
        gameObject.SetActive(true);
        MoveTowardsTarget();
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            timer += Time.deltaTime;
            if (timer > 10f && !runAway)
            {
                RunAway();
                runAway = true;
            }
            if (!runAway && target != null)
            {
                MoveTowardsTarget();
            }
        }
    }

    void MoveTowardsTarget()
    {
        if (agent.isActiveAndEnabled)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > 0.5f)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                agent.SetDestination(transform.position);
            }
        }
    }

    void RunAway()
    {
        Vector3 directionAwayFromTarget = transform.position - target.position;
        Vector3 runAwayPosition = transform.position + directionAwayFromTarget.normalized * 8;
        agent.SetDestination(runAwayPosition);
        _audioSource.PlayOneShot(_antSound, .7f);

        StartCoroutine(WaitAndDie());
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }
    
    IEnumerator WaitAndDie()
    {
        while (Vector3.Distance(transform.position, target.position) < 5f)
        {
            yield return null; // Wait until next frame before rechecking the condition
        }
        Die();
    }
}