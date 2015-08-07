using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageHover : MonoBehaviour {

	public Color colorImageHightlight;
	public GameObject goUnitToHightlight;
	public Color colorUnitHighlight;

	RectTransform thisRectTrans;
	Image thisImage;
	Color imageOriColor;

	//For the unit to be highlighted
	Vector3[] worldCorners;
	SpriteRenderer unitToHighlight;
	Color spriteOriColor;

	bool resetColor = false;

	void Start() {
		thisRectTrans = gameObject.GetComponent<RectTransform>();
		thisImage = GetComponent<Image>();
		imageOriColor = thisImage.color;

		worldCorners = new Vector3[4];
		unitToHighlight = goUnitToHightlight.GetComponent<SpriteRenderer>();
		spriteOriColor = unitToHighlight.color;
	}

	void Update () {
		if(!IsMouseOverImage())
			return;

		
		unitToHighlight.color = colorUnitHighlight;
		thisImage.color = colorImageHightlight;

		resetColor = true;
	}

	public bool IsMouseOverImage() {
		thisRectTrans.GetWorldCorners(worldCorners);
		Vector2 mousePosition = Input.mousePosition;
		
		if(mousePosition.x >= worldCorners[0].x && mousePosition.x < worldCorners[2].x 
		   && mousePosition.y >= worldCorners[0].y && mousePosition.y < worldCorners[2].y) {
			return true;
		}

		if(resetColor) {
			unitToHighlight.color = spriteOriColor;
			thisImage.color = imageOriColor;
			resetColor = false;
		}

		return false;
	}    

}

