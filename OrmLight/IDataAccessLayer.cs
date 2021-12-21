using OrmLight.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmLight
{
    public interface IDataAccessLayer
    {
        public abstract QueryableSource<TEntity> Get<TEntity>();
    }
}
