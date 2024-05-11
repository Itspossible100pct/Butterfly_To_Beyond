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

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collider has the tag 'Swatter' and is on the 'Swatter' layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Swatter"))
        {
            
            Debug.Log("Collision happening with: " + collision.gameObject.name);
            Rigidbody swatterRigidbody = collision.rigidbody;
            if (swatterRigidbody != null)
            {
                // Calculate the force to apply on the ant based on the swatter's velocity
                Vector3 forceDirection = -swatterRigidbody.velocity.normalized;
                float forceMagnitude = swatterRigidbody.velocity.magnitude * 50f; // Adjust the multiplier as needed for desired effect

                // Apply the force to the ant's Rigidbody
                Rigidbody antRigidbody = GetComponent<Rigidbody>();
                if (antRigidbody != null)
                {
                    antRigidbody.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
                    // Play the ant sound effect
                    _audioSource.PlayOneShot(_antSound, 1f);
                }
                else
                {
                    Debug.LogError("No Rigidbody attached to the ant.");
                }
            }
        }
    }
    // void RunAway()
    // {
    //     // Generate a random direction vector
    //     Vector3 randomDirection = Random.insideUnitSphere.normalized;
    //
    //     // Ensure the random direction is primarily horizontal by resetting the y component
    //     randomDirection.y = 0;
    //
    //     // Calculate the run away position based on the random direction
    //     Vector3 runAwayPosition = transform.position + randomDirection * 8;
    //
    //     // Set the destination to the new run away position
    //     agent.SetDestination(runAwayPosition);
    //
    //     // Rotate the ant to face away from the run away position
    //     Vector3 directionFromTarget = (transform.position - runAwayPosition).normalized;
    //     Quaternion lookRotation = Quaternion.LookRotation(directionFromTarget);
    //     transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    //
    //     // Play the ant sound effect
    //     _audioSource.PlayOneShot(_antSound, .7f);
    //
    //     // Start the coroutine to wait and then deactivate the ant
    //     StartCoroutine(WaitAndDie());
    // }
    
    
    void RunAway()
    {
        StartCoroutine(DelayedRunAway());
    }
    
    IEnumerator DelayedRunAway()
    {
        // Wait for 1 second after the collision
        yield return new WaitForSeconds(1f);

        // Check if the ant is still within 5 units of the caterpillar
        if (Vector3.Distance(transform.position, target.position) < 5f)
        {
            // Generate the direction vector away from the caterpillar
            Vector3 directionFromTarget = (transform.position - target.position).normalized;

            // Calculate the run away position based on the direction away from the caterpillar
            Vector3 runAwayPosition = transform.position + directionFromTarget * 10; // Adjust the multiplier for desired speed

            // Set the agent's speed to a higher value for faster movement
            agent.speed = 5.0f;

            // Set the destination to the new run away position
            agent.SetDestination(runAwayPosition);

            // Rotate the ant to face away from the caterpillar
            Quaternion lookRotation = Quaternion.LookRotation(directionFromTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);

            // Play the ant sound effect
            _audioSource.PlayOneShot(_antSound, 0.7f);

            // Wait for 2 seconds while the ant moves away
            yield return new WaitForSeconds(2f);

            // Destroy the game object after moving away
            Destroy(gameObject);
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }

    IEnumerator WaitAndDie()
    {
        // Wait for 2 seconds while the ant moves away
        yield return new WaitForSeconds(2f);

        // Destroy the game object after moving away
        Destroy(gameObject);
    }


}