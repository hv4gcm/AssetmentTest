using UnityEngine;
using UnityEngine.UI;

public class UIPanelWin : MonoBehaviour, IMenu
{
    private UIMainManager m_mngr;

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
        // Tự động tìm Button đóng nếu có
        Button btn = GetComponentInChildren<Button>();
        if(btn != null)
        {
            btn.onClick.AddListener(OnClickClose);
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void OnClickClose()
    {
        m_mngr.ShowMainMenu();
    }
}
