using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Control_Block
{
    class ModuleMTMagnet : Module 
    {
        public enum MTMagTypes : byte
        {
            Fixed,
            Ball,
            Swivel
        }
        public MTMagTypes Identity;
        public float TransformCorrection = 0.3f, VelocityCorrection = 0.75f;
        public Action<ModuleMTMagnet, Rigidbody> ConfigureNewJoint;
        public Joint joint;
        public Vector3 Effector = Vector3.up * 0f;
        bool _Binded;
        bool _BondIsValid;
        bool _ExpectBond;
        bool _BindedTo;
        public ModuleMTMagnet _BoundBody;
        bool Heart = false;
        bool Recalc = false;
        public Vector3 LocalPosWithEffector => block.transform.localPosition + Effector;
        public Vector3 GetEffector => block.transform.rotation * Effector;

        void OnTriggerStay(Collider other)
        {
            try
            {
                if (Heart != Class1.PistonHeart || _BindedTo)
                {
                    return;
                }
                if (!block.IsAttached) return;
                var NewBody = other.GetComponentInParent<ModuleMTMagnet>();
                if (Recalc || !_Binded)
                {
                    if (((_Binded && NewBody == _BoundBody) || (!_Binded && NewBody != null && !NewBody._Binded && !NewBody._BindedTo && NewBody.Identity == Identity)) && NewBody.block.IsAttached && (!NewBody.block.tank.IsAnchored || !block.tank.IsAnchored))
                    {
                        if (joint != null)
                        {
                            Destroy(joint); // Destroy bond
                            joint = null;
                        }
                        Recalc = false;
                        _Binded = true;
                        _BoundBody = NewBody;
                        _BoundBody._BindedTo = true;
                        var oldPos = block.tank.transform.position; var oldrot = block.tank.transform.rotation;
                        switch (Identity)
                        {
                            case MTMagTypes.Fixed:
                                var inv = Quaternion.Inverse(transform.rotation);
                                block.tank.transform.rotation *= Quaternion.FromToRotation(inv * transform.up, inv * -_BoundBody.transform.up);

                                var angle = Vector3.SignedAngle(transform.forward, _BoundBody.transform.forward, transform.up) + 360;
                                block.tank.transform.Rotate(transform.localRotation * Vector3.down, Mathf.Round(angle / 90) * 90 - angle, Space.Self);

                                block.tank.transform.position += _BoundBody.block.transform.position + _BoundBody.GetEffector - block.transform.position - GetEffector;
                                Class1.CFixedJoint(this, _BoundBody.block.tank.rbody);
                                break;
                            case MTMagTypes.Ball:
                                block.tank.transform.position += _BoundBody.block.transform.position + _BoundBody.GetEffector - block.transform.position - GetEffector;
                                Class1.CBallJoint(this, _BoundBody.block.tank.rbody);
                                break;
                            case MTMagTypes.Swivel:
                                var inv2 = Quaternion.Inverse(transform.rotation);
                                block.tank.transform.rotation *= Quaternion.FromToRotation(inv2 * transform.up, inv2 * -_BoundBody.transform.up);
                                block.tank.transform.position += _BoundBody.block.transform.position + _BoundBody.GetEffector - block.transform.position - GetEffector;
                                Class1.CSwivelJoint(this, _BoundBody.block.tank.rbody);
                                break;
                        }
                        block.tank.transform.position = oldPos;
                        block.tank.transform.rotation = oldrot;
                        _BondIsValid = true;
                        _ExpectBond = false;
                    }
                }
                if (_Binded)
                {
                    if (NewBody == _BoundBody)
                    {
                        _BondIsValid = true;
                        var Bm = _BoundBody.block.tank.rbody.mass;
                        var Am = block.tank.rbody.mass;
                        var offset = (_BoundBody.block.transform.position + _BoundBody.GetEffector - block.transform.position - GetEffector) * TransformCorrection;
                        if (!block.tank.IsAnchored && !block.tank.beam.IsActive)
                        {
                            block.tank.transform.position += offset * (Am / (Am + Bm));
                        }
                        block.tank.rbody.AddForceAtPosition(offset * VelocityCorrection, block.transform.position + GetEffector);
                        if (!_BoundBody.block.tank.IsAnchored && !_BoundBody.block.tank.beam.IsActive)
                        {
                            _BoundBody.block.tank.transform.position -= offset * (Bm / (Am + Bm));
                        }
                        _BoundBody.block.tank.rbody.AddForceAtPosition(-offset * VelocityCorrection, _BoundBody.block.transform.position + GetEffector);
                    }
                }
            }
            catch(Exception E)
            {
                try
                {
                    if (_Binded)
                    _BoundBody._BindedTo = false;
                }
                catch { }
                Console.WriteLine(E.Message);
                Console.WriteLine(E.StackTrace);
            }
        }
        void FixedUpdate()
        {
            Recalc = true;
            if (!block.IsAttached)
            {
                if (joint != null)
                {
                    Destroy(joint); // Destroy bond
                    _BoundBody._BindedTo = false;
                    joint = null;
                    _BoundBody = null;
                }
                _Binded = false;
                return;
            }
            if (Heart != Class1.PistonHeart)
            {
                if (joint != null)
                {
                    Destroy(joint); // Destroy bond
                    //_BoundBody._BindedTo = false;
                    joint = null;
                    //_BoundBody = null;
                }
                //_Binded = false;
            }
            if ((_ExpectBond||_Binded) && !_BondIsValid) // If bond body could not be reached
            {
                if (_ExpectBond) // If it was just loaded
                {
                    ManTechs.inst.CheckSleepRange(block.tank); // Put the tech to sleep if it is far
                    if (block.tank.IsSleeping) return; // If it is asleep, leave as is
                    if (ManGameMode.inst.GetModePhase() != ManGameMode.GameState.InGame) // If it is still loading the game, freeze the tech
                    {
                        block.tank.SetSleeping(true);
                        return;
                    }
                    _ExpectBond = false;
                    ManTechs.inst.CheckSleepRange(block.tank);
                }
                try
                {
                    if (_Binded)
                        _BoundBody._BindedTo = false;
                }
                catch { }
                if (joint != null)
                {
                    Destroy(joint); // Destroy bond
                    joint = null;
                    _BoundBody = null;
                }
                _Binded = false;
            }
            else
            {
                _BondIsValid = false;
            }
            if (_Binded)
            {
                if (_BoundBody == null || (_BoundBody._BoundBody != null && _BoundBody._BoundBody != this))
                {
                    if (joint != null)
                    {
                        Destroy(joint);
                        joint = null;
                        _BoundBody = null;
                    }
                    _ExpectBond = false;
                    _BondIsValid = false;
                    _Binded = false;
                    try
                    {
                        _BoundBody._BindedTo = false;
                    }
                    catch { }
                    return;
                }
                _BoundBody._BindedTo = true;
            }
        }

        void Update()
        {
            Heart = Class1.PistonHeart;
            _BindedTo = false;
        }

        private void OnPool()
        {
            block.AttachEvent += OnAttach;
            block.DetachEvent += OnDetach;
            foreach(BoxCollider box in gameObject.GetComponentsInChildren<BoxCollider>())
            {
                if (!box.isTrigger) box.material = new PhysicMaterial() { dynamicFriction = 0, frictionCombine = PhysicMaterialCombine.Maximum, staticFriction = 0f };
            }
        }
        
        private void OnDetach()
        {
            _ExpectBond = false;
            if (_Binded)
            {
                Destroy(joint); // Destroy bond
                joint = null;
                Console.WriteLine("Destroyed: Unattached");
                _Binded = false;
            }
        }

        private void OnAttach()
        {
            if (_Binded)
            {
                Destroy(joint); // Destroy bond
                joint = null;
                Console.WriteLine("Destroyed: Attached");
                _Binded = false;
            }
        }

        private void OnSpawn()
        {
            _ExpectBond = true;
        }
    }
}
