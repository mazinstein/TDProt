using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private Tower _placedTower;
    private static HashSet<Vector2> _occupiedPositions = new HashSet<Vector2>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter: " + collision.name);

        if (_placedTower != null)
            return;

        Tower tower = collision.GetComponent<Tower>();
        if (tower != null && tower.PlacePosition == null)
        {
            Vector2 pos = transform.position;
            if (_occupiedPositions.Contains(pos))
                return; // точка уже занята

            tower.SetPlacePosition(pos);
            tower.LockPlacement(); // <--- добавьте эту строку
            _placedTower = tower;
            _occupiedPositions.Add(pos);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Exit: " + collision.name);

        if (_placedTower == null)
            return;

        Vector2 pos = transform.position;
        _placedTower.SetPlacePosition(null);
        _placedTower = null;
        _occupiedPositions.Remove(pos);
    }
}
