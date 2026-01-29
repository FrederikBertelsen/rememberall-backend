using Microsoft.EntityFrameworkCore;
using rememberall.src.Entities;

namespace rememberall.src.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<TodoList> TodoLists { get; init; }
    public DbSet<ListCollaborator> ListCollaborators { get; init; }
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
            .WithOne(item => item.List)
            .HasForeignKey(i => i.ListId)
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
        modelBuilder.Entity<ListCollaborator>()
            .HasIndex(listCollaborator => new { listCollaborator.ListId, listCollaborator.UserId })
            .IsUnique();

        modelBuilder.Entity<Invite>()
            .HasIndex(invite => new { invite.InviteRecieverId, invite.ListId })
            .IsUnique();

        // ListCollaborator <=> User
        modelBuilder.Entity<User>()
            .HasMany(user => user.Collaborations)
            .WithOne(listCollaborator => listCollaborator.User)
            .HasForeignKey(listCollaborator => listCollaborator.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ListCollaborator => List
        modelBuilder.Entity<TodoList>()
            .HasMany(list => list.Collaborators)
            .WithOne(listCollaborator => listCollaborator.List)
            .HasForeignKey(listCollaborator => listCollaborator.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}