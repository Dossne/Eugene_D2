using Cysharp.Threading.Tasks;
using Infrastructure.InputControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.TooltipControl;
using Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
namespace Features.Tutorial
{
    public class MaskTutorialPanel : MonoBehaviour
    {
        private const string TOOLTIP_ASSET_ID = "SimpleTutorialTooltip";

        [SerializeField] private GameObject raycastMaskPf;
        [SerializeField] private TutorialPointer pointerPf;
        [SerializeField] private TutorialButton buttonPf;
        [SerializeField] private Transform raycastMaskParent;
        [SerializeField] private Transform tooltipParent;
        [SerializeField] private Transform pointerParent;
        [SerializeField] private RectTransform buttonParent;

        private Dictionary<string, GameObject> activeMasks = new();
        private List<Image> containImages = new();

        private Dictionary<string, GameObject> activeTooltips = new();
        private Dictionary<string, TutorialPointer> activePointers = new();
        private Dictionary<string, TutorialButton> activeButtons = new();
        private HashSet<object> owners = new();

        private TooltipService tooltipService;
        private InputService inputService;
        private Dictionary<string, CancellationTokenSource> tooltipCts = new();

        private static Dictionary<UiOrientationType, (Vector3 direction, Quaternion rotation)> orientations = new() 
        {
            { UiOrientationType.TopDown    , (Vector3.up   , Quaternion.Euler(0  , 180, 180)) },
            { UiOrientationType.BottomUp   , (Vector3.down , Quaternion.Euler(0  ,   0,   0)) },
            { UiOrientationType.LeftToRight, (Vector3.left , Quaternion.Euler(0  ,   0, -90)) },
            { UiOrientationType.RightToLeft, (Vector3.right, Quaternion.Euler(180,   0,  90)) },
        };

        public RectTransform ButtonParent => buttonParent;
        public void Construct(TooltipService tooltipService, InputService inputService) 
        {
            this.tooltipService = tooltipService;
            this.inputService = inputService;
        }

        public void Acquire(object owner) 
        { 
            inputService.ResetInput();
            owners.Add(owner);
            gameObject.SetActive(owners.Count > 0);
        }

        public void Release(object owner) 
        {
            owners.Remove(owner);
            gameObject.SetActive(owners.Count > 0);
        }

        public List<string> AddNewComplexMask(RectTransform targetTransform, bool excludeOverlapped = true)
        {
            List<string> maskList = new();
            var targetImages = targetTransform.GetComponentsInChildren<Image>(false);
            var length = targetImages.Length;

            if (length == 0)
                return maskList;

            if (length == 1)
            {
                maskList.Add(AddNewMask(targetImages[0].rectTransform, targetImages[0]));
                return maskList;
            }

            containImages = GetMaskImageList(targetImages, excludeOverlapped);

            for (int i = 0; i < containImages.Count; i++)
                maskList.Add(AddNewMask(containImages[i].rectTransform, containImages[i]));

            return maskList;
        }

        public void RemoveComplexMask(List<string> maskList)
        {
            if (maskList == null)
                return;

            for (int i = 0; i < maskList.Count; i++)
                RemoveMask(maskList[i]);
        }

        public string AddNewMask(RectTransform targetTransform, Image maskImageOverride = null) 
        {
            string maskId = Utils.GenerateUniqueId();
            activeMasks.Add(maskId, Instantiate(raycastMaskPf, raycastMaskParent));
            SetupMaskTransform(activeMasks[maskId], targetTransform);
            if (maskImageOverride != null)
            {
                var image = activeMasks[maskId].GetComponent<Image>();
                image.sprite = maskImageOverride.sprite;
                image.type = maskImageOverride.type;
                image.pixelsPerUnitMultiplier = maskImageOverride.pixelsPerUnitMultiplier;
            }
            return maskId;
        }

        public void RemoveMask(string maskId) 
        { 
            if (!activeMasks.TryGetValue(maskId, out var mask))
                return;

            Destroy(mask);
            activeMasks.Remove(maskId);
        }


        public string AddNewButton(RectTransform targetTransform, Action callback)
        {
            string buttonId = Utils.GenerateUniqueId();
            activeButtons.Add(buttonId, Instantiate(buttonPf, buttonParent));
            RectTransform rt = targetTransform != null ? targetTransform : buttonParent.GetComponent<RectTransform>();
            SetupButtonTransform(activeButtons[buttonId].gameObject, rt);
            activeButtons[buttonId].Setup(callback);
            return buttonId;
        }

        public void RemoveButton(string buttonId)
        {
            if (!activeButtons.TryGetValue(buttonId, out var button))
                return;

            button.Dispose();
            activeButtons.Remove(buttonId);
        }

        public string AddNewTooltip(string text,
                                    RectTransform targetTransform,
                                    UiOrientationType orientationType,
                                    Vector3 configOffset,
                                    Vector2 size)
        {
            string tooltipId = Utils.GenerateUniqueId();

            if (!tooltipCts.TryGetValue(tooltipId, out var cts))
                tooltipCts.Add(tooltipId, new CancellationTokenSource());
            else
                tooltipCts[tooltipId] = new CancellationTokenSource();

            ShowTooltipAsync(tooltipId, text, targetTransform, orientationType, configOffset, size, tooltipCts[tooltipId].Token).Forget();

            return tooltipId;
        }

        public void RemoveTooltip(string tooltipId)
        {
            if (tooltipCts.ContainsKey(tooltipId))
            {
                tooltipCts[tooltipId].Cancel();
                tooltipCts[tooltipId].Dispose();
                tooltipCts.Remove(tooltipId);
            }

            if (!activeTooltips.TryGetValue(tooltipId, out var tooltip))
                return;

            if (tooltip != null)
                tooltip.GetComponent<SimpleTutorialTooltip>().Dispose();

            activeTooltips.Remove(tooltipId);
        }

        public string AddNewPointer(RectTransform targetTransform,
                                    UiOrientationType orientationType,
                                    Vector3 configOffset,
                                    Vector2 size)
        {
            string pointerId = Utils.GenerateUniqueId();
            activePointers.Add(pointerId, Instantiate(pointerPf, pointerParent));
            SetupPointerTransform(activePointers[pointerId].gameObject, targetTransform, orientationType, configOffset, size);
            activePointers[pointerId].Initialize();
            activePointers[pointerId].Show();
            return pointerId;
        }

        public void RemovePointer(string pointerId)
        {
            if (!activePointers.TryGetValue(pointerId, out var pointer))
                return;

            pointer.Dispose();
            activePointers.Remove(pointerId);
        }

        private void SetupMaskTransform(GameObject mask, RectTransform targetTransform)
        {
            var rectTransform = mask.GetComponent<RectTransform>();
            SetupDynamicElementTransform(rectTransform, targetTransform, targetTransform.position, Quaternion.identity, Vector3.zero, targetTransform.rect.size);
        }

        private async UniTaskVoid ShowTooltipAsync(string tooltipId, 
                                                   string text, 
                                                   RectTransform targetTransform, 
                                                   UiOrientationType orientationType,
                                                   Vector3 configOffset,
                                                   Vector2 size,
                                                   CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
                var tooltip = await tooltipService.CreateSimpleTutorialTooltipAsync<SimpleTutorialTooltip>(TOOLTIP_ASSET_ID, tooltipParent.GetComponent<RectTransform>(), cancellationToken, false, true);
                activeTooltips.Add(tooltipId, tooltip.gameObject);
                SetupTooltipTransform(activeTooltips[tooltipId], targetTransform, orientationType, configOffset, size);
                tooltip.Initialize();
                tooltip.Show(text, orientationType);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
                return;
            }
        }

        private void SetupTooltipTransform(GameObject tooltip, 
                                           RectTransform targetTransform,
                                           UiOrientationType orientationType,
                                           Vector3 configOffset,
                                           Vector2 size)
        {
            var orientation = orientations[orientationType];
            var rectTransform = tooltip.GetComponent<RectTransform>();
            Vector3 offsetVector = Vector3.zero;
            switch (orientationType)
            {
                case UiOrientationType.TopDown:
                case UiOrientationType.BottomUp:
                    offsetVector = orientation.direction * (targetTransform.rect.size.y / 2 + rectTransform.rect.size.y / 2) + configOffset;
                    break;
                case UiOrientationType.LeftToRight:
                case UiOrientationType.RightToLeft:
                    //orientation.rotation == Quaternion.identity, using X/2 + X/2
                    offsetVector = orientation.direction * (targetTransform.rect.size.x / 2 + rectTransform.rect.size.x / 2) + configOffset;
                    break;
            }
            SetupDynamicElementTransform(rectTransform, targetTransform, targetTransform.position, Quaternion.identity, offsetVector, size);
        }

        private void SetupPointerTransform(GameObject pointer, 
                                           RectTransform targetTransform,
                                           UiOrientationType orientationType,
                                           Vector3 configOffset,
                                           Vector2 size)
        {
            var orientation = orientations[orientationType];
            var rectTransform = pointer.GetComponent<RectTransform>();
            Vector3 offsetVector = Vector3.zero;
            switch (orientationType)
            {
                case UiOrientationType.TopDown:
                case UiOrientationType.BottomUp:
                    offsetVector = orientation.direction * (targetTransform.rect.size.y / 2 + rectTransform.rect.size.y / 2) + configOffset;
                    break;
                case UiOrientationType.LeftToRight:
                case UiOrientationType.RightToLeft:
                    //orientation.rotation != Quaternion.identity, using X/2 + Y/2
                    offsetVector = orientation.direction * (targetTransform.rect.size.x / 2 + rectTransform.rect.size.y / 2) + configOffset;
                    break;
            }
            SetupDynamicElementTransform(rectTransform, targetTransform, targetTransform.position, orientation.rotation, offsetVector, size);
        }


        private void SetupButtonTransform(GameObject button, RectTransform targetTransform)
        {
            var rectTransform = button.GetComponent<RectTransform>();
            SetupDynamicElementTransform(rectTransform, targetTransform, targetTransform.position, Quaternion.identity, Vector3.zero, targetTransform.rect.size);
        }

        private void SetupDynamicElementTransform(RectTransform elementTransform,
                                                  RectTransform targetTransform,
                                                  Vector3 position,
                                                  Quaternion rotation,
                                                  Vector3 offset,
                                                  Vector2 size)
        {
            elementTransform.SetSiblingIndex(0);
            elementTransform.anchorMin = targetTransform.anchorMin;
            elementTransform.anchorMax = targetTransform.anchorMax;
            elementTransform.pivot     = targetTransform.pivot;
            elementTransform.SetPositionAndRotation(position, rotation);
            elementTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            elementTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   size.y);
            elementTransform.anchoredPosition3D = elementTransform.anchoredPosition3D + offset;
        }
        
        private List<Image> GetMaskImageList(Image[] targetImages, bool excludeOverlapped)
        {
            List<Image> result = new();
            if (!excludeOverlapped)
            {
                result = targetImages.ToList();
                return result;
            }

            int length = targetImages.Length;            
            for (int i = 0; i < length; i++)
            {
                var rectTransform = targetImages[i].rectTransform;
                bool isOverlap = false;
                for (int j = result.Count - 1; j >= 0; j--)
                {
                    if (result[j].rectTransform.Contains(rectTransform))
                    {
                        isOverlap = false;
                        break;
                    }

                    if (rectTransform.Contains(result[j].rectTransform))
                        result.RemoveAt(j);

                    isOverlap = true;
                }
                if (isOverlap || result.Count < 1)
                    result.Add(targetImages[i]);
            }
            return result;
        }
    }
}
