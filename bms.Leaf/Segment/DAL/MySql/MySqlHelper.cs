using MySqlConnector;
using System.Data;

namespace bms.Leaf.Segment.DAL.MySql
{
    public sealed class MySqlHelper
    {
        private static string _connectionString;
        private MySqlHelper() { }

        public static void SetConnString(string connString)
        {
            _connectionString = connString;
        }

        public static async Task ExecuteTransactionAsync(Func<MySqlCommand, Task> executeAction, Action<Exception> exceptionAction = null, CancellationToken cancellationToken = default)
        {
            await using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                var command = conn.CreateCommand();
                await using (var tran = await conn.BeginTransactionAsync(cancellationToken))
                {
                    command.Connection = conn;
                    command.Transaction = tran;

                    try
                    {
                        await executeAction.Invoke(command);
                        await tran.CommitAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await tran.RollbackAsync(cancellationToken);
                        exceptionAction?.Invoke(ex);
                    }
                }
            }
        }
        public static void ExecuteTransaction(Action<MySqlCommand> executeAction, Action<Exception> exceptionAction = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var command = conn.CreateCommand();
                using (var tran = conn.BeginTransaction())
                {
                    command.Connection = conn;
                    command.Transaction = tran;

                    try
                    {
                        executeAction.Invoke(command);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        exceptionAction?.Invoke(ex);
                    }
                }
            }
        }

        public static async Task ExecuteReaderAsync(string commandText, Func<MySqlDataReader, Task> action,
            Dictionary<string, object> paramDict = null, CancellationToken cancellationToken = default)
        {
            await using (var conn = GetConnection())
            {
                var command = new MySqlCommand();
                await PrepareCommandAsync(command, conn, null, CommandType.Text, commandText, paramDict, cancellationToken);

                await using (var dataReader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken))
                {
                    await action.Invoke(dataReader);
                }
            }
        }

        private static async Task PrepareCommandAsync(MySqlCommand command, MySqlConnection connection, MySqlTransaction transaction, CommandType commandType,
            string commandText, Dictionary<string, object> paramDict, CancellationToken cancellationToken = default)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }
            command.Connection = connection;
            command.CommandText = commandText;

            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (paramDict != null && paramDict.Any())
            {
                MySqlParameter[] parameters = new MySqlParameter[paramDict.Count];
                var keyList = new List<string>(paramDict.Keys);
                for (int i = 0; i < keyList.Count; i++)
                {
                    var key = keyList[i];
                    parameters[i] = new MySqlParameter($"@{key}", paramDict[key]);
                }

                AttachParameters(command, parameters);
            }
        }

        private static void AttachParameters(MySqlCommand command, MySqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (MySqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // 检查未分配值的输出参数,将其分配以DBNull.Value. 
                        if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) &&
                            p.Value == null)
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }

        private static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
