using Cysharp.Threading.Tasks;
using Infrastructure.Reward;
using Nakama;
using R3;
using SayGames.Services.Mail;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace Features.Social
{
    public class MailService
    {
        public const string AttachmentKey = "Attachment";
        public const string LeaderBoardNameKey = "LBName";

        private readonly SocialService socialService;
        private readonly UserAuthenticateEvent userAuthenticateEvent;
        private readonly MailReceivedEvent mailReceivedEvent;
        private readonly LeaderboardCachedEvent leaderboardCachedEvent;
        private readonly CompositeDisposable disposables;

        bool isInit = false;
        string inboxHash = string.Empty;



        public MailService(SocialService socialService,
            UserAuthenticateEvent userAuthenticateEvent,
            MailReceivedEvent mailReceivedEvent,
            LeaderboardCachedEvent leaderboardCachedEvent)
        {
            this.socialService = socialService;
            this.userAuthenticateEvent = userAuthenticateEvent;
            this.mailReceivedEvent = mailReceivedEvent;
            this.leaderboardCachedEvent = leaderboardCachedEvent;

            this.disposables = new CompositeDisposable();
        }


        public void Initialize()
        {
            if (isInit)
            {
                return;
            }
            isInit = true;

            leaderboardCachedEvent.Subscribe(CalculateNextFetch).AddTo(disposables);
        }


        public void Deinitialize()
        {
            if (!isInit)
            {
                return;
            }

            isInit = false;

            disposables.Dispose();
        }


        public async UniTask<bool> FetchLettersAsync(CancellationToken cancellationToken = default)
        {
            if (!socialService.IsAuthenticated)
            {
                return false;
            }

            try
            {
                IFetchLettersResult result = await socialService.Client.FetchLettersAsync(socialService.Session, canceller: cancellationToken);
                if (inboxHash != result.Hash)
                {
                    inboxHash = result.Hash;
                    mailReceivedEvent.Execute(result.Letters);
                    DeleteClaimedLetters(result.Letters);
                }
                return true;
            }
            catch
            {
                Debug.LogWarning("Fetch Letters Async failed!");
            }

            return false;
        }


        public async UniTask<bool> ClaimLetterAsync(string letterId, CancellationToken cancellationToken = default)
        {
            if (!await socialService.TryAuthenticateAsync())
            {
                return false;
            }

            try
            {
                await socialService.Client.ClaimLetterAsync(socialService.Session, letterId, canceller: cancellationToken);
                return true;
            }
            catch
            {
                Debug.LogWarning("Claim Letter Async failed!");
            }

            return false;
        }


        private void DeleteClaimedLetters(IReadOnlyCollection<ILetter> letters)
        {
            List<string> lettersForDelete = new List<string>();
            foreach(ILetter letter in letters)
            {
                if(letter.Claimed)
                {
                    lettersForDelete.Add(letter.Id);
                }
            }

            if(lettersForDelete.Count > 0)
            {
                DeleteLettersAsync(lettersForDelete).Forget();
            }
        }


        private async UniTask<bool> DeleteLettersAsync(List<string> letterIds, CancellationToken cancellationToken = default)
        {
            if (!socialService.IsAuthenticated
                || letterIds == null
                || letterIds.Count == 0)
            {
                return false;
            }

            try
            {
                await socialService.Client.DeleteLettersAsync(socialService.Session, letterIds, canceller: cancellationToken);
                return true;
            }
            catch
            {
                Debug.LogWarning("Delete Letters Async failed!");
            }

            return false;
        }


        private void CalculateNextFetch(LeaderboardState leaderboardState)
        {

        }

#if UNITY_EDITOR || PR_CHEAT

        private async UniTask<bool> ResetDevLettersAsync(CancellationToken cancellationToken = default)
        {
            if (!socialService.IsAuthenticated)
            {
                return false;
            }

            try
            {
                await socialService.Client.ResetDevLettersAsync(socialService.Session, cancellationToken: cancellationToken);
                return true;
            }
            catch
            {
                Debug.LogWarning("Reset Dev Letters Async failed!");
            }

            return false;
        }

#endif
    }
}

