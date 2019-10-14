//using System;
//using System.Collections.Generic;
//using UnityEngine;

//internal class ClusterTech : MonoBehaviour
//{
//    private class Offset
//    {
//        public static float OffsetCorrectionStrength = 1f;
//        public void Set(Transform Body, Transform Target, Tank TargetRef, Vector3 RelativeAxisToIgnore)
//        {
//            A = Body; B = Target;
//            this.TargetRef = TargetRef;
//            this.RelativeAxisToIgnore = RelativeAxisToIgnore;
//        }

///// <summary>
///// A:Body, B:Target
///// </summary>
//        public Transform A, B;
//        /// <summary>
//        /// B:Target(Tank)
//        /// </summary>
//        public Tank TargetRef;

///// <summary>
///// Axis to flatten with plane projection, relative to B:Target's rotation
///// </summary>
//        public Vector3 RelativeAxisToIgnore;

//        public Vector3 TranslationToTarget() 
//        {
//            var result = B.position - A.position * OffsetCorrectionStrength;
//            if (RelativeAxisToIgnore != Vector3.zero)
//            {
//                return Vector3.ProjectOnPlane(result, B.rotation * RelativeAxisToIgnore);
//            }
//            return result;
//        }
//    }

//    Dictionary<Tank, Offset> Techs = new Dictionary<Tank, Offset>();

//    bool Dirty;
//    private void CleanDirty()
//    {
//        var list = Techs;
//        Techs = new Dictionary<Tank, Offset>();
//        foreach (var t in list)
//        {
//            try
//            {
//                if (t.Key == null) continue;
//                t.Key.trans.parent = transform.parent;
//            }
//            catch
//            {
//                Console.WriteLine($"Error at cleaning tech from {name}");
//            }
//        }
//        foreach (var t in list)
//        {
//            try
//            {
//                if (t.Key == null || t.Value == null) continue;
//                VerifyJoin(t.Key, t.Value.TargetRef).Techs.Add(t.Key, t.Value);
//            }
//            catch
//            {
//                Console.WriteLine($"Error at cleaning tech on {name}");
//            }
//        }
//        list.Clear();
//        Dirty = false;
//    }

//    public void FixedUpdate()
//    {
//        if (Dirty)
//        {
//            CleanDirty();
//            return;
//        }
//        float totalMass = 0;
//        foreach (var Tech in Techs) // Pre-iterate to get total mass for placement correction
//        {
//            totalMass += Tech.Key.rbody.mass;
//        }

//        foreach (var Tech in Techs)
//        {
//            if (Tech.Value == null) continue;
//            var ofst = Tech.Value.TranslationToTarget();
//            float float1 = -(Tech.Key.rbody.mass / totalMass); // How much all other bodies must move collectively
//            // Equal and opposite reaction
//            var ofstA = (1 + float1) * ofst;
//            var ofstB = float1 * ofst;
//            //Tech.Key.rbody.AddForceAtPosition(ofstA, Tech.Value.A.position, ForceMode.VelocityChange);
//            Tech.Key.trans.position += ofstA;
//            foreach (var OtherTech in Techs)
//            {
//                if (OtherTech.Key == Tech.Key) continue;
//                OtherTech.Key.trans.position += ofstB;
//            }
//        }
//    }



///// <summary>
///// Set how a body should be corrected
///// </summary>
//    public static void SetOffset(Tank Body, Transform EffectorBody, Transform EffectorTarget, Tank EffectorTank, Vector3 RelativeAxisToIgnore)
//    {
//        var CT = Body.GetComponentInParent<ClusterTech>();
//        if (CT == null)
//        {
//            Console.WriteLine($"Tried to modify body {Body.name}'s offset, but it was not on a ClusterTech!");
//            return;
//        }
//        if (CT.Techs.ContainsKey(Body))
//        {
//            if (CT.Techs[Body] == null)
//                CT.Techs[Body] = new Offset();
//            CT.Techs[Body].Set(EffectorBody, EffectorTarget, EffectorTank, RelativeAxisToIgnore);
//        }
//        else
//        {
//            Console.WriteLine("Tried to modify offset from ClusterTech " + Body.name + ", but the body referenced is not added!");
//        }
//    }

//    /// <summary>
//    /// Set how a body should be corrected
//    /// </summary>
//    public static void SetOffset(Tank Body, Vector3 RelativeAxisToIgnore)
//    {
//        var CT = Body.GetComponentInParent<ClusterTech>();
//        if (CT == null)
//        {
//            Console.WriteLine($"Tried to modify body {Body.name}'s offset, but it was not on a ClusterTech!");
//            return;
//        }
//        if (CT.Techs.ContainsKey(Body))
//        {
//            if (CT.Techs[Body] == null)
//                CT.Techs[Body] = new Offset();
//            CT.Techs[Body].RelativeAxisToIgnore = RelativeAxisToIgnore;
//        }
//        else
//        {
//            Console.WriteLine("Tried to modify offset from ClusterTech " + Body.name + ", but the body referenced is not added!");
//        }
//    }

//    public static void ResetOffset(Tank Body)
//    {
//        var CT = Body.GetComponentInParent<ClusterTech>();
//        if (CT == null)
//        {
//            Console.WriteLine($"Tried to reset body {Body.name}'s offset, but it was not on a ClusterTech!");
//            return;
//        }
//        if (CT.Techs.ContainsKey(Body))
//        {
//            CT.Techs[Body] = null;
//        }
//        else
//        {
//            Console.WriteLine("Tried to modify offset from ClusterTech " + Body.name + ", but the body referenced is not added!");
//        }
//    }

//    private static ClusterTech GenerateNewCluster(Tank FirstBody)
//    {
//        ClusterTech result = new GameObject(FirstBody.name).AddComponent<ClusterTech>();
//        result.transform.parent = FirstBody.trans.parent;
//        Console.WriteLine("Created new ClusterTech " + FirstBody.name);
//        result.Add(FirstBody);
//        return result;
//    }

//    public static ClusterTech VerifyJoin(Tank Host, Tank Body)
//    {
//        ClusterTech CT = Host.GetComponentInParent<ClusterTech>();
//        if (!CT)
//        {
//            CT = GenerateNewCluster(Host);
//        }
//        if (Body != null && !CT.Techs.ContainsKey(Body))
//        {
//            CT.Add(Body);
//        }
//        return CT;
//    }

//    public static void Remove(Tank Body)
//    {
//        if (Body == null)
//        {
//            Console.WriteLine("Tried to remove a null Tank from ClusterTech! What do I do?");
//            return;
//        }
//        var CT = Body.GetComponentInParent<ClusterTech>();
//        if (CT != null)
//        {
//            if (CT.Techs.Count <= 2)
//            {
//                Console.WriteLine("Removed ClusterTech " + CT.name);
//                foreach (var t in CT.Techs)
//                {
//                    if (t.Key != null)
//                        t.Key.trans.parent = CT.transform.parent;
//                    else
//                        Console.WriteLine("Body in list is already null!");
//                }
//                GameObject.DestroyImmediate(CT.gameObject);
//                return;
//            }
//            CT.Techs.Remove(Body);

//            Body.trans.parent = CT.transform.parent;
//            CT.Dirty = true;
//            Console.WriteLine("Removed body from ClusterTech " + CT.name);
//        }
//        else
//        {
//            Console.WriteLine("Attempted to remove body from ClusterTech " + CT.name + ", but none was present!");
//        }
//    }

//    public void Add(Tank Body)
//    {
//        Body.trans.parent = transform;
//        Techs.Add(Body, null);
//        Console.WriteLine("Added body to ClusterTech " + name);
//    }
//}