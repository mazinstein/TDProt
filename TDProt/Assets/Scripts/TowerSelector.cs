using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSelector : MonoBehaviour
{
    public TowerPanelUI towerPanelUI;
    public LayerMask towerLayerMask;

    private Tower _selectedTower;

    void Update()
    {
        // ???????????? ?????? ??????? ????? ??????
        if (!Input.GetMouseButtonDown(0)) return;

        // ???? ???? ?? UI — ?????????? ????? ?????
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // ?? ????????????? ??????? ????????? — ???? ??? ?? UI
            return;
        }

        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = new Vector2(wp.x, wp.y);

        RaycastHit2D hit = Physics2D.Raycast(p, Vector2.zero, 0f, towerLayerMask);
        if (hit.collider != null)
        {
            var tower = hit.collider.GetComponentInParent<Tower>();
            if (tower != null)
            {
                SelectTower(tower);
                return;
            }
        }

        // ???? ? ?????? ????? — ????? ?????????
        DeselectCurrent();
    }

    public void SelectTower(Tower tower)
    {
        if (tower == null) return;
        if (_selectedTower == tower) return;

        DeselectCurrent();

        _selectedTower = tower;
        _selectedTower.ToggleOrderInLayer(true);
        if (towerPanelUI != null) towerPanelUI.ShowForTower(_selectedTower);
    }

    public void DeselectCurrent()
    {
        if (_selectedTower != null)
        {
            _selectedTower.ToggleOrderInLayer(false);
            _selectedTower = null;
        }

        if (towerPanelUI != null) towerPanelUI.Hide();
    }
}