﻿using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category>
{
    void Update(Category category);
    Task Save();
}
