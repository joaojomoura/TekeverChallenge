using System.Data;

namespace TvShowAPI.Data;

// This factory will be used to create database connections
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}