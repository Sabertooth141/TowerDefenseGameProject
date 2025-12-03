namespace Entity.Enemy.EnemyStateMachine
{
    public interface IEnemyState
    {
        void Enter();
        void Exit();
        void Update();
    }
}