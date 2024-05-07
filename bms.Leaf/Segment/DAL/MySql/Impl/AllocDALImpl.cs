using bms.Leaf.Entity;
using Microsoft.EntityFrameworkCore;

namespace bms.Leaf.Segment.DAL.MySql.Impl
{
    public class AllocDALImpl : IAllocDAL
    {
        private readonly LeafContext dbContext;
        public AllocDALImpl(LeafContext leafContext)
        {
            dbContext = leafContext;
        }

        public async Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.LeafAlloc.Select(p => p.BizTag).ToListAsync(cancellationToken);
        }

        public async Task<LeafAlloc> UpdateMaxIdAndGetLeafAllocAsync(string bizTag, CancellationToken cancellationToken = default)
        {
            LeafAlloc? entity = null;
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var allocToUpdate = await dbContext.LeafAlloc.FirstOrDefaultAsync(p => p.BizTag == bizTag, cancellationToken);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += allocToUpdate.Step;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                // 执行查询操作
                entity = await dbContext.LeafAlloc.Where(p => p.BizTag == bizTag).FirstOrDefaultAsync(cancellationToken);

                // 提交事务
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            return entity;
        }

        public async Task<LeafAlloc> UpdateMaxIdByCustomStepAndGetLeafAllocAsync(LeafAlloc leafAlloc, CancellationToken cancellationToken = default)
        {
            LeafAlloc? entity = null;
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var allocToUpdate = await dbContext.LeafAlloc.FirstOrDefaultAsync(p => p.BizTag == leafAlloc.BizTag, cancellationToken);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += leafAlloc.Step;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                // 执行查询操作
                entity = await dbContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).FirstOrDefaultAsync(cancellationToken);

                // 提交事务
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            return entity;
        }

        public LeafAlloc UpdateMaxIdAndGetLeafAlloc(string bizTag)
        {
            LeafAlloc? entity = null;
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                var allocToUpdate = dbContext.LeafAlloc.FirstOrDefault(p => p.BizTag == bizTag);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += allocToUpdate.Step;
                    dbContext.LeafAlloc.Update(allocToUpdate);
                    dbContext.SaveChanges();
                }

                // 执行查询操作
                entity = dbContext.LeafAlloc.Where(p => p.BizTag == bizTag).FirstOrDefault();

                // 提交事务
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            return entity;
        }

        public LeafAlloc UpdateMaxIdByCustomStepAndGetLeafAlloc(LeafAlloc leafAlloc)
        {
            LeafAlloc? entity = null;
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                var allocToUpdate = dbContext.LeafAlloc.FirstOrDefault(p => p.BizTag == leafAlloc.BizTag);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += leafAlloc.Step;
                    dbContext.LeafAlloc.Update(allocToUpdate);
                    dbContext.SaveChanges();
                }

                // 执行查询操作
                entity = dbContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).FirstOrDefault();

                // 提交事务
                transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction.RollbackAsync();
                throw;
            }
            return entity;
        }
    }
}
