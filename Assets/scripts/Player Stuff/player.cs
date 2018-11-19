﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class player : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        mTransform = GetComponent<Transform>();
        mRigidBody2D = GetComponent<Rigidbody2D>();
        mAnimator = GetComponent<Animator>();

        collidableLayerMask = LayerMask.GetMask("collidable");

        mSpriteRenderer = GetComponent<SpriteRenderer>();

        //Get the global data.
        mGlobalData = GameObject.Find("globalData").GetComponent<globalData>();

	    mFiringKey = KeyCode.F;
	}

	// Update is called once per frame
	void Update () 
	{
		Movement ();
		BulletCreation();
		
		mSpriteRenderer.enabled = true;
		
		//Perform the camera controlling.
		if(mCameraObject == null)
		{
			//Get the main camera.
			mCameraObject = GameObject.Find("MainCamera(Clone)");
			
			//Get the other main camera components.
			if(mCameraObject != null)
			{
				mCameraScript = mCameraObject.GetComponent<camera>();
				mCameraTransform = mCameraObject.transform;
			}
		}
		
		//Only control the camera if it exists.
		if(mCameraObject != null)
		{
			UpdateCameraPosition();
			CheckIfBelowCamera ();
		}

		//Compute invincibility period.
		if(mIsHurt == true)
		{
			if(mHurtInvincibilityPeriodAmount > 0.1)
			{
				mSpriteRenderer.enabled = true;
			}

			mHurtInvincibilityPeriodAmount += Time.deltaTime;
			mFlashPeriodAmount += Time.deltaTime;

			if(mHurtInvincibilityPeriodAmount >= mHurtInvincibilityPeriod)
			{
				mIsHurt = false;
			}
			
			if(mFlashPeriodAmount >= mFlashPeriod)
			{
				mSpriteRenderer.enabled = false;
				mFlashPeriodAmount = 0.0f;
			}
		}
		
		//Code to run while dead.
		if(mIsDead)
		{
			//Get the gui fader object and check if it exists. If not, it is destroyed.
			GameObject guiFaderObj = GameObject.Find("guiFader(Clone)");
			
			//The component of the gui fader object.
			guiFader guiFaderComp = guiFaderObj.GetComponent<guiFader>();
			
			//If the gui fader object is destroyed, reload the current level. 
			// Otherwise, make the fader fade in.
			if(guiFaderComp.GetIsDoneFading() == true)
			{
				//The name of the current scene being reloaded.
				string sceneName = SceneManager.GetActiveScene().name;
				
				mGlobalData.ChangeMap(sceneName);
			}
			else
			{
				guiFaderComp.SetIsFadingIn(true);
			}
		}

		//Code to run while won.
		if(mFoundExit)
		{
			//Get the gui fader object and check if it exists. If not, it is destroyed.
			GameObject guiFaderObj = GameObject.Find("guiFader(Clone)");

			//The component of the gui fader object.
			guiFader guiFaderComp = guiFaderObj.GetComponent<guiFader>();

			//If the gui fader object is destroyed, reload the current level. 
			// Otherwise, make the fader fade in.
			if(guiFaderComp.GetIsDoneFading() == true)
			{
				//The name of the current scene.
				string sceneName = SceneManager.GetActiveScene().name;

				mGlobalData.SetCheckpointEnabled(false);

				//Decide which scene to switch to.
				if (sceneName == "level_garage") 
				{
					mGlobalData.ChangeMap ("level_wall_fade");
				}
				else if (sceneName == "level_wall_fade") 
				{
					mGlobalData.ChangeMap ("level_kitchen");
				}
			}
			else
			{
				guiFaderComp.SetIsFadingIn(true);
			}
		}
	}

	//Use this for anything involving collision or anything else that is discrete every 1 / 60th of a frame.
	void FixedUpdate()
	{
		//Check if grounded.
		if(mRigidBody2D.velocity.y <= 0.0f)
		{
			//Perform a raycast below the player to check if the player is grounded.

			//The Left ray.
			RaycastHit2D groundRayL = Physics2D.Raycast(mTransform.position - new Vector3(0.5f, 0.5f, 0.0f), 
				new Vector3(0.0f, -1.0f, 0.0f), 0.2f, collidableLayerMask);

			//The right ray.
			RaycastHit2D groundRayR = Physics2D.Raycast(mTransform.position - new Vector3(-0.5f, 0.5f, 0.0f), 
				new Vector3(0.0f, -1.0f, 0.0f), 0.2f, collidableLayerMask);

            //The middle ray.
            RaycastHit2D groundRayM = Physics2D.Raycast(mTransform.position - new Vector3(0.0f, 0.5f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f), 0.2f, collidableLayerMask);

            //If the collision occurs, check if grounded. If not, assume the player is in the air and set grounded to 
            // false.
            if (groundRayL)
            {
                //If the distance is smaller than the previous distance, wait until the object stops falling. If the 
                // distance is larger than or equal to the previous distance, then the object is grounded.
                if (groundRayL.distance < mPrevDistanceFromGroundL)
                {
                    mPrevDistanceFromGroundL = groundRayL.distance;
                }
                else
                {
                    mPrevDistanceFromGroundL = 999.0f;
                    mGrounded = true;
                    mAnimator.SetBool("Grounded", true);
                }
            }
            else if (groundRayR)
            {
                //If the distance is smaller than the previous distance, wait until the object stops falling. If the 
                // distance is larger than or equal to the previous distance, then the object is grounded.
                if (groundRayL.distance < mPrevDistanceFromGroundR)
                {
                    mPrevDistanceFromGroundR = groundRayL.distance;
                }
                else
                {
                    mPrevDistanceFromGroundR = 999.0f;
                    mGrounded = true;
                    mAnimator.SetBool("Grounded", true);
                }
            }
            else if (groundRayM)
            {
                //If the distance is smaller than the previous distance, wait until the object stops falling. If the 
                // distance is larger than or equal to the previous distance, then the object is grounded.
                if (groundRayM.distance < mPrevDistanceFromGroundM)
                {
                    mPrevDistanceFromGroundM = groundRayM.distance;
                }
                else
                {
                    mPrevDistanceFromGroundM = 999.0f;
                    mGrounded = true;
                    mAnimator.SetBool("Grounded", true);
                }
            }
            else
            {
                mGrounded = false;
                mAnimator.SetBool("Grounded", false);
            }
        }
    }

    //Collision Callbacks (Non-Trigger).
    void OnCollisionEnter2D(Collision2D collision)
    {

    }

    //Collision Callbacks on Enter(Trigger).
    void OnTriggerEnter2D(Collider2D collider)
    {
        //Check if colliding with bottle caps.
        if (collider.gameObject.tag == "gold")
        {
            //The bottle cap object being collided with.
            bottleCap bottleCap1 = collider.gameObject.GetComponent<bottleCap>();

            mGlobalData.SetCurrency(mGlobalData.GetCurrency() + bottleCap1.GetCurrency());

            //Debug.Log("Currency: " + mGlobalData.GetCurrency());

            Destroy(collider.gameObject);
        }

        //Check if colliding with a cookie.
        if (collider.gameObject.tag == "cookie")
        {
			if(mArmorCookies == 0)
			{
				mCurHealth++;

				if (mCurHealth > mMaxHealth)
				{
					mCurHealth = mMaxHealth;
				}

				//Debug.Log("Health: " + mCurHealth);
			}

			Destroy(collider.gameObject);
		}
		
		 //Check if colliding with an armor cookie.
        if (collider.gameObject.tag == "armor_cookie")
        {
			if(mCurHealth == mMaxHealth)
			{
				mArmorCookies += 1;
			}
			else
			{
				mCurHealth += 1;
				
				Debug.Log("Health: " + mCurHealth);
			}

			Destroy(collider.gameObject);
		}
		
		//Check if colliding with a scene exit.
		if (collider.gameObject.name == "sceneExit") 
		{
            //Debug.Log("Reached the exit!");
			
			//Get the gui fader object and check if it exists. If not, it is destroyed.
			GameObject guiFaderObj = GameObject.Find("guiFader(Clone)");
			
			//The component of the gui fader object.
			guiFader guiFaderComp = guiFaderObj.GetComponent<guiFader>();
			
			//Set the fader to is fading in and make the player enter invincibility 
			// mode and be stuck in place.
			guiFaderComp.SetIsFadingIn(true);
			
			mFoundExit = true;
		}

		//Check if colliding with a checkpoint object.
		if (collider.gameObject.name == "checkpoint") 
		{
			//Debug.Log("Reached a checkpoint!");

			mGlobalData.SetCheckpointEnabled(true);
			mGlobalData.SetCheckpointPosition(collider.gameObject.transform.position);

			Destroy(collider.gameObject);
		}
	}

    //Collision Callbacks on Stay(Trigger).
    void OnTriggerStay2D(Collider2D collider)
    {
        //Check if colliding with an enemy.
        if (collider.gameObject.tag == "enemy1")
        {
            //The other enemy that the player is colliding with.
            DustBunny enemy1 = collider.gameObject.GetComponent<DustBunny>();

            //Only get affected by the enemy if it isn't already dead.
            if (!enemy1.GetIsDead())
            {
                TakeDamage();
            }
		} 
		else if (collider.gameObject.tag == "spider") 
		{
            //The other enemy that the player is colliding with.
            spider spiderComp = collider.gameObject.GetComponent<spider>();

            //Only get affected by the enemy if it isn't already dead.
            if (!spiderComp.GetIsDead())
            {
                TakeDamage();
            }
		}
	}

	//Methods.
	void Movement()
	{
		//Only allow the player to move if the player is not dead and not being knocked back.
		if(mIsBeingKnockedBack == false && mIsDead == false && mFoundExit == false)
		{
			//The final walk speed.
			float finalWalkSpeed = 0.0f;

			//The final vertical speed.
			float finalVerticalSpeed = mRigidBody2D.velocity.y;

			//Checks if the jump key is pressed.
			bool jumpingAllowed = false;

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			//Process movement for left and right walking.
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

			//--------------------------------------------------------------------------------------------------------------
			//Checking for pressed keys.
			//--------------------------------------------------------------------------------------------------------------

			//Check if the left walking keys are down.
			if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
	            mFacingRight = false;
				mWalkingLeft = true;
                mAnimator.SetBool("Moving", true);
                mSpriteRenderer.flipX = true;
            }

            //Check if the right walking keys are down.
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                mFacingRight = true;
                mWalkingRight = true;
                mAnimator.SetBool("Moving", true);
                mSpriteRenderer.flipX = false;
            }

            else
            {
                mAnimator.SetBool("Moving", false);
            }

            //Check if the jumping keys are down.
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            {
                //Only jump if the player is grounded.
                if (mGrounded)
                {
                    mGrounded = false;
                    mAnimator.SetBool("Grounded", false);
                    jumpingAllowed = true;
                }
            }

            //Check if the quit key is down.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            //--------------------------------------------------------------------------------------------------------------
            //Checking for released keys.
            //--------------------------------------------------------------------------------------------------------------

            //Check if the left walking keys are released.
            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            {
                mWalkingLeft = false;
            }

            //Check if the right walking keys are released.
            else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                mWalkingRight = false;
            }

            //--------------------------------------------------------------------------------------------------------------
            //Process the movement.
            //--------------------------------------------------------------------------------------------------------------

            if (!(mWalkingLeft && mWalkingRight))
            {
                //Set the walking speed if left walking.
                if (mWalkingLeft)
                {
                    finalWalkSpeed = -mBaseWalkSpeed;
                }

                //Set the walking speed if right walking.
                if (mWalkingRight)
                {
                    finalWalkSpeed = mBaseWalkSpeed;
                }
            }

            //If the jumping key is pressed and was validated, then set the base jump speed.
            if (jumpingAllowed)
            {
                finalVerticalSpeed = mBaseJumpSpeed;
            }

            //Set the final velocity.
            mRigidBody2D.velocity = new Vector3(finalWalkSpeed, finalVerticalSpeed, 0.0f);
        }
        else
        {
            if (mGrounded)
            {
                mRigidBody2D.velocity = new Vector2(0.0f, mRigidBody2D.velocity.y);
            }

            if (mHurtInvincibilityPeriodAmount >= mKnockbackPeriod)
            {
                mIsBeingKnockedBack = false;
            }
        }
    }

    void BulletCreation()
    {
        //Check if the left walking keys are down.
        if (Input.GetKeyDown(mFiringKey))
        {
            if (mIsFiringBullets == false)
            {
                mIsFiringBullets = true;
                mFiringPeriodAmount = mFiringPeriod;
            }
        }

        else if (Input.GetKeyUp(mFiringKey))
        {
            mIsFiringBullets = false;
        }

        mFiringPeriodAmount += Time.deltaTime;

        //If firing bullets, create them after the correct amount of time has elapsed.
        if (mIsFiringBullets == true)
        {
            while (mFiringPeriodAmount >= mFiringPeriod)
            {
				//The bullet offset amount on the x axis.
				float xOffset = 0.0f;
				
				if(mFacingRight)
				{
					xOffset = 1.0f;
				}
				else
				{
					xOffset = -1.0f;
				}
				
                GameObject bulletPrefab = Instantiate(mBulletPrefab, new Vector2(
					transform.position.x + xOffset, transform.position.y + 0.5f), 
					Quaternion.identity);
					
                bullet bulletComponent = bulletPrefab.GetComponent<bullet>();
                bulletComponent.SetFacingRight(mFacingRight);
                mFiringPeriodAmount -= mFiringPeriod;
            }
        }
    }

    void UpdateCameraPosition()
    {
        //A quick reference to the player's position.
        Vector3 curPosition = mTransform.position;

        //The current bounds of the camera.
        Bounds curBounds = mCameraScript.GetBounds();

        //Camera offset for forcing the camera away from the player.
        Vector3 cameraOffset = new Vector3(0.0f, 0.0f, 0.0f);

        float transformY = 0.0f;

        float cameraYPos = mCameraTransform.position.y;

        float ySpeedInterpolation;

        bool cameraIsScrollingUp = mCameraScript.GetIsScrollingUp();

        if (cameraIsScrollingUp)
        {
            if (cameraYPos < 20.0f)
            {
                if (mScrollSpeed1Interpolation1 < 1.0f)
                {
                    mScrollSpeed1Interpolation1 += 0.1f * Time.deltaTime;

                    if (mScrollSpeed1Interpolation1 > 1.0f)
                    {
                        mScrollSpeed1Interpolation1 = 1.0f;
                    }

                    ySpeedInterpolation = Mathf.Lerp(0.0f, mScrollSpeed1, mScrollSpeed1Interpolation1);

                    transformY = cameraYPos + (ySpeedInterpolation * Time.deltaTime);
                }
                else
                {
                    transformY = cameraYPos + (mScrollSpeed1 * Time.deltaTime);
                }
            }
            else if (cameraYPos >= 20.0f && cameraYPos < 62.0f)
            {
                mScrollSpeed1Interpolation1 = 0.0f;

                if (mScrollSpeed1Interpolation2 < 1.0f)
                {
                    mScrollSpeed1Interpolation2 += 0.2f * Time.deltaTime;

                    if (mScrollSpeed1Interpolation2 > 1.0f)
                    {
                        mScrollSpeed1Interpolation2 = 1.0f;
                    }

                    ySpeedInterpolation = Mathf.Lerp(mScrollSpeed1, mScrollSpeed2, mScrollSpeed1Interpolation2);

                    transformY = cameraYPos + (ySpeedInterpolation * Time.deltaTime);
                }
                else
                {
                    transformY = cameraYPos + (mScrollSpeed2 * Time.deltaTime);
                }
            }
            else
            {
                if (mScrollSpeed1Interpolation1 < 1.0f)
                {
                    mScrollSpeed1Interpolation1 += 0.1f * Time.deltaTime;

                    if (mScrollSpeed1Interpolation1 > 1.0f)
                    {
                        mScrollSpeed1Interpolation1 = 1.0f;
                    }

                    ySpeedInterpolation = Mathf.Lerp(mScrollSpeed2, 0.0f, mScrollSpeed1Interpolation1);

                    transformY = cameraYPos + (ySpeedInterpolation * Time.deltaTime);
                }
                else
                {
                    transformY = cameraYPos;
                }
            }
        }
        else
        {
            transformY = mTransform.position.y;
        }

        //The final camera position.
        Vector3 finalCameraPosition = new Vector3(mTransform.position.x, transformY,
                                            mCameraTransform.position.z);

        //Check if the camera is outside the x bounds.
        if (curPosition.x < curBounds.min.x)
        {
            cameraOffset.x = curBounds.min.x - curPosition.x;
        }
        else if (curPosition.x > curBounds.max.x)
        {
            cameraOffset.x = curBounds.max.x - curPosition.x;
        }

        if (!cameraIsScrollingUp)
        {
            //Check if the camera is outside the y bounds.
            if (curPosition.y < curBounds.min.y)
            {
                cameraOffset.y = curBounds.min.y - curPosition.y;
            }
            else if (curPosition.y > curBounds.max.y)
            {
                cameraOffset.y = curBounds.max.y - curPosition.y;
            }
        }

        finalCameraPosition += cameraOffset;

        mCameraTransform.position = finalCameraPosition;
    }

	void CheckIfBelowCamera()
	{
		if (!mIsDead && mTransform.position.y < (mCameraTransform.position.y - 6.0f)) 
		{
			mIsDead = true;
			//Debug.Log ("Fell out of the level.");
			mWalkingLeft = false;
			mWalkingRight = false;
			mRigidBody2D.velocity = new Vector3 (0.0f, 0.0f, 0.0f);
		}
	}
	
	void TakeDamage()
	{
		//If the player isn't currently hurt, hurt the player.
		if (mIsHurt == false && mIsDead == false && mFoundExit == false)
		{
			mIsBeingKnockedBack = true;
			mWalkingLeft = false;
			mWalkingRight = false;
			mGrounded = false;
            mAnimator.SetBool("Grounded", false);
            mIsHurt = true;
            mAnimator.Play("Hurt");

			mSpriteRenderer.enabled = false;

			if(mArmorCookies != 0)
			{
				mArmorCookies -= 1;
				
				mHurtInvincibilityPeriodAmount = 0.0f;

				if (mRigidBody2D.velocity.x > 0)
				{
					mRigidBody2D.velocity = new Vector2(-3.0f, 10.0f);
					mFacingRight = true;
				}
				else if (mRigidBody2D.velocity.x < 0)
				{
					mRigidBody2D.velocity = new Vector2(3.0f, 10.0f);
					mFacingRight = false;
				}
				else
				{
					mRigidBody2D.velocity = new Vector2(0.0f, 10.0f);
				}
			}
			else
			{
				//Kill the player if out of health. Otherwise take damage.
				if (mCurHealth == 1)
				{
					//Debug.Log("Dead!");
					mIsDead = true;
					mAnimator.Play("Death");

					if (mRigidBody2D.velocity.x > 0)
					{
						mRigidBody2D.velocity = new Vector2(-5.0f, 20.0f);
						mFacingRight = true;
					}
					else if (mRigidBody2D.velocity.x < 0)
					{
						mRigidBody2D.velocity = new Vector2(5.0f, 20.0f);
						mFacingRight = false;
					}
					else
					{
						mRigidBody2D.velocity = new Vector2(0.0f, 20.0f);
					}
				}
				else
				{
					mCurHealth--;
					mHurtInvincibilityPeriodAmount = 0.0f;

					if (mRigidBody2D.velocity.x > 0)
					{
						mRigidBody2D.velocity = new Vector2(-3.0f, 10.0f);
						mFacingRight = true;
					}
					else if (mRigidBody2D.velocity.x < 0)
					{
						mRigidBody2D.velocity = new Vector2(3.0f, 10.0f);
						mFacingRight = false;
					}
					else
					{
						mRigidBody2D.velocity = new Vector2(0.0f, 10.0f);
					}
				}
			}
		}
	}

	//Getters:
	public uint GetHealth()
	{
		return mCurHealth;
	}
	
	public uint GetArmorCookies()
	{
		return mArmorCookies;
	}
	
    //Variables:

    //The current health the player has.
    private uint mCurHealth = 3;

    //The maximum health the player can hold.
    private uint mMaxHealth = 3;
	
	//The number of armor cookies the player has collected.
	private uint mArmorCookies = 0;

    //The base walking speed.
    private float mBaseWalkSpeed = 6.0f;

    //The base jump speed.
    private float mBaseJumpSpeed = 16.0f;

    //The previous distance from the ground for the left ray.
    private float mPrevDistanceFromGroundL = 999.0f;

    //The previous distance from the ground for the right ray.
    private float mPrevDistanceFromGroundR = 999.0f;

    //The previous distance from the ground for the middle ray.
    private float mPrevDistanceFromGroundM = 999.0f;

    //The time elapsed for a single bullet to fire. The denominator is the bullets per second 
    // fired by the player.
    private float mFiringPeriod = 1.0f / 6.0f;

    //The amount of time elapsed for firing bullets. It is initially set to the firing frequency.
    private float mFiringPeriodAmount;

    //The current invincibility amount after the player gets hurt. When equal to max, the player
    // is no longer invincible.
    private float mHurtInvincibilityPeriodAmount = 0.0f;

    //The maximum hurt invincibility amount in seconds.
    private float mHurtInvincibilityPeriod = 2.0f;
	
	 //The current flashing amount after the player gets hurt. When equal to max, the player
    // flashes once.
    private float mFlashPeriodAmount = 0.0f;

    //The maximum flash period amount in seconds.
    private float mFlashPeriod = 0.1f;

    //The scrolling speed for the camera in the wall level at the bottom of the level.
    public float mScrollSpeed1 = 0.3f;

    //The scrolling speed for the camera in the wall level at the middle of the level.
    public float mScrollSpeed2 = 0.6f;

    //The interpolation for the scroll speed at the bottom of the level.
    public float mScrollSpeed1Interpolation1 = 0.0f;

    //The interpolation for the scroll speed at the middle of the level.
    public float mScrollSpeed1Interpolation2 = 0.0f;

    //The maximum knockback amount in seconds.
    private float mKnockbackPeriod = 0.5f;

    //Used to check if the player is walking left.
    private bool mWalkingLeft = false;

    //Used to check if the player is walking right.
    private bool mWalkingRight = false;

    //Checks if the player is grounded. If so, the player can jump.
    private bool mGrounded = true;

    //The direction the player is initially facing.
    private bool mFacingRight = true;

    //Checks if the player is currently firing bullets.
    private bool mIsFiringBullets = false;

    //Checks if the player has taken damage by an enemy.
    private bool mIsHurt = false;

	//Checks if the player is dead.
	private bool mIsDead = false;
	
	//Checks if the player found the exit.
	private bool mFoundExit = false;

    //Checks if the player is being knocked back.
    private bool mIsBeingKnockedBack = false;

    //The firing key.
    private KeyCode mFiringKey;

    //The main camera object.
    private GameObject mCameraObject;

    //The camera script for the main camera.
    private camera mCameraScript;

    //The camera transform component for the main camera.
    private Transform mCameraTransform;

    //The global game data.
    private globalData mGlobalData;

    //The layer mask of the collidable objects.
    private LayerMask collidableLayerMask;

    //The local transform of the object.
    private Transform mTransform;

    //The local rigid body of the object.
    private Rigidbody2D mRigidBody2D;

    //The sprite renderer.
    private SpriteRenderer mSpriteRenderer;

    //The animator.
    private Animator mAnimator;

    //Public variables:

    //The prefab for the bullet object.
    public GameObject mBulletPrefab;
}