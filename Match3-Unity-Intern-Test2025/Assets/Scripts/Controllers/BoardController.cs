using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;
    private bool m_gameWin;


    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
        // FindMatchesAndCollapse(); // Tile Go doesn't auto collapse on start

        if (m_gameManager.AutoPlayMode != GameManager.eAutoPlayMode.None)
        {
            StartCoroutine(CoroutineAutoPlay());
        }
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;  
                // StopHints();
                break;
            case GameManager.eStateGame.GAME_WIN:
                m_gameWin = true;
                break;
        }
    }

    private List<Item> m_slotItems = new List<Item>();
    private GameObject m_slotBackground;

    public void Update()
    {
        if (m_gameOver || m_gameWin) return;
        if (IsBusy) return;
        if (m_gameManager.AutoPlayMode != GameManager.eAutoPlayMode.None) return; // Không cho click khi AutoPlay

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = m_cam.ScreenToWorldPoint(Input.mousePosition);

            // Kiểm tra click vào Slot Bar (trả cá về bảng) ở chế độ Time Attack
            if (m_gameManager.IsTimeAttackMode && mousePos.y > -6.0f && mousePos.y < -4.0f)
            {
                for (int i = 0; i < m_slotItems.Count; i++)
                {
                    Vector3 itemPos = new Vector3(-2f + i, -5f, 0);
                    if (Vector2.Distance(mousePos, itemPos) < 0.6f)
                    {
                        ReturnItemToBoard(m_slotItems[i]);
                        return; // Xử lý xong, không check raycast bảng nữa
                    }
                }
            }

            // Nếu slot bar đã đầy (5 item) thì không cho bấm trên bảng nữa
            if (m_slotItems.Count >= 5) return;

            var hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && !cell.IsEmpty && cell.Item != null && !m_slotItems.Contains(cell.Item))
                {
                    MoveItemToSlotBar(cell);
                }
            }
        }
    }

    private void ReturnItemToBoard(Item item)
    {
        if (item.OriginalCell == null) return;

        m_slotItems.Remove(item);
        Cell cell = item.OriginalCell;
        cell.Assign(item);
        
        item.View.transform.DOKill();
        item.View.transform.DOJump(cell.transform.position, 1.5f, 1, 0.3f);
        
        UpdateSlotBarVisuals();
    }

    private void MoveItemToSlotBar(Cell cell)
    {
        if (m_slotItems.Count >= 5) return; // Slot bar is full

        Item item = cell.Item;
        item.OriginalCell = cell; // Lưu lại vị trí cũ
        cell.Free();

        m_slotItems.Add(item);

        float yPos = -5f; // Below the board
        
        // Let's create slot background if not exists
        if (m_slotBackground == null)
        {
            CreateSlotBackground(yPos);
        }

        UpdateSlotBarVisuals();

        if (!CheckMatchesInSlotBar())
        {
            CheckWinLossCondition();
        }
    }

    private void CreateSlotBackground(float yPos)
    {
        m_slotBackground = new GameObject("SlotBackground");
        m_slotBackground.transform.position = new Vector3(0, yPos, 0);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for(int i = 0; i < 5; i++)
        {
            GameObject bg = Instantiate(prefabBG, m_slotBackground.transform);
            bg.transform.localPosition = new Vector3(-2f + i, 0, 0);
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if(sr != null) sr.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
    }

    private void UpdateSlotBarVisuals()
    {
        float yPos = -5f;
        for (int i = 0; i < m_slotItems.Count; i++)
        {
            Vector3 targetPos = new Vector3(-2f + i, yPos, 0);
            
            // Nếu khoảng cách xa (mới rớt từ bảng xuống), nhảy DOJump
            if (Vector3.Distance(m_slotItems[i].View.transform.position, targetPos) > 0.1f)
            {
                m_slotItems[i].View.transform.DOKill();
                m_slotItems[i].View.transform.DOJump(targetPos, 1.5f, 1, 0.3f);
            }
            else
            {
                // Nếu chỉ đang trượt ngang trên thanh slot bar
                m_slotItems[i].View.transform.DOMove(targetPos, 0.2f);
            }

            m_slotItems[i].SetSortingLayerHigher();
        }
    }

    private bool CheckMatchesInSlotBar()
    {
        Dictionary<string, List<Item>> typeCount = new Dictionary<string, List<Item>>();
        
        foreach(var item in m_slotItems)
        {
            if (item is NormalItem normalItem)
            {
                string t = normalItem.ItemType.ToString();
                if (!typeCount.ContainsKey(t)) typeCount[t] = new List<Item>();
                typeCount[t].Add(normalItem);
            }
        }

        foreach(var kvp in typeCount)
        {
            if (kvp.Value.Count >= 3)
            {
                // Lấy vị trí của item thứ 2 (nằm giữa) để làm điểm tụ lại
                Vector3 centerPos = kvp.Value[1].View.transform.position;

                for(int i = 0; i < 3; i++)
                {
                    Item toRemove = kvp.Value[i];
                    m_slotItems.Remove(toRemove);
                    
                    toRemove.View.transform.DOKill();
                    // Di chuyển gom lại giữa và phóng to nhẹ
                    toRemove.View.transform.DOMove(centerPos, 0.2f).SetEase(Ease.InBack);
                    toRemove.View.transform.DOScale(1.3f, 0.15f).OnComplete(() =>
                    {
                        // Thu nhỏ và biến mất
                        toRemove.View.transform.DOScale(0f, 0.15f).OnComplete(() =>
                        {
                            Destroy(toRemove.View.gameObject);
                        });
                    });
                }
                
                DOVirtual.DelayedCall(0.35f, () => {
                    UpdateSlotBarVisuals();
                    CheckWinLossCondition();
                });

                return true;
            }
        }
        
        return false;
    }

    private void CheckWinLossCondition()
    {
        if (m_board.IsBoardEmpty() && m_slotItems.Count == 0)
        {
            m_gameWin = true;
            m_gameManager.GameWin();
        }
        else if (m_slotItems.Count >= 5)
        {
            // Trong chế độ Time Attack, không thua khi đầy thanh
            if (!m_gameManager.IsTimeAttackMode)
            {
                m_gameOver = true;
                m_gameManager.GameOver();
            }
        }
    }

    private IEnumerator CoroutineAutoPlay()
    {
        yield return new WaitForSeconds(1f); // Đợi fill xong animation ban đầu
        
        while (!m_gameOver && !m_gameWin)
        {
            yield return new WaitForSeconds(0.5f);

            if (m_gameOver || m_gameWin) break;

            List<Cell> filledCells = m_board.GetAllFilledCells();
            if (filledCells.Count == 0) break;

            Cell targetCell = null;

            if (m_gameManager.AutoPlayMode == GameManager.eAutoPlayMode.AutoWin)
            {
                // Auto Win logic
                List<NormalItem.eNormalType> slotTypes = new List<NormalItem.eNormalType>();
                foreach (var item in m_slotItems)
                {
                    if (item is NormalItem ni) slotTypes.Add(ni.ItemType);
                }

                if (slotTypes.Count > 0)
                {
                    NormalItem.eNormalType targetType = slotTypes[0];
                    foreach (var cell in filledCells)
                    {
                        if (cell.Item is NormalItem ni && ni.ItemType == targetType)
                        {
                            targetCell = cell;
                            break;
                        }
                    }
                }

                if (targetCell == null)
                {
                    targetCell = filledCells[0];
                }
            }
            else if (m_gameManager.AutoPlayMode == GameManager.eAutoPlayMode.AutoLose)
            {
                // Auto Lose logic
                Dictionary<NormalItem.eNormalType, int> slotCounts = new Dictionary<NormalItem.eNormalType, int>();
                foreach (var item in m_slotItems)
                {
                    if (item is NormalItem ni)
                    {
                        if (slotCounts.ContainsKey(ni.ItemType)) slotCounts[ni.ItemType]++;
                        else slotCounts[ni.ItemType] = 1;
                    }
                }

                foreach (var cell in filledCells)
                {
                    if (cell.Item is NormalItem ni)
                    {
                        int count = slotCounts.ContainsKey(ni.ItemType) ? slotCounts[ni.ItemType] : 0;
                        if (count < 2) 
                        {
                            targetCell = cell;
                            break;
                        }
                    }
                }

                if (targetCell == null)
                {
                    targetCell = filledCells[0];
                }
            }

            if (targetCell != null)
            {
                MoveItemToSlotBar(targetCell);
            }
        }
    }

    internal void Clear()
    {
        m_board.Clear();
    }

/* === OLD CODE ===
    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (!m_hintIsShown)
        {
            m_timeAfterFill += Time.deltaTime;
            if (m_timeAfterFill > m_gameSettings.TimeForHint)
            {
                m_timeAfterFill = 0f;
                ShowHint();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                m_isDragging = true;
                m_hitCollider = hit.collider;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetRayCast();
        }

        if (Input.GetMouseButton(0) && m_isDragging)
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                if (m_hitCollider != null && m_hitCollider != hit.collider)
                {
                    StopHints();

                    Cell c1 = m_hitCollider.GetComponent<Cell>();
                    Cell c2 = hit.collider.GetComponent<Cell>();
                    if (AreItemsNeighbor(c1, c2))
                    {
                        IsBusy = true;
                        SetSortingLayer(c1, c2);
                        m_board.Swap(c1, c2, () =>
                        {
                            FindMatchesAndCollapse(c1, c2);
                        });

                        ResetRayCast();
                    }
                }
            }
            else
            {
                ResetRayCast();
            }
        }
    }

    private void ResetRayCast()
    {
        m_isDragging = false;
        m_hitCollider = null;
    }

    private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    {
        if (cell1.Item is BonusItem)
        {
            cell1.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else if (cell2.Item is BonusItem)
        {
            cell2.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else
        {
            List<Cell> cells1 = GetMatches(cell1);
            List<Cell> cells2 = GetMatches(cell2);

            List<Cell> matches = new List<Cell>();
            matches.AddRange(cells1);
            matches.AddRange(cells2);
            matches = matches.Distinct().ToList();

            if (matches.Count < m_gameSettings.MatchesMin)
            {
                m_board.Swap(cell1, cell2, () =>
                {
                    IsBusy = false;
                });
            }
            else
            {
                OnMoveEvent();

                CollapseMatches(matches, cell2);
            }
        }
    }

    private void FindMatchesAndCollapse()
    {
        List<Cell> matches = m_board.FindFirstMatch();

        if (matches.Count > 0)
        {
            CollapseMatches(matches, null);
        }
        else
        {
            m_potentialMatch = m_board.GetPotentialMatches();
            if (m_potentialMatch.Count > 0)
            {
                IsBusy = false;

                m_timeAfterFill = 0f;
            }
            else
            {
                StartCoroutine(ShuffleBoardCoroutine());
            }
        }
    }

    private List<Cell> GetMatches(Cell cell)
    {
        List<Cell> listHor = m_board.GetHorizontalMatches(cell);
        if (listHor.Count < m_gameSettings.MatchesMin)
        {
            listHor.Clear();
        }

        List<Cell> listVert = m_board.GetVerticalMatches(cell);
        if (listVert.Count < m_gameSettings.MatchesMin)
        {
            listVert.Clear();
        }

        return listHor.Concat(listVert).Distinct().ToList();
    }

    private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if(matches.Count > m_gameSettings.MatchesMin)
        {
            m_board.ConvertNormalToBonus(matches, cellEnd);
        }

        StartCoroutine(ShiftDownItemsCoroutine());
    }

    private IEnumerator ShiftDownItemsCoroutine()
    {
        m_board.ShiftDownItems();

        yield return new WaitForSeconds(0.2f);

        m_board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    private IEnumerator RefillBoardCoroutine()
    {
        m_board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        m_board.Fill();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        m_board.Shuffle();

        yield return new WaitForSeconds(0.3f);

        FindMatchesAndCollapse();
    }


    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    private void ShowHint()
    {
        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        foreach (var cell in m_potentialMatch)
        {
            cell.StopHintAnimation();
        }

        m_potentialMatch.Clear();
    }
=== OLD CODE === */
}
