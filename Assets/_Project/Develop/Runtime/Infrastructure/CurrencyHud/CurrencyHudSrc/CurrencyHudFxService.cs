using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.WalletSystem;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Pool;
using UnityEngine;
using VContainer;
using Infrastructure.Configs;
using Infrastructure.CameraControl;
using Infrastructure.Pool.FloatingText;
using Infrastructure.HapticControl;
using Features.CurrencyView;
using Infrastructure.AudioControl;
using Infrastructure.Collections;


namespace Infrastructure.CurrencyHud
{
    public class CurrencyHudFxService : MonoBehaviour
    {
        public class SheduledData
        {
            public int amount;
            public int totalStartValue;


            public SheduledData(int amount, int totalStartValue)
            {
                this.amount = amount;
                this.totalStartValue = totalStartValue;
            }

        }

        
        private Dictionary<CurrencyType, SheduledData> scheduled = new();

        private CurrencyHudService hudService;
        private RectTransform fxFlyToUiStartPos;
        private PoolService poolService;
        private CurrencyModelPool currencyModelPool;
        private FloatingTextPool floatingTextPool;
        private ConfigProvider configProvider;
        private Overlay3dCameraService overlay3dCameraService;
        private bool isInit;
        
        [Inject]
        public void Construct(
            CurrencyHudService hudService,
            MainUIProvider mainUIProvider,
            PoolService poolService,
            ConfigProvider configProvider,
            Overlay3dCameraService overlay3dcameraService)
        {
            this.hudService = hudService;
            this.poolService = poolService;
            this.fxFlyToUiStartPos = mainUIProvider.FxFlyToUiStartArea;
            this.configProvider = configProvider;
            this.overlay3dCameraService = overlay3dcameraService;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            floatingTextPool = poolService.Get<FloatingTextPool>();
            currencyModelPool = poolService.Get<CurrencyModelPool>();
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            scheduled.Clear();
            isInit = false;
        }


        public void Schedule(CurrencyType currency, int amount, int totalStartValue)
        {
            if (scheduled.ContainsKey(currency))
            {
                SheduledData sheduledData = scheduled[currency];
                sheduledData.amount += amount;
                sheduledData.totalStartValue = Mathf.Min(sheduledData.totalStartValue, totalStartValue);
                return;
            }

            scheduled.Add(currency, new SheduledData(amount, totalStartValue));
        }


        public async UniTask ExecuteScheduledAsync(CancellationToken cancellationToken)
        {
            if (scheduled.Count == 0)
                return;

            overlay3dCameraService.Acquire(this);
            CancellationTokenRegistration scheduledReg = cancellationToken.Register(() => { overlay3dCameraService.Release(this); });

            var scheduledCopy = new Dictionary<CurrencyType, SheduledData>(scheduled);
            scheduled.Clear();

            configProvider.CurrencyModelAnimConfig.TryGet(FxType.FlyToUi, out CurrencyModelFxData fxData);

            AudioService.I.PlaySfx(SfxType.CoinsAppear);
            using PooledList<UniTask> flyingTasks = PooledList<UniTask>.Get();
            
            foreach (var item in scheduledCopy)
            {
                CurrencyType currencyType = item.Key;
                SheduledData data = item.Value;

                ShowFlyToUiText(data.amount);

                CancellationTokenRegistration reg = cancellationToken.Register(() => { hudService.RefreshOnIncrease(currencyType, data.totalStartValue + data.amount); });

                int totalStartValue = data.totalStartValue;
                int totalItemsCount = Mathf.Min(data.amount, fxData.itemsCount);
                int remainingAmount = data.amount;

                for (int i = 0; i < totalItemsCount; i++)
                {
                    int amountPerItem = Mathf.Max(1, remainingAmount / (totalItemsCount - i));
                    remainingAmount -= amountPerItem;
                    totalStartValue += amountPerItem;
                    flyingTasks.Add(FlyModelAsync(i + 1, currencyType, totalStartValue, fxData, cancellationToken));
                    await UniTask.WaitForSeconds(fxData.delay, true, cancellationToken: cancellationToken);
                }

                reg.Dispose();
            }

            await UniTask.WhenAll(flyingTasks);
            await UniTask.WaitForEndOfFrame(this);
            overlay3dCameraService.Release(this);
            scheduledReg.Dispose();
        }


        private void ShowFlyToUiText(int amount)
        {
            if (floatingTextPool.TryGetItem(FloatingTextType.FlyToUiFx, out PoolableFloatingText flyText))
            {
                flyText.Show($"+{amount}", default, Vector2.one, isAnchorPos: true);
            }
        }


        private UniTask FlyModelAsync(int idx, CurrencyType currency, int amount, CurrencyModelFxData fxData, CancellationToken cancellationToken)
        {
            if (!hudService.TryGetIconTarget(currency, out RectTransform iconTransform))
                return UniTask.CompletedTask;

            float zDistance = idx * fxData.zOffset;
            Vector2 randomStartPoint = new Vector2(Random.Range(fxFlyToUiStartPos.rect.xMin, fxFlyToUiStartPos.rect.xMax),
                                                   Random.Range(fxFlyToUiStartPos.rect.yMin, fxFlyToUiStartPos.rect.yMax));
            Vector3 startPosition = overlay3dCameraService.RectTransformToWorldPointOverlay3DCamera(fxFlyToUiStartPos, randomStartPoint, zDistance);
            Vector3 endPosition = overlay3dCameraService.RectTransformToWorldPointOverlay3DCamera(iconTransform, iconTransform.rect.center, zDistance);

            if (!currencyModelPool.TryGetItem(currency, out PoolableCurrencyModel currencyModel))
                return UniTask.CompletedTask;
            
            return currencyModel.FlyAsync(fxData, startPosition, endPosition, () =>
            {
                hudService.Refresh(currency, amount, true, Utilities.ActionType.Set);
                PlayFx(fxData);

            }, cancellationToken);
        }


        private void PlayFx(CurrencyModelFxData fxData)
        {
            HapticService.I.Haptic(fxData.hapticOnFinish);
            AudioService.I.PlaySfx(SfxType.CoinWalletApply);
        }


#if UNITY_EDITOR
        [SerializeField] private int debugCount = 50;


        [TriInspector.Button]
        private void TestFly_Editor()
        {
            Schedule(CurrencyType.Coins, debugCount, hudService.GetCurrentState(CurrencyType.Coins));
            ExecuteScheduledAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
        }


#endif

    }
}