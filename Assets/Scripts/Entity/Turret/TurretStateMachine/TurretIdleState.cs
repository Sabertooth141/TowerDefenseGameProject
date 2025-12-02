using Unity.Mathematics;
using UnityEngine;

namespace Entity.Turret.TurretStateMachine
{
    public class TurretIdleState : ITurretState
    {
        private readonly TurretController _controller;
        private float _resetTimer;
        
        public TurretIdleState(TurretController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            _resetTimer = 0;
        }

        public void Update()
        {
            if (_controller.HasTargets)
            {
                _controller.currTarget = _controller.targets[0];
                _controller.stateMachine.ChangeState(new TurretTrackingState(_controller));
                return;
            }

            _resetTimer += Time.deltaTime;
            if (_resetTimer >= _controller.timeToReset)
            {
                _controller.turretHead.rotation = Quaternion.Slerp(_controller.turretHead.rotation, Quaternion.identity, Time.deltaTime * _controller.rotationSpeed);
            }
        }

        public void Exit()
        {
            
        }
    }
}