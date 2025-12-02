namespace Entity.Turret.TurretStateMachine
{
    public class TurretStateMachine
    {
        private ITurretState CurrentState { get; set; }

        public void ChangeState(ITurretState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}