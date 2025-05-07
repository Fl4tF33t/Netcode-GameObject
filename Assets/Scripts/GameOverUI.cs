using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI resultTextMesh;
    [SerializeField]
    private Color winColor;
    [SerializeField]
    private Color loseColor;
    [SerializeField] 
    private Color tieColor;
    [SerializeField]
    private Button rematchButton;

    private void Awake() {
        rematchButton.onClick.AddListener(() => {
            GameManager.Instance.RematchRpc();            
        });
    }

    private void Start() {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameRematch += GameManager_OnGameRematch;
        GameManager.Instance.OnGameDraw += GameManager_OnGameDraw;
        Hide();
    }

    private void GameManager_OnGameDraw() {
        resultTextMesh.text = "Draw!";
        resultTextMesh.color = tieColor;
        Show();
    }

    private void GameManager_OnGameRematch() {
        Hide();
    }

    private void GameManager_OnGameWin(GameManager.Line line, GameManager.PlayerType type) {
        if (GameManager.Instance.GetLocalPlayerType() == type) {
            resultTextMesh.text = "You Win!";
            resultTextMesh.color = winColor;
        } else {
            resultTextMesh.text = "You Lose!";
            resultTextMesh.color = loseColor;
        }
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}
