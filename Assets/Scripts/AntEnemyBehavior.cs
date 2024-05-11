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
        gameObject.SetActive(true);
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
                // Rotate the ant to face away from the target
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Quaternion
                    lookRotation =
                        Quaternion.LookRotation(-directionToTarget); // Notice the negative sign to invert the direction
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            }
            else
            {
                agent.SetDestination(transform.position);
            }
        }
    }

    void RunAway()
    {
        // Generate a random direction vector
        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        // Ensure the random direction is primarily horizontal by resetting the y component
        randomDirection.y = 0;

        // Calculate the run away position based on the random direction
        Vector3 runAwayPosition = transform.position + randomDirection * 8;

        // Set the destination to the new run away position
        agent.SetDestination(runAwayPosition);

        // Rotate the ant to face away from the run away position
        Vector3 directionFromTarget = (transform.position - runAwayPosition).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionFromTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);

        // Play the ant sound effect
        _audioSource.PlayOneShot(_antSound, .7f);

        // Start the coroutine to wait and then deactivate the ant
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