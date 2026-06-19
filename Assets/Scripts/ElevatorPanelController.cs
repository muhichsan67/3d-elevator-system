using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElevatorSystem
{
    /// Mengatur seluruh interaksi panel UI Elevator (World Space Canvas) dengan Audio Integration
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

        [Header("Audio Settings")]
        [Tooltip("Drag AudioSource yang menempel pada object panel ini.")]
        [SerializeField] private AudioSource audioSource;
        
        [Tooltip("Suara saat tombol lantai berhasil ditekan.")]
        [SerializeField] private AudioClip buttonClickClip;
        
        [Tooltip("Suara error saat akses lantai ditolak.")]
        [SerializeField] private AudioClip accessDeniedClip;
        
        [Tooltip("Suara 'Ding' saat lift telah sampai di tujuan.")]
        [SerializeField] private AudioClip arrivalDingClip;

        private Image[] floorButtonImages;
        private Coroutine activeProgressCoroutine;

        private void Awake()
        {
            // Auto-get AudioSource jika lupa di-drag di Inspector
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (floorButtons != null)
            {
                floorButtonImages = new Image[floorButtons.Length];
                for (int i = 0; i < floorButtons.Length; i++)
                {
                    if (floorButtons[i] == null) continue;

                    floorButtonImages[i] = floorButtons[i].GetComponent<Image>();

                    int floorIndex = i;
                    floorButtons[i].onClick.AddListener(() => OnFloorButtonClicked(floorIndex));
                }
            }
        }

        private void Start()
        {
            ResetProgressBar();
            SetStatusIdle();
            HighlightCurrentFloorButton();
        }

        private void Update()
        {
            if (ElevatorManager.Instance != null)
            {
                if (currentFloorText != null)
                {
                    currentFloorText.text = floorLabels[Mathf.Clamp(
                        ElevatorManager.Instance.currentFloor, 0, floorLabels.Length - 1)];
                }

                if (!ElevatorManager.Instance.isMoving && activeProgressCoroutine == null)
                {
                    HighlightCurrentFloorButton();
                }
            }
        }

        public void OnFloorButtonClicked(int targetFloorIndex)
        {
            if (ElevatorManager.Instance == null)
            {
                Debug.LogError("ElevatorPanelController: ElevatorManager.Instance belum ada di scene!");
                return;
            }

            // ==================== BARU: PROTEKSI LANTAI TERTINGGI ====================
            // Karena lantai ada 4 (GF=0, F1=1, F2=2, F3=3), maka lantai tertinggi adalah indeks ke-3 (length - 1)
            int highestFloorIndex = floorButtons.Length - 1; 

            if (targetFloorIndex == highestFloorIndex)
            {
                // Langsung munculkan teks "Access Denied", putar SFX error, dan batalkan proses jalan
                ShowAccessDenied();
                return;
            }
            // =========================================================================

            // Validasi via core logic Orang 1. Return false = ditolak.
            bool accepted = ElevatorManager.Instance.MoveToFloor(targetFloorIndex);

            if (!accepted)
            {
                ShowAccessDenied();
                return;
            }

            // Diterima -> jalankan progress bar coroutine
            PlaySFX(buttonClickClip);

            if (activeProgressCoroutine != null) StopCoroutine(activeProgressCoroutine);
            activeProgressCoroutine = StartCoroutine(MoveProgressRoutine(targetFloorIndex));
        }

        private IEnumerator MoveProgressRoutine(int targetFloorIndex)
        {
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

            ElevatorManager.Instance.TeleportPlayer(targetFloorIndex);

            SetStatusArrived(targetFloorIndex);

            // BARU: Mainkan SFX Ding saat status berubah jadi 'Arrived'
            PlaySFX(arrivalDingClip);

            yield return new WaitForSeconds(0.6f);

            ResetProgressBar();
            SetStatusIdle();
            LockButtonsVisual(false);
            HighlightCurrentFloorButton();

            activeProgressCoroutine = null;
        }

        public void UpdateProgressBar(float value)
        {
            if (progressBarFill == null) return;
            progressBarFill.fillAmount = Mathf.Clamp01(value);
            progressBarFill.color = colorProgress;
        }

        private void ResetProgressBar()
        {
            if (progressBarFill == null) return;
            progressBarFill.fillAmount = 0f;
            progressBarFill.color = colorProgress;
        }

        private void ShowAccessDenied()
        {
            if (statusText != null)
            {
                statusText.text = "Access Denied";
                statusText.color = colorDenied;
            }

            // BARU: Mainkan SFX Error saat akses ditolak
            PlaySFX(accessDeniedClip);

            StartCoroutine(ResetDeniedAfterDelay());
        }

        private IEnumerator ResetDeniedAfterDelay()
        {
            yield return new WaitForSeconds(deniedMessageDuration);
            if (ElevatorManager.Instance != null && !ElevatorManager.Instance.isMoving)
            {
                SetStatusIdle();
            }
        }

        private void SetStatusIdle()
        {
            if (statusText == null) return;
            statusText.text = "Idle";
            statusText.color = colorTextNormal;
        }

        private void SetStatusMoving(int targetIndex)
        {
            if (statusText == null) return;
            string label = floorLabels != null && targetIndex >= 0 && targetIndex < floorLabels.Length
                ? floorLabels[targetIndex] : targetIndex.ToString();
            statusText.text = $"Moving... {label}";
            statusText.color = colorTextNormal;
        }

        private void SetStatusArrived(int targetIndex)
        {
            if (statusText == null) return;
            string label = floorLabels != null && targetIndex >= 0 && targetIndex < floorLabels.Length
                ? floorLabels[targetIndex] : targetIndex.ToString();
            statusText.text = $"Arrived - {label}";
            statusText.color = colorActive;
        }

        private void LockButtonsVisual(bool locked)
        {
            if (floorButtons == null) return;
            for (int i = 0; i < floorButtons.Length; i++)
            {
                if (floorButtons[i] == null) continue;
                floorButtons[i].interactable = !locked;
            }
        }

        private void HighlightTargetButton(int targetIndex)
        {
            if (floorButtonImages == null) return;
            for (int i = 0; i < floorButtonImages.Length; i++)
            {
                if (floorButtonImages[i] == null) continue;
                floorButtonImages[i].color = (i == targetIndex) ? colorActive : colorNormal;
            }
        }

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

        // BARU: Fungsi helper jembatan untuk memutar audio secara aman
        private void PlaySFX(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}