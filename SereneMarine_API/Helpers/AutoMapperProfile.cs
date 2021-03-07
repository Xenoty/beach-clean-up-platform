using AutoMapper;
using WebApi.Entities;
using WebApi.Models.ApiStats;
using WebApi.Models.Events;
using WebApi.Models.Petitions;
using WebApi.Models.PetitionsSigned;
using WebApi.Models.ThreadMessages;
using WebApi.Models.Threads;
using WebApi.Models.Users;

namespace WebApi.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //users
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<UpdateModel, User>();

            //events
            CreateMap<Event, EventsModel>();
            CreateMap<EventRegisterModel, Event>();
            CreateMap<EventUpdateModel, Event>();

            //petitions
            CreateMap<Petition, PetitionsModel>();
            CreateMap<PetitionRegisterModel, Petition>();
            CreateMap<PetitionUpdateModel, Petition>();

            //petitions_signed
            CreateMap<PetitionSigned, PetitionsSignedModel>();
            CreateMap<PetitionsSignedRegisterModel, PetitionSigned>();

            //threads
            CreateMap<Thread, ThreadsModel>();
            CreateMap<ThreadsRegisterModel, Thread>();
            CreateMap<ThreadsUpdateModel, Thread>();

            //thread_messages
            CreateMap<ThreadMessage, ThreadMessagesModel>();
            CreateMap<ThreadMessagesRegisterModel, ThreadMessage>();
            CreateMap<ThreadMessagesUpdateModel, ThreadMessage>();

            //api_statistics
            CreateMap<Statistics, StatisticsModel>();
        }
    }
}