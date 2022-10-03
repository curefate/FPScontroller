public class StateBase
{

    //��ÿ��״̬����һ��ID
    public int ID { get; set; }

    //����ǰ����������
    public StateMachine machine;

    public StateBase(int id)
    {
        this.ID = id;
    }

    //�������ṩ����
    public virtual void OnEnter(params object[] args) { }
    public virtual void OnStay(params object[] args) { }
    public virtual void OnExit(params object[] args) { }

}

/// <summary>
/// ״̬ӵ����
/// </summary>
public class StateTemplate<T> : StateBase
{
    public T owner;   //ӵ����(����)

    public StateTemplate(int id, T o) : base(id)
    {
        owner = o;
    }
}