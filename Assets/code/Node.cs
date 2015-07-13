using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour
{
	public Type type;
	public enum Type
	{
		Wall,
		Path,
		Home,
		Slot,
		Table
	}
	;


	void Start ()
	{
		this.gameObject.layer = LayerMask.NameToLayer ("Node");
	}

	public Type GetTileType ()
	{
		return this.type;
	}

	public static bool CanTraverse (Node node)
	{
		return node.type == Type.Path || node.type == Type.Home;
	}
}
