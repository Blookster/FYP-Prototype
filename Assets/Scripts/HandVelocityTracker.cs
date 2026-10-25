using UnityEngine;

// ============================================================================
//  HandVelocityTracker  -  measures how fast (and which way) a glove is moving.
// ============================================================================
// WHY WE NEED IT
// A "punch" is not just "a hand touching something" - it is a hand MOVING FAST in a
// direction. Unity can't tell us that for free: a hand moved by VR tracking (or by
// setting transform.position) has no Rigidbody velocity. So we measure it ourselves:
//
//        velocity = (position this frame - position last frame) / time between frames
//
// You don't need to add this to the gloves yourself - AttackPointBase adds it
// automatically the first time a glove touches a bubble.
public class HandVelocityTracker : MonoBehaviour
{
    [Tooltip("How much of the new reading to blend in each frame. 1 = raw (jittery), small = smooth but laggy.")]
    [Range(0.05f, 1.0f)]
    public float smoothing = 0.5f;

    // "{ get; private set; }" = anyone can READ it, only this script can WRITE it.
    public Vector3 Velocity { get; private set; }
    public float Speed => Velocity.magnitude;

    // False until we have measured at least once. A brand new tracker has no history,
    // so its velocity would read 0 - we must not treat that as "a very slow punch".
    public bool IsReady { get; private set; }

    private Vector3 lastPosition;
    private bool hasLastPosition;

    void OnEnable()
    {
        hasLastPosition = false;
        IsReady = false;
        Velocity = Vector3.zero;
    }

    // LateUpdate = runs after every Update this frame, i.e. after the controller pose /
    // FistFollower has already moved the glove. We measure the glove's FINAL position.
    void LateUpdate()
    {
        if (Time.deltaTime <= 0.0f) return; // paused game: dividing by 0 would give infinity

        Vector3 currentPosition = transform.position;

        if (hasLastPosition)
        {
            Vector3 instantVelocity = (currentPosition - lastPosition) / Time.deltaTime;

            // Lerp = blend. Mixes the old velocity with the new reading to remove
            // single-frame jitter while still reacting quickly.
            Velocity = Vector3.Lerp(Velocity, instantVelocity, smoothing);
            IsReady = true;
        }

        lastPosition = currentPosition;
        hasLastPosition = true;
    }
}
