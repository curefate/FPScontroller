public class StateBase
{

    //给每个状态设置一个ID
    public int ID { get; set; }

    //被当前机器所控制
    public StateMachine machine;

    public StateBase(int id)
    {
        this.ID = id;
    }

    //给子类提供方法
    public virtual void OnEnter(params object[] args) { }
    public virtual void OnStay(params object[] args) { }
    public virtual void OnExit(params object[] args) { }

}

/// <summary>
/// 状态拥有者
/// </summary>
public class StateTemplate<T> : StateBase
{
    public T owner;   //拥有者(范型)

    public StateTemplate(int id, T o) : base(id)
    {
        owner = o;
    }
}