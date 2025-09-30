﻿using RcTracking.Shared.Model;

namespace RcTracking.UI.Interface
{
    public interface IPlaneService
    {
        Dictionary<Guid, PlaneModel> Planes { get; }
        bool HasLoaded { get; }
        Task LoadPlanesAsync();
        Task AddPlaneAsync(PlaneModel plane);
        Task UpdatePlaneAsync(PlaneModel plane);
        Task DeletePlaneAsync(Guid planeId);
    }
}
