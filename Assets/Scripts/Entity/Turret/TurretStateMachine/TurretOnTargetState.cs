using UnityEngine;

namespace Entity.Turret.TurretStateMachine
{
    public class TurretOnTargetState : ITurretState
    {
        private readonly TurretController _controller;
        private float _timeOnTarget;

        public TurretOnTargetState(TurretController controller)
        {
            _controller = controller;
        }
        public void Enter()
        {
            _timeOnTarget = 0;
        }

        public void Exit()
        {
            
        }

        public void Update()
        {
            if (!_controller.HasTargets)
            {
                _controller.stateMachine.ChangeState(new TurretIdleState(_controller));
                return;
            }

            _timeOnTarget += Time.deltaTime;

            if (_timeOnTarget >= _controller.lockOnTime)
            {
                // Debug.Log("Turret Firing");
                _controller.Fire();
                _timeOnTarget = 0;
            }
            
            Vector3 dir = _controller.currTarget.transform.position - _controller.turretFiringPoint.position;
            dir.y = 0;

            Quaternion rotAngle = Quaternion.LookRotation(dir);
            _controller.turretHead.rotation = Quaternion.Slerp(_controller.turretHead.rotation, rotAngle, _controller.rotationSpeed * Time.deltaTime);
            
            float angle = Quaternion.Angle(_controller.turretFiringPoint.rotation, rotAngle);
            if (angle > _controller.loseLockAngle)
            {
                _controller.stateMachine.ChangeState(new TurretTrackingState(_controller));
            }
        }
    }
}