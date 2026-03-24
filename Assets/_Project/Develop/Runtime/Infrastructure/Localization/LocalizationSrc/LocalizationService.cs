using Infrastructure.Settings;
using Infrastructure.SystemsLifeCycle;

namespace Infrastructure.Localization
{
    public class LocalizationService : Singleton<LocalizationService>
    {
        private LocalizationConfig config;
        private SystemSettingsConfig systemSettingsConfig;


        public void Construct(LocalizationConfig config, SystemSettingsConfig systemSettingsConfig)
        {
            this.config = config;
            this.systemSettingsConfig = systemSettingsConfig;
        }


        public string Get(string key, string val1 = null, string val2 = null, string val3 = null, string val4 = null, string val5 = null)
        {
#if PR_SAYKIT_ENABLED
            if (systemSettingsConfig.SettingsData.isUsingSayKit)
                return SayKit.getLocalizedString(key, val1, val2, val3, val4, val5); 
#endif
            
            if (config == null || !config.TryGet(key, out LocalizationData data))
            {
                return "null_key";
            }
            
            string text = data.en;
            if (string.IsNullOrEmpty(val1))
                return text;
            
            text = text.Replace("{1}", val1);
            
            if (string.IsNullOrEmpty(val2))
                return text;
            
            text = text.Replace("{2}", val2);
            
            if (string.IsNullOrEmpty(val3))
                return text;
            
            text = text.Replace("{3}", val3);

            if (string.IsNullOrEmpty(val4))
                return text;

            text = text.Replace("{4}", val4);

            if (string.IsNullOrEmpty(val5))
                return text;

            text = text.Replace("{5}", val5);

            return text;
        }
    }
}