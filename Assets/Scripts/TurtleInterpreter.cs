using UnityEngine;

// Interprets an L-system word with a 3D turtle.
//   f : move the turtle l units forward (no geometry)
//   F : create a capsule of height l and radius 1, then move l units forward
// Attach to an empty GameObject; its transform is the turtle's initial state.
public class TurtleInterpreter : MonoBehaviour
{
    [SerializeField] private string word = "FFfFfFF";
    [SerializeField] private float l = 4f;   // step length

    // Turtle state: position + orientation. The heading is the local up (Y)
    // axis, which matches the axis of Unity's capsule primitive.
    private struct TurtleState
    {
        public Vector3 position;
        public Quaternion orientation;

        public Vector3 Heading => orientation * Vector3.up;
    }

    private void Start()
    {
        Interpret(word);
    }

    public void Interpret(string w)
    {
        TurtleState state = new TurtleState
        {
            position = transform.position,
            orientation = transform.rotation
        };

        foreach (char symbol in w)
        {
            switch (symbol)
            {
                case 'f':
                    state = Move(state, l);
                    break;

                case 'F':
                    CreateCapsule(state, l);
                    state = Move(state, l);
                    break;

                default:
                    // Symbols without a geometric interpretation are ignored.
                    break;
            }
        }
    }

    // f: advance the turtle l units along its current heading.
    private TurtleState Move(TurtleState s, float distance)
    {
        s.position += s.Heading * distance;
        return s;   // struct: returns an updated copy
    }

    // F (geometry part): capsule spanning from the turtle's position
    // to the point l units ahead.
    private void CreateCapsule(TurtleState s, float height)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.SetParent(transform, worldPositionStays: true);

        // Unity's default capsule: height 2, radius 0.5, centred at its origin,
        // aligned with local Y. So we place it at the segment midpoint...
        capsule.transform.position = s.position + s.Heading * (height / 2f);
        capsule.transform.rotation = s.orientation;

        // ...and scale it: radius 0.5 * 2 = 1, height 2 * (height / 2) = height.
        capsule.transform.localScale = new Vector3(2f, height / 2f, 2f);
    }
}
