using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour {

    private const float GRID_SIZE = 3.1f;

    [SerializeField]
    private Transform crossPrefab;
    [SerializeField]
    private Transform circlePrefab;
    [SerializeField]
    private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList;

    private void Awake() {
        visualGameObjectList = new List<GameObject>();
    }

    private void Start() {
        GameManager.Instance.OnGridPositionClicked += GameManager_OnGridPositionClicked;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameRematch += GameManager_OnGameRematch;
    }

    private void GameManager_OnGameRematch() {
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        foreach (GameObject visualGameObject in visualGameObjectList) {
            Destroy(visualGameObject);
        }
        visualGameObjectList.Clear();
    }

    private void GameManager_OnGameWin(GameManager.Line line, GameManager.PlayerType playerType) {
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        float eulerZ = 0f;
        switch (line.orientation) {
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = -45f;
                break;
        }
        Transform lineCompleteTransform = Instantiate(lineCompletePrefab, GetGridWorldPosition(line.centerGridPosition.x, line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnGridPositionClicked(int x, int y, GameManager.PlayerType playerType) {
        SpawnObjectRpc(x, y, playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType) {
        Transform prefab = null;
        switch (playerType) {
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
            case GameManager.PlayerType.None:
                Debug.LogError("Invalid player type");
                return;
        }

        Transform spawnedCrossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(spawnedCrossTransform.gameObject);
    } 

    private Vector2 GetGridWorldPosition(int x, int y) {
        return new Vector2(-GRID_SIZE + (x * GRID_SIZE), -GRID_SIZE + (y * GRID_SIZE));
    }
}
