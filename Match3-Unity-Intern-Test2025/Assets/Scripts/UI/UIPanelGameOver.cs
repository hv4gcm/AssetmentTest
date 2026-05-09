using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnClose.onClick.AddListener(OnClickClose);
    }

    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
    }

    private void OnClickClose()
    {
        m_mngr.ShowMainMenu();
    }

    public void SetWinStatus(bool isWin)
    {
        Text[] texts = GetComponentsInChildren<Text>(true);
        foreach(var t in texts)
        {
            if (t.text == "LEVEL WIN" || t.text == "LEVEL FAILED" || t.text == "GAME OVER")
            {
                t.text = isWin ? "LEVEL WIN" : "LEVEL FAILED";
            }
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

}
