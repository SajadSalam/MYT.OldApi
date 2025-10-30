using AutoMapper;
using Events.DATA.DTOs;
using Events.DATA.DTOs.ArticleDto;
using Events.DATA.DTOs.Book;
using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.Chart;
using Events.DATA.DTOs.Event;
using Events.DATA.DTOs.Notifications;
using Events.DATA.DTOs.PointOfSale;
using Events.DATA.DTOs.roles;
using Events.DATA.DTOs.SupportMessage;
using Events.DATA.DTOs.Tag;
using Events.DATA.DTOs.Tickets;
using Events.DATA.DTOs.Tickets.TicketTemplate;
using Events.DATA.DTOs.User;
using Events.Entities;
using Events.Entities.Book;
using Events.Entities.Ticket;
using OneSignalApi.Model;
using SeatsioDotNet.Charts;
using SeatsioDotNet.Events;


namespace Events.Helpers
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<ArticleForm, Article>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ArticleUpdate, Article>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AppUser, UserDto>();
            CreateMap<RegisterForm, App>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Role, RoleDto>();
            CreateMap<Article, ArticleDto>();
            CreateMap<AppUser, AppUser>();

            CreateMap<Permission, PermissionDto>();

            // CategoryChartForm
            CreateMap<BaseCategory, Category>()
                .ForMember(dist => dist.Label, opt => opt.MapFrom(src => src.Name))
                .ForMember(dist => dist.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Id.ToString()));


            CreateMap<CategoryChartForm, BaseCategory>();

            CreateMap<BaseCategory, CategoryDto>();

            CreateMap<ChartDto, BaseChart>().ReverseMap()
                .ForMember(dist => dist.Key, opt => opt.MapFrom(src => src.ChartKey))
                ;

            CreateMap<Chart, ChartDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.PublishedVersionThumbnailUrl))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                ;

            CreateMap<EventEntity, EventDto>()
                .ForMember(dest => dest.PointOfSales, opt => opt.MapFrom(src => src.PointOfSales.Select(x => x.PointOfSale)))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Chart.Categories))
                .ForMember(dest => dest.WorkspaceKey, opt => opt.MapFrom(src => src.Chart.WorkspaceKey))
                .ForMember(dest => dest.SecretKey, opt => opt.MapFrom(src => src.Chart.User.WorkspaceSecretKey))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.EventTags.Select(x => x.Tag)))
                .ForMember(dest => dest.NumberOfAttendance, opt => opt.MapFrom(src => src.Books.Count(x => x.Objects.Any(y => y.Ticket.IsUsed == true))))
                ;


            CreateMap<TicketTemplateForm, TicketTemplate>();
            CreateMap<TicketTemplate, TicketTemplateDto>();

            CreateMap<TicketTemplateField, TicketTemplateFieldDto>();


            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.BookObject.Book.Event.Name))
                .ForMember(dest => dest.EventAddress, opt => opt.MapFrom(src => src.BookObject.Book.Event.Address))
                .ForMember(dest => dest.EventDate, opt => opt.MapFrom(src => src.BookObject.Book.Event.StartEvent))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.BookObject.Book.IsPaid))
                .ForMember(dest => dest.TicketTemplate, opt => opt.MapFrom(src => src.BookObject.Book.Event.TicketTemplates.FirstOrDefault()))
                .ForMember(dest => dest.BookInfo, opt => opt.MapFrom(src => src.BookObject))
                .ForMember(dest => dest.IsCanceled, opt => opt.MapFrom(src => src.BookObject.IsCanceled))
                .ForMember(dest=>dest.SeatCategory, opt=>opt.MapFrom(src=>src.BookObject.Category.Name))

                ;

            CreateMap<EventEntity, EventUpdate>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<PointOfSale, PointOfSaleDto>()
                .ForMember(dest => dest.TotalSoldTicket, src => src.MapFrom(x => x.Books.Count(b => b.IsPaid == true)))
                .ForMember(dest => dest.IncomeAmount, src => src.MapFrom(x => x.Books.Where(b => b.IsPaid == true).Sum(b => b.TotalPrice)));


            CreateMap<Book, BookDto>()
              .ForMember(dest => dest.TotalPriceAfterDiscount, opt => opt.MapFrom(src => src.TotalPrice - src.Discount))
              .ForMember(dest=>dest.InvoiceNumber,opt=>opt.MapFrom(src=>src.Bill.BillId));

            CreateMap<BookObject, BookObjectDto>()
                .ForMember(dest => dest.ExpiredTime, opt => opt.MapFrom(src => GetRemainingTime(src)));


            CreateMap<BookObject, BookObjectTicketDto>();


            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.NumberOfEvent, src => src.MapFrom(x => x.EventTags.Count(et => et.Event.StartEvent >= DateTime.UtcNow && et.Event.StartEvent <= DateTime.UtcNow && et.Event.IsPublish == true)))
                ;
            CreateMap<TagForm, Tag>();
            CreateMap<TagUpdate, Tag>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<NotificationsForm, Notifications>();
            CreateMap<AppUser, UserWithoutTokenDto>();

            CreateMap<UpdatePointOfSaleForm, PointOfSale>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));




            CreateMap<EventEntity,EventUpdate>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null))
                ;


            CreateMap<SupportMessage, SupportMessageDto>();
            CreateMap< SupportMessageUpdate,SupportMessage>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SupportMessageForm,SupportMessage >();



        }
        public static string GetRemainingTime(BookObject src)
        {
            if (src.BookHoldInfo == null)
            {
                return null!;
            }

            if (src.CreationDate.HasValue)
            {

                var remainingTime = TimeSpan.FromMinutes(src.BookHoldInfo.ExpiredMinutes) - (DateTime.Now - src.CreationDate.Value);
                if (remainingTime.TotalMinutes < 0)
                {
                    return null!;
                }

                return $"{(int)remainingTime.TotalMinutes}:{remainingTime.Seconds}";
            }

            return null!;
        }


    }
}