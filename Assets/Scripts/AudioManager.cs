using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    [SerializeField]
    private Transform placeSfxPrefab;
    [SerializeField]
    private Transform winSfxPrefab;
    [SerializeField]
    private Transform loseSfxPrefab;

    private void Start() {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(GameManager.Line line, GameManager.PlayerType type) {
        if (GameManager.Instance.GetLocalPlayerType() == type) {
            Transform sfxTransform = Instantiate(winSfxPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        } else {
            Transform sfxTransform = Instantiate(loseSfxPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
    }

    private void GameManager_OnPlacedObject() {
        Transform sfxTransform = Instantiate(placeSfxPrefab);
        Destroy(sfxTransform.gameObject, 5f);
    }
}
