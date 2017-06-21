/* * Handle all operations of the tomato object
 *  - to be used inside the tomato prefab -  */ 
public enum TomatoState{
	notActive,Active,	Attached,	Dropped,Inbox
}

public class TomatoHandler : MonoBehaviour {

	void Start () {

		gameController = GameController.GetGameController();
		tomatoMaker = GameObject.Find("Control").GetComponent<TomatoMaker>();
		number = tomatoMaker.newTomatoNumber;
		
		WingAnim = transform.GetComponent<Animation>();		// Find and assign the wings animation component
		WingAnim.wrapMode = WrapMode.Once;					// Set the animation to play once on play
		
		// Find and assign the selection animation component
		SelectedAnim = transform.Find("ActivedAnimation").GetComponent<Animation>();
		SelectedAnim.wrapMode = WrapMode.Once;
		gameObject.SampleAnimation(SelectedAnim.clip, 0);		//Rewind the selection animation to Frame 0
		
		// Create the array to hold all the wing flap sound clips
		wingsFlapOnceArray = new AudioClip[4];
		wingsFlapOnceArray[0] = wingsFlapOnce0;
		wingsFlapOnceArray[1] = wingsFlapOnce1;
		wingsFlapOnceArray[2] = wingsFlapOnce2;
		wingsFlapOnceArray[3] = wingsFlapOnce3;
		
		timeOfLastFlap = Time.time - flapInterval;			// Set the timeOfLastFlap to ensure on start the tomato will flap
		
		// Tomatoes start from the BoxEntrance position holder
		transform.position = GameObject.Find("BoxEntrance").transform.position;

		// Instantiate variables
		state = TomatoState.notActive;
		currentPosition = transform.position;
		oldPosition = transform.position;

		speed = Vector3.up*tomatoSpeed*.7f;					// Initialize an upwards speed for the tomatoes to go out of the box
		// Randomize Z factor by a small factor so that tomatoes seam in front of each other in a random order
		Vector3 ZOffsetPosition = transform.position;
		ZOffsetPosition.z = ZOffsetPosition.z + Random.Range(-0.1f,0.1f);	
		transform.position = ZOffsetPosition;
		gravity = Vector3.down*gravityForce;

		// Set the correct number
		TextMesh myText = GetComponentInChildren<TextMesh>();
		myText.text = number.ToString();
		
		// Find the target object in Scene (objects are named PositionHolder# where # is in range 0-9)
		targetPosition = (GameObject.Find("PositionHolder" + number.ToString()) ?? this.gameObject).transform;
		musicSource = GetComponent<AudioSource>();
	
	}
	
	/*
	 *	On Update do: 
	 *	>>Attached?
	 *		- Check if tomato should be discarted (average speed above given threshold
	 * 		- Check if tomato should be put in the box
	 * 		- Move (follow hand)
	 * 	>>Not Attached?
	 * 		- Check if it should be attached to players hand
	 * 		- Move (to position / around the position)
	 */	
	void Update () {
		/*
		 * Movement Handler
		 */
		currentPosition = transform.position;	// Set the current position of the Tomato

		switch (state) {
			case TomatoState.notActive:			// Try to reach target
				applyGravity();
				if (speed.y<0)					// If tomato is falling
					flapWings();
					applySpeed();
				// If the tomato goes close to the target set as Active
				if (Vector3.Distance(currentPosition, targetPosition.position)<1f){
					musicSource.clip = selected;
					musicSource.Play();
					state = TomatoState.Active;
					SelectedAnim.Play();		// Play the animation for selection	
					tomatoSpeed *= 0.3f;
					}
				break;
			case TomatoState.Active:			// Try to keep still near target
				applyGravity();
				// If tomato is really falling & is below the target
				if (speed.y<0.5f && currentPosition.y < targetPosition.position.y)				
					flapWings();
					applySpeed();
				break;
			case TomatoState.Attached:			// Stay attached
					moveToParentHolder();		// Move to the parentHolder position
				break;
			case TomatoState.Dropped:			// Drop
					applyGravity();				// Apply gravitational force to speed
					applySpeed();				// Apply the speed to position
					checkForDeletion();			// Check whether the object is off screen and delete if so
				break;
			case TomatoState.Inbox:				// Destroy Self
				Destroy(this.gameObject);		// Kill the gameObject (Tomato)
				break;
		}
	 
		 // If the tomato is attached to either hand Check whether the average speed indicates it should be thrown away

		if (state.Equals(TomatoState.Attached)) 
		{
			// Calculate the average speed over the last 10 frames (units/sec)
			// Frame-to-frame speed values are kept in an Array (speedValues) which is limited to 10 elements
			float averageSpeed = 0;
			currentPosition = transform.position;
			speedValues.Insert(0,(float) Mathf.Abs(currentPosition.x - oldPosition.x)/Time.deltaTime);		// Insert the new speed first
			if (speedValues.Count>10) speedValues.RemoveAt(10);												// Remove the last element
			foreach(float speed in speedValues){															// Calculate the average speed
				averageSpeed += speed/10;
			}	
			
			// If the average speed in the duration of the last 10 frames exceeds 
			// the input threshold, discard the tomato object  
			if (averageSpeed  >= speedThreshold)
			{
				musicSource.clip = canceled;
				musicSource.Play();
				setSpeed();							// Set the speed variable to the actual frame-to-frame speed
				state = TomatoState.Dropped;
			}
		
		}
		
		oldPosition = currentPosition;				// The old position will be the current position		

		Ray myFrontRay = new Ray(transform.position,Vector3.back);
		Ray myBacktRay = new Ray(transform.position,-1*Vector3.back);
		if (Physics.Raycast(myFrontRay,out hittingFront,100f) || Physics.Raycast(myBacktRay,out hittingBack,100f))
		{
			// Get the hitting object that got the trigger (prefer front)
			if (hittingFront.transform != null) hitting = hittingFront;
			else hitting = hittingBack;
			
			if (state.Equals(TomatoState.Active) && hitting.transform.tag == "SelectingBodyPart" && hitting.transform.GetComponentInChildren<ItemInHand>()!=null && hitting.transform.GetComponentInChildren<ItemInHand>().containing == null )
			{
				musicSource.clip = selected;
				musicSource.Play();
				state = TomatoState.Attached;	
				gameObject.SampleAnimation(SelectedAnim.clip, 0);		// Set the selected animation to frame 0 (nothing visible)
				parentHolder = hitting.transform;
				// Find the ItemInHand script on the hand and set the containing object
				parentHolder.GetComponentInChildren<ItemInHand>().containing = this.transform;
				tomatoMaker.setInstantiated(number, false);				// Create a new tomato
			}
		
			// The tomato is already attached to the hand , now just check that you are hitting the basket 
			if (state.Equals(TomatoState.Attached) && hitting.transform.name == "Select")
			{
				// Put the value in the tomato in the basket 
				hitting.transform.GetComponent<BoxContaining>().AddValue(number);
				parentHolder = hitting.transform;	// Set the box as the parent holder
				moveToParentHolder();				// Move to the parent holder position
				state = TomatoState.Inbox;
			}
		}	
	}
	
	// Flap wings to reach the target
	private void flapWings(){
		// If enough (flapInterval) time has passed since last flap
		if (Time.time > timeOfLastFlap + flapInterval) 
		{
			float flapStrenght = tomatoSpeed*(Random.Range(0.66f, 1.2f));			// Compute random speed
			// Construct a random vector
			Vector3 random;
			random.x = Random.Range(-1.0f, 1.0f);random.y = Random.Range(-1.0f, 1.0f);random.z = Random.Range(-1.0f, 1.0f);
			Vector3 target = targetPosition.position;								// Get target position
			Vector3 direction = currentPosition - target;							// Find the vector towards the target
			direction.Normalize();													// Normalize
			random.Scale(new Vector3(1f, 0.2f, 0.5f));								// Add randomness to the direction vector
			direction = direction - random;	
			direction.Scale(new Vector3(flapStrenght,flapStrenght,flapStrenght));	// Apply random speed
			direction.Scale(new Vector3(0.2f, 1.5f, 0.0f));							// Scale the speed on the X,Y axis
			direction.y += 1.5f*speed.y;											// Add more to vertical speed vector to negate downforce
			direction.y = Mathf.Min(0,direction.y);									// Flapping can only result to an upwards boost
			speed = speed - direction;												// Apply the direction Vector to the tomato speed
			WingAnim.Stop();														// Rewind the wing animation
			gameObject.SampleAnimation(WingAnim.clip, 0);
			WingAnim.Play();														// Play the wing animation
			musicSource.clip = wingsFlapOnceArray[Random.Range(0,3)];				// Play a random wings flap once clip
			musicSource.volume = Random.Range(3.5f, 6.0f);							// Randomize the sound volume of the wings flapping
			musicSource.Play();
			timeOfLastFlap = Time.time;												// Set the time of last flap
		}
	}
	
	// Move to Parent Holder
	private void moveToParentHolder(){
		if (parentHolder!=null){
			this.transform.position = parentHolder.position;
		} else {
			// Player went out of the screen - drop tomato
			musicSource.clip = canceled;
			musicSource.Play();
			setSpeed();							// Set the speed variable to the actual frame-to-frame speed
			//parentHolder.GetComponentInChildren<ItemInHand>().containing = null;	// Set the hand not to contain/hold the tomato
			state = TomatoState.Dropped;			
		}
	}
	
	// Calculate current speed and set the global variable
	private void setSpeed(){
		speed  = (currentPosition - oldPosition)/Time.deltaTime;
		if (speed!=Vector3.zero)
			Debug.Log(speed);
	}
	
	// Apply the gravitational force to the tomato speed
	private void applyGravity(){
		speed += gravity*Time.deltaTime;
		speed.x *= Mathf.Pow(0.6f, Time.deltaTime);		// The speed on the X axis slowly detiorates (40% each second)
	}
	
	// Move the tomato according to its speed
	private void applySpeed(){
		transform.position += speed*Time.deltaTime;
	}
	
	// Check if the tomato is off bounds and delete if so
	private void checkForDeletion(){
		Vector3 pos = transform.position;
		musicSource.clip = canceled;
		musicSource.Play();
		if (pos.y<-2)
			Destroy(this.gameObject);		// Tomato is below the floor}
}
