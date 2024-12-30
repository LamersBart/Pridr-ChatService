using System.Collections.Concurrent;
using ChatService.Data.Encryption;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Data;

public class AppDbContext :DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Connection> Connections { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Connection>()
            .Property(c => c.ConnectedAt)
            .HasColumnType("timestamp with time zone") // PostgreSQL vereist UTC
            .HasDefaultValueSql("now() at time zone 'utc'");
        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(e => e.SenderId)
                .HasColumnType("text");
                
            entity.Property(e => e.ReceiverId)
                .HasColumnType("text");
            
            entity.Property(e => e.MessageText)
                .HasColumnType("text");
            entity.Property(e => e.MessageText)
                .HasConversion(new EncryptedReferenceConverter<string>());
            
            entity.Property(e => e.SenderUserName)
                .HasColumnType("text");
            entity.Property(e => e.SenderUserName)
                .HasConversion(new EncryptedReferenceConverter<string>());
            
            entity.Property(e => e.ReceiverUserName)
                .HasColumnType("text");
            entity.Property(e => e.ReceiverUserName)
                .HasConversion(new EncryptedReferenceConverter<string>());
        });
    }
}