using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YugiDeck.Core.Entities;

namespace YugiDeck.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Card> Cards => Set<Card>();
    public DbSet<UserCard> UserCards => Set<UserCard>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();
    public DbSet<Duel> Duels => Set<Duel>();
    public DbSet<LPLog> LPLogs => Set<LPLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserCard>()
            .HasIndex(x => new { x.UserId, x.CardId })
            .IsUnique();

        builder.Entity<DeckCard>()
            .HasIndex(x => new { x.DeckId, x.CardId, x.Section });
    }
}