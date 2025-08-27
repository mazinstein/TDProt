using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn; // Assign the prefab in the Inspector
    public Transform[] attachPoints; // Points to attach spawned objects
    public float spawnInterval = 5f; // Time between spawns
    public float cooldownDuration = 10f; // Cooldown duration
    public bool isWaveActive = false; // Set this to true during waves

    private bool isCooldown = false;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (!isWaveActive && !isCooldown)
            {
                foreach (var point in attachPoints)
                {
                    if (point != null)
                    {
                        Instantiate(objectToSpawn, point.position, point.rotation, point);
                    }
                }

                isCooldown = true;
                yield return new WaitForSeconds(cooldownDuration);
                isCooldown = false;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}