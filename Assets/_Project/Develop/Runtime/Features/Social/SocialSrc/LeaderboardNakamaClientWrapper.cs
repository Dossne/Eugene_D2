using Cysharp.Threading.Tasks;
using Nakama;
using SayGames.Services.Leaderboards;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Features.Social
{
    public class LeaderboardNakamaClientWrapper
    {
        private readonly LeaderboardNakamaClient leaderboardNakamaClient;
        private readonly LeaderboardRequestEvent leaderboardRequestEvent;

        HashSet<LeaderboardRequestType> forcedFailureRequests = new HashSet<LeaderboardRequestType>();



        public LeaderboardNakamaClientWrapper(LeaderboardNakamaClient leaderboardNakamaClient,
            LeaderboardRequestEvent leaderboardRequestEvent)
        {
            this.leaderboardNakamaClient = leaderboardNakamaClient;
            this.leaderboardRequestEvent = leaderboardRequestEvent;
        }


        public async UniTask<bool> JoinLeaderboardAsync(string leaderboardName, CancellationToken cancellationToken = default)
        {
            LeaderboardRequestType leaderboardRequestType = LeaderboardRequestType.Join;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, LeaderboardRequestState.InProgress);
            bool isSuccess = false;
            if (!forcedFailureRequests.Contains(leaderboardRequestType))
            {
                isSuccess = await leaderboardNakamaClient.JoinLeaderboardAsync(leaderboardName, cancellationToken);
            }

            LeaderboardRequestState leaderboardRequestState = isSuccess ? LeaderboardRequestState.Success : LeaderboardRequestState.Failure;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, leaderboardRequestState);

            return isSuccess;
        }


        public async UniTask<bool> WriteRecordAsync(string leaderboardName, long score, long subscore, CancellationToken cancellationToken = default)
        {
            LeaderboardRequestType leaderboardRequestType = LeaderboardRequestType.SendScore;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, LeaderboardRequestState.InProgress);

            bool isSuccess = false;
            if (!forcedFailureRequests.Contains(leaderboardRequestType))
            {
                isSuccess = await leaderboardNakamaClient.WriteRecordAsync(leaderboardName, score, subscore, cancellationToken);
            }

            LeaderboardRequestState leaderboardRequestState = isSuccess ? LeaderboardRequestState.Success : LeaderboardRequestState.Failure;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, leaderboardRequestState);

            return isSuccess;
        }


        public async UniTask<ILeaderboardState> GetLeaderboardStateAsync(string leaderboardName, CancellationToken cancellationToken = default)
        {
            LeaderboardRequestType leaderboardRequestType = LeaderboardRequestType.GetState;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, LeaderboardRequestState.InProgress);

            ILeaderboardState leaderboardState = null;
            if (!forcedFailureRequests.Contains(leaderboardRequestType))
            {
                leaderboardState = await leaderboardNakamaClient.GetLeaderboardStateAsync(leaderboardName, cancellationToken);
            }

            LeaderboardRequestState leaderboardRequestState = leaderboardState != null ? LeaderboardRequestState.Success : LeaderboardRequestState.Failure;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, leaderboardRequestState);

            return leaderboardState;
        }


        public async UniTask<IApiLeaderboardRecordList> ListLeaderboardRecordsAroundOwnerAsync(string leaderboardName, int limit, CancellationToken cancellationToken = default)
        {
            LeaderboardRequestType leaderboardRequestType = LeaderboardRequestType.GetRecords;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, LeaderboardRequestState.InProgress);

            IApiLeaderboardRecordList leaderboardRecordList = null;
            if (!forcedFailureRequests.Contains(leaderboardRequestType))
            {
                leaderboardRecordList = await leaderboardNakamaClient.ListLeaderboardRecordsAroundOwnerAsync(leaderboardName, limit);
            }

            LeaderboardRequestState leaderboardRequestState = leaderboardRecordList != null ? LeaderboardRequestState.Success : LeaderboardRequestState.Failure;
            leaderboardRequestEvent.Execute(leaderboardName, leaderboardRequestType, leaderboardRequestState);

            return leaderboardRecordList;
        }


        public void AddForcedFailureRequest(LeaderboardRequestType leaderboardRequestType)
        {
            forcedFailureRequests.Add(leaderboardRequestType);
        }


        public void RemoveForcedFailureRequest(LeaderboardRequestType leaderboardRequestType)
        {
            forcedFailureRequests.Remove(leaderboardRequestType);
        }
    }
}

