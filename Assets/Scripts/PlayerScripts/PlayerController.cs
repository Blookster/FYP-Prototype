using UnityEngine;

using System.Collections;



[RequireComponent(typeof(Rigidbody))]

[RequireComponent(typeof(HealthSystem))]

[RequireComponent(typeof(PlayerInputHandler))]

[RequireComponent(typeof(FighterVisualManager))]

[RequireComponent(typeof(PlayerParameter))]

// ^ Why it's coded like this: In Unity, scripts often crash with NullReferenceException if a required component is forgotten on the GameObject. These attributes tell Unity: "If you attach PlayerController to a capsule, automatically force-attach these other scripts too." It guarantees structural integrity before the game even presses Play.



public class PlayerController : MonoBehaviour

{

    public enum FighterState

    {

        Idle,

        Attack,

        Blocking,

        Dodge,

        BeenHit

    }



    public PlayerParameter playerParameter;



    [Header("State")]

    public FighterState currentPlayerState;



    void Start()

    {

        playerParameter = gameObject.GetComponent<PlayerParameter>();



        currentPlayerState = FighterState.Idle;



        // Cache all modular component references

        playerParameter.rb = GetComponent<Rigidbody>();

        playerParameter.inputHandler = GetComponent<PlayerInputHandler>();

        playerParameter.healthSystem = GetComponent<HealthSystem>();

        playerParameter.visualManager = GetComponent<FighterVisualManager>();



        playerParameter.xrOrigin = transform.parent;

        playerParameter.fixedLocalPosition = transform.localPosition;



        playerParameter.originalHandPositions = new Vector3[2];

        if (playerParameter.leftHand) playerParameter.originalHandPositions[0] = playerParameter.leftHand.localPosition;

        if (playerParameter.rightHand) playerParameter.originalHandPositions[1] = playerParameter.rightHand.localPosition;

        playerParameter.originalHandY = playerParameter.leftHand ? playerParameter.leftHand.localPosition.y : 0f;

    }



    void Update()

    {

        // State Machine Logic Loop

        switch (currentPlayerState)

        {

            case FighterState.Idle:

                ReturnToFixedPosition();

                ReturnHandsToRest();

                if (CheckArmExtension())

                {

                    currentPlayerState = FighterState.Attack;

                }

                break;



            case FighterState.Attack:

                ReturnToFixedPosition();

                ReturnHandsToRest();

                if (!CheckArmExtension())

                {

                    currentPlayerState = FighterState.Idle;

                }

                break;



            case FighterState.Blocking:

                ReturnToFixedPosition();

                RaiseHandsForBlock();

                if (!playerParameter.inputHandler.IsBlocking)

                {

                    currentPlayerState = FighterState.Idle;

                }

                break;



            case FighterState.Dodge:

                LeanBack();

                if (!playerParameter.inputHandler.IsDodge)

                {

                    currentPlayerState = FighterState.Idle;

                }

                break;



            case FighterState.BeenHit:

                // Handled via external damage triggers

                break;

        }



        // Check Input-driven defensive states

        CheckInputStates();



        // Handle special weapon timer

        if (playerParameter.isSpecialActive)

        {

            playerParameter.specialTimer -= Time.deltaTime;

            if (playerParameter.specialTimer <= 0)

            {

                DeactivateSpecialPower();

            }

        }



        CheckFistBump();

        CheckGroundSlamJump();

    }



    void CheckInputStates()

    {

        if (currentPlayerState == FighterState.BeenHit) return;



        if (playerParameter.inputHandler.IsBlocking)

        {

            currentPlayerState = FighterState.Blocking;

        }

        else if (playerParameter.inputHandler.IsDodge)

        {

            currentPlayerState = FighterState.Dodge;

        }

    }



    bool CheckArmExtension()

    {

        if (!playerParameter.leftHand || !playerParameter.rightHand || !playerParameter.headCamera) return false;

        float leftZ = playerParameter.leftHand.position.z - playerParameter.headCamera.position.z;

        float rightZ = playerParameter.rightHand.position.z - playerParameter.headCamera.position.z;

        return (leftZ > playerParameter.attackExtensionThreshold || rightZ > playerParameter.attackExtensionThreshold);

    }



    void CheckFistBump()

    {

        if (!playerParameter.isSpecialActive && playerParameter.leftHand != null && playerParameter.rightHand != null)

        {

            float distance = Vector3.Distance(playerParameter.leftHand.position, playerParameter.rightHand.position);

            if (distance < playerParameter.fistBumpDistanceThreshold)

            {

                ActivateSpecialPower();

            }

        }

    }



    void ActivateSpecialPower()

    {

        playerParameter.isSpecialActive = true;

        playerParameter.specialTimer = playerParameter.specialDuration;

        Debug.Log("Special Power Activated! Fists are super-sized.");

        if (playerParameter.leftHand != null) playerParameter.leftHand.localScale = Vector3.one * 1.5f;

        if (playerParameter.rightHand != null) playerParameter.rightHand.localScale = Vector3.one * 1.5f;

    }



    void DeactivateSpecialPower()

    {

        playerParameter.isSpecialActive = false;

        Debug.Log("Special Power Ended.");

        if (playerParameter.leftHand != null) playerParameter.leftHand.localScale = Vector3.one;

        if (playerParameter.rightHand != null) playerParameter.rightHand.localScale = Vector3.one;

    }



    void CheckGroundSlamJump()

    {

        if (!playerParameter.isJumping && playerParameter.leftHand.position.y < 0.4f && playerParameter.rightHand.position.y < 0.4f)

        {

            playerParameter.isJumping = true;

            currentPlayerState = FighterState.Dodge;

            StartCoroutine(JumpSequence());

        }

    }



    IEnumerator JumpSequence()

    {

        float elapsed = 0f;

        float duration = 0.3f;

        Vector3 startPos = transform.localPosition;

        Vector3 peakPos = playerParameter.fixedLocalPosition + Vector3.up * playerParameter.jumpHeight;



        while (elapsed < duration)

        {

            elapsed += Time.deltaTime;

            transform.localPosition = Vector3.Lerp(startPos, peakPos, elapsed / duration);

            yield return null;

        }



        elapsed = 0f;

        startPos = transform.localPosition;

        while (elapsed < duration)

        {

            elapsed += Time.deltaTime;

            transform.localPosition = Vector3.Lerp(startPos, playerParameter.fixedLocalPosition, elapsed / duration);

            yield return null;

        }



        transform.localPosition = playerParameter.fixedLocalPosition;

        playerParameter.isJumping = false;

    }



    void ReturnToFixedPosition()

    {

        if (playerParameter == null) return;

        transform.localPosition = Vector3.Lerp(transform.localPosition, playerParameter.fixedLocalPosition, 15f * Time.deltaTime);

    }



    void ReturnHandsToRest()

    {

        if (playerParameter.leftHand) playerParameter.leftHand.localPosition = Vector3.Lerp(playerParameter.leftHand.localPosition, playerParameter.originalHandPositions[0], 10f * Time.deltaTime);

        if (playerParameter.rightHand) playerParameter.rightHand.localPosition = Vector3.Lerp(playerParameter.rightHand.localPosition, playerParameter.originalHandPositions[1], 10f * Time.deltaTime);

    }



    void RaiseHandsForBlock()

    {

        float targetY = playerParameter.originalHandY + playerParameter.blockHandRaiseY;

        if (playerParameter.leftHand)

        {

            Vector3 pos = playerParameter.leftHand.localPosition;

            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);

            playerParameter.leftHand.localPosition = pos;

        }

        if (playerParameter.rightHand)

        {

            Vector3 pos = playerParameter.rightHand.localPosition;

            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);

            playerParameter.rightHand.localPosition = pos;

        }

    }



    void LeanBack()

    {

        Vector3 targetLocalPos = playerParameter.fixedLocalPosition + Vector3.back * playerParameter.dodgeBackDistance;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, 10f * Time.deltaTime);

    }



    // Called externally by the HitReceiver script when an enemy fist hits the player

    public void HandleHit(float damageAmount)

    {

        if (currentPlayerState == FighterState.Blocking)

        {

            Debug.Log("Attack blocked! Reduced damage taken.");

            //playerParameter.healthSystem.TakeDamage(damageAmount * 0.2f);

        }

        else if (currentPlayerState == FighterState.Dodge)

        {

            Debug.Log("Dodged successfully! Zero damage.");

        }

        else

        {

            currentPlayerState = FighterState.BeenHit;

            //playerParameter.healthSystem.TakeDamage(damageAmount);

            if (playerParameter.visualManager != null) playerParameter.visualManager.TriggerFlash(Color.red, 0.15f);

        }

    }

}