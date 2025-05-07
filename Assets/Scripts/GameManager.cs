using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {

    public static GameManager Instance { get; private set; }

    public event Action<int, int, PlayerType> OnGridPositionClicked;
    public event Action OnGameStarted;
    public event Action OnCurrentPlayablePlayerTypeChanged;
    public event Action<Line, PlayerType> OnGameWin;
    public event Action OnGameRematch;
    public event Action OnGameDraw;
    public event Action OnScoreChanged;
    public event Action OnPlacedObject;

    public enum Orientation {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }

    public struct Line {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    public enum PlayerType {
        None,
        Cross,
        Circle
    }

    private PlayerType localPlayerType;
    private PlayerType[,] playerTypeArray;
    private List<Line> linesList;

    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        playerTypeArray = new PlayerType[3, 3];
        linesList = new List<Line> {
            // Horizontal lines
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },
            // Vertical lines
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },
            // Diagonal lines
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA
            },
            new Line {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB
            }
        };
    }

    public override void OnNetworkSpawn() {
        switch (NetworkManager.Singleton.LocalClientId) {
            case 0:
                localPlayerType = PlayerType.Cross;
                break;
            case 1:
                localPlayerType = PlayerType.Circle;
                break;
            default:
                localPlayerType = PlayerType.None;
                break;
        }
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType previousValue, PlayerType newValue) => {
            OnCurrentPlayablePlayerTypeChanged?.Invoke();
        };

        playerCrossScore.OnValueChanged += (int previousValue, int newValue) => {
            OnScoreChanged?.Invoke();
        };
        playerCircleScore.OnValueChanged += (int previousValue, int newValue) => {
            OnScoreChanged?.Invoke();
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj) {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2) {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc() {
        OnGameStarted?.Invoke();
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType) {
        if (playerType != currentPlayablePlayerType.Value) {
            Debug.Log("Not your turn");
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None) {
            return;
        }

        playerTypeArray[x, y] = playerType;

        OnGridPositionClicked?.Invoke(x, y, playerType);
        TriggerOnPlaceObjectRpc();
        switch (currentPlayablePlayerType.Value) {
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
            default:
                Debug.LogError("Invalid player type");
                break;
        }

        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlaceObjectRpc() {
        OnPlacedObject?.Invoke();
    }

    private bool TestWinnerLine(Line line) {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            );
    }

    private bool TestWinnerLine(PlayerType playerType1, PlayerType playerType2, PlayerType playerType3) {
        return
            playerType1 != PlayerType.None &&
            playerType1 == playerType2 &&
            playerType2 == playerType3;
    }

    private void TestWinner() {
        for (int i = 0; i < linesList.Count; i++) {
            Line line = linesList[i];
            if (TestWinnerLine(line)) {
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType = playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];
                switch (winPlayerType) {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;
                    case PlayerType.Circle:
                        playerCircleScore.Value++;
                        break;
                }
                TriggerOnGameWinRpc(i, winPlayerType);
                return;
            }
        }

        bool isDraw = true;
        for (int i = 0; i < playerTypeArray.GetLength(0); i++) {
            for (int j = 0; j < playerTypeArray.GetLength(1); j++) {
                if (playerTypeArray[i, j] == PlayerType.None) {
                    isDraw = false;
                    break;
                }
            }
        }

        if (isDraw) {
            currentPlayablePlayerType.Value = PlayerType.None;
            TriggerOnGameDrawRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameDrawRpc() {
        OnGameDraw?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType) {
        Line line = linesList[lineIndex];
        OnGameWin?.Invoke(line, winPlayerType);
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc() {
        for (int i = 0; i < playerTypeArray.GetLength(0); i++) {
            for (int j = 0; j < playerTypeArray.GetLength(1); j++) {
                playerTypeArray[i, j] = PlayerType.None;
            }
        }
        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnGameRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerOnGameRematchRpc() {
        OnGameRematch?.Invoke();
    }


    public PlayerType GetLocalPlayerType() {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType() {
        return currentPlayablePlayerType.Value;
    }

    public void GetScores(out int crossScore, out int circleScore) {
        crossScore = playerCrossScore.Value;
        circleScore = playerCircleScore.Value;
    }
}
