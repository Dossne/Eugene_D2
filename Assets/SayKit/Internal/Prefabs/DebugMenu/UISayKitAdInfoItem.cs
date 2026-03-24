using UnityEngine;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class UISayKitAdInfoItem : MonoBehaviour
    {
        public Text adTime;
        public Text adType;
        public Text adNetwork;
        public Text creativeId;

        public void CopyAdInfoToClipboard() {
            var te = new TextEditor
            {
                text = $"{adTime.text}, {adType.text}, {adNetwork.text}, {creativeId.text}"
            };
            te.SelectAll();
            te.Copy();

            SayKitToast.Show("Copied", 0.3f, Color.white, ToastPosition.BottomCenter);
        }
    }
}