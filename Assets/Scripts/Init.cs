using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Init : MonoBehaviour
{
    public GameObject ringPrefab;
    public GameObject towerPrefab;
    public GameObject groundPrefab;

    public TMP_InputField ringInput;
    public TMP_InputField speedInput;
    public Toggle txtToggle;
    public TMP_Text ringWarningText;
    public TMP_Text errorText;
    public GameObject menuPanel;
    public GameObject HUDPanel;
    public GameObject infoPanel;
    public Camera mainCamera;
    public GameObject plane;

    public int ringLimit;
    public int towerLimit;
    public float ringMoveSpeed = 10f;

    private int ringNumber = 0;
    private float towerDistance = 0;
    private float xGroundSize = 0;
    private float ringHeight = 0.5f;
    private float towerHeight;
    private bool isSimulationStarted = false;
    private Vector3 cameraPosition;
    private bool isFinalResultFound = false;
    private Coroutine AnimateMovesRoutine;
    private bool isAnimationStarted = false;

    private GameObject ground;
    private List<GameObject> towers = new List<GameObject>();
    private List<GameObject> rings = new List<GameObject>();
    private List<Stack<GameObject>> towerList = new List<Stack<GameObject>>();
    private List<Move> moves = new List<Move>();
    private List<string> infoAboutMovesList = new List<string>();
    void Start()
    {
        if (ringLimit == 0) ringLimit = 20;
        ringWarningText.text = "(0 - " + ringLimit + ")";

        cameraPosition = mainCamera.transform.position;
    }

    void Update()
    {
        float scroll = Input.mouseScrollDelta.y;
        bool isKeyPadPlusDown = Input.GetKey(KeyCode.KeypadPlus);
        bool isKeyPadMinusDown = Input.GetKey(KeyCode.KeypadMinus);
        bool isKeyRDown = Input.GetKey(KeyCode.R);
        bool isKeyWDown = Input.GetKey(KeyCode.W);
        bool isKeySDown = Input.GetKey(KeyCode.S);
        bool isKeyADown = Input.GetKey(KeyCode.A);
        bool isKeyDDown = Input.GetKey(KeyCode.D);
        bool isKeySpaceDown = Input.GetKey(KeyCode.Space);

        //if (Input.anyKeyDown)
        //{
        //    foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
        //    {
        //        if (Input.GetKeyDown(k))
        //        {
        //            Debug.Log("Pressed: " + k);
        //        }
        //    }
        //}

        if (isKeySpaceDown && !isAnimationStarted)
        {
            SolveHanoi(ringNumber, 0, 2, 1);
            if (txtToggle.isOn) CreateTxtFileWithMoves();
            AnimateMovesRoutine = StartCoroutine(AnimateMoves());
        }

        bool isCameraMoving = false;

        Vector3 pos = mainCamera.transform.position;
        float xPos = pos.x;
        float yPos = pos.y;
        float zPos = pos.z;

        if (scroll != 0)
        {
            zPos = pos.z + scroll;
            isCameraMoving = true;
        }

        if (isKeyPadPlusDown)
        {
            zPos += 0.1f;
            isCameraMoving = true;
        }

        if (isKeyPadMinusDown)
        {
            zPos -= 0.1f;
            isCameraMoving = true;
        }

        if (isKeyRDown)
        {
            xPos = cameraPosition.x;
            yPos = cameraPosition.y;
            zPos = cameraPosition.z;
            isCameraMoving = true;
        }

        if (isKeyWDown)
        {
            yPos += 0.1f;
            isCameraMoving = true;
        }

        if (isKeySDown)
        {
            yPos -= 0.1f;
            isCameraMoving = true;
        }

        if (isKeyADown)
        {
            xPos -= 0.1f;
            isCameraMoving = true;
        }

        if (isKeyDDown)
        {
            xPos += 0.1f;
            isCameraMoving = true;
        }

        if (isCameraMoving)
        {
            if (zPos > plane.transform.position.z) zPos = plane.transform.position.z - 5;
            if (zPos < 775) zPos = 770;

            mainCamera.transform.position = new Vector3(xPos, yPos, zPos);
            //Debug.Log(xPos + ", " + yPos + ", " + zPos);
        }
    }

    public void CheckNumberOfRings()
    {
        if (isSimulationStarted)
        {
            //mainCamera.transform.position = cameraPosition;
            //DestroyObjects();
        }

        string invalidNumErr = "Invalid number!";
        string outOfBoundsNumErr = "Out of bounds number!";
        string greaterThanZeroErr = "Must be greater than Zero!";
        string ringsText = " (ring number)";
        string speedText = " (speed)";

        if (int.TryParse(ringInput.text, out int ringNumber))
        {
            this.ringNumber = ringNumber;
            if (ringNumber >= 0 && ringNumber <= ringLimit)
            {
                if (float.TryParse(speedInput.text, out float speed))
                {
                    if (speed > 0)
                    {
                        ringMoveSpeed = speed;
                        errorText.text = "";
                        isFinalResultFound = false;
                        moves.Clear();
                        StartSimOfHanoiTowers();
                    }
                    else errorText.text = greaterThanZeroErr + speedText;
                }
                else errorText.text = invalidNumErr + speedText;
            }
            else errorText.text = outOfBoundsNumErr + ringsText;
        }
        else errorText.text = invalidNumErr + ringsText;
    }

    public void StartSimOfHanoiTowers()
    {
        BuildBase();
        Debug.Log("Simulation started!");
        isSimulationStarted = true;
        isAnimationStarted = false;
        menuPanel.SetActive(false);
        HUDPanel.SetActive(true);
    }

    void BuildBase()
    {
        InitTowerList();

        towerDistance = ringNumber * 2 + 1;
        Vector3 groundPosition = new Vector3(0f, -3f, 1495f);

        Vector3 cameraPos = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z - ringNumber);

        SpawnGround(groundPosition);
        ResizeGround();
        ground.transform.position = groundPosition;

        PlaceTowers();
        PlaceRings();
    }

    void InitTowerList()
    {
        towerList.Clear();
        for (int i = 0; i < 3; i++)
        {
            towerList.Add(new Stack<GameObject>());
        }
    }

    void SpawnGround(Vector3 position)
    {
        ground = Instantiate(groundPrefab, position, Quaternion.identity);
    }

    void ResizeGround()
    {
        xGroundSize = towerDistance * towerLimit;
        ground.transform.localScale = new Vector3(xGroundSize, 1f, ringNumber + 1f);
    }

    void PlaceTowers()
    {
        float xLocationOfTower = ground.transform.position.x + towerDistance * 0.5f - xGroundSize * 0.5f;
        towerHeight = ringNumber * ringHeight;
        float yBuff = ringNumber * 0.25f + 0.5f;

        for (int i = 0; i < towerLimit; i++)
        {
            SpawnTower(xLocationOfTower, ground.transform.position.y + yBuff + towerHeight * 0.5f, ground.transform.position.z);
            ResizeTower(towers[i], towerHeight);
            xLocationOfTower += towerDistance;
        }
    }

    void SpawnTower(float x, float y, float z)
    {
        towers.Add(Instantiate(towerPrefab, new Vector3(x, y, z), Quaternion.identity));
    }

    void ResizeTower(GameObject tower, float ySize)
    {
        tower.transform.localScale = new Vector3(0.5f, ySize, 0.5f);
    }

    void PlaceRings()
    {
        float yLocationOfRing = ground.transform.position.y + 0.75f;

        for (int i = 0; i < ringNumber; i++)
        {
            SpawnRing(towers[0].transform.position.x, yLocationOfRing, ground.transform.position.z);
            ResizeRing(rings[i], ringNumber - i);
            ColorRing(rings[i]);
            yLocationOfRing += ringHeight;
        }
    }

    void SpawnRing(float x, float y, float z)
    {
        GameObject ring = Instantiate(ringPrefab, new Vector3(x, y, z), Quaternion.identity);
        rings.Add(ring);
        towerList[0].Push(ring);
    }

    void ResizeRing(GameObject ring, float x_zSize)
    {
        ring.transform.localScale = new Vector3(x_zSize, 0.25f, x_zSize);
    }

    void ColorRing(GameObject ring)
    {
        Renderer r = ring.GetComponent<Renderer>();
        if (r != null) r.material.color = new Color(Random.value, Random.value, Random.value);
    }

    void DestroyObjects()
    {
        Destroy(ground);

        foreach (GameObject obj in towers) Destroy(obj);
        towers.Clear();

        foreach (GameObject obj in rings) Destroy(obj);
        rings.Clear();

        foreach (GameObject obj in rings) Destroy(obj);
        rings.Clear();

        foreach (Stack<GameObject> tower in towerList)
        {
            foreach (GameObject ring in tower) Destroy(ring);
            tower.Clear();
        }

        infoAboutMovesList.Clear();
    }

    public void GoToMenu()
    {
        HUDPanel.SetActive(false);
        infoPanel.SetActive(false);
        menuPanel.SetActive(true);
        StopCoroutine(AnimateMovesRoutine);
        mainCamera.transform.position = cameraPosition;
        DestroyObjects();
    }

    public void GoToInfo()
    {
        infoPanel.SetActive(true);
    }

    public void GoToHUDFromInfo()
    {
        infoPanel.SetActive(false);
    }

    void SolveHanoi(int n, int from, int to, int aux)
    {
        if (n == 0) return;
        if (isFinalResultFound) return;

        SolveHanoi(n - 1, from, aux, to);  // Move n-1 rings to auxiliary
        CreateMove(from, to);                // Move the largest ring to target
        SolveHanoi(n - 1, aux, to, from);  // Move n-1 rings from auxiliary to target
    }

    void CreateMove(int from, int to)
    {
        if (towerList[from].Count == 0)
        {
            //Debug.LogError("No ring to move!");
            isFinalResultFound = true;
            return;
        }

        GameObject ring = towerList[from].Pop();
        float whichRing = ring.transform.localScale.x;
        towerList[to].Push(ring);

        // Update ring position
        //ring.transform.position = GetRingPosition(to, towerList[to].Count - 1);
        //Debug.Log($"Move ring {whichRing} from Tower {from} to Tower {to}");

        moves.Add(new Move(ring, GetRingPosition(to, towerList[to].Count - 1)));
        infoAboutMovesList.Add($"Ring {whichRing} moved from Tower {from} to Tower {to}");
    }

    Vector3 GetRingPosition(int towerIndex, int ringIndex)
    {
        float yLocationOfRing = ground.transform.position.y + 0.75f;
        Transform tower = towers[towerIndex].transform;
        float y = yLocationOfRing + ringHeight * ringIndex;
        // 0.5f / 2 + (0.5f + 'ringNumber = 2' * 2 + 1) * '1' = 5.75
        return new Vector3(tower.position.x, y, tower.position.z);
    }
    // you have to make the transition not only final result

    GameObject FindTowerFromX(float x)
    {
        foreach (GameObject tower in towers)
        {
            float towerX = tower.transform.position.x;
            if (towerX + 1 >= x && towerX - 1 <= x) return tower;
        }
        return null;
    }

    IEnumerator AnimateMoves()
    {
        isAnimationStarted = true;
        foreach (Move move in moves)
        {
            GameObject ring = move.Ring;
            Vector3 startPos = ring.transform.position;
            GameObject tower = FindTowerFromX(startPos.x);
            float liftHeight = tower.transform.position.y + towerHeight * 0.5f + 3 - startPos.y;
            Vector3 aboveStart = startPos + Vector3.up * liftHeight;
            Vector3 aboveTarget = move.FuturePosition + Vector3.up * liftHeight;

            yield return StartCoroutine(MoveObject(ring, startPos, aboveStart));
            yield return StartCoroutine(MoveObject(ring, aboveStart, aboveTarget));
            yield return StartCoroutine(MoveObject(ring, aboveTarget, move.FuturePosition));
        }
    }

    IEnumerator MoveObject(GameObject obj, Vector3 from, Vector3 to)
    {
        float moveSpeed = ringMoveSpeed + ringNumber * 1.5f;
        float distance = Vector3.Distance(from, to);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = to;  // ensure exact position
    }

    public void CreateTxtFileWithMoves()
    {
        string filePath = Application.dataPath + "/Moves.txt"; // path in Assets folder

        using (StreamWriter writer = new StreamWriter(filePath, false)) // false = overwrite
        {
            writer.WriteLine("Number of moves : " + infoAboutMovesList.Count);
            foreach (string move in infoAboutMovesList)
            {
                writer.WriteLine(move);
            }
        }

        Debug.Log("File written to: " + filePath);
    }

    public void ExitClicked()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}