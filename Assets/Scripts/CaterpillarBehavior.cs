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
    private bool isFed = false;
    
    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private bool enemiesDefeated = false;
    
    // Serialize fields for the specific leaf fragments to check
    [SerializeField] private GameObject criticalFragmentLeaf1;
    [SerializeField] private GameObject criticalFragmentLeaf2;

    private enum State { Idle, Hungry, Eating, Afraid, Happy, Celebration }
    private State currentState = State.Idle;

    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        audioSource.playOnAwake = false;
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.autoBraking = false; // Prevents the agent from slowing down at waypoints

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object.");
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        // Start by moving to the first waypoint
        currentState = State.Idle;
        GoToNextState();
    }

    void GoToNextState()
    {
        switch (currentState)
        {
            case State.Idle:
                // Move to the next waypoint if not already moving
                if (currentWaypoint == 0) {
                    MoveToNextWaypoint();
                } else {
                    StartCoroutine(BehaveHungry());
                }
                break;
            case State.Hungry:
                // Check if the critical leaf fragment is inactive to change state
                if ((currentWaypoint == 1 && !criticalFragmentLeaf1.activeSelf) ||
                    (currentWaypoint == 3 && !criticalFragmentLeaf2.activeSelf))
                {
                    StartCoroutine(EatingBehavior());
                }
                break;
            case State.Eating:
                MoveToNextWaypoint();
                break;
            case State.Afraid:
                if (currentWaypoint == 2) {
                    // Ensure we trigger the Afraid behavior only after reaching the 2nd waypoint
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
    }

    IEnumerator BehaveHungry()
    {
        // Ensure we are at the correct waypoint to start being hungry
        if (currentWaypoint >= 1) {
            audioSource.PlayOneShot(hungrySound);
            animator.SetTrigger("isHungry");
        }
        yield return new WaitForSeconds(2);
        currentState = State.Hungry;
    }

    public void Feed()
    {
        if (currentState == State.Hungry && (currentWaypoint == 0 || currentWaypoint == 2))
        {
            isFed = true;
            audioSource.PlayOneShot(eatSound);
            animator.SetTrigger("eat");
            transform.localScale *= 1.1f; // Grow slightly when fed
            currentState = State.Eating;
        }
    }

    IEnumerator EatingBehavior()
    {
        audioSource.PlayOneShot(cuteSound);
        animator.SetTrigger("wiggle");
        yield return new WaitForSeconds(2);
        currentState = State.Afraid; // Move to Afraid to check for enemy defeat
        GoToNextState();
    }

    void MoveToNextWaypoint()
    {
        if (currentWaypoint < waypoints.Length - 1)
        {
            if (currentWaypoint < waypoints.Length - 1)
            {
                currentWaypoint++;
                agent.SetDestination(waypoints[currentWaypoint].position);

                // Check if it's time to start the Hungry behavior
                if (currentWaypoint == 1 || currentWaypoint == 3) {
                    StartCoroutine(BehaveHungry());
                } 
                else 
                {
                    currentState = State.Idle;
                    GoToNextState();
                }
            } 
            else 
            {
                currentState = State.Idle; // Continue idle until the next action is determined
            }
            GoToNextState();
        }
        else
        {
            currentState = State.Celebration;
            GoToNextState();
        }
    }


    IEnumerator ShowHappyAndRelief()
    {
        audioSource.PlayOneShot(happySound);
        yield return new WaitForSeconds(1);
        audioSource.PlayOneShot(reliefSound);
        yield return new WaitForSeconds(1);
        enemiesDefeated = false; // Reset enemies defeated status
        currentState = State.Happy;
        GoToNextState();
    }

    // Method to spawn ants with delay
    public IEnumerator SpawnAnts()
    {
        for (int i = 0; i < totalAnts; i++)
        {
            GameObject ant = Instantiate(antPrefab);
            ant.GetComponent<AntEnemyBehavior>().ActivateAnt(transform);

            if (i < totalAnts - 1)
            {
                yield return new WaitForSeconds(3); // 3-second delay between each ant spawn
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AntSpawnTrigger") && currentWaypoint == 2)
        {
            StartCoroutine(SpawnAnts());
        }
    }

    public void EnemyDefeated()
    {
        defeatedEnemiesCount++;
        if (defeatedEnemiesCount >= totalAnts)
        {
            enemiesDefeated = true;
            if (currentState == State.Afraid)
            {
                GoToNextState();
            }
        }
    }

    IEnumerator StartCelebration()
    {
        animator.SetTrigger("celebrate");
        celebrationParticles.Play();
        while (true)
        {
            audioSource.PlayOneShot(happySound);
            yield return new WaitForSeconds(happySound.length);
        }
    }
    
    IEnumerator PerformAfraidBehavior()
    {
        // Play the scared sound and trigger the animator only if it's the right waypoint
        if (currentWaypoint == 2) {
            audioSource.PlayOneShot(scaredSound);
            animator.SetTrigger("scared");
            yield return new WaitForSeconds(2);
            currentState = State.Happy;
            GoToNextState();
        }
    }
}
