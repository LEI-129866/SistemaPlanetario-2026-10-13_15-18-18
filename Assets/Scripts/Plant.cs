using UnityEngine;

public class Plant : MonoBehaviour
{
	/**
    * How many times have the rules been applied to the initial stem.
    */
	[SerializeField] int age = 5;
	[SerializeField] float delta = 30;
	[SerializeField] float distance = 1;
	[SerializeField] GameObject treeTrunk;
   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
      GameObject stem = GameObject.CreatePrimitive (PrimitiveType.Capsule);
      stem.transform.parent = this.transform;
      stem.transform.localPosition = Vector3.zero;
		this.loop (this.rewrite ("F", this.age), this.transform, true, 0, new State ());
   }

   // Update is called once per frame
   void Update()
   {
        
   }

   private string rewrite (string current, int n)
   {
      if (n <= 0) {
         return current;
      }
      else {
         string result = "";
         foreach (char c in current) {
            if (c == 'F')
					result += rewrite ("[+F]F[-F]F", n - 1);
					// result += rewrite ("FF", n - 1);
				else
					result += c;
         }
         return result;
      }
   }

	private int loop (string word, Transform transform, bool firstPrefab, int from, State state)
	{
		int i = from;
		while (i < word.Length) {
			char ch = word[i];
			switch (ch) {
			case 'f':
				state.translation += this.distance;
				break;
			case '+':
				state.angleZ += this.delta;
				break;
			case '-':
				state.angleZ -= this.delta;
				break;
			case '[':
				i = this.loop (word, transform, firstPrefab, i + 1, new State (state));
				break;
			case ']':
				return i;
			case 'F':
				GameObject go = Instantiate (this.treeTrunk);
				go.transform.parent = transform;
				if (firstPrefab) {
					firstPrefab = false;
					go.transform.localPosition = UnityEngine.Vector3.zero;
				}
				else
					go.transform.localPosition = new UnityEngine.Vector3 (0, this.distance, 0);
				go.transform.localRotation = UnityEngine.Quaternion.Euler (0, 0, state.angleZ);
				transform = go.transform;
				state.reset ();
				break;
			}
			i++;
		}
		return word.Length;
	}

}


public class State
{
	public float angleZ = 0;
	public float translation = 0;
	public State () { }
	public State (State s)
	{
		this.angleZ = s.angleZ;
		this.translation = s.translation;
	}
	public void reset ()
	{
		this.angleZ = 0;
	}
}
