using UnityEngine;

namespace ElevatorSystem
{
    /// Mengelola inti logika elevator, sistem penguncian global, 
    /// dan perpindahan koordinat Player (Singleton Architect).
    public class ElevatorManager : MonoBehaviour
    {
        public static ElevatorManager Instance { get; private set; }

        [Header("Elevator Status")]
        public int currentFloor = 0; // Default awal di lantai B1 (Index: 0)
        public bool isMoving = false;
        public bool isLocked = false;

        [Header("Player & Level References")]
        [SerializeField] private Transform playerTransform; 
        [SerializeField] private Transform[] floorSpawnPoints;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            Cursor.visible = false;
        }

        public void LockButtons()
        {
            isLocked = true;
        }

        public void UnlockButtons()
        {
            isLocked = false;
        }

        /// Memvalidasi dan memulai proses perpindahan lantai elevator.
        public bool MoveToFloor(int targetFloorIndex)
        {
            if (isLocked || isMoving || targetFloorIndex == currentFloor || targetFloorIndex == 3)
            {
                Debug.LogWarning($"ElevatorManager: Akses ke lantai index {targetFloorIndex} ditolak!");
                return false; 
            }

            LockButtons();
            isMoving = true;

            // Matikan CharacterController SEJAK LIFT MULAI BERGERAK (proses loading dimulai)
            if (playerTransform != null)
            {
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false; 
                    Debug.Log("ElevatorManager: Pergerakan Player DIKUNCI.");
                }
            }
            
            return true;
        }

        /// Mengeksekusi teleportasi fisik koordinat Player ke lantai tujuan setelah proses loading selesai.
        public void TeleportPlayer(int targetFloorIndex)
        {
            // Validasi keamanan array koordinat
            if (floorSpawnPoints == null || targetFloorIndex < 0 || targetFloorIndex >= floorSpawnPoints.Length)
            {
                Debug.LogError("ElevatorManager: Target Floor Index atau Spawn Points tidak valid!");
                isMoving = false;
                UnlockButtons();

                // Jaga-jaga jika error, kembalikan kontrol player
                if (playerTransform != null)
                {
                    CharacterController cc = playerTransform.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = true;
                }
                return;
            }

            if (playerTransform != null)
            {
                // Pindahkan koordinat posisi dan rotasi langsung (Aman karena CC sudah mati dari awal)
                playerTransform.position = floorSpawnPoints[targetFloorIndex].position;
                playerTransform.rotation = floorSpawnPoints[targetFloorIndex].rotation;

                // Hidupkan kembali CharacterController SETELAH LIFT SAMPAI di tujuan
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = true;
                    Debug.Log("ElevatorManager: Pergerakan Player DIBUKA KEMBALI.");
                }

                Debug.Log($"Player berhasil dipindahkan ke lantai index: {targetFloorIndex}.");
            }
            else
            {
                Debug.LogWarning("ElevatorManager: Player Transform belum dipasang di Inspector!");
            }

            currentFloor = targetFloorIndex;
            isMoving = false;
            UnlockButtons();
        }
    }
}