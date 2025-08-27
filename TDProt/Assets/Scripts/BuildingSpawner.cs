using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawner : MonoBehaviour
{
    public GameObject[] unitPrefabs; // Префабы юнитов (назначить в инспекторе)
    public Transform spawnPoint; // Точка выхода юнитов
    public float spawnTime = 5f; // Время спавна одного юнита
    public int populationLimit = 50; // Лимит населения

    private Queue<int> spawnQueue = new Queue<int>(); // Очередь юнитов (индексы префабов)
    private int currentPopulation = 0; // Текущее население
    private bool isSpawning = false;

    void Update()
    {
        // Проверка на ввод игрока (например, добавление юнита в очередь)
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Нажатие "1" добавляет первого юнита
        {
            AddUnitToQueue(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) // Нажатие "2" добавляет второго юнита
        {
            AddUnitToQueue(1);
        }
    }

    public void AddUnitToQueue(int unitIndex)
    {
        if (currentPopulation >= populationLimit)
        {
            Debug.Log("Population limit reached!");
            return;
        }

        spawnQueue.Enqueue(unitIndex);
        Debug.Log($"Unit {unitIndex} added to queue!");

        if (!isSpawning)
        {
            StartCoroutine(SpawnUnits());
        }
    }

    IEnumerator SpawnUnits()
    {
        isSpawning = true;

        while (spawnQueue.Count > 0)
        {
            int unitIndex = spawnQueue.Dequeue();
            SpawnUnit(unitIndex);
            yield return new WaitForSeconds(spawnTime);
        }

        isSpawning = false;
    }

    void SpawnUnit(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= unitPrefabs.Length)
        {
            Debug.LogError("Invalid unit index!");
            return;
        }

        Instantiate(unitPrefabs[unitIndex], spawnPoint.position, spawnPoint.rotation);
        currentPopulation++;
        Debug.Log($"Unit {unitIndex} spawned! Current population: {currentPopulation}");
    }

    public void RemoveUnit()
    {
        currentPopulation = Mathf.Max(0, currentPopulation - 1);
    }
}