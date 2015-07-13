using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
	// static
	public static int tileIndex;

	private AudioSource wrongMove;

	// const
	private float tileDistance = 1.856f;
	private int threshold = 1;

	private IList<Tile> adjacents;
	private Tile[] boardTiles;
	
	private Vector3 screenPoint;

	private Vector3 lowerBound;
	private Vector3 upperBound;

	private bool isHoriz = false;
	private Vector2 startIndex;
	private Vector2 endIndex;

	//Directions
	private Vector2 direction = Vector2.zero;
	private Vector2 UP = Vector2.up;
	private Vector2 DOWN = -1 * Vector2.up;
	private Vector2 RIGHT = Vector2.right;
	private Vector2 LEFT = -1 * Vector2.right;
	private Vector2 NONE = Vector2.zero;

	//Controllers
	private Controller[] knights;

	public bool dragging = false;
	private Color selected;
	private Color original;

	private bool turnEnded = false;

	private Vector3 inPlaceBound;
	private Vector3 adjacentBound;

	private ScoreTracker scoreTracker;


	void Start ()
	{
		this.gameObject.layer = LayerMask.NameToLayer ("Tile");
		this.adjacents = new List<Tile> ();
		this.boardTiles = GameObject.FindObjectsOfType<Tile> ();
		this.knights = GameObject.FindObjectsOfType<Controller> ();
		this.original = this.GetComponent<SpriteRenderer> ().color;
		this.scoreTracker = GameObject.FindObjectOfType<ScoreTracker> ();
		Color.TryParseHexString ("979067", out selected);
		this.wrongMove = GameObject.FindGameObjectWithTag ("error").GetComponent<AudioSource> ();

		tileIndex = 1 << LayerMask.NameToLayer ("Tile");
	}

	void Update ()
	{
		if (!IsInPlace ()) {
			dragging = true;
			this.GetComponent<SpriteRenderer> ().color = selected;
		} else {
			dragging = false;
			this.GetComponent<SpriteRenderer> ().color = original;
		}

	}

	void OnMouseDown ()
	{
		direction = GetMoveDirection ();
		isHoriz = IsHoriz ();
		UpdateBounds ();
		screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
		this.dragging = true;
		turnEnded = false;
		startIndex = GetCoordinate ();
	}

	private void KnightsMoving (bool isMoving)
	{
		foreach (Controller knight in knights) {
			knight.SetMoving (isMoving);
		}
	}

	private bool IsHoriz ()
	{
		return direction == RIGHT || direction == LEFT;
	}

	
	private void UpdateBounds ()
	{
		Vector2 coordinate = GetCoordinate ();
		bool isPosDir = (direction.x + direction.y) > 0;
		inPlaceBound = CoordinateToPosition (coordinate);
		adjacentBound = CoordinateToPosition (coordinate + direction);
		lowerBound = isPosDir ? inPlaceBound : adjacentBound;
		upperBound = isPosDir ? adjacentBound : inPlaceBound;
	}

	private Vector2 GetMoveDirection ()
	{
		Vector2 slot = FindEmptyTileCoordinate (boardTiles);
		Vector2 me = this.GetCoordinate ();
		bool sameRow = me.y == slot.y;
		bool sameCol = me.x == slot.x;
		bool sameRowOrCol = sameRow || sameCol;

		if (!sameRowOrCol) {
			return NONE;
		}

		if (sameCol) {
			return slot.y > me.y ? UP : DOWN;
		}

		return slot.x > me.x ? RIGHT : LEFT;

	}

	private Vector2 GetCoordinate ()
	{
		return PositionToCoordinate (this.transform.position);
	}

	private void AddNextTile (Tile t)
	{
		Tile adjacentTile = GetAdjacentTile (t);
		if (adjacentTile != null) {
			Vector3 tilePos = t.transform.position;
			Vector3 adjacentPos = adjacentTile.transform.position;
			float d = Vector3.Distance (tilePos, adjacentPos);
			if (d < tileDistance) {
				adjacentTile.transform.position = new Vector3 (tilePos.x + (tileDistance * direction.x), tilePos.y + (tileDistance * direction.y), 0);
				adjacentPos = adjacentTile.transform.position;
				d = Vector3.Distance (tilePos, adjacentPos);
			}
			if (Mathf.Approximately (tileDistance, d)) {
				adjacents.Add (adjacentTile);
				AddNextTile (adjacentTile);
			}
		}
	}

	void OnMouseDrag ()
	{
		if (!turnEnded) {
			//Touch touch = Input.GetTouch (0);
			Vector3 currentScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
			Vector3 currentPosition = Camera.main.ScreenToWorldPoint (currentScreenPoint);

			DetatchChildren ();
			if (!adjacents.Contains (this)) {
				adjacents.Add (this);
			}
			AddNextTile (this);

			if (!HasKnightChild ()) {
				
				Vector3 difference = currentScreenPoint - screenPoint;
				difference.Normalize ();
				float distance = Vector3.Distance (screenPoint, currentScreenPoint);
				if (distance < threshold) {
					return;
				}
				
				float f = isHoriz ? Mathf.RoundToInt (Vector3.Dot (difference, RIGHT)) : Mathf.RoundToInt (Vector3.Dot (difference, UP));
				int dIndex = isHoriz ? Mathf.RoundToInt (direction.x) : Mathf.RoundToInt (direction.y);
				Vector3 newPosition = UpdatePosition (transform.position, currentPosition);
				
				foreach (Tile tile in adjacents) {
					UpdateKnights (tile);
					if (f == dIndex) {
						tile.transform.parent = transform;
					}
					if (f == -dIndex) {
						ChangeDirection ();
					}
				}
				transform.position = newPosition;
				screenPoint = currentScreenPoint;
				float d = Vector3.Distance (newPosition, adjacentBound);
				if (d < 0.05f) {
					turnEnded = true;
					EndTurn ();
				}

			} else {
				PlayWrongMoveSound ();
				EndTurn ();
			}

				
		}
	} 

	private void PlayWrongMoveSound ()
	{
		if (!wrongMove.isPlaying) {
			wrongMove.Play ();
		}
	}

	bool HasKnightChild ()
	{
		foreach (Tile t in adjacents) {
			if (t.GetComponentInChildren<Controller> () != null) {
				return true;
			}
		}
		return this.GetComponentInChildren<Controller> () != null;
	}

	private bool IsInPlace ()
	{
		Vector3 inPlace = CoordinateToPosition (this.GetCoordinate ());
		return Vector3.Distance (transform.position, inPlace) < 0.1f;
	}

	public bool isDragging ()
	{
		return dragging;
	}

	void UpdateKnights (Tile tile)
	{
		foreach (Controller knight in knights) {
			if (knight.GetComponentInParent<Tile> () == tile || knight.GetNextTile () == tile) {
				knight.SetMoving (false);
			}

		}

	}

	private void ChangeDirection ()
	{
		direction = -1 * direction;
	}

	private Vector3 UpdatePosition (Vector3 position, Vector3 newPosition)
	{
		if (isHoriz) {
			position.x = newPosition.x;
			if (position.x > upperBound.x) {
				position.x = upperBound.x;
			}
			if (position.x < lowerBound.x) {
				position.x = lowerBound.x;
			}
		} else {			
			position.y = newPosition.y;
			if (position.y >= upperBound.y) {
				position.y = upperBound.y;
			}
			if (position.y <= lowerBound.y) {
				position.y = lowerBound.y;
			}
		}
		return position;
	}

	void OnMouseUp ()
	{
		if (!turnEnded) {
			EndTurn ();
		}

	}

	private void EndTurn ()
	{
		endIndex = GetCoordinate ();
		if (startIndex != endIndex) {
			scoreTracker.IncrementCurrentScore ();
		}
		SnapToPosition ();
		DetatchChildren ();
		SetDraggingFalse ();
	}

	void SetDraggingFalse ()
	{
		foreach (Tile t in boardTiles) {
			t.dragging = false;
		}
	}


	void DetatchChildren ()
	{
		Board board = GameObject.FindObjectOfType<Board> ();
		foreach (Tile tile in adjacents) {
			if (tile != null) {
				tile.transform.parent = board.transform;
			}
		}
		adjacents = new List<Tile> ();
	}

	private void SnapToPosition ()
	{
		foreach (Tile tile in GameObject.FindObjectsOfType<Tile>()) {
			tile.transform.position = CoordinateToPosition (tile.GetCoordinate ());
		}
	}

	private Vector2 PositionToCoordinate (Vector3 position)
	{
		return new Vector2 (Mathf.RoundToInt (position.x / tileDistance), Mathf.RoundToInt (position.y / tileDistance));
	}

	private Vector3 CoordinateToPosition (Vector2 coordinate)
	{
		return new Vector3 (coordinate.x * tileDistance, coordinate.y * tileDistance, 0);
	}

	private Vector2 FindEmptyTileCoordinate (Tile[] allTiles)
	{
		IList<Vector2> coords = GetAllTileCoordinates (allTiles);
		for (int i=-2; i<3; i++) {
			for (int j=-2; j<3; j++) {
				Vector2 coord = new Vector2 (i, j);
				if (!coords.Contains (coord)) {
					return coord;
				}
			}
		}
		//TODO Handle this better
		Debug.Log ("Empty location not found, something went wrong");
		return Vector2.zero;
	}

	private IList<Vector2> GetAllTileCoordinates (Tile[] allTiles)
	{
		IList<Vector2> coords = new List<Vector2> ();
		foreach (Tile tile in allTiles) {
			coords.Add (tile.GetCoordinate ());
		}
		return coords;
	}

	private Tile GetAdjacentTile (Tile t)
	{

		RaycastHit2D me = Physics2D.Raycast (t.transform.position, direction, 0.1f, tileIndex);
		me.collider.enabled = false;

		RaycastHit2D next = Physics2D.Raycast (t.transform.position, direction, tileDistance / 2 + 0.1f, tileIndex);
		if (next.collider == null) {
			me.collider.enabled = true;
			return null;
		}
		me.collider.enabled = true;
		return next.collider.gameObject.GetComponent<Tile> ();
	}

}
