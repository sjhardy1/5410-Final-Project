public partial interface ICombatantState
{
    void Enter(RunState runState);
    void Exit();
    void Process(double delta);
}