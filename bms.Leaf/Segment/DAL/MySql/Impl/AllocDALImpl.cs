using bms.Leaf.Entity;
using Microsoft.EntityFrameworkCore;

namespace bms.Leaf.Segment.DAL.MySql.Impl
{
    public class AllocDALImpl(LeafContext leafContext) : IAllocDAL
    {
        public async Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
        {
            return await leafContext.LeafAlloc.Select.ToListAsync(p => p.BizTag, cancellationToken);
        }

        public async Task<LeafAlloc?> UpdateMaxIdAndGetLeafAllocAsync(string bizTag, CancellationToken cancellationToken = default)
        {
            LeafAlloc? entity = null;
            using var transaction = leafContext.UnitOfWork.GetOrBeginTransaction();
            try
            {
                var allocToUpdate = await leafContext.LeafAlloc.Where(p => p.BizTag == bizTag).FirstAsync(cancellationToken);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += allocToUpdate.Step;
                    await leafContext.LeafAlloc.UpdateAsync(allocToUpdate, cancellationToken);
                }

                // 执行查询操作
                entity = await leafContext.LeafAlloc.Where(p => p.BizTag == bizTag).FirstAsync(cancellationToken);

                await leafContext.SaveChangesAsync(cancellationToken);
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
            using var transaction = leafContext.UnitOfWork.GetOrBeginTransaction();
            try
            {
                var allocToUpdate = await leafContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).FirstAsync(cancellationToken);
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += leafAlloc.Step;
                    await leafContext.LeafAlloc.UpdateAsync(allocToUpdate, cancellationToken);
                }
                // 执行查询操作
                entity = await leafContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).FirstAsync(cancellationToken);

                await leafContext.SaveChangesAsync(cancellationToken);
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
            using var transaction = leafContext.UnitOfWork.GetOrBeginTransaction();
            try
            {
                var allocToUpdate = leafContext.LeafAlloc.Where(p => p.BizTag == bizTag).First();
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += allocToUpdate.Step;
                    leafContext.LeafAlloc.Update(allocToUpdate);
                }

                // 执行查询操作
                entity = leafContext.LeafAlloc.Where(p => p.BizTag == bizTag).First();

                // 提交事务
                leafContext.SaveChanges();
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
            using var transaction = leafContext.UnitOfWork.GetOrBeginTransaction();
            try
            {
                var allocToUpdate = leafContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).First();
                if (allocToUpdate != null)
                {
                    allocToUpdate.MaxId += leafAlloc.Step;
                    leafContext.LeafAlloc.Update(allocToUpdate);
                }

                // 执行查询操作
                entity = leafContext.LeafAlloc.Where(p => p.BizTag == leafAlloc.BizTag).First();

                // 提交事务
                leafContext.SaveChanges();
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
