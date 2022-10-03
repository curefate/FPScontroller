using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMotionStates
{
    /// <summary>
    /// Idle State
    /// </summary>
    public class IdleState : StateTemplate<ControlManager>
    {
        public IdleState(int id, ControlManager player):base(id, player) { }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);

            // TODO ANI

            owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, 0, 0.5f);
            owner.desireMove.x = owner.moveDir.x * owner.currentSpeed;
            owner.desireMove.y = -owner.stickToGroundForce;
            owner.desireMove.z = owner.moveDir.z * owner.currentSpeed;
        }

        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
        }
    }

    /// <summary>
    /// Walk State
    /// </summary>
    public class WalkState : StateTemplate<ControlManager>
    {
        public WalkState(int id, ControlManager player) : base(id, player) { }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            owner.ActiveBob("WALK");
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);

            // TODO ANI

            if(owner.m_Input.y < 0)
            {
                owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, owner.speed_Walk * owner.backSpeedFactor, 0.5f);
            }
            else
            {
                owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, owner.speed_Walk, 0.5f);
            }
            owner.desireMove.x = owner.moveDir.x * owner.currentSpeed;
            owner.desireMove.y = -owner.stickToGroundForce;
            owner.desireMove.z = owner.moveDir.z * owner.currentSpeed;
        }

        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
        }
    }

    /// <summary>
    /// Run State
    /// </summary>
    public class RunState : StateTemplate<ControlManager>
    {
        public RunState(int id, ControlManager player) : base(id, player) { }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            owner.ActiveBob("RUN");
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);

            // TODO ANI

            if (owner.m_Input.y < 0)
            {
                owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, owner.speed_Run * owner.backSpeedFactor, 0.2f);
            }
            else
            {
                owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, owner.speed_Run, 0.2f);
            }
            owner.desireMove.x = owner.moveDir.x * owner.currentSpeed;
            owner.desireMove.y = -owner.stickToGroundForce;
            owner.desireMove.z = owner.moveDir.z * owner.currentSpeed;
        }

        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
        }
    }

    /// <summary>
    /// Crouch State
    /// </summary>
    public class CrouchState : StateTemplate<ControlManager>
    {
        public CrouchState(int id, ControlManager player) : base(id, player) { }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            owner.ActiveBob("CROUCH");
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);

            // TODO ANI

            owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, owner.speed_Crouch, 0.5f);
            owner.desireMove.x = owner.moveDir.x * owner.currentSpeed;
            owner.desireMove.y = -owner.stickToGroundForce;
            owner.desireMove.z = owner.moveDir.z * owner.currentSpeed;
        }

        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
        }
    }

    /// <summary>
    /// Jump State
    /// </summary>
    public class JumpState : StateTemplate<ControlManager>
    {
        public JumpState(int id, ControlManager player) : base(id, player) { }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            owner.desireMove = owner.lastDesireMove;
            owner.desireMove.y = owner.jumpForce;
            owner.statePointer = ControlManager.ePlayerMotionStates.FALL;
            owner.soundPlayer.PlayJumpSounds();
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);
        }

        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
        }
    }

    /// <summary>
    /// Fall State
    /// </summary>
    public class FallState : StateTemplate<ControlManager>
    {
        private float fallTimer = 0f;
        private Vector3[] rayDir;
        private bool falltowallrun = false;
        private Vector3 inertia;

        public FallState(int id, ControlManager player) : base(id, player)
        {
            rayDir = new Vector3[]
            {
                Vector3.right,
                Vector3.right + Vector3.forward,
                Vector3.forward,
                Vector3.left + Vector3.forward,
                Vector3.left
            };
        }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            owner.ActiveBob("FALL");
            inertia = owner.lastDesireMove;
            fallTimer = 0f;
            falltowallrun = false;
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);

            // TODO ANI

            fallTimer += Time.fixedDeltaTime;
            owner.desireMove = inertia += Physics.gravity * 0.5f * fallTimer * fallTimer * owner.gravityMultiplier;
            owner.desireMove.x += owner.moveDir.x * owner.speed_AirMoving;
            owner.desireMove.z += owner.moveDir.z * owner.speed_AirMoving;

            if (owner.controller.isGrounded)
            {
                if (owner.currentSpeed >= owner.speed_Walk)
                {
                    owner.statePointer = ControlManager.ePlayerMotionStates.RUN;
                }
                else
                {
                    owner.statePointer = ControlManager.ePlayerMotionStates.IDLE;
                }
            }
            else if (owner.lastStatePointer != ControlManager.ePlayerMotionStates.WALLRUN && owner.currentSpeed >= owner.speed_Walk && fallTimer >= owner.minimumFallDuration && fallTimer <= owner.maximumFallDuration)
            {
                if (!Physics.Raycast(owner.transform.position, Vector3.down, owner.minimumHeight + owner.controller.height / 2))
                {
                    RaycastHit[] hits = new RaycastHit[rayDir.Length];
                    for(int i = 0; i < rayDir.Length; i++)
                    {
                        Vector3 dir = owner.transform.TransformDirection(rayDir[i]);
                        Physics.Raycast(owner.transform.position, dir, out hits[i], owner.maximumWallDistance + owner.controller.radius);
                    }
                    hits = hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
                    if (hits.Length > 0)
                    {
                        owner.statePointer = ControlManager.ePlayerMotionStates.WALLRUN;
                        owner.wallHitInfo = hits[0];
                        falltowallrun = true;
                    }
                }
            }
        }

        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
            if (!falltowallrun)
            {
                owner.ActiveBob("LAND");
                owner.soundPlayer.PlayLandingSounds();
            }
        }
    }

    /// <summary>
    /// Slide State
    /// </summary>
    public class SlideState : StateTemplate<ControlManager>
    {
        private Vector3 bodyDir = Vector3.zero;

        public SlideState(int id, ControlManager player) : base(id, player)
        {

        }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            bodyDir = owner.transform.forward;
            owner.ActiveBob("SLIDE");
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);

            // TODO ANI

            // 移动
            if(owner.m_Input.y > 0)
            {
                owner.m_Input.y = 0.001f;
                owner.m_Input.Normalize();
            }
            owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, owner.slideSpeedCurve.Evaluate(owner.slideEnergy), 0.4f);
            owner.desireMove.x += bodyDir.x * owner.currentSpeed + owner.moveDir.x * owner.speed_AirMoving;
            owner.desireMove.y = -owner.stickToGroundForce;
            owner.desireMove.z += bodyDir.z * owner.currentSpeed + owner.moveDir.z * owner.speed_AirMoving;

            owner.soundPlayer.PlaySlideSounds();

            // 根据角度加速或减速
            RaycastHit hitInfo;
            Physics.Raycast(owner.transform.position, Vector3.down, out hitInfo);
            float angle = Vector3.Angle(owner.transform.forward, hitInfo.normal);
            if (angle == 90 && owner.slideEnergy > owner.slideEnergyConsumeStep)
            {
                owner.slideEnergy -= owner.slideEnergyConsumeStep;
            }
            else if (angle > 90 && owner.slideEnergy > owner.slideEnergyConsumeStep)
            {
                owner.slideEnergy -= owner.slideEnergyConsumeStep / Mathf.Tan(angle - 90);
            }
            else if (angle < 90 && owner.currentSpeed < owner.maxSlideSpeedEnergy)
            {
                owner.slideEnergy += owner.slideEnergyIncreseStep / Mathf.Tan(90 - angle);
            }
        }
        
        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
            owner.lastSlideOverTime = Time.realtimeSinceStartup;
            owner.slideEnergy = owner.lowSlideSpeedEnergy;
        }
    }

    /// <summary>
    /// Wall Run State
    /// </summary>
    public class WallRunState : StateTemplate<ControlManager>
    {

        public WallRunState(int id, ControlManager player) : base(id, player) { }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            owner.ActiveBob("WALLRUN");
        }

        public override void OnStay(params object[] args)
        {
            base.OnStay(args);

            // TODO ANI

            Vector3 dir = Vector3.ProjectOnPlane(owner.transform.forward, owner.wallHitInfo.normal).normalized;
            owner.currentSpeed = Mathf.Lerp(owner.currentSpeed, owner.speed_Run, 0.2f);
            owner.desireMove.x = dir.x * owner.currentSpeed;
            owner.desireMove += Physics.gravity * owner.gravityOnWall;
            owner.desireMove.z = dir.z * owner.currentSpeed;
        }

        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
        }
    }

    /// <summary>
    /// Wall Jump State
    /// </summary>
    public class WallJumpState : StateTemplate<ControlManager>
    {
        public WallJumpState(int id,ControlManager player) : base(id, player) { }
    
        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            owner.desireMove = owner.lastDesireMove;
            owner.desireMove += (owner.wallHitInfo.normal + owner.transform.up).normalized * owner.jumpForce;
            owner.statePointer = ControlManager.ePlayerMotionStates.FALL;
            owner.soundPlayer.PlayJumpSounds();
        }
    
        public override void OnStay(params object[] args)
        {
            base.OnStay(args);
        }
    
        public override void OnExit(params object[] args)
        {
            base.OnExit(args);
        }
    }
}
