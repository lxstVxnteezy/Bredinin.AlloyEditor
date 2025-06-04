using Bredinin.MyPetProject.DAL;
using Bredinin.MyPetProject.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.DAL.Core
{
    internal class Repository<TEntity>(ServiceContext context) : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly ServiceContext Context = context;
        public IQueryable<TEntity> Query => Context.Set<TEntity>();
        public virtual TEntity GetById(Guid id)
        {
            return Context.Set<TEntity>().Single(x => x.Id == id);
        }
        public virtual Task<TEntity> GetByIdAsync(Guid id, CancellationToken ctn)
        {
            return Context.Set<TEntity>().SingleAsync(x => x.Id == id, ctn);
        }
        public TEntity? FoundById(Guid id)
        {
            return Context.Set<TEntity>().Find(id);
        }
        public Task<TEntity?> FoundByIdAsync(Guid id, CancellationToken ctn)
        {
            return Context.Set<TEntity>().SingleOrDefaultAsync(x => x.Id == id, ctn);
        }
        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }
        public void AddRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }
        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }
        public void Remove(Guid id)
        {
            var entity = GetById(id);

            Context.Set<TEntity>().Remove(entity);
        }
        public void RemoveRange(TEntity[] entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }
        public void RemoveById(Guid id)
        {
            var entity = GetById(id);

            Context.Set<TEntity>().Remove(entity);
        }

        public Task SaveChanges(CancellationToken ctn)
        {
            return Context.SaveChangesAsync(ctn);
        }
    }
}
