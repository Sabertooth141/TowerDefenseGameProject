using UnityEngine;

namespace Entity.Turret.TurretStateMachine
{
    public class TurretTrackingState : ITurretState
    {
        private readonly TurretController _controller;
        
        public TurretTrackingState(TurretController controller)
        {
            _controller = controller;
        }

        public void Enter()
        {
            
        }

        public void Update()
        {
            if (!_controller.HasTargets)
            {
                _controller.stateMachine.ChangeState(new TurretIdleState(_controller));
                return;
            }

            if (_controller.currTarget == null)
            {
                _controller.stateMachine.ChangeState(new TurretIdleState(_controller));
                return;
            }
            
            Vector3 dir = _controller.currTarget.transform.position - _controller.turretFiringPoint.position;
            // dir.y = 0;

            Quaternion rotAngle = Quaternion.LookRotation(dir);
            _controller.turretHead.rotation = Quaternion.Slerp(_controller.turretHead.rotation, rotAngle, _controller.rotationSpeed * Time.deltaTime);
            
            Vector3 angleToTarget = (_controller.currTarget.transform.position - _controller.turretFiringPoint.position).normalized;
            float angle = Vector3.Angle(_controller.turretFiringPoint.forward, angleToTarget);

            if (angle < _controller.firingAngle)
            {
                _controller.stateMachine.ChangeState(new TurretOnTargetState(_controller));
            }
            
        }

        public void Exit()
        {
            
        }
    }
}