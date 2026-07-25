using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Rat_P
{
    public class RatPSystem : Singleton<RatPSystem>
    {
        [Header("UI References")] 
        [SerializeField] private Transform contentParent; // The Canvas / Panel template

        private Slider _timerSlider;
        [SerializeField] private GameObject buttonPrefab; // Your Button UI Prefab
        [SerializeField] private GameObject wirePrefab;

        [Header("Grid Config")] 
        [SerializeField] private int rows = 3; // Configurable Rows (3x3 = 9 buttons)
        [SerializeField] private int cols = 3; // Configurable Columns
        [SerializeField] private float buttonSize = 50f; // Width & Height of each button
        [SerializeField] private float spacing = 10f; // Gap between buttons

        private List<List<GameObject>> grid = new List<List<GameObject>>();
        private GameObject currentPanelInstance;
        private List<ButtonModifier> buttonModifiers = new List<ButtonModifier>();

        [Header("Setup Params")]
        [SerializeField] private List<ButtonIconData> _buttonIconDataList = new List<ButtonIconData>();

        private float minigame_timelimit = 30f; // seconds
        private float elapsedTime = 0f;

        private bool bIsBacktracking = false; 
        public bool IsBacktracking() { return bIsBacktracking; } 
        public void SetBacktracking(bool value) { bIsBacktracking = value; }

        public int CurrentButtonInd = 0; 
        public int GetCurrentButtonInd() { return CurrentButtonInd; }

        
        private bool _isWarningPlaying = false;
        private bool _isIntensePlaying = false;
        
        public void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ToggleRatP();
            }
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                GridBacktrack();
            }
    
            // Check music state using time remaining percentage (works whether UI is open or closed)
            float timeRemainingPercent = 1f - (elapsedTime / minigame_timelimit);

            // Warning Alarm (< 50% time)
            if (timeRemainingPercent < 0.5f && !_isWarningPlaying)
            {
                _isWarningPlaying = true;
                UAudio.Instance.PlayMusic(UAudio.Instance.RATP_WarningAlarmMusic);
            }
            else if (timeRemainingPercent >= 0.5f && _isWarningPlaying)
            {
                _isWarningPlaying = false;
                UAudio.Instance.StopMusic(UAudio.Instance.RATP_WarningAlarmMusic);
            }

            // Intense Music (< 15% time)
            if (timeRemainingPercent < 0.15f && !_isIntensePlaying)
            {
                _isIntensePlaying = true;
                UAudio.Instance.PlayMusic(UAudio.Instance.RatP_IntenseMusic);
            }
            else if (timeRemainingPercent >= 0.15f && _isIntensePlaying)
            {
                _isIntensePlaying = false;
                UAudio.Instance.StopMusic(UAudio.Instance.RatP_IntenseMusic);
            }
        }

        public void Start()
        {
            if (contentParent)
            {
                currentPanelInstance = Instantiate(contentParent.gameObject, contentParent.parent);
                 

                // Direct search for Slider named "Timer_Slider" among inactive children
                Slider[] sliders = currentPanelInstance.GetComponentsInChildren<Slider>(true);
                foreach (Slider s in sliders)
                {
                    if (s.gameObject.name == "Timer_Slider")
                    {
                        _timerSlider = s;
                        break;
                    }
                }

                if (_timerSlider != null)
                {
                    StartCoroutine(UpdateTimerSlider());
                }
                
                // at the very end, we diable
                currentPanelInstance.SetActive(false);
            }
        }

        public void SetCurrentButtonInd(int index, bool resetIfExceeds = true)
        {
            CurrentButtonInd = index;
            

            
            if (resetIfExceeds && CurrentButtonInd >= buttonModifiers.Count)
            {
                Debug.Log("CurrentButtonInd exceeded buttonModifiers count. Resetting to 0.");
                ResetMinigame();
            }
        }
        
        private void ResetMinigame()
        {
            SetBacktracking(true);
            print("Resetting minigame");
            CurrentButtonInd = 0;
            SetCurrentButtonInd(0, false);
            // we dont wanna reset the timer, instead we wanna make it feel like we gained 25% of the total time, clamped to the max time limit
            elapsedTime = Mathf.Max(0f, elapsedTime - minigame_timelimit * 0.25f);
            PopulateGrid(currentPanelInstance.transform);
            
            // There is a bug here, where after we reset the minigame, it takes in the last input from the player
            // we will add a VERY slight delay before we allow the player to input again, to avoid this issue
            StartCoroutine(ResetInputDelayCoroutine());
        }
        IEnumerator ResetInputDelayCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            SetBacktracking(false);
        }

        public void ToggleRatP()
        {
            if (currentPanelInstance.activeSelf)
            {
                UAudio.Instance.StopMusic_RATP_BacktrackMusic();
                CloseRatP();
            }
            else
            {
                UAudio.Instance.PlayMusic_RATP_BacktrackMusic();
                OpenRatP();
            }
        }

        public void OpenRatP()
        {
            CurrentButtonInd = 0;
            print("Opening RatP");
            if (contentParent == null)
            {
                Debug.LogError("RatPSystem: contentParent is not assigned in the Inspector!");
                return;
            }

            currentPanelInstance.SetActive(true); // Ensure spawned instance is visible

            // Immediately catch up slider visuals to current timer state
            if (_timerSlider != null)
            {
                _timerSlider.value = Mathf.Clamp01(1f - (elapsedTime / minigame_timelimit));
            }

            PopulateGrid(currentPanelInstance.transform);
        }

        private IEnumerator UpdateTimerSlider()
        {
            // Don't reset elapsedTime to 0 here if you want a continuous background timer!
            while (elapsedTime < minigame_timelimit)
            {
                elapsedTime += Time.deltaTime;

                // ONLY update UI components when active in hierarchy to prevent TLS memory errors
                if (currentPanelInstance != null && currentPanelInstance.activeInHierarchy && _timerSlider != null)
                {
                    _timerSlider.value = 1f - (elapsedTime / minigame_timelimit);
                    // clamp
                    _timerSlider.value = Mathf.Clamp01(_timerSlider.value);
                }

                yield return null;
            }

            // Time's up logic here
        }

        public void CloseRatP()
        {
            print("Closing RatP");

            // Clean up old button instances from memory/scene
            ClearGrid();

            if (currentPanelInstance != null)
            {
                currentPanelInstance.SetActive(false);
            }
        }

        private void ClearGrid()
        {
            for (int r = 0; r < grid.Count; r++)
            {
                for (int c = 0; c < grid[r].Count; c++)
                {
                    if (grid[r][c] != null)
                    {
                        Destroy(grid[r][c]);
                    }
                }
            }
            grid.Clear();
            buttonModifiers.Clear();
        }

        public void PopulateGrid(Transform spawnedPanel)
        {
            // Clear existing grid first to avoid duplicate overlapping buttons
            ClearGrid();

            Transform startPoint = null;
            Transform[] allTransforms = spawnedPanel.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allTransforms)
            {
                if (t.name == "StartPoint")
                {
                    startPoint = t;
                    break;
                }
            }

            if (startPoint == null)
            {
                Debug.LogError($"RatPSystem: 'StartPoint' child object was not found inside {spawnedPanel.name}!");
                return;
            }

            Transform basePrefabTransform = buttonPrefab.transform.Find("Button_Base");
            if (basePrefabTransform == null)
            {
                Debug.LogError("RatPSystem: Could not find child 'Button_Base' inside buttonPrefab!");
                return;
            }

            RectTransform sampleBaseRT = basePrefabTransform.GetComponent<RectTransform>();
            float width = sampleBaseRT.rect.width;
            float height = sampleBaseRT.rect.height;

            for (int r = 0; r < rows; r++)
            {
                List<GameObject> rowList = new List<GameObject>();
                for (int c = 0; c < cols; c++)
                {
                    GameObject buttonInstance = Instantiate(buttonPrefab, startPoint, false);

                    ButtonModifier modifier = buttonInstance.GetComponent<ButtonModifier>();
                    if (modifier == null)
                    {
                        modifier = buttonInstance.GetComponentInChildren<ButtonModifier>();
                    }

                    if (modifier != null)
                    {
                        // We will just fill it with the NON from a random icon data, since we will generate the path later and assign the correct icon data to the buttons that are part of the path
                        ButtonIconData randomIconData = _buttonIconDataList[UnityEngine.Random.Range(0, _buttonIconDataList.Count)];
                        ButtonIconData iconData = new ButtonIconData(ButtonOption.NONE, randomIconData.GetButtonIconSprite(), randomIconData.GetButtonSpriteNone(), randomIconData.GetButtonSpriteNonActivated(), randomIconData.GetButtonSpriteActivated());
                        modifier.SetButtonIconData(iconData);
                        modifier.UpdateButtonVisuals();
                    }

                    RectTransform rootRT = buttonInstance.GetComponent<RectTransform>();
                    rootRT.localScale = Vector3.one;
                    rootRT.anchorMin = new Vector2(0, 1);
                    rootRT.anchorMax = new Vector2(0, 1);
                    rootRT.pivot = new Vector2(0, 1);

                    float xPos = c * (width + spacing);
                    float yPos = -r * (height + spacing);
                    rootRT.anchoredPosition = new Vector2(xPos, yPos);

                    rowList.Add(buttonInstance);
                }

                grid.Add(rowList);
            }

            GeneratePath();
        }

        public void GridBacktrack()
        {
            SetBacktracking(true);
            UAudio.Instance.PlayRATP_PlayGridBacktrackSound();
            StartCoroutine(BacktrackCoroutine());
        }

        IEnumerator BacktrackCoroutine()
        {
            yield return new WaitForSeconds(1f);

            for (int i = GetCurrentButtonInd() - 1; i >= 0; i--)
            {
                if (i < buttonModifiers.Count && buttonModifiers[i] != null)
                {
                    ButtonModifier modifier = buttonModifiers[i];
                    UAudio.Instance.PlayRATP_ButtonFailSound();
                    modifier.SetButtonState(ButtonActivityState.NonActivated);
                    yield return new WaitForSeconds(0.2f);
                }
            }

            SetCurrentButtonInd(0, false);
            SetBacktracking(false);
        }

        private void GeneratePath()
        {
            SetCurrentButtonInd(0, false);

            for (int r = 0; r < grid.Count; r++)
            {
                for (int c = 0; c < grid[r].Count; c++)
                {
                    GameObject buttonObj = grid[r][c];
                    ButtonModifier modifier = buttonObj.GetComponent<ButtonModifier>();
                    if (modifier == null)
                    {
                        modifier = buttonObj.GetComponentInChildren<ButtonModifier>();
                    }

                    if (modifier != null)
                    {
                        modifier.IsButtonApartOfThePath = false;
                        modifier.SetButtonState(ButtonActivityState.NonSelected);
                        modifier.SetButtonIndex(-1);
                        modifier.SetWiresVisible(new WiresVisible());
                        modifier.UpdateButtonVisuals();
                    }
                }
            }

            buttonModifiers.Clear();

            if (grid == null || grid.Count == 0 || grid[0].Count == 0)
            {
                Debug.LogWarning("RatPSystem: Grid is not initialized yet!");
                return;
            }

            int totalRows = grid.Count;
            int totalCols = grid[0].Count;
            int currentButtonIndex = 0;

            bool[,] visited = new bool[totalRows, totalCols];
            List<(int row, int col)> path = new List<(int row, int col)>();

            int startRow = UnityEngine.Random.Range(0, totalRows);

            if (FindPathDFS(startRow, 0, totalRows, totalCols, visited, path))
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var (r, c) = path[i];
                    GameObject buttonObj = grid[r][c];

                    ButtonModifier modifier = buttonObj.GetComponent<ButtonModifier>();
                    if (modifier == null)
                    {
                        modifier = buttonObj.GetComponentInChildren<ButtonModifier>();
                    }

                    if (modifier != null)
                    {
                        modifier.IsButtonApartOfThePath = true;
                        modifier.SetButtonState(ButtonActivityState.NonActivated);
                        modifier.SetButtonIndex(currentButtonIndex);

                        if (_buttonIconDataList != null && _buttonIconDataList.Count > 0)
                        {
                            ButtonIconData iconData = _buttonIconDataList[UnityEngine.Random.Range(0, _buttonIconDataList.Count)];
                            modifier.SetButtonIconData(iconData);
                        }

                        modifier.UpdateButtonVisuals();
                        buttonModifiers.Add(modifier);

                        WiresVisible wires = new WiresVisible();

                        if (i == 0)
                        {
                            wires.LeftWireVisible = true;
                        }
                        else
                        {
                            var (prevR, prevC) = path[i - 1];
                            int dr = prevR - r;
                            int dc = prevC - c;

                            if (dc < 0) wires.LeftWireVisible = true;
                            if (dr < 0) wires.TopWireVisible = true;
                            if (dr > 0) wires.BottomWireVisible = true;
                        }

                        if (i == path.Count - 1)
                        {
                            wires.RightWireVisible = true;
                        }
                        else
                        {
                            var (nextR, nextC) = path[i + 1];
                            int dr = nextR - r;
                            int dc = nextC - c;

                            if (dc > 0) wires.RightWireVisible = true;
                            if (dr < 0) wires.TopWireVisible = true;
                            if (dr > 0) wires.BottomWireVisible = true;
                        }

                        modifier.SetWiresVisible(wires);
                    }
                    currentButtonIndex++;
                }

                Debug.Log($"Path generated successfully! Total steps: {path.Count}");
            }
            else
            {
                Debug.LogError("Failed to generate a valid path to the final column.");
            }
        }

        private bool FindPathDFS(int r, int c, int maxRows, int maxCols, bool[,] visited, List<(int, int)> path)
        {
            if (r < 0 || r >= maxRows || c < 0 || c >= maxCols)
                return false;

            if (visited[r, c])
                return false;

            visited[r, c] = true;
            path.Add((r, c));

            if (c == maxCols - 1)
                return true;

            List<(int dr, int dc)> directions = new List<(int dr, int dc)>
            {
                (0, 1),
                (-1, 0),
                (1, 0)
            };

            ShuffleList(directions);

            foreach (var (dr, dc) in directions)
            {
                int nextRow = r + dr;
                int nextCol = c + dc;

                if (FindPathDFS(nextRow, nextCol, maxRows, maxCols, visited, path))
                    return true;
            }

            visited[r, c] = false;
            path.RemoveAt(path.Count - 1);

            return false;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = UnityEngine.Random.Range(0, i + 1);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }
        }
    }
}