using UnityEngine;

/// <summary>
/// Simple adapter to handle screen scaling for PC and Mobile.
/// Attach to Canvas root or bootstrap object.
/// </summary>
namespace MiniIT.CORE
{
    public class ResolutionAdapter : MonoBehaviour
    {
        [SerializeField]
        private Canvas _canvas = null;

        private void Awake()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }

            AdjustCanvasScaler();
        }

        /// <summary>
        /// Configure Canvas scaler for aspect ratios and mobile/pc.
        /// </summary>
        private void AdjustCanvasScaler()
        {
            // Use Unity's Canvas Scaler component manually in editor.
            // This method exists to document adaptation; changes often happen in editor.
        }
    }
}