//  -----------------------------------------------------------------------
//  <copyright file="WynnAnalyticsManager.cs" company="Torpedo Labs">
//   Copyright (c) 2017 Torpedo Labs All rights reserved.
//  </copyright>
//  <author>Ramy Eldaoushy</author>
//  -----------------------------------------------------------------------

using System.Collections.Generic;
using Com.TorpedoLabs.Propeller.Analytics;
using Com.TorpedoLabs.Propeller.Extensions;
using Com.TorpedoLabs.Wynn.Data;
using Com.TorpedoLabs.Wynn.Casino;
using Com.TorpedoLabs.Propeller;
using System;
using Com.TorpedoLabs.Propeller.Debugging;
using Com.TorpedoLabs.Propeller.GameServicesSystem;
using Com.TorpedoLabs.Wynn.Backend;
using Com.TorpedoLabs.Wynn.Facebook;
using Com.TorpedoLabs.Wynn.GameModules;
using UnityEngine;

namespace Com.TorpedoLabs.Wynn.Analytics
{
    public class WynnAnalyticsManager : IAnalyticsManager
    {
        private const string LobbyLastLogTimeKey = "WynnAnalyticsManager.LobbyLastLogTimeKey";
        private const string GameLastLogTimeKey = "WynnAnalyticsManager.GameLastLogTimeKey_";


        private List<IAnalyticsLibraryWrapper> wrappers;
        private string advertisingId = "N/A";

        private SpinResultAnalyticsTO spinResults;

        public GameServiceTypeEnum GameServiceType
        {
            get { return GameServiceTypeEnum.Core; }
        }

        public string ServiceReport
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Init()
        {
            if (wrappers != null)
            {
                GameEngine.LogWarning("WynnAnalyticsManager is already initialized.  Aborting initialization request.");
                return;
            }

            Application.RequestAdvertisingIdentifierAsync(OnAdvertisingIdResult);
            wrappers = new List<IAnalyticsLibraryWrapper>();
            wrappers.Add(SetupFacebook());
            wrappers.Add(SetupAmplitude());
#if !UNITY_EDITOR
            //Rudder SDK only works on device, crashes and has errors when running in editor.
            //Add Rudder Client to wrapper collection
            GameEngine.LogError("WynnAnalyticsManager: Initialized RudderWrapper");
            wrappers.Add(SetupRudder());
#endif
            spinResults = null;
        }

        private void OnAdvertisingIdResult(string advertisingId, bool trackingEnabled, string errorMsg)
        {
            if (advertisingId.HasValue())
            {
                this.advertisingId = advertisingId;
            }
        }

        public string GetAdvertisingId()
        {
            if (advertisingId == "N/A")
            {
                return null;
            }

            return advertisingId;
        }

        public void AppendAppsFlyer()
        {
            wrappers.Add(SetupAppsFlyer());
        }

        public void AppendAdjustManager()
        {
            wrappers.Add(SetupAdjustManager());
        }

        public void ResumeSession()
        {
            if (wrappers != null)
            {
                foreach (var wrapper in wrappers)
                {
                    wrapper.ResumeSession();
                }
            }
        }

        public void PauseSession()
        {
            if (wrappers != null)
            {
                foreach (var wrapper in wrappers)
                {
                    wrapper.PauseSession();
                }
            }
        }

        public void Destroy()
        {
            if (wrappers != null)
            {
                foreach (var wrapper in wrappers)
                {
                    wrapper.Destroy();
                }
            }
        }

        public bool ShouldRecordLobbyTime
        {
            get
            {
                string lastDateStr = null;
                GameEngine.LocalPlayerDataManager.GetString(LobbyLastLogTimeKey, out lastDateStr);

                DateTime lastDateTime = DateTime.MinValue;

                if (lastDateStr.HasValue())
                {
                    lastDateTime = DateTime.Parse(lastDateStr);
                }

                int hourSpan = 24;

                if (WynnEngine.SettingsConfigController != null)
                {
                    hourSpan = WynnEngine.SettingsConfigController.LoadTimeLoggingIntervalHours;
                }

                return (DateTime.Now - lastDateTime).TotalHours >= hourSpan;
            }
        }

        public bool ShouldRecordSlotGameLoadTime(CasinoGamesEnum casinoGame)
        {
            string lastDateStr = null;
            GameEngine.LocalPlayerDataManager.GetString(GameLastLogTimeKey + casinoGame.ToString(), out lastDateStr);

            DateTime lastDateTime = DateTime.MinValue;

            if (lastDateStr.HasValue())
            {
                lastDateTime = DateTime.Parse(lastDateStr);
            }

            int hourSpan = 24;

            if (WynnEngine.SettingsConfigController != null)
            {
                hourSpan = WynnEngine.SettingsConfigController.LoadTimeLoggingIntervalHours;
            }

            return (DateTime.Now - lastDateTime).TotalHours >= hourSpan;
        }

        private IAnalyticsLibraryWrapper SetupFacebook()
        {
            FacebookAnalyticsManager fbAnalyticsManager = new FacebookAnalyticsManager();
            fbAnalyticsManager.Init(this);
            return fbAnalyticsManager;
        }

        private IAnalyticsLibraryWrapper SetupAmplitude()
        {
            AmplitudeAnalyticsManager manager = new AmplitudeAnalyticsManager();
            manager.Init(this);
            return manager;
        }

        //Rudder initialization
        private IAnalyticsLibraryWrapper SetupRudder()
        {
            RudderAnalyticsManager manager = new RudderAnalyticsManager();
            manager.Init(this);
            GameEngine.LogError("WynnAnalyticsManager: Initialized RudderAnalyticsManager");
            return manager;
        }

        private IAnalyticsLibraryWrapper SetupAppsFlyer()
        {
            AppsFlyerAnalyticsManager afAnalyticsManager = new AppsFlyerAnalyticsManager();
            afAnalyticsManager.Init(this);
            return afAnalyticsManager;
        }

        private IAnalyticsLibraryWrapper SetupAdjustManager()
        {
            AdjustAnalyticsManager adjustAnalyticsManager = new AdjustAnalyticsManager();
            adjustAnalyticsManager.Init(this);
            return adjustAnalyticsManager;
        }

        public Dictionary<string, object> EventsCommonData()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            try
            {
                if (WynnEngine.PlayerId.HasValue())
                {
                    PlayerStatsStateController statsController = WynnEngine.GetStateController<PlayerStatsStateController>();
                    parameters[WynnAnalyticsDataConstants.AT_USER_ID] = WynnEngine.PlayerId;
                    parameters[WynnAnalyticsDataConstants.ME_USER_LEVEL] = statsController.PlayerLevel;
                    parameters[WynnAnalyticsDataConstants.ME_COIN_BALANCE] = statsController.RegularCurrency;
                    parameters[WynnAnalyticsDataConstants.ME_GEM_BALANCE] = statsController.PremiumCurrency;
                    parameters[WynnAnalyticsDataConstants.ME_LIFETIME_GEM_BALANCE] = statsController.LifeTimePremiumCurrency > 0 ? statsController.LifeTimePremiumCurrency : statsController.PremiumCurrency;
                    parameters[WynnAnalyticsDataConstants.ME_TOTAL_PAYMENTS] = statsController.TotalPayments;
                    parameters[WynnAnalyticsDataConstants.ME_PLAYER_TOTAL_BATTLES] = statsController.PlayerStatsTO.BattlesPlayed;
                    parameters[WynnAnalyticsDataConstants.ME_PLAYER_TOTAL_SHIELDS] = statsController.PlayerStatsTO.Shields;
                    parameters[WynnAnalyticsDataConstants.AT_ABTEST_TAGS] = GetABTestTags();
                    parameters[WynnAnalyticsDataConstants.AT_ABTEST_VARIANT] = GetABTestVariants();
                    parameters[WynnAnalyticsDataConstants.AT_START_DATE] = statsController.CreateDate;
                    parameters[WynnAnalyticsDataConstants.AT_FB_PROFILE] = statsController.PlayerStatsTO.FacebookId > 0 ? "1" : "0";
                }
                else
                {
                    parameters[WynnAnalyticsDataConstants.AT_USER_ID] = "N/A";
                    parameters[WynnAnalyticsDataConstants.ME_USER_LEVEL] = -1;
                    parameters[WynnAnalyticsDataConstants.ME_COIN_BALANCE] = -1;
                    parameters[WynnAnalyticsDataConstants.ME_GEM_BALANCE] = -1;
                    parameters[WynnAnalyticsDataConstants.ME_TOTAL_PAYMENTS] = -1;
                    parameters[WynnAnalyticsDataConstants.ME_PLAYER_TOTAL_SHIELDS] = -1;
                    parameters[WynnAnalyticsDataConstants.AT_ABTEST_TAGS] = "N/A";
                    parameters[WynnAnalyticsDataConstants.AT_ABTEST_VARIANT] = "N/A";
                }

                if (GameEngine.HasService<WynnGameModulesManger>())
                {
                    WynnGameModulesManger modulesManger = GameEngine.GetService<WynnGameModulesManger>();
                    parameters[WynnAnalyticsDataConstants.AT_CURRENT_MODULE_NAME] = modulesManger.CurrentModuleName;
                    parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = modulesManger.CurrentCasinoGameEnum.ToString();
                }
                else
                {
                    parameters[WynnAnalyticsDataConstants.AT_CURRENT_MODULE_NAME] = "N/A";
                    parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = "N/A";
                }

                if (GameEngine.LocalPlayerDataManager != null)
                {
                    parameters[WynnAnalyticsDataConstants.ME_VERSION_SESSION_COUNT] = GameEngine.LocalPlayerDataManager.GetVersionNumberOfSessions();
                }
                else
                {
                    parameters[WynnAnalyticsDataConstants.ME_VERSION_SESSION_COUNT] = -1;
                }

                parameters[WynnAnalyticsDataConstants.AT_INTERNET_REACHABILITY] = GameEngine.InternetReachability.ToString();
                parameters[WynnAnalyticsDataConstants.AT_IDFA] = advertisingId;
                parameters[WynnAnalyticsDataConstants.ME_FPS] = Convert.ToInt64(GameEngine.CurrentFPS);
                parameters[WynnAnalyticsDataConstants.AT_GRAPHICS_QUALITY] = GameEngine.IsSdPerformance ? "SD" : "HD";
                parameters[WynnAnalyticsDataConstants.AT_IS_LOW_END_DEVICE] = GameEngine.IsPossiblyLowEndDevice;

                return parameters;
            }
            catch (Exception e)
            {
                GameEngine.LogError(e);
                return parameters;
            }
        }

        public void RecordCustomEvent(string eventType, Dictionary<string, object> eventData)
        {
            if (wrappers != null)
            {
                foreach (var wrapper in wrappers)
                {
                    wrapper.RecordCustomEvent(eventType, eventData);
                }
            }
        }

        public void RecordPurchase(string id, double price, double amountPurchased, string currency = null, string store = null, string transactionId = null)
        {
            if (wrappers != null)
            {
                foreach (var wrapper in wrappers)
                {
                    wrapper.RecordPurchase(id, price, amountPurchased, currency, store, transactionId);
                }
            }
        }

        public void RecordSpinResult(string gameId,
                                     long bet,
                                     long bet_multiplier,
                                     long win,
                                     long jackpotWin,
                                     string progressiveVOId,
                                     bool isF,
                                     bool isTurboMode,
                                     string competitiveGameId,
                                     bool isAutoSpin,
                                     string tournamentId,
                                     bool isHighRoller,
                                     string featureGameType,
                                     long betLevel,
                                     int additionalBetIndex,
                                     string bingoDroppedNumbers,
                                     string extraParams)
        {
            string wonProgressiveType = string.Empty;

            if (progressiveVOId.HasValue())
            {
                SlotsProgressivesDataVO progressiveVO = GameEngine.GetVO<SlotsProgressivesDataVO>(progressiveVOId);

                if (progressiveVO != null)
                {
                    wonProgressiveType = progressiveVO.JackpotType;
                }
            }

            String preProgressiveType = string.Empty;



            if (spinResults == null)
            {
                spinResults = new SpinResultAnalyticsTO
                {
                    GameId = gameId,
                    Bet = bet,
                    BetMultiplier = bet_multiplier,
                    Win = win,
                    NoOfSpin = 1,
                    IsF = isF,
                    IsAutoSpin = isAutoSpin,
                    IsTurboMode = isTurboMode,
                    IsHighRoller = isHighRoller,
                    JackpotAmount = jackpotWin,
                    JackpotType = wonProgressiveType,
                    TournamentId = tournamentId,
                    CompetitiveGameId = competitiveGameId,
                    FeatureGameType = featureGameType,
                    BetLevel = betLevel,
                    AdditionalBetIndex = additionalBetIndex,
                    BingoDroppedNumbers = bingoDroppedNumbers,
                    EParam = extraParams
                };
            }
            else
            {
                preProgressiveType = spinResults.JackpotType;
                spinResults.Bet += bet;
                spinResults.Win += win;
                spinResults.NoOfSpin += 1;
                spinResults.IsAutoSpin = isAutoSpin;
                spinResults.IsTurboMode = isTurboMode;
                spinResults.IsHighRoller = isHighRoller;
                spinResults.TournamentId = tournamentId;
                spinResults.IsF = isF;
                spinResults.BetLevel = betLevel;
                spinResults.AdditionalBetIndex = additionalBetIndex;
                spinResults.BingoDroppedNumbers = bingoDroppedNumbers;
                if (StringExtensions.HasValue(extraParams) && StringExtensions.HasValue(spinResults.EParam))
                {
                    spinResults.EParam += "&" + extraParams;
                }
                else
                {
                    spinResults.EParam = extraParams;
                }
            }

            if (ShouldFireSpinResult(gameId, bet_multiplier, competitiveGameId, preProgressiveType, featureGameType))
            {
                FireSpinResultEvent();
            }
        }


        public void FireSpinResultEvent()
        {
            if (spinResults == null)
            {
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            string eventName = spinResults.CompetitiveGameId.HasValue() ? WynnAnalyticsDataConstants.AE_RECORD_BATTLE_SPIN_RESULT : WynnAnalyticsDataConstants.AE_SPIN_RESULT;

            data[WynnAnalyticsDataConstants.AT_GAME_ID] = spinResults.GameId;
            data[WynnAnalyticsDataConstants.ME_BET_MULTIPLIER] = spinResults.BetMultiplier;
            data[WynnAnalyticsDataConstants.ME_BET] = spinResults.Bet;
            data[WynnAnalyticsDataConstants.ME_WIN] = spinResults.Win;
            data[WynnAnalyticsDataConstants.ME_NO_OF_SPIN] = spinResults.NoOfSpin;
            data[WynnAnalyticsDataConstants.ME_WIN_JACKPOT_AMOUNT] = spinResults.JackpotAmount;
            data[WynnAnalyticsDataConstants.AT_WIN_JACKPOT_TYPE] = spinResults.JackpotType;
            data[WynnAnalyticsDataConstants.AT_IS_F] = spinResults.IsF;
            data[WynnAnalyticsDataConstants.AT_IS_TURBO] = spinResults.IsTurboMode;
            data[WynnAnalyticsDataConstants.AT_BATTLE_ID] = spinResults.CompetitiveGameId;
            data[WynnAnalyticsDataConstants.AT_IS_AUTO_SPIN] = spinResults.IsAutoSpin;
            data[WynnAnalyticsDataConstants.AT_TOURNAMENT_ID] = spinResults.TournamentId;
            data[WynnAnalyticsDataConstants.AT_IS_HIGH_ROLLER] = spinResults.IsHighRoller;
            data[WynnAnalyticsDataConstants.AT_FEATURE_GAME_TYPE] = spinResults.FeatureGameType;
            data[WynnAnalyticsDataConstants.ME_BET_LEVEL] = spinResults.BetLevel;
            data[WynnAnalyticsDataConstants.ME_ADDITIONAL_BET_INDEX] = spinResults.AdditionalBetIndex;
            data[WynnAnalyticsDataConstants.AT_BINGO_DROPPED_NUMBERS] = spinResults.BingoDroppedNumbers;
            data[WynnAnalyticsDataConstants.AT_EXTRA_PARAM] = spinResults.EParam;
            data[WynnAnalyticsDataConstants.ME_DAYS_IN_GAME] = WynnEngine.GetStateController<PlayerStatsStateController>().AgeInDays;

            RecordCustomEvent(eventName, data);

            spinResults = null;
        }

        private bool ShouldFireSpinResult(string gameId, long bet_multiplier, string competitiveGameId, string wonProgressiveType, string featureGameType)
        {
            if (spinResults == null)
            {
                return false;
            }

            if (featureGameType.HasValue())
            {
                return true;
            }

            if (spinResults.GameId != gameId)
            {
                return true;
            }

            if (spinResults.BetMultiplier != bet_multiplier)
            {
                return true;
            }

            if (spinResults.CompetitiveGameId != competitiveGameId)
            {
                return true;
            }

            if (spinResults.JackpotType.HasValue() || spinResults.JackpotType != wonProgressiveType)
            {
                return true;
            }

            if (spinResults.FeatureGameType.HasValue())
            {
                return true;
            }

            int spinResultBatchSize = 10;

            try
            {
                spinResultBatchSize = WynnEngine.SettingsConfigController.AnalyticsSpinResultBatchSize;

            }
            catch (Exception e)
            {
                GameEngine.LogError(e);
                spinResultBatchSize = 10;
            }

            if (spinResults.NoOfSpin >= spinResultBatchSize)
            {
                return true;
            }


            return false;
        }

        public void RecordFeatureGameSelection(string gameId, CasinoGamesEnum gameEnum, int currentTriggerCount,
                                               string featureGameType, bool playFeature)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_ID] = gameId;
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = gameEnum.ToString();
            parameters[WynnAnalyticsDataConstants.ME_CURRENT_TRIGGER_COUNT] = currentTriggerCount;
            parameters[WynnAnalyticsDataConstants.AT_FEATURE_GAME_TYPE] = featureGameType;
            parameters[WynnAnalyticsDataConstants.AT_PLAY_FEATURE_GAME] = playFeature;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_FEATURE_GAME_SELECTION, parameters);
        }

        public void RecordAutoSpinSelection(int numberOfSpins, long bet)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_AUTO_SPIN_COUNT] = numberOfSpins;
            parameters[WynnAnalyticsDataConstants.ME_BET] = bet;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_AUTO_SPIN_SELECTION, parameters);
        }

        public void RecordResumeGameInvoked(string gameId, CasinoGamesEnum gameEnum)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_ID] = gameId;
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = gameEnum.ToString();

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RESUME_GAME_INVOKED, parameters);
        }

        public void RecordDailyBonusClaim(string id, long bonusAmount, long jackpotReward, int loginCount,
                                          long spinReward)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_DAILY_BONUS_ID] = id;
            parameters[WynnAnalyticsDataConstants.ME_DAILY_BONUS_AMOUNT] = bonusAmount;
            parameters[WynnAnalyticsDataConstants.ME_DAILY_JACKPOT_AMOUNT] = jackpotReward;
            parameters[WynnAnalyticsDataConstants.ME_DAILY_BONUS_LOGIN_COUNT] = loginCount;
            parameters[WynnAnalyticsDataConstants.ME_DAILY_BONUS_SPIN_REWARD] = spinReward;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_DAILY_BONUS, parameters);
        }

        public void RecordDailyRewardClaim(string id, long rewardAmount, long minutesSinceReady, long rewardMultiplier)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_DAILY_REWARD_ID] = id;
            parameters[WynnAnalyticsDataConstants.ME_DAILY_REWARD_AMOUNT] = rewardAmount;
            parameters[WynnAnalyticsDataConstants.ME_DAILY_REWARD_MULTIPLIER] = rewardMultiplier;
            parameters[WynnAnalyticsDataConstants.ME_MINUTES_SINCE_READY] = minutesSinceReady;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_DAILY_REWARDS_CLAIM, parameters);
        }

        public void RecordFBIncentiveClaim(long amount)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_REWARD_AMOUNT_1] = amount;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_FB_INCENTIVE_CLAIM, parameters);
        }

        public void RecordOutOfSyncSpin(string gameId, CasinoGamesEnum gameEnum, long serverWins, long clientWins, string battleId, long betLevel, bool isHighRoller)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_ID] = gameId;
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = gameEnum.ToString();
            parameters[WynnAnalyticsDataConstants.ME_SERVER_SPIN_WIN] = serverWins;
            parameters[WynnAnalyticsDataConstants.ME_CLIENT_SPIN_WINS] = clientWins;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.ME_BET_LEVEL] = betLevel;
            parameters[WynnAnalyticsDataConstants.AT_IS_HIGH_ROLLER] = isHighRoller;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_OUT_OF_SYNC_SPIN, parameters);
        }

        public void RecordSyncStateError(CasinoGamesEnum gameEnum, long serverCoins, long clientCoins, long serverXP, long clientXP, string battleId, bool isHighRoller)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = gameEnum.ToString();
            parameters[WynnAnalyticsDataConstants.ME_SERVER_COINS] = serverCoins;
            parameters[WynnAnalyticsDataConstants.ME_CLIENT_COINS] = clientCoins;
            parameters[WynnAnalyticsDataConstants.ME_SERVER_XP] = serverXP;
            parameters[WynnAnalyticsDataConstants.ME_CLIENT_XP] = clientXP;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.AT_IS_HIGH_ROLLER] = isHighRoller;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_SYNC_STATE_ERROR, parameters);
        }

        public void RecordSettingsViewClosed(float sfxVolume, float musicVolume, float ambienceVolume)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_SFX_VOL] = sfxVolume;
            parameters[WynnAnalyticsDataConstants.ME_MUSIC_VOL] = musicVolume;
            parameters[WynnAnalyticsDataConstants.ME_AMBIENCE_VOL] = ambienceVolume;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_SETTINGS_VIEW_CLOSED, parameters);
        }

        public void RecordHelpButtonClick()
        {
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_HELP_BUTTON_EVENT, null);
        }

        public void RecordStoreOpen(string moduleId)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_CURRENT_MODULE_NAME] = moduleId;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_STORE_OPEN, parameters);
        }

        public void RecordDeepLinkRewarded(string deepLinkId, string rewardType, long rewardAmount)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_DEEP_LINK_ID] = deepLinkId;
            parameters[WynnAnalyticsDataConstants.AT_REWARD_TYPE_1] = rewardType;
            parameters[WynnAnalyticsDataConstants.ME_REWARD_AMOUNT_1] = rewardAmount;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_DEEP_LINK_REWARDED, parameters);
        }

        public void RecordNotEnoughPopupAction(string action)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_NOT_ENOUGHT_CREDIT_ACTION] = action;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_NOT_ENOUGH_CREDIT_POPUP, parameters);
        }

        public void RecordLowCoinPromoPopupAction(string action)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_NOT_ENOUGHT_CREDIT_ACTION] = action;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_LOW_COIN_PROMO_POPUP, parameters);
        }

        public void RecordBuyProductClick(string productStoreId)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_PRODUCT_ID] = productStoreId;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_BUY_PRODUCT_CLICK, parameters);
        }

        public void RecordAchievementClaim(string id, long coin_reward, long gem_reward, long xp_reward)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_ACHIEVEMENT_ID] = id;
            parameters[WynnAnalyticsDataConstants.ME_ACHIEVEMENT_COIN_REWARD] = coin_reward;
            parameters[WynnAnalyticsDataConstants.ME_ACHIEVEMENT_GEM_REWARD] = gem_reward;
            parameters[WynnAnalyticsDataConstants.ME_ACHIEVEMENT_XP_REWARD] = xp_reward;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_ACHIEVEMENT_CLAIM, parameters);
        }

        public void RecordGameInitTime(string name,
                                        long totalLoadTime,
                                        long playerDataLoadTime,
                                        long manfiestSyncTime,
                                        long iapInitTime,
                                        long assetsLoadTime)
        {
            if (!ShouldRecordLobbyTime)
            {
                return;
            }

            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_TOTAL_LOAD_TIME] = totalLoadTime;
            parameters[WynnAnalyticsDataConstants.ME_PLAYER_DATA_LOAD_TIME] = playerDataLoadTime;
            parameters[WynnAnalyticsDataConstants.ME_MANFIEST_SYNC_TIME] = manfiestSyncTime;
            parameters[WynnAnalyticsDataConstants.ME_IAP_INIT_TIME] = iapInitTime;
            parameters[WynnAnalyticsDataConstants.ME_ASSETS_LOAD_TIME] = assetsLoadTime;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_GAME_INIT_TIME, parameters);
        }

        public void RecordPlayerHomeLoadTime(string name,
                                       long totalLoadTime,
                                       long totalInitTime,
                                       long uiInitTime)
        {
            if (!ShouldRecordLobbyTime)
            {
                return;
            }

            GameEngine.LocalPlayerDataManager.SetString(LobbyLastLogTimeKey, DateTime.Now.ToString());

            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_TOTAL_LOAD_TIME] = totalLoadTime;
            parameters[WynnAnalyticsDataConstants.ME_PLAYER_HOME_INIT_TIME] = totalInitTime;
            parameters[WynnAnalyticsDataConstants.ME_UI_INIT_TIME] = uiInitTime;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_PLAYER_HOME_LOAD_TIME, parameters);
        }

        public void RecordCasinoGameLoadTime(string name,
                                             CasinoGamesEnum casinoGame,
                                             long totalLoadTime,
                                             long assetsLoadTime,
                                             long serverResponseTime,
                                             bool isAssetsCached)
        {
            if (!ShouldRecordSlotGameLoadTime(casinoGame))
            {
                return;
            }

            GameEngine.LocalPlayerDataManager.SetString(GameLastLogTimeKey + casinoGame.ToString(), DateTime.Now.ToString());

            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = casinoGame.ToString();
            parameters[WynnAnalyticsDataConstants.ME_TOTAL_LOAD_TIME] = totalLoadTime;
            parameters[WynnAnalyticsDataConstants.ME_ASSETS_LOAD_TIME] = assetsLoadTime;
            parameters[WynnAnalyticsDataConstants.ME_SERVER_RESPONSE_TIME] = serverResponseTime;
            parameters[WynnAnalyticsDataConstants.AT_IS_ASSETS_CACHED] = isAssetsCached;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_CASINO_GAME_LOAD_TIME, parameters);
        }

        public void RecordTransitionError(string moduleName)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_CURRENT_MODULE_NAME] = moduleName;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_TRANSITION_ERROR, parameters);
        }

        public void LogLobbyFPS()
        {
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_LOBBY_FPS, null);
        }

        public void LogServiceErrorEvent(int failedResponseResultCode, string responseName, long httpResponseCode)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_SERVER_RESPONSE_RESULT] = failedResponseResultCode;
            parameters[WynnAnalyticsDataConstants.AT_SERVER_RESPONSE_NAME] = responseName;
            parameters[WynnAnalyticsDataConstants.AT_HTTP_RESPONSE_CODE] = httpResponseCode;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_SERVER_RESPONSE_ERROR, parameters);
        }

        public void LogBatchError(string batcherUrl, long httpResponseCode)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATCH_URL] = batcherUrl;
            parameters[WynnAnalyticsDataConstants.AT_HTTP_RESPONSE_CODE] = httpResponseCode;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATCH_ERROR, parameters);
        }

        public void LogBundleLoadError(string bundleUrl, string error, long httpResponseCode)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BUNDLE_URL] = bundleUrl;
            parameters[WynnAnalyticsDataConstants.AT_ERROR] = error;
            parameters[WynnAnalyticsDataConstants.AT_HTTP_RESPONSE_CODE] = httpResponseCode;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BUNDLE_LOAD_ERROR, parameters);
        }

        public void RecordeLevelUpEvent(int level, string rewardType1, long rewardAmount1, string rewardType2, long rewardAmount2)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_USER_LEVEL] = level;
            parameters[WynnAnalyticsDataConstants.AT_REWARD_TYPE_1] = rewardType1;
            parameters[WynnAnalyticsDataConstants.ME_REWARD_AMOUNT_1] = rewardAmount1;
            parameters[WynnAnalyticsDataConstants.AT_REWARD_TYPE_2] = rewardType2;
            parameters[WynnAnalyticsDataConstants.ME_REWARD_AMOUNT_2] = rewardAmount2;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_PLAYER_LEVEL_UP, parameters);
        }

        public void RecordPushNotificationReceived(string id)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_PUSH_CAMPAIGN_ID] = id;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_PUSH_NOTIFICATION_RECEIVED, parameters);
        }

        public void RecordPushNotificationClick(string id)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_PUSH_CAMPAIGN_ID] = id;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_PUSH_NOTIFICATION_CLICK, parameters);
        }

        public void RecordSpecialOfferDisplayed(string offerId)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_OFFER_ID] = offerId;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_OFFER_DISPLAYED, parameters);
        }

        public void RecordSpecialOfferBuyClick(string productId)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_PRODUCT_ID] = productId;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_OFFER_BUY_CLICK, parameters);
        }

        public void RecordSpecialOfferDismissClick(string offerId)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_OFFER_ID] = offerId;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_OFFER_DISMISS_CLICK, parameters);
        }

        public void RecordBattleMatchMaking(string battleId, CasinoGamesEnum selectedGame, long betChoice, bool isFBProfile, long totalBattles, long rank, int winRatio, int matchMakingSeconds)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = selectedGame.ToString();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_IS_FB_PROFILE] = isFBProfile;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_BET_CHOICE] = betChoice;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_RANK] = rank;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_WIN_RATIO] = winRatio;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_MATCH_MAKING_TIME_SEC] = matchMakingSeconds;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_MATCH_MAKING, parameters);
        }

        public void RecordBattleEnd(string battleId, string selectedGameEnum, long betChoice, string winLossTie, long totalWin, long playerBalance, bool isAbandoned, long timeSinceSpinsEnded)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = selectedGameEnum;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_BET_CHOICE] = betChoice;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_END_RESULT] = winLossTie;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_TOTAL_WIN] = totalWin;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_END_PLAYER_BALANCE] = playerBalance;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_TIME_SINCE_SPINS_ENDED] = timeSinceSpinsEnded;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_IS_ABANDONED] = isAbandoned;

            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_END, parameters);
        }

        public void RecordBattleRedeem(long redeemAmount)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_REDEEM_AMOUNT] = redeemAmount;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_REDEEM, parameters);
        }

        public void RecordBattleDisconnected(string battleId, bool isWinning, string source)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_WINNING_LOSING] = GetWinLossTieString(false, isWinning);
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_DISCONNECTION_SOURCE] = source;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_DISCONNECTED, parameters);
        }

        public void RecordBattleReconnected(string battleId, bool requireHardReload, bool requireSoftReload, bool isBattleDone,
                                            long disconnectionTime, bool isValidReconnection)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_REQUIRE_HARD_RELOAD] = requireHardReload;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_REQUIRE_SOFT_RELOAD] = requireSoftReload;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_IS_BATTLE_DONE] = isBattleDone;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_DISCONNECTION_TIME] = disconnectionTime;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_IS_VALID_RECONNECTION] = isValidReconnection;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_RECONNECTED, parameters);
        }

        public void RecordBattleSendEmote(string battleId, string emoteId)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_EMOTE_ID] = emoteId;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_SEND_EMOTE, parameters);
        }

        public void RecordBattleViewOpponentProfile(string battleId)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_VIEW_OPPONENT_PROFILE, parameters);
        }

        public void RecordOpenLeaderboard()
        {
            var parameters = new Dictionary<string, object>();
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_OPEN_LEADERBOARD, parameters);
        }

        public void RecordOpenTournaments(string tournamentType)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_TOURNAMENT_TYPE] = tournamentType;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_OPEN_TOURNAMENTS, parameters);
        }


        public void RecordOpenEvent(String eventName)
        {
            RecordCustomEvent(eventName, new Dictionary<string, object>());
        }

        public void RecordOpenHighRoller(bool isHighRoller)
        {
            if (isHighRoller)
            {
                RecordOpenEvent(WynnAnalyticsDataConstants.AE_RECORD_OPEN_HIGH_ROLLER);
            }
            else
            {
                RecordOpenEvent(WynnAnalyticsDataConstants.AE_RECORD_CLOSE_HIGH_ROLLER);
            }
        }


        public void RecordBattlePlayerSpinsEnded(string battleId, long opponentNumberOfSpins)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BATTLE_ID] = battleId;
            parameters[WynnAnalyticsDataConstants.ME_BATTLE_OPPONENT_NUM_OF_SPINS] = opponentNumberOfSpins;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_BATTLE_PLAYER_SPINS_ENDED, parameters);
        }

        public void RecordGiftSendEvent(int count)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.ME_SENT_GIFT_COUNT] = count;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_SENT_GIFT, parameters);
        }

        #region Quests

        public void RecordQuestButtonClick(bool hasActiveQuest)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_HAS_ACTIVE_QUEST] = hasActiveQuest;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_QUEST_BUTTON_CLICK, parameters);
        }

        public void RecordQuestItemClaim(string questId, string questItemId, string rewardType, long rewardAmount, string scratcherVoId, long minutesSinceQuestStart)
        {
            string scratcherType = "N/A";
            string scratcherWinType = "N/A";

            if (scratcherVoId.HasValue())
            {
                ScratcherVO scratcherVo = GameEngine.GetVO<ScratcherVO>(scratcherVoId);
                rewardAmount = scratcherVo.RewardAmount;
                rewardType = "Coins";
                scratcherType = scratcherVo.ScratcherType;
                scratcherWinType = scratcherVo.WinType;
            }

            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_QUEST_ID] = questId;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_ITEM_ID] = questItemId;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_ITEM_REWARD_TYPE] = rewardType;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_ITEM_REWARD_AMOUNT] = rewardAmount;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_MINUTES_SINCE_START] = minutesSinceQuestStart;
            parameters[WynnAnalyticsDataConstants.AT_SCRATCHER_ID] = scratcherVoId;
            parameters[WynnAnalyticsDataConstants.AT_SCRATCHER_TYPE] = scratcherType;
            parameters[WynnAnalyticsDataConstants.AT_SCRATCHER_WIN_TYPE] = scratcherWinType;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_QUEST_ITEM_CLAIM, parameters);
        }

        public void RecordQuestClaim(string questId, string scratcherId, string scratcherType, string scratcherWinType, long scratcherAmount, long minutesSinceQuestStart)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_QUEST_ID] = questId;
            parameters[WynnAnalyticsDataConstants.AT_SCRATCHER_ID] = scratcherId;
            parameters[WynnAnalyticsDataConstants.AT_SCRATCHER_TYPE] = scratcherType;
            parameters[WynnAnalyticsDataConstants.AT_SCRATCHER_WIN_TYPE] = scratcherWinType;
            parameters[WynnAnalyticsDataConstants.AT_SCRATCHER_WIN_AMOUNT] = scratcherAmount;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_MINUTES_SINCE_START] = minutesSinceQuestStart;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_QUEST_CLAIM, parameters);
        }

        public void RecordQuestTimeExpired(string questId, string questState, int numberOfCompletedQuestItems, long elapsedTimeMinutes)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_QUEST_ID] = questId;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_STATE] = questState;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_NUMBER_OF_COMPLETED_ITEMS] = numberOfCompletedQuestItems;
            parameters[WynnAnalyticsDataConstants.AT_QUEST_MINUTES_SINCE_START] = elapsedTimeMinutes;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_QUEST_TIME_EXPIRED, parameters);
        }
        #endregion

        #region Hyper Bonus

        public void RecordHyperBonusClick(CasinoGamesEnum casinoGameEnum)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = casinoGameEnum.ToString();
            parameters[WynnAnalyticsDataConstants.AT_IS_HIGH_ROLLER] = WynnEngine.IsHighRoller.ToString();
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_HYPER_BONUS_CLICK, parameters);
        }

        public void RecordHyperBonusCancel(CasinoGamesEnum casinoGameEnum)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = casinoGameEnum.ToString();
            parameters[WynnAnalyticsDataConstants.AT_IS_HIGH_ROLLER] = WynnEngine.IsHighRoller.ToString();
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_HYPER_BONUS_CANCEL, parameters);
        }

        public void RecordHyperBonusPurchase(CasinoGamesEnum casinoGameEnum, int spins, long cost, long betMultiplier, long betLevel)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_GAME_NAME] = casinoGameEnum.ToString();
            parameters[WynnAnalyticsDataConstants.ME_NO_OF_SPIN] = spins;
            parameters[WynnAnalyticsDataConstants.ME_COST] = cost;
            parameters[WynnAnalyticsDataConstants.ME_BET_MULTIPLIER] = betMultiplier;
            parameters[WynnAnalyticsDataConstants.ME_BET_LEVEL] = betLevel;
            parameters[WynnAnalyticsDataConstants.AT_IS_HIGH_ROLLER] = WynnEngine.IsHighRoller.ToString();
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_RECORD_HYPER_BONUS_PURCHASE, parameters);
        }
        #endregion

        #region Bingo

        public void RecordBingoRewardClaim(string wonLineIndices, string wonLineNumbers, string ranking, string rewardType)
        {
            var parameters = new Dictionary<string, object>();
            parameters[WynnAnalyticsDataConstants.AT_BINGO_WON_LINE_INDICES] = wonLineIndices;
            parameters[WynnAnalyticsDataConstants.AT_BINGO_WON_LINE_NUMBERS] = wonLineNumbers;
            parameters[WynnAnalyticsDataConstants.AT_BINGO_REWARD_TIER] = ranking;
            parameters[WynnAnalyticsDataConstants.AT_BINGO_REWARD_TYPE] = rewardType;
            RecordCustomEvent(WynnAnalyticsDataConstants.AE_BINGO_REWARD_CLAIM, parameters);
        }

        #endregion

        public static string GetWinLossTieString(bool isTie, bool isWin)
        {
            if (isTie)
            {
                return "TIE";
            }
            else if (isWin)
            {
                return "WIN";
            }

            return "LOSS";
        }

        public static long GetWinLossTieReward(bool isTie, bool isWin, long totalPot)
        {
            if (isTie)
            {
                return totalPot / 2;
            }
            else if (isWin)
            {
                return totalPot;
            }

            return 0;
        }

        private string GetABTestTags()
        {
            try
            {
                PlayerStatsStateController statsController = WynnEngine.GetStateController<PlayerStatsStateController>();

                if (statsController.ABTests != null)
                {
                    List<string> testTags = new List<string>();
                    foreach (ABTestTO to in statsController.ABTests)
                    {
                        testTags.Push(to.TestTag);
                    }

                    return String.Join(",", testTags.ToArray());
                }

                return "N/A";
            }
            catch (Exception e)
            {
                GameEngine.LogError(e);
                return "N/A";
            }
        }

        private string GetABTestVariants()
        {
            try
            {
                PlayerStatsStateController statsController = WynnEngine.GetStateController<PlayerStatsStateController>();

                if (statsController.ABTests != null)
                {
                    List<string> testVariants = new List<string>();
                    foreach (ABTestTO to in statsController.ABTests)
                    {
                        testVariants.Push(to.Variant);
                    }
                    return String.Join(",", testVariants.ToArray());
                }
                return "N/A";
            }
            catch (Exception e)
            {
                GameEngine.LogError(e);
                return "N/A";
            }
        }
    }
}
