public partial interface ICombatantState
{
    void Enter();
    void Exit();
    void Process(double delta);
}