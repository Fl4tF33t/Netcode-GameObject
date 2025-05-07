using UnityEngine;

public class GridPosition : MonoBehaviour {
    [Header("Grid Position")]
    [SerializeField]
    private int x;
    [SerializeField]
    private int y;

    private void OnMouseDown() {
        GameManager.Instance.ClickedOnGridPositionRpc(x, y, GameManager.Instance.GetLocalPlayerType());
    }

    private void OnValidate() {
        this.name = $"Grid Position ({x}, {y})";
    }
}
