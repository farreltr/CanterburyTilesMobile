using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
	public static float STOPPED = 0.0f;
	private Vector2 direction;	
	private Animator animator;
	public static Vector2 RIGHT = new Vector2 (1.0f, 0.0f);
	public static Vector2 LEFT = new Vector2 (-1.0f, 0.0f);
	public static Vector2 UP = new Vector2 (0.0f, 1.0f);
	public static Vector2 DOWN = new Vector2 (0.0f, -1.0f);
	public static Vector2 STOP = new Vector2 (0.0f, 0.0f);
	public Vector3 startDirection;
	new string name;
	private Vector3 endPosition;
	private bool moving = false;
	private float duration = 70f; //at duration = 100 there is a stall at the center tile. Probably due to approximation
	private int nodeLayerIndex;
	private float offset = 0f; // this fucks up raycasting. Need to account for offset

	// Pledge Algorithm
	//private bool rightSensor;
	private bool leftSensor;
	private bool frontSensor;

	private bool isPledge = false;
	private int pledgeCounter = 0;
	private static int COUNT = 0;
	private Node nextNode;

	private Vector3 originalPosition;

	private bool nextNodeHome = false;

	void Start ()
	{
		animator = this.GetComponent<Animator> ();
		direction = startDirection;
		nodeLayerIndex = 1 << LayerMask.NameToLayer ("Node");
	}
	
	void Update ()
	{
		originalPosition = new Vector3 (transform.position.x, transform.position.y - offset, 0);
		UpdateAnimation ();
		UpdateCurrentLocation ();
		if (this.GetComponentInParent<Tile> ().isDragging ()) {
			return;
		}
		if (moving) {
			MoveToNextNode ();
		} else {
			DoSense ();
			if (isPledge) {
				MovePledge ();
			} else {
				MoveNormally ();
			}
		}
	}

	private void UpdateCurrentLocation ()
	{
		RaycastHit2D hit = Physics2D.Raycast (originalPosition, Vector3.up, 0.01f, Tile.tileIndex);
		if (hit.collider != null) {
			transform.parent = hit.collider.transform;
		}
	}

	private void MoveToNextNode ()
	{
		if (!Mathf.Approximately (gameObject.transform.position.magnitude, endPosition.magnitude)) {
			gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, endPosition, 1 / (duration * (Vector3.Distance (gameObject.transform.position, endPosition))));
		} else {
			if (nextNodeHome) {
				TurnOnPortrait (this.gameObject.name);
			}
			moving = false;
		}
	}

	private void TurnOnPortrait (string name)
	{
		foreach (GameObject o in GameObject.FindGameObjectsWithTag (name)) {
			o.SetActive (false);
		}
		COUNT++;
		if (COUNT == 4) {
			GameObject.FindObjectOfType<ScoreTracker> ().Save ();
			LaunchWinScreen ();
		}

	}

	private void LaunchWinScreen ()
	{
		Application.LoadLevel (1);
	}

	private void MoveNormally ()
	{
		if (frontSensor) {
			MoveForward ();
		} else {
			TurnRight (); // obstacle is now on left
			isPledge = true;
		}
	}

	private void MovePledge ()
	{
		if (leftSensor) { // obstacle is not on my left
			TurnLeft ();
			MoveForward ();

		} else { // obstacle is on my left

			if (frontSensor) {
				MoveForward ();
			} else {
				TurnRight ();
			}
		}
		isPledge = pledgeCounter != 0;
	}

	public void DoSense ()
	{
		RaycastHit2D hit = Physics2D.Raycast (originalPosition, UP, 0.1f, nodeLayerIndex);
		if (hit.collider != null) {
			transform.parent = hit.collider.transform;
			hit.collider.enabled = false;
			leftSensor = LookLeft ();
			//rightSensor = LookRight ();
			frontSensor = LookForward ();
			hit.collider.enabled = true;
		} else {
			ChangeDirection ();
		}

	}

	private bool LookLeft ()
	{
		Vector2 direction = GetLeftDirection ();
		float distance = direction == UP ? 0.8f : 0.8f;
		Tile parentTile = this.GetComponentInParent<Tile> ();
		RaycastHit2D hit = Physics2D.Raycast (transform.position, direction, distance, nodeLayerIndex);
		if (hit.collider == null)
			return false;
		Node node = hit.transform.gameObject.GetComponent<Node> ();
		if (parentTile.isDragging () && node.GetComponentInParent<Tile> () == !(parentTile && node.GetTileType () == Node.Type.Path)) {
			return false;
		}
		if (node.GetTileType () == Node.Type.Path && !isDragging (node)) {
			return true;
		}
		return false;
	}

	private bool LookRight ()
	{
		Vector2 direction = GetRightDirection ();
		float distance = direction == UP ? 0.8f : 0.8f;
		RaycastHit2D hit = Physics2D.Raycast (originalPosition, direction, distance, nodeLayerIndex);
		if (hit.collider == null)
			return false;
		Node node = hit.transform.gameObject.GetComponent<Node> ();
		if (node.GetTileType () == Node.Type.Path && !isDragging (node)) {
			return true;
		}
		return false;
	}

	private bool LookForward ()
	{
		float distance = direction == UP ? 0.8f : 0.8f;
		RaycastHit2D hit = Physics2D.Raycast (transform.position, direction, distance, nodeLayerIndex);
		Tile parentTile = this.GetComponentInParent<Tile> ();
		if (hit.collider == null)
			return false;
		Node node = hit.transform.gameObject.GetComponent<Node> ();
		if (node.GetTileType () == Node.Type.Home) {
			if (node.transform.parent.name == this.gameObject.name) {
				nextNode = node;
				nextNodeHome = true;
				return true;
			}
			return false;
		}
		if (parentTile.isDragging () && node.GetComponentInParent<Tile> () == parentTile && !(node.GetTileType () == Node.Type.Path)) {
			return false;
		}
		if (node.GetTileType () == Node.Type.Path && !isDragging (node)) {
			nextNode = node;
			return true;
		}
		return false;
	}

	private bool isDragging (Node node)
	{
		if (node == null) {
			return false;
		}
		Tile parentTile = node.GetComponentInParent<Tile> ();
		if (parentTile != null && parentTile == this.GetComponentInParent<Tile> ()) {
			return false;
		}
		return parentTile.dragging;

	}

	
	void UpdateAnimation ()
	{
		if (direction.y > 0) {
			animator.SetInteger ("direction", 2);
		} else if (direction.y < 0) {
			animator.SetInteger ("direction", 0);
		} else if (direction.x > 0) {
			animator.SetInteger ("direction", 3);
		} else if (direction.x < 0) {
			animator.SetInteger ("direction", 1);
		}
	}
	
	private void ChangeDirection ()
	{
		direction = -1 * direction;
	}
	
	private void Stop ()
	{
		direction = STOP;
		transform.Translate (direction);
	}

	private void MoveForward ()
	{
		DoSense ();
		endPosition = nextNode.transform.position;
		endPosition.y += offset;
		moving = true;
	}

	public Tile GetNextTile ()
	{
		return nextNode.GetComponentInParent<Tile> ();

	}
	
	private void TurnRight ()
	{
		direction = GetRightDirection ();
		pledgeCounter++;
	}

	private Vector3 GetRightDirection ()
	{
		if (direction == RIGHT) {
			return DOWN;
		} else if (direction == LEFT) {
			return UP;
		} else if (direction == UP) {
			return RIGHT;
		} else if (direction == DOWN) {
			return LEFT;
		}
		return STOP;
	}
	
	private void TurnLeft ()
	{
		direction = GetLeftDirection ();
		pledgeCounter--;
		
	}

	public void SetMoving (bool isMoving)
	{
		this.moving = isMoving;
	}

	private Vector3 GetLeftDirection ()
	{
		if (direction == RIGHT) {
			return UP;
		} else if (direction == LEFT) {
			return DOWN;
		} else if (direction == UP) {
			return LEFT;
		} else if (direction == DOWN) {
			return RIGHT;
		}
		return STOP;
	}
	
}