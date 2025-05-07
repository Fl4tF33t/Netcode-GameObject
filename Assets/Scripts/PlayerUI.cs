using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    private GameObject crossArrowGameObject;
    [SerializeField]
    private GameObject circleArrowGameObject;
    [SerializeField]
    private GameObject crossYouTextGeameObject;
    [SerializeField]
    private GameObject circleYouTextGameObject;
    [SerializeField]
    private TextMeshProUGUI crossScoreTextMesh;
    [SerializeField]
    private TextMeshProUGUI circleScoreTextMesh;

    private void Awake() {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossYouTextGeameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);

        crossScoreTextMesh.text = "";
        circleScoreTextMesh.text = "";
    }

    private void Start() {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
    }

    private void GameManager_OnScoreChanged() {
        GameManager.Instance.GetScores(out int crossScore, out int circleScore);
        crossScoreTextMesh.text = crossScore.ToString();
        circleScoreTextMesh.text = circleScore.ToString();
    }

    private void GameManager_OnGameStarted() {
        switch (GameManager.Instance.GetLocalPlayerType()) {
            case GameManager.PlayerType.Cross:
                crossYouTextGeameObject.SetActive(true);
                break;
            case GameManager.PlayerType.Circle:
                circleYouTextGameObject.SetActive(true);
                break;
        }
        crossScoreTextMesh.text = "0";
        circleScoreTextMesh.text = "0";

        UpdateCurrentArrow();
    }
    
    private void GameManager_OnCurrentPlayablePlayerTypeChanged() {
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow() {
        switch (GameManager.Instance.GetCurrentPlayablePlayerType()) {
            case GameManager.PlayerType.Cross:
                crossArrowGameObject.SetActive(true);
                circleArrowGameObject.SetActive(false);
                break;
            case GameManager.PlayerType.Circle:
                crossArrowGameObject.SetActive(false);
                circleArrowGameObject.SetActive(true);
                break;
            default:
                crossArrowGameObject.SetActive(false);
                circleArrowGameObject.SetActive(false);
                break;
        }
    }
}
