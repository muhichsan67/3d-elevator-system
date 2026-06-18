using UnityEngine;
using StarterAssets; // Namespace wajib jika menggunakan Unity Starter Assets

namespace ElevatorSystem
{
    /// <summary>
    /// Mengatur kemunculan kursor mouse secara otomatis saat Player mendekati panel lift.
    /// </summary>
    public class ElevatorTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Cek apakah yang mendekati panel adalah Player
            if (other.CompareTag("Player"))
            {
                // Munculkan dan bebaskan kursor mouse agar bisa klik UI
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // Matikan input rotasi kamera Starter Assets agar kamera tidak ikut berputar liar
                var inputs = other.GetComponent<StarterAssetsInputs>();
                if (inputs != null) inputs.cursorInputForLook = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Kunci kembali kursor ke tengah layar saat player menjauh
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Aktifkan kembali input rotasi kamera Starter Assets
                var inputs = other.GetComponent<StarterAssetsInputs>();
                if (inputs != null) inputs.cursorInputForLook = true;
            }
        }
    }
}