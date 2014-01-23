namespace TinyMoneyManager.Component
{
    using RapidRepository;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NkjRepository<T> : RapidRepository.RapidRepository<T> where T: IRapidEntity
    {
        public override T Add(T entity)
        {
            T local = base.Add(entity);
            this.SubmitChanges();
            return local;
        }

        public override void Delete(T entity)
        {
            base.Delete(entity);
            this.SubmitChanges();
        }

        public void Delete(System.Func<T,Boolean><T, bool> where)
        {
            this.Where(where).ToList<T>().ForEach(delegate (T p) {
                this.Delete(p.Id);
            });
        }

        public override void Delete(System.Guid id)
        {
            base.Delete(id);
            this.SubmitChanges();
        }

        public bool Exists(System.Func<T,Boolean><T, bool> where)
        {
            return (this.Where(where).Count<T>() > 0);
        }

        public T FirstOrDefault()
        {
            System.Collections.Generic.IList<T><T> all = base.GetAll();
            if ((all.Count != 0) && (all[0] != null))
            {
                return all[0];
            }
            return default(T);
        }

        public override T GetById(System.Guid id)
        {
            if (!this.Exists(id))
            {
                return default(T);
            }
            return base.GetById(id);
        }

        public virtual void SubmitChanges()
        {
            RapidContext.CurrentContext.SaveChanges();
        }

        public override T Update(T entity)
        {
            T local = base.Update(entity);
            this.SubmitChanges();
            return local;
        }

        public NkjRepository<T> UpdateOnSubmit(T entity)
        {
            base.Update(entity);
            return (NkjRepository<T>) this;
        }

        public System.Collections.Generic.IEnumerable<T><T> Where(System.Func<T,Boolean><T, bool> whereSeletor)
        {
            return base.GetAll().Where<T>(whereSeletor);
        }
    }
}

