using System;
using System.Data;

namespace Npgsql
{
    /// <summary>
    /// Represents a transaction to be made in a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    public interface INpgsqlTransaction : IDbTransaction, IDisposable
    {
        /// <summary>
        /// Creates a transaction save point.
        /// </summary>
        void CreateSavepoint(string name);

        /// <summary>
        /// Rolls back a transaction from a pending savepoint state.
        /// </summary>
        void RollbackToSavepoint(string name);

        /// <summary>
        /// Rolls back a transaction from a pending savepoint state.
        /// </summary>
        void ReleaseSavepoint(string name);
    }
}
