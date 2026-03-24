using R3;
using SayGames.Services.Mail;
using System.Collections.Generic;


namespace Features.Social
{
    public class MailReceivedEvent : ReactiveCommand<IReadOnlyCollection<ILetter>>
    {

    }
}