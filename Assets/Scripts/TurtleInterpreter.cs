using System.Collections.Generic;
using UnityEngine;

// Interprets an L-system word with a 3D turtle.
public class TurtleInterpreter : MonoBehaviour
{
    private struct TurtleState
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Transform currentFrame;
        public Transform lastBodyInFrame;
        public Vector3 lastBodyLocalPos;

        public Vector3 Heading => localRotation * Vector3.forward;
        public Vector3 Left    => localRotation * Vector3.left;
        public Vector3 Up      => localRotation * Vector3.up;
    }

    // Interpreta a palavra da Fase 2 usando os parâmetros do SimulationConfig e constrói a cena via SceneBuilder
    public void Interpret(List<Symbol> word, SimulationConfig config, Transform rootFrame, SceneBuilder sceneBuilder)
    {
        if (word == null || config == null || rootFrame == null || sceneBuilder == null) return;

        Stack<TurtleState> stateStack = new Stack<TurtleState>();

        // Estado inicial (Secção 1.1): origem de rootFrame, h = z, l = -x, u = y
        TurtleState state = new TurtleState
        {
            localPosition = Vector3.zero,
            localRotation = Quaternion.identity,
            currentFrame = rootFrame,
            lastBodyInFrame = null,
            lastBodyLocalPos = Vector3.zero
        };

        float c = config.stepDistance;
        float angleDeg = config.rotationAngle * Mathf.Rad2Deg; // Unity usa graus nas rotações

        foreach (Symbol symbol in word)
        {
            switch (symbol.Id)
            {
                // --- 1.2.1 Estado da Tartaruga ---
                case "ff":
                    state.localPosition += state.Heading * c;
                    break;

                case "y+": // Vira para a direita a radianos (em torno de u)
                    state.localRotation *= Quaternion.AngleAxis(angleDeg, Vector3.up);
                    break;

                case "y-": // Vira para a esquerda a radianos (em torno de u)
                    state.localRotation *= Quaternion.AngleAxis(-angleDeg, Vector3.up);
                    break;

                case "x+": // Inclina para baixo a radianos (em torno de l)
                    state.localRotation *= Quaternion.AngleAxis(angleDeg, Vector3.right);
                    break;

                case "x-": // Inclina para cima a radianos (em torno de l)
                    state.localRotation *= Quaternion.AngleAxis(-angleDeg, Vector3.right);
                    break;

                case "z+": // Rola para a esquerda a radianos (em torno de h)
                    state.localRotation *= Quaternion.AngleAxis(angleDeg, Vector3.forward);
                    break;

                case "z-": // Rola para a direita a radianos (em torno de h)
                    state.localRotation *= Quaternion.AngleAxis(-angleDeg, Vector3.forward);
                    break;

                // --- 1.2.2 Tartaruga e Sistemas de Referência ---
                case "[":
                case "[_":
                    // Guarda estado atual e Fa na pilha
                    stateStack.Push(state);

                    // Cria novo sistema de referência Fn
                    GameObject newFrameObj = new GameObject("RefFrame_Fn");
                    Transform newFrame = newFrameObj.transform;

                    if (state.lastBodyInFrame != null)
                    {
                        // Fn acompanha a órbita do último astro criado em Fa, mantendo o deslocamento relativo
                        newFrame.SetParent(state.lastBodyInFrame, worldPositionStays: false);
                        newFrame.localPosition = state.localPosition - state.lastBodyLocalPos;
                        newFrame.rotation = state.currentFrame.rotation * state.localRotation;
                    }
                    else
                    {
                        // Se não houver astro anterior em Fa, posiciona diretamente no referencial Fa
                        newFrame.SetParent(state.currentFrame, worldPositionStays: false);
                        newFrame.localPosition = state.localPosition;
                        newFrame.localRotation = state.localRotation;
                    }

                    // Regista no SceneBuilder para ser limpo quando mudar de passo
                    sceneBuilder.RegisterSpawnedObject(newFrameObj);

                    // Reinicia tartaruga no novo referencial Fn
                    state = new TurtleState
                    {
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity,
                        currentFrame = newFrame,
                        lastBodyInFrame = null,
                        lastBodyLocalPos = Vector3.zero
                    };
                    break;

                case "]":
                case "_]":
                    if (stateStack.Count > 0)
                    {
                        state = stateStack.Pop();
                    }
                    break;

                // --- 1.2.3 Objetos Estelares ---
                case "Nv":
                case "Eb":
                case "Ea":
                case "G1":
                case "Pc":
                case "Pb":
                case "Pa":
                case "R1":
                case "La":
                    Transform createdBody = sceneBuilder.SpawnStellarObject(
                        symbol,
                        config,
                        state.currentFrame,
                        state.localPosition,
                        state.localRotation
                    );

                    // Guarda o último astro sólido criado neste referencial (ignora nuvens e anéis)
                    if (createdBody != null && symbol.Id != "Nv" && symbol.Id != "G1" && symbol.Id != "R1")
                    {
                        state.lastBodyInFrame = createdBody;
                        state.lastBodyLocalPos = state.localPosition;
                    }
                    break;

                default:
                    break;
            }
        }
    }

    //private void Start()
    //{
    //    Interpret(word);
    //}

    //public void Interpret(string w)
    //{
    //    TurtleState state = new TurtleState
    //    {
    //        position = transform.position,
    //        orientation = transform.rotation
    //    };

    //    foreach (char symbol in w)
    //    {
    //        switch (symbol)
    //        {
    //            case 'f':
    //                state = Move(state, l);
    //                break;

    //            case 'F':
    //                CreateCapsule(state, l);
    //                state = Move(state, l);
    //                break;

    //            default:
    //                // Symbols without a geometric interpretation are ignored.
    //                break;
    //        }
    //    }
    //}

    //// f: advance the turtle l units along its current heading.
    //private TurtleState Move(TurtleState s, float distance)
    //{
    //    s.position += s.Heading * distance;
    //    return s;   // struct: returns an updated copy
    //}

    //// F (geometry part): capsule spanning from the turtle's position
    //// to the point l units ahead.
    //private void CreateCapsule(TurtleState s, float height)
    //{
    //    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    //    capsule.transform.SetParent(transform, worldPositionStays: true);

    //    // Unity's default capsule: height 2, radius 0.5, centred at its origin,
    //    // aligned with local Y. So we place it at the segment midpoint...
    //    capsule.transform.position = s.position + s.Heading * (height / 2f);
    //    capsule.transform.rotation = s.orientation;

    //    // ...and scale it: radius 0.5 * 2 = 1, height 2 * (height / 2) = height.
    //    capsule.transform.localScale = new Vector3(2f, height / 2f, 2f);
    //}
}