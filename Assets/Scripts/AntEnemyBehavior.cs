using UnityEngine;
using UnityEngine.AI;

public class AntEnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target; // The target (Caterpillar) the ant will move towards

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        gameObject.SetActive(false); // Start with the ant deactivated
    }

    public void ActivateAnt(Transform caterpillarTransform)
    {
        target = caterpillarTransform;

        // Spawn this ant at a random position around the caterpillar within a 3-unit radius
        Vector3 spawnPosition = target.position + Random.insideUnitSphere * 3;
        spawnPosition.y = target.position.y; // Ensure they are on the same ground level if needed
        transform.position = spawnPosition;

        gameObject.SetActive(true);

        // Start moving towards the caterpillar
        MoveTowardsTarget();
    }

    void Update()
    {
        if (target != null && gameObject.activeSelf)
        {
            MoveTowardsTarget();
        }
    }

    void MoveTowardsTarget()
    {
        if (agent.isActiveAndEnabled)
        {
            agent.SetDestination(target.position);
        }
    }

    public void Die()
    {
        // Play any death animation or sound
        // For now, we will just deactivate the GameObject
        gameObject.SetActive(false);

        // Notify the CaterpillarBehaviour script that an enemy has been defeated
        //CaterpillarBehaviour.Instance.EnemyDefeated();
    }
    
    
}