using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Control_Block
{
    internal class ModuleSteeringRegulator : Module
    {
        float cachedDrive = 0f, cachedTurn = 0f;
        /// <summary>
        /// Constants for controlling the compensation calculation
        /// </summary>
        const float InputDeadZone = 0.35f, AngularStrength = .2f, LinearStrength = .1f, VelocityRatio = 6.4f, RotationRatio = 10f;
        /// <summary>
        /// Inverse InputDeadZone (1 / InputDeadZone)
        /// </summary>
        const float _idz = 2.857142857142857f;
        /// <summary>
        /// Result effector based on given input
        /// </summary>
        float VerticalMultiplier = 0f, SteeringMultiplier = 0f;

        public bool UseGroundMode { get; private set; } = true;
        MeshRenderer _mr;
        MeshRenderer mr
        {
            get
            {
                if (_mr == null)
                {
                    _mr = block.GetComponentInChildren<MeshRenderer>();
                }
                return _mr;
            }
        }
        public void SetColor(Color color)
        {
            mr.material.SetColor("_EmissionColor", color);
        }

        public Vector3 PositionalFixingVector { get; private set; } = Vector3.zero;

        private void OnPool()
        {
            base.block.AttachEvent += this.OnAttach;
            base.block.DetachEvent += this.OnDetach;
        }
        private void OnDetach()
        {
            base.block.tank.control.driveControlEvent -= this.DriveControlInput;
            SetColor(Color.white);
        }
        private void OnAttach()
        {
            base.block.tank.control.driveControlEvent += this.DriveControlInput;
        }

        private void DriveControlInput(float drive, float turn)
        {
            cachedDrive = drive;
            cachedTurn = turn;
        }

        private float IfNotInDeadZone(float Value, float DeadZone)
        {
            if (Mathf.Abs(Value) < DeadZone)
                return 0f;
            return Value;
        }

        Vector3 Target = Vector3.zero;

        void FixedUpdate()
        {
            try
            {
                if (block.tank == null)
                    return;
                if (Vector3.Distance(Target, block.transform.position) > 2.5f)
                    Target = block.transform.position;



                if (base.block.tank.GetComponentInChildren<ModuleSteeringRegulator>() != this)
                {
                    SetColor(Color.black);
                    return;
                }
                if (block.tank != null && !Singleton.Manager<ManPauseGame>.inst.IsPaused)
                {
                    VerticalMultiplier = Mathf.Max(InputDeadZone - Mathf.Abs(cachedDrive), 0f) * _idz;
                    SteeringMultiplier = Mathf.Max(InputDeadZone - Mathf.Abs(cachedTurn), 0f) * _idz;

                    {
                        var tr = Quaternion.Inverse(block.tank.control.FirstController.transform.rotation);
                        var rb = block.tank.rbody;
                        var linearVel = tr * rb.velocity;
                        var angularVel = rb.angularVelocity.y;
                        var linearOffset = tr * (block.transform.position - Target);
                        PositionalFixingVector = new Vector3(
                            -(linearVel.x + linearOffset.x) * SteeringMultiplier * LinearStrength,
                            -(linearVel.z + linearOffset.z) * VerticalMultiplier * LinearStrength,
                            IfNotInDeadZone(angularVel * SteeringMultiplier * AngularStrength * VerticalMultiplier, .0125f));

                        UseGroundMode = block.tank.control.BoostControl || (SteeringMultiplier == 0) || ((Mathf.Abs(PositionalFixingVector.x) + Mathf.Abs(PositionalFixingVector.y)) * VelocityRatio < Mathf.Abs(PositionalFixingVector.z) * RotationRatio + .26f);
                        SetColor(UseGroundMode ? Color.red : Color.white);
                    }
                }
            }
            catch { }
        }
    }
}
