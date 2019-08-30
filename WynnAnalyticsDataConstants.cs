//  -----------------------------------------------------------------------
//  <copyright file="WynnAnalyticsDataConstants.cs" company="Torpedo Labs">
//   Copyright (c) 2018 Torpedo Labs All rights reserved.
//  </copyright>
//  <author>Ramy Eldaoushy</author>
//  -----------------------------------------------------------------------

using Com.TorpedoLabs.Propeller.Analytics;

namespace Com.TorpedoLabs.Wynn.Analytics
{
    public class WynnAnalyticsDataConstants : BaseAnalyticsDataConstants
    {
        public const string AE_SPIN_RESULT = "spin_result";
        public const string AE_AUTO_SPIN_SELECTION = "auto_spin_selection";
        public const string AE_RESUME_GAME_INVOKED = "resume_game_invoked";
        public const string AE_OUT_OF_SYNC_SPIN = "out_of_sync_spin_event";
        public const string AE_SYNC_STATE_ERROR = "sync_state_error_event";
        public const string AE_RECORD_BATTLE_MATCH_MAKING = "battle_match_making";
        public const string AE_RECORD_BATTLE_SPIN_RESULT = "battle_spin_result";
        public const string AE_RECORD_BATTLE_END = "battle_end";
        public const string AE_RECORD_BATTLE_REDEEM = "battle_redeem";
        public const string AE_RECORD_BATTLE_DISCONNECTED = "battle_disconnected";
        public const string AE_RECORD_BATTLE_RECONNECTED = "battle_reconnected";
        public const string AE_RECORD_BATTLE_SEND_EMOTE = "battle_send_emote";
        public const string AE_RECORD_BATTLE_VIEW_OPPONENT_PROFILE = "battle_view_opponent_profile";
        public const string AE_RECORD_BATTLE_PLAYER_SPINS_ENDED = "battle_player_spins_ended";
        public const string AE_RECORD_OPEN_LEADERBOARD = "open_leaderboard";
        public const string AE_RECORD_OPEN_TOURNAMENTS = "open_tournaments";
        public const string AE_RECORD_OPEN_LOYALTY_REWARDS = "open_loyalty_rewards";
        public const string AE_RECORD_OPEN_DAILY_REWARDS = "open_daily_rewards";
        public const string AE_RECORD_OPEN_HIGH_ROLLER = "open_high_roller_room";
        public const string AE_RECORD_CLOSE_HIGH_ROLLER = "close_high_roller_room";
        public const string AE_RECORD_RATE_US_SHOW = "rate_us_popup_shown";
        public const string AE_RECORD_RATE_US_CLICK_LATER = "rate_us_popup_click_later";
        public const string AE_RECORD_RATE_US_CLICK_RATE = "rate_us_popup_click_rate";
        public const string AE_RECORD_CALIM_TOURNAMENT_REWARD = "claim_tournament_reward";
        public const string AE_RECORD_CALIM_MINI_TOURNAMENT_REWARD = "claim_mini_tournament_reward";
        public const string AE_RECORD_QUEST_BUTTON_CLICK = "quest_button_click";
        public const string AE_RECORD_QUEST_ITEM_CLAIM = "questItemClaim";
        public const string AE_RECORD_QUEST_CLAIM = "questClaim";
        public const string AE_RECORD_QUEST_TIME_EXPIRED = "questTimeExpired";
        public const string AE_RECORD_HYPER_BONUS_CLICK = "hyper_bonus_click";
        public const string AE_RECORD_HYPER_BONUS_CANCEL = "hyper_bonus_cancel";
        public const string AE_RECORD_HYPER_BONUS_PURCHASE = "hyper_bonus_purchase";
        public const string AE_FEATURE_GAME_SELECTION = "feature_game_selection";
        public const string AE_FB_INCENTIVE_CLAIM = "fb_incentive_claim";
        public const string AE_INBOX_MESSAGE_CLICK = "inbox_message_click";
        public const string AE_SENT_GIFT = "sent_gift";

        public const string AT_WIN_JACKPOT_TYPE = "jackpot_win_type";
        public const string AT_CLOSE_COMPETITIVE_CREDIT_PURCHASE_POPUP = "close_not_enough_credit_popup";
        public const string AT_SPIN_INFO_STO = "spin_sto";
        public const string AT_IS_F = "isf";
        public const string AT_IS_TURBO = "is_turbo";
        public const string AT_IS_AUTO_SPIN = "is_auto_spin";
        public const string AT_BATTLE_ID = "battle_id";
        public const string AT_BATTLE_IS_FB_PROFILE = "battle_is_fb_profile";
        public const string AT_BATTLE_IS_ABANDONED = "battle_is_abandoned";
        public const string AT_BATTLE_END_RESULT = "battle_end_result";
        public const string AT_BATTLE_WINNING_LOSING = "battle_winning_losing";
        public const string AT_BATTLE_IS_BATTLE_DONE = "battle_is_battle_done";
        public const string AT_BATTLE_EMOTE_ID = "battle_emote_id";
        public const string AT_BATTLE_DISCONNECTION_SOURCE = "battle_disconnection_source";
        public const string AT_BATTLE_REQUIRE_HARD_RELOAD = "battle_require_hard_reload";
        public const string AT_BATTLE_REQUIRE_SOFT_RELOAD = "battle_require_soft_reload";
        public const string AT_BATTLE_IS_VALID_RECONNECTION = "battle_is_valid_reconnection";
        public const string AT_TOURNAMENT_TYPE = "tournament_type";
        public const string AT_TOURNAMENT_ID = "tournament_id";
        public const string AT_IS_HIGH_ROLLER = "ishighroller";
        public const string AT_FEATURE_GAME_TYPE = "featureGameType";
        public const string AT_PLAY_FEATURE_GAME = "playFeatureGame";
        public const string AT_HAS_ACTIVE_QUEST = "hasActiveQuest";
        public const string AT_QUEST_ID = "questId";
        public const string AT_QUEST_STATE = "questState";
        public const string AT_QUEST_NUMBER_OF_COMPLETED_ITEMS = "questNumberOfCompletedItems";
        public const string AT_QUEST_ITEM_ID = "questItemId";
        public const string AT_QUEST_ITEM_REWARD_TYPE = "questItemRewardType";
        public const string AT_QUEST_ITEM_REWARD_AMOUNT = "questItemRewardAmount";
        public const string AT_QUEST_MINUTES_SINCE_START = "questMinutesSinceStart";
        public const string AT_SCRATCHER_ID = "scratcherId";
        public const string AT_SCRATCHER_WIN_AMOUNT = "scratcherWinAmount";
        public const string AT_SCRATCHER_TYPE = "scratcherType";
        public const string AT_SCRATCHER_WIN_TYPE = "scratcherWinType";
        public const string AT_FB_PROFILE = "fb_profile";
        public const string AT_EXTRA_PARAM = "extra_param";
        public const string AT_MESSAGE_TYPE = "message_type";
        public const string AT_REWARD_TYPE = "reward_type";


        public const string ME_SFX_VOL = "sfx_vol";
        public const string ME_MUSIC_VOL = "music_vol";
        public const string ME_AMBIENCE_VOL = "ambience_vol";
        public const string ME_DAILY_JACKPOT_AMOUNT = "daily_bonus_jackpot_amount";
        public const string ME_BET = "bet_amount";
        public const string ME_BET_MULTIPLIER = "bet_multiplier";
        public const string ME_WIN = "win_amount";
        public const string ME_WIN_JACKPOT_AMOUNT = "jackpot_win_amount";
        public const string ME_SERVER_SPIN_WIN = "server_spin_win";
        public const string ME_CLIENT_SPIN_WINS = "client_spin_wins";
        public const string ME_SERVER_COINS = "server_coins";
        public const string ME_CLIENT_COINS = "client_coins";
        public const string ME_SERVER_XP = "server_xp";
        public const string ME_CLIENT_XP = "client_xp";
        public const string ME_AUTO_SPIN_COUNT = "auto_spin_count";
        public const string ME_BATTLE_BET_CHOICE = "battle_bet_choice";
        public const string ME_BATTLE_TOTAL_BATTLES = "battle_total_battles";
        public const string ME_BATTLE_RANK = "battle_rank";
        public const string ME_BATTLE_WIN_RATIO = "battle_win_ratio";
        public const string ME_BATTLE_MATCH_MAKING_TIME_SEC = "battle_match_making_time_sec";
        public const string ME_BATTLE_TOTAL_WIN = "battle_total_win";
        public const string ME_PLAYER_TOTAL_BATTLES = "player_total_battles";
        public const string ME_PLAYER_TOTAL_SHIELDS = "player_total_shields";
        public const string ME_BATTLE_END_PLAYER_BALANCE = "battle_end_player_balance";
        public const string ME_BATTLE_REDEEM_AMOUNT = "battle_redeem_amount";
        public const string ME_BATTLE_DISCONNECTION_TIME = "battle_disconnection_time";
        public const string ME_BATTLE_OPPONENT_NUM_OF_SPINS = "battle_opponent_num_of_spins";
        public const string ME_BATTLE_TIME_SINCE_SPINS_ENDED = "battle_time_since_spins_ended";
        public const string ME_NO_OF_SPIN = "no_of_spin";
        public const string ME_COST = "cost";
        public const string ME_CURRENT_TRIGGER_COUNT = "current_trigger_count";
        public const string ME_BET_LEVEL = "bet_level";
        public const string ME_ADDITIONAL_BET_INDEX = "additional_bet_index";
        public const string ME_REWARD_AMOUNT = "reward_amount";
        public const string ME_MESSAGE_ID = "message_id";
        public const string ME_SENT_GIFT_COUNT = "send_gift_count";
        public const string ME_DAYS_IN_GAME = "days_in_game";

        // Adding default writeKey for RudderClient. 
        // Get the writeKey from the RudderDashboard by enabling the source "Unity"
        public const string RUDDER_WRITE_KEY = "1Q8oS6rzUqcFrBj08j43l4tmmS7";
    }
}
