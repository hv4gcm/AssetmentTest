using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES,
        TIME_ATTACK
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        GAME_WIN
    }

    public bool IsTimeAttackMode { get; private set; }

    public enum eAutoPlayMode
    {
        None,
        AutoWin,
        AutoLose
    }
    public eAutoPlayMode AutoPlayMode { get; private set; }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        AutoPlayMode = eAutoPlayMode.None;
        IsTimeAttackMode = (mode == eLevelMode.TIME_ATTACK);

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);

        if (IsTimeAttackMode)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            // Thời gian 60 giây cho Time Attack
            m_levelCondition.Setup(60f, m_uiMenu.GetLevelConditionView(), this);
            m_levelCondition.ConditionCompleteEvent += GameOver;
        }

        /* === OLD CODE: Level Modes (Moves/Timer) ===
        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
        }

        m_levelCondition.ConditionCompleteEvent += GameOver;
        */

        State = eStateGame.GAME_STARTED;
    }

    public void LoadLevelAutoPlay(eAutoPlayMode autoMode)
    {
        AutoPlayMode = autoMode;
        // Tái sử dụng code LoadLevel
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);
        State = eStateGame.GAME_STARTED;
    }


    public void GameOver()
    {
        StartCoroutine(WaitBoardController(eStateGame.GAME_OVER));
    }
    public void GameWin()
    {
        StartCoroutine(WaitBoardController(eStateGame.GAME_WIN));
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController(eStateGame finalState)
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = finalState;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;

            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
}
