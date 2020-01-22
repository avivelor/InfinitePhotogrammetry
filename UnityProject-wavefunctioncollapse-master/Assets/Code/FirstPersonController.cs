using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {

	[Range(1f, 5f)]
	public float MovementSpeed = 1f;

	[Range(1, 200f)]
	public float LookSensitivity = 50f;

	[Range(1, 100f)]
	public float JumpStrength = 2f;

	private bool bIsInNoClipMode = false;
	private bool bIsInfoPanelHidden = false;

	private CharacterController characterController;
	private Transform cameraTransform;

	private float cameraTilt = 0f;
	private float verticalSpeed = 0f;
	private float timeInAir = 0f;
	private bool jumpLocked = false;

	void Start () {
		this.characterController = this.GetComponent<CharacterController>();
		this.cameraTransform = this.GetComponentInChildren<Camera>().transform;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;
	}
	
	void Update () {
		bool touchesGround = this.onGround();
		float runMultiplier = 1f + 2f * Input.GetAxis("Run");
		float y = this.transform.position.y;
		Vector3 movementVector = this.transform.forward * Input.GetAxis("Move Y") + this.transform.right * Input.GetAxis("Move X");
		if (movementVector.sqrMagnitude > 1) { // this check prevents partial joystick input from becoming 100% speed
			movementVector.Normalize();  // this prevents diagonal movement form being too fast
		}
		this.characterController.Move(movementVector * Time.deltaTime * this.MovementSpeed * runMultiplier);
		float verticalMovement = this.transform.position.y - y;
		if (verticalMovement < 0) {
			this.transform.position += Vector3.down * verticalMovement;
		}

		if((Input.GetMouseButton(0)))
		{
			this.transform.rotation = Quaternion.AngleAxis(Input.GetAxis("Look X") * Time.deltaTime * this.LookSensitivity*3f, Vector3.up) * this.transform.rotation;
			this.cameraTilt = Mathf.Clamp(this.cameraTilt - Input.GetAxis("Look Y") * this.LookSensitivity*3f * Time.deltaTime, -90f, 90f);
			this.cameraTransform.localRotation = Quaternion.AngleAxis(this.cameraTilt, Vector3.right);
		}

		/*if(Input.GetMouseButton(0)) {
             transform.Rotate(new Vector3(Input.GetAxis("Look X") * this.LookSensitivity/10, -Input.GetAxis("Look Y") * this.LookSensitivity/10, 0));
             float X = transform.rotation.eulerAngles.x;
             float Y = transform.rotation.eulerAngles.y;
             transform.rotation = Quaternion.Euler(Y, X, 0);
		}*/

		if (touchesGround) {
			this.timeInAir = 0;
		} else {
			this.timeInAir += Time.deltaTime;
		}

		if (touchesGround && this.verticalSpeed < 0) {
			this.verticalSpeed = 0;
		} else {
			this.verticalSpeed -= 9.18f * Time.deltaTime;
		}
		if (Input.GetAxisRaw("Jump") < 0.1f) {
			this.jumpLocked = false;
		}
		if (!this.jumpLocked && this.timeInAir < 0.5f && Input.GetAxisRaw("Jump") > 0.1f) {
			this.timeInAir = 0.5f;
			this.verticalSpeed = this.JumpStrength;
			this.jumpLocked = true;
		}
		if (Input.GetAxisRaw("Jetpack") > 0.1f) {
			this.verticalSpeed = 2f;
		}
		this.characterController.Move(Vector3.up * Time.deltaTime * this.verticalSpeed);

		if (Input.GetKeyDown(FlightController.OnOffKey)) {
			var flyBehaviour = this.GetComponent<FlightController>();
			if (flyBehaviour != null) {
				this.GetComponent<FlightController>().enabled = true;
			}
			this.cameraTilt = 24;
		}

		if(Input.GetKeyDown(KeyCode.X))
		{
			bIsInNoClipMode = !bIsInNoClipMode;
			Physics.IgnoreLayerCollision(0,0, bIsInNoClipMode);
		}

		if(Input.GetKeyDown(KeyCode.H))
		{
			bIsInfoPanelHidden = !bIsInfoPanelHidden;
			if(bIsInfoPanelHidden)
			{
				Button PanelOnButton = GameObject.Find("Button-HidePanel-On").GetComponent<Button>();
				if(PanelOnButton) PanelOnButton.onClick.Invoke();
			}
			else
			{
				Button PanelOnButton = GameObject.Find("Button-HidePanel-Off").GetComponent<Button>();
				if(PanelOnButton) PanelOnButton.onClick.Invoke();
			}
		}

		if(Input.GetKeyDown(KeyCode.O))
		{
			SceneManager.LoadScene("StaticHouse", LoadSceneMode.Single);
		}

		if(Input.GetKeyDown(KeyCode.I))
		{
			SceneManager.LoadScene("GameMeshScans", LoadSceneMode.Single);
		}

		if(Input.GetKeyDown(KeyCode.R))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	public void Enable() {
		this.verticalSpeed = 0;
	}

	private bool onGround() {
		var ray = new Ray(this.transform.position, Vector3.down);
		return Physics.SphereCast(ray, this.characterController.radius, this.characterController.height / 2 - this.characterController.radius + 0.1f);
	}
}
