using UnityEngine;

namespace Control_Block
{
    class SimpleConnectLineRenderer : Module
    {
        LineRenderer renderer;
        public Material material;
        public float width;
        public Vector3 strPos;
        public GameObject refObj;
        public Vector3 refPos;

        void OnPool()
        {
            renderer = gameObject.AddComponent<LineRenderer>();
            renderer.material = material;
            renderer.widthMultiplier = width;
            renderer.useWorldSpace = false;
            renderer.SetPosition(0, strPos);
            renderer.SetPosition(1, refObj.transform.localPosition + refPos);
        }

        void Update()
        {
            renderer.SetPosition(1, refObj.transform.localPosition + refPos);
        }
    }
}
