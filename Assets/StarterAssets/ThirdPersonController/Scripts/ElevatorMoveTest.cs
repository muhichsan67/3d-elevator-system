using UnityEngine;
using System.Collections;

namespace StarterAssets
{
    public class ElevatorMoveManager : MonoBehaviour
    {
        [Header("Lift Setup")]
        public Transform liftObject;
        public Transform playerObject;
        public float moveSpeed = 2f;

        [Header("Floor Target Points")]
        public Transform b1Point;
        public Transform f1Point;
        public Transform f2Point;
        public Transform f3Point;

        private Transform targetPoint;
        private bool isMoving = false;
        private Vector3 playerOffset = Vector3.zero;

        private Rigidbody liftRB;
        private CharacterController playerCC;

        private void Start()
        {
            if (liftObject != null)
                liftRB = liftObject.GetComponent<Rigidbody>();

            if (playerObject != null)
                playerCC = playerObject.GetComponent<CharacterController>();
        }

        public void MoveToB1()
        {
            targetPoint = b1Point;
            MoveToTarget();
        }

        public void MoveToF1()
        {
            targetPoint = f1Point;
            MoveToTarget();
        }

        public void MoveToF2()
        {
            targetPoint = f2Point;
            MoveToTarget();
        }

        public void MoveToF3()
        {
            targetPoint = f3Point;
            MoveToTarget();
        }

        public void MoveToTarget()
        {
            if (!isMoving && targetPoint != null && liftObject != null)
                StartCoroutine(MoveLiftCoroutine());
        }

        private IEnumerator MoveLiftCoroutine()
        {
            isMoving = true;

            if (playerObject != null)
                playerOffset = playerObject.position - liftObject.position;

            ThirdPersonController tpc = playerObject != null
                ? playerObject.GetComponent<ThirdPersonController>()
                : null;

            if (tpc != null)
                tpc.IsOnMovingLift = true;

            DisablePhysics();

            yield return null;

            while (Vector3.Distance(liftObject.position, targetPoint.position) > 0.05f)
            {
                Vector3 oldLiftPos = liftObject.position;

                liftObject.position = Vector3.MoveTowards(
                    liftObject.position,
                    targetPoint.position,
                    moveSpeed * Time.deltaTime
                );

                if (playerObject != null)
                {
                    Vector3 deltaMove = liftObject.position - oldLiftPos;
                    playerObject.position += deltaMove;
                }

                yield return null;
            }

            liftObject.position = targetPoint.position;

            if (playerObject != null)
                playerObject.position = liftObject.position + playerOffset;

            if (tpc != null)
                tpc.IsOnMovingLift = false;

            EnablePhysics();

            isMoving = false;
        }

        private void DisablePhysics()
        {
            if (liftRB != null)
            {
                liftRB.isKinematic = true;
                liftRB.linearVelocity = Vector3.zero;
                liftRB.angularVelocity = Vector3.zero;
            }

            if (playerCC != null)
                playerCC.enabled = false;

            if (liftObject != null)
            {
                Collider[] allColliders = liftObject.GetComponentsInChildren<Collider>();

                foreach (Collider col in allColliders)
                {
                    if (col.transform == liftObject || col.GetComponent<CharacterController>() != null)
                        continue;

                    col.enabled = false;
                }
            }
        }

        private void EnablePhysics()
        {
            if (liftRB != null)
                liftRB.isKinematic = false;

            if (playerCC != null)
                playerCC.enabled = true;

            if (liftObject != null)
            {
                Collider[] allColliders = liftObject.GetComponentsInChildren<Collider>();

                foreach (Collider col in allColliders)
                {
                    if (col.transform == liftObject)
                        continue;

                    col.enabled = true;
                }
            }
        }

        public bool IsMoving => isMoving;
    }
}