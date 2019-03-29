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
            Swivel,
            LargeBall
        }
        public MTMagTypes Identity;
        public float TransformCorrection = 0.3f, VelocityCorrection = 0.4f;
        public Action<ModuleMTMagnet, ModuleMTMagnet> ConfigureNewJoint;
        public Joint joint;
        public Vector3 Effector = Vector3.up * 0f;
        bool _Binded;
        bool _BondIsValid;
        bool _ExpectBond;
        bool _BindedTo;
        public ModuleMTMagnet _BoundBody;
        bool Heart = false;
        bool HeartChanged = false;
        bool Recalc = false;
        public Vector3 LocalPosWithEffector => block.transform.localPosition + (block.transform.localRotation * Effector);
        public Vector3 GetEffector => block.transform.rotation * Effector;
        private Vector3 GetEffectorOffsetDownBy10 => block.transform.rotation * (Effector + Vector3.down * 10f);

        void OnTriggerStay(Collider other)
        {
            try
            {
                if (HeartChanged || _BindedTo)
                {
                    return;
                }
                if (!block.IsAttached || block.tank.beam.IsActive) return; //If block is not attached or if the tank is in build beam, leave
                var NewBody = other.GetComponentInParent<ModuleMTMagnet>();
                if (Recalc || !_Binded)
                {
                    if (((_Binded && NewBody == _BoundBody) // Same body as before, OR
                        || (!_Binded && NewBody != null && !NewBody._Binded && !NewBody._BindedTo && NewBody.Identity == Identity)) // Bond is possible, THEN
                        && NewBody.block.IsAttached && (!NewBody.block.tank.IsAnchored || !block.tank.IsAnchored)) // If other block is indeed attached and at least one of the two techs is unanchored
                    {
                        if (joint != null)
                        {
                            Destroy(joint); // Destroy bond for updating
                            joint = null;
                        }
                        Recalc = false;
                        _Binded = true;
                        _BoundBody = NewBody;
                        _BoundBody._BindedTo = true;
                        //var oldPos = block.tank.transform.position; var oldrot = block.tank.transform.rotation;
                        switch (Identity)
                        {
                            case MTMagTypes.Fixed:
                                {
                                    //var inv = Quaternion.Inverse(_BoundBody.transform.rotation);
                                    //block.tank.transform.rotation *= Quaternion.FromToRotation(inv * transform.up, inv * (-_BoundBody.transform.up));

                                    //var angle = Vector3.SignedAngle(transform.forward, _BoundBody.transform.forward, transform.up) + 360;
                                    //block.tank.transform.Rotate(transform.localRotation * Vector3.up, angle - Mathf.Round(angle / 90) * 90, Space.Self);

                                    //block.tank.transform.position = _BoundBody.block.transform.position + _BoundBody.GetEffector - GetEffector;
                                    Class1.CFixedJoint(this, _BoundBody);
                                    break;
                                }
                            case MTMagTypes.LargeBall:
                            case MTMagTypes.Ball:
                                {
                                    //block.tank.transform.position = _BoundBody.block.transform.position + _BoundBody.GetEffector - GetEffector;
                                    Class1.CBallJoint(this, _BoundBody);
                                    break;
                                }
                            case MTMagTypes.Swivel:
                                {
                                    //var inv2 = Quaternion.Inverse(_BoundBody.transform.rotation);
                                    //block.tank.transform.rotation *= Quaternion.FromToRotation(inv2 * transform.up, inv2 * (-_BoundBody.transform.up));
                                    //block.tank.transform.position = _BoundBody.block.transform.position + _BoundBody.GetEffector - GetEffector;
                                    Class1.CSwivelJoint(this, _BoundBody);
                                    break;
                                }
                        }
                        //block.tank.transform.position = oldPos;
                        //block.tank.transform.rotation = oldrot;
                        _BondIsValid = true;
                        if (_ExpectBond)
                        {
                            if (block.tank.IsSleeping)
                            {
                                block.tank.SetSleeping(false);
                            }
                            _ExpectBond = false;
                        }
                    }
                }
                if (_Binded && NewBody == _BoundBody)
                {
                    _BondIsValid = true;
                    var Bm = _BoundBody.block.tank.rbody.mass;
                    var Am = block.tank.rbody.mass;
                    var offset = (_BoundBody.block.transform.position + _BoundBody.GetEffector - block.transform.position - GetEffector) * TransformCorrection;
                    var tension = Vector3.Project(_BoundBody.block.tank.rbody.velocity - block.tank.rbody.velocity, (_BoundBody.block.transform.position + _BoundBody.GetEffectorOffsetDownBy10 - block.transform.position - GetEffectorOffsetDownBy10)) ;
                    if (!block.tank.IsAnchored && !block.tank.beam.IsActive)
                    {
                        block.tank.transform.position += offset * (Am / (Am + Bm));
                        block.tank.rbody.AddForceAtPosition((offset + tension) * VelocityCorrection, block.transform.position + GetEffector);
                        debugLR.enabled = true;
                        debugLR.SetPosition(0, GetEffector);
                        debugLR.SetPosition(1, transform.InverseTransformVector((offset + tension) * 30f));
                    }
                    if (!_BoundBody.block.tank.IsAnchored && !_BoundBody.block.tank.beam.IsActive)
                    {
                        _BoundBody.block.tank.transform.position -= offset * (Bm / (Am + Bm));
                        _BoundBody.block.tank.rbody.AddForceAtPosition((offset + tension) * -VelocityCorrection, _BoundBody.block.transform.position + GetEffector);
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
                HeartChanged = true;
                if (joint != null)
                {
                    Destroy(joint); // Destroy bond
                    //_BoundBody._BindedTo = false;
                    joint = null;
                    //_BoundBody = null;
                }
                //_Binded = false;
            }
            else
            {
                HeartChanged = false;
            }
            if ((_ExpectBond||_Binded) && !_BondIsValid) // If bond body could not be reached
            {
                if (_ExpectBond) // If it was just loaded
                {
                    if ((block.tank.boundsCentreWorld - Singleton.cameraTrans.position).sqrMagnitude > 40000)
                    {
                        if (!block.tank.IsSleeping)
                        {
                            block.tank.SetSleeping(true);
                        }
                        return;
                    }
                    if (ManGameMode.inst.GetModePhase() != ManGameMode.GameState.InGame) // If it is still loading the game, freeze the tech
                    {
                        block.tank.SetSleeping(true);
                        return;
                    }
                    _ExpectBond = false;
                    if (block.tank.IsSleeping)
                    {
                        block.tank.SetSleeping(false);
                    }
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
            _BindedTo = false;
        }

        private void OnPool()
        {
            block.AttachEvent.Subscribe(OnAttach);
            block.DetachEvent.Subscribe(OnDetach);
            if (Identity != MTMagTypes.Ball || Identity != MTMagTypes.LargeBall)
            foreach(Collider box in gameObject.GetComponentsInChildren<Collider>())
            {
                if (!box.isTrigger) box.material = new PhysicMaterial() { dynamicFriction = 0, frictionCombine = PhysicMaterialCombine.Maximum, staticFriction = 0f };
            }
            var g = new GameObject();
            g.transform.parent = transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            debugLR = g.AddComponent<LineRenderer>();
            debugLR.positionCount = 2;
            debugLR.startWidth = 1f;
            debugLR.endWidth = 0.5f;
            debugLR.useWorldSpace = true;
            debugLR.material = Nuterra.BlockInjector.GameObjectJSON.MaterialFromShader();
            debugLR.enabled = false;
        }

        LineRenderer debugLR;

        private void OnDetach()
        {
            debugLR.enabled = false;
            _ExpectBond = false;
            if (_Binded)
            {
                Destroy(joint); // Destroy bond
                joint = null;
                _Binded = false;
                _BoundBody._BindedTo = false;
                _BoundBody = null;
            }
        }

        private void OnAttach()
        {
            debugLR.enabled = true;
            if (_Binded)
            {
                Destroy(joint); // Destroy bond
                joint = null;
                _Binded = false;
                _BoundBody._BindedTo = false;
                _BoundBody = null;
            }
        }

        private void OnSpawn()
        {
            _ExpectBond = true;
        }
    }
}
