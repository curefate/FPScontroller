using System.Collections.Generic;

/// <summary>
/// 状态机器类：由Player控制。完成状态的存储，切换，和状态的保持
/// </summary>
public class StateMachine
{
    private int currentID = -1;
    private int previousID = -1;

    //用来存储当前机器所控制的所有状态
    protected Dictionary<int, StateBase> m_StateCache;

    //定义上一个状态
    protected StateBase m_previousState;
    //定义当前状态
    protected StateBase m_currentState;

    //机器初始化时，没有上一个状态
    public StateMachine(StateBase beginState)
    {
        m_previousState = null;
        m_currentState = beginState;

        m_StateCache = new Dictionary<int, StateBase>();
        //把状态添加到集合中
        AddState(beginState);
        m_currentState.OnEnter();
    }

    public StateBase GetCurrentState()
    {
        return m_currentState;
    }
    public StateBase GetPreviousState()
    {
        return m_previousState;
    }
    public int GetCurrentStateID()
    {
        return currentID;
    }
    public int GetPreviousStateID()
    {
        return previousID;
    }

    public void AddState(StateBase state)
    {
        if (!m_StateCache.ContainsKey(state.ID))
        {
            m_StateCache.Add(state.ID, state);
            state.machine = this;
        }
    }

    //通过Id来切换状态
    public void TranslateState(int id)
    {
        if (!m_StateCache.ContainsKey(id) || id == currentID)
        {
            return;
        }

        previousID = currentID;
        currentID = id;

        m_previousState = m_currentState;
        m_currentState = m_StateCache[id];

        m_previousState.OnExit();
        m_currentState.OnEnter();
    }

    //状态保持
    public void StateOnStay()
    {
        if (m_currentState != null)
        {
            m_currentState.OnStay();
        }
    }
}