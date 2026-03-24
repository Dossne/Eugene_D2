using Cysharp.Threading.Tasks;
using Nakama;
using SayGames.Services.Leaderboards;
using System.Threading;
using UnityEngine;


namespace Features.Social
{
    public class LeaderboardNakamaClient
    {
        const long StatusCodeAlreadyJoined = 500;

        private readonly SocialService socialService;



        public LeaderboardNakamaClient(SocialService socialService)
        {
            this.socialService = socialService;
        }


        public async UniTask<ILeaderboardState> GetLeaderboardStateAsync(string leaderboardName, CancellationToken cancellationToken = default)
        {
            if (!await socialService.TryAuthenticateAsync())
            {
                return null;
            }

            try
            {
                ILeaderboardState leaderboardState = await socialService.Client.GetLeaderboardStateAsync(socialService.Session,
                    leaderboardName,
                    canceller: cancellationToken).AsUniTask();
                return leaderboardState;
            }
            catch
            {
                Debug.LogWarning("Get Leaderboard State failed!");
            }

            return null;
        }


        public async UniTask<IApiLeaderboardRecordList> ListLeaderboardRecordsAroundOwnerAsync(string leaderboardName, int limit, CancellationToken cancellationToken = default)
        {
            if (!await socialService.TryAuthenticateAsync())
            {
                return null;
            }

            try
            {
                IApiLeaderboardRecordList leaderboardRecordList = await socialService.Client.ListLeaderboardRecordsAroundOwnerAsync(
                    socialService.Session,
                    leaderboardName,
                    socialService.Session.UserId,
                    limit: limit,
                    canceller: cancellationToken).AsUniTask();
                return leaderboardRecordList;
            }
            catch
            {
                Debug.LogWarning("List Leaderboard Records Around Owner failed!");
            }

            return null;
        }


        public async UniTask<bool> JoinLeaderboardAsync(string leaderboardName, CancellationToken cancellationToken = default)
        {
            if (!await socialService.TryAuthenticateAsync())
            {
                return false;
            }

            try
            {
                await socialService.Client.JoinLeaderboardAsync(socialService.Session, leaderboardName);
                return true;
            }
            catch (ApiResponseException apiResponseException)
            {
                if (apiResponseException.StatusCode == StatusCodeAlreadyJoined)
                {
                    return true;
                }
            }

            Debug.LogWarning("Join Leaderboard failed!");
            return false;
        }


        public async UniTask<bool> WriteRecordAsync(string leaderboardName, long score, long subscore, CancellationToken cancellationToken = default)
        {
            if (!await socialService.TryAuthenticateAsync())
            {
                return false;
            }

            try
            {
                await socialService.Client.WriteLeaderboardRecordAsync(socialService.Session,
                    leaderboardName,
                    score,
                    subscore,
                    canceller: cancellationToken).AsUniTask();
                return true;
            }
            catch
            {
                Debug.LogWarning("Write Leaderboard Record failed!");
            }

            return false;
        }
    }
}

