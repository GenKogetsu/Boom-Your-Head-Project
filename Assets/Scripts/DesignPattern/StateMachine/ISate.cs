namespace Genoverrei.DesignPattern;

public interface ISate
{
 
}

public interface IEnterState : ISate
{
    void OnEnter();
}

public interface IUpdateState : ISate
{
    void OnUpdate();
}

public interface IExitState : ISate
{
    void OnExit();
}
