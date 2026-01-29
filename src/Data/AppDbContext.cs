using Microsoft.EntityFrameworkCore;
using RememberAll.src.Entities;

namespace RememberAll.src.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<TodoList> TodoLists { get; init; }
    public DbSet<ListAccess> ListAccesss { get; init; }
    public DbSet<TodoItem> TodoItems { get; init; }
    public DbSet<Invite> Invites { get; init; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User <=> TodoList
        modelBuilder.Entity<User>()
            .HasMany(user => user.Lists)
            .WithOne(list => list.Owner)
            .HasForeignKey(list => list.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);


        // TodoList <=> TodoItem
        modelBuilder.Entity<TodoList>()
            .HasMany(list => list.Items)
            .WithOne(item => item.TodoList)
            .HasForeignKey(i => i.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);


        // Invite <=> User

        // Inviter
        modelBuilder.Entity<Invite>()
            .HasOne(invite => invite.InviterSender)
            .WithMany(user => user.InvitesSent)
            .HasForeignKey(invite => invite.InviteSenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invitee
        modelBuilder.Entity<Invite>()
            .HasOne(invite => invite.InviteReciever)
            .WithMany(user => user.InvitesReceived)
            .HasForeignKey(invite => invite.InviteRecieverId)
            .OnDelete(DeleteBehavior.Restrict);


        // Invite => List
        modelBuilder.Entity<Invite>()
            .HasOne(invite => invite.List)
            .WithMany(list => list.Invites)
            .HasForeignKey(invite => invite.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraints / indexes
        modelBuilder.Entity<ListAccess>()
            .HasIndex(listAccess => new { listAccess.ListId, listAccess.UserId })
            .IsUnique();

        modelBuilder.Entity<Invite>()
            .HasIndex(invite => new { invite.InviteRecieverId, invite.ListId })
            .IsUnique();

        // ListAccess <=> User
        modelBuilder.Entity<User>()
            .HasMany(user => user.ListAccess)
            .WithOne(listAccess => listAccess.User)
            .HasForeignKey(listAccess => listAccess.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ListAccess => List
        modelBuilder.Entity<TodoList>()
            .HasMany(list => list.Accessors)
            .WithOne(listAccess => listAccess.List)
            .HasForeignKey(listAccess => listAccess.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}