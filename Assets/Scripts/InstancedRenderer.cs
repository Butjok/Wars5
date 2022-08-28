using UnityEngine;

[ExecuteInEditMode]
public class InstancedRenderer : MonoBehaviour {
    public float range;

    public Material material;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    private Bounds bounds;

    public Foliage foliage;
    public int foliageIndex;

    public Vector3 offset;

    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties {
        public Matrix4x4 mat;
        public Vector4 color;

        public static int Size() {
            return
                sizeof(float) * 4 * 4 + // matrix;
                sizeof(float) * 4;      // color;
        }
    }

    private void Setup() {
        //Mesh mesh = CreateQuad();

        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        bounds = new Bounds();
        foreach (var matrix in foliage.types[foliageIndex].transforms) {
            // https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
            var position = matrix.GetColumn(3);
            bounds.Encapsulate(position);
        }

        InitializeBuffers();
    }

    private void InitializeBuffers() {

        var population = foliage.types[foliageIndex].transforms.Length;
        var mesh = foliage.types[foliageIndex].mesh;
        
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)population;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        // Initialize buffer with the given population.
        MeshProperties[] properties = new MeshProperties[population];
        for (int i = 0; i < population; i++) {
            MeshProperties props = new MeshProperties();
            Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            Vector3 scale = Vector3.one;

            props.mat = Matrix4x4.TRS(position, rotation, scale);
            props.mat = foliage.types[foliageIndex].transforms[i];
            props.color = Color.Lerp(Color.red, Color.blue, Random.value);

            properties[i] = props;
        }

        meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties);
        material.SetBuffer("_Properties", meshPropertiesBuffer);
    }

    /*private Mesh CreateQuad(float width = 1f, float height = 1f) {
        ...
    }*/

    private void OnEnable() {
        Setup();
    }

    private void Update() {
        Graphics.DrawMeshInstancedIndirect(foliage.types[foliageIndex].mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDisable() {
        // Release gracefully.
        if (meshPropertiesBuffer != null) {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (argsBuffer != null) {
            argsBuffer.Release();
        }
        argsBuffer = null;
    }
}