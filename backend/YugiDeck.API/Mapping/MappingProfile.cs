using AutoMapper;
using YugiDeck.Core.DTOs.Cards;
using YugiDeck.Core.DTOs.Collection;
using YugiDeck.Core.DTOs.Decks;
using YugiDeck.Core.DTOs.Duels;
using YugiDeck.Core.Entities;

namespace YugiDeck.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Card, CardDto>();
        CreateMap<UserCard, UserCardDto>()
            .ForMember(d => d.Card, o => o.MapFrom(s => s.Card));
        CreateMap<Deck, DeckDto>()
            .ForMember(d => d.MainCount, o => o.MapFrom(s => s.DeckCards.Count(dc => dc.Section == "main")))
            .ForMember(d => d.ExtraCount, o => o.MapFrom(s => s.DeckCards.Count(dc => dc.Section == "extra")))
            .ForMember(d => d.SideCount, o => o.MapFrom(s => s.DeckCards.Count(dc => dc.Section == "side")));
        CreateMap<Duel, DuelDto>();
        CreateMap<LPLog, LPLogDto>();
    }
}
