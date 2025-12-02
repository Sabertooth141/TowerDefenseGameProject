namespace Entity.Turret.TurretStateMachine
{
    public interface ITurretState
    {
        void Enter();
        void Exit();
        void Update();
    }
}