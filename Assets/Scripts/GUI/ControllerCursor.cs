using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ControllerCursor : MonoBehaviour
{
	public Selectable currentSelected;
	Transform thisTransform;
	RectTransform rectTransform;
	public Vector3 desiredPos;
	Image cursorSprite;
	float moveTime;
	bool isMoving = false;

	protected Vector2 inputDir;

	private Selectable useSelectable = null;
	private Selectable cancelSelectable = null;

	// Use this for initialization
	void Start ()
	{
		thisTransform = this.transform;
		rectTransform = GetComponent<RectTransform> ();
		cursorSprite = GetComponent<Image> ();
		FindNewSelectable ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateInput ();
		desiredPos = currentSelected.transform.position;
		thisTransform.position = Vector3.Lerp (thisTransform.position, desiredPos, (Time.time - moveTime) * 3);

		if (inputDir.magnitude > 0.75f) {
			if (isMoving == false) {
				MoveSelected (inputDir);
			}
			isMoving = true;

		} else if (inputDir.magnitude < 0.2f) {
			isMoving = false;
		}

	}

	public void FindNewSelectable ()
	{
		for (int i = 0; i < Selectable.allSelectables.Count; i++) {
			if (Selectable.allSelectables [i].gameObject.activeSelf) {
				currentSelected = Selectable.allSelectables [i];
				currentSelected.OnPointerEnter (null);
				return;
			}
		}

	}

	protected abstract void UpdateInput ();


	/// <summary>
	/// Fakes a left click down the current selected component
	/// </summary>
	protected void UseStart ()
	{
		useSelectable = currentSelected;
		PointerEventData data = new PointerEventData (EventSystem.current);
		data.button = PointerEventData.InputButton.Left;
		data.pointerPress = currentSelected.gameObject;
		data.clickCount = 1;
		ExecuteEvents.Execute (currentSelected.gameObject, data, ExecuteEvents.pointerDownHandler);
		//currentSelected.OnPointerDown (data);
	}

	/// <summary>
	/// Fakes a left click up over the current selected component
	/// </summary>
	protected void UseEnd ()
	{
		PointerEventData data = new PointerEventData (EventSystem.current);
		data.button = PointerEventData.InputButton.Left;
		data.pointerPress = currentSelected.gameObject;
		data.clickCount = 1;

		if (useSelectable == currentSelected) {
			ExecuteEvents.Execute (currentSelected.gameObject, data, ExecuteEvents.pointerClickHandler);
		} else {
			ExecuteEvents.Execute (useSelectable.gameObject, data, ExecuteEvents.pointerUpHandler);
		}
		ExecuteEvents.Execute (currentSelected.gameObject, data, ExecuteEvents.pointerUpHandler);
		useSelectable = null;
		//currentSelected.OnPointerUp (data);
	}

	/// <summary>
	/// Fakes a right click down over the current selected component
	/// </summary>
	protected void CancelStart ()
	{
		PointerEventData data = new PointerEventData (EventSystem.current);
		data.button = PointerEventData.InputButton.Right;
		data.pointerPress = currentSelected.gameObject;
		data.clickCount = 1;
		currentSelected.OnPointerDown (data);
	}

	/// <summary>
	/// Fakes a right click up over the current selected component
	/// </summary>
	protected void CancelEnd ()
	{
		PointerEventData data = new PointerEventData (EventSystem.current);
		data.button = PointerEventData.InputButton.Right;
		data.pointerPress = currentSelected.gameObject;
		data.clickCount = 1;
		if (cancelSelectable == currentSelected) {
			Button but = (Button)currentSelected;
			if (but) {
				but.OnPointerClick (data);
			}
		} else {
			cancelSelectable.OnPointerUp (data);
		}
		cancelSelectable = null;
		currentSelected.OnPointerUp (data);
	}

	void MoveSelected (Vector2 dir)
	{
		Selectable nextSelected = currentSelected.FindSelectable (dir);
		if (nextSelected != null) {
			currentSelected.OnPointerExit (null);
			currentSelected = nextSelected;
			currentSelected.OnPointerEnter (null);
			moveTime = Time.time;
		}
	}
}
