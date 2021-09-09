using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace GitHubTopRepos.Data.Entities
{
    public class RequestTrackingContext : DbContext, IRequestTrackingContext
    {
        public RequestTrackingContext(DbContextOptions options) : base(options)
        {
        }
        private DbConnection EnsureConnection()
        {
            var connection = Database.GetDbConnection();
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection;
        }

        public async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType, IEnumerable<DbParameter> parameters)
        {
            var connection = EnsureConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters.ToArray());
                }
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<TResult> ExecuteReaderAsync<TResult>(string commandText, CommandType commandType, IEnumerable<DbParameter> parameters, Func<DbDataReader, TResult> function)
        {
            var connection = EnsureConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters.ToArray());
                }
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    var result = await reader.ReadAsync() ? function(reader) : default;
                    return result;
                }
            }
        }

        public DbSet<RequestLog> RequestLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Language>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("language");

                entity.HasIndex(e => e.Name, "idx_language_name");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(60)
                    .HasColumnName("name");

                entity.Property(e => e.Popular).HasColumnName("popular");
            });

            modelBuilder.Entity<RequestLog>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("request_log");

                entity.HasIndex(e => e.DateRequested, "idx_request_log_date_requested");

                entity.HasIndex(e => e.Language, "idx_request_log_language");

                entity.Property(e => e.DateRequested)
                    .HasColumnName("date_requested")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasMaxLength(60)
                    .HasColumnName("language");

                entity.Property(e => e.RecordsReturned).HasColumnName("records_returned");

                entity.Property(e => e.RequestQuery)
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasColumnName("request_query");
            });
        }

    }
}
