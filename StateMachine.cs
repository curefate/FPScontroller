using System.Collections.Generic;

/// <summary>
/// ״̬�����ࣺ��Player���ơ����״̬�Ĵ洢���л�����״̬�ı���
/// </summary>
public class StateMachine
{
    private int currentID = -1;
    private int previousID = -1;

    //�����洢��ǰ���������Ƶ�����״̬
    protected Dictionary<int, StateBase> m_StateCache;

    //������һ��״̬
    protected StateBase m_previousState;
    //���嵱ǰ״̬
    protected StateBase m_currentState;

    //������ʼ��ʱ��û����һ��״̬
    public StateMachine(StateBase beginState)
    {
        m_previousState = null;
        m_currentState = beginState;

        m_StateCache = new Dictionary<int, StateBase>();
        //��״̬��ӵ�������
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

    //ͨ��Id���л�״̬
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

    //״̬����
    public void StateOnStay()
    {
        if (m_currentState != null)
        {
            m_currentState.OnStay();
        }
    }
}