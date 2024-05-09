using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CaterpillarBehaviour : MonoBehaviour
{
    public static CaterpillarBehaviour Instance;
    
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip hungrySound;
    public AudioClip eatSound;
    public AudioClip cuteSound;
    public AudioClip scaredSound;
    public AudioClip reliefSound;
    public AudioClip happySound;
    public Transform[] waypoints;
    
    public float moveSpeed = 1.0f; // Speed of movement, adjustable in the inspector
    public ParticleSystem celebrationParticles; // Particle system for the celebration moment

    public GameObject antPrefab; // Reference to the Ant prefab
    public int totalAnts = 3; // Total number of ants to spawn
    private int defeatedEnemiesCount = 0;
    public bool isFed = false;
    
    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private bool enemiesDefeated = false;
    
    // Serialize fields for the specific leaf fragments to check
    [SerializeField] private GameObject criticalFragmentLeaf1;
    [SerializeField] private GameObject criticalFragmentLeaf2;

    private enum State { Idle, Hungry, Eating, Afraid, Happy, Celebration }
    private State currentState = State.Idle;

    private bool isAntSpawned;

    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object.");
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        agent.speed = moveSpeed;
        agent.autoBraking = false;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Caterpillar is not on the NavMesh at the start.");
            if (!agent.Warp(waypoints[0].position))
            {
                Debug.LogError("Failed to warp the agent to the starting waypoint.");
            }
        }

        currentWaypoint = 1;
        GoToNextState();
    }

    void GoToNextState()
    {
        Debug.Log($"Transitioning from {currentState} to next state at waypoint {currentWaypoint}");
        switch (currentState)
        {
            case State.Idle:
                MoveToNextWaypoint();
                break;
            case State.Hungry:
                // Ensure the caterpillar only moves if it is fed and the critical consumable object is deactivated
                if (isFed && ((currentWaypoint == 1 && !criticalFragmentLeaf1.activeSelf) ||
                              (currentWaypoint == 3 && !criticalFragmentLeaf2.activeSelf)))
                {
                    MoveToNextWaypoint();
                    isFed = false;
                }
                break;
            case State.Eating:
                //MoveToNextWaypoint();
                break;
            case State.Afraid:
                if (currentWaypoint == 2 && !enemiesDefeated) {
                    StartCoroutine(PerformAfraidBehavior());
                }
                break;
            case State.Happy:
                if (currentWaypoint == 3)
                {
                    currentState = State.Celebration;
                    GoToNextState();
                }
                else
                {
                    MoveToNextWaypoint();
                }
                break;
            case State.Celebration:
                StartCoroutine(StartCelebration());
                break;
        }
        Debug.Log($"New state is {currentState}");
    }
    
    void MoveToNextWaypoint()
    {
        if (currentWaypoint < waypoints.Length - 1)
        {
            //currentWaypoint++;
            
            if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            {
                Debug.LogError("Agent is not active or not on NavMesh when trying to move.");
                return;
            }
            
            agent.SetDestination(waypoints[currentWaypoint].position);
            Debug.Log($"Setting destination to waypoint {currentWaypoint} at position {waypoints[currentWaypoint].position}");

            // Prepare the caterpillar for the next state based on the waypoint
            if (currentWaypoint == 1 || currentWaypoint == 3)
            {
                StartCoroutine(BehaveHungry());
            }
            else if (currentWaypoint == 2)
            {
                // At waypoint 2, check if ants should spawn
                if (!isAntSpawned) {
                    SpawnAnts(); // Spawn ants if they have not been spawned
                    isAntSpawned = true; // Prevents re-spawning ants
                }
                currentState = State.Afraid; // Set state to Afraid
                GoToNextState(); // Proceed to process the next state
            }
        }
        else
        {
            currentState = State.Celebration;
            GoToNextState();
        }
    }

    IEnumerator BehaveHungry()
    {
        yield return new WaitForSeconds(0.1f);
        if ((currentWaypoint == 1 || currentWaypoint == 3)) {
            audioSource.PlayOneShot(hungrySound);
            //animator.SetTrigger("isHungry");  // instead of animation LookAt rotation
            yield return new WaitForSeconds(2);
            currentState = State.Hungry;
        }
    }

    public void Feed()
    {
        StartCoroutine(HandleFeeding());
    }
    
    IEnumerator PerformAfraidBehavior()
    {
        
        
        // Check if the caterpillar is at the right waypoint to start the "Afraid" behavior
        if (currentWaypoint == 2 && !isAntSpawned) 
        {
            
            // Spawn ants here and ensure it's not repeated
            SpawnAnts();
            isAntSpawned = true;
            
            yield return new WaitForSeconds(5);
            
            audioSource.PlayOneShot(scaredSound);
            animator.SetTrigger("Afraid");

            yield return new WaitForSeconds(2);
            currentState = State.Happy;
            GoToNextState();
        }
    }

    IEnumerator HandleFeeding()
    {
        if (currentState == State.Hungry && (currentWaypoint == 1 || currentWaypoint == 3))
        {
            Debug.Log("Feeding at waypoint: " + currentWaypoint);
            isFed = true;
            audioSource.PlayOneShot(eatSound);
            animator.SetTrigger("Eating");
            transform.localScale *= 1.05f;
            //currentState = State.Eating;
            Debug.Log("State changed to Eating after feeding");
            
            yield return new WaitForSeconds(2); // Wait for the eating animation to finish
            
            
            // Check if it's the first feeding at Waypoint 1
            if (currentWaypoint == 1 && !criticalFragmentLeaf1.activeSelf)
            {
                currentState = State.Idle; // Make sure the state is reset to idle before moving
                currentWaypoint = 2;  // Move to the next waypoint logically
                Debug.Log("Waypoint change to 2. !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                MoveToNextWaypoint(); // Move physically to the next waypoint
                
                
            }
            else if (currentWaypoint == 3 && !criticalFragmentLeaf2.activeSelf)
            {
                Debug.Log("Transition to celebration after feeding at waypoint 3.");
                yield return new WaitForSeconds(2);
                currentState = State.Celebration;
                GoToNextState();
            }
            else
            {
                Debug.Log("Unexpected feeding situation at waypoint " + currentWaypoint);
                currentState = State.Eating;
                MoveToNextWaypoint(); // Make sure to continue moving correctly
            }
            
            
        }
    }



    IEnumerator ShowHappyAndRelief()
    {
        audioSource.PlayOneShot(happySound);
        yield return new WaitForSeconds(1);
        audioSource.PlayOneShot(reliefSound);
        yield return new WaitForSeconds(1);
        enemiesDefeated = false;
        currentState = State.Happy;
        GoToNextState();
    }

    public void SpawnAnts()
    {
        Debug.Log("Spawning ants now.");
        for (int i = 0; i < totalAnts; i++)
        {
            GameObject ant = Instantiate(antPrefab, waypoints[currentWaypoint].position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), Quaternion.identity);
            ant.GetComponent<AntEnemyBehavior>().ActivateAnt(transform);
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("AntSpawnTrigger") && currentWaypoint == 2)
    //     {
    //         Debug.Log("AntSpawnTrigger activated at waypoint " + currentWaypoint);
    //         StartCoroutine(SpawnAnts());
    //     }
    // }

    public void EnemyDefeated()
    {
        defeatedEnemiesCount++;
        Debug.Log($"Enemy defeated. Total defeated: {defeatedEnemiesCount}. Required: {totalAnts}");
        if (defeatedEnemiesCount >= totalAnts)
        {
            enemiesDefeated = true;
            if (currentState == State.Afraid)
            {
                StartCoroutine(ShowHappyAndRelief()); // Show happy and relief after all enemies are defeated
            }
        }
    }

    IEnumerator StartCelebration()
    {
        animator.SetTrigger("Celebrate");
        celebrationParticles.Play();
        audioSource.PlayOneShot(happySound);
            yield return new WaitForSeconds(happySound.length);
        
    }
}
