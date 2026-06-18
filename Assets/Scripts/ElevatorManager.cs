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
            // [PERBAIKAN] Tambahkan validasi targetFloorIndex == 3 (Index untuk lantai F3)
            // Jadi sistem akan langsung MENOLAK jika pemain mencoba mendatangi lantai F3
            if (isLocked || isMoving || targetFloorIndex == currentFloor || targetFloorIndex == 3)
            {
                Debug.LogWarning($"ElevatorManager: Akses ke lantai index {targetFloorIndex} ditolak (Lantai F3 Terkunci/Disabled)!");
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
                // [PERBAIKAN] Ambil komponen CharacterController dari Player
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                
                // [PERBAIKAN] Matikan sementara agar tidak bentrok dengan perpindahan posisi instan
                if (cc != null)
                {
                    cc.enabled = false;
                }

                // Pindahkan koordinat posisi dan rotasi
                playerTransform.position = floorSpawnPoints[targetFloorIndex].position;
                playerTransform.rotation = floorSpawnPoints[targetFloorIndex].rotation;

                // [PERBAIKAN] Hidupkan kembali setelah Player resmi berada di posisi lantai baru
                if (cc != null)
                {
                    cc.enabled = true;
                }

                Debug.Log($"Player berhasil dipindahkan ke lantai index: {targetFloorIndex} (CharacterController diamankan).");
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