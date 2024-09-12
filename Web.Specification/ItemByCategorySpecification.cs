using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Entities;

namespace Web.Specification
{
    public class ItemByCategorySpecification : ISpecification<Item>
    {
        private readonly int _categoryId;

        public ItemByCategorySpecification(int categoryId)
        {
            _categoryId = categoryId;
        }

        public Expression<Func<Item, bool>> ItemsCriteria
        {
            get { return item => item.CategoryId == _categoryId; }
        }
    }
}
