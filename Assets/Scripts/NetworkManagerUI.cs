using UnityEngine;

public class NetworkManagerUI : MonoBehaviour {
    private void Start() {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
    }

    private void GameManager_OnGameStarted() {
        Hide();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}
