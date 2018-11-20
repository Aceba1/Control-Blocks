﻿using System;
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
        const float PassIfAbove = .025f, AngularSensitivity = 0.005f, LinearSensitivity = 0.01f, InputDeadZone = 0.35f, AngularStrength = 3f, LinearStrength = 2f, MaxLinearRange = 0.5f, MaxAngularRange = 0.5f, VelocityToIgnoreAngle = .2f, MinAngularVelocity = 0.02f, DecayStart = .25f, DecayRate = .95f;
        const float _idz = 2.857142857142857f;
        /// <summary>
        /// Result effector based on given input
        /// </summary>
        float VerticalMultiplier = 0f, SteeringMultiplier = 0f;
        bool Heart = false;
        public bool UseGroundMode(Vector3 calculated)
        {
            return (SteeringMultiplier == 0 || Heart) && Mathf.Abs(calculated.z) > MinAngularVelocity && (Mathf.Abs(calculated.x) < VelocityToIgnoreAngle);
        }
        //public float SteerFixing
        //{
        //    get
        //    {
        //        var thing = FixedSteering * SteeringMultiplier * AngularStrength;
        //        if (Mathf.Abs(thing) > PassIfAbove)
        //            return thing;
        //        return 0f;
        //    }
        //}

        public Vector3 PositionalFixingVector
        {
            get
            {
                var tr = Quaternion.Inverse(block.tank.control.FirstController.transform.rotation);
                var rb = block.tank.rbody;
                var linearVel = tr * rb.velocity;
                var angularVel = (tr * rb.angularVelocity).y;
                return new Vector3(-linearVel.x* SteeringMultiplier * LinearStrength, -linearVel.z * VerticalMultiplier * LinearStrength, -angularVel * SteeringMultiplier * AngularStrength);

            }
        }

        private void OnPool()
        {
            base.block.AttachEvent += this.OnAttach;
            base.block.DetachEvent += this.OnDetach;
        }
        private void OnDetach()
        {
            base.block.tank.control.driveControlEvent -= this.DriveControlInput;
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
        void LateUpdate()
        {
            try
            {
                if (block.tank != null && !Singleton.Manager<ManPauseGame>.inst.IsPaused)
                {
                    VerticalMultiplier = Mathf.Max(InputDeadZone - Mathf.Abs(cachedDrive), 0f) * _idz;
                    SteeringMultiplier = Mathf.Max(InputDeadZone - Mathf.Abs(cachedTurn), 0f) * _idz;
                    Heart = !Heart;
                }
            }
            catch { }
        }
    }
}
