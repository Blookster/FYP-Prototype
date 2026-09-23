
public interface IState
{
    void OnEnter();
    
    void OnUpdate(float dist);

    void OnExit();
}
