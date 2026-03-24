namespace Infrastructure.Localization
{
    public static class LocKeys
    {
        public static class Settings
        {
            public const string Title = "setting_title";
            public const string Music = "setting_music";
            public const string Sound = "setting_sound";
            public const string Haptic = "setting_haptic";
            public const string Quality = "settings_quality";
            public const string Fps = "settings_fps";
            public const string LeaveLevel = "settings_leave_level";
            public const string WithdrawConsent = "settings_withdraw_consent";
            public const string RestorePurchases = "settings_restore_purchases";
        }

        public static class Utils
        {
            public const string TimeD = "utils_t_d";
            public const string TimeH = "utils_t_h";
            public const string TimeM = "utils_t_m";
            public const string TimeS = "utils_t_s";
        }

        public static class Ads
        {
            public const string Loading = "notify_ads_load";
            public const string NotFinish = "notify_ads_not_finish";
        }

        public static class NoAds
        {
            public const string NoAdsText = "no_ads";
            public const string NoAdsPlusText = "no_ads_plus";
        }

        public static class NoAdsRecommendation
        {
            public const string Header = "no_ads_rec_header";
            public const string Description = "no_ads_rec_description";
            public const string WithAdsDescription = "no_ads_rec_with_ads_description";
            public const string NoAdsDescription = "no_ads_rec_no_ads_description";
            public const string WithAdsButton = "no_ads_rec_with_ads_button";
            public const string NoAdsButton = "no_ads_rec_no_ads_button";
        }

        public static class Shop
        {
            public const string ShopText = "shop";
        }

        public static class Team
        {
            public const string TeamsText = "teams";
        }

        public static class Character
        {
            public const string Size = "character_size";
            public const string LevelUp = "character_lvlup_size";
            public const string SizeUp = "character_size_up";
        }

        public static class MetaHud
        {
            public const string StartGame = "metahud_start_game";
            public const string DifficultyHard = "metahud_difficulty_hard";
            public const string DifficultyVeryHard = "metahud_difficulty_very_hard";
            public const string DifficultyInsane = "metahud_difficulty_insane";
        }

        public static class CompletePopup
        {
            public const string Header = "popup_compl_hdr";
            public const string Btn = "popup_compl_btn";
        }

        public static class LoosePopup
        {
            public const string Header = "popup_loose_hdr";
            public const string Btn = "popup_loose_btn";
        }

        public static class MessagePopup
        {
            public const string Load = "message_popup_load";
            public const string OkBtn = "message_popup_ok";
        }

        public static class ResurrectPopup
        {
            public const string HeaderBomb = "popup_resurr_hdr_bomb";
            public const string BombCondition = "popup_resurr_bomb_cnd";
            public const string HeaderTime = "popup_resurr_hdr_time";
            public const string TimeCondition = "popup_resurr_time_cnd";
            public const string LooseItem = "popup_resurr_hdr_loose_itm";
            public const string LoseAllAchievements = "popup_resurr_hdr_lose_all";
            public const string Btn = "popup_resurr_btn";
            public const string AdsButton = "popup_resurr_ads_btn";
            public const string AdsButtonTime = "popup_resurr_ads_btn_time";

            public const string LostWinstreak = "popup_resurr_lost_winstreak";
            public const string LostRewardTrack = "popup_resurr_lost_reward_track";
            public const string LostLavaQuest = "popup_resurr_lost_lava_quest";
            public const string LostCoins = "popup_resurr_lost_coins";
            public const string LostLives = "popup_resurr_lost_lives";
            public const string LostSuperSpeed = "popup_resurr_lost_superspeed";
            public const string LostCompetitionWinstreak = "popup_resurr_competition_winstreak";
        }

        public static class Currency
        {
            public const string NotEnough = "not_enough_currency";
        }

        public static class Boosters
        {
            public const string Lock = "boosters_lock";
            public const string Free = "boosters_free";
            public const string PopupReward = "preboost_popup_reward";
            public const string SelectBoosters = "preboost_popup_select_booster";
            public const string Level = "preboost_popup_level";
            public const string BoosterBuy = "boost_buy_popup_btn";
            public const string FloatingTextInfiniteBooster = "floating_text_infinite_booster";
            public const string BoosterName = "_name";
        }

        public static class Purchase
        {
            public const string NoInternet = "floating_text_no_internet";
            public const string LoadingPopupConnecting = "loading_popup_connecting";
            public const string LoadingPopupPurchasing = "loading_popup_purchasing";
            public const string SuccessPurchase = "shop_reward_popup_success_purchase";
            public const string TapToCollect = "tap_to_collect";
            public const string BestPrice = "best_price_offer";
            public const string PopularOffer = "popular_offer";
        }

        public static class Life
        {
            public const string FullLife = "full_life";
        }

        public static class QuitLevelPopup
        {
            public const string Header = "quit_level_popup_header";
            public const string QuitButton = "quit_level_popup_quit";
            public const string LoseLifeText = "quit_level_popup_lose_life";
            public const string LoseCoins = "quit_level_popup_lose_coins";
            public const string LoseAllItems = "quit_level_popup_lose_all_items";
        }

        public static class CollectableTutorialDescription
        {
            public const string Bomb = "collectable_tutorial_description_bomb";
        }

        public static class CollectableTutorialPopup
        {
            public const string Header = "collectable_tutorial_popup_header";
            public const string Button = "collectable_tutorial_popup_button";
        }

        public static class FirstBombResurrectPopup
        {
            public const string Header = "first_bomb_resurrect_popup_header";
            public const string Description = "first_bomb_resurrect_popup_description";
            public const string Button = "first_bomb_resurrect_popup_button";
        }

        public static class RewardTrackTutorial
        {
            public const string UnlockTooltip = "reward_track_tutorial_unlock_tooltip";
        }

        public static class PlayerProfileTutorial
        {
            public const string WidgetTooltip = "player_profile_tutorial_widget_tooltip";
        }

        public static class ItemTutorialPopup
        {
            public const string SubHeader = "item_tutorial_popup_subheader";
            public const string TapButton = "item_tutorial_popup_tapbutton";
        }

        public static class BuyLifePopup
        {
            public const string Header = "buy_life_popup_header";
            public const string Refill = "buy_life_popup_refill";
            public const string TimeToNext = "buy_life_popup_time_to_next";
            public const string Rewarded = "buy_life_popup_rewarded";
        }

        public static class PreBoosterPopup
        {
            public const string PlayButton = "pre_booster_popup_play";
            public const string RetryButton = "pre_booster_popup_retry";
            public const string LevelFailed = "pre_booster_popup_level_failed";
            public const string SuperSpeedWidgetTooltip = "pre_booster_popup_ss_widget_tooltip";
        }

        public static class Common
        {
            public const string TapToContinue = "tap_to_continue";
            public const string Locked = "locked";
            public const string Level = "level";
            public const string ComingSoon = "coming_soon";
            public const string Free = "free";
        }

        public static class WinStreak
        {
            public const string PanelHeader = "win_streak_pnl_hdr";
            public const string PanelTooltip = "win_streak_pnl_tooltip";

            public const string BannerHeader = "win_streak_ban_hdr";
            public const string Unlock = "win_streak_ban_unlock";
        }

        public static class BestDeal
        {
            public const string BestDeal1 = "rh_best_deal_1";
            public const string BestDeal2 = "rh_best_deal_2";
            public const string BestDeal3 = "rh_best_deal_3";
        }

        public static class RewardTrack
        {
            public const string Lock = "reward_track_lock";
            public const string StateHeader = "reward_track_state_header";
            public const string Tooltip = "reward_track_popup_tooltip";
            public const string LastReward = "reward_track_last_reward";
        }

        public static class LavaQuest
        {
            public const string Name = "lava_quest_name";
            public const string Start = "lava_quest_start";
            public const string Finish = "lava_quest_finish";
            public const string StartPopupDescr = "lava_quest_start_popup_descr";
            public const string StartPopupFail = "lava_quest_start_popup_fail";
            public const string EventComplete = "lava_quest_event_popup_win";
            public const string EventLoose = "lava_quest_event_popup_loose";
            public const string EventFinished = "lava_quest_event_popup_expire";
            public const string EventPopupDescr = "lava_quest_event_popup_descr";
            public const string InfoPopupPlayers = "lava_quest_info_popup_players";
            public const string InfoPopupLevels = "lava_quest_info_popup_levels";
            public const string InfoPopupShare = "lava_quest_info_popup_winshare";
            public const string CongratPopupSharing = "lava_quest_congrat_popup_sharing";
            public const string WidgetLevel = "lava_quest_widget_level";
            public const string WidgetTooltip = "lava_quest_widget_tooltip";

            public const string Tutorial_1 = "lava_quest_tutor_1";
            public const string Tutorial_2 = "lava_quest_tutor_2";
            public const string Tutorial_3 = "lava_quest_tutor_3";
        }

        public static class Tooltips
        {
            public const string MoveTutorialTooltip = "move_tutorial_tooltip";
            public const string EatTutorialTooltip = "eat_tutorial_tooltip";
            public const string FinishLevelTutorialTooltip = "finish_level_tutorial_tooltip";
            public const string SuperSpeedTutorialTooltip = "super_speed_tutorial_tooltip";
        }

        public static class LoadScreen
        {
            public const string Loading = "loadscreen_loading";
        }

        public static class Support
        {
            public const string SupportButton = "support_button";
        }

        public static class SuperSpeedActivationPopup
        {
            public const string Header = "ss_activation_popup_header";
            public const string Description = "ss_activation_popup_description";
        }

        public static class SuperSpeedInfoPopup
        {
            public const string Header = "ss_info_popup_header";
            public const string Levels = "ss_info_popup_levels";
            public const string Activation = "ss_info_popup_activation";
            public const string Boost = "ss_info_popup_boost";
        }

        public static class SeasonPass
        {
            public const string Free = "season_pass_free";
            public const string Prem = "season_pass_prem";
            public const string WidgetTooltip = "season_pass_widget_tooltip";
            public const string LockProgressTooltip = "season_pass_lock_by_progress";
            public const string LockInAppTooltip = "season_pass_lock_by_inapp";
        }

        public static class PlayerProfilePopup
        {
            public const string NoConnectionTooltip = "profile_popup_no_connection";
            public const string StatHeader = "profile_popup_stat_header";
            public const string StatSlotFirstWin = "profile_popup_first_win";
            public const string StatSlotTotalWin = "profile_popup_total_win";
            public const string StatSlotWinStreak = "profile_popup_win_streak";
        }

        public static class PlayerProfileEditPopup
        {
            public const string Header = "profile_edit_popup_header";
            public const string SaveButton = "profile_edit_popup_save";
            public const string TabAvatar = "profile_edit_popup_tab_avatar";
            public const string TabFrame = "profile_edit_popup_tab_frame";
            public const string TabName = "profile_edit_popup_tab_name";
            public const string TabToken = "profile_edit_popup_tab_token";
            public const string InvalidSymbolAlert = "profile_edit_popup_invalid_symbol";
            public const string InvalidNameAlert = "profile_edit_popup_invalid_name";
            public const string ExplicitNameAlert = "profile_edit_popup_explicit_name";
            public const string SaveSuccessAlert = "profile_edit_popup_save_success";
            public const string AvatarNotSelectedAlert = "profile_edit_popup_avatar_not_selected";
        }

        public static class PlayerProfileNamePopup
        {
            public const string Header = "profile_name_popup_header";
            public const string Change = "profile_name_popup_change";
            public const string SaveButton = "profile_name_popup_save";
        }

        public static class PlayerProfileAvatarPopup
        {
            public const string Header = "profile_avatar_popup_choose";
        }

        public class Competition
        {
            public const string Lock = "competition_lock";
            public const string Complete = "competition_complete_key";
            public const string WidgetTooltip = "competition_widget_tooltip";
            public const string WidgetLevel = "competition_widget_level";
            public const string WidgetOpen = "competition_widget_open";
            public const string WidgetFinished = "competition_widget_finished";
            public const string StatePopupHeader = "competition_state_popup_header";

            public const string BeginPopupHeader = "competition_begin_popup_header";
            public const string BeginPopupDescr = "competition_begin_popup_descr";
            public const string BeginPopupButton = "competition_begin_popup_button";
            public const string MultiTooltip1 = "competition_m_tooltip_1";
            public const string MultiTooltip2 = "competition_m_tooltip_2";
            public const string RewardTrackTooltip = "competition_reward_track_tooltip";
            public const string AlertNoConnectionStart = "competition_alert_no_connect_start";
            public const string AlertNoConnection = "competition_alert_no_connect";
            public const string AlertWait = "competition_alert_wait";
            public const string RankHeader = "competition_rank_header";
            public const string RewardHeader = "competition_reward_header";
            public const string StageRewardPopupHeader = "competition_reward_track_header";
            public const string Continue = "competition_continue";
            public const string ServerRewardPopupHeader = "competition_reward_server_header";
        }
    }
}