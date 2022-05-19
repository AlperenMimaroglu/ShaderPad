using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AM.ComputeShaders
{
    public struct ShapeData
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 colour;
        public int shapeType;
        public int operation;
        public float blendStrength;
        public int numChildren;

        public static int GetSize()
        {
            return sizeof(float) * 10 + sizeof(int) * 3;
        }
    }

    [ExecuteInEditMode]
    public class RaymarchMaster : MonoBehaviour
    {
        [SerializeField] private UniversalRendererData rendererData;
        [SerializeField] private string featureName = string.Empty;

        private bool TryGetFeature(out ScriptableRendererFeature feature)
        {
            feature = rendererData.rendererFeatures.FirstOrDefault(f => f.name == featureName);
            return feature != null;

            // use rendererData.SetDirty(); to tell to renderer to refresh itself
        }

        private void Update()
        {
            if (!TryGetFeature(out var feature)) return;

            var rf = feature as RaymarchFeature;

            var shapeData = CreateScene();
            rf.Init(shapeData);
        }

        private ShapeData[] CreateScene()
        {
            List<Shape> allShapes = new List<Shape>(FindObjectsOfType<Shape>());
            allShapes.Sort((a, b) => a.operation.CompareTo(b.operation));

            List<Shape> orderedShapes = new List<Shape>();

            foreach (var shape in allShapes)
            {
                // Add top-level shapes (those without a parent)
                if (shape.transform.parent != null) continue;

                var parentShape = shape.transform;
                orderedShapes.Add(shape);

                shape.numChildren = parentShape.childCount;
                // Add all children of the shape (nested children not supported currently)
                for (int i = 0; i < parentShape.childCount; i++)
                {
                    if (parentShape.GetChild(i).GetComponent<Shape>() == null) continue;
                    orderedShapes.Add(parentShape.GetChild(i).GetComponent<Shape>());
                    orderedShapes[^1].numChildren = 0;
                }
            }

            ShapeData[] shapeData = new ShapeData[orderedShapes.Count];
            for (int i = 0; i < orderedShapes.Count; i++)
            {
                var s = orderedShapes[i];
                Vector3 col = new Vector3(s.colour.r, s.colour.g, s.colour.b);
                shapeData[i] = new ShapeData()
                {
                    position = s.Position,
                    scale = s.Scale, colour = col,
                    shapeType = (int) s.shapeType,
                    operation = (int) s.operation,
                    blendStrength = s.blendStrength * 3,
                    numChildren = s.numChildren
                };
            }

            return shapeData;
        }
    }
}