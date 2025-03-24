using System.Collections.Generic;
using UnityEngine;

public class BombPool : MonoBehaviour
{
    public GameObject bombPrefab;
    public int poolSize = 15;
    private Queue<GameObject> bombPool = new Queue<GameObject>();

    void Start()
    {
        // Pre-instantiate bomb objects and add them to the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bomb = Instantiate(bombPrefab);
            bomb.SetActive(false);  // Make sure it's not active when pooled
            bombPool.Enqueue(bomb);
        }
    }

    public GameObject GetBomb()
    {
        if (bombPool.Count > 0)
        {
            GameObject bomb = bombPool.Dequeue();
            bomb.SetActive(true);  // Activate bomb when taken from pool
            return bomb;
        }
        else
        {
            // Optionally expand the pool if needed
            GameObject bomb = Instantiate(bombPrefab);
            return bomb;
        }
    }

    public void ReturnBombToPool(GameObject bomb)
    {
        bomb.SetActive(false);  // Deactivate bomb when returning to pool
        bombPool.Enqueue(bomb);
    }
}
