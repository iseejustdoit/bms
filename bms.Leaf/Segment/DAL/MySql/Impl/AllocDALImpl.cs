using bms.Leaf.Segment.Model;

namespace bms.Leaf.Segment.DAL.MySql.Impl
{
    public class AllocDALImpl : IAllocDAL
    {
        private const string SELECT_ALL_TAGS = "SELECT BizTag FROM Alloc";
        private const string UPDATE_MAXID_BY_TAG = "UPDATE Alloc SET MaxId = MaxId + Step WHERE BizTag = @bizTag";
        private const string SELECT_ALLOC_BY_TAG = "SELECT BizTag,MaxId,Step FROM Alloc WHERE BizTag = @bizTag";
        private const string UPDATE_MAXID_BY_CUSTOM_STEP = "UPDATE Alloc SET MaxId = MaxId + @step WHERE BizTag = @bizTag";

        public AllocDALImpl(string connectionString)
        {
            MySqlHelper.SetConnString(connectionString);
        }

        public List<LeafAllocModel> GetAllLeafAllocs()
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetAllTagsAsync()
        {
            var resultList = new List<string>();
            await MySqlHelper.ExecuteReaderAsync(SELECT_ALL_TAGS, async (dataReader) =>
            {
                while (await dataReader.ReadAsync())
                {
                    resultList.Add(dataReader.GetString(0));
                }
            }); return resultList;
        }

        public async Task<LeafAllocModel> UpdateMaxIdAndGetLeafAllocAsync(string bizTag)
        {
            LeafAllocModel leafAllocModel = null;
            Exception exception = null;
            await MySqlHelper.ExecuteTransactionAsync(async (command) =>
            {
                command.Parameters.AddWithValue("@bizTag", bizTag);

                command.CommandText = UPDATE_MAXID_BY_TAG;
                await command.ExecuteNonQueryAsync();

                command.CommandText = SELECT_ALLOC_BY_TAG;
                await using (var dataReader = await command.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        leafAllocModel = new LeafAllocModel
                        {
                            Key = dataReader.GetString(0),
                            MaxId = dataReader.GetInt64(1),
                            Step = dataReader.GetInt32(2),
                        };
                    }
                }
            }, (ex) => { exception = ex; });

            if (exception != null)
                throw exception;

            return leafAllocModel;
        }

        public async Task<LeafAllocModel> UpdateMaxIdByCustomStepAndGetLeafAllocAsync(LeafAllocModel leafAlloc)
        {
            LeafAllocModel leafAllocModel = null;
            Exception exception = null;
            await MySqlHelper.ExecuteTransactionAsync(async (command) =>
            {
                command.CommandText = UPDATE_MAXID_BY_CUSTOM_STEP;
                command.Parameters.AddWithValue("@bizTag", leafAlloc.Key);
                command.Parameters.AddWithValue("@step", leafAlloc.Step);
                await command.ExecuteNonQueryAsync();

                command.Parameters.Clear();
                command.CommandText = SELECT_ALLOC_BY_TAG;
                command.Parameters.AddWithValue("@bizTag", leafAlloc.Key);
                await using (var dataReader = await command.ExecuteReaderAsync())
                {
                    if (await dataReader.ReadAsync())
                    {
                        leafAllocModel = new LeafAllocModel
                        {
                            Key = dataReader.GetString(0),
                            MaxId = dataReader.GetInt64(1),
                            Step = dataReader.GetInt32(2),
                        };
                    }
                }
            }, (ex) => { exception = ex; });

            if (exception != null)
                throw exception;

            return leafAllocModel;
        }
    }
}
