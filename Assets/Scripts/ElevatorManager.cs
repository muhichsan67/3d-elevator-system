using UnityEngine;

namespace ElevatorSystem
{
    /// Mengelola inti logika elevator, sistem penguncian global, 
    /// dan perpindahan koordinat Player (Singleton Architect).
    public class ElevatorManager : MonoBehaviour
    {
        // Pola Singleton untuk akses global dari skrip lain
        public static ElevatorManager Instance { get; private set; }

        [Header("Elevator Status")]
        public int currentFloor = 1; // Default awal di lantai F1 (Index: 1)
        public bool isMoving = false;
        public bool isLocked = false;

        [Header("Player & Level References")]
        [SerializeField] private Transform playerTransform; 
        [SerializeField] private Transform[] floorSpawnPoints; // Diisi 4 koordinat dari Orang 4 (B1, F1, F2, F3)

        private void Awake()
        {
            // Inisialisasi Singleton
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

        /// Mengunci seluruh interaksi tombol pada panel elevator secara global.
        public void LockButtons()
        {
            isLocked = true;
        }

        /// Membuka kembali interaksi tombol pada panel elevator.
        public void UnlockButtons()
        {
            isLocked = false;
        }

        /// Memvalidasi dan memulai proses perpindahan lantai elevator.
        public bool MoveToFloor(int targetFloorIndex)
        {
            // Validasi jika elevator sedang mengunci, sedang bergerak, atau memilih lantai yang sama
            if (isLocked || isMoving || targetFloorIndex == currentFloor)
            {
                return false; 
            }

            // Kunci interaksi tombol segera setelah proses dimulai
            LockButtons();
            isMoving = true;
            
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
                return;
            }

            // Eksekusi pemindahan posisi Player jika komponen terpasang
            if (playerTransform != null)
            {
                playerTransform.position = floorSpawnPoints[targetFloorIndex].position;
                playerTransform.rotation = floorSpawnPoints[targetFloorIndex].rotation;
                Debug.Log($"Player berhasil dipindahkan ke lantai index: {targetFloorIndex}");
            }
            else
            {
                Debug.LogWarning("ElevatorManager: Player Transform belum dipasang di Inspector!");
            }

            // Perbarui status lantai saat ini dan buka kembali kunci panel
            currentFloor = targetFloorIndex;
            isMoving = false;
            UnlockButtons();
        }
    }
}