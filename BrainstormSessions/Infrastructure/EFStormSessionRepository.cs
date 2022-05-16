using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BrainstormSessions.Infrastructure
{
    public class EFStormSessionRepository : IBrainstormSessionRepository
    {
        private readonly AppDbContext _dbContext;

        public EFStormSessionRepository(AppDbContext dbContext)
        {
            Log.Debug("Start db initialization.");
            try
            {
                //Бешенная логика инициализации
                _dbContext = dbContext ?? throw new InvalidOperationException();
            }
            catch
            {
                Log.Fatal("Db initialization failed.");
            }
        }

        public Task<BrainstormSession> GetByIdAsync(int id)
        {
            return _dbContext.BrainstormSessions
                .Include(s => s.Ideas)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public Task<List<BrainstormSession>> ListAsync()
        {
            return _dbContext.BrainstormSessions
                .Include(s => s.Ideas)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();
        }

        public Task AddAsync(BrainstormSession session)
        {
            _dbContext.BrainstormSessions.Add(session);
            return _dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(BrainstormSession session)
        {
            _dbContext.Entry(session).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync();
        }
    }
}
