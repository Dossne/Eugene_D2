using UnityEngine;

namespace Infrastructure.Utilities
{
    public static class CanvasUtils
    {
        /// <summary>
        /// Set rect transform position clamped to safe zone. Guaranteed that all rect area will be seen on screen
        /// </summary>
        /// <param name="targetTransform">Rect transform which should be positioned</param>
        /// <param name="calcTransform">Rect transform corners of which should be checked, in most cases equals to targetTransform</param>
        /// <param name="safeSpace">Space that should be added in calculations</param>
        /// <param name="correctiveCoefH">Current screen height corrective coefficient relative to main canvas height 1920</param>
        /// <param name="correctiveCoefW">Current screen width corrective coefficient relative to main canvas width 1080</param>
        public static void SetClampedPosition(RectTransform targetTransform, RectTransform calcTransform, float safeSpace = 0f, float correctiveCoefW = 1f, float correctiveCoefH = 1f)
        {
            Vector3[] corners = new Vector3[4];
            calcTransform.GetWorldCorners(corners);
            SetClampedPositionHorizontal(targetTransform, corners, safeSpace, correctiveCoefW);
            SetClampedPositionVertical(targetTransform, corners[1], safeSpace, correctiveCoefH);
        }
        
        
        /// <summary>
        /// Set rect transform vertical position clamped to safe zone
        /// </summary>
        /// <param name="targetTransform">Rect transform which should be positioned</param>
        /// <param name="upperCorner">Position of upper corner to check. See GetWorldCorners unity method</param>
        /// <param name="safeSpace">Space that should be added in calculations</param>
        /// <param name="correctiveCoefH">Current screen height corrective coefficient relative to main canvas height 1920</param>
        public static void SetClampedPositionVertical(RectTransform targetTransform, Vector3 upperCorner, float safeSpace = 0f, float correctiveCoefH = 1f)
        {
            var safeSpaceCorrected = safeSpace * correctiveCoefH;
            if (upperCorner.y > Screen.safeArea.height - safeSpaceCorrected)
            {
                var posY = upperCorner.y - Screen.safeArea.height + safeSpaceCorrected;
                targetTransform.position -= new Vector3(0, posY);
            }
        }

        /// <summary>
        /// Set rect transform horizontal position clamped to safe zone
        /// </summary>
        /// <param name="targetTransform">Rect transform which should be positioned</param>
        /// <param name="corners">Rect transform corners. See GetWorldCorners unity method</param>
        /// <param name="safeSpace">Space that should be added in calculations</param>
        /// <param name="correctiveCoefW">Current screen width corrective coefficient relative to main canvas width 1080</param>
        public static void SetClampedPositionHorizontal(RectTransform targetTransform, Vector3[] corners, float safeSpace = 0f, float correctiveCoefW = 1f)
        {
            var safeSpaceCorrected = safeSpace * correctiveCoefW;

            if (corners[0].x <= safeSpaceCorrected)
            {
                var posX = corners[0].x - safeSpaceCorrected;
                targetTransform.position -= new Vector3(posX, 0);
            }
            else if (corners[3].x >= Screen.safeArea.width - safeSpaceCorrected)
            {
                var posX = corners[3].x - Screen.safeArea.width + safeSpaceCorrected;
                targetTransform.position -= new Vector3(posX, 0);
            }
        }
    }
}