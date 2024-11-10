using bms.Leaf.Entity;
using Microsoft.EntityFrameworkCore;

namespace bms.Leaf.Segment.DAL.MySql.Impl
{
    public class AllocDALImpl(LeafContext leafContext) : IAllocDAL
    {
        public async Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
        {
            return await leafContext.LeafAlloc.Select(p => p.BizTag).ToListAsync(cancellationToken);
        }

        public async Task<LeafAlloc?> UpdateMaxIdAndGetLeafAllocAsync(string bizTag, CancellationToken cancellationToken = default)
        {
            LeafAlloc? entity = null;
            await using var transaction = await leafContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var allocToUpdate = await leafContext.LeafAlloc.FirstOrDefaultAsync(p => p.BizTag == bizTag, cancellationToken);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += allocToUpdate.Step;
                    await leafContext.SaveChangesAsync(cancellationToken);
                }

                // 执行查询操作
                entity = await leafContext.LeafAlloc.Where(p => p.BizTag == bizTag).FirstOrDefaultAsync(cancellationToken);

                // 提交事务
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            return entity;
        }

        public async Task<LeafAlloc?> UpdateMaxIdByCustomStepAndGetLeafAllocAsync(LeafAlloc leafAlloc, CancellationToken cancellationToken = default)
        {
            LeafAlloc? entity = null;
            await using var transaction = await leafContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var allocToUpdate = await leafContext.LeafAlloc.FirstOrDefaultAsync(p => p.BizTag == leafAlloc.BizTag, cancellationToken);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += leafAlloc.Step;
                    await leafContext.SaveChangesAsync(cancellationToken);
                }

                // 执行查询操作
                entity = await leafContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).FirstOrDefaultAsync(cancellationToken);

                // 提交事务
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            return entity;
        }

        public LeafAlloc? UpdateMaxIdAndGetLeafAlloc(string bizTag)
        {
            LeafAlloc? entity = null;
            using var transaction = leafContext.Database.BeginTransaction();
            try
            {
                var allocToUpdate = leafContext.LeafAlloc.FirstOrDefault(p => p.BizTag == bizTag);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += allocToUpdate.Step;
                    leafContext.LeafAlloc.Update(allocToUpdate);
                    leafContext.SaveChanges();
                }

                // 执行查询操作
                entity = leafContext.LeafAlloc.Where(p => p.BizTag == bizTag).FirstOrDefault();

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

        public LeafAlloc? UpdateMaxIdByCustomStepAndGetLeafAlloc(LeafAlloc leafAlloc)
        {
            LeafAlloc? entity = null;
            using var transaction = leafContext.Database.BeginTransaction();
            try
            {
                var allocToUpdate = leafContext.LeafAlloc.FirstOrDefault(p => p.BizTag == leafAlloc.BizTag);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += leafAlloc.Step;
                    leafContext.LeafAlloc.Update(allocToUpdate);
                    leafContext.SaveChanges();
                }

                // 执行查询操作
                entity = leafContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).FirstOrDefault();

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
    }
}
