using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElevatorSystem
{
    /// Mengatur seluruh interaksi panel UI Elevator (World Space Canvas):
    /// - Menerima klik tombol lantai (B1, F1, F2, F3)
    /// - Mengisi Progress Bar (Image.fillAmount 0 -> 1) via Coroutine
    /// - Memperbarui Status Text ("Idle", "Moving...", "Access Denied", "Arrived")
    /// - Berkomunikasi dengan ElevatorManager (Singleton) milik Orang 1
    public class ElevatorPanelController : MonoBehaviour
    {
        [Header("Floor Buttons (urutan: B1=0, F1=1, F2=2, F3=3)")]
        [Tooltip("Drag 4 Button dari World Space Canvas sesuai urutan lantai.")]
        [SerializeField] private Button[] floorButtons;

        [Header("Progress Bar UI")]
        [Tooltip("Image dengan Image Type = Filled, Fill Method = Horizontal.")]
        [SerializeField] private Image progressBarFill;

        [Header("Status & Floor Display")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text currentFloorText;

        [Header("Movement Settings")]
        [Tooltip("Total durasi loading (detik) dari 0% ke 100%.")]
        [SerializeField] private float moveDuration = 3f;

        [Tooltip("Durasi tampilan pesan 'Access Denied' sebelum reset ke Idle.")]
        [SerializeField] private float deniedMessageDuration = 1.5f;

        [Header("UI Color Palette (sesuai GDD)")]
        [SerializeField] private Color colorNormal       = new Color(0.498f, 0.549f, 0.553f); // #7F8C8D Muted Silver
        [SerializeField] private Color colorActive       = new Color(0.161f, 0.502f, 0.725f); // #2980B9 Electric Blue
        [SerializeField] private Color colorProgress     = new Color(0.204f, 0.596f, 0.859f); // #3498DB Cyan Progress
        [SerializeField] private Color colorDenied       = new Color(0.753f, 0.224f, 0.169f); // #C0392B Crimson Red
        [SerializeField] private Color colorTextNormal   = new Color(0.925f, 0.941f, 0.945f); // #ECF0F1 Off-White

        [Header("Floor Labels (untuk Status Text)")]
        [SerializeField] private string[] floorLabels = new string[] { "B1", "F1", "F2", "F3" };

        // Cache komponen Image dari masing-masing tombol untuk efek warna
        private Image[] floorButtonImages;

        // Coroutine progress bar aktif (biar bisa di-stop kalau perlu)
        private Coroutine activeProgressCoroutine;

        private void Awake()
        {
            // Cache Image dari setiap Button + pasang listener klik
            if (floorButtons != null)
            {
                floorButtonImages = new Image[floorButtons.Length];
                for (int i = 0; i < floorButtons.Length; i++)
                {
                    if (floorButtons[i] == null) continue;

                    floorButtonImages[i] = floorButtons[i].GetComponent<Image>();

                    // Capture index untuk dipakai di lambda (hindari closure bug)
                    int floorIndex = i;
                    floorButtons[i].onClick.AddListener(() => OnFloorButtonClicked(floorIndex));
                }
            }
        }

        private void Start()
        {
            // Inisialisasi UI di kondisi Idle
            ResetProgressBar();
            SetStatusIdle();
            HighlightCurrentFloorButton();
        }

        private void Update()
        {
            // Sinkronisasi tampilan saat ElevatorManager dikunci dari luar (mis. mid-move)
            if (ElevatorManager.Instance != null && currentFloorText != null)
            {
                currentFloorText.text = floorLabels[Mathf.Clamp(
                    ElevatorManager.Instance.currentFloor, 0, floorLabels.Length - 1)];
            }
        }

        /// Dipanggil tiap kali user klik salah satu tombol lantai.
        public void OnFloorButtonClicked(int targetFloorIndex)
        {
            // Guard: pastikan Singleton ElevatorManager sudah ada di scene
            if (ElevatorManager.Instance == null)
            {
                Debug.LogError("ElevatorPanelController: ElevatorManager.Instance belum ada di scene!");
                return;
            }

            // Validasi via core logic Orang 1. Return false = ditolak.
            bool accepted = ElevatorManager.Instance.MoveToFloor(targetFloorIndex);

            if (!accepted)
            {
                // Tampilkan Access Denied dan SFX denied (kalau ada)
                ShowAccessDenied();
                return;
            }

            // Diterima -> jalankan progress bar coroutine
            if (activeProgressCoroutine != null) StopCoroutine(activeProgressCoroutine);
            activeProgressCoroutine = StartCoroutine(MoveProgressRoutine(targetFloorIndex));
        }

        /// Coroutine inti: isi fillAmount 0 -> 1 selama moveDuration, lalu teleport.
        private IEnumerator MoveProgressRoutine(int targetFloorIndex)
        {
            // Setup UI di state Moving
            LockButtonsVisual(true);
            UpdateProgressBar(0f);
            SetStatusMoving(targetFloorIndex);
            HighlightTargetButton(targetFloorIndex);

            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / moveDuration);
                UpdateProgressBar(progress);
                yield return null;
            }

            UpdateProgressBar(1f);

            // Eksekusi teleport via core logic Orang 1
            ElevatorManager.Instance.TeleportPlayer(targetFloorIndex);

            // Tampilkan status arrived sebentar
            SetStatusArrived(targetFloorIndex);

            yield return new WaitForSeconds(0.6f);

            // Reset ke kondisi Idle
            ResetProgressBar();
            SetStatusIdle();
            LockButtonsVisual(false);
            HighlightCurrentFloorButton();

            activeProgressCoroutine = null;
        }

        /// Memperbarui nilai fillAmount progress bar (0 - 1).
        public void UpdateProgressBar(float value)
        {
            if (progressBarFill == null) return;
            progressBarFill.fillAmount = Mathf.Clamp01(value);
            progressBarFill.color = colorProgress;
        }

        /// Mereset progress bar ke 0 dan warna Idle.
        private void ResetProgressBar()
        {
            if (progressBarFill == null) return;
            progressBarFill.fillAmount = 0f;
            progressBarFill.color = colorProgress;
        }

        /// Menampilkan status "Access Denied" + warna merah sementara.
        private void ShowAccessDenied()
        {
            if (statusText != null)
            {
                statusText.text = "Access Denied";
                statusText.color = colorDenied;
            }
            StartCoroutine(ResetDeniedAfterDelay());
        }

        private IEnumerator ResetDeniedAfterDelay()
        {
            yield return new WaitForSeconds(deniedMessageDuration);
            // Hanya reset kalau elevator tidak sedang bergerak
            if (ElevatorManager.Instance != null && !ElevatorManager.Instance.isMoving)
            {
                SetStatusIdle();
            }
        }

        /// Status default saat elevator standby.
        private void SetStatusIdle()
        {
            if (statusText == null) return;
            statusText.text = "Idle";
            statusText.color = colorTextNormal;
        }

        /// Status saat elevator sedang bergerak menuju lantai tujuan.
        private void SetStatusMoving(int targetIndex)
        {
            if (statusText == null) return;
            string label = floorLabels != null && targetIndex >= 0 && targetIndex < floorLabels.Length
                ? floorLabels[targetIndex] : targetIndex.ToString();
            statusText.text = $"Moving... {label}";
            statusText.color = colorTextNormal;
        }

        /// Status saat elevator baru saja tiba di lantai tujuan.
        private void SetStatusArrived(int targetIndex)
        {
            if (statusText == null) return;
            string label = floorLabels != null && targetIndex >= 0 && targetIndex < floorLabels.Length
                ? floorLabels[targetIndex] : targetIndex.ToString();
            statusText.text = $"Arrived - {label}";
            statusText.color = colorActive;
        }

        /// Nonaktifkan/aktifkan interaksi seluruh tombol lantai (visual + interactable).
        private void LockButtonsVisual(bool locked)
        {
            if (floorButtons == null) return;
            for (int i = 0; i < floorButtons.Length; i++)
            {
                if (floorButtons[i] == null) continue;
                floorButtons[i].interactable = !locked;
            }
        }

        /// Highlight tombol lantai yang sedang dituju (Electric Blue).
        private void HighlightTargetButton(int targetIndex)
        {
            if (floorButtonImages == null) return;
            for (int i = 0; i < floorButtonImages.Length; i++)
            {
                if (floorButtonImages[i] == null) continue;
                floorButtonImages[i].color = (i == targetIndex) ? colorActive : colorNormal;
            }
        }

        /// Highlight tombol lantai yang sedang ditempati (current floor).
        private void HighlightCurrentFloorButton()
        {
            if (floorButtonImages == null || ElevatorManager.Instance == null) return;
            int current = ElevatorManager.Instance.currentFloor;
            for (int i = 0; i < floorButtonImages.Length; i++)
            {
                if (floorButtonImages[i] == null) continue;
                floorButtonImages[i].color = (i == current) ? colorActive : colorNormal;
            }
        }
    }
}
