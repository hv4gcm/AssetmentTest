using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private Button btnAutoWin;
    [SerializeField] private Button btnAutoLose;
    [SerializeField] private Button btnTimeAttack;

    private UIMainManager m_mngr;

    private void Awake()
    {
        if (btnMoves) btnMoves.onClick.AddListener(OnClickMoves);
        if (btnTimer) btnTimer.onClick.AddListener(OnClickTimer);

        if (btnAutoWin)
        {
            btnAutoWin.onClick.AddListener(() => OnClickAuto(GameManager.eAutoPlayMode.AutoWin));
        }
        if (btnAutoLose)
        {
            btnAutoLose.onClick.AddListener(() => OnClickAuto(GameManager.eAutoPlayMode.AutoLose));
        }
        if (btnTimeAttack)
        {
            btnTimeAttack.onClick.AddListener(OnClickTimeAttack);
        }
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickTimer()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickTimeAttack()
    {
        m_mngr.LoadLevelTimeAttack();
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevelMoves();
    }

    private void OnClickAuto(GameManager.eAutoPlayMode mode)
    {
        m_mngr.LoadLevelAutoPlay(mode);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
