using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.WalletSystem;
using Infrastructure.AssetManagement;
using Infrastructure.Configs;
using UnityEngine;
using UnityEngine.U2D;

namespace Infrastructure.SpriteAtlasControl
{
    public class SpriteAtlasService
    {
        private readonly AssetProvider assetProvider;
        private readonly CurrencyConfig currencyConfig;
        private SpriteAtlas mainUIAtlas;
        private SpriteAtlas popupSuperSaleAtlas;


        public SpriteAtlasService(AssetProvider assetProvider, ConfigProvider configProvider)
        {
            this.assetProvider = assetProvider;
            this.currencyConfig = configProvider.CurrencyConfig;
        }


        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            mainUIAtlas = await assetProvider.AddressableLoadAssetAsync<SpriteAtlas>(AssetKeys.AtlasMainUI, cancellationToken);
            popupSuperSaleAtlas = await assetProvider.AddressableLoadAssetAsync<SpriteAtlas>(AssetKeys.AtlasPopupSuperSale, cancellationToken);
        }


        public Sprite GetFromMain(string spriteName)
        {
            return mainUIAtlas.GetSprite(spriteName);
        }


        public Sprite GetFromPopupSuperSale(string spriteName)
        {
            return popupSuperSaleAtlas.GetSprite(spriteName);
        }


        public Sprite GetCurrencyIcon(CurrencyType currencyType)
        {
            return !currencyConfig.TryGet(currencyType, out var data) ? null : mainUIAtlas.GetSprite(data.iconName);
        }

    }
}