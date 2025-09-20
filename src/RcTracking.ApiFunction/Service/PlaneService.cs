using Microsoft.EntityFrameworkCore;
using RcTracking.ApiFunction.Context;
using RcTracking.ApiFunction.Interface;
using RcTracking.ApiFunction.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RcTracking.ApiFunction.Service
{
    public class PlaneService : IPlaneService
    {
        private readonly PlaneContext _dbContext;
        public PlaneService(PlaneContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlaneModel> CreatePlaneAsync(string name)
        {
            var id = Guid.NewGuid();
            var plane = new PlaneModel(id, name);
            await _dbContext.Planes.AddAsync(plane);
            await _dbContext.SaveChangesAsync();
            return plane;
        }

        public async Task DeletePlaneAsync(Guid id)
        {
            var plane = await _dbContext.FindAsync<PlaneModel>(id);
            if (plane != null)
            {
                _dbContext.Planes.Remove(plane);
                await _dbContext.SaveChangesAsync();
            }
        }

        public Task<PlaneModel?> GetPlaneAsync(Guid id)
        {
            return _dbContext.FindAsync<PlaneModel>(id).AsTask();
        }

        public async Task<IEnumerable<PlaneModel>> GetPlanesAsync()
        {
            var list = _dbContext.Planes
                .AsNoTracking()
                .ToListAsync();
            return await list;
        }

        public async Task<PlaneModel> UpdatePlaneAsync(Guid id, string name)
        {
            var plane = await _dbContext.FindAsync<PlaneModel>(id);
            if (plane == null)
            {
                throw new KeyNotFoundException($"Plane with id {id} not found");
            }
            plane.Name = name;
            await _dbContext.SaveChangesAsync();
            return plane;
        }
    }
}
